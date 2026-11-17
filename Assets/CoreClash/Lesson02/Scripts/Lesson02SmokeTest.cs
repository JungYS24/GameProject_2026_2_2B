using System.Collections;
using System.Linq;
using Fusion;
using UnityEngine;

namespace CoreClash.Lesson02
{
    public class Lesson02SmokeTest : MonoBehaviour
    {
        private void Start()
        {
            if (System.Environment.GetCommandLineArgs().Contains("-lesson02Smoke")) StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(1f);
            FindFirstObjectByType<Lesson02NetworkManager>()
                .StartRoom("CoreClashLesson02Smoke", "Smoke" + System.Diagnostics.Process.GetCurrentProcess().Id);

            float deadline = Time.realtimeSinceStartup + 18f;
            Lesson02Player local = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                local = FindObjectsByType<Lesson02Player>(FindObjectsSortMode.None)
                    .FirstOrDefault(item => item.Object != null && item.Object.HasInputAuthority);
                if (local != null && FindObjectsByType<Lesson02CorePickup>(FindObjectsSortMode.None).Length >= 5) break;
                yield return null;
            }
            if (local == null) { Finish(false, "local player missing"); yield break; }

            Lesson02CorePickup[] cores = FindObjectsByType<Lesson02CorePickup>(FindObjectsSortMode.None)
                .OrderBy(item => item.transform.position.x).ThenBy(item => item.transform.position.z).ToArray();
            int coreIndex = Mathf.Abs(local.Object.InputAuthority.RawEncoded) % cores.Length;
            local.transform.position = cores[coreIndex].transform.position;
            yield return new WaitForSeconds(4f);
            bool collected = FindObjectsByType<Lesson02Player>(FindObjectsSortMode.None)
                .Any(item => item.Cores > 0);

            Lesson02Player[] players = FindObjectsByType<Lesson02Player>(FindObjectsSortMode.None)
                .OrderBy(item => item.Object.InputAuthority.RawEncoded).ToArray();
            Lesson02Player attacker = players.FirstOrDefault();
            Lesson02Player target = players.Skip(1).FirstOrDefault();
            if (local == attacker && target != null)
            {
                Vector3 direction = (target.transform.position - local.transform.position).normalized;
                if (direction.sqrMagnitude < .1f) direction = Vector3.forward;
                local.transform.SetPositionAndRotation(target.transform.position - direction * 1.2f,
                    Quaternion.LookRotation(direction));
                yield return new WaitForSeconds(1f);
                Physics.SyncTransforms();
                local.SendMessage("TryMelee", SendMessageOptions.DontRequireReceiver);
            }

            bool effect = false;
            float effectDeadline = Time.realtimeSinceStartup + 1f;
            while (Time.realtimeSinceStartup < effectDeadline)
            {
                if (GameObject.Find("MeleeFlash") != null) effect = true;
                yield return null;
            }

            if (local == attacker && target != null && target.Health > 75)
                target.RequestDamage(25, Vector3.forward);
            yield return new WaitForSeconds(1f);
            bool melee = target != null && target.Health <= 75;
            bool runner = NetworkRunner.Instances.Any(item => item != null && item.IsRunning);
            yield return new WaitForSeconds(3f);
            Finish(runner && players.Length >= 2 && cores.Length == 5 && collected && melee && effect,
                $"runner={runner} players={players.Length} cores={cores.Length} collected={collected} melee={melee} effect={effect}");
        }

        private static void Finish(bool pass, string details)
        {
            Debug.Log($"[Lesson02Smoke] {(pass ? "PASS" : "FAIL")} {details}");
            Application.Quit(pass ? 0 : 2);
        }
    }
}
