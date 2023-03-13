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
    public float ForwardSpeed { get => CurrentGroundedZSpeed; }

    //How fast we are moving upward.
    public float UpwardSpeed { get => CurrentYSpeed; }

    //Quick check to see if there is significant movement
    public bool IsMoving { get => Mathf.Abs(CurrentGroundedXSpeed + CurrentGroundedZSpeed) > .1f; }

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    /*** Default Upper Thresholds for movement ***/

    //How fast the character walks.
    private const float MAX_WALK_SPEED_FORWARD = 9f;
    private const float MAX_WALK_SPEED_SIDEWAYS = 7f;

    //How fast the character walks when crouched.
    private const float MAX_CROUCH_WALK_SPEED = 2f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;

    //How extra hard the character jumps while boosted
    private const float JUMP_BOOST_MULTIPLIER = 1f;

    //Minimum vertical velocity needed to wall jump
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
    private const float SLIDING_DRAG = 2.5f;

    //How fast we need to be going to slide.
    private const float SLIDE_MINIMUM_VELOCITY = 2f;

    //Minimum Angle to slide
    private const float SLIDE_DRAG_ANGLE = 1f;

    //Time and timer to determine if we spply drag while sliding.
    private const float SLIDE_TIME = 1.5f;
    private float slideTimer = SLIDE_TIME;

    //How much to set the sliding falloff penalty to.
    private const float SLIDING_MOVEMENT_PENALTY = .1f;

    //How fast the slide timer should recover in "seconds per second".
    //e.g. 1 second of slide time takes two seconds to recover.
    private const float SLIDE_TIMER_RECOVERY_RATE = .5f;

    //Y positon of the last frame, if its higher than this frame, we can state we are sliding downhill.
    private float slidingLastFrameYCheck;

    //Flag to see if the pawn was sliding last frame, so we know when to trigger "Landing" if they stop sliding at a wildly different angle.
    public bool WasSlidingLastFrame = false;
    //The angle of the pawn when it was last grounded when it started sliding.
    public float LastSlidingFrameAngle;

    //Current Speed the character is moving in all directions.
    public float CurrentGroundedXSpeed = 0;
    public float CurrentGroundedZSpeed = 0;
    public float CurrentYSpeed = 0;

    //Velocity to move the pawn based on being grounded.
    public Vector3 XZGroundVelocity = Vector3.zero;

    //Velocity to move the pawn based on sliding.
    public Vector3 XZSlideVelocity = Vector3.zero;

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
    private const float WALLJUMP_OFF_ANGLE_THRESHOLD = 45f;

    //Multiplier used to indicate how much the character slows down when they do something to kill their momentum.
    //Examples are landing 180 from the angle which you jumped, or sliding in one direction, 180 then stand up.
    private float movementPenalty = 1f;

    //How much the penalty recovers per second.
    private const float LANDING_RECOVERY_RATE_PER_SECOND = .75f;
    //How much to set the landing falloff penalty to.
    private const float LANDING_MOVEMENT_PENALTY = .5f;

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

        //We want to ensure the pawn total magnitude along X/Z is greater than the threshold needed for sliding.
        float TotalMagnitude = finalXZVector.magnitude;

        //Lets determine if we should slide first
        if (pawn.IsGrounded && pawn.IsCrouching && TotalMagnitude > SLIDE_MINIMUM_VELOCITY)
        {
            if (!WasSlidingLastFrame)
                LastSlidingFrameAngle = transform.rotation.eulerAngles.y;
            
            pawn.IsSliding = true;
            slideTimer -= Time.deltaTime;
            WasSlidingLastFrame = true;
            //If we are on an angle and sliding downhill, allow infinite sliding, otherwise add to the drag.
            if (slideTimer <= 0 && transform.position.y >= slidingLastFrameYCheck && GetPawnSlopeAngle() >= SLIDE_DRAG_ANGLE)
            {
                CurrentGroundedZSpeed -= (CurrentGroundedZSpeed * SLIDING_DRAG * Time.deltaTime);
                CurrentGroundedXSpeed -= (CurrentGroundedXSpeed * SLIDING_DRAG * Time.deltaTime);
            }
        }
        else
        {
         
            //We dont want someone to be able to 180 while sliding and keep full momentum.
            if (WasSlidingLastFrame)
            {
                float slideAngleDifference = ParkourUtils.DifferenceInBetweenTwoAngles(transform.rotation.eulerAngles.y, LastSlidingFrameAngle);

                if (slideAngleDifference > JUMP_MOMENTUM_TOLERANCE_LOW)                
                    movementPenalty = SLIDING_MOVEMENT_PENALTY;
            }            

            //If we aren't sliding, don't slide, LOL! GENIUS!
            pawn.IsSliding = false;
            WasSlidingLastFrame = false;

            //Recover the slide timer.
            slideTimer = Mathf.Clamp(slideTimer + Time.deltaTime * SLIDE_TIMER_RECOVERY_RATE, 0, SLIDE_TIME);
        }

        //Set the current Y position for the frame
        slidingLastFrameYCheck = transform.position.y;
    }

    //Make sure the pawn loses momentum if they land in a direction they were not facing when they jumped.
    private void Land()
    {
        /************************/
        /********Landing*********/
        /************************/

        //Landing Falloff needs to be adjusted up.
        movementPenalty = Mathf.Clamp(movementPenalty + LANDING_RECOVERY_RATE_PER_SECOND * Time.deltaTime, 0, 1);

        //Check to see if we should land, if we are grounded this frame but were not the last, and are not sliding.
        if (pawn.IsGrounded)
        {
            if (!WasGroundedLastFrame && !pawn.IsSliding)
            {
                if (CurrentYSpeed < GRAVITY_CAMERA_DIP_THRESHOLD)
                    pawnLook.DipCamera(LAND_DIP_ANGLE, LAND_DIP_SMOOTH_TIME, LAND_DIP_RAISE_TIME);

                float landDifference = ParkourUtils.DifferenceInBetweenTwoAngles(transform.rotation.eulerAngles.y, LastGroundedFrameAngle);

                if (landDifference > JUMP_MOMENTUM_TOLERANCE_LOW) 
                    movementPenalty = LANDING_MOVEMENT_PENALTY;

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
        if (CurrentGroundedZSpeed < MAX_WALK_SPEED_FORWARD)
            pawn.RemoveAllSpeedCharges();

        /*********************************/
        /*** Calculate the X/Z changes ***/
        /*********************************/

        if (pawn.IsGrounded)
        {
            XZAirVelocity = Vector3.zero;
            //If we are sliding, remove velocity on Z at a constant rate.
            if (pawn.IsSliding) //Slide script controls the Z speed.
            {
                XZSlideVelocity = (transform.right * GetSlidingXSpeed()) + (transform.forward * GetSlidingZSpeed());
                XZGroundVelocity = (pawn.ForwardVector * CurrentGroundedZSpeed) + (pawn.RightVector * CurrentGroundedXSpeed);
            }
            else
            {
                XZSlideVelocity = Vector3.zero;
                pawn.RightVector = transform.right;
                pawn.ForwardVector = transform.forward;
                CurrentGroundedZSpeed = GetRunningZSpeed() * movementPenalty;
                CurrentGroundedXSpeed = GetRunningXSpeed() * movementPenalty;
                XZGroundVelocity = (pawn.ForwardVector * CurrentGroundedZSpeed) + (pawn.RightVector * CurrentGroundedXSpeed);
            }            
        }
        else //aka in the air. 
        {
            XZSlideVelocity = Vector3.zero;
            XZSlideVelocity = (transform.right * GetAirXSpeed()) + (transform.forward * GetAirZSpeed());
        }

        finalXZVector = XZGroundVelocity + XZAirVelocity + XZSlideVelocity;
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
            if (pawn.PawnInput.JumpPressedThisFrame)
                Jump();
            else
                CurrentYSpeed = GRAVITY_DEFAULT; //Always ensure we are trying to push the character down due to slopes. 
        }
        /****************************/
        /*********Wall Jump**********/
        /****************************/
        else if (pawn.PawnInput.JumpPressedThisFrame && CurrentYSpeed >= WALL_JUMP_MINIMUM_VELOCITY)
            WallJump();

        //Apply Gravity
        CurrentYSpeed += Gravity * Time.deltaTime; //Magic Number for now

        //Calculate the Y
        yVelocity = (transform.up * CurrentYSpeed);
    }

    private void Jump()
    {
        pawnLook.DipCamera(JUMP_DIP_ANGLE, JUMP_DIP_SMOOTH_TIME, JUMP_DIP_RAISE_TIME);
        //If the jump boost sensor isnt colliding, we are near an egde, so lets boost the character!
        if (jumpBoostSensor.CollidedObjects == 0)
        {
            //If we are moving forward or stopped, you get a speed charge and momentum boost
            if (pawn.SpeedCharges < MAX_SPEED_CHARGES && CurrentGroundedZSpeed >= 0f)
            {
                pawn.AddSpeedCharge();
                CurrentGroundedZSpeed += SPEED_CHARGE_MAX_VELOCITY;
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
            pawnLook.DipCamera(WALLJUMP_DIP_ANGLE, WALLJUMP_DIP_SMOOTH_TIME, WALLJUMP_DIP_RAISE_TIME);
            CurrentYSpeed = JUMP_FORCE;
            pawn.AddVaultLock(.35f);
            XZGroundVelocity = Vector3.Reflect(XZGroundVelocity, hit.normal);

            //If we did some crazy rotation to hit the wall, reflect the angle, normalize it, then add a Vector3 to "push off" the wall instead of a "Bounce".
            //JK also testing adding an additional force and NOT normalizing the initial vector.
            if (ParkourUtils.DifferenceInBetweenTwoAngles(currentYAngle, LastGroundedFrameAngle) >= WALLJUMP_OFF_ANGLE_THRESHOLD)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hit.normal, transform.up);
                Vector3 wallForce = ParkourUtils.GenerateDirectionalForceVector(targetRotation, WALLJUMP_OFF_ANGLE_BONUS_FORCE);
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
            CurrentGroundedXSpeed = 0f;
        if (haltY)
            CurrentYSpeed = 0f;
        if (haltZ)
            CurrentGroundedZSpeed = 0f;
    }

    //Check to see if the player is moving faster than a certain speed;
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return Mathf.Abs(CurrentGroundedXSpeed + CurrentGroundedZSpeed) > targetVelocity;
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

        if (CurrentGroundedZSpeed > maxScriptZSpeed)
        {
            CurrentGroundedZSpeed -= MAX_SPEED_EXCEEDED_FALLOFF * Time.deltaTime;
            return pawn.PawnInput.ZDirection * CurrentGroundedZSpeed;
        }

        return pawn.PawnInput.ZDirection * maxScriptZSpeed;
    }

    //Determine the maximum speed on the X axis.  
    private float GetRunningXSpeed()
    {
        float maxScriptXSpeed = pawn.IsCrouching && !pawn.IsSliding ? MAX_CROUCH_WALK_SPEED : MAX_WALK_SPEED_SIDEWAYS;

        if (CurrentGroundedXSpeed > maxScriptXSpeed)
        {
            CurrentGroundedXSpeed -= MAX_SPEED_EXCEEDED_FALLOFF * Time.deltaTime;
            return pawn.PawnInput.XDirection * CurrentGroundedXSpeed;
        }

        return pawn.PawnInput.XDirection * maxScriptXSpeed;
    }

    private float GetSlidingZSpeed()
    {
        return pawn.PawnInput.ZDirection * 1f;
    }

    //Determine the maximum speed on the X axis.  
    private float GetSlidingXSpeed()
    {
        return pawn.PawnInput.XDirection * 1f;
    }

    private float GetAirZSpeed()
    {
        return pawn.PawnInput.ZDirection * 1f;
    }

    //Determine the maximum speed on the X axis.  
    private float GetAirXSpeed()
    {
        return pawn.PawnInput.XDirection * 1f;
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
