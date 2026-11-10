using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreClash.Lesson01.Course
{
    // UI는 완성 코드입니다. 수업에서는 수정하지 않습니다.
    public class TodoRoomUI : MonoBehaviour
    {
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private InputField roomNameInput;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Text roomListText;
        [SerializeField] private Text statusText;
        [SerializeField] private TodoNetworkManager networkManager;

        private void Awake()
        {
            createButton.onClick.AddListener(() => Submit(true));
            joinButton.onClick.AddListener(() => Submit(false));
            refreshButton.onClick.AddListener(networkManager.JoinLobby);
            disconnectButton.onClick.AddListener(networkManager.Disconnect);
            disconnectButton.interactable = false;
        }

        private void Start() => networkManager.JoinLobby();

        private void Submit(bool createRoom)
        {
            networkManager.StartRoom(roomNameInput.text, nicknameInput.text, createRoom);
        }

        public void ShowRooms(IReadOnlyList<string> rooms)
        {
            roomListText.text = rooms.Count == 0
                ? "현재 공개된 방이 없습니다."
                : "공개 방\n• " + string.Join("\n• ", rooms);
        }

        public void SetStatus(string message) => statusText.text = message;

        public void SetConnecting(bool connecting)
        {
            nicknameInput.interactable = !connecting;
            roomNameInput.interactable = !connecting;
            createButton.interactable = !connecting;
            joinButton.interactable = !connecting;
            refreshButton.interactable = !connecting;
        }

        public void SetConnected(bool connected)
        {
            gameObject.SetActive(!connected);

            if (!connected)
                SetConnecting(false);

            disconnectButton.interactable = connected;
        }
    }
}
