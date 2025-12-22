using UnityEngine;

public class Plant : MonoBehaviour
{
    public string plantName;
    public int growthStages = 6; // Array of growth stage names or identifiers
    public int currentGrowthStage;
    public int currentGrowthStageName; // Set in the editor

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // These are the plant growth stages
        // 0 = "Seed"
        // 1 = "Seedling"
        // 2 = "Immature"
        // 3 = "Mature"
        // 4 = "Flowering"
        // 5 = "Fruiting"
        // 6 = "Dying"

        currentGrowthStage = currentGrowthStageName; // Set to whatever stage is in the editor
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
