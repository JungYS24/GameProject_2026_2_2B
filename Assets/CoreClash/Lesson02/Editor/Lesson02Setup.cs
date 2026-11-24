using CoreClash.Lesson01.Course;
using Fusion;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

namespace CoreClash.Lesson02.Editor
{
    public static class Lesson02Setup
    {
        private const string Root = "Assets/CoreClash/Lesson02";
        private const string SourceScene = "Assets/CoreClash/Lesson01/Scenes/Lesson01_TODO.unity";
        private const string SourcePlayer = "Assets/CoreClash/Lesson01/Prefabs/Lesson01TodoPlayer.prefab";
        private const string ScenePath = Root + "/Scenes/Lesson02_CoreAndMelee.unity";
        private const string PlayerPath = Root + "/Prefabs/Lesson02Player.prefab";
        private const string CorePath = Root + "/Prefabs/Lesson02Core.prefab";
        private const string SpawnerPath = Root + "/Prefabs/Lesson02CoreSpawner.prefab";
        private const string TestExe = "F:/CodexOutPut/2026-11-09/e/work/Lesson02Build/CoreClash_Lesson02.exe";

        [InitializeOnLoadMethod]
        private static void CreateOnceAfterImport()
        {
            // Keep the generated teaching scene and smoke-test build reproducible.
            EditorApplication.delayCall += () =>
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                {
                    CreateContent();
                    return;
                }
                /* 자동 빌드 및 패키지 추출 비활성화
                if (!File.Exists(TestExe))
                {
                    BuildTestPlayer();
                    ExportPackageDefault();
                }
                */
            };
        }
        Relo

        public static void CreateContent()
        {
            EnsureFolder(Root, "Materials");
            CreateCore();
            CreateSpawner();
            CreatePlayer();
            CreateScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.ExecuteMenuItem("Tools/Fusion/Rebuild Prefab Table");
            Debug.Log("Lesson02 content created: " + ScenePath);
        }

        public static void BuildTestPlayer()
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = TestExe,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Lesson02 build failed: " + report.summary.result);
            Debug.Log("Lesson02 test build created");
        }

        public static void ExportPackageDefault()
        {
            AssetDatabase.ExportPackage(Root,
                "F:/CodexOutPut/2026-11-09/e/outputs/CoreClash_Lesson02.unitypackage",
                ExportPackageOptions.Recurse);
            Debug.Log("Lesson02 package exported");
        }

        private static void CreateCore()
        {
            Delete(CorePath);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Lesson02Core";
            go.transform.localScale = Vector3.one * .65f;
            go.GetComponent<SphereCollider>().isTrigger = true;
            Rigidbody body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            go.AddComponent<NetworkObject>();
            go.AddComponent<NetworkTransform>();
            go.AddComponent<Lesson02CorePickup>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"))
                { color = new Color(.1f, 1f, 1f) };
            string materialPath = Root + "/Materials/Lesson02Core.mat";
            Delete(materialPath);
            AssetDatabase.CreateAsset(material, materialPath);
            go.GetComponent<Renderer>().sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(go, CorePath);
            Object.DestroyImmediate(go);
        }

        private static void CreateSpawner()
        {
            Delete(SpawnerPath);
            GameObject go = new GameObject("Lesson02CoreSpawner");
            go.AddComponent<NetworkObject>();
            Lesson02CoreSpawner spawner = go.AddComponent<Lesson02CoreSpawner>();
            SerializedObject data = new SerializedObject(spawner);
            data.FindProperty("corePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<NetworkObject>(CorePath);
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, SpawnerPath);
            Object.DestroyImmediate(go);
        }

        private static void CreatePlayer()
        {
            Delete(PlayerPath);
            AssetDatabase.CopyAsset(SourcePlayer, PlayerPath);
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPath);
            if (root.GetComponent<Lesson02Player>() == null) root.AddComponent<Lesson02Player>();
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void CreateScene()
        {
            Delete(ScenePath);
            AssetDatabase.CopyAsset(SourceScene, ScenePath);
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            TodoNetworkManager oldManager = Object.FindFirstObjectByType<TodoNetworkManager>();
            TodoRoomUI oldUI = Object.FindFirstObjectByType<TodoRoomUI>();
            SerializedObject oldData = new SerializedObject(oldUI);

            Object[] refs =
            {
                oldData.FindProperty("nicknameInput").objectReferenceValue,
                oldData.FindProperty("roomNameInput").objectReferenceValue,
                oldData.FindProperty("createButton").objectReferenceValue,
                oldData.FindProperty("joinButton").objectReferenceValue,
                oldData.FindProperty("refreshButton").objectReferenceValue,
                oldData.FindProperty("disconnectButton").objectReferenceValue,
                oldData.FindProperty("roomListText").objectReferenceValue,
                oldData.FindProperty("statusText").objectReferenceValue
            };

            GameObject managerObject = oldManager.gameObject;
            GameObject uiObject = oldUI.gameObject;
            Object.DestroyImmediate(oldManager);
            Object.DestroyImmediate(oldUI);
            Lesson02NetworkManager manager = managerObject.AddComponent<Lesson02NetworkManager>();
            Lesson02RoomUI ui = uiObject.AddComponent<Lesson02RoomUI>();

            SerializedObject managerData = new SerializedObject(manager);
            managerData.FindProperty("playerPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<NetworkObject>(PlayerPath);
            managerData.FindProperty("roomUI").objectReferenceValue = ui;
            managerData.ApplyModifiedPropertiesWithoutUndo();

            string[] names = { "nicknameInput", "roomNameInput", "createButton", "joinButton", "refreshButton", "disconnectButton", "roomListText", "statusText" };
            SerializedObject uiData = new SerializedObject(ui);
            for (int i = 0; i < names.Length; i++) uiData.FindProperty(names[i]).objectReferenceValue = refs[i];
            uiData.FindProperty("networkManager").objectReferenceValue = manager;
            uiData.ApplyModifiedPropertiesWithoutUndo();

            GameObject systems = new GameObject("Lesson02Systems");
            Lesson02Bootstrap bootstrap = systems.AddComponent<Lesson02Bootstrap>();
            systems.AddComponent<Lesson02HUD>();
            systems.AddComponent<Lesson02SmokeTest>();
            SerializedObject bootstrapData = new SerializedObject(bootstrap);
            bootstrapData.FindProperty("spawnerPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<NetworkObject>(SpawnerPath);
            bootstrapData.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void Delete(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
