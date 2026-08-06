using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using ARSolution;

namespace ARSolutionEditor
{
    /// <summary>
    /// テディベア(Bear.fbx)を使ったP0キャリブレーション検証シーンを自動構築する。
    /// メニュー: AR Solution > Setup Test Scene
    /// 手動でAR Session / Canvas / スクリプト参照配線を組む手間を省く。
    /// </summary>
    public static class ARSceneSetup
    {
        private const string BearFbxPath = "Assets/Models/TeddyBear/Bear.fbx";

        [MenuItem("AR Solution/Setup Test Scene")]
        public static void SetupScene()
        {
            // --- AR Session / AR Session Origin / AR Camera ---
            GameObject sessionGO = GameObject.Find("AR Session");
            if (sessionGO == null) sessionGO = new GameObject("AR Session");
            if (sessionGO.GetComponent<ARSession>() == null) sessionGO.AddComponent<ARSession>();
            if (sessionGO.GetComponent<ARInputManager>() == null) sessionGO.AddComponent<ARInputManager>();

            GameObject originGO = GameObject.Find("AR Session Origin");
            if (originGO == null) originGO = new GameObject("AR Session Origin");
            ARSessionOrigin sessionOrigin = originGO.GetComponent<ARSessionOrigin>();
            if (sessionOrigin == null) sessionOrigin = originGO.AddComponent<ARSessionOrigin>();

            // 前回の途中失敗などで Camera を欠いた不完全な "AR Camera" が残っている場合は作り直す
            GameObject camGO = GameObject.Find("AR Camera");
            if (camGO != null && camGO.GetComponent<Camera>() == null)
            {
                Object.DestroyImmediate(camGO);
                camGO = null;
            }
            if (camGO == null)
            {
                camGO = new GameObject("AR Camera");
                camGO.transform.SetParent(originGO.transform);
            }
            Camera cam = camGO.GetComponent<Camera>();
            if (cam == null) cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            if (camGO.GetComponent<ARCameraManager>() == null) camGO.AddComponent<ARCameraManager>();
            if (camGO.GetComponent<ARCameraBackground>() == null) camGO.AddComponent<ARCameraBackground>();
            sessionOrigin.camera = cam;

            // 既存の "Main Camera"（テストシーンの初期カメラ）は無効化して重複を避ける
            var oldCam = GameObject.Find("Main Camera");
            if (oldCam != null && oldCam != camGO) Object.DestroyImmediate(oldCam);

            // --- Bear モデル（キャリブレーション対象）配置 ---
            var bearPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BearFbxPath);
            GameObject bearGO = GameObject.Find("Bear_ReferenceTarget");
            if (bearGO == null)
            {
                if (bearPrefabAsset == null)
                {
                    Debug.LogError($"[ARSceneSetup] Bear.fbx が見つかりません: {BearFbxPath}");
                }
                else
                {
                    bearGO = (GameObject)PrefabUtility.InstantiatePrefab(bearPrefabAsset);
                    bearGO.name = "Bear_ReferenceTarget";
                    bearGO.transform.SetParent(originGO.transform);
                    bearGO.transform.localPosition = new Vector3(0f, 0f, 1.0f); // カメラ正面1m
                }
            }

            // --- Canvas / UI ---
            GameObject canvasGO = GameObject.Find("AR UI Canvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("AR UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = canvasGO.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            if (GameObject.Find("EventSystem") == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            GameObject calibRoot = FindOrCreateChild(canvasGO.transform, "CalibrationUI");
            Button confirmButton = FindOrCreateButton(calibRoot.transform, "ConfirmButton", "位置を確定", new Vector2(0, -300));

            GameObject exhibitRoot = FindOrCreateChild(canvasGO.transform, "ExhibitUI");
            Button toggleButton = FindOrCreateButton(exhibitRoot.transform, "ToggleBodyButton", "テディベア ON/OFF", new Vector2(0, -300));

            // --- CalibrationController 配線 ---
            GameObject controllerGO = GameObject.Find("ARController");
            if (controllerGO == null) controllerGO = new GameObject("ARController");
            CalibrationController calib = controllerGO.GetComponent<CalibrationController>();
            if (calib == null) calib = controllerGO.AddComponent<CalibrationController>();
            var calibSO = new SerializedObject(calib);
            calibSO.FindProperty("referenceTarget").objectReferenceValue = bearGO != null ? bearGO.transform : null;
            calibSO.FindProperty("calibrationUIRoot").objectReferenceValue = calibRoot;
            calibSO.FindProperty("exhibitUIRoot").objectReferenceValue = exhibitRoot;
            calibSO.ApplyModifiedPropertiesWithoutUndo();

            LayerToggleController layerCtrl = controllerGO.GetComponent<LayerToggleController>();
            if (layerCtrl == null) layerCtrl = controllerGO.AddComponent<LayerToggleController>();
            var layerSO = new SerializedObject(layerCtrl);
            var layersProp = layerSO.FindProperty("layers");
            layersProp.arraySize = 1;
            var elem0 = layersProp.GetArrayElementAtIndex(0);
            elem0.FindPropertyRelative("label").stringValue = "Body";
            elem0.FindPropertyRelative("target").objectReferenceValue = bearGO;
            layerSO.ApplyModifiedPropertiesWithoutUndo();

            // ボタンのonClickを配線（エディタ上で永続的に）
            UnityEditor.Events.UnityEventTools.AddPersistentListener(confirmButton.onClick, calib.ConfirmCalibration);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(toggleButton.onClick, layerCtrl.ToggleBody);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[ARSceneSetup] セットアップ完了。Hierarchy を確認し、シーンを保存してください（Cmd+S）。");
        }

        private static GameObject FindOrCreateChild(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t.gameObject;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        private static Button FindOrCreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();

            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 100);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }
    }
}
