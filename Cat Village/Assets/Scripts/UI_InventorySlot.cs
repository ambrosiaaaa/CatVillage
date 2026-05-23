using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Added for Image

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Player_Inventory playerInventory;
    public int slotIndex;
    public bool isHovering = false;
    public bool isToolbeltSlot = false;
    public bool isHatSlot = false;
    public bool isTopSlot = false;
    public bool isBottomsSlot = false;
    public Player_Outfitter playerOutfitter;
    public Shovel shovelScript;
    public Hoe hoeScript;
    public CaughtObj caughtObjScript;

    void Start()
    {
        // Initialization logic (kept as is)
        if (playerInventory == null)
        {
            GameObject gm = GameObject.Find("Game Manager");
            if (gm != null)
            {
                playerInventory = gm.GetComponent<Player_Inventory>();
                shovelScript = gm.GetComponent<Shovel>();
                hoeScript = gm.GetComponent<Hoe>();
                caughtObjScript = gm.GetComponent<CaughtObj>();
            }
        }

        if (playerOutfitter == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerOutfitter = player.GetComponent<Player_Outfitter>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        RefreshTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (playerInventory != null)
        {
            // Only clear it if we are the slot currently marked as hovered
            playerInventory.HideToolTip();
        }
    }

    // New helper method to handle the complex tooltip logic in one place
    public void RefreshTooltip()
    {
        if (playerInventory == null) return;

        // If holding an item with the mouse cursor, don't show a tooltip for the slot underneath
        if (playerInventory.tempSlot.item != null)
        {
            playerInventory.HideToolTip();
            return;
        }

        // Handle Burying Item State
        if (playerInventory.isBuryingItem)
        {
            int activeToolSlotIndex = playerInventory.inventorySlots.Length + playerInventory.activeToolIndex;
            if (isToolbeltSlot && slotIndex == activeToolSlotIndex)
            {
                playerInventory.HideToolTip();
            }
            else
            {
                playerInventory.ShowBuryToolTip(slotIndex);
                playerInventory.etoDropLabel.text = "Press B to bury item";
                playerInventory.pToPlaceLabel.text = "";
            }
            return;
        }

        // Handle Burying Seed State
        if (playerInventory.isBuryingSeed)
        {
            if (playerInventory.isSeedItem(playerInventory.slots[slotIndex].item))
            {
                playerInventory.ShowBuryToolTip(slotIndex);
                playerInventory.etoDropLabel.text = "Press B to plant seed";
                playerInventory.pToPlaceLabel.text = "";
            }
            else
            {
                playerInventory.HideToolTip();
            }
            return;
        }

        // Default Normal State
        if (playerInventory.slots[slotIndex].item != null)
        {
            Debug.Log("Hovering over" + slotIndex);
            playerInventory.ShowInventoryToolTip(slotIndex);
            playerInventory.etoDropLabel.text = "Press E to drop item";
            playerInventory.pToPlaceLabel.text = "Press P to place item";
        }
        else
        {
            playerInventory.HideToolTip();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerInventory == null || eventData.button != PointerEventData.InputButton.Left) return;
        if (playerInventory.isBuryingItem || playerInventory.isBuryingSeed) return;

        if (playerInventory.tempSlot.item != null && playerInventory.slots[slotIndex].item != null)
        {
            playerInventory.Inventory_SwapItems(slotIndex);
        }
        else if (playerInventory.tempSlot.item == null && playerInventory.slots[slotIndex].item != null)
        {
            playerInventory.Inventory_PickupItem(slotIndex);
        }
        else if (playerInventory.tempSlot.item != null && playerInventory.slots[slotIndex].item == null)
        {
            // Type restrictions for special slots
            bool canDrop = true;
            if (isToolbeltSlot && playerInventory.tempSlot.item.itemType != "Tool") canDrop = false;
            else if (isHatSlot && playerInventory.tempSlot.item.itemType != "Hat") canDrop = false;
            else if (isTopSlot && playerInventory.tempSlot.item.itemType != "Top") canDrop = false;
            else if (isBottomsSlot && playerInventory.tempSlot.item.itemType != "Bottoms") canDrop = false;

            if (canDrop) playerInventory.Inventory_DropItem(slotIndex);
        }

        // Refresh tooltip after moving items around
        RefreshTooltip();
    }

    void Update()
    {
        if (isHovering && playerInventory != null)
        {
            // Drop item logic
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerInventory.RemoveItemFromInventory(slotIndex);
                RefreshTooltip();
            }

            // Bury Logic
            if (playerInventory.isBuryingItem)
            {
                int activeToolSlotIndex = playerInventory.inventorySlots.Length + playerInventory.activeToolIndex;
                if (slotIndex != activeToolSlotIndex && Input.GetKeyDown(KeyCode.B))
                {
                    ExecuteBuryItem();
                }
            }
            else if (playerInventory.isBuryingSeed)
            {
                if (playerInventory.slots[slotIndex].item != null &&
                    playerInventory.slots[slotIndex].item.plant != null &&
                    playerInventory.slots[slotIndex].item.plant.currentGrowthStage == 0)
                {
                    if (Input.GetKeyDown(KeyCode.B)) ExecuteBurySeed();
                }
            }
        }

        CheckHatSlot();
        CheckTopSlot();
        CheckBottomsSlot();
    }

    private void ExecuteBuryItem()
    {
        GameObject item = playerInventory.slots[slotIndex].itemObject;
        item.SetActive(true);
        shovelScript.SelectObjectToBury(item);
        shovelScript.BuryHole();
        FinishBuryAction(shovelScript.buryItemUI);
        playerInventory.isBuryingItem = false;
    }

    private void ExecuteBurySeed()
    {
        GameObject item = playerInventory.slots[slotIndex].itemObject;
        item.SetActive(true);
        hoeScript.SelectSeedToBury(item);
        hoeScript.BurySeed();
        FinishBuryAction(hoeScript.burySeedPopupUI);
        playerInventory.isBuryingSeed = false;
    }

    private void FinishBuryAction(GameObject uiToHide)
    {
        playerInventory.RemoveItemForBurial(slotIndex);
        playerInventory.HideInventory();
        uiToHide.SetActive(false);
        StartCoroutine(caughtObjScript.MoveCameraToOldPosition());
        playerInventory.canSwapTool = true;
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
