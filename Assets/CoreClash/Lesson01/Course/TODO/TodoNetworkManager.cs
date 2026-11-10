using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CoreClash.Lesson01.Course
{
    // 세 강의에서 작성할 네트워크 핵심 코드입니다.
    public class TodoNetworkManager : TodoRunnerCallbacks
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private TodoRoomUI roomUI;

        private NetworkRunner runner;
        private string localNickname;
        private readonly Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();

        // 로비, 방 목록, 방 만들기와 입장 
        public async void JoinLobby()
        {
           
        }

        public override void OnSessionListUpdated(NetworkRunner networkRunner, List<SessionInfo> sessionList)
        {
            List<string> rooms = new();
            foreach (SessionInfo session in sessionList)
            {
                if (!session.IsVisible || !session.IsOpen) continue;
                rooms.Add($"{session.Name}  ({session.PlayerCount}/{session.MaxPlayers})");
            }

            roomUI.ShowRooms(rooms);
        }

        public async void StartRoom(string roomName, string nickname, bool createRoom)
        {
            roomName = roomName.Trim();
            nickname = nickname.Trim();

            if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(nickname))
            {
                roomUI.SetStatus("닉네임과 방 이름을 모두 입력해 주세요");
                return;
            }

            localNickname = nickname;
            roomUI.SetConnected(true);

            if (runner != null)
            {
                await runner.Shutdown();
            }

            runner = CreateRunner();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);

            NetworkSceneManagerDefault sceneManger = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            StartGameResult result = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManger,
                IsOpen = true,
                IsVisible = true,
                PlayerCount = 8
            });

            roomUI.SetConnected(result.Ok);
            roomUI.SetStatus(result.Ok ? $"{roomName} 방에 연결 되었습니다. ." : $"연결 실패 : {result.ShutdownReason}");

        }

        // PlayerRef, 권한, Spawn, 닉네임 
        public override void OnPlayerJoined(NetworkRunner networkRunner, PlayerRef player)
        {
            if (player != networkRunner.LocalPlayer || playerPrefab == null) return;

            float x = (player.RawEncoded % 5 - 2) * 1.5f;
            NetworkObject prefabObject = playerPrefab.GetComponent<NetworkObject>();
            NetworkObject spawned = networkRunner.Spawn(prefabObject, new Vector3(x, 1f, 0f), Quaternion.identity, player,
                (_, obj) => obj.GetComponent<TodoPlayer>().SetInitialName(localNickname));

            spawnedPlayers[player] = spawned;                  
        }

        public override void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef player)
        {
            if (!spawnedPlayers.TryGetValue(player, out NetworkObject playerObject)) return;

            if (playerObject.HasStateAuthority)
                networkRunner.Despawn(playerObject);
            spawnedPlayers.Remove(player);
        }

        public override void OnShutdown(NetworkRunner networkRunner, ShutdownReason reason)
        {
            spawnedPlayers.Clear();
            roomUI.SetConnected(false);
            roomUI.SetStatus($"연결 종료: {reason}");
            runner = null;
        }

        public async void Disconnect()
        {
            if (runner == null) return;
            await runner.Shutdown();
        }

        // 입력 패킷 전송과 수신
        public override void OnInput(NetworkRunner networkRunner, NetworkInput input)
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 move = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) move.y += 1f;
                if (keyboard.sKey.isPressed) move.y -= 1f;
                if (keyboard.aKey.isPressed) move.x -= 1f;
                if (keyboard.dKey.isPressed) move.x += 1f;
            }

            input.Set(new TodoInputData
            {
                Move = Vector2.ClampMagnitude(move, 1f)
            });
        }

        private static NetworkRunner CreateRunner()
        {
            GameObject runnerObject = new GameObject("TodoNetworkRunner");
            return runnerObject.AddComponent<NetworkRunner>();
        }
    }
}
