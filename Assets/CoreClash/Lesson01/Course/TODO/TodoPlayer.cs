using Fusion;
using UnityEngine;

namespace CoreClash.Lesson01.Course
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public class TodoPlayer : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private TextMesh nicknameLabel;

        [Networked] public NetworkString<_16> PlayerName { get; set; }

        public void SetInitialName(string nickname)
        {
            if (HasStateAuthority)
                PlayerName = nickname;
        }

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            TodoCamera cameraController = Camera.main != null
                ? Camera.main.GetComponent<TodoCamera>()
                : null;
            cameraController?.SetTarget(transform);
        }

        public override void Render()
        {
            if (nicknameLabel != null)
                nicknameLabel.text = PlayerName.ToString();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out TodoInputData data))
                ApplyMovement(data.Move);
        }

        // 게임 이동과 회전은 완성 코드입니다.
        private void ApplyMovement(Vector2 input)
        {
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude < 0.001f) return;

            direction.Normalize();

            transform.position += direction * moveSpeed * Runner.DeltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Runner.DeltaTime);
        }
    }
}
