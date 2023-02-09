using UnityEngine;

//Pogo Stick, and item which makes the player go bouncy.
public class PogoStick : EquippableItem
{
    private GameObject jumpBoostSensor;

    private CharacterController characterController;

    private PawnInput pawnInput;
    private Vector3 lastXRight;
    private Vector3 lastZForward;
    private Vector3 xzAirVelocity;

    private float jumpForce = 0f;
    private float lastGroundedFrameAngle;

    public override void AssignToPawn(Pawn pawn)
    {
        base.AssignToPawn(pawn);
        characterController = pawn.gameObject.GetComponent<CharacterController>();
        pawnInput = pawn.gameObject.GetComponent<PawnInput>();
        lastGroundedFrameAngle = pawn.transform.rotation.eulerAngles.y;
        jumpBoostSensor = pawn.transform.Find("Sensors/JumpBoostSensor").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (pawn == null)
        {
            return;
        }

        if (!Equipped || !IsAssignedToPawn)
        {
            pawn.MovementLocked = false;
            return;
        }

        pawn.MovementLocked = true;

        if (pawn.IsGrounded)
        {
            lastXRight = pawn.transform.right;
            lastZForward = pawn.transform.forward;
            jumpForce = pawn.PawnInput.JumpIsPressed ? 20f : 15f;
        }
        else //aka in the air.
        {
            xzAirVelocity = (lastZForward * pawn.PawnInput.VerticalDirection * 8) + (lastXRight * pawn.PawnInput.HorizontalDirection * 2f);
            if (pawnInput.JumpedThisFrame)
               WallJump();
        }
        lastGroundedFrameAngle = pawn.transform.rotation.eulerAngles.y;

        jumpForce += pawn.PawnMovement.Gravity * Time.deltaTime;

        CollisionFlags lastFrameCollisionFlags = characterController.Move((xzAirVelocity + (pawn.transform.up * jumpForce)) * Time.deltaTime);

        if ((lastFrameCollisionFlags & CollisionFlags.CollidedAbove) != 0)
        {
            jumpForce = -5;
        }
    }

    private void WallJump()
    {
        //Basically, Raycast in front of the player and see if we hit a wall.
        if (Physics.Raycast(jumpBoostSensor.transform.position + (pawn.transform.forward * -1f), pawn.transform.forward, out RaycastHit hit, 2f, LayerMask.GetMask("MapGeometry")))
        {
            //We hit a wall, two scenarios.
            //IF we are running at a decent angle towards the wall we will perfectly reflect the wall jump
            //In a scenario where we run perpendicular to it, a reflect will not do much, so push the player away from the wall.

            //Validate the current angle we jumped at.
            //IMPROVEMENT - figure out which euler y the vector is traveling instead of locking in the angles at certain times.
            //If we have a difference of less than 45 degrees between the current angle and the angle we jumped at, reflect all velocity perfectly.
            var currentYAngle = pawn.transform.rotation.eulerAngles.y;
            jumpForce = 15f;
            xzAirVelocity = Vector3.Reflect(xzAirVelocity, hit.normal);

            //If we did some crazy rotation to hit the wall, reflect the angle, normalize it, then add a Vector3 to "push off" the wall instead of a "Bounce".
            //JK also testing adding an additional force and NOT normalizing the initial vector.
            if (Utils.DifferenceInBetweenTwoAngles(currentYAngle, lastGroundedFrameAngle) >= 45)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hit.normal, pawn.transform.up);
                Vector3 wallForce = Utils.GenerateDirectionalForceVector(targetRotation, 3f);
                xzAirVelocity += wallForce;
            }

            //Finally, Re-adjust the current Locked Vectors pointing towards the angle from the wall jump.
            lastXRight = Quaternion.LookRotation(xzAirVelocity) * Vector3.right;
            lastZForward = Quaternion.LookRotation(xzAirVelocity) * Vector3.forward;

        }
    }
}
