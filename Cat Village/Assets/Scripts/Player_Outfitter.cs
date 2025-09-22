using UnityEngine;
using System.Collections;

public class Player_Outfitter : MonoBehaviour
{
    // Attach this script to a NPC cat or player to manage their outfit's top and bottoms
    // Player's material
    public Material playerMaterial;

    // Load in outfit's top texture
    public Texture2D outfitTopTexture;

    // Load in outfit's bottom texture
    public Texture2D outfitBottomTexture;

    // Texture for no texture
    public Texture2D noTexture;

    // Colour the bottom and top
    public Color topColor = Color.white;
    public Color bottomColor = Color.white;

    // Reference to the player's head bone
    public Transform playerHeadBone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start coroutine to wait before getting the material
        StartCoroutine(InitializeMaterialAfterDelay());

        // Get head bone from character's children, it is named head
        if (playerHeadBone == null)
        {
            Transform headTransform = transform.Find("Armature_Cat/Root/hip/body/head");
            if (headTransform != null)
            {
                playerHeadBone = headTransform;
                Debug.Log("Head bone found and assigned.");
            }
            else
            {
                Debug.LogWarning("Head bone not found in hierarchy.");
            }
        }
    }

    // Coroutine to wait a few seconds before getting the Cat's renderer and material
    IEnumerator InitializeMaterialAfterDelay()
    {
        // Wait 3 seconds for the cat's material to be loaded and attached
        yield return new WaitForSeconds(0.1f);

        // Find the Cat child object and get its renderer material
        Transform catChild = transform.Find("Cat");
        if (catChild != null)
        {
            Renderer catRenderer = catChild.GetComponent<Renderer>();
            if (catRenderer != null)
            {
                playerMaterial = catRenderer.sharedMaterial;
                Debug.Log("Player material initialized from Cat renderer.");
            }
            else
            {
                Debug.LogError("Cat child object does not have a Renderer component!");
            }
        }
        else
        {
            Debug.LogError("Cat child object not found!");
        }

        // Check clothes
        if (outfitTopTexture == null)
        {
            outfitTopTexture = noTexture;
        }

        if (outfitBottomTexture == null)
        {
            outfitBottomTexture = noTexture;
        }

        // Apply initial textures and colors to the material if it's already assigned
        if (playerMaterial != null)
        {
            playerMaterial.SetTexture("_ClothingTop", outfitTopTexture);
            playerMaterial.SetTexture("_ClothingBottom", outfitBottomTexture);
            playerMaterial.SetColor("_ClothingTopColour", topColor);
            playerMaterial.SetColor("_ClothingBottomColour", bottomColor);
        }
        else
        {
            Debug.LogWarning("Player material is not assigned at Start.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Methods for changing top, bottom
    public void ChangeOutfitTop(Texture2D newTopTexture)
    {
        if (newTopTexture == null)
        {
            Debug.LogWarning("New top texture is null. Please provide a valid texture.");
            return;
        }
        outfitTopTexture = newTopTexture;
        playerMaterial.SetTexture("_ClothingTop", outfitTopTexture);
    }

    public void ChangeOutfitBottom(Texture2D newBottomTexture)
    {
        if (newBottomTexture == null)
        {
            Debug.LogWarning("New bottom texture is null. Please provide a valid texture.");
            return;
        }
        outfitBottomTexture = newBottomTexture;
        playerMaterial.SetTexture("_ClothingBottom", outfitBottomTexture);
    }

    // Methods for removing top, bottom
    public void RemoveOutfitTop()
    {
        outfitTopTexture = noTexture;
        playerMaterial.SetTexture("_ClothingTop", outfitTopTexture);
    }
    public void RemoveOutfitBottom()
    {
        outfitBottomTexture = noTexture;
        playerMaterial.SetTexture("_ClothingBottom", outfitBottomTexture);
    }

    // Recolour the top and bottom
    public void RecolorOutfitTop(Color newColor)
    {
        topColor = newColor;
        playerMaterial.SetColor("_ClothingTopColour", topColor);
    }

    public void RecolorOutfitBottom(Color newColor)
    {
        bottomColor = newColor;
        playerMaterial.SetColor("_ClothingBottomColour", bottomColor);
    }

    public void WearHat(Item hatItem)
    {
        if (hatItem.gameObject == null)
        {
            Debug.LogWarning("Hat prefab is null. Please provide a valid hat prefab.");
            return;
        }

        if (playerHeadBone == null)
        {
            Debug.LogError("Player head bone is not assigned. Cannot wear hat.");
            return;
        }
        // Remove any existing hat first
        foreach (Transform child in playerHeadBone)
        {
            Destroy(child.gameObject);
        }

        // Instantiate the hat prefab
        GameObject hatPrefab = hatItem.gameObject;
        // hatPrefab.SetActive(true);

        // Instantiate the hat and parent it to the head bone
        GameObject hatInstance = Instantiate(hatPrefab, playerHeadBone);

        // Disable all colliders on the hat (including child objects)
        Collider[] hatColliders = hatInstance.GetComponentsInChildren<Collider>();
        foreach (Collider collider in hatColliders)
        {
            collider.enabled = false;
        }
        hatInstance.SetActive(true);
        hatInstance.transform.localPosition = Vector3.zero + hatItem.hatPositionOffset; // Adjust as needed
        hatInstance.transform.localRotation = Quaternion.identity * hatItem.hatRotationOffset; // Adjust as needed
        hatInstance.transform.localScale = hatItem.hatScale; // Adjust scale if necessary
    }

    public void RemoveHat()
    {
        if (playerHeadBone == null)
        {
            Debug.LogError("Player head bone is not assigned. Cannot remove hat.");
            return;
        }

        // Destroy all children of the head bone (assuming only the hat is a child)
        foreach (Transform child in playerHeadBone)
        {
            Destroy(child.gameObject);
        }
    }
}
