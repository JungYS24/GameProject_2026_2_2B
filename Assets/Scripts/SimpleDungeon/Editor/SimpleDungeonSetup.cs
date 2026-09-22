using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SimpleDungeonSetup
{
    private const string FloorPath = "Assets/FBX format/template-floor.fbx";
    private const string WallPath = "Assets/FBX format/template-wall.fbx";

    [MenuItem("Tools/Simple Dungeon/Create Generator With Kenney Models")]
    public static void CreateGenerator()
    {
        GameObject floor = AssetDatabase.LoadAssetAtPath<GameObject>(FloorPath);
        GameObject wall = AssetDatabase.LoadAssetAtPath<GameObject>(WallPath);

        if (floor == null || wall == null)
        {
            EditorUtility.DisplayDialog(
                "Simple Dungeon",
                "Kenney FBX를 찾지 못했습니다. Assets/FBX format 폴더를 확인하세요.",
                "확인");
            return;
        }

        SimpleDungeon existing = Object.FindFirstObjectByType<SimpleDungeon>();
        GameObject generatorObject;

        if (existing != null)
        {
            generatorObject = existing.gameObject;
            Undo.RecordObject(existing, "Configure Simple Dungeon");
        }
        else
        {
            generatorObject = new GameObject("Dungeon Generator");
            Undo.RegisterCreatedObjectUndo(generatorObject, "Create Simple Dungeon");
            existing = generatorObject.AddComponent<SimpleDungeon>();
        }

        existing.floorModel = floor;
        existing.wallModel = wall;
        existing.tileSize = 2f;
        existing.wallThickness = 0.2f;
        existing.automaticallyFitModels = true;

        EditorUtility.SetDirty(existing);
        CreateOrConfigurePlayer();
        Selection.activeGameObject = generatorObject;
        EditorSceneManager.MarkSceneDirty(generatorObject.scene);

        EditorUtility.DisplayDialog(
            "Simple Dungeon",
            "설정이 끝났습니다. Play를 누르거나 컴포넌트 메뉴의 Generate Dungeon을 실행하세요.",
            "확인");
    }

    [MenuItem("Tools/Simple Dungeon/Create Player And Camera")]
    public static void CreateOrConfigurePlayer()
    {
        SimpleDungeonPlayer player = Object.FindFirstObjectByType<SimpleDungeonPlayer>();
        GameObject playerObject;

        if (player == null)
        {
            playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player";
            Undo.RegisterCreatedObjectUndo(playerObject, "Create Dungeon Player");

            Collider primitiveCollider = playerObject.GetComponent<Collider>();
            if (primitiveCollider != null) Object.DestroyImmediate(primitiveCollider);

            CharacterController controller = playerObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = Vector3.zero;

            player = playerObject.AddComponent<SimpleDungeonPlayer>();
            playerObject.transform.position = new Vector3(0f, 1.1f, 0f);
            playerObject.tag = "Player";
        }
        else
        {
            playerObject = player.gameObject;
            Undo.RecordObject(player, "Configure Dungeon Player");
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Dungeon Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        SimpleCameraFollow follow = camera.GetComponent<SimpleCameraFollow>();
        if (follow == null) follow = Undo.AddComponent<SimpleCameraFollow>(camera.gameObject);
        follow.target = playerObject.transform;
        camera.transform.position = playerObject.transform.position + follow.offset;
        camera.transform.LookAt(playerObject.transform.position + Vector3.up);
        player.cameraTransform = camera.transform;

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(follow);
        EditorSceneManager.MarkSceneDirty(playerObject.scene);
        Selection.activeGameObject = playerObject;
    }
}
