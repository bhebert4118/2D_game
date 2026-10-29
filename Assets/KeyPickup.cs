using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private GameObject wallToRemove;

    private void OnTriggerEnter(Collider other)
    {
    if (!other.CompareTag("Player"))
    {
    return;
    }

    if (wallToRemove != null)
    {
    wallToRemove.SetActive(false);
    }

    Destroy(gameObject);
    }
}