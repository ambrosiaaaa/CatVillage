using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Axe : MonoBehaviour
{
    GameObject player;
    Animator playerAnimator;
    bool isTreeInfront = false;
    GameObject currentTree;
    Transform playerHand;
    GameObject axe;
    Quaternion originalAxeRotation;
    Vector3 originalAxePosition;
    Renderer axeRenderer;
    bool hasRecordedOriginalRotation = false;
    public bool runScript = false;
    public GameObject treeHealthUI; // UI element to show tree health (if any)

    public Player_SoundEffects playerSoundEffects;
    public AttackRadius attackRadiusScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player != null ? player.GetComponent<Animator>() : null;
        treeHealthUI.SetActive(false);

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
        GetAxe();
        TreeCheck();
    }

    void TreeCheck()
    {
        // Check for a tree in front of the player
        if (player != null)
        {
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.5f; // Start raycast from player's mid-body height
            Vector3 rayDirection = player.transform.forward;
            float rayDistance = 0.5f; // 0.5 meters in front of player

            // Visualize the raycast in the Scene view
            //Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 0.5f);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
            {
                // Check if the hit object's name contains "Tree"
                if (hit.collider.gameObject.name.Contains("Tree"))
                {
                    // Tree found in front of player
                    //Debug.Log("Tree detected: " + hit.collider.gameObject.name);
                    isTreeInfront = true;
                    currentTree = hit.collider.gameObject;
                    //get the tree's health to display on ui
                    Tree treeComponent = currentTree.GetComponent<Tree>();
                    
                    treeHealthUI.transform.position = hit.collider.transform.position;
                    //get child of treeHealthUI with textmeshpro component and set text to tree health
                    TextMeshProUGUI treeHealthText = treeHealthUI.GetComponentInChildren<TextMeshProUGUI>();
                    treeHealthText.text = "Hits remaining: " +treeComponent.hits.ToString() + "\n Regenerates after " + treeComponent.remainingTime.ToString("F1") + "s";
                    if(treeComponent.hits > 0)
                    {
                        treeHealthUI.SetActive(true);
                    }
                    else
                    {
                        treeHealthUI.SetActive(false);  
                    }
                }
                else
                {
                    isTreeInfront = false;
                    currentTree = null;
                    treeHealthUI.SetActive(false);
                }
            }
            else
            {
                isTreeInfront = false;
                currentTree = null;
                treeHealthUI.SetActive(false);
            }
        }
    }

    void GetAxe()
    {
        // Axe is the object at the end of the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            GameObject currentAxe = playerHand.GetChild(0).gameObject;

            // Check if this is a new axe or the first time we're detecting it
            if (axe != currentAxe || !hasRecordedOriginalRotation)
            {
                axe = currentAxe;

                // Save the original rotation and position only once
                if (!hasRecordedOriginalRotation && axe != null)
                {
                    originalAxeRotation = axe.transform.localRotation;
                    originalAxePosition = axe.transform.localPosition;
                    hasRecordedOriginalRotation = true;

                    axeRenderer = axe.transform.GetChild(0).GetComponent<Renderer>();
                }
            }
        }
        else
        {
            axe = null;
            // Reset the flag when no axe is present
            hasRecordedOriginalRotation = false;
        }
    }

    void HitTree()
    {
        if (isTreeInfront)
        {
            // Play axe swing animation
            if (playerAnimator != null)
            {
                //playerAnimator.SetTrigger("AxeSwing");
            }
        }
    }

    public void AxeStrike()
    {
        if (isTreeInfront)
        {
            playerSoundEffects.Axe_HitTree();
            // Logic for when the axe actually strikes the tree
            //Debug.Log("Axe struck the tree!");
            // Get tree's Tree component and call TakeHit
            Tree treeComponent = currentTree.GetComponent<Tree>();
            if (treeComponent != null)
            {
                treeComponent.TakeHit();
            }
        }
        else if (attackRadiusScript.struckNPC)
        {
            playerSoundEffects.Axe_HitFlesh();
        }
        else
        {
            playerSoundEffects.Axe_Miss();
        }
    }
}
