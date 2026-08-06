using UnityEngine;
using UnityEngine.EventSystems;

namespace ARSolution
{
    /// <summary>
    /// P0検証: リファレンスモデル（テディベア）をタッチ操作で実物の位置に手動アラインメントし、
    /// 確定後に座標を固定する。AR_CALIBRATION_ARCHITECTURE.md の Step 1-4 に対応。
    /// </summary>
    public class CalibrationController : MonoBehaviour
    {
        [Header("対象")]
        [SerializeField] private Transform referenceTarget; // Bear モデルのルート
        [SerializeField] private GameObject calibrationUIRoot;
        [SerializeField] private GameObject exhibitUIRoot;

        [Header("操作感度")]
        [SerializeField] private float dragSpeed = 0.0025f;
        [SerializeField] private float rotateSpeed = 0.3f;
        [SerializeField] private float minScale = 0.3f;
        [SerializeField] private float maxScale = 3.0f;

        private bool _isCalibrated;
        private Vector2 _lastSingleTouchPos;
        private float _lastPinchDistance;
        private float _lastTwoFingerAngle;

        public bool IsCalibrated => _isCalibrated;

        private void Awake()
        {
            if (calibrationUIRoot != null) calibrationUIRoot.SetActive(true);
            if (exhibitUIRoot != null) exhibitUIRoot.SetActive(false);
        }

        private void Update()
        {
            if (_isCalibrated || referenceTarget == null) return;
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (IsOverUI(t.fingerId)) return;

                if (t.phase == TouchPhase.Began)
                {
                    _lastSingleTouchPos = t.position;
                }
                else if (t.phase == TouchPhase.Moved)
                {
                    Vector2 delta = t.position - _lastSingleTouchPos;
                    // カメラ相対の左右・上下移動（X/Z平面 + Y高さ）
                    Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;
                    Vector3 up = Vector3.up;
                    referenceTarget.position += right * (delta.x * dragSpeed) + up * (delta.y * dragSpeed);
                    _lastSingleTouchPos = t.position;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                float currentDistance = Vector2.Distance(t0.position, t1.position);
                float currentAngle = Mathf.Atan2(
                    (t1.position - t0.position).y,
                    (t1.position - t0.position).x) * Mathf.Rad2Deg;

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    _lastPinchDistance = currentDistance;
                    _lastTwoFingerAngle = currentAngle;
                    return;
                }

                // ピンチ: スケール調整
                if (_lastPinchDistance > 0f)
                {
                    float scaleDelta = currentDistance / _lastPinchDistance;
                    float newScale = Mathf.Clamp(
                        referenceTarget.localScale.x * scaleDelta, minScale, maxScale);
                    referenceTarget.localScale = Vector3.one * newScale;
                }

                // 回転: Y軸回転
                float angleDelta = currentAngle - _lastTwoFingerAngle;
                referenceTarget.Rotate(Vector3.up, -angleDelta * rotateSpeed, Space.World);

                _lastPinchDistance = currentDistance;
                _lastTwoFingerAngle = currentAngle;
            }
        }

        private bool IsOverUI(int fingerId)
        {
            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        /// <summary>確定ボタンから呼び出す。ワールド座標をスナップショットし展示モードへ遷移。</summary>
        public void ConfirmCalibration()
        {
            if (_isCalibrated) return;
            _isCalibrated = true;

            if (calibrationUIRoot != null) calibrationUIRoot.SetActive(false);
            if (exhibitUIRoot != null) exhibitUIRoot.SetActive(true);

            Debug.Log($"[Calibration] Confirmed at pos={referenceTarget.position} " +
                      $"rot={referenceTarget.rotation.eulerAngles} scale={referenceTarget.localScale.x}");
        }

        /// <summary>やり直す場合。キャリブレーションUIへ戻す。</summary>
        public void ResetCalibration()
        {
            _isCalibrated = false;
            if (calibrationUIRoot != null) calibrationUIRoot.SetActive(true);
            if (exhibitUIRoot != null) exhibitUIRoot.SetActive(false);
        }
    }
}
