using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using HexabodyVR.PlayerController;
using HurricaneVR.Framework.Core.Utils;

public class VRIKFootsteps : MonoBehaviour
{
    public VRIK vrik;
    public HexaBodyPlayer4 hexaBody;
    public AudioClip[] walkingFootsteps;
    public AudioClip[] runningFootsteps;
    public float footstepCooldown = 0.4f;
    public float stepHeightThreshold = 0.02f;
    public float velocityThreshold = 0.1f;
    public float walkingVolume = 0.3f;
    public float runningVolume = 0.7f;

    private Transform leftFoot, rightFoot;
    private Vector3 previousLeftFootPos, previousRightFootPos;
    private float leftFootCooldownTimer = 0f;
    private float rightFootCooldownTimer = 0f;

    void Start()
    {
        if (vrik != null && vrik.references != null)
        {
            leftFoot = vrik.references.leftFoot;
            rightFoot = vrik.references.rightFoot;
            previousLeftFootPos = leftFoot.position;
            previousRightFootPos = rightFoot.position;
        }
    }

    void Update()
    {
        // Update cooldown timers
        leftFootCooldownTimer -= Time.deltaTime;
        rightFootCooldownTimer -= Time.deltaTime;

        // Only check for footsteps if grounded
        if (hexaBody != null && hexaBody.IsGrounded)
        {
            if (leftFoot != null && leftFootCooldownTimer <= 0f && FootStepped(leftFoot, ref previousLeftFootPos))
                StartCoroutine(PlayFootstep(true));

            if (rightFoot != null && rightFootCooldownTimer <= 0f && FootStepped(rightFoot, ref previousRightFootPos))
                StartCoroutine(PlayFootstep(false));
        }
    }

    private bool FootStepped(Transform foot, ref Vector3 previousPos)
    {
        float velocity = (foot.position - previousPos).magnitude / Time.deltaTime;
        bool stepped = velocity > velocityThreshold && Mathf.Abs(foot.position.y - previousPos.y) > stepHeightThreshold;
        previousPos = foot.position;
        return stepped;
    }

    private IEnumerator PlayFootstep(bool isLeftFoot)
    {
        if (isLeftFoot)
            leftFootCooldownTimer = footstepCooldown;
        else
            rightFootCooldownTimer = footstepCooldown;

        AudioClip clip = GetFootstepClip(out float volume);
        if (clip != null)
        {
            Vector3 footPosition = isLeftFoot ? leftFoot.position : rightFoot.position;
            SFXPlayer.Instance.PlaySFX(clip, footPosition, 1f, volume);
        }

        yield return null;
    }

    private AudioClip GetFootstepClip(out float volume)
    {
        volume = walkingVolume;

        if (hexaBody != null && hexaBody.Sprinting && runningFootsteps.Length > 0)
        {
            volume = runningVolume;
            return runningFootsteps[Random.Range(0, runningFootsteps.Length)];
        }

        return walkingFootsteps.Length > 0 ? walkingFootsteps[Random.Range(0, walkingFootsteps.Length)] : null;
    }
}
