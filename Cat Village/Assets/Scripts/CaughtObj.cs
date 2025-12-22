using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class CaughtObj : MonoBehaviour
{

    //Player
    public GameObject player;
    public GameObject handBone;
    public Player_Inventory player_Inventory;
    public Animator anim;
    public Camera camera;
    public Vector3 camPos;
    public Quaternion camRot;
    public GameObject camFocus;

    public GameObject UIMenu; // UI Menu to show when object is caught, shovelling objects
    public GameObject UIMenu2; // Second UI Menu to show when object is caught, hoeing seeds
    public TextMeshProUGUI objectName; // Text for object name
    public TextMeshProUGUI objectDescription; // Text for object description
    public TextMeshProUGUI objectName2; // Text for object name
    public TextMeshProUGUI objectDescription2; // Text for object description
    public Button dropItemButton; // Button to drop the item
    public Button addToInventoryButton; // Button to add item to inventory
    public bool isInventoryFull = false;

    public Shovel shovelScript;
    public Hoe hoeScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player_Inventory = this.GetComponent<Player_Inventory>();
        anim = player.GetComponent<Animator>();
        shovelScript = this.gameObject.GetComponent<Shovel>();
        hoeScript = this.gameObject.GetComponent<Hoe>();
        HideMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayCaughtObject(GameObject obj)
    {
        obj.GetComponent<Collider>().enabled = false;
        // Display caught object to player
        Debug.Log("Caught object: " + obj.name);

        // Set the object to be a child of the player's hand bone
        obj.transform.SetParent(handBone.transform);
        obj.transform.localPosition = Vector3.zero; // Adjust position as needed
        obj.transform.localRotation = Quaternion.identity; // Adjust rotation as needed
    
        // Disable any tool in hand of the player
        player_Inventory.activeToolIndex = 4; // Set to unarmed
        player_Inventory.canSwapTool = false;

        // Make player unable to move while holding object & play catch animation
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = false;
        if (sounds != null) sounds.enabled = false;

        anim.SetInteger("MovementPhase", 0);
        anim.SetInteger("toolUsed", 0);
        anim.SetBool("catch", true);
        anim.SetBool("holdTool", false);

        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);
        hoeScript.burySeedPopupUI.SetActive(false);

        // Hide all tools
        player_Inventory.activeToolIndex = 0; // Unarmed
        player_Inventory.Toolbelt_DisplayTool();

        // Rotate the player to face the camera
        player.transform.rotation = Quaternion.Euler(0, camera.transform.eulerAngles.y+ 180, 0);

        // Store camera's old position and rotation
        camPos = camera.transform.position;
        camRot = camera.transform.rotation;

        // Disable the camera's follow script
        Player_Camera pc = camera.GetComponent<Player_Camera>();
        if (pc != null)
        {
            pc.enabled = false;
        }

        // Move camera to focus on player holding object gradually
        StartCoroutine(MoveCameraToFocus());

        // Check if player's inventory is full or not
        isInventoryFull = player_Inventory.IsInventoryFull();

        // UI STUFF HERE
        if (isInventoryFull)
        {
            addToInventoryButton.interactable = false;
        }
        else
        {
            addToInventoryButton.interactable = true;
        }

        // Check which menu to display based on object type
        if (hoeScript.runScript)
        {
            // Hoe menu
            ShowHoeMenu();
        }
        else if (shovelScript.runScript)
        {
            // Shovel menu
            ShowMenu();
        }

        // Get object's info from its Item script
        Item itemScript = obj.GetComponent<Item>();

        Debug.Log("Item script found: " + (itemScript != null));

        if (itemScript != null)
        {
            if(isInventoryFull)
            {
                UpdateMenuInfo(itemScript.itemName, itemScript.itemDescription + "\n ... my inventory is full so I can't pick it up.", itemScript.isEvil);
            }
            else
            {
                Debug.Log("Item name: " + itemScript.itemName);
                Debug.Log("Item description: " + itemScript.itemDescription);

                UpdateMenuInfo(itemScript.itemName, itemScript.itemDescription, itemScript.isEvil);
            }
        }
        else {
            // Fallback info if no Item script is found
            if(isInventoryFull)
            {
                UpdateMenuInfo(obj.name, "No description available...\n my inventory is full so I can't pick it up.", false);
            }
            else 
            {
                UpdateMenuInfo(obj.name, "No description available...", false);
            }
        }
    }

    public void FocusCameraBuryObject()
    {
        // Make player unable to move while burying object
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = false;
        if (sounds != null) sounds.enabled = false;

        anim.SetInteger("MovementPhase", 0);

        // Rotate the player to face the camera
        player.transform.rotation = Quaternion.Euler(0, camera.transform.eulerAngles.y+ 180, 0);

        // Store camera's old position and rotation
        camPos = camera.transform.position;
        camRot = camera.transform.rotation;

        // Disable the camera's follow script
        Player_Camera pc = camera.GetComponent<Player_Camera>();
        if (pc != null)
        {
            pc.enabled = false;
        }

        // Move camera to focus on player holding object gradually
        StartCoroutine(MoveCameraToFocus());

        // UI STUFF HERE

    }

    public void HideMenu()
    {
        UIMenu.SetActive(false);
        UIMenu2.SetActive(false);
    }

    void ShowMenu()
    {
        UIMenu.SetActive(true);
    }

    void ShowHoeMenu()
    {
        UIMenu2.SetActive(true);
    }

    public void TossObject() // Shovel
    {
        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menu
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Toss object stuff here...
        // Detach object from hand
        GameObject obj = handBone.transform.GetChild(0).gameObject;
        obj.transform.SetParent(null);
        // Enable object's collider
        obj.GetComponent<Collider>().enabled = true;
        // Place object in front of player with lerp
        StartCoroutine(TossObjectAnimation(obj));
    }

    private IEnumerator TossObjectAnimation(GameObject obj)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = player.transform.position + new Vector3(0f, 0f, -0.5f);

        // Make sure targetPos.y is at ground level
        targetPos.y = 0.02f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        obj.transform.position = targetPos;
    }

    public void AddToInventory()
    {
        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);
        hoeScript.burySeedPopupUI.SetActive(false);
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menu
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Add object to inventory stuff here...
        // Get Item script from object
        Item item = handBone.transform.GetChild(0).gameObject.GetComponent<Item>();
        // Add to player's inventory
        player_Inventory.AddItemToInventory(item);
    }

    public void ReburyItem()
    {
        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);
        hoeScript.burySeedPopupUI.SetActive(false);
        Debug.Log("Reburying item...");
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menus
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Rebury object stuff here...
        // Detach object from hand
        GameObject obj = handBone.transform.GetChild(0).gameObject;

        Debug.Log("ITem to rebury: " + obj.name);
        obj.transform.SetParent(null);

        // Rebury in nearest soil using the Shovel script mechanism...
        shovelScript.SelectObjectToBury(obj);

        shovelScript.BuryHole();
    }

    // Rebury in specific hole
    public void ReburyItemInSoil(Soil soil)
    {
        Debug.Log("Reburying item in specific soil...");
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menu
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Rebury object stuff here...
        // Detach object from hand
        if (soil == null)
        {
            Debug.LogError("ReburyItemInSoil called with NULL soil reference");
            return;
        }

        if (handBone.transform.childCount == 0)
        {
            Debug.LogError("ReburyItemInSoil: No object in hand to rebury");
            return;
        }

        GameObject obj = handBone.transform.GetChild(0).gameObject;
        Debug.Log($"ReburyItemInSoil: soil='{soil.name}' (enabled={soil.isActiveAndEnabled}), obj='{obj.name}' (active={obj.activeSelf})");
        obj.transform.SetParent(null);
        // Disable collider to avoid immediate physics interactions
        var col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Rebury in specified soil
        soil.BuryObject(obj);
        Debug.Log($"After BuryObject: soil.buriedObject='{(soil.buriedObject != null ? soil.buriedObject.name : "NULL")}'");
    }

    void UpdateMenuInfo(string name, string description, bool isEvil)
    {
        objectName.text = "Wow! You just picked up a " + name + "!";
        objectName2.text = "Wow! You just picked up a " + name + "!";
        objectDescription.text = description;
        objectDescription2.text = description;
        if (isEvil)
        {
            objectName.text += " (Evil Item)";
            objectName2.text += "(Evil Item)";
        }
    }

    /// Hoe methods ///
    public void BurySeedButton()
    {
        // Hide bury UI
        hoeScript.burySeedPopupUI.SetActive(false);
        Debug.Log("Retilling item...");
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menu
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Rebury object stuff here...
        // Detach object from hand
        GameObject obj = handBone.transform.GetChild(0).gameObject;

        Debug.Log("ITem to rebury: " + obj.name);
        obj.transform.SetParent(null);

        // Rebury in nearest soil using the Shovel script mechanism...
        hoeScript.SelectSeedToBury(obj);

        hoeScript.BurySeed();
    }

    public void BurySeedInSpecificHole(TilledSoil ts)
    {
        Debug.Log("Reburying item in specific tilled soil...");
        // Re-enable camera movement
        StartCoroutine(MoveCameraToOldPosition());
        // Hide menu
        HideMenu();
        // Renable player movement
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = true;
        if (sounds != null) sounds.enabled = true;
        anim.SetBool("catch", false);
        player_Inventory.canSwapTool = true;

        // Rebury object stuff here...
        // Detach object from hand
        if (ts == null)
        {
            Debug.LogError("ReburySeedInSpecificHole called with NULL soil reference");
            return;
        }

        if (handBone.transform.childCount == 0)
        {
            Debug.LogError("ReburySeedInSpecificHole: No object in hand to rebury");
            return;
        }

        GameObject obj = handBone.transform.GetChild(0).gameObject;
        obj.transform.SetParent(null);
        // Disable collider to avoid immediate physics interactions
        var col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Rebury in specified tilled soil
        hoeScript.SelectSeedToBury(obj);
        hoeScript.BurySeed();
    }

    /////// Camera movement coroutines ///////

    private IEnumerator MoveCameraToFocus()
    {
        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);

        float duration = 1f; // Time to move camera
        float elapsedTime = 0f;
        
        Vector3 startPos = camPos;
        Quaternion startRot = camRot;
        Vector3 targetPos = camFocus.transform.position;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // Use smooth step for easing
            t = t * t * (3f - 2f * t);
            
            camera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            //camera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position and rotation are exact
        camera.transform.position = targetPos;
    }

    public IEnumerator MoveCameraToOldPosition()
    {
        // Hide bury UI
        shovelScript.buryPopupUI.SetActive(false);
        
        float duration = 1f; // Time to move camera back
        float elapsedTime = 0f;
        
        Vector3 startPos = camera.transform.position;
        Quaternion startRot = camera.transform.rotation;
        Vector3 targetPos = camPos;
        Quaternion targetRot = camRot;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // Use smooth step for easing
            t = t * t * (3f - 2f * t);
            
            camera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            camera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position and rotation are exact
        camera.transform.position = targetPos;
        camera.transform.rotation = targetRot;
        
        // Re-enable the camera's follow script
        Player_Camera pc = camera.GetComponent<Player_Camera>();
        if (pc != null)
        {
            pc.enabled = true;
        }
    }
}
