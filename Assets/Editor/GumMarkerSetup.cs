using System.IO;
using UnityEditor;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;

namespace ARSolutionEditor
{
    /// <summary>
    /// 新マーカー(gum1022.arobject)をプロジェクトに取り込み、既存のKettleReferenceObjectLibraryへ
    /// "gum"エントリとして追加する。Test1022_gum.unityは既にこのライブラリ(guid 83a300ee...)を
    /// 参照済みのため、ライブラリへの追加だけで検知可能になる。
    /// CLIから `-executeMethod ARSolutionEditor.GumMarkerSetup.ImportAndRegister` で実行する。
    /// </summary>
    public static class GumMarkerSetup
    {
        private const string SourceArobjectPath = "/Users/tqy/Dropbox/melon/ar-solution/models/gum1022.arobject";
        private const string DestFolder = "Assets/Models/Gum";
        private const string DestArobjectPath = DestFolder + "/gum1022.arobject";
        private const string LibraryPath = "Assets/XR/KettleReferenceObjectLibrary.asset";
        private const string EntryName = "gum";

        [MenuItem("AR Solution/Import Gum Marker and Register")]
        public static void ImportAndRegister()
        {
            if (!AssetDatabase.IsValidFolder(DestFolder))
                AssetDatabase.CreateFolder("Assets/Models", "Gum");

            File.Copy(SourceArobjectPath, DestArobjectPath, overwrite: true);
            AssetDatabase.ImportAsset(DestArobjectPath, ImportAssetOptions.ForceSynchronousImport);

            var entry = AssetDatabase.LoadAssetAtPath<ARKitReferenceObjectEntry>(DestArobjectPath);
            if (entry == null)
            {
                Debug.LogError($"[GumMarkerSetup] {DestArobjectPath} からARKitReferenceObjectEntryを読み込めませんでした。");
                return;
            }

            var library = AssetDatabase.LoadAssetAtPath<XRReferenceObjectLibrary>(LibraryPath);
            if (library == null)
            {
                Debug.LogError($"[GumMarkerSetup] ライブラリが見つかりません: {LibraryPath}");
                return;
            }

            // 既に同名エントリがあれば置き換え、無ければ新規追加
            var index = -1;
            for (var i = 0; i < library.count; i++)
            {
                if (library[i].name == EntryName) { index = i; break; }
            }
            if (index < 0) index = library.Add();

            library.SetReferenceObjectName(index, EntryName);
            library.SetReferenceObjectEntry(index, typeof(ARKitReferenceObjectEntry), entry);

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GumMarkerSetup] '{EntryName}' をindex={index}として{LibraryPath}へ登録しました。" +
                      $"referenceOrigin={entry.referenceOrigin}");
        }
    }
}
