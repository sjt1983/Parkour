
using UnityEngine;

public class PawnArmsAnimator : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;
    
    [SerializeField]
    private Animator pawnArmsAnimator;


    private void Update()
    {

        if (pawn.IsGrounded && pawn.IsMovingFasterThan(2f) && !pawn.IsSliding)
        {
            pawnArmsAnimator.SetBool("Running", true);
        }
        else
        {
            pawnArmsAnimator.SetBool("Running", false);
        }
    }
    

    public void SetTrigger(string trigger)
    {
        pawnArmsAnimator.SetTrigger("Vault");
    }

    public void ResetTrigger(string trigger)
    {
        pawnArmsAnimator.ResetTrigger("Vault");
    }

    public void Play(string animation)
    {
        pawnArmsAnimator.Play(animation);
    }
}
