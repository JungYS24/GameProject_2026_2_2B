using System.Collections;
using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public class Lesson02Player : NetworkBehaviour
    {
        [SerializeField] private float meleeRange = 2.2f;
        [SerializeField] private int meleeDamage = 25;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float turnSpeed = 14f;

        // TODO 17: 체력, 점수, 공격 쿨다운, 이전 버튼을 Networked 속성으로 만드세요.
        [Networked] public int Health { get; private set; }
        [Networked] public int Cores { get; private set; }
        [Networked] private TickTimer MeleeCooldown { get; set; }
        [Networked] private NetworkButtons PreviousButtons { get; set; }

        public override void Spawned()
        {
            // TODO 18: StateAuthority에서 체력을 초기화하고 PlayerObject를 등록하세요.
            if (!Object.HasStateAuthority) return;

            Health = 100;
            Runner.SetPlayerObject(Object.InputAuthority, Object);
        }

        public override void FixedUpdateNetwork()
        {
            // TODO 19: 입력을 읽어 이동하고 공격 버튼의 시작 순간을 처리하세요.
            if (!Object.HasStateAuthority || !GetInput(out Lesson02InputData input))
                return;

            ApplyMovement(input.Move);

            if (input.Buttons.WasPressed(PreviousButtons, (int)Lesson02Button.Attack))
            {
                TryMelee();
            }

            PreviousButtons = input.Buttons;
        }

        private void ApplyMovement(Vector2 input)
        {
            // TODO 20: 입력 방향으로 이동하고 바라보는 방향을 회전하세요.
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            direction.Normalize();
            transform.position += direction * moveSpeed * Runner.DeltaTime;

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turnSpeed * Runner.DeltaTime);
        }

        public void RequestAddCore(int amount) => RPC_AddCore(amount);
        public void RequestDamage(int amount, Vector3 direction) => RPC_Damage(amount, direction);

        // TODO 21: 점수 RPC의 Source와 Target을 지정하세요.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_AddCore(int amount)
        {
            // TODO 22: 점수를 안전하게 증가시키세요.
            Cores += Mathf.Max(0, amount);
        }

        // TODO 23: 피해 RPC의 Source와 Target을 지정하세요.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_Damage(int amount, Vector3 direction)
        {
            // TODO 24: 체력을 감소시키고 공격 방향으로 넉백시키세요.
            Health = Mathf.Max(0, Health - amount);
            transform.position += direction.normalized * 0.45f;
        }

        private void TryMelee()
        {
            // TODO 25: 쿨다운, 이펙트 RPC, OverlapSphere, 피해 요청을 작성하세요.
            if (!MeleeCooldown.ExpiredOrNotRunning(Runner)) return;

            MeleeCooldown = TickTimer.CreateFromSeconds(Runner, 0.6f);

            RPC_PlayMeleeEffect();

            Vector3 center = transform.position + transform.forward * 1.2f;

            foreach (Collider hit in Physics.OverlapSphere(center, meleeRange))
            {
                Lesson02Player target = hit.GetComponentInParent<Lesson02Player>();

                if (target == null || target == this)
                    continue;

                target.RequestDamage(meleeDamage, transform.forward);
                break;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayMeleeEffect()
        {
            StartCoroutine(PlayMeleeEffect());
        }

        private IEnumerator PlayMeleeEffect()
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "MeleeFlash";
            Destroy(flash.GetComponent<Collider>());

            Renderer flashRenderer = flash.GetComponent<Renderer>();
            Renderer sourceRenderer = GetComponentInChildren<Renderer>();
            Material material = sourceRenderer != null && sourceRenderer.sharedMaterial != null
                ? new Material(sourceRenderer.sharedMaterial)
                : flashRenderer.material;
            material.color = new Color(0.1f, 1f, 0.9f, 1f);
            flashRenderer.material = material;

            Vector3 start = transform.position + Vector3.up * 0.8f + transform.forward * 0.7f;
            Vector3 end = transform.position + Vector3.up * 0.8f + transform.forward * 2.0f;
            float duration = 0.3f;

            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float t = time / duration;
                flash.transform.position = Vector3.Lerp(start, end, t);
                flash.transform.rotation = transform.rotation;
                flash.transform.localScale = Vector3.Lerp(
                    new Vector3(0.25f, 0.25f, 0.25f),
                    new Vector3(2.4f, 0.18f, 0.7f), t);
                yield return null;
            }

            Destroy(flash);
            if (material != null) Destroy(material);
        }
    }
}