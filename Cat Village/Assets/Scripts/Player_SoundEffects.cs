using UnityEngine;

public class Player_SoundEffects : MonoBehaviour
{
    public Player_Movement playerMovement;
    public AudioSource audioSource;
    public AudioClip footstepsClip;
    public float walkPitch = 0.8f;
    public float jogPitch = 1.2f;
    public float runPitch = 2f;

    void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<Player_Movement>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepsClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (playerMovement != null && playerMovement.isGrounded && !playerMovement.isColliding)
        {
            float curPitch;
            if (Input.GetKey(KeyCode.LeftShift)) curPitch = runPitch;
            else if (playerMovement.capsLockOn) curPitch = walkPitch;
            else curPitch = jogPitch;

            audioSource.pitch = curPitch;

            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
