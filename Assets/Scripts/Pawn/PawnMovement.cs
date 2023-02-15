using UnityEngine;

public sealed class PawnMovement : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    //Main GameObject controlled by the player.
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private Sensor jumpBoostSensor;

    /************************/
    /*** Class properties ***/
    /************************/

    //How fast we are moving forward.
    public float ForwardSpeed { get => CurrentZSpeed; }

    //How fast we are moving upward.
    public float UpwardSpeed { get => CurrentYSpeed; }

    //Quick check to see if there is significant movement
    public bool IsMoving { get => Mathf.Abs(CurrentXSpeed + CurrentZSpeed) > .1f; }

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    /*** Default Upper Thresholds for movement ***/

    //How fast the character walks.
    private const float MAX_WALK_SPEED_FORWARD = 7f;
    private const float MAX_WALK_SPEED_SIDEWAYS = 4f;

    private float airAccelerationZ = 0f;
    private float airAccelerationX = 0f;

    //How fast the character walks when crouched.
    private const float MAX_CROUCH_WALK_SPEED = 2f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;

    //How extra hard the character jumps while boosted
    private const float JUMP_BOOST_MULTIPLIER = 1f;

    //Minimum vertical veelocity needed to wall jump
    private const float WALL_JUMP_MINIMUM_VELOCITY = -1.25f;

    //Default Gravity if the character is grounded to ensure we stay grounded when going down slopes.
    private const int GRAVITY_DEFAULT = -5;

    //Gravity
    [SerializeField]
    public float Gravity = -30;

    //Used to know to dip the camera when we land if the pawn is falling fast enough
    private const float GRAVITY_CAMERA_DIP_THRESHOLD = -9f;

    //How many speed charges the character can have.
    private const int MAX_SPEED_CHARGES = 2;

    //How much of a speed boost the character gets per charge
    private const float SPEED_CHARGE_MAX_VELOCITY = 1f;

    //How much drag we want to add per second;
    private const float SLIDING_DRAG = 3f;

    //How fast we need to be going to slide.
    private const float SLIDE_MINIMUM_VELOCITY = 2f;

    //Minimum Angle to slide
    private const float SLIDE_DRAG_ANGLE = 1f;

    //Y positon of the last frame, if its higher than this frame, we can state we are sliding downhill.
    private float slidingLastFrameYCheck;

    //Current Speed the character is moving in all directions.
    public float CurrentXSpeed = 0;
    public float CurrentYSpeed = 0;
    public float CurrentZSpeed = 0;

    //Velocity to move the pawn based on being grounded.
    public Vector3 XZGroundVelocity = Vector3.zero;

    //Velocity to move the pawn based on being grounded.
    public Vector3 XZAirVelocity = Vector3.zero;

    //Final clamped XZ Vector for this script;
    Vector3 finalXZVector = Vector3.zero;

    //We calculate y separately because we need to do funky stuff to xz if the character is in the air.
    private Vector3 yVelocity;

    //How many degrees difference from the jump angle the pawn can be to maintain momentum
    private const float JUMP_MOMENTUM_TOLERANCE_LOW = 70.0f;

    //Camera Dip params for jumping.
    private const float JUMP_DIP_ANGLE = 3f;
    private const float JUMP_DIP_SMOOTH_TIME = .1f;
    private const float JUMP_DIP_RAISE_TIME = .1f;

    //Camera Dip params for landing.
    private const float LAND_DIP_ANGLE = 3f;
    private const float LAND_DIP_SMOOTH_TIME = .1f;
    private const float LAND_DIP_RAISE_TIME = .1f;

    //Camera Dip params for wall jumping.
    private const float WALLJUMP_DIP_ANGLE = 3f;
    private const float WALLJUMP_DIP_SMOOTH_TIME = .1f;
    private const float WALLJUMP_DIP_RAISE_TIME = .1f;

    //How fast velocity falls off when you land while exceeding your max speed.
    private const float MAX_SPEED_EXCEEDED_FALLOFF = .1f;

    //How much you "Push off" the wall when you wall jump due to the direction of the pawn being "too perpendicular" to the wall you jump off of.
    private const float WALLJUMP_OFF_ANGLE_BONUS_FORCE = 3f;
    //The angle in which the pawn is "too perpendicular" for a reflected walljump.
    private const float WALL_JUMP_OFF_ANGLE_THRESHOLD = 45f;

    //How fast you accelerate in the air.
    private const float AIR_ACCELERATION_Z = 1f;
    private const float AIR_ACCELERATION_X = 1f;
    
    //Flag to see if the pawn was grounded last frame, so we know when to trigger "Landing".
    public bool WasGroundedLastFrame = false;

    //The angle of the pawn when it was last grounded.
    public float LastGroundedFrameAngle;

    //Collision flags from the last time we moved.
    private CollisionFlags lastFrameCollisionFlags;

    private bool initialized = false;
    //                ^-----------This guy!!!1

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        if (!initialized)
            Initialize();

        //This means some other script has taken control of the character, e.g. the script to vault the character.
        if (pawn.MovementLocked)
            return;

        Slide();
        Land();
        MoveXZ();
        MoveY();

        /******************************/
        /*** Finally, Move the pawn ***/
        /******************************/

        //Finally, move the controller.
        lastFrameCollisionFlags = pawn.Move((finalXZVector + yVelocity) * Time.deltaTime);

        if ((lastFrameCollisionFlags & CollisionFlags.CollidedAbove) != 0)
        {
            CurrentYSpeed = GRAVITY_DEFAULT;
        }
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    private void Slide()
    {
        /************************/
        /********Sliding*********/
        /************************/

        //Lets determine if we should slide first
        if (pawn.IsGrounded && pawn.IsCrouching && pawn.CurrentZSpeed > SLIDE_MINIMUM_VELOCITY)
        {
            pawn.IsSliding = true;

            //If we are on an angle and sliding downhill, allow infinite sliding, otherwise add to the drag.
            if (transform.position.y >= slidingLastFrameYCheck && GetPawnSlopeAngle() >= SLIDE_DRAG_ANGLE)
                CurrentZSpeed -= SLIDING_DRAG * Time.deltaTime;
        }
        else //If we aren't sliding, don't slide, LOL! GENIUS!
            pawn.IsSliding = false;

        slidingLastFrameYCheck = transform.position.y;
    }

    //Make sure the pawn loses momentum if they land in a direction they were not facing when they jumped.
    private void Land()
    {

        /************************/
        /********Landing*********/
        /************************/

        //Check to see if we should land, if we are grounded this frame but were not the last.
        if (pawn.IsGrounded)
        {
            if (!WasGroundedLastFrame && !pawn.IsSliding)
            {
                if (CurrentYSpeed < GRAVITY_CAMERA_DIP_THRESHOLD)
                    pawnLook.Dip(LAND_DIP_ANGLE, LAND_DIP_SMOOTH_TIME, LAND_DIP_RAISE_TIME);

                float landDifference = Utils.DifferenceInBetweenTwoAngles(transform.rotation.eulerAngles.y, LastGroundedFrameAngle);

                //2 levels, pawn gets to keep some momentum if they dont stray too far
                if (landDifference > JUMP_MOMENTUM_TOLERANCE_LOW) 
                {
                    CurrentZSpeed = 0f;
                    CurrentXSpeed = 0f;
                }
            }

            WasGroundedLastFrame = true;
            LastGroundedFrameAngle = transform.rotation.eulerAngles.y;
        }
        else
            WasGroundedLastFrame = false;
       
    }

    private void MoveXZ()
    {
        //If the pawns speed goes below the default max speed we remove all speed charges
        if (CurrentZSpeed < MAX_WALK_SPEED_FORWARD)
            pawn.RemoveAllSpeedCharges();

        /*********************************/
        /*** Calculate the X/Z changes ***/
        /*********************************/

        if (pawn.IsGrounded)
        {
            //If we are sliding, remove velocity on Z at a constant rate.
            if (pawn.IsSliding) //Slide script controls the Z speed.
                CurrentXSpeed = 0;
            else
            {
                pawn.RightVector = transform.right;
                pawn.ForwardVector = transform.forward;
                CurrentZSpeed = GetRunningZSpeed();
                CurrentXSpeed = GetRunningXSpeed();
            }
            XZGroundVelocity = (pawn.ForwardVector * CurrentZSpeed) + (pawn.RightVector * CurrentXSpeed);
            XZAirVelocity = Vector3.zero;

            airAccelerationX = 0f;
            airAccelerationZ = 0f;
        }
        else //aka in the air. 
        {
            airAccelerationZ += pawn.PawnInput.ZDirection * AIR_ACCELERATION_Z * Time.deltaTime;
            airAccelerationX += pawn.PawnInput.XDirection * AIR_ACCELERATION_X * Time.deltaTime;
            XZAirVelocity = (transform.forward * airAccelerationZ) + (transform.right * airAccelerationX);
        }

        finalXZVector = XZGroundVelocity + XZAirVelocity;
    }

    private void MoveY()
    {
        /*******************************************/
        /*** Calculate the Y (Jump/Fall) changes ***/
        /*******************************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.IsGrounded)
        {
            //Jump if the user pressed jump this frame, since holding jump will have significance
            if (pawn.PawnInput.JumpedThisFrame)
                Jump();
            else
                CurrentYSpeed = GRAVITY_DEFAULT; //Always ensure we are trying to push the character down due to slopes. 
        }
        /****************************/
        /*********Wall Jump**********/
        /****************************/
        else if (pawn.PawnInput.JumpedThisFrame && CurrentYSpeed >= WALL_JUMP_MINIMUM_VELOCITY)
            WallJump();

        //Apply Gravity
        CurrentYSpeed += Gravity * Time.deltaTime; //Magic Number for now

        //Calculate the Y
        yVelocity = (transform.up * CurrentYSpeed);
    }

    private void Jump()
    {
        pawnLook.Dip(JUMP_DIP_ANGLE, JUMP_DIP_SMOOTH_TIME, JUMP_DIP_RAISE_TIME);
        //If the jump boost sensor isnt colliding, we are near an egde, so lets boost the character!
        if (jumpBoostSensor.CollidedObjects == 0)
        {
            //If we are moving forward or stopped, you get a speed charge and momentum boost
            if (pawn.SpeedCharges < MAX_SPEED_CHARGES && CurrentZSpeed >= 0f)
            {
                pawn.AddSpeedCharge();
                CurrentZSpeed += SPEED_CHARGE_MAX_VELOCITY;
            }
            //Jump with the boost multiplier.
            CurrentYSpeed = JUMP_FORCE + JUMP_BOOST_MULTIPLIER;
        }
        //Or just jump this frame.
        else
            CurrentYSpeed = JUMP_FORCE;
    }

    private void WallJump()
    {
        //Basically, Raycast in front of the player and see if we hit a wall.
        if (Physics.Raycast(jumpBoostSensor.transform.position + (transform.forward * -1f), transform.forward, out RaycastHit hit, 2f, LayerMask.GetMask("MapGeometry")))
        {
            //We hit a wall, two scenarios.
            //IF we are running at a decent angle towards the wall we will perfectly reflect the wall jump
            //In a scenario where we run perpendicular to it, a reflect will not do much, so push the player away from the wall.

            //Validate the current angle we jumped at.
            //IMPROVEMENT - figure out which euler y the vector is traveling instead of locking in the angles at certain times.
            //If we have a difference of less than 45 degrees between the current angle and the angle we jumped at, reflect all velocity perfectly.
            var currentYAngle = transform.rotation.eulerAngles.y;
            pawnLook.Dip(WALLJUMP_DIP_ANGLE, WALLJUMP_DIP_SMOOTH_TIME, WALLJUMP_DIP_RAISE_TIME);
            CurrentYSpeed = JUMP_FORCE;
            pawn.AddVaultLock(.35f);
            XZGroundVelocity = Vector3.Reflect(XZGroundVelocity, hit.normal);

            //If we did some crazy rotation to hit the wall, reflect the angle, normalize it, then add a Vector3 to "push off" the wall instead of a "Bounce".
            //JK also testing adding an additional force and NOT normalizing the initial vector.
            if (Utils.DifferenceInBetweenTwoAngles(currentYAngle, LastGroundedFrameAngle) >= WALL_JUMP_OFF_ANGLE_THRESHOLD)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hit.normal, transform.up);
                Vector3 wallForce = Utils.GenerateDirectionalForceVector(targetRotation, WALLJUMP_OFF_ANGLE_BONUS_FORCE);
                XZGroundVelocity += wallForce;
            }

            //Finally, Re-adjust the current Locked Vectors pointing towards the angle from the wall jump.
            pawn.RightVector = Quaternion.LookRotation(XZGroundVelocity) * Vector3.right;
            pawn.ForwardVector = Quaternion.LookRotation(XZGroundVelocity) * Vector3.forward;
        }
    }


    //Stop all movement on the pawns directions;
    public void HaltMovement(bool haltX, bool haltY, bool haltZ)
    {
        if (haltX)
            CurrentXSpeed = 0f;
        if (haltY)
            CurrentYSpeed = 0f;
        if (haltZ)
            CurrentZSpeed = 0f;
    }

    //Check to see if the player is moving faster than a certain speed;
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return Mathf.Abs(CurrentXSpeed + CurrentZSpeed) > targetVelocity;
    }

    //Quick check to see if any X/Z movement is TRYING to happen.
    public bool IsTryingToMove()
    {
        return pawn.PawnInput.XDirection != 0f || pawn.PawnInput.ZDirection != 0f;
    }

    //Generic Initialization function.
    private void Initialize()
    {
        LastGroundedFrameAngle = transform.rotation.eulerAngles.y;
        initialized = true;
    }
    
    //Determine the maximum speed on the Z axis.
    private float GetRunningZSpeed()
    {
        float maxScriptZSpeed = pawn.IsCrouching && !pawn.IsSliding ? MAX_CROUCH_WALK_SPEED : (MAX_WALK_SPEED_FORWARD + (pawn.SpeedCharges * SPEED_CHARGE_MAX_VELOCITY));

        if (CurrentZSpeed > maxScriptZSpeed)
        {
            CurrentZSpeed -= MAX_SPEED_EXCEEDED_FALLOFF * Time.deltaTime;
            return pawn.PawnInput.ZDirection * CurrentZSpeed;
        }

        return pawn.PawnInput.ZDirection * maxScriptZSpeed;
    }

    //Determine the maximum speed on the X axis.  
    private float GetRunningXSpeed()
    {
        float maxScriptXSpeed = pawn.IsCrouching && !pawn.IsSliding ? MAX_CROUCH_WALK_SPEED : MAX_WALK_SPEED_SIDEWAYS;

        if (CurrentXSpeed > maxScriptXSpeed)
        {
            CurrentXSpeed -= MAX_SPEED_EXCEEDED_FALLOFF * Time.deltaTime;
            return pawn.PawnInput.XDirection * CurrentXSpeed;
        }

        return pawn.PawnInput.XDirection * maxScriptXSpeed;
    }

    //Try to raycast down and see if we are hitting a sloped surface, if we are not, just return 1 and assume we are not.
    private float GetPawnSlopeAngle()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.02f, LayerMask.GetMask("MapGeometry")))
        {
            return hit.normal.y;
        }
        return 1;
    }

}
