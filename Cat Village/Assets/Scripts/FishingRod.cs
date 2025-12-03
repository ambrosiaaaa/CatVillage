using UnityEngine;
using System.Collections;

public class FishingRod : MonoBehaviour
{
    GameObject player;
    Animator anim;
    public bool hasCasted = false;
    public bool isReelingIn = false;
    public bool waterInfront = false;
    public GameObject lurePrefab;
    public GameObject lureInstance;
    public float castDuration = 1.0f; // Duration of the cast animation
    public float arcHeight = 1.0f; // Height of the arc
    public Transform playerHand;
    public Vector3 fishingLineOffset;
    private LineRenderer fishingLine;
    public GameObject fishingRod;
    public Quaternion fishingRodOffset;
    public Vector3 fishingRodPositionOffset;
    public Quaternion originalRodRotation;
    public Vector3 originalRodPosition;
    private bool hasRecordedOriginalRotation = false;

    public bool runScript = false;

    public float multi = 0.4f;
    public Renderer rodRenderer;
    public Player_SoundEffects playerSoundEffects;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anim = player != null ? player.GetComponent<Animator>() : null;
        playerSoundEffects = player.GetComponent<Player_SoundEffects>();

        // Create and configure the fishing line
        fishingLine = gameObject.AddComponent<LineRenderer>();
        fishingLine.material = new Material(Shader.Find("Sprites/Default"));
        fishingLine.startColor = Color.white;
        fishingLine.endColor = Color.white;
        fishingLine.startWidth = 0.005f;
        fishingLine.endWidth = 0.005f;
        fishingLine.positionCount = 2;
        fishingLine.useWorldSpace = true;
        fishingLine.generateLightingData = false;
        fishingLine.enabled = false; // Initially hidden
    }

    // cast, reel in, pull fish, display catch

    // Update is called once per frame
    void Update()
    {
        if (runScript)
        {
            WaterCheck();
            UpdateFishingLine();
            GetFishingRod();

            if (hasCasted)
            {
                //playerSoundEffects.FishingRod_Cast();
                FixFishingRod();

                //TESTING
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    ReelIn();
                }
            }

            if (isReelingIn)
            {
                //TESTING
                if (Input.GetKeyDown(KeyCode.R))
                {
                    DisplayCatch();
                    Uncast();
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                HideCatch();
            }
        }
    }

    void UpdateFishingLine()
    {
        // Update fishing line position if lure exists and line is enabled
        if (fishingLine != null && fishingLine.enabled && lureInstance != null && fishingRod != null)
        {
            // Get the end of the fishing rod using local bounds and transform
            Vector3 handPosition;

            if (rodRenderer != null)
            {
                // Calculate rod length using local bounds
                float rodLength = rodRenderer.bounds.size.z; // Assuming rod extends along Z-axis
                // Use local position and transform to world space
                //Vector3 localRodTip = Vector3.forward * (rodLength * 0.5f); // Local tip position
                Vector3 localRodTip = new Vector3(0, 0, rodLength * multi); // Local tip position
                Vector3 worldRodTip = fishingRod.transform.TransformPoint(localRodTip); // Convert to world space
                handPosition = worldRodTip + fishingRod.transform.TransformDirection(fishingLineOffset);
            }
            else
            {
                handPosition = playerHand != null ? playerHand.position : player.transform.position + Vector3.up * 1.0f;
            }


            Vector3 lurePosition = lureInstance.transform.position;

            fishingLine.SetPosition(0, handPosition);
            fishingLine.SetPosition(1, lurePosition);
        }
    }

    public void WaterCheck()
    {
        if (player != null)
        {
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.5f; // Start raycast from player's mid-body height
            Vector3 rayDirection = player.transform.forward;
            float rayDistance = 0.5f; // 0.5 meters in front of player

            // Create layer mask for Water layer
            int waterLayer = LayerMask.NameToLayer("Water");
            LayerMask waterLayerMask = 1 << waterLayer;

            // Visualize the raycast in the Scene view
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 0.5f);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, waterLayerMask))
            {
                waterInfront = true;
            }
            else
            {
                waterInfront = false;
                if (hasCasted)
                {
                    Uncast();
                }
            }
        }
    }

    public void GetFishingRod()
    {
        //fishing rod is the object at the end of the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            GameObject currentRod = playerHand.GetChild(0).gameObject;

            // Check if this is a new fishing rod or the first time we're detecting it
            if (fishingRod != currentRod || !hasRecordedOriginalRotation)
            {
                fishingRod = currentRod;

                // Save the original rotation and position only once
                if (!hasRecordedOriginalRotation && fishingRod != null)
                {
                    originalRodRotation = fishingRod.transform.localRotation;
                    originalRodPosition = fishingRod.transform.localPosition;
                    hasRecordedOriginalRotation = true;

                    rodRenderer = fishingRod.transform.GetChild(0).GetComponent<Renderer>();
                }
            }
        }
        else
        {
            fishingRod = null;
            // Reset the flag when no fishing rod is present
            hasRecordedOriginalRotation = false;
        }
    }

    public void Cast()
    {
        // Cast the fishing rod
        // Debug.Log("Fishing rod cast!");
        if (waterInfront)
        {
            playerSoundEffects.FishingRod_Cast(); // kind of repeaty- will fix later
            hasCasted = true;
            CastLure();
            CastIdle();
        }
    }

    public void CastIdle()
    {
        //Debug.Log("Fishing rod cast idle!");
        if (anim != null)
        {
            anim.SetBool("castSuccess", true);
            //playerSoundEffects.FishingRod_Cast();
        }
    }

    public void ReelIn()
    {
        //Debug.Log("Reeling in the fishing rod!");
        if (anim != null)
        {
            anim.SetBool("reelIn", true);
            playerSoundEffects.FishingRod_Reel();
        }
        isReelingIn = true;
        // isCasted is true, and isReelingIn is true.
    }

    public void Uncast()
    {
        //Debug.Log("Fishing rod uncast!");
        anim.SetBool("castSuccess", false);
        anim.SetBool("reelIn", false);
        hasCasted = false;
        isReelingIn = false;
        anim.SetInteger("toolUsed", 0);
        playerSoundEffects.FishingRod_StopReel();
        UncastLure();
        if (fishingRod != null)
        {
            fishingRod.transform.localRotation = originalRodRotation;
            fishingRod.transform.localPosition = originalRodPosition;
        }
    }

    public void CastLure()
    {
        if (lureInstance == null && lurePrefab != null)
        {
            Vector3 startPosition = playerHand != null ? playerHand.position + fishingLineOffset : player.transform.position + Vector3.up * 1.0f;
            Vector3 targetPosition = player.transform.position + player.transform.forward * 1.0f; // In front of player
            Quaternion spawnRotation = Quaternion.LookRotation(player.transform.forward);

            lureInstance = Instantiate(lurePrefab, startPosition, spawnRotation);

            // Enable fishing line
            if (fishingLine != null)
            {
                fishingLine.enabled = true;
            }

            StartCoroutine(AnimateLureCast(startPosition, targetPosition));
        }
    }

    private System.Collections.IEnumerator AnimateLureCast(Vector3 startPos, Vector3 targetPos)
    {
        if (lureInstance == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < castDuration)
        {
            if (lureInstance == null) yield break; // Safety check in case lure is destroyed

            float progress = elapsedTime / castDuration;

            // Linear interpolation for horizontal movement
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);

            // Add arc height using a parabolic curve
            float arcProgress = Mathf.Sin(progress * Mathf.PI); // Creates arc from 0 to 1 back to 0
            currentPos.y += arcProgress * arcHeight;

            lureInstance.transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exactly at target
        if (lureInstance != null)
        {
            lureInstance.transform.position = targetPos;
            //play lure sound effect
            playerSoundEffects.FishingRod_Lure();
        }
    }

    public void UncastLure()
    {
        if (lureInstance != null)
        {
            Destroy(lureInstance);
            lureInstance = null;
        }

        // Hide fishing line
        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }
    }

    public void FixFishingRod()
    {
        if (fishingRod != null)
        {
            fishingRod.transform.localRotation = fishingRodOffset;
            fishingRod.transform.localPosition = originalRodPosition + fishingRodPositionOffset;
        }
    }

    public void DisplayCatch()
    {
        // Display the caught fish to the player
        //Debug.Log("Displaying caught fish!");

        anim.SetBool("reelIn", false);
        anim.SetInteger("toolUsed", 0);
        anim.SetBool("catchSuccess", true);
        anim.SetBool("holdCatch", true);
    }

    public void HideCatch()
    {
        // Hide the caught fish display
        //Debug.Log("Hiding caught fish display!");

        anim.SetBool("catchSuccess", false);
        anim.SetBool("holdCatch", false);
    }
}
