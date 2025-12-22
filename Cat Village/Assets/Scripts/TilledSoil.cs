using UnityEngine;
using System.Collections;

public class TilledSoil : MonoBehaviour
{
    [Header("Buried Seed")]
    public GameObject buriedSeed; // The seed planted in the tilled soil

    [Header("Tilled Soil Prefabs")]
    public GameObject tilledSoilPrefab; // Prefab of tilled soil
    public GameObject filledSoilPrefab; // Prefab of filled soil when seed is planted

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckIfContainsSeed();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        else
        {
            // If seed is planted, spawn filled soil prefab
            // Destroy any children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            // No object buried, spawn hole prefab
            Instantiate(filledSoilPrefab, transform.position, Quaternion.Euler(-90, 0, 0), transform);
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
