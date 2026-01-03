using UnityEngine;
using System.Collections;

public class TilledSoil : MonoBehaviour
{
    [Header("Buried Seed")]
    public GameObject buriedSeed; // The seed planted in the tilled soil
    Plant buriedSeedScript; // The Plant script of the buried seed

    [Header("Tilled Soil Prefabs")]
    public GameObject tilledSoilPrefab; // Prefab of tilled soil
    public GameObject filledSoilPrefab; // Prefab of filled soil when seed is planted

    [Header("Filled Soil Materials")]
    public Material filledSoilMaterial_default; // Material for filled soil
    public Material filledSoilMaterial_wet; // Material for filled soil

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckIfContainsSeed();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSoilMoisture();
    }

    public void UpdateSoilMoisture()
    {
        // Update the soil material based on whether the buried seed is watered
        if (buriedSeedScript != null && buriedSeedScript.currentWaterTime < buriedSeedScript.maxTimeWithoutWater)
        {
            // If the plant is watered, use wet material
            Renderer soilRenderer = GetComponentInChildren<Renderer>();
            if (soilRenderer != null && filledSoilMaterial_wet != null)
            {
                soilRenderer.material = filledSoilMaterial_wet;
            }
        }
        else
        {
            // If the plant is not watered, use default material
            Renderer soilRenderer = GetComponentInChildren<Renderer>();
            if (soilRenderer != null && filledSoilMaterial_default != null)
            {
                soilRenderer.material = filledSoilMaterial_default;
            }
        }
    }

    public void CheckIfContainsSeed()
    {
        // Check if there is a planted seed in the tilled soil
        if (buriedSeed == null)
        {
            // If no seed, spawn tilled soil prefab
            // Destroy any children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            // No object buried, spawn hole prefab
            Instantiate(tilledSoilPrefab, transform.position, Quaternion.Euler(-90, 0, 0), transform);
        }
        else // IF buried seed is not null
        {
            // If seed is planted, spawn filled soil prefab
            // Destroy any children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            // No object buried, spawn hole prefab
            Instantiate(filledSoilPrefab, transform.position, Quaternion.Euler(-90, 0, 0), transform);
            // Move buried seed to soil position
            buriedSeed.transform.position = transform.position;
            // Set plant script of the seed to be buried
            buriedSeedScript = buriedSeed.GetComponent<Plant>();
            if (buriedSeedScript != null)
            {
                buriedSeedScript.isBuried = true;
            }
        }
    }

    public void BurySeed(GameObject seed)
    {
        // Validate input object
        if (seed == null)
        {
            Debug.LogWarning($"BurySeed called with null on tilled soil '{name}'");
            return;
        }

        if (buriedSeed == null)
        {
            Debug.Log($"Burying seed '{seed.name}' on tilled soil '{name}'");
            buriedSeed = seed;
            buriedSeed.SetActive(false);
            buriedSeed.transform.position = transform.position;
            // Set plant script of the seed to be buried
            buriedSeedScript = buriedSeed.GetComponent<Plant>();
            if (buriedSeedScript != null)
            {
                buriedSeedScript.isBuried = true;
            }
        }
        else
        {
            Debug.LogWarning($"Tilled soil '{name}' already has a buried seed.");
        }
        CheckIfContainsSeed();
    }

    public GameObject DigUpSeed()
    {
        GameObject seedToReturn = buriedSeed;

        if (seedToReturn != null)
        {
            Debug.Log($"Digging up seed '{seedToReturn.name}' from tilled soil '{name}'");
            seedToReturn.SetActive(true);
            buriedSeedScript = seedToReturn.GetComponent<Plant>();
            // Reset plant script of the seed to be no longer buried
            if (buriedSeedScript != null)
            {
                buriedSeedScript.isBuried = false;
            }
            seedToReturn.transform.parent = null; // Detach from soil
            buriedSeed = null;
        }
        else
        {
            Debug.LogWarning($"No seed to dig up from tilled soil '{name}'");
        }
        CheckIfContainsSeed();
        return seedToReturn;
    }
}
