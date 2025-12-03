using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player_SoundEffects : MonoBehaviour
{
    [Header("Player Movement")]
    public Player_Movement playerMovement;
    public AudioSource audioSource;
    public AudioClip footstepsClip;
    public float walkPitch = 0.8f;
    public float jogPitch = 1.2f;
    public float runPitch = 2f;
    // Get inventory script from player
    public Player_Inventory playerInventory;

    // Tool clips
    [Header("Tool Sounds")]
    public AudioSource toolAudioSource;
    public AudioSource toolReelSource;
    public AudioClip axe_HitWood_Clip;
    public AudioClip axe_HitFlesh_Clip;
    public AudioClip axe_Miss_Clip;

    public AudioClip fishingRod_Cast_Clip;
    public AudioClip fishingRod_Reel_Clip;
    public AudioClip fishingRod_Lure_Clip;

    void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<Player_Movement>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepsClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        if (toolAudioSource == null)
            toolAudioSource = gameObject.AddComponent<AudioSource>();
        toolAudioSource.playOnAwake = false;

        if (toolReelSource == null)
            toolReelSource = gameObject.AddComponent<AudioSource>();
        toolReelSource.loop = true;
        toolReelSource.playOnAwake = false;
        toolReelSource.clip = fishingRod_Reel_Clip;

        //Get inventory script
        playerInventory = GameObject.Find("Game Manager").GetComponent<Player_Inventory>();
    }

    void Update()
    {
        PlayerMovementSounds();

        //Debug.Log("Tool id being used: " + playerInventory.ReturnCurrentToolTypeID());
    }

    // Axe sound effects
    public void Axe_HitTree()
    {
        toolAudioSource.loop = false;
        toolAudioSource.PlayOneShot(axe_HitWood_Clip);
    }
    public void Axe_HitFlesh()
    {
        toolAudioSource.loop = false;
        toolAudioSource.PlayOneShot(axe_HitFlesh_Clip);
    }
    public void Axe_Miss()
    {
        toolAudioSource.loop = false;
        toolAudioSource.PlayOneShot(axe_Miss_Clip);
    }

    public void FishingRod_Cast()
    {
        toolAudioSource.loop = false;
        toolAudioSource.PlayOneShot(fishingRod_Cast_Clip);
    }

    public void FishingRod_Reel()
    {
        toolReelSource.Play();
    }

    public void FishingRod_StopReel()
    {
        toolReelSource.Stop();
    }

    public void FishingRod_Lure()
    {
        toolAudioSource.loop = false;
        toolAudioSource.PlayOneShot(fishingRod_Lure_Clip);
    }


    void PlayerMovementSounds()
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
