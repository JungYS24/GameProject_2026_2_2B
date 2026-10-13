using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DungeonGameClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://127.0.0.1:3000";
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button chestButton;
    [SerializeField] private Button restButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button returnButton;

    void Start()
    {
        enterButton.onClick.AddListener(() => StartCoroutine(Post("/api/dungeon/enter", "{}")));
        attackButton.onClick.AddListener(() => SendAction("ATTACK"));
        chestButton.onClick.AddListener(() => SendAction("OPEN_CHEST"));
        restButton.onClick.AddListener(() => SendAction("REST"));
        nextButton.onClick.AddListener(() => SendAction("NEXT_ROOM"));
        returnButton.onClick.AddListener(() => SendAction("RETURN"));
        StartCoroutine(GetState());
    }

    private void SendAction(string action)
    {
        string json = JsonUtility.ToJson(new DungeonActionRequest { action = action });
        StartCoroutine(Post("/api/dungeon/action", json));
    }

    private IEnumerator GetState()
    {
        using UnityWebRequest request = UnityWebRequest.Get(serverUrl + "/api/game/state");

        yield return request.SendWebRequest();
        ReadResponse(request);
    }

    private IEnumerator Post(string path, string json)
    {
        using UnityWebRequest request = new UnityWebRequest(serverUrl + path, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        ReadResponse(request);
    }

    private void ReadResponse(UnityWebRequest request)
    {
        string json = request.downloadHandler.text;

        DungeonResponse response = JsonUtility.FromJson<DungeonResponse>(json);

        if (response == null)
        {
            messageText.text = "서버 응답을 읽을 수 없습니다.";
            Debug.LogError(json);
            return;
        }

        messageText.text = response.message;

        if (!response.success || response.data == null)
        {
            Debug.LogError($"{response.code} : {response.message}");
            return;
        }

        Draw(response.data);
    }

    private void Draw(DungeonStateData state)
    {
        DungeonPlayerData player = state.player;
        playerText.text = $"{player.name} Lv.{player.level}\n" +
                          $"HP {player.hp}/{player.maxHp}\n" +
                          $"Gold {player.gold}";

        // JsonUtility는 null인 객체도 빈 객체로 채우므로 id로 진행 중인 탐험 여부를 함께 확인
        if (state.run == null || string.IsNullOrEmpty(state.run.id))
        {
            roomText.text = "진행 중인 던전 없음";
            enterButton.interactable = true;
            SetActionButtons(false, false, false, false, false);
            return;
        }

        DungeonRoomData room = state.run.currentRoom;
        roomText.text = $"방 {room.index + 1}\n\n" +
                        $"{GetRoomDescription(room)}\n\n" +
                        $"방 상태 : {room.state}\n" +
                        $"탐험 골드 : {state.run.runGold}";

        UpdateButtons(room, state.run.state);
    }

    private void SetActionButtons(bool attack, bool chest, bool rest, bool next, bool returnToTown)
    {
        attackButton.interactable = attack;
        chestButton.interactable = chest;
        restButton.interactable = rest;
        nextButton.interactable = next;
        returnButton.interactable = returnToTown;
    }

    private void UpdateButtons(DungeonRoomData room, string runState)
    {
        bool playing = runState == "IN_PROGRESS";
        bool cleared = room.state == "CLEARED";

        enterButton.interactable = !playing;
        SetActionButtons(
            playing && room.type == "MONSTER" && !cleared,
            playing && room.type == "TREASURE" && !cleared,
            playing && room.type == "HEAL" && !cleared,
            playing && cleared,
            playing && room.type == "EXIT"
        );
    }

    private string GetRoomDescription(DungeonRoomData room)
    {
        switch (room.type)
        {
            case "MONSTER":
                return $"{room.monsterName} 출현!\n" +
                       $"몬스터 HP : {room.monsterHp}\n" +
                       $"몬스터 공격력 : {room.monsterAttack}\n" +
                       $"처치 보상 : {room.rewardGold} Gold";
            case "TREASURE":
                return $"보물 상자를 발견 했습니다.\n 보상 : {room.rewardGold} Gold";
            case "HEAL":
                return "회복의 샘을 발견했습니다.\n 휴식하면 HP를 30 회복합니다.";
            case "EXIT":
                return "던전 출구를 발견했습니다.\n 귀환하면 탐험 골드를 획득합니다.";
            default:
                return "알 수 없는 방입니다.";
        }
    }
}