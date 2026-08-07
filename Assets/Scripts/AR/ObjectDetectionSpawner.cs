using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARSolution
{
    /// <summary>
    /// ARKit Object Detection（.arobjectスキャン→自動認識）で対象物が見つかったら、
    /// その場にARコンテンツをInstantiateする。手動アラインメント(CalibrationController)の後継。
    /// 認識方式の決定経緯は docs/AR_CALIBRATION_ARCHITECTURE.md「認識方式の決定変更」参照。
    /// </summary>
    [RequireComponent(typeof(ARTrackedObjectManager))]
    public class ObjectDetectionSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject contentPrefab;
        [SerializeField] private bool removeContentWhenLost = false;

        private ARTrackedObjectManager _trackedObjectManager;
        private readonly Dictionary<TrackableId, GameObject> _spawnedContent = new();

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
                    content.transform.SetPositionAndRotation(
                        trackedObject.transform.position, trackedObject.transform.rotation);
                    // Tracking直後は数フレームLimitedを経由することがあるため、
                    // Noneのときだけ隠す（Trackingでの厳密一致だと検出直後に消えてしまう）
                    content.SetActive(trackedObject.trackingState != TrackingState.None);
                }
            }

            foreach (var trackedObject in args.removed)
            {
                if (!_spawnedContent.TryGetValue(trackedObject.trackableId, out var content)) continue;

                if (removeContentWhenLost)
                {
                    Destroy(content);
                    _spawnedContent.Remove(trackedObject.trackableId);
                }
                else
                {
                    content.SetActive(false);
                }
            }
        }

        private void Spawn(ARTrackedObject trackedObject)
        {
            if (contentPrefab == null || _spawnedContent.ContainsKey(trackedObject.trackableId)) return;

            var content = Instantiate(contentPrefab, trackedObject.transform.position,
                trackedObject.transform.rotation, trackedObject.transform);
            _spawnedContent[trackedObject.trackableId] = content;

            Debug.Log($"[ObjectDetectionSpawner] Detected '{trackedObject.referenceObject.name}' " +
                      $"at {trackedObject.transform.position}");
        }
    }
}
