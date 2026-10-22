using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ARSolution;

namespace ARSolutionEditor
{
    /// <summary>
    /// オフセット原因調査（H2仮説: contentOffset={0,3,0}はコミットされたままの古いデバッグ値）の検証用に、
    /// Test0818.unityを複製してcontentOffsetだけゼロに戻したシーンを作る。
    /// 元のTest0818.unityは過去の記録として変更しない（非破壊）。
    /// CLIから `-executeMethod ARSolutionEditor.H2BaselineSceneSetup.CreateBaseline` で実行する。
    /// メニュー: AR Solution > Create H2 Baseline Scene (contentOffset=0)
    /// </summary>
    public static class H2BaselineSceneSetup
    {
        private const string SourceScenePath = "Assets/Scene/Test0818.unity";
        private const string BaselineScenePath = "Assets/Scene/Test0818_H2baseline.unity";

        [MenuItem("AR Solution/Create H2 Baseline Scene (contentOffset=0)")]
        public static void CreateBaseline()
        {
            if (!AssetDatabase.CopyAsset(SourceScenePath, BaselineScenePath))
            {
                Debug.LogError($"[H2BaselineSceneSetup] シーンの複製に失敗: {SourceScenePath} -> {BaselineScenePath}");
                return;
            }
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(BaselineScenePath, OpenSceneMode.Single);

            var spawner = Object.FindObjectOfType<ObjectDetectionSpawner>();
            if (spawner == null)
            {
                Debug.LogError("[H2BaselineSceneSetup] ObjectDetectionSpawnerが複製シーン内に見つかりません。");
                return;
            }

            var so = new SerializedObject(spawner);
            var offsetProp = so.FindProperty("contentOffset");
            var before = offsetProp.vector3Value;
            offsetProp.vector3Value = Vector3.zero;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[H2BaselineSceneSetup] {BaselineScenePath} を作成。contentOffset: {before} -> {Vector3.zero}。" +
                      "Build SettingsにこのシーンをSceneTest0818の代わりに追加してビルドしてください。");
        }
    }
}
