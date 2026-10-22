using UnityEditor;
using UnityEngine;

namespace ARSolutionEditor
{
    /// <summary>
    /// H2仮説検証用: Test0818_H2baseline.unity をBuild Settingsの唯一のActive Sceneとして登録し、
    /// iOS向けXcodeプロジェクトをBuilds/iOS_H2baselineへ生成する。
    /// CLIから `-executeMethod ARSolutionEditor.H2BaselineBuildSetup.BuildIos` で実行する。
    /// 元のEditorBuildSettings（Test0818.unity等）は実行前にログへ退避先を明示するのみで、
    /// このメソッド自体は上書きする（検証用途・必要なら元の構成に戻す別メソッドは用意しない。
    /// 戻す場合はEditor上でBuild Settingsを開き直すか、gitで ProjectSettings/EditorBuildSettings.asset を復元する）。
    /// </summary>
    public static class H2BaselineBuildSetup
    {
        private const string BaselineScenePath = "Assets/Scene/Test0818_H2baseline.unity";
        private const string BuildOutputPath = "Builds/iOS_H2baseline";

        [MenuItem("AR Solution/Build H2 Baseline Scene for iOS")]
        public static void BuildIos()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BaselineScenePath, true)
            };
            Debug.Log($"[H2BaselineBuildSetup] EditorBuildSettings.scenes を {BaselineScenePath} のみに設定しました。" +
                      "元に戻す場合は `git checkout -- ProjectSettings/EditorBuildSettings.asset` を実行してください。");

            var report = BuildPipeline.BuildPlayer(
                EditorBuildSettings.scenes,
                BuildOutputPath,
                BuildTarget.iOS,
                BuildOptions.None);

            Debug.Log($"[H2BaselineBuildSetup] Build result: {report.summary.result}, " +
                      $"totalErrors={report.summary.totalErrors}, outputPath={report.summary.outputPath}");
        }
    }
}
