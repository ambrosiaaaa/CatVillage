using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Added for Image

public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Player_Inventory playerInventory; // Assign in Inspector or find at runtime
    public int slotIndex; // Set this to the slot's index in the inventory
    public bool isHovering = false;
    public bool isToolbeltSlot = false;

    void Start()
    {
        if (playerInventory == null)
        {
            GameObject gm = GameObject.Find("Game Manager");
            if (gm != null)
            {
                playerInventory = gm.GetComponent<Player_Inventory>();
                if (playerInventory == null)
                {
                    Debug.LogWarning("Player_Inventory component not found on Game Manager.");
                }
            }
            else
            {
                Debug.LogWarning("Game Manager object not found in scene.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (playerInventory != null)
        {
            playerInventory.ShowInventoryToolTip(slotIndex);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (playerInventory != null)
        {
            playerInventory.HideToolTip();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // If contains an item, pick up item or drop item
        if (playerInventory != null && eventData.button == PointerEventData.InputButton.Left)
        {
            if (playerInventory.tempSlot.item != null && playerInventory.slots[slotIndex].item != null)
            {
                Debug.Log("swapping item into slot " + slotIndex);
                // If temp slot is full, and player slot is full, swap items
                playerInventory.Inventory_SwapItems(slotIndex);
            }
            else if (playerInventory.tempSlot.item == null && playerInventory.slots[slotIndex].item != null)
            {
                Debug.Log("Picking up item from slot " + slotIndex);
                // If temp slot is empty, pick up item
                playerInventory.Inventory_PickupItem(slotIndex);
            }
            else if (playerInventory.tempSlot.item != null && playerInventory.slots[slotIndex].item == null)
            {
                // If this is a toolbelt slot, only allow dropping if item is a Tool
                if (isToolbeltSlot)
                {
                    if (playerInventory.tempSlot.item.itemType == "Tool")
                    {
                        Debug.Log("Dropping tool into toolbelt slot " + slotIndex);
                        playerInventory.Inventory_DropItem(slotIndex);
                    }
                    else
                    {
                        Debug.Log("Cannot place non-tool item into toolbelt slot " + slotIndex);
                        // Optionally, show a UI warning here
                    }
                }
                else
                {
                    Debug.Log("Dropping item into slot " + slotIndex);
                    playerInventory.Inventory_DropItem(slotIndex);
                }
            }
        }
    }

    void Update()
    {
        if (isHovering && playerInventory != null)
        {
            // Make sure temp items is empty
            if (playerInventory.tempSlot.item == null)
            {
                playerInventory.ShowInventoryToolTip(slotIndex);
            }

            // If press E, remove item from inventory.
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerInventory.RemoveItemFromInventory(slotIndex);
            }
        }
    }
}
