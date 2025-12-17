using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Added for Image

public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Player_Inventory playerInventory; // Assign in Inspector or find at runtime
    public int slotIndex; // Set this to the slot's index in the inventory
    public bool isHovering = false;
    public bool isToolbeltSlot = false;
    public bool isHatSlot = false;
    public bool isTopSlot = false;
    public bool isBottomsSlot = false;
    public Player_Outfitter playerOutfitter; // Reference to the Player_Outfitter script
    public Shovel shovelScript; // Reference to the Shovel script
    public CaughtObj caughtObjScript; // Reference to the CaughtObj script

    void Start()
    {
        if (playerInventory == null)
        {
            GameObject gm = GameObject.Find("Game Manager");
            if (gm != null)
            {
                playerInventory = gm.GetComponent<Player_Inventory>();
                shovelScript = gm.GetComponent<Shovel>();
                caughtObjScript = gm.GetComponent<CaughtObj>();
                if (playerInventory == null)
                {
                    Debug.LogWarning("Player_Inventory component not found on Game Manager.");
                }
                if(shovelScript == null)
                {
                    Debug.LogWarning("Shovel component not found on Game Manager.");
                }
            }
            else
            {
                Debug.LogWarning("Game Manager object not found in scene.");
            }
        }

        if (playerOutfitter == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerOutfitter = player.GetComponent<Player_Outfitter>();
                if (playerOutfitter == null)
                {
                    Debug.LogWarning("Player_Outfitter component not found on Player.");
                }
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found in scene.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(playerInventory.isBuryingItem)
        {
            int activeToolSlotIndex = playerInventory.inventorySlots.Length + playerInventory.activeToolIndex;
            if (isToolbeltSlot && playerInventory.activeToolIndex < 4 && slotIndex == activeToolSlotIndex)
            {
                // Make sure tooltip is hidden if hovering over active tool slot
                // Also make sure you cannot bury the active tool
                playerInventory.HideToolTip();
                return; // Exit early to avoid showing bury tooltip
            }
        }

        isHovering = true;
        if(playerInventory.isBuryingItem)
        {
            // Show bury tool tip
            if (playerInventory != null)
            {
                // Check if the toolbelt slot is the active tool- and if it matches the current slot we are hovering over
                // Calculate the active tool slot index: inventorySlots.Length + activeToolIndex
                int activeToolSlotIndex = playerInventory.inventorySlots.Length + playerInventory.activeToolIndex;
                if (isToolbeltSlot && playerInventory.activeToolIndex < 4 && slotIndex == activeToolSlotIndex)
                {
                    // Make sure tooltip is hidden if hovering over active tool slot
                    // Also make sure you cannot bury the active tool
                    playerInventory.HideToolTip();
                    // Do not add burying logic here
                }
                else
                {
                    playerInventory.ShowBuryToolTip(slotIndex);
                    playerInventory.etoDropLabel.text = "Press B to bury item";
                    playerInventory.pToPlaceLabel.text = "";
                }
            }
        }
        else
        {
            // Show normal tool tip
            if (playerInventory != null)
            {
                playerInventory.ShowInventoryToolTip(slotIndex);
                playerInventory.etoDropLabel.text = "Press E to drop item";
                playerInventory.pToPlaceLabel.text = "Press P to place item";
            }
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
        // If contains an item, pic
        // Also make sure we are not burying an item!k         
        if (playerInventory != null && eventData.button == PointerEventData.InputButton.Left && !playerInventory.isBuryingItem)
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
                else if (isHatSlot)
                {
                    if (playerInventory.tempSlot.item.itemType == "Hat")
                    {
                        Debug.Log("Dropping hat into hat slot " + slotIndex);
                        playerInventory.Inventory_DropItem(slotIndex);
                    }
                    else
                    {
                        Debug.Log("Cannot place non-hat item into hat slot " + slotIndex);
                        // Optionally, show a UI warning here
                    }
                }
                else if (isTopSlot)
                {
                    if (playerInventory.tempSlot.item.itemType == "Top")
                    {
                        Debug.Log("Dropping top into top slot " + slotIndex);
                        playerInventory.Inventory_DropItem(slotIndex);
                    }
                    else
                    {
                        Debug.Log("Cannot place non-top item into top slot " + slotIndex);
                        // Optionally, show a UI warning here
                    }
                }
                else if (isBottomsSlot)
                {
                    if (playerInventory.tempSlot.item.itemType == "Bottoms")
                    {
                        Debug.Log("Dropping bottoms into bottoms slot " + slotIndex);
                        playerInventory.Inventory_DropItem(slotIndex);
                    }
                    else
                    {
                        Debug.Log("Cannot place non-bottoms item into bottoms slot " + slotIndex);
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

            if (playerInventory.isBuryingItem)
            {
                int activeToolSlotIndex = playerInventory.inventorySlots.Length + playerInventory.activeToolIndex;
                if (playerInventory.activeToolIndex < 4 && slotIndex != activeToolSlotIndex)
                {
                    // If not hovering over active toolbelt slot, allow burying
                    if (Input.GetKeyDown(KeyCode.B))
                    {
                        // Get item data from this slotIndex
                        GameObject item = playerInventory.slots[slotIndex].itemObject;

                        item.SetActive(true);

                        shovelScript.SelectObjectToBury(item);

                        shovelScript.BuryHole();
                        
                        // Remove item from inventory
                        //THIS DOES NOT WORK
                        playerInventory.RemoveItemForBurial(slotIndex);
                        
                        playerInventory.HideInventory(); // Hide inventory UI
                        shovelScript.buryItemUI.SetActive(false); // Hide bury item UI
                        
                        // Unfocus camera
                        shovelScript.StartCoroutine(caughtObjScript.MoveCameraToOldPosition());
                        // Allow tool swapping again
                        playerInventory.canSwapTool = true;
                        playerInventory.isBuryingItem = false;
                    }
                } 
            }
        }

        CheckHatSlot();
        CheckTopSlot();
        CheckBottomsSlot();
    }

    void CheckHatSlot()
    {
        if (isHatSlot)
        {
            // Check the inventory script to see if the item in this slot contains a hat
            if (playerInventory.slots[slotIndex].item != null && playerInventory.slots[slotIndex].item.itemType == "Hat")
            {
                // Set the hat's position and rotation on the player
                playerOutfitter.WearHat(playerInventory.slots[slotIndex].item);
            }
            else
            {
                // This is a hat slot but does not contain a hat
                playerOutfitter.RemoveHat();
            }
        }
    }

    void CheckTopSlot()
    {
        if (isTopSlot)
        {
            // Check the inventory script to see if the item in this slot contains a top
            if (playerInventory.slots[slotIndex].item != null && playerInventory.slots[slotIndex].item.itemType == "Top")
            {
                // Set the top's color and texture on the player
                playerOutfitter.ChangeOutfitTop(playerInventory.slots[slotIndex].item.itemTexture);
                playerOutfitter.RecolorOutfitTop(playerInventory.slots[slotIndex].item.itemColor);
            }
            else
            {
                // This is a top slot but does not contain a top, reset to default
                playerOutfitter.RemoveOutfitTop();
            }
        }
    }

    void CheckBottomsSlot()
    {
        if (isBottomsSlot)
        {
            // Check the inventory script to see if the item in this slot contains bottoms
            if (playerInventory.slots[slotIndex].item != null && playerInventory.slots[slotIndex].item.itemType == "Bottoms")
            {
                // Set the bottoms' color and texture on the player
                playerOutfitter.ChangeOutfitBottom(playerInventory.slots[slotIndex].item.itemTexture);
                playerOutfitter.RecolorOutfitBottom(playerInventory.slots[slotIndex].item.itemColor);
            }
            else
            {
                // This is a bottoms slot but does not contain bottoms, reset to default
                playerOutfitter.RemoveOutfitBottom();
            }
        }
    }
}
