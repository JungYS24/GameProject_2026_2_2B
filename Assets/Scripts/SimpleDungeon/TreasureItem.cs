using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [SerializeField] private int goldAmount = 100;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"보물 획득! 골드 +{goldAmount}");
        Destroy(gameObject);
    }
}
