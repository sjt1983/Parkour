
using UnityEngine;

public class PawnArmsAnimator : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;
    
    [SerializeField]
    private Animator pawnArmsAnimator;

    public bool ikActive = false;

    [SerializeField]
    public Transform rightHandObj;
    [SerializeField]
    public Transform leftHandObj;

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

                // Set the look target position, if one has been assigned
                if (leftHandObj != null)
                {
                    pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    pawnArmsAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    pawnArmsAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null)
                {
                    pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    pawnArmsAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    pawnArmsAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                pawnArmsAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                pawnArmsAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                pawnArmsAnimator.SetLookAtWeight(0);
            }
        }
    }

    public void SetActiveIk(Vector3 leftHandTarget, Vector3 rightHandTarget)
    {
        ikActive = true;

        leftHandObj.position = leftHandTarget;
        rightHandObj.position = rightHandTarget;
    }

    public void ClearIk()
    {
        ikActive = false;
    }

}
