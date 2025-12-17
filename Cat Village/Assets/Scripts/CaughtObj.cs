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

    public GameObject UIMenu; // UI Menu to show when object is caught
    public TextMeshProUGUI objectName; // Text for object name
    public TextMeshProUGUI objectDescription; // Text for object description
    public Button dropItemButton; // Button to drop the item
    public Button addToInventoryButton; // Button to add item to inventory
    public bool isInventoryFull = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player_Inventory = this.GetComponent<Player_Inventory>();
        anim = player.GetComponent<Animator>();
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

        // Make player unable to move while holding object & play catch animation
        var movement = player.GetComponent<Player_Movement>();
        var sounds = player.GetComponent<Player_SoundEffects>();
        if (movement != null) movement.enabled = false;
        if (sounds != null) sounds.enabled = false;

        anim.SetInteger("MovementPhase", 0);
        anim.SetInteger("toolUsed", 0);
        anim.SetBool("catch", true);
        anim.SetBool("holdTool", false);

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
        ShowMenu();
        // Get object's info from its Item script
        Item itemScript = obj.GetComponent<Item>();

        if (itemScript != null)
        {
            if(isInventoryFull)
            {
                UpdateMenuInfo(itemScript.itemName, itemScript.itemDescription + "\n ... my inventory is full so I can't pick it up.", itemScript.isEvil);
            }
            else
            {
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

    void HideMenu()
    {
        UIMenu.SetActive(false);
    }

    void ShowMenu()
    {
        UIMenu.SetActive(true);
    }

    public void TossObject()
    {
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

        // Add object to inventory stuff here...
        // Get Item script from object
        Item item = handBone.transform.GetChild(0).gameObject.GetComponent<Item>();
        // Add to player's inventory
        player_Inventory.AddItemToInventory(item);
    }

    public void ReburyItem()
    {
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

        // Rebury object stuff here...
        // Detach object from hand
        GameObject obj = handBone.transform.GetChild(0).gameObject;
        obj.transform.SetParent(null);

        // Rebury in nearest soil using the Shovel script mechanism...
/// TODOOOOO
    }

    void UpdateMenuInfo(string name, string description, bool isEvil)
    {
        objectName.text = "Wow! You just picked up a " + name + "!";
        objectDescription.text = description;
        if (isEvil)
        {
            objectName.text += " (Evil Item)";
        }
    }

        private IEnumerator MoveCameraToFocus()
    {
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

    private IEnumerator MoveCameraToOldPosition()
    {
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
