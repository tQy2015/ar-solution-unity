using UnityEngine;

namespace ARSolution
{
    /// <summary>
    /// P0では単一レイヤー（テディベア本体）のみ。P2で骨格・筋肉・臓器 GameObject を
    /// layers に追加すれば同じ仕組みでON/OFFボタンが機能する（AR_CALIBRATION_ARCHITECTURE.md Phase P3）。
    /// </summary>
    public class LayerToggleController : MonoBehaviour
    {
        [System.Serializable]
        public struct Layer
        {
            public string label;
            public GameObject target;
        }

        [SerializeField] private Layer[] layers;

        public void ToggleLayer(int index)
        {
            if (index < 0 || index >= layers.Length) return;
            GameObject target = layers[index].target;
            if (target != null) target.SetActive(!target.activeSelf);
        }

        public void SetLayerActive(int index, bool active)
        {
            if (index < 0 || index >= layers.Length) return;
            if (layers[index].target != null) layers[index].target.SetActive(active);
        }

        /// <summary>UnityEvent永続リスナー用（ラムダ不可のため引数なしメソッドとして用意）。</summary>
        public void ToggleBody() => ToggleLayer(0);
    }
}
