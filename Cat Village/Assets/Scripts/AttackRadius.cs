using UnityEngine;

public class AttackRadius : MonoBehaviour
{
    // Game manager for player attacks
    public GameObject gameManager;
    public GameObject character; // Either player or NPC using this attack radius
    public bool isPlayer = false; // Is this the player's attack radius?
    public int damageToCause = 0;
    public bool struckNPC = false;

    // Player's health script

    // NPC's health script
    public NPC npc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager");

        if (character == null)
        {
            character = transform.parent.gameObject;

            if (character.CompareTag("Player"))
            {
                isPlayer = true;
            }
            else
            {
                isPlayer = false;
            }
        }

        // Add a trigger collider if it doesn't exist
        if (GetComponent<Collider>() == null)
        {
            BoxCollider triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = Vector3.one; // Adjust size as needed
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isPlayer)
        {
            // Player's attack radius - detect NPCs continuously
            if (other.CompareTag("NPC"))
            {
                Debug.Log("NPC is in player's attack range: " + other.name);
                npc = other.GetComponent<NPC>();

                DamageNPC(npc, damageToCause); // Example damage value
                struckNPC = true;
            }
        }
        else
        {
            // NPC's attack radius - detect Player continuously
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player is in NPC's attack range");
                // This will run every frame while the player is in range
                // You can add logic here for NPC attacking player
                // For example: gameManager.GetComponent<GameManager>().NPCTargetInRange(character, other.gameObject);
                struckNPC = false;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (isPlayer)
        {
            // Player's attack radius - NPC left
            if (other.CompareTag("NPC"))
            {
                Debug.Log("NPC left player's attack range: " + other.name);
                //Return bool that a NPC has been struck
                struckNPC = false;
                npc = null;
            }
        }
        else
        {
            // NPC's attack radius - Player left
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player left NPC's attack range");
                // You can add logic here for when player leaves NPC's range
                // For example: gameManager.GetComponent<GameManager>().NPCTargetExited(character, other.gameObject);
                struckNPC = false;
            }
        }
    }

    void DamageNPC(NPC npc, int dmg)
    {
        if (npc != null)
        {
            npc.currentHealth -= dmg;
        }
    }
}
