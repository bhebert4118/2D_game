using UnityEngine;

public class RocketPart : MonoBehaviour
{
    public bool canBePickedUp = true;

    void OnTriggerEnter(Collider other)
    {
    if (!canBePickedUp)
    {
    return;
    }

    if (!other.CompareTag("Player"))
    {
    return;
    }

    PlayerInventory inventory = other.GetComponent<PlayerInventory>();

    if (inventory == null)
    {
    return;
    }

    bool pickedUp = inventory.PickUpItem(gameObject);

    if (pickedUp)
    {
    gameObject.SetActive(false);
    }
    }
}