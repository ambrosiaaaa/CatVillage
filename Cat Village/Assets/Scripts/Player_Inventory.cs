using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Player_Inventory : MonoBehaviour
{
    [Header("Inventory GUI")]
    public GameObject inventoryUI; // Reference to the inventory UI GameObject  
    public GameObject pickupPopupUI; // Reference to the pickup UI GameObject
    public GameObject collisionPopupUI; // Reference to the collision popup UI GameObject
    public GameObject placeObjectPopupUI; // Reference to the place object popup UI GameObject
    public GameObject inventoryTooFullPopupUI; // Reference to the inventory too full popup UI GameObject

    [Header("Inventory Slots")]
    public GameObject[] inventorySlots; // Array to hold inventory slot GameObjects

    [Header("Toolbelt Slots")]
    public GameObject[] toolbeltSlots; // Array to hold toolbelt slot GameObjects
    public GameObject activeToolUI;
    public int activeToolIndex = 4; // If 4, no active tool, maximum of 4.

    [Header("Outfit Slots")]
    public GameObject headSlot;
    public GameObject torsoSlot;
    public GameObject bottomSlot;

    [Header("Current Item")]
    public Item currentItem; // Reference to the currently selected item
    public bool isHoldingItem = false;

    [Header("Player Reference")]
    public GameObject player; // Drag and drop the player character here
    public GameObject playerHand; // Where to hold the active tool
    public GameObject activeToolObject; // The currently displayed tool in hand
    public GameObject playerAttackRadius; // The player's attack radius

    [Header("Miscellaneous Settings")]
    public float pickupRange = 0.5f; // Range within which the player can pick up items
    public GameObject inventorySlotIcon; // Image that is to be placed within each inventory slot
    public GameObject inventoryToolTip; // Tooltip for items in inventory
    public GameObject foodToolTip; // Tooltip for food items
    public TMPro.TMP_Text livesText; // How many lives a food restores
    public TMPro.TMP_Text itemNameText;
    public TMPro.TMP_Text itemDescriptionText;
    public TMPro.TMP_Text itemTypeText;
    public TMPro.TMP_Text food_itemNameText;
    public TMPro.TMP_Text food_itemDescriptionText;
    public TMPro.TMP_Text food_itemTypeText;
    public Vector3 toolTipOffset = new Vector3(115f, 60f, 0f);

    [Header("Tool Scripts")]
    public FishingRod fr;

    public class InventorySlot
    {
        public GameObject slot;
        public GameObject itemObject;
        public Item item = null;
        public Image icon = null;
    }

    public InventorySlot[] slots;

    private float collisionPopupTimer = 0f;
    private bool collisionPopupActive = false;
    private CanvasGroup collisionPopupCanvasGroup;
    private float inventoryTooFullPopupTimer = 0f;
    private bool inventoryTooFullPopupActive = false;
    private CanvasGroup inventoryTooFullPopupCanvasGroup;

    public GameObject previewObject = null;
    private Material previewMaterial = null;
    public Color previewColor = new Color(1f, 1f, 1f, 0.5f);
    public Vector3 previewOffset = new Vector3(0f, 0.5f, 0f);

    private bool isPlacementMode = false;
    private int placementSlotIndex = -1;
    private float placementRotationY = 0f;
    private float placementTargetRotationY = 0f;
    public float placementRotationSpeed = 360f; // degrees per second

    private Vector3 lastValidPreviewPosition = Vector3.zero;
    [Header("Placement Collision Mask")]
    public LayerMask placementCollisionMask;

    public GameObject ItemCollisionPopup2; // Reference to the new collision popup UI
    private float itemCollisionPopup2Timer = 0f;
    private bool itemCollisionPopup2Active = false;

    //Temporary slot to store item when picking up/dragging
    public InventorySlot tempSlot = new InventorySlot();
    GameObject newIconObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide inventory UI on load
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        // Hide tooltip on load
        if (inventoryToolTip != null)
        {
            inventoryToolTip.SetActive(false);
        }
        // Hide pickup popup UI on load
        if (pickupPopupUI != null)
        {
            pickupPopupUI.SetActive(false);
        }
        if (foodToolTip != null)
        {
            foodToolTip.SetActive(false);
        }
        if (activeToolUI != null)
        {
            activeToolUI.SetActive(false);
        }
        if (inventoryTooFullPopupUI != null)
        {
            inventoryTooFullPopupUI.SetActive(false);
            // Add or get CanvasGroup for fading
            inventoryTooFullPopupCanvasGroup = inventoryTooFullPopupUI.GetComponent<CanvasGroup>();
            if (inventoryTooFullPopupCanvasGroup == null)
            {
                inventoryTooFullPopupCanvasGroup = inventoryTooFullPopupUI.AddComponent<CanvasGroup>();
            }
            inventoryTooFullPopupCanvasGroup.alpha = 1f;
        }
        // Hide collision popup UI on load
        if (collisionPopupUI != null)
        {
            collisionPopupUI.SetActive(false);
            // Add or get CanvasGroup for fading
            collisionPopupCanvasGroup = collisionPopupUI.GetComponent<CanvasGroup>();
            if (collisionPopupCanvasGroup == null)
            {
                collisionPopupCanvasGroup = collisionPopupUI.AddComponent<CanvasGroup>();
            }
            collisionPopupCanvasGroup.alpha = 1f;
        }
        if (ItemCollisionPopup2 != null)
        {
            ItemCollisionPopup2.SetActive(false);
        }

        InitialiseInventorySlots();

        fr = this.gameObject.GetComponent<FishingRod>();

        playerAttackRadius = player.gameObject.transform.Find("AttackRadius").gameObject;
        if (playerAttackRadius != null)
        {
            playerAttackRadius.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toolbelt functionality
        Toolbelt_SwapTool();
        Toolbelt_MoveActiveIcon();
        Toolbelt_DisplayTool();

        // Inventory icon movement
        Inventory_FollowMouse();

        // Open/Close inventory UI
        if (player == null)
        {
            //Debug.LogWarning("Player reference not set in Player_Inventory script.");
            return;
        }
        OpenCloseInventory();
        LookAtItem();
        PopupAboveItem();

        // Placement mode logic
        if (isPlacementMode)
        {
            // Handle rotation input
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                placementTargetRotationY -= 90f;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                placementTargetRotationY += 90f;
            }
            // Smoothly interpolate rotation
            placementRotationY = Mathf.MoveTowardsAngle(placementRotationY, placementTargetRotationY, placementRotationSpeed * Time.deltaTime);
            ShowPlacementPreview(placementSlotIndex, placementRotationY);
            ShowPlaceObjectPopup();
            EnablePlayerMovementAndSounds(); // Let player move while placing

            // Cancel placement with C
            if (Input.GetKeyDown(KeyCode.C))
            {
                isPlacementMode = false;
                placementSlotIndex = -1;
                placementRotationY = 0f;
                placementTargetRotationY = 0f;
                HidePlacementPreview();
                HidePlaceObjectPopup();
                return;
            }

            // Confirm placement with E
            if (Input.GetKeyDown(KeyCode.E))
            {
                RemoveItemFromInventory(placementSlotIndex, placementRotationY);
                isPlacementMode = false;
                placementSlotIndex = -1;
                placementRotationY = 0f;
                placementTargetRotationY = 0f;
                HidePlacementPreview();
                HidePlaceObjectPopup();
            }
        }
        else if (inventoryUI != null && inventoryUI.activeSelf)
        {
            int hoveredSlot = GetHoveredInventorySlot();
            if (hoveredSlot != -1)
            {
                ShowPlacementPreview(hoveredSlot, 0f);
                // Enter placement mode with P
                if (Input.GetKeyDown(KeyCode.P))
                {
                    isPlacementMode = true;
                    placementSlotIndex = hoveredSlot;
                    placementRotationY = 0f;
                    placementTargetRotationY = 0f;
                    inventoryUI.SetActive(false);
                    HideToolTip();
                }
            }
            else
            {
                HidePlacementPreview();
            }
            HidePlaceObjectPopup();
        }
        else
        {
            HidePlacementPreview();
            HidePlaceObjectPopup();
        }
            // Handle inventory too full popup timer and fade
            if (inventoryTooFullPopupActive && inventoryTooFullPopupUI != null && inventoryTooFullPopupCanvasGroup != null)
            {
                inventoryTooFullPopupTimer -= Time.deltaTime;
                // Fade out in the last 1 second
                if (inventoryTooFullPopupTimer <= 1f)
                {
                    inventoryTooFullPopupCanvasGroup.alpha = Mathf.Clamp01(inventoryTooFullPopupTimer / 1f);
                }
                else
                {
                    inventoryTooFullPopupCanvasGroup.alpha = 1f;
                }
                if (inventoryTooFullPopupTimer <= 0f)
                {
                    inventoryTooFullPopupUI.SetActive(false);
                    inventoryTooFullPopupActive = false;
                    inventoryTooFullPopupCanvasGroup.alpha = 1f;
                }
            }

        // Handle collision popup timer and fade
        if (collisionPopupActive && collisionPopupUI != null && collisionPopupCanvasGroup != null)
        {
            collisionPopupTimer -= Time.deltaTime;
            // Fade out in the last 1 second
            if (collisionPopupTimer <= 1f)
            {
                collisionPopupCanvasGroup.alpha = Mathf.Clamp01(collisionPopupTimer / 1f);
            }
            else
            {
                collisionPopupCanvasGroup.alpha = 1f;
            }
            if (collisionPopupTimer <= 0f)
            {
                collisionPopupUI.SetActive(false);
                collisionPopupActive = false;
                collisionPopupCanvasGroup.alpha = 1f;
            }
        }

        // Show ItemCollisionPopup2 and hide preview if player is colliding
        var movement = player != null ? player.GetComponent<Player_Movement>() : null;
        if (isPlacementMode && movement != null && movement.isColliding)
        {
            if (previewObject != null) previewObject.SetActive(false);
            if (placeObjectPopupUI != null) placeObjectPopupUI.SetActive(false);
            if (ItemCollisionPopup2 != null)
            {
                Vector3 popupPosition = player.transform.position + Vector3.up * 0.5f; // Match height of ItemCollisionPopup
                ItemCollisionPopup2.transform.position = popupPosition;
                ItemCollisionPopup2.SetActive(true);
                HidePlaceObjectPopup();
            }
        }
        else if (ItemCollisionPopup2 != null && !itemCollisionPopup2Active)
        {
            ItemCollisionPopup2.SetActive(false);
        }
    }

    // Returns the index of the hovered inventory slot, or -1 if none
    private int GetHoveredInventorySlot()
    {
        if (inventorySlots == null) return -1;
        Vector3 mousePos = Input.mousePosition;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            RectTransform rt = inventorySlots[i].GetComponent<RectTransform>();
            if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos))
            {
                return i;
            }
        }
        return -1;
    }

    void OpenCloseInventory()
    {
        //if press TAB, toggle inventory UI
        if (Input.GetKeyDown(KeyCode.Tab) && inventoryUI != null)
        {
            bool isOpen = !inventoryUI.activeSelf;
            inventoryUI.SetActive(isOpen);
            // Enable/disable Player_Movement script
            if (player != null)
            {
                var movement = player.GetComponent<Player_Movement>();
                var sounds = player.GetComponent<Player_SoundEffects>();
                if (movement != null && sounds != null)
                {
                    sounds.enabled = !isOpen ? true : false;
                    movement.enabled = !isOpen ? true : false;
                }
            }
        }
    }

    public void ShowInventoryToolTip(int slotIndex)
    {
        //Debug.Log($"ShowInventoryToolTip called for slot {slotIndex}, item: {slots[slotIndex].item}");

        if (slots[slotIndex].item != null)
        {
            // Check if item is food
            if (slots[slotIndex].item.itemType == "Food" && foodToolTip != null)
            {
                food_itemNameText.text = slots[slotIndex].item.itemName;
                food_itemDescriptionText.text = slots[slotIndex].item.itemDescription;
                food_itemTypeText.text = slots[slotIndex].item.itemType;
                foodToolTip.SetActive(true);
                livesText.text = $"Restores +{slots[slotIndex].item.foodIncreaseAmount} lives";
                foodToolTip.transform.position = Input.mousePosition + toolTipOffset;
                // Make sure inventory tooltip is hidden
                inventoryToolTip.SetActive(false);
            }
            else
            {
                inventoryToolTip.SetActive(true);
                itemNameText.text = slots[slotIndex].item.itemName;
                itemDescriptionText.text = slots[slotIndex].item.itemDescription;
                itemTypeText.text = slots[slotIndex].item.itemType;
                inventoryToolTip.transform.position = Input.mousePosition + toolTipOffset;
                //Make sure food tooltip is hidden
                foodToolTip.SetActive(false);
            }
        }
        else
        {
            inventoryToolTip.SetActive(false);
            foodToolTip.SetActive(false);
        }
    }

    public void HideToolTip()
    {
        if (inventoryToolTip != null)
        {
            inventoryToolTip.SetActive(false);
        }
        if (foodToolTip != null)
        {
            foodToolTip.SetActive(false);
        }
    }

    void LookAtItem()
    {
        // Disable pickup when in placement mode
        if (isPlacementMode)
        {
            currentItem = null;
            if (pickupPopupUI != null) pickupPopupUI.SetActive(false);
            return;
        }
        // Use raycast to look at item in front of player, from mid-body height
        Vector3 rayOrigin = player.transform.position + Vector3.up * 0.3f;
        Vector3 rayDirection = player.transform.forward;
        // Visualize the raycast in the Scene view
        //Debug.DrawRay(rayOrigin, rayDirection * pickupRange, Color.cyan);
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, pickupRange))
        {
            // Try to get Item component from the collider or its parents
            Item item = hit.collider.GetComponent<Item>();

            if (item != null)
            {
                currentItem = item;
                // Debugging
                //Debug.Log("Looking at item: " + currentItem.itemName + " (ID: " + currentItem.itemID + ")");

                // If click E, pick up item
                if (Input.GetKeyDown(KeyCode.E))
                {
                    AddItemToInventory(currentItem); // Add item to inventory
                }
            }
        }
        else
        {
            currentItem = null; // Clear current item if nothing is hit
        }
    }

    void PopupAboveItem()
    {
        if (currentItem != null && pickupPopupUI != null)
        {
            // Position the popup UI above the item in world space
            Vector3 popupPosition = currentItem.transform.position + Vector3.up * 0.5f; // Adjust height as needed
            pickupPopupUI.transform.position = popupPosition;

            // Show the popup UI
            pickupPopupUI.SetActive(true);
        }
        else if (pickupPopupUI != null)
        {
            // Hide the popup UI if no item is looked at
            pickupPopupUI.SetActive(false);
        }
    }

    void InitialiseInventorySlots()
    {
        // Initialise the inventory slots (inventory + toolbelt + 3 outfit slots)
        slots = new InventorySlot[inventorySlots.Length + toolbeltSlots.Length + 3]; // Add space for toolbelt slots and outfit slots

        // First, add inventory slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // Instantiate the icon as a child of the slot and set its local position to zero
            GameObject iconObj = Instantiate(inventorySlotIcon, inventorySlots[i].transform);
            iconObj.transform.localPosition = Vector3.zero;
            Image iconImage = iconObj.GetComponent<Image>();

            slots[i] = new InventorySlot
            {
                slot = inventorySlots[i],
                itemObject = null,
                item = null,
                icon = iconImage
            };
        }

        // Then, add toolbelt slots
        for (int i = 0; i < toolbeltSlots.Length; i++)
        {
            // Instantiate the icon as a child of the slot and set its local position to zero
            GameObject iconObj = Instantiate(inventorySlotIcon, toolbeltSlots[i].transform);
            iconObj.transform.localPosition = Vector3.zero;
            Image iconImage = iconObj.GetComponent<Image>();

            slots[inventorySlots.Length + i] = new InventorySlot
            {
                slot = toolbeltSlots[i],
                itemObject = null,
                item = null,
                icon = iconImage
            };
        }

        // Finally, add the outfit slots (hat, top, and bottom) with dedicated indices
        // Hat slot - index after all inventory and toolbelt slots
        int hatSlotIndex = inventorySlots.Length + toolbeltSlots.Length;
        GameObject iconObjHat = Instantiate(inventorySlotIcon, headSlot.transform);
        iconObjHat.transform.localPosition = Vector3.zero;
        Image iconImageHat = iconObjHat.GetComponent<Image>();

        //Debug.Log($"Hat slot index: {hatSlotIndex}, total slots: {slots.Length}");
        slots[hatSlotIndex] = new InventorySlot
        {
            slot = headSlot,
            itemObject = null,
            item = null,
            icon = iconImageHat
        };

        // Top slot - index after hat slot
        int topSlotIndex = inventorySlots.Length + toolbeltSlots.Length + 1;
        GameObject iconObjTop = Instantiate(inventorySlotIcon, torsoSlot.transform);
        iconObjTop.transform.localPosition = Vector3.zero;
        Image iconImageTop = iconObjTop.GetComponent<Image>();

        slots[topSlotIndex] = new InventorySlot
        {
            slot = torsoSlot,
            itemObject = null,
            item = null,
            icon = iconImageTop
        };

        //Debug.Log($"Top slot index: {topSlotIndex}, total slots: {slots.Length}");
        
        // Bottom slot - index after top slot
        int bottomSlotIndex = inventorySlots.Length + toolbeltSlots.Length + 2;
        GameObject iconObjBottom = Instantiate(inventorySlotIcon, bottomSlot.transform);
        iconObjBottom.transform.localPosition = Vector3.zero;
        Image iconImageBottom = iconObjBottom.GetComponent<Image>();

        slots[bottomSlotIndex] = new InventorySlot
        {
            slot = bottomSlot,
            itemObject = null,
            item = null,
            icon = iconImageBottom
        };
        //Debug.Log($"Bottom slot index: {bottomSlotIndex}, total slots: {slots.Length}");
    }

    void AddItemToInventory(Item item)
    {
        // Find the first empty slot in the inventory
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null && slot.itemObject == null) // If the slot is empty
            {
                // Add the item to this slot
                slot.item = item;
                slot.itemObject = item.gameObject;
                
                // Set the item's owner to the player
                item.owner = player;
                
                if (slot.icon != null)
                {
                    slot.icon.sprite = item.itemIcon;
                    // Make alpha of the Color fully 1
                    Color iconColor = slot.icon.color;
                    iconColor.a = 1f;
                    slot.icon.color = iconColor;
                }
                //Debug.Log($"Added item: {slot.item.itemName} to inventory slot: {slot.slot.name}");

                // Make item not active in the world, and set currentItem to null
                item.gameObject.SetActive(false); // Deactivate the item in the world
                currentItem = null; // Clear current item after picking up
                return;
            }
        }
        //Debug.Log($"Inventory full! Cannot add item: {item.itemName}");
            // Show inventory too full popup at player's position and start fade timer
            if (inventoryTooFullPopupUI != null && player != null && inventoryTooFullPopupCanvasGroup != null)
            {
                Vector3 popupPosition = player.transform.position + Vector3.up * 0.5f;
                inventoryTooFullPopupUI.transform.position = popupPosition;
                inventoryTooFullPopupUI.SetActive(true);
                inventoryTooFullPopupTimer = 2f;
                inventoryTooFullPopupActive = true;
                inventoryTooFullPopupCanvasGroup.alpha = 1f;
            }
    }

    public void RemoveItemFromInventory(int slotIndex, float rotationY = 0f)
    {
        InventorySlot slot = slots[slotIndex];
        var movement = player != null ? player.GetComponent<Player_Movement>() : null;
        if (movement != null && movement.isColliding)
        {
            TryShowCollisionPopupAndCloseInventory(movement);
            return;
        }
        HideCollisionPopup();
        if (slot.item != null)
        {
            //Debug.Log($"Removing item: {slot.item.itemName} from inventory slot: {slot.slot.name}");
            // Only place if not colliding with another object
            if (!IsPlacementColliding(slot, rotationY))
            {
                PlaceItemOnGround(slot, rotationY);
                slot.item = null;
                slot.itemObject = null;
                ClearSlotIcon(slot);
            }
            else
            {
                //Debug.LogWarning("Cannot place item: placement position is colliding with another object.");
                ShowCollisionPopupAtPlayer();
            }
        }
        else
        {
            Debug.LogWarning("No item to remove in slot: " + slot.slot.name);
        }
    }

    private bool IsPlacementColliding(InventorySlot slot, float rotationY)
    {
        if (slot.itemObject == null) return false;
        // Temporarily instantiate a test object at the intended position
        GameObject testObj = Instantiate(slot.itemObject);
        SetLayerRecursively(testObj, 2); // Ignore Raycast
        var testColliders = testObj.GetComponentsInChildren<Collider>();
        foreach (var col in testColliders) col.enabled = true;
        // Set intended position and rotation
        float objectHeight = 0.0f;
        Renderer rend = testObj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            float topY = rend.bounds.max.y;
            float bottomY = rend.bounds.min.y;
            objectHeight = topY - bottomY;
        }
        Vector3 dropPosition = player.transform.position + player.transform.forward * 0.5f;
        dropPosition.y = 0f + (objectHeight / 2f);
        testObj.transform.position = dropPosition;
        testObj.transform.rotation = Quaternion.Euler(0f, slot.itemObject.transform.eulerAngles.y + rotationY, 0f);
        // Check for collision
        bool isColliding = false;
        foreach (var col in testColliders)
        {
            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents, testObj.transform.rotation, placementCollisionMask);
            foreach (var hit in hits)
            {
                if (hit.gameObject != testObj && !hit.isTrigger && hit.gameObject != player)
                {
                    isColliding = true;
                    break;
                }
            }
            if (isColliding) break;
        }
        Destroy(testObj);
        return isColliding;
    }

    private void ShowCollisionPopupAtPlayer()
    {
        if (collisionPopupUI != null && player != null && collisionPopupCanvasGroup != null)
        {
            Vector3 popupPosition = player.transform.position + Vector3.up * 0.5f;
            collisionPopupUI.transform.position = popupPosition;
            collisionPopupUI.SetActive(true);
            collisionPopupTimer = 2.5f;
            collisionPopupActive = true;
            collisionPopupCanvasGroup.alpha = 1f;
        }
    }

    private void TryShowCollisionPopupAndCloseInventory(Player_Movement movement)
    {
        //Debug.LogWarning("Cannot place item: player is colliding with something.");
        if (collisionPopupUI != null && player != null && collisionPopupCanvasGroup != null)
        {
            Vector3 popupPosition = player.transform.position + Vector3.up * 0.5f;
            collisionPopupUI.transform.position = popupPosition;
            collisionPopupUI.SetActive(true);
            collisionPopupTimer = 2.5f;
            collisionPopupActive = true;
            collisionPopupCanvasGroup.alpha = 1f;
            if (inventoryUI != null)
            {
                inventoryUI.SetActive(false);
                movement.enabled = true;
                var sounds = player.GetComponent<Player_SoundEffects>();
                if (sounds != null)
                {
                    sounds.enabled = true;
                }
            }
        }
    }

    private void HideCollisionPopup()
    {
        if (collisionPopupUI != null && collisionPopupCanvasGroup != null)
        {
            collisionPopupUI.SetActive(false);
            collisionPopupActive = false;
            collisionPopupTimer = 0f;
            collisionPopupCanvasGroup.alpha = 1f;
        }
    }

    private void PlaceItemOnGround(InventorySlot slot, float rotationY = 0f)
    {
        GameObject obj = slot.itemObject;
        Renderer rend = obj != null ? obj.GetComponentInChildren<Renderer>() : null;
        float objectHeight = 0.0f;
        float bottomY = 0.0f;
        if (rend != null)
        {
            float topY = rend.bounds.max.y;
            bottomY = rend.bounds.min.y;
            objectHeight = topY - bottomY;
        }
        Vector3 dropPosition = player.transform.position + player.transform.forward * 0.5f;
        float raycastStartHeight = 2f;
        Vector3 rayOrigin = new Vector3(dropPosition.x, dropPosition.y + raycastStartHeight, dropPosition.z);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hitInfo, raycastStartHeight * 2f))
        {
            dropPosition.y = hitInfo.point.y + (objectHeight / 2f);
        }
        else
        {
            dropPosition.y = objectHeight + (-0.5f);
        }
        obj.transform.position = dropPosition;
        obj.transform.rotation = Quaternion.Euler(0f, slot.itemObject.transform.eulerAngles.y + rotationY, 0f);
        
        // Clear the item's owner when it's dropped
        Item itemScript = obj.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.owner = null;
        }
        
        // Set placed object and all children to Ground layer (6)
        SetLayerRecursively(obj, 6);
        obj.SetActive(true);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void ClearSlotIcon(InventorySlot slot)
    {
        if (slot.icon != null)
        {
            slot.icon.sprite = null;
            Color iconColor = slot.icon.color;
            iconColor.a = 0f;
            slot.icon.color = iconColor;
        }
    }

    void UpdateInventorySprites()
    {
        // Set the sprite of the inventory slot to the item's icon
        foreach (InventorySlot slot in slots)
        {
            if (slot.item != null && slot.icon != null)
            {
                // Set icon of the slot to the item's icon
                slot.icon.sprite = slot.item.itemIcon;
            }
        }
    }

    public void ShowPlacementPreview(int slotIndex, float rotationY = 0f)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return; // Invalid slot index
        var slot = slots[slotIndex];
        if (slot.itemObject == null) return;

        // Only instantiate if needed
        if (previewObject == null || previewObject.name != slot.itemObject.name + "(Clone)")
        {
            if (previewObject != null) Destroy(previewObject);
            previewObject = Instantiate(slot.itemObject);
            lastValidPreviewPosition = Vector3.zero;
        }

        // Set previewObject and all children to Ignore Raycast layer (2)
        SetLayerRecursively(previewObject, 2);

        // Disable collisions between player and previewObject
        var previewColliders = previewObject.GetComponentsInChildren<Collider>();
        var playerColliders = player.GetComponentsInChildren<Collider>();
        foreach (var pCol in playerColliders)
        {
            foreach (var prCol in previewColliders)
            {
                Physics.IgnoreCollision(pCol, prCol, true);
            }
        }

        // Disable colliders before raycast/collision check
        foreach (var col in previewColliders) col.enabled = false;

        // Calculate intended drop position
        float objectHeight = 0.0f;
        Renderer rend = slot.itemObject != null ? slot.itemObject.GetComponentInChildren<Renderer>() : null;
        if (rend != null)
        {
            float topY = rend.bounds.max.y;
            float bottomY = rend.bounds.min.y;
            objectHeight = topY - bottomY;
        }
        Vector3 dropPosition = player.transform.position + player.transform.forward * 0.5f;
        float raycastStartHeight = 2f;
        Vector3 rayOrigin = new Vector3(dropPosition.x, dropPosition.y + raycastStartHeight, dropPosition.z);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hitInfo, raycastStartHeight * 2f))
        {
            dropPosition.y = hitInfo.point.y + (objectHeight / 2f) + 0.01f; // Slightly above ground to avoid z-fighting
        }
        else
        {
            dropPosition.y = objectHeight + (-0.5f) + 0.01f;
        }
        // Set rotation before collision check
        Quaternion intendedRotation = Quaternion.Euler(0f, slot.itemObject.transform.eulerAngles.y + rotationY, 0f);
        previewObject.transform.rotation = intendedRotation;
        previewObject.transform.position = dropPosition;
        previewObject.SetActive(true);

        // Enable colliders for collision check
        foreach (var col in previewColliders) col.enabled = true;

        // Check for collision at intended position, but ignore collisions with the player
        bool isColliding = false;
        foreach (var col in previewColliders)
        {
            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents, previewObject.transform.rotation, placementCollisionMask);
            foreach (var hit in hits)
            {
                if (hit.gameObject != previewObject && !hit.isTrigger && hit.gameObject != player)
                {
                    isColliding = true;
                    break;
                }
            }
            if (isColliding) break;
        }
        if (isColliding)
        {
            // Make preview invisible
            foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
            // Show ItemCollisionPopup2 above player's head
            if (ItemCollisionPopup2 != null)
            {
                ItemCollisionPopup2.transform.position = player.transform.position + Vector3.up * 0.5f;
                ItemCollisionPopup2.SetActive(true);
            }
            // Hide placeObjectPopupUI
            if (placeObjectPopupUI != null)
            {
                placeObjectPopupUI.SetActive(false);
            }
            // Hide pickupPopupUI
            if (pickupPopupUI != null)
            {
                pickupPopupUI.SetActive(false);
            }
        }
        else
        {
            // Make preview visible
            foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
            if (ItemCollisionPopup2 != null)
            {
                ItemCollisionPopup2.SetActive(false);
            }
            lastValidPreviewPosition = previewObject.transform.position;
        }
        // Make all renderers semi-transparent (URP compatible)
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        foreach (var r in previewObject.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                if (urpLit != null)
                {
                    mat.shader = urpLit;
                    mat.SetFloat("_Surface", 1f); // 1 = Transparent
                    mat.SetFloat("_Blend", 0f);   // 0 = Alpha
                    mat.SetFloat("_ZWrite", 0f);
                    mat.renderQueue = 3000;
                    Color c = mat.color;
                    c.a = 0.5f;
                    mat.color = c;
                }
            }
        }
    }

    public void HidePlacementPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    private void ShowPlaceObjectPopup()
    {
        if (placeObjectPopupUI != null && previewObject != null)
        {
            Vector3 popupPosition = previewObject.transform.position + Vector3.up * 0.5f;
            placeObjectPopupUI.transform.position = popupPosition;
            placeObjectPopupUI.SetActive(true);
        }
    }
    private void HidePlaceObjectPopup()
    {
        if (placeObjectPopupUI != null)
        {
            placeObjectPopupUI.SetActive(false);
        }
    }
    private void EnablePlayerMovementAndSounds()
    {
        if (player != null)
        {
            var movement = player.GetComponent<Player_Movement>();
            var sounds = player.GetComponent<Player_SoundEffects>();
            if (movement != null) movement.enabled = true;
            if (sounds != null) sounds.enabled = true;
        }
    }


    // Inventory item icon movements

    // Call this to pick up an item from the inventory UI and attach it to the mouse
    public void Inventory_PickupItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return; // Invalid slot index
        var slot = slots[slotIndex];
        if (slot.item == null || slot.icon == null) return; // No item to pick up

        // Instantiate a new icon from the prefab and set it to the root canvas
        newIconObj = Instantiate(inventorySlotIcon, inventoryUI.transform.root);
        Image newIcon = newIconObj.GetComponent<Image>();
        if (newIcon != null && slot.icon != null)
        {
            newIcon.sprite = slot.icon.sprite;
            newIcon.raycastTarget = false;
            newIcon.transform.position = Input.mousePosition;
            // Ensure the icon is fully visible
            Color iconColor = newIcon.color;
            iconColor.a = 1f;
            newIcon.color = iconColor;
            // Set width and height
            RectTransform rt = newIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(10f, 15f);
            }
        }

        // Store only the item and itemObject in the temporary slot
        tempSlot.item = slot.item;
        tempSlot.itemObject = slot.itemObject;

        // Clear the original slot's item and icon sprite, but keep the icon reference
        slot.item = null;
        slot.itemObject = null;
        if (slot.icon != null)
        {
            slot.icon.sprite = null;
            Color iconColor = slot.icon.color;
            iconColor.a = 0f;
            slot.icon.color = iconColor;
        }
    }

    public void Inventory_DropItem(int slotIndex)
    {
        //Place item into the item slot
        if (slotIndex < 0 || slotIndex >= slots.Length) return; // Invalid slot index

        // Check if the item can be dropped in this slot type
        if (!CanItemBeDroppedInSlot(tempSlot.item, slotIndex))
        {
            //Debug.LogWarning($"Cannot drop {tempSlot.item.itemType} item in this slot type.");
            return;
        }

        var slot = slots[slotIndex];

        // Check if slot is already occupied
        if (slot.item != null)
        {
            //Debug.LogWarning("Slot is already occupied.");
            return;
        }

        // Place the item into the slot
        slot.item = tempSlot.item;
        slot.itemObject = tempSlot.itemObject;

        // Update the slot's icon sprite and make visible
        if (slot.icon != null)
        {
            slot.icon.sprite = slot.item.itemIcon;
            Color iconColor = slot.icon.color;
            iconColor.a = 1f;
            slot.icon.color = iconColor;
        }

        // Clear the temporary slot
        tempSlot.item = null;
        tempSlot.itemObject = null;

        // Destroy the drag icon visual
        if (newIconObj != null)
        {
            Destroy(newIconObj);
            newIconObj = null;
        }
    }

    public void Inventory_SwapItems(int slotIndex)
    {
        // Drop the held temp item into the specified slot, and set the item that was inside this slot as the held temp item
        if (slotIndex < 0 || slotIndex >= slots.Length) return; // Invalid slot index

        var slot = slots[slotIndex];

        // Check if the temp item can be dropped in this slot type
        if (!CanItemBeDroppedInSlot(tempSlot.item, slotIndex))
        {
            //Debug.LogWarning($"Cannot drop {tempSlot.item.itemType} item in this slot type.");
            return;
        }

        // Store the slot's current item and itemObject in newTempSlot
        Item prevItem = slot.item;
        GameObject prevItemObject = slot.itemObject;

        // Place the held temp item into the slot
        slot.item = tempSlot.item;
        slot.itemObject = tempSlot.itemObject;

        // Update the slot's icon sprite and make visible
        if (slot.icon != null)
        {
            if (slot.item != null)
            {
                slot.icon.sprite = slot.item.itemIcon;
                Color iconColor = slot.icon.color;
                iconColor.a = 1f;
                slot.icon.color = iconColor;
            }
            else
            {
                slot.icon.sprite = null;
                Color iconColor = slot.icon.color;
                iconColor.a = 0f;
                slot.icon.color = iconColor;
            }
        }

        // Destroy the old drag icon visual
        if (newIconObj != null)
        {
            Destroy(newIconObj);
            newIconObj = null;
        }

        // Set the new tempSlot to the previously held item
        tempSlot.item = prevItem;
        tempSlot.itemObject = prevItemObject;

        // Instantiate a new icon from the prefab and set it to the root canvas (for drag visual)
        newIconObj = Instantiate(inventorySlotIcon, inventoryUI.transform.root);
        Image newIcon = newIconObj.GetComponent<Image>();
        if (newIcon != null)
        {
            if (tempSlot.item != null)
            {
                newIcon.sprite = tempSlot.item.itemIcon;
                newIcon.raycastTarget = false;
                newIcon.transform.position = Input.mousePosition;
                // Ensure the icon is fully visible
                Color iconColor = newIcon.color;
                iconColor.a = 1f;
                newIcon.color = iconColor;
            }
            else
            {
                newIcon.sprite = null;
                Color iconColor = newIcon.color;
                iconColor.a = 0f;
                newIcon.color = iconColor;
            }
            // Set width and height
            RectTransform rt = newIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(10f, 15f);
            }
        }

    }

    public void Inventory_CancelDrag(int slotIndex)
    {
        // Return the held temp item back to its original slot
        if (slotIndex < 0 || slotIndex >= slots.Length) return; // Invalid slot index

        var slot = slots[slotIndex];

        // Place the held temp item back into the slot
        slot.item = tempSlot.item;
        slot.itemObject = tempSlot.itemObject;

        // Update the slot's icon sprite and make visible
        if (slot.icon != null)
        {
            if (slot.item != null)
            {
                slot.icon.sprite = slot.item.itemIcon;
                Color iconColor = slot.icon.color;
                iconColor.a = 1f;
                slot.icon.color = iconColor;
            }
            else
            {
                slot.icon.sprite = null;
                Color iconColor = slot.icon.color;
                iconColor.a = 0f;
                slot.icon.color = iconColor;
            }
        }

        // Clear the temporary slot
        tempSlot.item = null;
        tempSlot.itemObject = null;

        // Destroy the drag icon visual
        if (newIconObj != null)
        {
            Destroy(newIconObj);
            newIconObj = null;
        }
    }

    public void Inventory_FollowMouse()
    {
        if (newIconObj != null)
        {
            newIconObj.transform.position = Input.mousePosition;
        }

        // Always hide tooltip when dragging an item
        if (tempSlot.item != null)
        {
            HideToolTip();
        }
    }

    // Toolbelt functionality

    public void Toolbelt_SwapTool()
    {
        // Use scroll wheel to shift the toolbelt items, if activeToolIndex = 4, no tool is out

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        int maxIndex = toolbeltSlots.Length;
        if (scroll > 0f)
        {
            // Scroll forward
            int previousToolIndex = activeToolIndex;
            activeToolIndex = (activeToolIndex + 1) % (maxIndex + 1);
            
            // Call fishing rod methods when switching tools
            if (previousToolIndex != activeToolIndex && fr != null && fr.gameObject != null)
            {
                fr.Uncast();
                fr.HideCatch();
            }
        }
        else if (scroll < 0f)
        {
            // Scroll backward
            int previousToolIndex = activeToolIndex;
            activeToolIndex = (activeToolIndex - 1 + (maxIndex + 1)) % (maxIndex + 1);
            
            // Call fishing rod methods when switching tools
            if (previousToolIndex != activeToolIndex && fr != null && fr.gameObject != null)
            {
                fr.Uncast();
                fr.HideCatch();
            }
        }
    }

    public void Toolbelt_MoveActiveIcon()
    {
        switch (activeToolIndex)
        {
            case 0:
                if (activeToolUI != null && toolbeltSlots.Length > 0)
                {
                    activeToolUI.SetActive(true);
                    activeToolUI.transform.position = toolbeltSlots[0].transform.position;
                }
                break;
            case 1:
                if (activeToolUI != null && toolbeltSlots.Length > 1)
                {
                    activeToolUI.SetActive(true);
                    activeToolUI.transform.position = toolbeltSlots[1].transform.position;
                }
                break;
            case 2:
                if (activeToolUI != null && toolbeltSlots.Length > 2)
                {
                    activeToolUI.SetActive(true);
                    activeToolUI.transform.position = toolbeltSlots[2].transform.position;
                }
                break;
            case 3:
                if (activeToolUI != null && toolbeltSlots.Length > 3)
                {
                    activeToolUI.SetActive(true);
                    activeToolUI.transform.position = toolbeltSlots[3].transform.position;
                }
                break;
            case 4:
                if (activeToolUI != null)
                {
                    // Set invisible if no tool is out
                    activeToolUI.SetActive(false);
                    Destroy(activeToolObject);
                    activeToolObject = null;
                }
                break;
            default:
                break;
        }
    }

    public void Toolbelt_DisplayTool()
    {
        // Get the toolbelt slot corresponding to activeToolIndex
        InventorySlot slot = null;

        switch (activeToolIndex)
        {
            case 0:
                slot = slots.Length > inventorySlots.Length + 1 ? slots[inventorySlots.Length + 0] : null;
                break;
            case 1:
                slot = slots.Length > inventorySlots.Length + 1 ? slots[inventorySlots.Length + 1] : null;
                break;
            case 2:
                slot = slots.Length > inventorySlots.Length + 1 ? slots[inventorySlots.Length + 2] : null;
                break;
            case 3:
                slot = slots.Length > inventorySlots.Length + 1 ? slots[inventorySlots.Length + 3] : null;
                break;
            case 4:
                slot = null;
                break;
        }

        // Check if we need to change the tool (only recreate when necessary)
        bool needsToolChange = false;
        
        if (activeToolIndex == 4)
        {
            // No tool should be active
            needsToolChange = (activeToolObject != null);
            if (needsToolChange)
            {
                isHoldingItem = false;
            }
        }
        else if (slot != null && slot.item != null)
        {
            // Check if current tool matches the slot's item
            if (activeToolObject == null)
            {
                needsToolChange = true;
            }
            else
            {
                Item currentItemScript = activeToolObject.GetComponent<Item>();
                if (currentItemScript == null || currentItemScript.itemName != slot.item.itemName)
                {
                    needsToolChange = true;
                }
            }
        }
        else
        {
            // Slot is empty, remove tool if one exists
            needsToolChange = (activeToolObject != null);
        }

        // Only recreate tool if needed
        if (needsToolChange)
        {
            // Remove existing tool
            if (activeToolObject != null)
            {
                Destroy(activeToolObject);
                activeToolObject = null;
            }

            if (activeToolIndex == 4 || slot == null || slot.item == null)
            {
                isHoldingItem = false;
                return;
            }

            // Create new tool
            activeToolObject = Instantiate(slot.itemObject);
            isHoldingItem = true;
            
            // Disable all colliders on the active tool object
            foreach (var col in activeToolObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
            activeToolObject.transform.SetParent(playerHand.transform);

            // Get the active tool's renderer and configure position
            Renderer toolRenderer = activeToolObject.GetComponentInChildren<Renderer>();
            float objectLength = 0.0f;
            float posOffset = 0.0f;
            float rotOffset = 0.0f;
            bool isTall = false;
            int toolId = 0;

            Vector3 activeToolOffset = Vector3.zero;
            // Try to get the Item script from the active tool object
            Item itemScript = activeToolObject.GetComponent<Item>();
            if (itemScript != null)
            {
                isTall = itemScript.isTall;
                posOffset = itemScript.positionOffset;
                rotOffset = itemScript.rotationOffset;
                toolId = itemScript.toolId;
            }
            if (toolRenderer != null)
            {
                if (isTall)
                {
                    objectLength = toolRenderer.bounds.size.y;
                    activeToolOffset = new Vector3(0.0f, objectLength * -0.8f, 0.0f); // Make object appear below hand if tall
                }
                else
                {
                    objectLength = toolRenderer.bounds.size.z;
                    activeToolOffset = new Vector3(0.0f, 0.0f, objectLength * 0.3f); // Make object appear in along hand if lengthwise
                }
            }

            // Apply offset based on length of the tool
            activeToolObject.transform.localPosition = Vector3.zero + activeToolOffset * posOffset;
            activeToolObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotOffset);
            activeToolObject.SetActive(true);
        }

        // Handle animations and input for the active tool (run every frame)
        if (activeToolObject != null)
        {
            Item itemScript = activeToolObject.GetComponent<Item>();
            int toolId = itemScript != null ? itemScript.toolId : 0;
            Animator anim = player != null ? player.GetComponent<Animator>() : null;

            switch (toolId)
            {
                case 1:
                    // Knife
                    fr.runScript = false;
                    break;
                case 2:
                    // Fishing rod
                    fr.runScript = true;
                    break;
                case 3:
                    // Axe
                    if (fr != null && fr.gameObject != null) fr.enabled = false;
                    fr.runScript = false;
                    break;
                case 4:
                    // Shovel
                    if (fr != null && fr.gameObject != null) fr.enabled = false;
                    fr.runScript = false;
                    break;
                default:
                    // Disable fishing rod script and clean up
                    fr.runScript = false;
                    break;
            }

            // Add listener for pressing LMB down to use the tool, make sure inventory is not open
            if (Input.GetMouseButtonDown(0) && inventoryUI.activeSelf == false)
            {
                UseTool(toolId);
            }
            else
            {
                // TOOL ANIMATIONS - IDLING
                if (fr != null && fr.gameObject != null && fr.hasCasted)
                {
                    if (anim != null) anim.SetInteger("toolUsed", 2);
                }
                else
                {
                    if (anim != null) anim.SetInteger("toolUsed", 0);
                    if (fr != null && fr.gameObject != null) fr.Uncast();
                }
            }
        }
        else
        {
            // No tool to display or slot is empty
            isHoldingItem = false;
        }
    }

    public void UseTool(int toolId)
    {
        // Implement tool usage logic based on toolId
        Debug.Log($"Using tool with ID: {toolId}");
        // Example: If toolId corresponds to a watering can, water the plant in front of the player

        // Get player's animator
        Animator anim = player != null ? player.GetComponent<Animator>() : null;
        if (anim != null)
        {
            switch (toolId)
            {
                case 1:
                    Debug.Log("Knife used");
                    anim.SetInteger("toolUsed", 1);
                    StartCoroutine(ActivateAttackRadius(1.0f, 10)); // Active for 1 second
                    break;
                case 2:
                    Debug.Log("Fishing rod used");
                    fr.Cast();
                    break;
                case 3:
                    Debug.Log("Axe used");
                    anim.SetInteger("toolUsed", 1);
                    //anim.SetInteger("toolUsed", 3);
                    StartCoroutine(ActivateAttackRadius(2.0f, 20)); // Active for 2 seconds
                    break;
                case 4:
                    Debug.Log("Shovel used");
                    anim.SetInteger("toolUsed", 4);
                    break;
                default:
                    //Debug.Log("Not a tool.");
                    anim.SetInteger("toolUsed", 0);
                    fr.Uncast();
                    fr.HideCatch();
                    break;
            }
        }
    }

    // Helper methods to identify slot types
    public bool IsInventorySlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inventorySlots.Length;
    }

    public bool IsToolbeltSlot(int slotIndex)
    {
        return slotIndex >= inventorySlots.Length && slotIndex < inventorySlots.Length + toolbeltSlots.Length;
    }

    public bool IsOutfitSlot(int slotIndex)
    {
        return slotIndex >= inventorySlots.Length + toolbeltSlots.Length && slotIndex < slots.Length;
    }

    public bool IsHatSlot(int slotIndex)
    {
        return slotIndex == inventorySlots.Length + toolbeltSlots.Length;
    }

    public bool IsTopSlot(int slotIndex)
    {
        return slotIndex == inventorySlots.Length + toolbeltSlots.Length + 1;
    }

    public bool IsBottomSlot(int slotIndex)
    {
        return slotIndex == inventorySlots.Length + toolbeltSlots.Length + 2;
    }

    // Validation method to check if an item can be placed in a specific slot type
    public bool CanItemBeDroppedInSlot(Item item, int slotIndex)
    {
        if (item == null) return false;

        // Items can always go in inventory slots
        if (IsInventorySlot(slotIndex)) return true;

        // Tools can go in toolbelt slots
        if (IsToolbeltSlot(slotIndex) && item.itemType == "Tool") return true;

        // Outfit items in their respective slots
        if (IsHatSlot(slotIndex) && item.itemType == "Hat") return true;
        if (IsTopSlot(slotIndex) && item.itemType == "Top") return true;
        if (IsBottomSlot(slotIndex) && item.itemType == "Bottoms") return true;

        return false;
    }

    // Coroutine to activate attack radius for a set duration
    private System.Collections.IEnumerator ActivateAttackRadius(float duration, int damage)
    {
        Animator anim = player != null ? player.GetComponent<Animator>() : null;

        if (playerAttackRadius != null)
        {
            playerAttackRadius.SetActive(true);
            Debug.Log("Attack radius activated");
            AttackRadius attackRadiusScript = playerAttackRadius.GetComponent<AttackRadius>();
            if (attackRadiusScript != null)
            {
                attackRadiusScript.damageToCause = damage;
            }

            // Stop the player moving for x amount of seconds
            // if (player != null)
            // {
            //     var movement = player.GetComponent<Player_Movement>();
            //     var sounds = player.GetComponent<Player_SoundEffects>();
            //     if (movement != null) movement.enabled = false;
            //     if (sounds != null) sounds.enabled = false;
            //     anim.SetInteger("MovementPhase", 0); // Set to idle
            // }

            // Wait for the specified duration
            yield return new WaitForSeconds(duration);

            playerAttackRadius.SetActive(false);

            // Debug.Log("Attack radius deactivated");
            // if (player != null)
            // {
            //     var movement = player.GetComponent<Player_Movement>();
            //     var sounds = player.GetComponent<Player_SoundEffects>();
            //     if (movement != null) movement.enabled = true;
            //     if (sounds != null) sounds.enabled = true;
            // }
        }
    }

}