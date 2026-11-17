using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    [RequireComponent(typeof(NetworkObject), typeof(Collider))]
    public class Lesson02CorePickup : NetworkBehaviour
    {
        [SerializeField] private float respawnSeconds = 5f;

        // TODO 10: 활성 상태와 리스폰 타이머를 Networked 속성으로 만드세요.
        [Networked]
        public NetworkBool IsAvailable { get; private set; }
        [Networked]
        private TickTimer RespawnTimer { get; set; }

        public override void Spawned()
        {
            // TODO 11: StateAuthority에서 초기화하고 표시 상태를 적용하세요.
            if (Object.HasStateAuthority)
                IsAvailable = true;

            ApplyVisibleState();
        }

        public override void FixedUpdateNetwork()
        {
            // TODO 12: 리스폰 타이머가 끝나면 코어를 다시 활성화하세요.
            if (Object.HasStateAuthority && !IsAvailable && RespawnTimer.Expired(Runner))
            {
                IsAvailable = true;
            }
        }

        public override void Render()
        {
            transform.Rotate(0f, 100f * Time.deltaTime, 0f);
            ApplyVisibleState();
        }

        private void OnTriggerEnter(Collider other)
        {
            // TODO 13: Local Player가 충돌했을 때 수집 RPC를 요청하세요.
            Lesson02Player player = other.GetComponentInParent<Lesson02Player>();

            if (player != null && player.Object.HasInputAuthority)
            {
                RPC_Collect(player.Object.InputAuthority);
            }
        }

        // TODO 14: 수집 RPC의 Source와 Target을 지정하세요.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_Collect(PlayerRef playerRef)
        {
            // TODO 15: 중복 수집 방지, 점수 증가, 비활성화, 리스폰 타이머를 작성하세요.
            if (!IsAvailable || !Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
            {
                return;
            }

            Lesson02Player player = playerObject.GetComponent<Lesson02Player>();

            if (player == null)
                return;

            player.RequestAddCore(1);
            IsAvailable = false;
            RespawnTimer = TickTimer.CreateFromSeconds(Runner, respawnSeconds);
        }

        private void ApplyVisibleState()
        {
            // TODO 16: Renderer와 Collider를 IsAvailable에 맞게 설정하세요.
            foreach (Renderer item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = IsAvailable;
            }
            GetComponent<Collider>().enabled = IsAvailable;
        }
    }
}