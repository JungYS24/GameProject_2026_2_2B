using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Collider))]
    public class Lesson02Projectile : NetworkBehaviour
    {
        [SerializeField] private float speed = 13f;
        [Networked] private PlayerRef Owner { get; set; }
        [Networked] private int Damage { get; set; }
        [Networked] private TickTimer Life { get; set; }

        public void Initialize(PlayerRef owner, int damage)
        {
            if (!Object.HasStateAuthority) return;

            Owner = owner;
            Damage = damage;
            Life = TickTimer.CreateFromSeconds(Runner, 3f);
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;

            transform.position += transform.forward * speed * Runner.DeltaTime;

            if (Life.Expired(Runner)) Runner.Despawn(Object);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Object.HasStateAuthority) return;

            Lesson02Player target = other.GetComponentInParent<Lesson02Player>();

            if (target == null) return;

            if (target.Object.InputAuthority == Owner) return;

            target.RequestDamage(Damage, transform.forward, Owner);

            Runner.Despawn(Object);
        }
    }
}