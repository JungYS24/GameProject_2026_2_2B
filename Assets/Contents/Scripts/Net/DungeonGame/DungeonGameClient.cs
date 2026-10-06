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
    }

    private IEnumerator Post(string path, string json)
    {
        using UnityWebRequest request = new UnityWebRequest(serverUrl + path, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
    }
}