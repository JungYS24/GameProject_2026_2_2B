using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreClash.Lesson02
{
    public class Lesson02RoomUI : MonoBehaviour
    {
        [SerializeField] private InputField nicknameInput;
        [SerializeField] private InputField roomNameInput;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Text roomListText;
        [SerializeField] private Text statusText;
        [SerializeField] private Lesson02NetworkManager networkManager;

        private void Awake()
        {
            createButton.onClick.AddListener(Submit);
            joinButton.onClick.AddListener(Submit);
            refreshButton.onClick.AddListener(networkManager.JoinLobby);
        }

        private void Start() => networkManager.JoinLobby();
        private void Submit() => networkManager.StartRoom(roomNameInput.text, nicknameInput.text);
        public void SetStatus(string message) => statusText.text = message;

        public void ShowRooms(IReadOnlyList<string> rooms)
        {
            roomListText.text = rooms.Count == 0 ? "현재 공개된 방이 없습니다." : "공개 방\n• " + string.Join("\n• ", rooms);
        }

        public void SetConnecting(bool value)
        {
            nicknameInput.interactable = !value;
            roomNameInput.interactable = !value;
            createButton.interactable = !value;
            joinButton.interactable = !value;
            refreshButton.interactable = !value;
        }

        public void SetConnected(bool value)
        {
            gameObject.SetActive(!value);
            if (!value) SetConnecting(false);
            disconnectButton.interactable = value;
        }
    }
}
