using UnityEngine;

public class Player_Camera : MonoBehaviour
{

    public GameObject player;
    public Vector3 offset;
    public float tiltStep = 5f; //degrees to tilt the camera up or down with each key press
    public int maxTilt = 2; //maximum upward tilts
    public int minTilt = -2; //maximum downward tilts, usually 0

    public int tiltLevel = 0; //current tilt level
    private float basePitch = 33f; //base camera pitch
    private float currentPitch; //current camera pitch
    private float targetPitch; //target camera pitch
    public float tiltSmoothSpeed = 5f; //speed of camera tilt adjustment

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tiltLevel = 0; // Start at neutral position
        currentPitch = basePitch;
        targetPitch = basePitch + (tiltLevel * tiltStep); // Calculate initial target pitch
    }

    // Update is called once per frame
    void Update()
    {
        // Handle up/down arrow input
        if (Input.GetKeyDown(KeyCode.DownArrow) && tiltLevel < maxTilt)
        {
            tiltLevel++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && tiltLevel > minTilt)
        {
            tiltLevel--;
        }

        // Calculate target pitch based on tiltLevel
        targetPitch = basePitch + (tiltLevel * tiltStep);

        // Smoothly interpolate currentPitch towards targetPitch
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * tiltSmoothSpeed);

        // Set camera position relative to player
        transform.position = player.transform.position + offset;

        // Rotate camera around X-axis (pitch), keep Y/Z rotation aligned with player
        Vector3 angles = transform.eulerAngles;
        angles.x = currentPitch;
        transform.eulerAngles = angles;
    }
}
