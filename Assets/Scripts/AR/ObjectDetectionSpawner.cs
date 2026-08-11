using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARSolution
{
    /// <summary>
    /// ARKit Object Detection（.arobjectスキャン→自動認識）で対象物が見つかったら、
    /// ARAnchorでワールド座標に固定した上でARコンテンツをInstantiateする。手動アラインメント(CalibrationController)の後継。
    /// 認識方式の決定経緯は docs/AR_CALIBRATION_ARCHITECTURE.md「認識方式の決定変更」参照。
    /// Step D（座標固定）: 認識Poseへ直接コンテンツを置くだけだとiPad移動時のドリフトに弱いため、
    /// 認識位置に空GameObject+ARAnchorを生成し、コンテンツをAnchorの子にする
    /// （docs/UNITY_AR_BUILD_RUNBOOK.md §9 Step D）。
    /// ARAnchorManager.AttachAnchor(ARPlane, Pose) はARPlane専用のオーバーロードしか存在せず
    /// ARTrackedObjectには使えないため、AddComponent&lt;ARAnchor&gt;()で直接アンカーを作る
    /// （このAR Foundationバージョンでは旧`ARAnchorManager.AddAnchor(Pose)`自体が非推奨化されており、
    /// Obsolete属性の指示どおりAddComponent&lt;ARAnchor&gt;()が正式な代替手段）。
    ///
    /// contentOffset について: ARKit Object Detectionの検知座標は、スキャン対象の見た目の中心ではなく
    /// スキャン時にScanningAppが記録した「参照オブジェクトの原点」（既定ではバウンディングボックスの底面中心）。
    /// そのため対象物ごとに検知座標と見た目中心のズレ量が異なり、コード側で自動補正する手段はAR Foundation側に
    /// 存在しない（ARReferenceObjectのcenter/extentはAR Foundation 5.2.2では非公開）。対象を切り替えるたびに
    /// 学生がInspectorで実測して調整することを想定している。
    /// </summary>
    [RequireComponent(typeof(ARTrackedObjectManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    public class ObjectDetectionSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject contentPrefab;
        [SerializeField] private bool removeContentWhenLost = false;
        [SerializeField]
        [Tooltip("検知座標（スキャン原点）からコンテンツ表示位置までのローカルオフセット。" +
                 "スキャン原点は対象の見た目中心と一致しないため、対象ごとに実機で見ながら調整する。")]
        private Vector3 contentOffset = Vector3.zero;

        private ARTrackedObjectManager _trackedObjectManager;
        private readonly Dictionary<TrackableId, GameObject> _spawnedContent = new();
        private readonly Dictionary<TrackableId, ARAnchor> _spawnedAnchors = new();

        private void Awake()
        {
            _trackedObjectManager = GetComponent<ARTrackedObjectManager>();
        }

        private void OnEnable()
        {
            _trackedObjectManager.trackedObjectsChanged += OnTrackedObjectsChanged;
        }

        private void OnDisable()
        {
            _trackedObjectManager.trackedObjectsChanged -= OnTrackedObjectsChanged;
        }

        private void OnTrackedObjectsChanged(ARTrackedObjectsChangedEventArgs args)
        {
            foreach (var trackedObject in args.added)
            {
                Spawn(trackedObject);
            }

            foreach (var trackedObject in args.updated)
            {
                if (_spawnedContent.TryGetValue(trackedObject.trackableId, out var content))
                {
                    // 位置追従はARAnchorが担うため、ここではSetActiveのみ行う。
                    // Tracking直後は数フレームLimitedを経由することがあるため、
                    // Noneのときだけ隠す（Trackingでの厳密一致だと検出直後に消えてしまう）
                    content.SetActive(trackedObject.trackingState != TrackingState.None);
                }
            }

            foreach (var trackedObject in args.removed)
            {
                if (!removeContentWhenLost)
                {
                    // 保持モード: ワールド固定のAnchorはそのまま残し、コンテンツだけ非表示にする。
                    // 再検出時に同じtrackableIdでSpawnが呼ばれてもContainsKeyでスキップされ続けるため、
                    // ここではAnchor/Contentのどちらも破棄・キー削除しない。
                    if (_spawnedContent.TryGetValue(trackedObject.trackableId, out var content))
                    {
                        content.SetActive(false);
                    }
                    continue;
                }

                if (_spawnedAnchors.TryGetValue(trackedObject.trackableId, out var anchor))
                {
                    Destroy(anchor.gameObject); // contentはanchorの子なので道連れに破棄される
                    _spawnedAnchors.Remove(trackedObject.trackableId);
                }

                _spawnedContent.Remove(trackedObject.trackableId);
            }
        }

        private void Spawn(ARTrackedObject trackedObject)
        {
            if (contentPrefab == null || _spawnedContent.ContainsKey(trackedObject.trackableId)) return;

            var anchorGO = new GameObject($"ObjectAnchor_{trackedObject.referenceObject.name}");
            anchorGO.transform.SetPositionAndRotation(
                trackedObject.transform.position, trackedObject.transform.rotation);
            var anchor = anchorGO.AddComponent<ARAnchor>();

            _spawnedAnchors[trackedObject.trackableId] = anchor;

            var content = Instantiate(contentPrefab, anchor.transform);
            content.transform.localPosition = contentOffset;
            content.transform.localRotation = Quaternion.identity;
            _spawnedContent[trackedObject.trackableId] = content;

            Debug.Log($"[ObjectDetectionSpawner] Detected '{trackedObject.referenceObject.name}' " +
                      $"anchor.trackableId={anchor.trackableId} at {anchor.transform.position} " +
                      $"(anchored, contentOffset={contentOffset}, contentLossyScale={content.transform.lossyScale}, " +
                      $"anchorLossyScale={anchor.transform.lossyScale})");

            // DEBUG（ベースライン再確認用・原因判明後に削除）: 前回確実に見えたY軸8分割の虹色表示を復元
            SpawnSegmentedAxis(anchor.transform, Vector3.up, "AxisY");
        }

        // DEBUG（ベースライン再確認用・原因判明後に削除）: 軸を0〜3mで8分割し、セグメントごとに色を変える
        private static void SpawnSegmentedAxis(Transform parent, Vector3 axis, string namePrefix)
        {
            const int segments = 8;
            const float totalLength = 3f;
            const float segLength = totalLength / segments;
            const float baseSize = 0.05f;
            for (var i = 0; i < segments; i++)
            {
                var hue = i / (float)segments;
                var color = Color.HSVToRGB(hue, 1f, 1f);
                var segGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segGO.name = $"{namePrefix}_seg{i}";
                segGO.transform.SetParent(parent, false);
                segGO.transform.localPosition = axis * (segLength * (i + 0.5f));
                segGO.transform.localScale = Vector3.one * baseSize + axis * (segLength - baseSize);
                segGO.GetComponent<Renderer>().material.color = color;
                Destroy(segGO.GetComponent<Collider>());
            }
        }
    }
}
