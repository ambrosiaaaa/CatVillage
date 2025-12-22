using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName; // Name of the item
    public string itemDescription; // Description of the item
    public Sprite itemIcon; // Icon representing the item
    public string itemType; // Type of the item ("Tool", "Food", "Hat", "Top", "Bottoms", "Furniture", "Material")
    public int itemValue; // Gold value of the item (e.g., for selling)
    public int foodIncreaseAmount; // Amount of health restored
    public bool isTall = false; // If an object is up and down, not lengthwise
    public float positionOffset = 0.0f; // Offset to adjust item position when held
    public float rotationOffset = 0.0f; // Offset to adjust item rotation when held
    public int toolId = 0; // Unique ID for tools, set to 0 for non-tools
    public bool isEvil = false; // Is this an evil item?

    // Clothing properties
    public Color itemColor = Color.white; // Color of the clothing item
    public Texture2D itemTexture; // Texture for the clothing item

    // Hat properties
    public Vector3 hatPositionOffset; // Offset to adjust hat position when worn
    public Quaternion hatRotationOffset; // Offset to adjust hat rotation when worn
    public Vector3 hatScale = Vector3.one; // Scale of the hat when worn

    public GameObject owner; // owner of the item (if applicable)
    public Plant plant; // Plant script attached to the item (if applicable)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plant = this.GetComponent<Plant>();
        // If there is a plant script attached to this object, update the description to include its current growht stage
        if (plant != null)
        {
            switch (plant.currentGrowthStage)
            {
                case 0:
                    itemDescription += "\nGrowth Stage: Seed";
                    break;
                case 1:
                    itemDescription += "\nGrowth Stage: Seedling";
                    break;
                case 2:
                    itemDescription += "\nGrowth Stage: Immature";
                    break;
                case 3:
                    itemDescription += "\nGrowth Stage: Mature";
                    break;
                case 4:
                    itemDescription += "\nGrowth Stage: Flowering";
                    break;
                case 5:
                    itemDescription += "\nGrowth Stage: Fruiting";
                    break;
                case 6:
                    itemDescription += "\nGrowth Stage: Dying";
                    break;
                default:
                    itemDescription += "\nGrowth Stage: (Unknown Stage)";
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
