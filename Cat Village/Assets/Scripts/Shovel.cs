using UnityEngine;
using System.Collections;

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

    public GameObject holePrefab; // Prefab of hole when dug up
    public GameObject dugUpObject;
    public GameObject catchTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        caughtObjScript = this.GetComponent<CaughtObj>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player != null ? player.GetComponent<Animator>() : null;
        playerSoundEffects = player.GetComponent<Player_SoundEffects>();
        attackRadiusScript = player.transform.Find("AttackRadius").GetComponent<AttackRadius>();
    }

    // Update is called once per frame
    void Update()
    {
        if (runScript)
        {
            RunScript();
        }
    }

    public void RunScript()
    {
        GetShovel();
        SoilCheck();
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
                }
                else
                {
                    // Any other object in front of the player
                    //Debug.Log("Non-soil detected: " + hit.collider.gameObject.name);
                    landClear = false;
                    isSoilInfront = false;
                    currentSoil = null;
                }
            }
            else
            {
                //Debug.Log("Land clear!");
                landClear = true;
                isSoilInfront = false;
                currentSoil = null;
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

   public void DigUp()
    {
        
    }

    public void Bury()
    {
        
    }

    public void DigHole()
    {
        // If land infront is clear, the player can dig a hole 0.5m infront of themself
        if (landClear && !isSoilInfront)
        {
            //play dig sound effect here too...
            Vector3 holePosition = player.transform.position + player.transform.forward * 0.5f;
            holePosition.y = 0.02f;
            Instantiate(holePrefab, holePosition, Quaternion.Euler(0, 0, 0));
        }
        else if (isSoilInfront && !landClear)
        {
            // if soil is infront of player, they can dig up the buried object
            // make the soil infront disappear
            Debug.Log("Dug up: " + currentSoil);
            //check if soil has buried object or not
            Soil sl = currentSoil.GetComponent<Soil>();

            if(sl.buriedObject != null)
            {
                dugUpObject = sl.buriedObject;
                sl.buriedObject.SetActive(true);
                sl.buriedObject.transform.parent = null;
                sl.buriedObject = null;
                
                Debug.Log("Dug up object: " + dugUpObject.name);
                
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

    public void BuryHole()
    {
        // If soil is infront of player, they can bury the hole
        if (isSoilInfront)
        {
            //currentSoil

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
}
