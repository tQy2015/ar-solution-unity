using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ARSolution.Editor
{
    /// <summary>
    /// iOSビルドの上書き実行時、UnityのiOSBuildPostprocessorが出力先ディレクトリの削除に失敗し
    /// 「IOException: Directory not empty」が出る問題への対処。原因は.DS_Storeや
    /// com.apple.provenance等の拡張属性が残っていることが多く（過去に~/DocumentsごとNGな出力先が
    /// 選ばれた事故もあったため、パスが安全な範囲内かも確認する）、ビルド直前に出力先を
    /// 事前クリーンしておくことでUnity側の削除処理を空振りさせる。
    /// </summary>
    public class CleanBuildOutputPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var outputPath = report.summary.outputPath;
            if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath)) return;

            // 安全確認: プロジェクト外の広範囲なディレクトリ（Documents直下等）を誤って
            // 掃除しないよう、プロジェクトルート配下のパスであることを必須とする
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var fullOutputPath = Path.GetFullPath(outputPath);
            if (!fullOutputPath.StartsWith(projectRoot))
            {
                Debug.LogWarning($"[CleanBuildOutputPreprocessor] Output path is outside the project " +
                                  $"({fullOutputPath}); skipping pre-clean for safety.");
                return;
            }

            try
            {
                foreach (var path in Directory.GetFileSystemEntries(fullOutputPath))
                {
                    if (File.Exists(path)) File.Delete(path);
                    else Directory.Delete(path, true);
                }
                Debug.Log($"[CleanBuildOutputPreprocessor] Pre-cleaned build output: {fullOutputPath}");
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[CleanBuildOutputPreprocessor] Pre-clean failed (build will proceed anyway): {e.Message}");
            }
        }
    }
}
