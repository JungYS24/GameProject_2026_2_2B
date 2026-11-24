using UnityEngine;

namespace CoreClash.Lesson02
{
    public class Lesson02HUD : MonoBehaviour
    {
        private GUIStyle style;

        private void OnGUI()
        {
            Lesson02Player local = null;
            foreach (Lesson02Player player in FindObjectsByType<Lesson02Player>(FindObjectsSortMode.None))
                if (player.Object != null && player.Object.HasInputAuthority) local = player;
            if (local == null) return;

            style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Box(new Rect(20, 20, 260, 195), string.Empty);
            GUI.Label(new Rect(36, 30, 220, 30), $"HP  {local.Health}", style);
            GUI.Label(new Rect(36, 65, 220, 30), $"CORE  {local.Cores}", style);
            GUI.Label(new Rect(36, 100, 260, 30), $"AMMO  {local.Ammo} / 6 ", style);
            GUI.Label(new Rect(36, 135, 220, 30), $"KILLS  {local.kills} ", style);

            Lesson02MatchState match = Lesson02MatchState.Instance;

            if (match != null)
            {
                GUI.Label(new Rect(36, 170, 260, 30), $"TIME  {Mathf.CeilToInt(match.RemainingSeconds)}", style);
                if (match.IsFinished)
                {
                    GUI.Label(new Rect(380, 30, 500, 50), $"WINNER  {match.winnerName}", style);
                }
            }
        }
    }
}