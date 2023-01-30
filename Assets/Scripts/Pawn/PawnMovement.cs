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
    private Sensor jumpBoostSensor;

    /************************/
    /*** Class properties ***/
    /************************/

    //How fast we are moving forward.
    public float ForwardSpeed { get => currentZSpeed; }

    //How fast we are moving upward.
    public float UpwardSpeed { get => xzCalculatedVelocity.y; }

    //Quick check to see if there is significant movement
    public bool IsMoving { get => Mathf.Abs(currentXSpeed + currentZSpeed) > .1f; }

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    /*** Default Upper Thresholds for movement ***/

    //How fast the character walks.
    private const float MAX_WALK_SPEED_FORWARD = 7f;
    private const float MAX_WALK_SPEED_SIDEWAYS = 4f;

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

    //A boost to how much gravity is applied to a character, using Unity's Physics.Gravity as a baseline.
    private const float UNITY_GRAVITY_BOOST_MULTIPLIER = 3.5f;

    //How many speed charges the character can have.
    private const int MAX_SPEED_CHARGES = 2;

    //How much of a speed boost the character gets per charge
    private const float SPEED_CHARGE_MAX_VELOCITY = 1f;

    //Current Speed the character is moving in all directions.
    [SerializeField]
    private float currentXSpeed = 0;
    [SerializeField]
    private float currentYSpeed = 0;
    [SerializeField]
    private float currentZSpeed = 0;

    //Velocity to move the pawn.
    private Vector3 xzCalculatedVelocity;

    //We calculate y separately because we need to do funky stuff to xz if the character is in the air.
    private Vector3 yCalculatedVelocity;

    //How many degrees difference from the jump angle the pawn can be to maintain momentum
    private const float JUMP_MOMENTUM_TOLERANCE_LOW = 90.0f;
    private const float JUMP_MOMENTUM_TOLERANCE_HIGH = 160.0f;

    //How much speed we deduce when the pawn lands after the low tolerance.
    private const float LAND_LOW_DEDUCTION = 2;
    //How much speed we deduce when the pawn lands after the high tolerance.
    private const float LAND_HIGH_DEDUCTION = 5;
    //How fast the pawn recovers from the landed deduction.
    private const float LAND_DEDUCTION_RECOVERY_RATE = 3;
    //The current landed deduction from speed.
    private float landedSpeedDeduction = 0f;

    //Flag to see if the pawn was grounded last frame, so we know when to trigger "Landing".
    private bool wasGroundedLastFrame = false;

    //The angle of the pawn when it was last grounded.
    private float lastGroundedFrameAngle;

    private Vector3 lastZForward;
    private Vector3 lastXRight;

    private bool initialized = false;
    //                ^-----------This guy!!!1

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        if (!initialized)
            Initialize();

        UIManager.Instance.DebugText2 = "2: " + pawn.IsSliding.ToString();
        UIManager.Instance.DebugText1 = "1: " + pawn.IsCrouching.ToString();

        //This means some other script has taken control of the character, e.g. the script to vault the character.
        if (pawn.MovementLocked)
            return;

        //Check to see if we should land, if we are grounded this frame but were not the last.
        if (pawn.IsGrounded)
        {   
            if (!wasGroundedLastFrame)
            {
                Land();
            }
            wasGroundedLastFrame = true;
            lastGroundedFrameAngle = transform.rotation.eulerAngles.y;
        }
        else
        {
            wasGroundedLastFrame = false;
        }
        //Recover from landing if needed
        landedSpeedDeduction = Mathf.Clamp(landedSpeedDeduction - (LAND_DEDUCTION_RECOVERY_RATE * Time.deltaTime), 0f, 100);

        //If the pawns speed goes below the default max speed we remove all speed charges
        if (currentZSpeed < MAX_WALK_SPEED_FORWARD)
        {
            pawn.RemoveAllSpeedCharges();
        }

        /*********************************/
        /*** Calculate the X/Z changes ***/
        /*********************************/

        if (pawn.IsGrounded)
        {
            currentZSpeed = pawn.PawnInput.VerticalDirection * GetZSpeed();
            currentXSpeed = pawn.PawnInput.HorizontalDirection * GetXSpeed();
            //If we are sliding, remove velocity on Z at a constant rate.
            if (pawn.IsSliding) //SLide script controls the Z speed.
            {
                currentXSpeed = 0;
            }
            else
            {
                lastXRight = transform.right;
                lastZForward = transform.forward;
            }
            xzCalculatedVelocity = (lastZForward * currentZSpeed) + (lastXRight * currentXSpeed);
            
        }
        else //aka in the air.
        {
            //Nothing for now.
        }

        /*******************************************/
        /*** Calculate the Y (Jump/Fall) changes ***/
        /*******************************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.IsGrounded)
        {           
            //Jump if the user pressed jump this frame, since holding jump will have significance
            if (pawn.PawnInput.JumpedThisFrame)
            {
                //If the jump boost sensor isnt colliding, we are near an egde, so lets boost the character!
                if (jumpBoostSensor.CollidedObjects == 0)
                {
                    //If we are moving forward or stopped, you get a speed charge and momentum boost
                    if (pawn.SpeedCharges < MAX_SPEED_CHARGES && currentZSpeed >= 0f)
                    {
                        pawn.AddSpeedCharge();
                        currentZSpeed += SPEED_CHARGE_MAX_VELOCITY;
                    }
                    //Jump with the boost multiplier.
                    currentYSpeed = JUMP_FORCE + JUMP_BOOST_MULTIPLIER;
                }
                //Or just jump this frame.
                else
                {
                    currentYSpeed = JUMP_FORCE;
                }
            }
            else
            {
                currentYSpeed = GRAVITY_DEFAULT; //Always ensure we are trying to push the character down due to slopes. 
            }
            
        }
        //WALL JUMP
        else if (pawn.PawnInput.JumpedThisFrame && currentYSpeed >= WALL_JUMP_MINIMUM_VELOCITY)
        {
            if (Physics.Raycast(jumpBoostSensor.transform.position + (transform.forward * -1f), transform.forward, out RaycastHit hit, 2f, LayerMask.GetMask("MapGeometry")))
            {
                currentYSpeed = JUMP_FORCE;
                xzCalculatedVelocity = Vector3.Reflect(xzCalculatedVelocity, hit.normal);
                pawn.AddVaultLock(.35f);
            }
        }

        //Apply Gravity
        currentYSpeed += Physics.gravity.y * UNITY_GRAVITY_BOOST_MULTIPLIER * Time.deltaTime;

        yCalculatedVelocity = (transform.up * currentYSpeed);

        /******************************/
        /*** Finally, Move the pawn ***/
        /******************************/

        //Finally, move the controller.
        CollisionFlags flags = pawn.Move((xzCalculatedVelocity + yCalculatedVelocity) * Time.deltaTime);

        if ((flags & CollisionFlags.CollidedAbove) != 0)
        {
            currentYSpeed = GRAVITY_DEFAULT;
        }
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/
    
    //Stop all movement on the pawns directions;
    public void HaltMovement(bool haltX, bool haltY, bool haltZ)
    {
        if (haltX)
            currentXSpeed = 0f;
        if (haltY)
            currentYSpeed = 0f;
        if (haltZ)
            currentZSpeed = 0f;
    }

    //Check to see if the player is moving faster than a certain speed;
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return Mathf.Abs(currentXSpeed + currentZSpeed) > targetVelocity;
    }

    //Quick check to see if any X/Z movement is TRYING to happen.
    public bool IsTryingToMove()
    {
        return pawn.PawnInput.HorizontalDirection != 0f || pawn.PawnInput.VerticalDirection != 0f;
    }

    //Generic Initialization function.
    private void Initialize()
    {
        lastGroundedFrameAngle = transform.rotation.eulerAngles.y;
        initialized = true;
    }

    //Make sure the pawn loses momentum if they land in a direction they were not facing when they jumped.
    private void Land()
    {
        float landDifference = Utils.DifferenceInBetweenTwoAngles(transform.rotation.eulerAngles.y, lastGroundedFrameAngle);
 
        //2 levels, pawn gets to keep some momentum if they dont stray too far
        if (landDifference > JUMP_MOMENTUM_TOLERANCE_LOW && landDifference < JUMP_MOMENTUM_TOLERANCE_HIGH) 
        {
            landedSpeedDeduction = LAND_LOW_DEDUCTION;
        }
        else if (landDifference >= JUMP_MOMENTUM_TOLERANCE_HIGH)
        {
            landedSpeedDeduction = LAND_HIGH_DEDUCTION;
        }
    }

    //Determine the maximum speed on the Z axis.
    private float GetZSpeed()
    {
        if (pawn.OverrideZSpeed != -1)
            return pawn.OverrideZSpeed;

        return pawn.IsCrouching && !pawn.IsSliding ? MAX_CROUCH_WALK_SPEED : (MAX_WALK_SPEED_FORWARD + (pawn.SpeedCharges * SPEED_CHARGE_MAX_VELOCITY)) - landedSpeedDeduction;
    }


    //Determine the maximum speed on the X axis.  
    private float GetXSpeed()
    {
        if (pawn.OverrideXSpeed != -1)
            return pawn.OverrideXSpeed;

        return pawn.IsCrouching && !pawn.IsSliding ? MAX_CROUCH_WALK_SPEED : MAX_WALK_SPEED_SIDEWAYS;
    }

 }
