using UnityEngine;

//Pogo Stick, and item which makes the player go bouncy.
public class PogoStick : EquippableItem
{
    //Local ref to the jumpBoostSensor, used for wall jumping.
    private GameObject jumpBoostSensor;

    //Local CharacterController ref since we want to control movement with this script.
    private CharacterController characterController;

    //Local ref to the pawn input so we can do our own Input Detection.
    private PawnInput pawnInput;

    //Local r ef to the pawnMovement script
    private PawnMovement pawnMovement;

    //How Fast we move along X/Z when in the air.
    private Vector3 xzAirVelocity;

    //In-Air movement speeds.
    private const float AIR_MOVEMENT_Z_SPEED = 12f;
    private const float AIR_MOVEMENT_X_SPEED = 2f;

    //How much force we are jumping with at the end of the frame.
    private float currentXSpeed = 0f;
    private float currentYSpeed = 0f;
    private float currentZSpeed = 0f;

    //Number of "Jump Charges", to simulate getting higher with each bounce.
    private int jumpCharges = 0;

    //Maximum number of jump charges.
    private const int JUMP_CHARGES_MAX = 3;

    //How much jump force is added epr charge.
    private const float JUMP_CHARGES_FORCE_PER_CHARGE = 4;

    //How much jump force before the bonus charges are applied.
    private const int STARTING_JUMP_FORCE = 6;

    //How much force generated from a wall jump.
    private const float WALL_JUMP_FORCE = 10f;

    //The angle the pawn was at the last time it was grounded, used to determine how we reflect off the wall when jumping
    private float lastGroundedFrameAngle; 

    //Assign the pawn to the item.
    public override void AssignToPawn(Pawn pawn)
    {
        //Call the superclass
        base.AssignToPawn(pawn);

        //Grab local refs to needed Components.
        characterController = pawn.gameObject.GetComponent<CharacterController>();
        pawnMovement = pawn.gameObject.GetComponent<PawnMovement>();
        pawnInput = pawn.gameObject.GetComponent<PawnInput>();        
        jumpBoostSensor = pawn.transform.Find("Sensors/JumpBoostSensor").gameObject;

        //Calibrate the last grounded frame angle.
        lastGroundedFrameAngle = pawn.transform.rotation.eulerAngles.y;

        //Ensure the script is not running right away.
        GracefullyReleaseControl();
    }

    // Update is called once per frame
    void Update()
    {
        //If this item is not actually assigned to a pawn, lets release control of the pawn and exit.
        if (!IsAssignedToPawn)
        {
            GracefullyReleaseControl();
            return;
        }

        //If the item is not equipped, lets release control of the pawn and exit.
        if (!Equipped)
        {
            GracefullyReleaseControl();
            return;
        }

        //If the item is not enabled, and nothing else is current locking the pawn, lets see if we should enable the item.
        if (!ItemEnabled && !pawn.MovementLocked)
        {
            //If jump is not currently presset, exit.
            if (!pawnInput.JumpPressed)
                return;
            //If Jump is pressed, lets enable it.
            ItemEnabled = true;
        }

        //If we hit the ground crouched, lets release control of the pawn and exit.
        if (pawn.IsGrounded && pawnInput.CrouchPressed)
        {
            GracefullyReleaseControl();
            return;
        }

        //If we hit this far and are item locked, disable the pawn, but do not touch the MovementLocked flag, as something else needed to lock this item but keep control of the pawn.
        if (pawn.ItemLocked)
        {
            ItemEnabled = false;
            return;
        }

        //Ok, lets bounce, MovementLock the pawn from the main movement script.
        pawn.MovementLocked = true;

        //If we are gounded we need to handle the bouncy bounce.
        if (pawn.IsGrounded)
        {
            //IF jump is pressed, add a charge up to the max.
            if (pawnInput.JumpPressed)
                jumpCharges = Mathf.Clamp(jumpCharges + 1, 0, JUMP_CHARGES_MAX);

            //Lets set the X/Z Vectors on the pawn
            pawn.RightVector = pawn.transform.right;
            pawn.ForwardVector = pawn.transform.forward;
            //Also set the last grounded frame angle
            lastGroundedFrameAngle = pawn.transform.rotation.eulerAngles.y;
            pawnMovement.LastGroundedFrameAngle = lastGroundedFrameAngle;
            //JUMP!!!!!!!
            currentYSpeed = STARTING_JUMP_FORCE + (JUMP_CHARGES_FORCE_PER_CHARGE * jumpCharges);
            pawnMovement.WasGroundedLastFrame = true;
        }
        else //aka in the air.
        {
            currentZSpeed = pawn.PawnInput.ZDirection * AIR_MOVEMENT_Z_SPEED;
            currentXSpeed = pawn.PawnInput.XDirection * AIR_MOVEMENT_X_SPEED;

            //In-Air movement
            xzAirVelocity = (pawn.ForwardVector * currentZSpeed) + (pawn.RightVector * currentXSpeed);

            pawnMovement.CurrentGroundedZSpeed = currentZSpeed;
            pawnMovement.CurrentGroundedXSpeed = currentXSpeed;
            pawnMovement.XZAirVelocity = Vector3.zero;
            pawnMovement.XZGroundVelocity = xzAirVelocity;
            
            //If the pawn tried to jump this frame, it has to be a wall jump.
            if (pawnInput.JumpPressedThisFrame)
               WallJump();

            pawnMovement.WasGroundedLastFrame = false;
        }

        //Apply gravity.
        currentYSpeed += pawn.PawnMovement.Gravity * Time.deltaTime;

        //Move the pawn, if we hit our heads, we fall next frame.
        CollisionFlags lastFrameCollisionFlags = characterController.Move((xzAirVelocity + (pawn.transform.up * currentYSpeed)) * Time.deltaTime);

        if ((lastFrameCollisionFlags & CollisionFlags.CollidedAbove) != 0)
        {
            currentYSpeed = -5;
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
            currentYSpeed = WALL_JUMP_FORCE;
            xzAirVelocity = Vector3.Reflect(xzAirVelocity, hit.normal);

            //If we did some crazy rotation to hit the wall, reflect the angle, normalize it, then add a Vector3 to "push off" the wall instead of a "Bounce".
            //JK also testing adding an additional force and NOT normalizing the initial vector.
            if (ParkourUtils.DifferenceInBetweenTwoAngles(currentYAngle, lastGroundedFrameAngle) >= 45)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hit.normal, pawn.transform.up);
                Vector3 wallForce = ParkourUtils.GenerateDirectionalForceVector(targetRotation, 3f);
                xzAirVelocity += wallForce;
            }

            //Finally, Re-adjust the current Locked Vectors pointing towards the angle from the wall jump.
            pawn.RightVector = Quaternion.LookRotation(xzAirVelocity) * Vector3.right;
            pawn.ForwardVector = Quaternion.LookRotation(xzAirVelocity) * Vector3.forward;

        }
    }

    //Gracefully turn control of the pawn over to something else.
    private void GracefullyReleaseControl()
    {
        ItemEnabled = false;
        jumpCharges = 0;
        if (IsAssignedToPawn)
        {
            pawn.MovementLocked = false;
        }
    }
}
