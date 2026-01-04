using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Insect : MonoBehaviour
{

    public bool isGround = true; // Is this insect grounded or flying?
    public float delayMin = 5f;
    public float delayMax = 50f;
    public float moveSpeed = 1f;
    public float turnSpeed = 360f; // degrees per second for smooth turning
    public bool isMoving = false;
    private Coroutine currentMoveRoutine = null;
    // Obstacle checking settings
    public float obstacleCheckDistance = 0.5f; // forward ray distance while moving
    public float obstaclePadding = 0.05f;      // stop this far before obstacle

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGround)
        {
            if(!isMoving)
            {
                Navigation();
            }
        }
        // else
        // {
        //     FlyMovement();
        // }
    }

    void Navigation()
    {
        // Pathfinding using raycasting. Not using Unity nasvmesh to challenge myself + this is useful for future projects.
        // Generate ray casts from the 4 cardinal directions (N, S, E, W) to detect obstacles
        Vector3 rayOrigin = this.transform.position + Vector3.up * 0.5f; // Start raycast from mid-collider body height
        Vector3 rayDirectionNorth = this.transform.forward;
        float rayDistance = 0.5f; // 0.5 meters in front of this insect

        // Visual debug for decision ray
        Debug.DrawRay(rayOrigin, rayDirectionNorth * rayDistance, Color.red, 0.0f, false);

        RaycastHit hit;
        // Check northern raycast
        if (Physics.Raycast(rayOrigin, rayDirectionNorth, out hit, rayDistance))
        {
            // Obstacle detected in front of the insect
            Debug.Log("Obstacle detected North: " + hit.collider.name);
            // Avoid moving north
            ChooseDirection(0);
        }
        else
        {
            // Move in any direction
            ChooseDirection(4);
        }
    }

    void ChooseDirection(int directionToAvoid) // If direction to avoid is 0=N,1=S,2=E,3=W, 4=none
    {
        // Choose from 8 directions (N, NE, E, SE, S, SW, W, NW) and avoid blocked direction
        Vector3[] candidates = GetEightDirections();
        float decisionDelay = Random.Range(delayMin, delayMax);

        // Compute avoidance vector from code (cardinals)
        Vector3 avoidVec = Vector3.zero;
        switch (directionToAvoid)
        {
            case 0: avoidVec = transform.forward; break;
            case 1: avoidVec = -transform.forward; break;
            case 2: avoidVec = transform.right; break;
            case 3: avoidVec = -transform.right; break;
            default: avoidVec = Vector3.zero; break;
        }

        // Filter candidates that are too aligned with the avoidance vector
        List<Vector3> filtered = new List<Vector3>(candidates.Length);
        const float avoidAngleDeg = 35f; // avoid directions within ~35° of the blocked side
        foreach (var c in candidates)
        {
            if (avoidVec == Vector3.zero)
            {
                filtered.Add(c);
            }
            else
            {
                float ang = Vector3.Angle(new Vector3(c.x, 0f, c.z), new Vector3(avoidVec.x, 0f, avoidVec.z));
                if (ang > avoidAngleDeg)
                {
                    filtered.Add(c);
                }
            }
        }
        if (filtered.Count == 0)
        {
            // Fallback to any candidate
            filtered.AddRange(candidates);
        }

        Vector3 dir = filtered[Random.Range(0, filtered.Count)];
        StartMove(dir, decisionDelay);
    }

    // void GroundMovement()
    // {
    //     Navigation();
    // }

    void FlyMovement()
    {
        
    }

    void StartMove(Vector3 dir,float duration)
    {
        if (isMoving) return; // Prevent overlapping moves
        if (currentMoveRoutine != null) StopCoroutine(currentMoveRoutine);
        currentMoveRoutine = StartCoroutine(MoveForDuration(dir, duration));
    }

    IEnumerator MoveForDuration(Vector3 dir, float duration)
    {
        isMoving = true;
        float t = 0f;

        // Smoothly rotate toward desired direction while moving
        Quaternion targetRot = (dir.sqrMagnitude > 0.0001f)
            ? Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z))
            : transform.rotation;

        while (t < duration)
        {
            // Visualize movement direction each frame
            Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
            // Rotate toward target smoothly
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            Vector3 fwd = transform.forward.normalized;
            Debug.DrawRay(rayOrigin, fwd * obstacleCheckDistance, Color.yellow, 0f, false);

            // Check for obstacle ahead
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, fwd, out hit, obstacleCheckDistance))
            {
                // Move up to the obstacle minus padding, then re-decide
                float step = moveSpeed * Time.deltaTime;
                float allowed = Mathf.Max(0f, hit.distance - obstaclePadding);
                if (allowed > 0f)
                {
                    transform.Translate(fwd * Mathf.Min(step, allowed), Space.World);
                }

                // Determine direction to avoid based on current forward vector
                int avoid = DirectionCodeFromVector(fwd);

                isMoving = false;
                currentMoveRoutine = null;
                // Immediately choose a new direction that avoids the detected obstacle
                ChooseDirection(avoid);
                yield break;
            }
            else
            {
                // Free path; move normally
                transform.Translate(fwd * moveSpeed * Time.deltaTime, Space.World);
            }

            t += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
        currentMoveRoutine = null;
    }

    // Map a movement vector to the avoidance code used by ChooseDirection
    int DirectionCodeFromVector(Vector3 d)
    {
        // Use dot products to classify relative to local axes
        Vector3 f = transform.forward;
        Vector3 r = transform.right;
        float fd = Vector3.Dot(d, f);
        float rd = Vector3.Dot(d, r);
        const float thresh = 0.7f; // ~45 degrees cone
        if (fd > thresh) return 0;      // North (forward)
        if (fd < -thresh) return 1;     // South (back)
        if (rd > thresh) return 2;      // East (right)
        if (rd < -thresh) return 3;     // West (left)
        return 4; // none
    }

    // Compute 8-direction vectors relative to current orientation
    Vector3[] GetEightDirections()
    {
        Vector3 f = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 r = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 b = -f;
        Vector3 l = -r;
        return new Vector3[]
        {
            f,                    // N
            (f + r).normalized,   // NE
            r,                    // E
            (b + r).normalized,   // SE
            b,                    // S
            (b + l).normalized,   // SW
            l,                    // W
            (f + l).normalized    // NW
        };
    }
}
