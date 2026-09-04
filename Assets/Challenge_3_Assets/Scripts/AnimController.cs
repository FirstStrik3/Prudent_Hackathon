using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimController : MonoBehaviour
{
    private Animator animator;

    [Header("Trigger Names")]
    public string explodeTriggerName = "Explode";
    public string assembleTriggerName = "Assemble";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayExplode()
    {
        if (animator == null) return;
        animator.ResetTrigger(assembleTriggerName);
        animator.SetTrigger(explodeTriggerName);
    }

    public void PlayAssemble()
    {
        if (animator == null) return;
        animator.ResetTrigger(explodeTriggerName);
        animator.SetTrigger(assembleTriggerName);
    }

    public void PlayStateDirect(string stateName)
    {
        if (animator == null) return;
        animator.Play(stateName);
    }
}