using UnityEngine;

public class NPC_MovementAlgorithm : MonoBehaviour
{
    public enum NavState
    {
        Idle,
        ModeOne,
        ModeTwo,
        ModeThree,
        Moving,
        Resting
    }

    [Header("Target")]
    public Vector3 targetGoal;
    public bool isWandering = true;

    [Header("Movement")]
    public float testSpeed = 3f;
    public float rotationSpeed = 5f;
    public float maxDistanceCanTravel;
    public float currentAccumulatedDistance;

    [Header("NPC Type")]
    public bool isLongDistanceTraveller = false;
    public bool isVeryLongDistanceTraveller = false;

    [Header("Attempts")]
    public int attempts;       // ModeOne attempts
    public int sc_attempts;    // ModeTwo attempts

    [Header("Resting")]
    public float restChance = 0.2f; //20% chance to rest after reaching target
    public float minRestTime = 2f; //seconds to rest
    public float maxRestTime = 6f; //seconds to rest
    private float restTimer = 0f;

    // Saved data from last hit for sideways mode
    private Vector3 lastHitNormal;
    private Vector3 lastLateralDirection;

    // Current target being moved to
    private Vector3 currentMoveTarget;

    // Current navigation state
    private NavState state = NavState.Idle;

    void Start()
    {
        state = NavState.Idle;
    }

    void Update()
    {
        switch (state)
        {
            case NavState.Idle:
                // NPC is waiting to start wandering
                if (isWandering)
                {
                    Debug.Log("[State] Idle: Starting wandering.");
                    BeginNavigation_Wandering();
                }
                break;

            case NavState.ModeOne:
                Debug.Log("[State] ModeOne: Attempting straight-line path to target.");
                RunModeOne();
                break;

            case NavState.ModeTwo:
                Debug.Log("[State] ModeTwo: Sideways checking for obstacles.");
                RunModeTwo();
                break;

            case NavState.ModeThree:
                Debug.Log("[State] ModeThree: Checking for building walls/doors.");
                RunModeThree();
                break;

            case NavState.Moving:
                PerformMovement();
                break;
            
            case NavState.Resting:
                RunResting();
                break;
        }
    }

    // --------------------------
    //   Begin Navigation (Wandering)
    // --------------------------
    void BeginNavigation_Wandering()
    {
        // Reset state for new navigation
        attempts = 0;
        sc_attempts = 0;
        currentAccumulatedDistance = 0f;

        // Determine random travel distance
        float minPercent = 0.1f; // default
        float maxPercent = 1f;

        // Longer distance NPCs
        if (isVeryLongDistanceTraveller) minPercent = 0.6f;
        else if (isLongDistanceTraveller) minPercent = 0.3f;

        float distance = Random.Range(minPercent * maxDistanceCanTravel, maxDistanceCanTravel);

        // Random direction
        Vector2 dir = Random.insideUnitCircle.normalized;
        targetGoal = transform.position + new Vector3(dir.x, 0f, dir.y) * distance;

        Debug.Log("[Navigation] New wandering targetGoal generated: " + targetGoal);

        // Move to ModeOne
        state = NavState.ModeOne;
    }

    // --------------------------
    //       Mode 1
    // --------------------------
    void RunModeOne()
    {
        Vector3 dir = (targetGoal - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetGoal);

        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, distanceToTarget))
        {
            // Obstacle detected
            lastHitNormal = hit.normal;
            lastLateralDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;

            float offset = 1f * (attempts + 1);
            Vector3 attemptPos = (attempts % 2 == 0) ? hit.point + lastLateralDirection * offset
                                                     : hit.point - lastLateralDirection * offset;

            attempts++;

            if (attempts > 5)
            {
                state = NavState.ModeTwo;   // switch to sideways checking
                return;
            }

            // Raycast to the attempt
            Vector3 attemptDir = (attemptPos - transform.position).normalized;
            float attemptDist = Vector3.Distance(transform.position, attemptPos);

            if (!Physics.Raycast(transform.position, attemptDir, attemptDist))
            {
                MoveTo(attemptPos);
                return;
            }
        }
        else
        {
            // Path is clear
            MoveTo(targetGoal);
        }
    }

    // --------------------------
    //       Mode 2
    // --------------------------
    void RunModeTwo()
    {
        if (sc_attempts >= 5)
        {
            state = NavState.ModeThree;
            return;
        }

        float offset = 1f * (sc_attempts + 1);
        Vector3 newPos = (sc_attempts % 2 == 0) ? targetGoal + lastLateralDirection * offset
                                                : targetGoal - lastLateralDirection * offset;

        sc_attempts++;

        float dist = Vector3.Distance(transform.position, newPos);
        if (currentAccumulatedDistance + dist > maxDistanceCanTravel)
        {
            state = NavState.ModeThree;
            return;
        }

        Vector3 dir = (newPos - transform.position).normalized;

        if (!Physics.Raycast(transform.position, dir, dist))
        {
            currentAccumulatedDistance += dist;
            MoveTo(newPos);
            return;
        }
    }

    // --------------------------
    //       Mode 3
    // --------------------------
    void RunModeThree()
    {
        Vector3 dir = (targetGoal - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, targetGoal);

        if (!Physics.Raycast(transform.position, dir, out RaycastHit hit, dist))
        {
            FailureToRoute();
            return;
        }

        Transform parent = hit.collider.transform.parent;
        if (parent == null || !parent.CompareTag("Building") || !hit.collider.CompareTag("Wall"))
        {
            FailureToRoute();
            return;
        }

        // Find closest door
        Transform closestDoor = null;
        float closestDist = Mathf.Infinity;

        foreach (Transform child in parent)
        {
            if (child.CompareTag("Door"))
            {
                float d = Vector3.Distance(transform.position, child.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    closestDoor = child;
                }
            }
        }

        if (closestDoor == null)
        {
            FailureToRoute();
            return;
        }

        MoveTo(closestDoor.position);
    }

    // --------------------------
    //       Movement
    // --------------------------
    void MoveTo(Vector3 pos)
    {
        currentMoveTarget = pos;
        state = NavState.Moving;
        Debug.Log("[Movement] Moving to: " + pos);
    }

    void PerformMovement()
    {
        Vector3 moveDir = (currentMoveTarget - transform.position).normalized;

        // Rotate smoothly toward movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move NPC
        transform.position = Vector3.MoveTowards(transform.position, currentMoveTarget, testSpeed * Time.deltaTime);

        // Check if reached target + add resting chance after reaching target
        if (Vector3.Distance(transform.position, currentMoveTarget) < 0.05f)
        {
            Debug.Log("[Movement] Reached target: " + currentMoveTarget);

            // After reaching a waypoint, decide if NPC should rest
            if (Random.value < restChance)
            {
                restTimer = Random.Range(minRestTime, maxRestTime);
                Debug.Log("[Resting] NPC is resting for " + restTimer + " seconds.");
                state = NavState.Resting;
            }
            else
            {
                state = NavState.ModeOne; // Continue normal movement
            }
        }
    }

    // --------------------------
    //       Failure Handling
    // --------------------------
    void FailureToRoute()
    {
        Debug.LogWarning("[Failure] NPC failed to route. Generating new targetGoal...");
        // Reset attempts and generate a new wandering target
        state = NavState.Idle;
    }

    // --------------------------
    //      Resting
    // --------------------------
    void RunResting()
    {
        restTimer -= Time.deltaTime;
        if (restTimer <= 0f)
        {
            Debug.Log("[Resting] Finished resting. Resuming wandering...");
            state = NavState.Idle;   // Go back to the wandering cycle
        }
    }
}
