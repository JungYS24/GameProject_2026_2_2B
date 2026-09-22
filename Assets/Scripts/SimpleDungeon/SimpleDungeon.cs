using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleDungeon : MonoBehaviour
{
    [Header("Dungeon")]
    [Min(2)] public int roomCount = 8;
    [Min(3)] public int minRoomSize = 4;
    [Min(3)] public int maxRoomSize = 7;
    [Min(5)] public int roomSpacing = 8;
    [Min(10)] public int maxPlacementAttempts = 200;
    public bool generateOnStart = true;

    [Header("Kenney Models")]
    [Tooltip("template-floor.fbx 또는 바닥 프리팹")]
    public GameObject floorModel;
    [Tooltip("template-wall.fbx 또는 벽 프리팹")]
    public GameObject wallModel;
    [Tooltip("보물로 사용할 모델. 비어 있으면 Cube를 사용합니다.")]
    public GameObject treasureModel;
    [Min(0.1f)] public float tileSize = 2f;
    [Min(0.02f)] public float wallThickness = 0.2f;
    [Tooltip("FBX 벽의 기본 방향이 맞지 않을 때 90도 단위로 조정합니다.")]
    public float wallRotationOffset;
    public bool automaticallyFitModels = true;

    [Header("Objects")]
    public bool spawnMarkers = true;
    [Min(0)] public int enemiesPerNormalRoom = 2;

    private readonly Dictionary<Vector2Int, Room> rooms = new(); // 방의 중심 좌표를 키로 사용하여 각 룸 정보를 저장
    private readonly HashSet<Vector2Int> floors = new(); // 방과 복도를 포함한 모든 바닥 좌표를 중복 없이 저장합니다
    private readonly HashSet<Vector2Int> roomFloors = new(); // 복도를 제외한 방 자체의 바닥 좌표만 저장합니다
    private Transform generatedRoot; //생성된 던전 오브젝트를의 부모 트랜스폼을 저장합니다

    private static readonly Vector2Int[] Directions =   //던전 탐색에 사용할 상하좌우 내 방향으로 배열을 저장
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        // R 키를 누르면 던전을 다시 생성한다.
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            Generate();
    }

    [ContextMenu("Generate Dungeon")]
    public void Generate()
    {
        ClearGenerated();                //기존 생성되어있던 던전 삭제
        CreateRoomsAndCorridors();           // 방을 생성하고 방 사이를 복도로 연결
        AssignSpecialRooms();
        RenderDungeon(); //던전을 렌더링 한다

        if (spawnMarkers) SpawnRoomMarkers(); //마커 생성 옵션이 켜져 있으면 적과 보물등을 생성한다
    }

    private void AssignSpecialRooms()                 //생성된 일반 방 중에서 보스방과 보물방을 지정 합니다.
    {
        Room boss = null;
        int farthest = -1;                          //가장 먼 거리 변수
        List<Room> normalRooms = new();             //보물방 후보

        foreach (Room room in rooms.Values)          //생성된 모든 방을 검사
        {
            if (room.type == RoomType.Start) continue;              //시작방은 보스방에서 제외

            int distance = Mathf.Abs(room.center.x) + Mathf.Abs(room.center.y); //원점에서 방 중심 가지 거리 계산

            if (distance > farthest)                //가장 먼방 찾기
            {
                farthest = distance;
                boss = room;                        //후보로 설정
            }
        }

        if (boss != null) boss.type = RoomType.Boss;

        foreach (Room room in rooms.Values)                          //생성된 방을 다시 순회
        {
            if (room.type == RoomType.Normal) normalRooms.Add(room); //남아 있는 일반 방만 보물방 후보 목록에 추가
        }

        int treasureCount = Mathf.Min(Mathf.Max(1, rooms.Count / 4), normalRooms.Count);     //전체 방의 약 1/4을 보물방으로 정한다.

        for (int i = 0; i < treasureCount; i++)
        {
            int index = Random.Range(0, normalRooms.Count);
            normalRooms[index].type = RoomType.Treasure;            //선택된 일반 방을 보물방으로 변경
            normalRooms.RemoveAt(index);                            //후보에서 제거한다. (중복되지 않게)
        }
    }

    private void CreateRoomsAndCorridors()
    {
        int startSize = Random.Range(minRoomSize, maxRoomSize + 1);

        TryAddRoom(Vector2Int.zero, startSize, RoomType.Start);        //던전의 원점에 시작 방을 생성 한다.

        int attempts = 0;                                             //현재까지 방 배치를 시도한 횟수

        while (rooms.Count < roomCount && attempts++ < maxPlacementAttempts) //목표 방 개수에 도달하거나 최대 시도 횟수를 넘을 때까지 반복
        {
            List<Room> existingRooms = new(rooms.Values);              //현재 생성되어 있는 방들을 List로 복사
            Room parent = existingRooms[Random.Range(0, existingRooms.Count)]; //기존 방 중 하나를 랜덤하게 선택하여 새 방의 부모방으로 사용한다
            List<Vector2Int> directions = ShuffledDirections();        //상하좌우 방향의 검사 순서를 랜덤하게 섞습니다

            foreach (Vector2Int direction in directions)                //랜덤하게 섞인 네 방향을 하나씩 검사합니다.
            {
                Vector2Int center = parent.center + direction * roomSpacing;  //부모방 중심에서 해당 방향으로 roomSpacing 만큼 떨어진 새 중심점을 계산
                int size = Random.Range(minRoomSize, maxRoomSize + 1);

                if (!TryAddRoom(center, size, RoomType.Normal)) continue;     //새 방이 기존 방과 겹쳐 생성할 수 없으면 다음 방향을 검사

                CreateCorridor(parent.center, center);                 //부모 방 중심과 새 방 중심 사이에 복도를 만듭니다.

                break;                                                 //방 하나가 성공적으로 추가 됐으므로 방향 반복을 종료
            }
        }

        if (rooms.Count < roomCount)
            Debug.LogWarning($"방 {roomCount} 개 중 {rooms.Count} 개를 생성 했습니다. ");
    }

    private static List<Vector2Int> ShuffledDirections()
    {
        List<Vector2Int> result = new(Directions);    //원본 방향 배열을 수정하지 않도록 새로운 List 만든다.
        for (int i = result.Count - 1; i > 0; i--)     //뒤쪽 원소부터 하나씩 랜덤 위치와 교환 하는 Fisher-Yates 방식
        {
            int index = Random.Range(0, i + 1);       //현재 범위 안에서 교환할 임의의 인덱스를 선택
            (result[i], result[index]) = (result[index], result[i]);    //두 방향의 위치를 서로 교환합니다.
        }

        return result;
    }

    private bool TryAddRoom(Vector2Int center, int size, RoomType type)
    {
        //방 중심으로 방이 시작되는 상대 좌표를 계산
        int min = -size / 2;
        int max = min + size;

        //기본 방 주변 한칸의 여백까지 검사하여 방끼리 바로 붙지 않게 합니다.
        for (int x = min - 1; x <= max; x++)        //X 범위보다 한칸
        {
            for (int y = min - 1; y <= max; y++)    //Y 범위 보다 한칸
            {
                if (roomFloors.Contains(center + new Vector2Int(x, y))) return false;
                //검사하려는 좌표가 기본 방 바닥에 포함되어 있으면 방 생성을 실패
            }
        }

        Room room = new Room(center, size, type);
        rooms.Add(center, room);                   //방의 중심 좌표를 Key로 사용하여 Dictionary에 방을 추가합니다.

        for (int x = min; x < max; x++)
        {
            for (int y = min; y < max; y++)
            {
                Vector2Int cell = center + new Vector2Int(x, y);    //방 중심에 상대 좌표를 더하여 실제 셀 좌표를 계산 합니다
                floors.Add(cell);                                   //해당 위치를 전체 던전의 바닥 목록에 추가
                roomFloors.Add(cell);                               //해당 위치를 방 전용 바닥 목록에도 추가
            }
        }

        return true;                               //정상적으로 방 생성을 했으므로 true를 반환한다.
    }
           

    private void CreateCorridor(Vector2Int start, Vector2Int end)        //시작 방과 끝 방 사이에 ㄱ 자 형태의 복도를 생성
    {
        Vector2Int current = start;

        if (Random.value < 0.5f)                                    //50% 확률로 X 축과 Y 축 중 어느 방향을 먼저 이동할지 결정 한다.
        {
            WalkX(ref current, end.x);                             //먼저 X축으로 목표 X좌표까지 이동합니다.
            WalkY(ref current, end.y);                             //그 다음 Y축으로 목표 Y좌표까지 이동합니다.
        }
        else
        {
            WalkY(ref current, end.y);                             //그 다음 Y축으로 목표 Y좌표까지 이동합니다.
            WalkX(ref current, end.x);                             //먼저 X축으로 목표 X좌표까지 이동합니다.
        }

        floors.Add(end);                                           //마지막 도착 위치도 바닥에 포함 시킨다.
    }

    private void WalkX(ref Vector2Int current, int target)
    {
        //현재 X좌표가 목표 X 좌표와 같아질 때까지 반복
        while (current.x != target)
        {
            floors.Add(current);
            current.x += current.x < target ? 1 : -1;        //목표가 오른쪽이면 +1, 왼쪽이면 -1씩 X 좌표를 이동
        }
    }

    private void WalkY(ref Vector2Int current, int target)
    {
        //현재 Y좌표가 목표 Y 좌표와 같아질 때까지 반복
        while (current.y != target)
        {
            floors.Add(current);
            current.y += current.y < target ? 1 : -1;        //목표가 위쪽이면 +1, 아래쪽이면 -1씩 X 좌표를 이동
        }
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        // 격자 좌표를 Unity 월드 좌표로 변환한다.
        return transform.position + new Vector3(cell.x * tileSize, 0f, cell.y * tileSize);
    }

    private void ClearGenerated()
    {
        rooms.Clear();                  //저장되어 있던 모든 방 정보를 삭제 합니다.
        floors.Clear();                 //저장되어있던 모든 바닥과 복도 좌표를 삭제 합니다.
        roomFloors.Clear();             //저장되어있던 방 전용 바닥 좌표를 삭제 합니다.

        Transform oldRoot = transform.Find("Generated Dungeon");

        if (oldRoot == null) return;    //기존에 생성된 던전이 없다면 종료

        if (Application.isPlaying) Destroy(oldRoot.gameObject);
        else DestroyImmediate(oldRoot.gameObject);    //에디터 모드라면 즉시 삭제 할 수 있는 DestroyImmediate를 사용 합니다.
    }

    // 계산된 바닥과 벽 데이터를 실제 Unity GameObject로 생성합니다.
    private void RenderDungeon()
    {
        // 생성된 던전 오브젝트들을 묶어둘 부모 GameObject를 만듭니다.
        generatedRoot = new GameObject("Generated Dungeon").transform;
        // Generated Dungeon을 현재 SimpleDungeon 오브젝트의 자식으로 설정합니다.
        generatedRoot.SetParent(transform, false);

        // 저장된 모든 바닥 셀을 하나씩 순회합니다.
        foreach (Vector2Int cell in floors)
        {
            // 격자 좌표를 Unity의 실제 월드 좌표로 변환합니다.
            Vector3 center = CellToWorld(cell);
            // 해당 위치에 바닥 모델을 생성합니다.
            GameObject floor = CreateVisual(floorModel, center, Quaternion.identity, "Floor");
            // 자동 크기 조절 옵션이 켜져 있으면 바닥 크기를 타일에 맞춥니다.
            if (floor != null && automaticallyFitModels) FitFloorToTile(floor);
            // 바닥에 Collider가 없다면 자동으로 추가합니다.
            EnsureCollider(floor, true);

            // 현재 바닥 셀의 상하좌우 네 방향을 검사합니다.
            for (int side = 0; side < Directions.Length; side++)
            {
                // 현재 검사할 방향을 가져옵니다.
                Vector2Int direction = Directions[side];
                // 해당 방향에 다른 바닥이 있다면 내부 경계이므로 벽을 만들지 않습니다.
                if (floors.Contains(cell + direction)) continue;

                // 현재 바닥의 가장자리에 벽이 배치될 월드 좌표를 계산합니다.
                Vector3 wallPosition = center + new Vector3(direction.x, 0f, direction.y) * (tileSize * 0.5f);
                // 현재 방향에 맞는 벽의 회전 각도를 계산합니다.
                float angle = GetWallAngle(direction);
                // 계산된 각도에 모델 방향 보정값을 추가하여 회전값을 만듭니다.
                Quaternion rotation = Quaternion.Euler(0f, angle + wallRotationOffset, 0f);
                // 계산한 위치와 회전값으로 벽 모델을 생성합니다.
                GameObject wall = CreateVisual(wallModel, wallPosition, rotation, "Wall");
                // 자동 크기 조절 옵션이 켜져 있으면 벽 크기를 타일에 맞춥니다.
                if (wall != null && automaticallyFitModels) FitWallToTile(wall);
                // 벽에 Collider가 없다면 자동으로 추가합니다.
                EnsureCollider(wall, false);
            }
        }
    }

    // 현재 벽이 바깥쪽을 향하도록 필요한 Y축 회전 각도를 계산합니다.
    private float GetWallAngle(Vector2Int outwardDirection)
    {
        // 기본적으로 벽 모델이 X축 방향으로 길다고 가정합니다.
        bool modelRunsAlongX = true;

        // 벽 모델이 존재하고 Mesh Bounds를 정상적으로 계산할 수 있는지 확인합니다.
        if (wallModel != null && TryGetCombinedLocalMeshBounds(wallModel, out Bounds modelBounds))
            // X크기와 Z크기를 비교하여 모델의 긴 방향을 판별합니다.
            modelRunsAlongX = modelBounds.size.x >= modelBounds.size.z;

        // 모델의 짧은 축을 벽 바깥 방향과 맞춘다. 반대편 벽에는 180도 회전도 적용된다.
        // 모델이 X축으로 길면 forward를, Z축으로 길면 right를 바깥 방향으로 사용합니다.
        Vector3 modelOutward = modelRunsAlongX ? Vector3.forward : Vector3.right;
        // Vector2Int 방향을 Unity의 3차원 방향 Vector3로 변환합니다.
        Vector3 desiredOutward = new Vector3(outwardDirection.x, 0f, outwardDirection.y);
        // 모델 방향에서 원하는 방향까지 Y축 기준으로 필요한 회전 각도를 반환합니다.
        return Vector3.SignedAngle(modelOutward, desiredOutward, Vector3.up);
    }

    // 전달받은 모델을 생성하고 모델이 없으면 기본 Cube를 대신 생성합니다.
    private GameObject CreateVisual(GameObject model, Vector3 position, Quaternion rotation, string objectName)
    {
        // 최종적으로 생성될 GameObject를 저장할 변수입니다.
        GameObject instance;
        // 사용할 모델이 Inspector에 등록되어 있는지 확인합니다.
        if (model != null)
        {
            // 모델을 지정된 위치와 회전값으로 복제하고 generatedRoot의 자식으로 설정합니다.
            instance = Instantiate(model, position, rotation, generatedRoot);
        }
        else
        {
            // 모델이 없으면 Unity의 기본 Cube를 생성합니다.
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Cube의 위치와 회전값을 한 번에 지정합니다.
            instance.transform.SetPositionAndRotation(position, rotation);
            // 생성된 Cube를 generatedRoot의 자식으로 설정합니다.
            instance.transform.SetParent(generatedRoot);
            // 생성 대상이 Floor인지 검사하여 바닥과 벽의 크기를 다르게 설정합니다.
            instance.transform.localScale = objectName == "Floor"
                // Floor라면 넓고 얇은 형태로 만듭니다.
                ? new Vector3(tileSize, 0.1f, tileSize)
                // Wall이라면 길고 높고 얇은 형태로 만듭니다.
                : new Vector3(tileSize, 1f, 0.1f);
        }

        // Hierarchy에서 구분하기 쉽도록 오브젝트 이름을 설정합니다.
        instance.name = objectName;
        // 생성된 GameObject를 반환합니다.
        return instance;
    }

    // 바닥 모델의 실제 크기를 tileSize에 맞도록 자동 조절합니다.
    private void FitFloorToTile(GameObject instance)
    {
        // Renderer의 전체 Bounds를 구할 수 없으면 크기 조절을 중단합니다.
        if (!TryGetCombinedBounds(instance, out Bounds bounds)) return;
        // 현재 X축 크기를 기준으로 목표 tileSize에 필요한 배율을 계산합니다.
        float scaleX = bounds.size.x > 0.001f ? tileSize / bounds.size.x : 1f;
        // 현재 Z축 크기를 기준으로 목표 tileSize에 필요한 배율을 계산합니다.
        float scaleZ = bounds.size.z > 0.001f ? tileSize / bounds.size.z : 1f;
        // 기존 Scale에 계산된 X축과 Z축 배율을 곱해 적용합니다.
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, new Vector3(scaleX, 1f, scaleZ));
    }

    // 벽 모델의 길이와 두께를 tileSize와 wallThickness에 맞춥니다.
    private void FitWallToTile(GameObject instance)
    {
        // 벽의 Local Mesh Bounds를 구할 수 없으면 크기 조절을 중단합니다.
        if (!TryGetCombinedLocalMeshBounds(instance, out Bounds bounds)) return;

        // 원본 FBX 비율과 무관하게 길이와 두께를 각각 정확한 월드 크기로 맞춘다.
        // 현재 벽 오브젝트의 Scale 값을 가져옵니다.
        Vector3 scale = instance.transform.localScale;
        // X축 크기가 Z축보다 크면 X축을 벽의 길이로 판단합니다.
        if (bounds.size.x >= bounds.size.z)
        {
            // X축 크기가 0에 너무 가깝지 않은지 확인합니다.
            if (bounds.size.x > 0.001f)
                // X축 길이가 tileSize가 되도록 Scale을 보정합니다.
                scale.x *= tileSize / bounds.size.x;
            // Z축 크기가 0에 너무 가깝지 않은지 확인합니다.
            if (bounds.size.z > 0.001f)
                // Z축 두께가 wallThickness가 되도록 Scale을 보정합니다.
                scale.z *= wallThickness / bounds.size.z;
        }
        else
        {
            // Z축 크기가 0에 너무 가깝지 않은지 확인합니다.
            if (bounds.size.z > 0.001f)
                // Z축 길이가 tileSize가 되도록 Scale을 보정합니다.
                scale.z *= tileSize / bounds.size.z;
            // X축 크기가 0에 너무 가깝지 않은지 확인합니다.
            if (bounds.size.x > 0.001f)
                // X축 두께가 wallThickness가 되도록 Scale을 보정합니다.
                scale.x *= wallThickness / bounds.size.x;
        }

        // 최종적으로 계산된 Scale 값을 벽 오브젝트에 적용합니다.
        instance.transform.localScale = scale;
    }

    // 오브젝트와 자식 Renderer들을 모두 포함하는 전체 Bounds를 계산합니다.
    private static bool TryGetCombinedBounds(GameObject instance, out Bounds bounds)
    {
        // 현재 오브젝트와 자식들에게 있는 모든 Renderer를 가져옵니다.
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        // Renderer가 하나도 없으면 Bounds를 계산할 수 없습니다.
        if (renderers.Length == 0)
        {
            // 반환할 Bounds를 기본값으로 초기화합니다.
            bounds = default;
            // Bounds 계산에 실패했음을 반환합니다.
            return false;
        }

        // 첫 번째 Renderer의 Bounds를 초기 전체 영역으로 사용합니다.
        bounds = renderers[0].bounds;
        // 두 번째 Renderer부터 모든 Bounds를 기존 Bounds에 포함시킵니다.
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        // 정상적으로 전체 Bounds를 계산했음을 반환합니다.
        return true;
    }

    // 오브젝트에 Collider가 없으면 Mesh 크기에 맞는 BoxCollider를 추가합니다.
    private static void EnsureCollider(GameObject instance, bool isFloor)
    {
        // 오브젝트가 없거나 이미 Collider가 존재하면 추가 작업을 하지 않습니다.
        if (instance == null || instance.GetComponentInChildren<Collider>() != null) return;
        // Mesh의 Local Bounds를 계산할 수 없다면 Collider 생성을 중단합니다.
        if (!TryGetCombinedLocalMeshBounds(instance, out Bounds localBounds)) return;

        // 현재 오브젝트에 새로운 BoxCollider를 추가합니다.
        BoxCollider box = instance.AddComponent<BoxCollider>();
        // Collider의 중심점을 Mesh Bounds의 중심점과 맞춥니다.
        box.center = localBounds.center;
        // Collider의 크기를 Mesh Bounds의 크기와 맞춥니다.
        box.size = localBounds.size;

        // 바닥 Collider의 높이가 너무 얇은지 확인합니다.
        if (isFloor && box.size.y < 0.1f)
            // 바닥 Collider의 Y크기가 최소 0.1이 되도록 보정합니다.
            box.size = new Vector3(box.size.x, 0.1f, box.size.z);
    }

    // 자식 Mesh들의 Bounds를 root 기준 Local 좌표계로 합쳐 계산합니다.
    private static bool TryGetCombinedLocalMeshBounds(GameObject root, out Bounds bounds)
    {
        // root와 모든 자식 오브젝트의 MeshFilter를 가져옵니다.
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
        // 유효한 Mesh Bounds를 하나라도 찾았는지 기록합니다.
        bool hasBounds = false;
        // 반환할 Bounds를 기본값으로 초기화합니다.
        bounds = default;

        // 찾은 모든 MeshFilter를 하나씩 검사합니다.
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // MeshFilter에 실제 Mesh가 연결되어 있지 않으면 건너뜁니다.
            if (meshFilter.sharedMesh == null) continue;

            // 현재 Mesh가 자체적으로 가지고 있는 Local Bounds를 가져옵니다.
            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            // MeshFilter의 좌표를 root 기준 Local 좌표로 변환할 행렬을 계산합니다.
            Matrix4x4 meshToRoot = root.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;

            // Bounds의 왼쪽과 오른쪽 X좌표를 검사합니다.
            for (int x = -1; x <= 1; x += 2)
                // Bounds의 아래쪽과 위쪽 Y좌표를 검사합니다.
                for (int y = -1; y <= 1; y += 2)
                    // Bounds의 앞쪽과 뒤쪽 Z좌표를 검사하여 총 8개의 꼭짓점을 확인합니다.
                    for (int z = -1; z <= 1; z += 2)
                    {
                        // Bounds 중심에서 extents만큼 이동하여 현재 꼭짓점의 좌표를 계산합니다.
                        Vector3 meshCorner = meshBounds.center + Vector3.Scale(
                            // Bounds의 중심에서 각 축 끝까지의 절반 크기를 사용합니다.
                            meshBounds.extents,
                            // 현재 검사 중인 X, Y, Z 방향을 지정합니다.
                            new Vector3(x, y, z));
                        // Mesh의 꼭짓점 좌표를 root 기준 Local 좌표로 변환합니다.
                        Vector3 rootCorner = meshToRoot.MultiplyPoint3x4(meshCorner);

                        // 아직 첫 번째 Bounds조차 만들어지지 않았는지 확인합니다.
                        if (!hasBounds)
                        {
                            // 첫 번째 꼭짓점 위치를 중심으로 크기 0인 Bounds를 만듭니다.
                            bounds = new Bounds(rootCorner, Vector3.zero);
                            // 이제 Bounds가 생성되었다는 상태를 기록합니다.
                            hasBounds = true;
                        }
                        else
                        {
                            // 기존 Bounds가 현재 꼭짓점까지 포함하도록 영역을 확장합니다.
                            bounds.Encapsulate(rootCorner);
                        }
                    }
        }

        // 유효한 Mesh를 찾아 Bounds 계산에 성공했는지 반환합니다.
        return hasBounds;
    }

    // 방의 종류에 따라 보물, 일반 적, 보스 마커를 생성합니다.
    private void SpawnRoomMarkers()
    {
        // 생성된 모든 방을 하나씩 확인합니다.
        foreach (Room room in rooms.Values)
        {
            // 시작 방에는 별도의 오브젝트를 생성하지 않습니다.
            if (room.type == RoomType.Start) continue;

            // 방 중심의 월드 좌표에서 약간 위쪽에 오브젝트를 배치합니다.
            Vector3 position = CellToWorld(room.center) + Vector3.up * 0.6f;
            // 현재 방이 보물방인지 확인합니다.
            if (room.type == RoomType.Treasure)
            {
                // 지정된 보물 모델을 방 중심에 생성합니다.
                GameObject treasure = CreateVisual(treasureModel, position, Quaternion.identity, "Treasure");
                // 기존 Collider가 있으면 사용하고 없으면 BoxCollider를 추가합니다.
                Collider trigger = treasure.GetComponent<Collider>() ?? treasure.AddComponent<BoxCollider>();
                // 보물 Collider를 물리 충돌용이 아닌 Trigger 용도로 설정합니다.
                trigger.isTrigger = true;
                // TreasureItem 컴포넌트가 없으면 새로 추가합니다.
                if (treasure.GetComponent<TreasureItem>() == null) treasure.AddComponent<TreasureItem>();
                // 보물 오브젝트의 색상을 해당 방의 색상으로 변경합니다.
                Tint(treasure, room.Color);
            }
            else
            {
                // 보스방이면 1개, 일반방이면 설정한 수만큼 마커를 생성합니다.
                int count = room.type == RoomType.Boss ? 1 : enemiesPerNormalRoom;
                // 생성해야 할 적 또는 보스 마커의 개수만큼 반복합니다.
                for (int i = 0; i < count; i++)
                {
                    // 보스는 Cube를 사용하고 일반 적은 Sphere를 사용하여 생성합니다.
                    GameObject marker = GameObject.CreatePrimitive(room.type == RoomType.Boss
                        // 보스방이면 Cube 타입을 선택합니다.
                        ? PrimitiveType.Cube : PrimitiveType.Sphere);
                    // 방 종류에 따라 Hierarchy에 표시할 마커 이름을 결정합니다.
                    marker.name = room.type == RoomType.Boss ? "Boss Marker" : "Enemy Marker";
                    // 생성된 마커를 Generated Dungeon 오브젝트의 자식으로 설정합니다.
                    marker.transform.SetParent(generatedRoot);
                    // 여러 적이 겹치지 않도록 X축으로 0.7씩 떨어뜨려 배치합니다.
                    marker.transform.position = position + new Vector3(i * 0.7f, 0f, 0f);
                    // 보스는 크게 만들고 일반 적은 작게 만들어 구분합니다.
                    marker.transform.localScale = room.type == RoomType.Boss ? Vector3.one * 1.5f : Vector3.one * 0.6f;
                    // 마커의 색상을 해당 방의 색상으로 변경합니다.
                    Tint(marker, room.Color);
                }
            }
        }
    }

    // 전달받은 GameObject와 자식 Renderer들의 색상을 변경합니다.
    private static void Tint(GameObject target, Color color)
    {
        // target과 자식에 있는 모든 Renderer를 하나씩 가져옵니다.
        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            // 각 Renderer의 Material 색상을 지정된 색상으로 변경합니다.
            renderer.material.color = color;
    }
}
  