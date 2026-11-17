using System.Linq;
using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    public class Lesson02Bootstrap : MonoBehaviour
    {
        [SerializeField] private NetworkObject spawnerPrefab;
        private NetworkRunner readyRunner;
        private float spawnAfter;
        private bool spawned;

        private void Update()
        {
            // TODO 8: 실행 중인 Shared Master Client를 기다린 뒤 Spawner를 Spawn하세요.

            if (spawned)
                return;

            NetworkRunner runner = NetworkRunner.Instances.FirstOrDefault(item => item != null && item.IsRunning);

            if (runner == null || !runner.IsSharedModeMasterClient || !runner.CanSpawn || !runner.LocalPlayer.IsValid)
            {
                return;
            }

            if (readyRunner != runner)
            {
                readyRunner = runner;
                spawnAfter = Time.unscaledTime + 1.5f;
                return;
            }

            if (Time.unscaledTime < spawnAfter)
                return;

            runner.Spawn(spawnerPrefab, Vector3.zero, Quaternion.identity);
            spawned = true;
        }
    }
}
