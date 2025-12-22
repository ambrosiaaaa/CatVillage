using UnityEngine;
using System.Collections;

public class Hoe : MonoBehaviour
{
    [Header("Hoe Settings")]
    bool isTSoilInfront = false;
    Transform playerHand;
    GameObject hoe;
    Quaternion originalHoeRotation;
    Vector3 originalHoePosition;
    Renderer hoeRenderer;
    bool hasRecordedOriginalRotation = false;
    public bool runScript = false;

    [Header("Player Getters")]
    GameObject player;
    Animator playerAnimator;
    public Player_SoundEffects playerSoundEffects;
    public AttackRadius attackRadiusScript;
    public CaughtObj caughtObjScript;
    public Player_Inventory player_Inventory;
    public GameObject catchTarget;

    [Header("Tilled Soil")]
    public bool isTilledSoilInfront = false;
    public bool isLandClear = false;
    public GameObject currentTilledSoil;
    public GameObject tilledSoilPrefab;
    public GameObject tilledSoilUI;
    public TMPro.TMP_Text buryPopupText;
    public GameObject burySeedPopupUI;
    public TilledSoil latestTilledSoil;
    public GameObject seedToBury;
    public GameObject seedToDigUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player != null ? player.GetComponent<Animator>() : null;
        player_Inventory = this.GetComponent<Player_Inventory>();
        playerSoundEffects = player.GetComponent<Player_SoundEffects>();
        attackRadiusScript = player.transform.Find("AttackRadius").GetComponent<AttackRadius>();

        caughtObjScript = this.GetComponent<CaughtObj>();
        tilledSoilUI.SetActive(false);
        burySeedPopupUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Run script if enabled
        if (runScript)
        {
            RunScript();
        }
        else
        {
            tilledSoilUI.SetActive(false);
            burySeedPopupUI.SetActive(false);
        }
    }

    public void RunScript()
    {
        GetHoe();
        TilledSoilCheck();
    }

    void GetHoe()
    {
        // Hoe is the object at the end of the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            GameObject currentHoe = playerHand.GetChild(0).gameObject;

            // Check if this is a new hoe or the first time we're detecting it
            if (hoe != currentHoe || !hasRecordedOriginalRotation)
            {
                hoe = currentHoe;

                // Save the original rotation and position only once
                if (!hasRecordedOriginalRotation && hoe != null)
                {
                    originalHoeRotation = hoe.transform.localRotation;
                    originalHoePosition = hoe.transform.localPosition;
                    hasRecordedOriginalRotation = true;

                    hoeRenderer = hoe.transform.GetChild(0).GetComponent<Renderer>();
                }
            }
        }
        else
        {
            hoe = null;
            // Reset the flag when no hoe is present
            hasRecordedOriginalRotation = false;
        }
    }

    void TilledSoilCheck()
    {
        // Check for tilled soil in front of the player
        if (player != null)
        {
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.3f; // Start raycast from player's mid-body height
            Vector3 rayDirection = player.transform.forward;
            float rayDistance = 0.7f; // 0.7 meters in front of player

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
            {
                if (hit.collider.CompareTag("TilledSoil"))
                {
                    // If tilled soil is detected...
                    isTilledSoilInfront = true;
                    isLandClear = false;
                    currentTilledSoil = hit.collider.gameObject;
                    latestTilledSoil = currentTilledSoil.GetComponent<TilledSoil>();
                    CheckTilledSoil(currentTilledSoil.GetComponent<TilledSoil>());
                }
            }
            else
            {
                // If no tilled soil is detected
                isTilledSoilInfront = false;
                isLandClear = true;
                currentTilledSoil = null;
                tilledSoilUI.SetActive(false);
            }
        }
    }

    public void TillSoil()
    {
        if(isLandClear && !isTilledSoilInfront)
        {
            playerSoundEffects.Shovel_Bury();
            // Instantiate tilled soil prefab at player's front position
            Vector3 spawnPosition = player.transform.position + player.transform.forward * 0.5f;
            spawnPosition.y = 0.02f; // Slightly above ground to avoid z-fighting
            Instantiate(tilledSoilPrefab, spawnPosition, Quaternion.Euler(0, 0, 0));
        }
        else if (isTilledSoilInfront && !isLandClear)
        {
            playerSoundEffects.Shovel_Bury();
            
            TilledSoil ts = currentTilledSoil.GetComponent<TilledSoil>();
            
            if (ts.buriedSeed != null)
            {
                seedToDigUp = ts.buriedSeed;
                ts.buriedSeed.SetActive(true);
                ts.buriedSeed.transform.parent = null;
                ts.buriedSeed = null;

                ts.CheckIfContainsSeed();
                StartCoroutine(MoveObjInArc(seedToDigUp, currentTilledSoil.transform.position, catchTarget.transform.position));
            }
            else
            {
                // If nothing buried, destroy the tilled soil
                Destroy(currentTilledSoil);
            }
        }
    }

    public void CheckTilledSoil(TilledSoil tilledSoil)
    {
        // When standing infront of tilled soil, show UI prompt to ask player if they want to plant a seed
        if(tilledSoil.buriedSeed == null)
        {
            // Check to make sure the player's inventory contains atleast one seed
            if (player_Inventory.CheckInventoryHasAtleastOneSeed())
            {
                // If so, show the bury seed UI prompt
                tilledSoilUI.SetActive(true);
                buryPopupText.text = "Press 'B' to bury seed";
                buryPopupText.transform.position = tilledSoil.gameObject.transform.position + Vector3.up * 0.75f;

                // If B is pressed... open player's inventory and focus on them to bury a seed from their inventory.
                if (Input.GetKeyDown(KeyCode.B))
                {
                    // Hide text
                    tilledSoilUI.SetActive(false);
                    // Open inventory, with a focus on seeds
                    player_Inventory.ShowInventory();
                    // Focus camera on the player
                    caughtObjScript.FocusCameraBuryObject();
                    // Stop player scrolling wheel
                    player_Inventory.canSwapTool = false;
                    // Show ui for burying a seed
                    burySeedPopupUI.SetActive(true);
                    player_Inventory.isBuryingSeed = true;

                    // Start coroutine to wait for player to select a seed or cancel
                    StartCoroutine(WaitForSeedBuryCancel());
                }
                else
                {
                    player_Inventory.canSwapTool = true;
                }
            }
            else
            {
                tilledSoilUI.transform.position = latestTilledSoil.transform.position + Vector3.up * 0.75f;
                tilledSoilUI.SetActive(true);
                buryPopupText.text = "I have no seeds I can plant...";
            }
        }
    }

    public void SelectSeedToBury(GameObject obj)
    {
        seedToBury = obj;
    }

    public void BurySeed()
    {
        burySeedPopupUI.SetActive(false);
        if (isTSoilInfront)
        {
            TilledSoil ts = currentTilledSoil.GetComponent<TilledSoil>();
            if(ts.buriedSeed != null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(ts));
                ts.BurySeed(ts.buriedSeed);
            }
            else if (ts.buriedSeed == null && seedToBury != null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(ts));
                ts.BurySeed(seedToBury);
                seedToBury = null;
            }
            else if (ts.buriedSeed == null)
            {
                playerSoundEffects.Shovel_Bury();
                StartCoroutine(MovePlayerToFaceHole(ts));
                Destroy(currentTilledSoil);
            }
        }
        else if (latestTilledSoil != null && latestTilledSoil.buriedSeed == null && seedToBury != null)
        {
            playerSoundEffects.Shovel_Bury();
            StartCoroutine(MovePlayerToFaceHole(latestTilledSoil));
            latestTilledSoil.BurySeed(seedToBury);
            seedToBury = null;
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
        burySeedPopupUI.SetActive(false);
        
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
        caughtObjScript.DisplayCaughtObject(seedToDigUp);
    }

    private IEnumerator MoveObjToBuryArc(GameObject log, Vector3 startPos, Vector3 endPos, TilledSoil soil)
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
        burySeedPopupUI.SetActive(false);
        
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
        soil.BurySeed(seedToBury);
        seedToBury = null; // Clear references after waiting for animation to end
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

    private IEnumerator WaitForSeedBuryCancel()
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
                tilledSoilUI.SetActive(false);
                // Allow tool swapping again
                player_Inventory.canSwapTool = true;
                waitingForCancel = false;
                player_Inventory.isBuryingSeed = false;
                caughtObjScript.HideMenu();
                burySeedPopupUI.SetActive(false);
            }
            yield return null; // Wait for next frame
        }
    }

    // Move the player to face the hole when burying
    public IEnumerator MovePlayerToFaceHole(TilledSoil soil)
    {
        Vector3 holePosition = soil.transform.position;
        Vector3 directionToHole = (holePosition - player.transform.position).normalized;
        directionToHole.y = 0; // Keep only horizontal direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToHole);

        float duration = 0.5f; // Time to rotate
        float elapsedTime = 0f;

        Quaternion initialRotation = player.transform.rotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            player.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exact
        player.transform.rotation = targetRotation;

        // Play till animation
        playerAnimator.SetInteger("toolUsed", 5);
    }
}
