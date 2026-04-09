using System.Collections;
using UnityEngine;

public class Net : MonoBehaviour
{
    // Variables
    GameObject player;
    Animator anim;
    public Transform playerHand;
    public float castDuration = 1.0f; // Duration of the cast animation
    public GameObject net;
    public bool runScript = false; // Flag to control script execution
    public Renderer netRenderer;
    public Player_SoundEffects playerSoundEffects;
    public bool hasCasted = false;
    public Player_Inventory pi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // First get the player and components
        player = GameObject.FindGameObjectWithTag("Player");
        anim = player != null ? player.GetComponent<Animator>() : null;
        playerSoundEffects = player != null ? player.GetComponent<Player_SoundEffects>() : null;
        pi = GameObject.Find("Game Manager").GetComponent<Player_Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (runScript)
        {
            // Get net, update if casted net.
            GetNet();
        }
    }

    void GetNet()
    {
        // Get net at the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            // Set currentNet to the first child of playerHand
            GameObject currentNet = playerHand.GetChild(0).gameObject;

            // Check if this is a new net or the first time we have detected this net
            if (net != currentNet)
            {
                net = currentNet; // Update the net reference
                netRenderer = net.transform.GetChild(0).GetComponent<Renderer>(); // Get the renderer
            }
        }
        else
        {
            // Set net to null if no net detected
            net = null;
        }
    }

    public void CastNet()
    {
        playerSoundEffects.FishingRod_Cast(); // Play cast sound effect, put fishing rod for testing
        hasCasted = true;
        Debug.Log("Net script ran!");
        /*if(anim != null)
        {
            anim.SetBool("");
        }*/
        BugCheck();
    }

    void BugCheck()
    {
        // Method to check if a insect is in front of the player or not
        if (player != null)
        {
            // Check if the player is looking at an insect
            RaycastHit hit;
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.5f;
            Vector3 rayDirection = player.transform.forward;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, 1.0f))
            {
                if (hit.collider.CompareTag("Insect"))
                {
                    // If looking at an insect, be able to catch that insect
                    Debug.Log("Caught insect!");
                    // Get the object the ray hit
                    GameObject bugHit = hit.transform.GetChild(0).GetComponent<Collider>().gameObject;
                    // Get the item script from the bugHit
                    if (bugHit != null)
                    {
                        // Display catch animation
                        DisplayCatch(bugHit);
                        /*
                        Item bugitemScript = bugHit.transform.GetChild(0).GetComponent<Item>();
                        // Add bug to inventory
                        if (bugitemScript != null)
                        {
                            Player_Inventory playerInventory = GameObject.Find("Game Manager").GetComponent<Player_Inventory>();
                            playerInventory.AddItemToInventory(bugitemScript);
                        }*/
                    }
                }
            }
        }
    }

    public void DisplayCatch(GameObject bug)
    {
        Debug.Log("Displaying catch!");
        // Stop player movement
        player.GetComponent<Player_Movement>().enabled = false;
        // Rotate player to face camera
        player.transform.rotation = Quaternion.Euler(0, 180, 0);
        // Hide net temporarily
        if (netRenderer != null)
        {
            netRenderer.enabled = false;
        }
        anim.SetBool("holdTool", false);
        anim.SetBool("catch", true);
        anim.SetBool("catchSuccess", true);
        anim.SetBool("holdCatch", true);
        // Get ai script of bug, stop it
        bug.transform.parent.GetComponent<NPC_MovementAlgorithm>().enabled = false;
        // Move caught bug to infront of the player
        bug.transform.position = player.transform.position + (Vector3.up * 0.5f) - (Vector3.forward * 0.2f);
        bug.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void HideCatch()
    {
        anim.SetBool("catch", false);
        anim.SetBool("catchSuccess", false);
        anim.SetBool("holdCatch", false);
        anim.SetBool("holdTool", true);
        // Add bug to inventory

        if (netRenderer != null)
        {
            netRenderer.enabled = true;
        }
    }
}
