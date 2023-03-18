using UnityEngine;

public class PawnWallRun : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnInput pawnInput;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private PawnMovement pawnMovement;

    [SerializeField]
    private Sensor wallRunSensor;

    //Flag to toggle checking for walls to the left or right of the player
    //We either check left or right, once per frame.
    private bool checkLeft = true;

    //The layer mask we can hit when looking to wall run..
    private LayerMask layerMask;

    //Overall state of the script
    private WallRunState wallRunState = WallRunState.CHECKING;

    //Position we lerp from once we can successfully wallrun.
    private Vector3 lerpFromPosition;
    //Position we lerp to once we can successfully wallrun.
    private Vector3 lerpToPosition;
    //Current lerp timer
    private float adjustmentLerpTimer = 0f;
    //How long the lerp should take
    private const float LERP_TIME = .2f;
    private float adjustmentLerpDuration;

    //Forward Vector for wall running, since we take control away from PlayerMovement
    private Vector3 forwardVector;

    //The Vector to apply when the pawn jumps off the wall.
    private Vector3 jumpVector;

    //Where we should raycast from to see if we should wall run
    private Vector3 raycastOrigin;

    //Current timer for raycasting to verify we are still on a wall
    private float raycastClampValidationTimer = 0f;
    //How often we should raycast to validate we are still on a wall.
    private const float RAYCAST_VALIDATION_TIME = .15f;

    //How far ahead of the pawn the initial raycast should start
    private const float RAYCAST_ORIGIN_FORWARD_BUFFER = 1f;

    //How far ahead of the pawn the initial raycast should start
    private const float RAYCAST_DISTANCE = .55f;

    //Difference in the angle in which the pawn jumped, and its current angle.
    float lastGroundedAngleDifference;
    //How bug the difference can be to allow wall running
    private const float WALL_RUN_ANGLE_LIMIT = 30f;

    private const float WALL_JUMP_MINIMUM_Y_VELOCITY = -5f;
    private const float WALL_JUMP_MAXIMUM_Y_VELOCITY = 1f;

    //How far we should Z rotate the camera.
    private const float TARGET_CAMERA_ANGLE = 15;

    //Forces applied when the pawn jumps off the wall.
    private const float JUMP_FORCE_Z = 5f;
    private const float JUMP_FORCE_Y = 8f;

    //Collision flags collected when we move the pawn.
    private CollisionFlags collisionFlags;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("MapGeometry");
    }

    // Update is called once per frame
    void Update()
    {
        //Constraints explained
        //The angle we left the ground at is not too different from the current angle, e.g. cant jump, rotate 90 degrees, then wall run.
        //CurrentYSpeed; This will be tweaked but the pawn needs to be at the "height" of the jump to make it feel good, so the frame we are "Falling" we will allow it, but not falling for too long.
        //We should not be grounded
        //We need at leasat 1 speed charge.
        //User is pressing the Parkour key
        //Current state of the Wallrun script is that we are not in the middle of a wall jump
        lastGroundedAngleDifference = ParkourUtils.DifferenceInBetweenTwoAngles(transform.rotation.eulerAngles.y, pawnMovement.LastGroundedFrameAngle);

        if (lastGroundedAngleDifference <= WALL_RUN_ANGLE_LIMIT && 
            pawnMovement.CurrentYSpeed <= WALL_JUMP_MAXIMUM_Y_VELOCITY && 
            pawnMovement.CurrentYSpeed >= WALL_JUMP_MINIMUM_Y_VELOCITY && 
            !pawn.IsGrounded && 
            pawn.SpeedCharges > 0 && 
            pawnInput.ParkourPressed && 
            wallRunState == WallRunState.CHECKING)
        {
            //Determine the first raycast origin point, which should be by the pawns feet and 1m in front of it (pawn's origin is center)
            raycastOrigin = transform.position + (transform.forward * RAYCAST_ORIGIN_FORWARD_BUFFER);
            //Toggle the checkLeft flag for the execution of the script.
            checkLeft = !checkLeft;

            //If we hit a runnable wall.
            if (Physics.Raycast(raycastOrigin, (checkLeft ? -transform.right : transform.right), out RaycastHit hitInfo, RAYCAST_DISTANCE, layerMask))
            {
                //Reset the lerp timer to zero
                adjustmentLerpTimer = 0f;

                //Generate the forward vector based off the surface normal we hit.
                Quaternion forwardRotation = Quaternion.FromToRotation(checkLeft ? Vector3.right : -Vector3.right, hitInfo.normal);
                Quaternion jumpRotation = Quaternion.FromToRotation(Vector3.forward, hitInfo.normal);
                forwardVector = ParkourUtils.GenerateDirectionalForceVector(forwardRotation, pawnMovement.CurrentGroundedZSpeed);
                jumpVector = ParkourUtils.GenerateDirectionalForceVector(jumpRotation, JUMP_FORCE_Z);

                //Set the pawns LastGroundedFrameAngle to ensure it matches the direction of the vector so we respec the Landing code in PawnMovement.
                pawnMovement.LastGroundedFrameAngle = forwardRotation.eulerAngles.y;

                //Generate the lerp from and to positions.
                lerpFromPosition = transform.position;
                //Raise up the 
                lerpToPosition = hitInfo.point;
                                
                //Rotate the Camera on the Z axis to give visual confirmation of the wall run.
                pawnLook.TargetZAngle = checkLeft ? -TARGET_CAMERA_ANGLE : TARGET_CAMERA_ANGLE;
                
                //Stop PawnMovement.
                pawnMovement.MovementLocked = true;
                pawn.IsWallRunning = true;

                //Move onto the next step in the script.
                wallRunState = WallRunState.ADJUSTING;
            }
        }
        else if (wallRunState == WallRunState.ADJUSTING)
        {
            //Lerp the pawn into position
            adjustmentLerpTimer += Time.deltaTime;
            adjustmentLerpDuration = adjustmentLerpTimer / LERP_TIME;
            transform.position = Vector3.Lerp(lerpFromPosition, lerpToPosition, adjustmentLerpDuration);

            //Lerp isnt 100% accurate, and I dont want to push the pawn too close to the wall and trigger a collision, so once we are close enough, call it good and move on.
            //Will need to be tweaked once we have players running around to ensure they are not too far off the wall.
            //IDEA!!!!!!!!!!!!!!!!! Call Pawn.Move(Vector3.zero) to get collision flags and stop once we collide.
            if (Vector3.Distance(lerpToPosition, transform.position) < .5)
               wallRunState = WallRunState.CLAMPED;
     
        }
        else if (wallRunState == WallRunState.CLAMPED)
        {
            //Now we have a series of checks to make sure we can still Wallrun.

            //First, are we still on the wall?
            raycastClampValidationTimer += Time.deltaTime;
            if (raycastClampValidationTimer >= RAYCAST_VALIDATION_TIME)
            {
                if (!Physics.Raycast(transform.position, (checkLeft ? -transform.right : transform.right), out RaycastHit hitInfo, 1f, layerMask))
                    GracefullyReleaseControl();
                else
                    raycastClampValidationTimer = 0f;
            }

            //Did the user stop hitting the forward key.
            if (pawnInput.ZDirection <= 0)
                 GracefullyReleaseControl();

            //Have we collided with another wall after moving??
            collisionFlags = pawn.Move(forwardVector * Time.deltaTime);
            if ((collisionFlags & CollisionFlags.CollidedSides) != 0)
                GracefullyReleaseControl();

            //Did the user jump off the wall?
            if (pawnInput.JumpPressedThisFrame)
            {
                pawnMovement.TransferState(forwardVector + jumpVector, JUMP_FORCE_Y);
                GracefullyReleaseControl();
            }

        }
    }

    //Hand control back to PawnMovement
    private void GracefullyReleaseControl()
    {
        raycastClampValidationTimer = 0f;
        pawnMovement.MovementLocked = false;
        wallRunState = WallRunState.CHECKING;
        pawnLook.TargetZAngle = 0;
        pawn.IsWallRunning = false;
    }
}

enum WallRunState
{
    CHECKING,  //Looking for opportunities to wallrun if the player is attempting to wall run.
    ADJUSTING, //Doing the initial adjustment to the "Snap target" on the wall
    CLAMPED    //We are actually running on the wall.
}
