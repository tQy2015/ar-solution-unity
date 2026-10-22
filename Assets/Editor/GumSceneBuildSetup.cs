using UnityEditor;
using UnityEngine;

namespace ARSolutionEditor
{
    /// <summary>
    /// gumマーカー検証用: Test1022_gum.unity をBuild Settingsの唯一のActive Sceneとして登録し、
    /// iOS向けXcodeプロジェクトをBuilds/iOS_gum1022へ生成する。
    /// CLIから `-executeMethod ARSolutionEditor.GumSceneBuildSetup.BuildIos` で実行する。
    /// </summary>
    public static class GumSceneBuildSetup
    {
        private const string ScenePath = "Assets/Scene/Test1022_gum.unity";
        private const string BuildOutputPath = "Builds/iOS_gum1022";

        [MenuItem("AR Solution/Build Gum Scene for iOS")]
        public static void BuildIos()
        {
            // インストール済みXcodeがiOS 14.0をサポート対象外(15.0〜)にしたため、
            // UNITY_AR_BUILD_RUNBOOK.md記載の旧推奨値(14.0)のままだと
            // "IPHONEOS_DEPLOYMENT_TARGET...range...15.0 to 27.1.x" 警告が出る。15.0へ上げる。
            PlayerSettings.iOS.targetOSVersionString = "15.0";

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Debug.Log($"[GumSceneBuildSetup] EditorBuildSettings.scenes を {ScenePath} のみに設定しました。" +
                      $" targetOSVersionString={PlayerSettings.iOS.targetOSVersionString}");

            var report = BuildPipeline.BuildPlayer(
                EditorBuildSettings.scenes,
                BuildOutputPath,
                BuildTarget.iOS,
                BuildOptions.None);

            Debug.Log($"[GumSceneBuildSetup] Build result: {report.summary.result}, " +
                      $"totalErrors={report.summary.totalErrors}, outputPath={report.summary.outputPath}");
        }
    }
}
