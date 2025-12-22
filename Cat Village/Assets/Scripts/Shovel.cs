using UnityEngine;
using System.Collections;
using TMPro;

public class Shovel : MonoBehaviour
{

    GameObject player;
    Animator playerAnimator;
    public bool isSoilInfront = false;
    public bool landClear = false;
    GameObject currentSoil;
    Transform playerHand;
    GameObject shovel;
    Quaternion originalShovelRotation;
    Vector3 originalShovelPosition;
    Renderer shovelRenderer;
    bool hasRecordedOriginalRotation = false;
    public bool runScript = false;

    public Player_SoundEffects playerSoundEffects;
    public AttackRadius attackRadiusScript;
    public Soil soilScript;
    public CaughtObj caughtObjScript;
    public Player_Inventory player_Inventory;

    public GameObject holePrefab; // Prefab of hole when dug up
    public GameObject dugUpObject;
    public GameObject catchTarget;
    public Soil latestSoil;

    //
    public GameObject objToBury;
    public GameObject buryPopupUI;
    public TMPro.TMP_Text buryPopupText;
    public GameObject buryItemUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        caughtObjScript = this.GetComponent<CaughtObj>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player != null ? player.GetComponent<Animator>() : null;
        playerSoundEffects = player.GetComponent<Player_SoundEffects>();
        attackRadiusScript = player.transform.Find("AttackRadius").GetComponent<AttackRadius>();
        player_Inventory = this.GetComponent<Player_Inventory>();

        // Hide bury text first
        buryPopupUI.SetActive(false);
        buryItemUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (runScript)
        {
            RunScript();
        }
        else
        {
            // If not running, hide everything
            buryPopupUI.SetActive(false);
            buryItemUI.SetActive(false);
            // Note: Do NOT clear latestSoil or objToBury here.
            // Reburial flows (e.g., CaughtObj.ReburyItem) may set objToBury and then call BuryHole()
            // while the shovel script is inactive. Clearing these values here causes race conditions
            // and prevents BuryHole from seeing the selected object/soil.
        }
    }

    public void RunScript()
    {
        GetShovel();
        if(!player_Inventory.isBuryingItem)
        {
            SoilCheck();
        }
    }

    void SoilCheck()
    {
        // Check for a tree in front of the player
        if (player != null)
        {
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.3f; // Start raycast from player's mid-body height
            Vector3 rayDirection = player.transform.forward;
            float rayDistance = 0.7f; // 0.7 meters in front of player

            // Visualize the raycast in the Scene view
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 0.5f);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
            {
                // Check if the hit object's name contains "Soil"
                if (hit.collider.gameObject.tag.Equals("Soil"))
                {
                    // Soil found in front of player
                    //Debug.Log("Soil detected: " + hit.collider.gameObject.name);
                    isSoilInfront = true;
                    landClear = false;
                    currentSoil = hit.collider.gameObject;
                    soilScript = currentSoil.GetComponent<Soil>(); // Get soil script from lump of soil
                    latestSoil = soilScript;

                    CheckHole(soilScript);
                }
                else
                {
                    // Any other object in front of the player
                    //Debug.Log("Non-soil detected: " + hit.collider.gameObject.name);
                    landClear = false;
                    isSoilInfront = false;
                    currentSoil = null;
                    // Hide bury popup if land is clear
                    buryPopupUI.SetActive(false);
                }
            }
            else
            {
                //Debug.Log("Land clear!");
                landClear = true;
                isSoilInfront = false;
                currentSoil = null;

                // Hide bury popup if land is clear
                buryPopupUI.SetActive(false);
            }
        }
    }

    public void CheckHole(Soil soil)
    {
        // When standing infront of soil, display the text UI pop up to ask if player wants to bury an item
        if (soil.buriedObject == null)
        {
            // If hole is empty...
            if (player_Inventory.CheckInventoryHasAtleastOneItem())
            {
                // If player has atleast one item to bury...
                buryPopupUI.SetActive(true);
                buryPopupText.transform.position = soil.gameObject.transform.position + Vector3.up * 0.75f; // Position text above soil
                buryPopupText.text = "Press B to bury an item";

                //if B pressed, open inventory to select item to bury
                if(Input.GetKeyDown(KeyCode.B))
                {
                    // Hide text
                    buryPopupUI.SetActive(false);
                    // Open inventory
                    player_Inventory.ShowInventory();
                    // Focus camera
                    caughtObjScript.FocusCameraBuryObject();
                    // BURY ITEM LOGIC...
                    // stop scroll wheelling, and use latestSoil to bury selected item
                    player_Inventory.canSwapTool = false;
                    buryItemUI.SetActive(true);
                    player_Inventory.isBuryingItem = true;
                    // If player presses TAB or ESC, close inventory and cancel burying
                    StartCoroutine(WaitForBuryCancel());
                }
                else {
                    // Allow tool swapping again
                    player_Inventory.canSwapTool = true;
                }
            }
            else {
                // If no items to bury, display alternative message
                buryPopupUI.SetActive(true);
                buryPopupText.text = "I have no items I can bury...";
                buryPopupUI.transform.position = latestSoil.transform.position + Vector3.up * 0.75f;
            }
        }
    }

    void GetShovel()
    {
        // Shovel is the object at the end of the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            GameObject currentShovel = playerHand.GetChild(0).gameObject;

            // Check if this is a new shovel or the first time we're detecting it
            if (shovel != currentShovel || !hasRecordedOriginalRotation)
            {
                shovel = currentShovel;

                // Save the original rotation and position only once
                if (!hasRecordedOriginalRotation && shovel != null)
                {
                    originalShovelRotation = shovel.transform.localRotation;
                    originalShovelPosition = shovel.transform.localPosition;
                    hasRecordedOriginalRotation = true;

                    shovelRenderer = shovel.transform.GetChild(0).GetComponent<Renderer>();
                }
            }
        }
        else
        {
            shovel = null;
            // Reset the flag when no shovel is present
            hasRecordedOriginalRotation = false;
        }
    }

    public void DigHole()
    {
        // If land infront is clear, the player can dig a hole 0.5m infront of themself
        if (landClear && !isSoilInfront)
        {
            // Play dig sound effect here...
            playerSoundEffects.Shovel_Dig();
            // Instantiate hole prefab infront of player
            Vector3 holePosition = player.transform.position + player.transform.forward * 0.5f;
            holePosition.y = 0.02f;
            Instantiate(holePrefab, holePosition, Quaternion.Euler(0, 0, 0));
        }
        else if (isSoilInfront && !landClear)
        {
            // Play dig sound effect here...
            playerSoundEffects.Shovel_Dig();
            // If soil is infront of player, they can dig up the buried object
            // Make the soil infront disappear
            //Debug.Log("Dug up: " + currentSoil);
            //check if soil has buried object or not
            Soil sl = currentSoil.GetComponent<Soil>();

            if(sl.buriedObject != null)
            {
                dugUpObject = sl.buriedObject;
                sl.buriedObject.SetActive(true);
                sl.buriedObject.transform.parent = null;
                sl.buriedObject = null;
                
                //Debug.Log("Dug up object: " + dugUpObject.name);
                
                // Update soil to show empty hole
                sl.CheckIfContains();

                // Deactivate object's collider
                dugUpObject.GetComponent<Collider>().enabled = false;

                //play dig sound effect here too...

                // Move log towards the player before doing caught object routine
                StartCoroutine(MoveObjInArc(dugUpObject, currentSoil.transform.position, catchTarget.transform.position));
            }
            else
            {
                Destroy(currentSoil);
                //turn this into filling hole
            }
        }
    }

    public void SelectObjectToBury(GameObject obj)
    {
        objToBury = obj;
        //Debug.Log("Selected object to bury: " + objToBury.name);
    }

    public void BuryHole()
    {
        // If soil is infront of player, they can bury the hole
        buryPopupUI.SetActive(false);  // Hide popup UI
        if (isSoilInfront)
        {
            // Play sound effect...
            //playerSoundEffects.Shovel_Bury();
            // Get currentSoil
            Soil sl = currentSoil.GetComponent<Soil>();
            // If there is an object to bury, bury it and keep as hole
            if (sl.buriedObject != null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(sl));
                sl.BuryObject(sl.buriedObject);
                playerAnimator.SetBool("isBury", false);
            }
            // If there is no object to bury and we are not reburying...
            else if (sl.buriedObject == null && objToBury != null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(sl));
                StartCoroutine(MoveObjToBuryArc(objToBury, catchTarget.transform.position, sl.transform.position, sl));
                playerAnimator.SetBool("isBury", false);
            }
            else if (sl.buriedObject == null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(sl));
                Destroy(currentSoil);
                playerAnimator.SetBool("isBury", false);
            }
        }
        // If there is a hole we just dug up that is empty...
        else if (latestSoil != null && latestSoil.buriedObject == null && objToBury != null)
        {
            // Play sound effect...
            playerSoundEffects.Shovel_Bury();
            StartCoroutine(MovePlayerToFaceHole(latestSoil));
            StartCoroutine(MoveObjToBuryArc(objToBury, catchTarget.transform.position, latestSoil.transform.position, latestSoil));
            playerAnimator.SetBool("isBury", false);
        }
    }  

    private IEnumerator WaitForBuryCancel()
    {
        bool waitingForCancel = true;

        while (waitingForCancel)
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                // Close inventory
                player_Inventory.HideInventory();
                // Unfocus camera
                StartCoroutine(caughtObjScript.MoveCameraToOldPosition());
                // Hide bury item UI
                buryItemUI.SetActive(false);
                // Allow tool swapping again
                player_Inventory.canSwapTool = true;
                waitingForCancel = false;
                player_Inventory.isBuryingItem = false;
            }
            yield return null; // Wait for next frame
        }
    }

    private IEnumerator MoveObjInArc(GameObject log, Vector3 startPos, Vector3 endPos)
    {
        float duration = 1f; // Time for the arc movement
        float elapsedTime = 0f;
        
        // Calculate arc height (midpoint will be higher)
        float arcHeight = 1.5f;
        Vector3 midPoint = (startPos + endPos) / 2f;
        midPoint.y += arcHeight;

        // Hide burial UI
        buryPopupUI.SetActive(false);
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            // Use quadratic bezier curve for smooth arc
            Vector3 currentPos = CalculateBezierPoint(t, startPos, midPoint, endPos);
            log.transform.position = currentPos;
            
            // Add some rotation during flight
            log.transform.Rotate(Vector3.right * (180f * Time.deltaTime));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position is exact
        log.transform.position = endPos;

        // Wait a moment to ensure position is set, then enable physics
        yield return new WaitForFixedUpdate();
        
        // Force position again after physics update
        log.transform.position = endPos;

        // NOW display object
        caughtObjScript.DisplayCaughtObject(dugUpObject);
    }

    private IEnumerator MoveObjToBuryArc(GameObject log, Vector3 startPos, Vector3 endPos, Soil soil)
    {
        log.SetActive(true); // Ensure object is active for movement
        // Disable all collisions on the log during flight
        var logColliders = log.GetComponentsInChildren<Collider>();
        foreach (var col in logColliders)
        {
            if (col != null) col.enabled = false;
        }

        float duration = 1f; // Time for the arc movement
        float elapsedTime = 0f;
        
        // Calculate arc height (midpoint will be higher)
        float arcHeight = 1.5f;
        Vector3 midPoint = (startPos + endPos) / 2f;
        midPoint.y += arcHeight;

        // Hide burial UI
        buryPopupUI.SetActive(false);
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            // Use quadratic bezier curve for smooth arc
            Vector3 currentPos = CalculateBezierPoint(t, startPos, midPoint, endPos);
            log.transform.position = currentPos;
            
            // Add some rotation during flight
            log.transform.Rotate(Vector3.right * (180f * Time.deltaTime));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position is exact
        log.transform.position = endPos;
        

        // Wait a moment to ensure position is set, then enable physics
        yield return new WaitForFixedUpdate();
        
        // Force position again after physics update
        log.transform.position = endPos;
        // Re-enable collisions on the log now that the arc is complete
        foreach (var col in logColliders)
        {
            if (col != null) col.enabled = true;
        }
        soil.BuryObject(objToBury);
        objToBury = null; // Clear references after waiting for animation to end
        // Do not null out local soil reference
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Quadratic Bezier curve formula
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2f * u * t * p1; // 2(1-t)t * P1
        p += tt * p2; // t^2 * P2
        
        return p;
    }

    // Move the player to face the hole when burying
    public IEnumerator MovePlayerToFaceHole(Soil soil)
    {
        Vector3 holePosition = soil.transform.position;
        Vector3 directionToHole = (holePosition - player.transform.position).normalized;
        directionToHole.y = 0; // Keep only horizontal direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToHole);

        float duration = 0.5f; // Time to rotate
        float elapsedTime = 0f;

        Quaternion initialRotation = player.transform.rotation;

        // Begin bury animation state
        playerAnimator.SetBool("isBury", true);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            player.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exact
        player.transform.rotation = targetRotation;

        // Play bury animation
        //playerAnimator.SetBool("Bury", true);
        playerAnimator.SetInteger("toolUsed", 4);
        // End bury animation state
        playerAnimator.SetBool("isBury", false);
    }
}
