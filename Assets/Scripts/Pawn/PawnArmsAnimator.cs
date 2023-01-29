
using UnityEngine;

public class PawnArmsAnimator : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;
    
    [SerializeField]
    private Animator pawnArmsAnimator;

    public bool ikActive = false;

    private Vector3 leftHandPosition;

    private Vector3 rightHandPosition;

    private Quaternion leftHandRotation;

    private Quaternion rightHandRotation;

    private void Update()
    {

        if (pawn.IsGrounded && !pawn.IsSliding)
        {
            pawnArmsAnimator.SetBool("Jump", false);
        }
        else if (!pawn.IsGrounded || pawn.IsSliding)
        {
            pawnArmsAnimator.SetBool("Jump", true);
        }

        if (pawn.IsMovingFasterThan(2f))
        {
            pawnArmsAnimator.SetFloat("Speed", 1f, .2f, Time.deltaTime);
            pawnArmsAnimator.speed = 2.5f;
        }
        else
        {
            pawnArmsAnimator.SetFloat("Speed", 0f, .2f, Time.deltaTime);
        }
    }

    void OnAnimatorIK()
    {
        if (pawnArmsAnimator)
        {
            //if the IK is active, set the position and rotation directly to the goal.
            if (ikActive)
            {
                pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                pawnArmsAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
                pawnArmsAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRotation);

                pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                pawnArmsAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
                pawnArmsAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandRotation);
            }
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                pawnArmsAnimator.SetLookAtWeight(0);
            }
        }
    }

    public void SetActiveIk(Vector3 leftHandTargetPosition, Quaternion leftHandTargetRotation, Vector3 rightHandTargetPosition, Quaternion rightHandTargetRotation)
    {
        ikActive = true;

        leftHandPosition = leftHandTargetPosition;
        rightHandPosition = rightHandTargetPosition;
        leftHandRotation = leftHandTargetRotation;
        rightHandRotation = rightHandTargetRotation;

    }

    public void ClearIk()
    {
        ikActive = false;
    }

}
