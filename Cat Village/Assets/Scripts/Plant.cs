using UnityEngine;

public class Plant : MonoBehaviour
{
    public string plantName;
    public int growthStages = 6; // Array of growth stage names or identifiers
    public int currentGrowthStage;
    public int currentGrowthStageName; // Set in the editor
    
    public bool isBuried = false; // Is the plant buried in soil?
    public float currentWaterTime = 0f; // Current time since last watered
    public float maxTimeWithoutWater = 300f; // Max time the plant can go without water (in seconds)

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
        if (!isBuried)
        {
            currentWaterTime = maxTimeWithoutWater; // Start not watered if unburied
        }
    }

    // Update is called once per frame
    void Update()
    {
        WaterTimer();
    }

    public void WaterTimer()
    {
        // This is the length of time the plant has went without water, default should be maximum time.
        currentWaterTime += Time.deltaTime;
    }
}
