using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName; // Name of the item
    public string itemDescription; // Description of the item
    public Sprite itemIcon; // Icon representing the item
    public string itemType; // Type of the item ("Tool", "Food", "Head", "Torso", "Bottom", "Furniture")
    public int itemID; // Unique identifier for the item
    public int itemValue; // Gold value of the item (e.g., for selling)
    public int foodIncreaseAmount; // Amount of health restored
    public bool isTall = false; // If an object is up and down, not lengthwise
    public float positionOffset = 0.0f; // Offset to adjust item position when held
    public float rotationOffset = 0.0f; // Offset to adjust item rotation when held

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
