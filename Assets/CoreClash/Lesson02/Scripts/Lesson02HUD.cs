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
                fontSize = 22, fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            GUI.Box(new Rect(20, 20, 260, 90), string.Empty);
            GUI.Label(new Rect(36, 30, 220, 30), $"HP  {local.Health}", style);
            GUI.Label(new Rect(36, 65, 220, 30), $"CORE  {local.Cores}", style);
        }
    }
}
