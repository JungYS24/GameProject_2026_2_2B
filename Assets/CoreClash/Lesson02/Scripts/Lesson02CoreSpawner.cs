using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    [RequireComponent(typeof(NetworkObject))]
    public class Lesson02CoreSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject corePrefab;

        public override void Spawned()
        {
            // TODO 9: StateAuthority가 다섯 위치에 Core를 Spawn하도록 작성하세요.

            if (!Object.HasStateAuthority)
                return;

            Vector3[] positions =
            {
        new Vector3(-5f, 0.8f, -3f),
        new Vector3( 0f, 0.8f, -4f),
        new Vector3( 5f, 0.8f, -3f),
        new Vector3(-4f, 0.8f,  3f),
        new Vector3( 4f, 0.8f,  3f),
    };

            foreach (Vector3 position in positions)
            {
                Runner.Spawn(corePrefab, position, Quaternion.identity);
            }
        }
    }
}
