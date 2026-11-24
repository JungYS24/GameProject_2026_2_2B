using UnityEngine;
using System.Linq;
using Fusion;

namespace CoreClash.Lesson02
{
    [RequireComponent(typeof(NetworkObject))]
    public class Lesson02MatchState : NetworkBehaviour
    {
        public static Lesson02MatchState Instance { get; private set; }
        [Networked] public TickTimer MatchTimer { get; private set; }
        [Networked] public NetworkBool IsFinished { get; private set; }
        [Networked] public NetworkString<_16> winnerName { get; private set; }

        public float RemainingSeconds => MatchTimer.RemainingTime(Runner) ?? 0f;
        public bool CanPlay => !IsFinished;

        public override void Spawned()
        {
            Instance = this;

            if (Object.HasStateAuthority)
            {
                MatchTimer = TickTimer.CreateFromSeconds(Runner, 90f);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (IsFinished) return;
            if (!MatchTimer.Expired(Runner)) return;

            Lesson02Player[] players = FindObjectsByType<Lesson02Player>(FindObjectsSortMode.None);

            int best = players.Length == 0 ? 0 : players.Max(item => item.kills);

            Lesson02Player[] winners = players.Where(item => item.kills == best).ToArray();

            winnerName = winners.Length == 1 ? winners[0].PlayerDisplayName : "DRAW";

            IsFinished = true;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}