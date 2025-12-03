using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
    public int hits = 3; // Number of hits required to chop down the tree
    public GameObject trunk; // Bottom of tree
    public GameObject top; // Top of tree
    public float fadeSpeed = 2f; // Speed of fading
    public float fallForce = 5f; // Force applied when tree top falls
    
    private bool isChopped = false;
    private Rigidbody topRigidbody;
    private Renderer[] topRenderers;

    public GameObject logPrefab; // Prefab for the log to spawn
    public float regenTimer;
    public float remainingTime = 0f;
    public float regenTime = 300f; // 5 minutes to regenerate one hit

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get Trunk and Top from children of this object
        trunk = transform.Find("Bottom").gameObject;
        top = transform.Find("Top").gameObject;
        
        // Get or add rigidbody to the top part
        if (top != null)
        {
            // Don't add rigidbody initially - we'll add it when needed
            
            // Get all renderers in the top part for fading
            topRenderers = top.GetComponentsInChildren<Renderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hits <= 0 && !isChopped)
        {
            ChopDownTree();
        }

        if(hits > 0 &&!isChopped)
        {
            RegenerateHits();
        }
    }

    public void RegenerateHits()
    {
        //Reset hits to 3 over time.

        if(hits < 3)
        {
            regenTimer += Time.deltaTime;
            remainingTime = regenTime - regenTimer;
            //Once it has been 5 irl minutes, reset hits to 3
            if (regenTimer >= regenTime)
            {
                hits++;
                regenTimer = 0f;
                remainingTime = regenTime;
            }
        }
    }

    public void ChopDownTree()
    {
        if (isChopped) return; // Prevent multiple calls
        
        isChopped = true;
        Debug.Log("Tree has been chopped down: " + gameObject.name);
        
        if (top != null)
        {
            // Add rigidbody only when tree is chopped
            if (topRigidbody == null)
            {
                topRigidbody = top.AddComponent<Rigidbody>();
                topRigidbody.mass = 150f; // Lighter mass
                topRigidbody.linearDamping = 7.5f; // Air resistance to slow down movement
                topRigidbody.angularDamping = 1f; // Resistance to slow down rotation
            }
            
            // Enable physics for falling
            topRigidbody.isKinematic = false;
            
            // Add some random force to make it fall naturally
            Vector3 randomDirection = new Vector3(
                Random.Range(-0.25f, 0.25f), 
                0f, 
                Random.Range(-0.25f, 0.25f)
            ).normalized;
            
            topRigidbody.AddForce(randomDirection * (fallForce / 2f), ForceMode.Impulse);
            topRigidbody.AddTorque(Random.insideUnitSphere * fallForce * 2f, ForceMode.Impulse); // Increased rotation
            
            // Start fading coroutine
            StartCoroutine(FadeAndDestroy());
        }
    }
    
    private IEnumerator FadeAndDestroy()
    {
        float elapsedTime = 0f;
        float fadeDuration = 1f / fadeSpeed;
        
        // Wait a bit before starting to fade
        yield return new WaitForSeconds(1f);
        
        // Store original materials and colors
        Material[][] originalMaterials = new Material[topRenderers.Length][];
        Color[][] originalColors = new Color[topRenderers.Length][];
        
        for (int i = 0; i < topRenderers.Length; i++)
        {
            originalMaterials[i] = topRenderers[i].materials;
            originalColors[i] = new Color[originalMaterials[i].Length];
            
            for (int j = 0; j < originalMaterials[i].Length; j++)
            {
                originalColors[i][j] = originalMaterials[i][j].color;
            }
        }
        
        // Fade out
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            for (int i = 0; i < topRenderers.Length; i++)
            {
                for (int j = 0; j < originalMaterials[i].Length; j++)
                {
                    Color newColor = originalColors[i][j];
                    newColor.a = alpha;
                    originalMaterials[i][j].color = newColor;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Deactivate the tree top after fading
        if (top != null)
        {
            if (topRigidbody != null)
            {
                DestroyImmediate(topRigidbody); // Remove rigidbody component completely
                topRigidbody = null;
            }
            top.SetActive(false);
        }
        
        // Disable the main tree's box collider after fading is complete
        BoxCollider treeCollider = GetComponent<BoxCollider>();
        if (treeCollider != null)
        {
            treeCollider.enabled = false;
        }
    }

    public void TakeHit()
    {
        hits--;
        Debug.Log("Tree hit! Remaining hits: " + hits);
        
        // Spawn a log when hit
        SpawnLog();
    }
    
    private void SpawnLog()
    {
        if (logPrefab == null) 
        {
            Debug.LogWarning("Log prefab is not assigned!");
            return;
        }
        
        // Spawn log from the tree's position (center/trunk area)
        Vector3 spawnPosition = new Vector3(
            transform.position.x + Random.Range(-0.2f, 0.2f), // Small random offset
            transform.position.y + Random.Range(0.5f, 1.5f), // Spawn above ground, from trunk area
            transform.position.z + Random.Range(-0.2f, 0.2f)  // Small random offset
        );
        
        // Spawn the log at the tree position with correct rotation
        GameObject newLog = Instantiate(logPrefab, spawnPosition, Quaternion.Euler(180f, Random.Range(0f, 360f), 0));
        
        // Add rigidbody to make it fall naturally
        Rigidbody logRigidbody = newLog.GetComponent<Rigidbody>();
        if (logRigidbody == null)
        {
            logRigidbody = newLog.AddComponent<Rigidbody>();
        }
        
        // Ignore collisions between log and tree parts
        Collider logCollider = newLog.GetComponent<Collider>();
        if (logCollider != null)
        {
            // Ignore collision with main tree collider
            Collider treeCollider = GetComponent<Collider>();
            if (treeCollider != null)
            {
                Physics.IgnoreCollision(logCollider, treeCollider);
            }
            
            // Ignore collision with trunk (bottom) collider
            if (trunk != null)
            {
                Collider trunkCollider = trunk.GetComponent<Collider>();
                if (trunkCollider != null)
                {
                    Physics.IgnoreCollision(logCollider, trunkCollider);
                }
            }
            
            // Ignore collision with top collider
            if (top != null)
            {
                Collider topCollider = top.GetComponent<Collider>();
                if (topCollider != null)
                {
                    Physics.IgnoreCollision(logCollider, topCollider);
                }
            }
            
            // Ignore collision with player collider
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider != null)
                {
                    Physics.IgnoreCollision(logCollider, playerCollider);
                }
            }
        }

        // Calculate target position avoiding player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 targetPosition;
        int maxAttempts = 10;
        int attempts = 0;
        
        float groundLevel = transform.position.y - 0.352f;
        
        do
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(0.3f, 0.6f); // Closer to tree
            
            targetPosition = new Vector3(
                transform.position.x + Mathf.Cos(angle) * distance,
                groundLevel, // Ground level
                transform.position.z + Mathf.Sin(angle) * distance
            );
            
            attempts++;
            
        } while (playerObj != null && Vector3.Distance(targetPosition, playerObj.transform.position) < 0.5f && attempts < maxAttempts);
        
        // If we couldn't find a good spot, use a far position
        if (playerObj != null && Vector3.Distance(targetPosition, playerObj.transform.position) < 0.5f)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            targetPosition = new Vector3(
                transform.position.x + Mathf.Cos(angle) * 0.8f, // Still close but away from player
                groundLevel,
                transform.position.z + Mathf.Sin(angle) * 0.8f
            );
        }
        
        // Start position at bottom of tree
        Vector3 startPosition = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );
        
        // Disable rigidbody for lerping (we'll enable it after)
        logRigidbody.isKinematic = true;
        
        // Start arc movement coroutine
        StartCoroutine(MoveLogInArc(newLog, startPosition, targetPosition, logRigidbody));
        
        Debug.Log("Log lerping from tree to: " + targetPosition);
    }
    
    private bool IsPositionOccupied(Vector3 position)
    {
        // Check if there's already a log or other object within 1 unit radius
        Collider[] colliders = Physics.OverlapSphere(position, 1f);
        
        foreach (Collider col in colliders)
        {
            // Check if it's a log (you can adjust this condition based on your log prefab)
            if (col.gameObject.name.Contains("Log") || col.gameObject == gameObject)
            {
                return true; // Position is occupied
            }
        }
        
        return false; // Position is free
    }
    
    private IEnumerator MoveLogInArc(GameObject log, Vector3 startPos, Vector3 endPos, Rigidbody logRigidbody)
    {
        float duration = 1f; // Time for the arc movement
        float elapsedTime = 0f;
        
        // Calculate arc height (midpoint will be higher)
        float arcHeight = 1.5f;
        Vector3 midPoint = (startPos + endPos) / 2f;
        midPoint.y += arcHeight;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            // Use quadratic bezier curve for smooth arc
            Vector3 currentPos = CalculateBezierPoint(t, startPos, midPoint, endPos);
            log.transform.position = currentPos;
            
            // Add some rotation during flight
            log.transform.Rotate(Vector3.right * (180f * Time.deltaTime));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position is exact
        log.transform.position = endPos;
        Debug.Log("Log final position set to: " + endPos + " (Y should be: " + endPos.y + ")");
        
        // Wait a moment to ensure position is set, then enable physics
        yield return new WaitForFixedUpdate();
        
        // Enable physics after landing
        if (logRigidbody != null)
        {
            // Stop any existing velocity to prevent bouncing
            logRigidbody.linearVelocity = Vector3.zero;
            logRigidbody.angularVelocity = Vector3.zero;
            logRigidbody.isKinematic = false;
            
            // Ensure position is maintained after enabling physics
            log.transform.position = endPos;
        }
        
        // Re-enable collision with player after landing
        Collider logCollider = log.GetComponent<Collider>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (logCollider != null && player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null)
            {
                // Re-enable collision between log and player
                Physics.IgnoreCollision(logCollider, playerCollider, false);
            }
        }
    }
    
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Quadratic Bezier curve formula
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2f * u * t * p1; // 2(1-t)t * P1
        p += tt * p2; // t^2 * P2
        
        return p;
    }
}
