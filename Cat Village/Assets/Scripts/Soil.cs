using UnityEngine;
using System.Collections;

public class Soil : MonoBehaviour
{
    public GameObject buriedObject; // The object buried in the soil

    public GameObject buriedPrefab; // Prefab of hole when covered up
    public GameObject holePrefab; // Prefab of hole when dug up

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckIfContains();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckIfContains(); this can be costly to run, just run at the end of the methods
    }

    public void CheckIfContains()
    {
        // Check if there is an object buried in the soil, spawn correct prefab
        if (buriedObject == null)
        {
            // Destroy any children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            // No object buried, spawn hole prefab
            Instantiate(holePrefab, transform.position, Quaternion.Euler(-90, 0, 0), transform);
        }
        else
        {
            // Destroy any children
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            // Object is buried, spawn covered up prefab
            Instantiate(buriedPrefab, transform.position, Quaternion.Euler(-90, 0, 0), transform);
            // Make buried object inactive and make at same position as the hole
            buriedObject.SetActive(false);
            buriedObject.transform.position = transform.position;
        }
    }

    public void BuryObject(GameObject obj)
    {
        // Validate input object
        if (obj == null)
        {
            Debug.LogWarning($"BuryObject called with null on soil '{name}'");
            return;
        }

        if (buriedObject == null)
        {
            // Only bury if there's no object already buried
            Debug.Log($"Burying object '{obj.name}' on soil '{name}'");
            buriedObject = obj;
            // Hide the object and snap it to soil position
            buriedObject.SetActive(false);
            buriedObject.transform.position = transform.position;
        }
        else
        {
            Debug.LogWarning($"Soil '{name}' already has a buried object '{buriedObject.name}'. Skipping bury.");
        }
        CheckIfContains();
    }

    public GameObject DigUpObject()
    {
        // Store reference of the buried object
        GameObject obj = buriedObject;

        if (buriedObject != null)
        {
            // Clear all references to the buried object
            buriedObject.SetActive(true); // Reveal the buried object
            buriedObject.transform.parent = null; // Unparent from soil
            buriedObject = null; // Clear reference
        }

        // Return the unearthed object
        obj.SetActive(true);

        CheckIfContains();
        return obj;
    }
}
