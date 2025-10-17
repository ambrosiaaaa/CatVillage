using UnityEngine;

public class NPC : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
        }
    }
}
