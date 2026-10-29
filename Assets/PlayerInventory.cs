using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public TMP_Text carryingText;

    public GameObject heldItem = null;
    public bool carryingItem = false;

    void Start()
    {
    UpdateCarryingText();
    }

    public bool PickUpItem(GameObject item)
    {
    // Can't pick up another item while already carrying one
    if (carryingItem)
    {
    return false;
    }

    heldItem = item;
    carryingItem = true;

    UpdateCarryingText();

    return true;
    }

    public GameObject PutDownItem()
    {
    if (!carryingItem)
    {
    return null;
    }

    GameObject item = heldItem;

    heldItem = null;
    carryingItem = false;

    UpdateCarryingText();

    return item;
    }

    void UpdateCarryingText()
    {
    if (carryingItem && heldItem != null)
    {
    carryingText.text = "Carrying: " + heldItem.name;
    }
    else
    {
    carryingText.text = "Carrying: -";
    }
    }
}