using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARSolution
{
    /// <summary>
    /// World Anchor座標固定の最小検証用。ボタン押下時のカメラ前方固定距離にワールド座標を計算し、
    /// AddComponent&lt;ARAnchor&gt;()でその場に固定する（ARAnchorManager.AddAnchor(Pose)はこのAR
    /// Foundationバージョンで非推奨のため、ObjectDetectionSpawner.csと同じ代替手段を踏襲）。
    /// カメラの子にしないことで、iPadを動かしてもロケータが実空間の同じ位置に残り続ける。
    /// </summary>
    [RequireComponent(typeof(ARAnchorManager))]
    public class WorldLocatorSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnDistance = 1.0f;
        [SerializeField] private float axisLength = 0.2f;
        [SerializeField] private float axisThickness = 0.02f;
        [Tooltip("実機ビルドでランタイム生成マテリアルのシェーダーがストリップされ"
                 + "ピンク(shader未検出)になる問題への対策。プロジェクト内アセットとして"
                 + "参照することでビルドに確実に含める（WorldLocatorTestSceneSetupが自動配線）。")]
        [SerializeField] private Material axisMaterialTemplate;

        private GameObject _currentAnchorGO;

        public void SpawnLocator()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[WorldLocatorSpawner] Camera.main が見つかりません。");
                return;
            }

            // 前回分を消してから生成する。複数個残すと同じ座標付近で軸同士が重なり、
            // Zファイティング等で一部の軸が見えなくなる（切り分けを妨げる）ため、
            // このテストでは常に1個だけに保つ。
            if (_currentAnchorGO != null) Destroy(_currentAnchorGO);

            var position = cam.transform.position + cam.transform.forward * spawnDistance;

            var anchorGO = new GameObject("WorldLocatorAnchor");
            anchorGO.transform.SetPositionAndRotation(position, Quaternion.identity);
            var anchor = anchorGO.AddComponent<ARAnchor>();
            _currentAnchorGO = anchorGO;

            // アンカー原点に必ず見える中心マーカー（白い球）を置く。3軸のどれかが視野外・
            // エッジオンで見えなくても「アンカーがそこに固定されているか」は必ず判定できる。
            SpawnMarker(anchor.transform, Vector3.zero, Color.white, axisThickness * 2f, "Center");

            SpawnAxis(anchor.transform, Vector3.right, Color.red, "AxisX");
            SpawnAxis(anchor.transform, Vector3.up, Color.green, "AxisY");
            SpawnAxis(anchor.transform, Vector3.forward, Color.blue, "AxisZ");

            Debug.Log($"[WorldLocatorSpawner] Spawned at {position} (trackableId={anchor.trackableId})");
        }

        private void SpawnAxis(Transform parent, Vector3 direction, Color color, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = direction * (axisLength * 0.5f);
            go.transform.localScale =
                Vector3.one * axisThickness + direction * (axisLength - axisThickness);
            ApplyMaterial(go, color);
            Destroy(go.GetComponent<Collider>());
        }

        private void SpawnMarker(Transform parent, Vector3 localPos, Color color, float size, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * size;
            ApplyMaterial(go, color);
            Destroy(go.GetComponent<Collider>());
        }

        private void ApplyMaterial(GameObject go, Color color)
        {
            // axisMaterialTemplate（シーンで自作Unlitシェーダーのマテリアルを配線済み）を複製して使う。
            // 万一未配線でも、自作シェーダーは AlwaysIncludedShaders 登録済みなので Shader.Find で確実に取れる。
            Material mat;
            if (axisMaterialTemplate != null)
            {
                mat = new Material(axisMaterialTemplate);
            }
            else
            {
                var shader = Shader.Find("WorldLocator/Unlit") ?? Shader.Find("Unlit/Color");
                if (shader == null)
                    Debug.LogError("[WorldLocatorSpawner] シェーダーが見つかりません（ビルドから除外された可能性）。");
                mat = new Material(shader);
            }
            mat.color = color;
            go.GetComponent<Renderer>().material = mat;
        }
    }
}
