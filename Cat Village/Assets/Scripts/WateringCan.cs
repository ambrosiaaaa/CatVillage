using System.Diagnostics.Contracts;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WateringCan : MonoBehaviour
{
    // Watering can script; Method to refill water, method to check if can is empty, make default empty (show ui of "It's empty, I need to fill this with water..."
    // Variables
    [Header("Watering Can Variables")]
    public int wateringTimes = 0; // Maximum times the watering can can be used before needing a refill
    public int maxWaterCount = 5; // Maximum water count when refilled
    public bool hasWater = false; // Does the watering can have water?
    public GameObject player;
    public Animator anim;
    public Player_SoundEffects playerSoundEffects;
    public bool waterInfront = false; // Is there water in front of the player?
    public bool runScript = false;
    public GameObject waterCanUI; // UI element to show watering can status (if any)
    [SerializeField] private ParticleSystem pourParticles;
    public GameObject wateringCan;
    public GameObject currentWateringCan;
    [Header("Player Settings")]
    public Transform playerHand;
    public Transform wateringCanSpoutOffset; // Offset from watering can position to spout
    [Header("Hoe/Tilled soil Settings")]
    public bool isTilledSoilInfront = false; // Is tilled soil infront of the player?
    public TilledSoil latestTilledSoil; // Latest tilled soil detected infront of the player
    public Plant buriedSeedInSoil; // Plant script of buried seed in the tilled soil (if any)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get player components
        player = GameObject.FindGameObjectWithTag("Player");
        anim = player != null ? player.GetComponent<Animator>() : null;
        playerSoundEffects = player.GetComponent<Player_SoundEffects>();
        if (waterCanUI != null)
        {
            waterCanUI.SetActive(false);
        }

        if (pourParticles == null)
        {
            pourParticles = GetComponentInChildren<ParticleSystem>(true);
        }

        if (pourParticles != null)
        {
            var main = pourParticles.main;
            main.playOnAwake = false;
            pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (hasWater)
        {
            wateringTimes = maxWaterCount;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(runScript)
        {
            GetWateringCan();
            CheckForWater();
            CheckForTilledSoil();
            if (pourParticles != null && wateringCanSpoutOffset != null)
            {
                FollowSpout();
            }
        }
    }

    void GetWateringCan()
    {
        // Axe is the object at the end of the player's hand
        if (playerHand != null && playerHand.childCount > 0)
        {
            GameObject currentWateringCan = playerHand.GetChild(0).gameObject;

            // Check if this is a new axe or the first time we're detecting it
            if (wateringCan != currentWateringCan)
            {
                wateringCan = currentWateringCan;
                wateringCanSpoutOffset = wateringCan.transform.Find("SpoutOffset");
            }
        }
        else
        {
            wateringCan = null;
            wateringCanSpoutOffset = null;
        }
    }

    public void CheckForWater()
    {
        // Raycast infront of the player to check for bodies of water
        if (player != null)
        {
            Vector3 rayOrigin = player.transform.position + Vector3.up * 0.5f; // Start raycast from player's mid-body height
            Vector3 rayDirection = player.transform.forward;
            float rayDistance = 0.5f; // 0.5 meters in front of player

            // Create layer mask for Water layer
            int waterLayer = LayerMask.NameToLayer("Water");
            LayerMask waterLayerMask = 1 << waterLayer;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, waterLayerMask))
            {
                waterInfront = true;
            }
            else
            {
                waterInfront = false;
            }
        }
    }

    public void CheckForTilledSoil()
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
                    latestTilledSoil = hit.collider.GetComponent<TilledSoil>();
                    // Get seed from the tilled soil (if any)
                    GameObject buriedSeed = latestTilledSoil != null ? latestTilledSoil.buriedSeed : null;
                    buriedSeedInSoil = buriedSeed != null ? buriedSeed.GetComponent<Plant>() : null;
                }
            }
            else
            {
                // If no tilled soil is detected
                isTilledSoilInfront = false;
                latestTilledSoil = null;
                buriedSeedInSoil = null;
            }
        }
    }

    public void RefillWaterOrPourWater()
    {
        // Can only refill if; a) there is water infront of the player, and b) the watering can is not already full
        if (waterInfront && wateringTimes < maxWaterCount)
        {
            wateringTimes = maxWaterCount;
            hasWater = true;
            // Play refill sound
            playerSoundEffects.WateringCan_Refill();
            // Play animation
            anim.SetInteger("toolUsed", 6);
        }
        else if (!waterInfront)
        {
            // Pour water if there is no water infront and the can has water
            if (hasWater && wateringTimes > 0)
            {
                // Check if soil in front
                if (isTilledSoilInfront && latestTilledSoil != null)
                {
                    // Water the plant if there is a buried seed in the soil
                    if (buriedSeedInSoil != null)
                    {
                        buriedSeedInSoil.currentWaterTime = 0f; // Reset water timer
                    }
                }
                wateringTimes--;
                // Play pour sound and animation
                playerSoundEffects.WateringCan_Pour();
                anim.SetInteger("toolUsed", 6);

                if (pourParticles != null)
                {
                    // Make partciles appear at watering can spout
                    // pourParticles.transform.position = wateringCanSpoutOffset.position;
                    // pourParticles.transform.rotation = wateringCanSpoutOffset.rotation;
                    if (!pourParticles.isPlaying)
                    {
                        pourParticles.Play();
                    }
                    StartCoroutine(StopParticleAfterDelay(1.2f));
                }

                // Change this to check if plant is infront and water that plant
                // For now, just reduce watering times

                if (wateringTimes <= 0)
                {
                    hasWater = false;
                    wateringTimes = 0;
                }
            }
            else
            {
                // Show UI message "This watering can is empty, I need to refill it with water..."
                if (waterCanUI != null)
                {
                    waterCanUI.transform.position = player.transform.position + Vector3.up * 0.75f;
                    waterCanUI.SetActive(true);
                    StartCoroutine(HideWaterCanUIAfterDelay(1.5f));
                }
            }
        }
    }

    public void FollowSpout()
    {
        pourParticles.transform.position = wateringCanSpoutOffset.position;
        pourParticles.transform.rotation = wateringCanSpoutOffset.rotation;
    }

    // waterCanUI ienumerator
    public IEnumerator HideWaterCanUIAfterDelay(float delay)
    {
        waterCanUI.transform.position = player.transform.position + Vector3.up * 0.75f;
                    waterCanUI.SetActive(true);
        yield return new WaitForSeconds(delay);
        if (waterCanUI != null)
        {
            //fade the ui element next...
            waterCanUI.SetActive(false);
        }
    }

    public IEnumerator StopParticleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pourParticles != null)
        {
            if (pourParticles.isPlaying)
            {
                pourParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
