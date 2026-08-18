using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using ARSolution;

namespace ARSolutionEditor
{
    /// <summary>
    /// World Anchor座標固定の最小検証シーンを自動構築する。既存のARSceneSetup（Bear/Calibration用）とは
    /// 独立した専用シーンで、物体認識を経由せず「ボタン→カメラ前方固定距離→Anchor」だけを確認する。
    /// メニュー: AR Solution > Setup World Locator Test Scene
    /// </summary>
    public static class WorldLocatorTestSceneSetup
    {
        [MenuItem("AR Solution/Setup World Locator Test Scene")]
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
            if (originGO.GetComponent<ARAnchorManager>() == null) originGO.AddComponent<ARAnchorManager>();

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

            // TrackedPoseDriverが無いとARKitのデバイス姿勢がCamera.transformへ反映されず、
            // ワールド座標計算(WorldLocatorSpawner)もカメラの見た目もiPadの動きに追随しない。
            var trackedPoseDriver = camGO.GetComponent<TrackedPoseDriver>();
            if (trackedPoseDriver == null) trackedPoseDriver = camGO.AddComponent<TrackedPoseDriver>();
            // ⚠️ 重要: iOS実機（ARKit ハンドヘルドAR）では <XRHMD> デバイスは生成されない。
            // ARKitのInputLayoutLoaderが登録するのは HandheldARInputDevice（product "(ARKit)"）で、
            // 姿勢はその devicePosition / deviceRotation コントロールに流れる。<XRHMD>/centerEye* だけを
            // バインドすると実機では解決先が無くTrackedPoseDriverは姿勢を1度も受け取らない（＝カメラが
            // 原点固定のまま・Spawn時に (0,0,1) が記録される既知の症状）。XROrigin.Awake の警告は
            // 「TrackedPoseDriverコンポーネントの有無」しか見ないため、誤バインドでも警告だけは消える。
            // → AR Foundation純正メニュー（XROriginCreateUtil.CreateARMainCamera）と全く同じく、
            //   1アクションにXRHMD（エディタ/他XR用）とHandheldARInputDevice（iOS実機用）の両方をバインドする。
            var positionAction = new UnityEngine.InputSystem.InputAction(
                "Position", binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3");
            positionAction.AddBinding("<HandheldARInputDevice>/devicePosition");
            var rotationAction = new UnityEngine.InputSystem.InputAction(
                "Rotation", binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion");
            rotationAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
            trackedPoseDriver.positionInput = new UnityEngine.InputSystem.InputActionProperty(positionAction);
            trackedPoseDriver.rotationInput = new UnityEngine.InputSystem.InputActionProperty(rotationAction);
            trackedPoseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            trackedPoseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

            var oldCam = GameObject.Find("Main Camera");
            if (oldCam != null && oldCam != camGO) Object.DestroyImmediate(oldCam);

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

            Button spawnButton = FindOrCreateButton(canvasGO.transform, "SpawnLocatorButton", "ロケータを配置", new Vector2(0, -300));

            // --- WorldLocatorSpawner 配線 ---
            GameObject spawnerGO = GameObject.Find("WorldLocatorSpawner");
            if (spawnerGO == null) spawnerGO = new GameObject("WorldLocatorSpawner");
            WorldLocatorSpawner spawner = spawnerGO.GetComponent<WorldLocatorSpawner>();
            if (spawner == null) spawner = spawnerGO.AddComponent<WorldLocatorSpawner>();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(spawnButton.onClick, spawner.SpawnLocator);

            // 実機ビルドでランタイム生成マテリアルのシェーダーがストリップされピンク(shader未検出)に
            // なる問題への対策。プロジェクトアセットとして実体化し、シーン上のコンポーネントから
            // 参照させることでビルドから除外されないようにする。
            const string materialPath = "Assets/Materials/WorldLocatorAxis.mat";
            var axisMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (axisMaterial == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                    AssetDatabase.CreateFolder("Assets", "Materials");
                // 組み込みの Unlit/Color は実機ビルドで未検出→マゼンタ化するため、単一バリアントの
                // 自作シェーダー(WorldLocator/Unlit・AlwaysIncludedShaders登録済み)を使う。
                var shader = Shader.Find("WorldLocator/Unlit") ?? Shader.Find("Unlit/Color");
                axisMaterial = new Material(shader) { name = "WorldLocatorAxis" };
                AssetDatabase.CreateAsset(axisMaterial, materialPath);
            }
            var spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("axisMaterialTemplate").objectReferenceValue = axisMaterial;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[WorldLocatorTestSceneSetup] セットアップ完了。Hierarchy を確認し、シーンを保存してください（Cmd+S）。");
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
