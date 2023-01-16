using UnityEngine;
using System.Collections.Generic;

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
    public float UpwardSpeed { get => inputCalculatedVelocity.y; }

    //Quick check to see if there is significant movement
    public bool IsMoving { get => Mathf.Abs(currentXSpeed + currentZSpeed) > .1f; }

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    /*** Default Upper Thresholds for movement ***/

    //How fast the character walks.
    private const float MAX_WALK_SPEED_FORWARD = 5f;
    private const float MAX_WALK_SPEED_SIDEWAYS = 3f;

    //How fast the character walks when crouched.
    private const float MAX_CROUCH_WALK_SPEED = 1f;

    //How fast the character accelerates when the input matches the direction
    private const float WALK_ACCELERATION = 13f;

    //How fast the character accelerates when NOT the input matches the direction
    private const float WALK_ACCELERATION_ABRUPT = 26f;

    //How fast the character stops moving.
    private const float WALK_STOP_FORCE = 20f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;
    private const float STOP_ZERO_THRESHOLD = .3f;

    //How extra hard the character jumps while boosted
    private const float JUMP_BOOST_MULTIPLIER = 1f;

    //Minimum vertical veelocity needed to wall jump
    private const float WALL_JUMP_MINIMUM_VELOCITY = -.25f;

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
    private Vector3 inputCalculatedVelocity;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        //This means some other script has taken control of the character, e.g. the script to vault the character.
        if (pawn.MovementLocked)
            return;

        UIManager.Instance.DebugText1 = pawn.IsGrounded.ToString();
        UIManager.Instance.DebugText2 = currentZSpeed.ToString();
        UIManager.Instance.DebugText3 = jumpBoostSensor.CollidedObjects.ToString();

        /*********************************/
        /*** Calculate the X/Z changes ***/
        /*********************************/

        //Determine how fast we should be going.

        //If we are grounded, do the thing.
        if (pawn.IsGrounded)
        {
            /****************************/
            /***** Z - Forward/Back *****/
            /****************************/

            //If we are sliding, remove velocity on Z at a constant rate.
            if (pawn.IsSliding)
            {
                currentZSpeed += (currentZSpeed < 0 ? pawn.Drag : -pawn.Drag) * Time.deltaTime;                
            }
            //If we are not sliding, and there is no input on this axis, stop the character.
            else if (pawn.PawnInput.VerticalDirection == 0f)
            {
                //Slow the character down until they hit the threshold then stop all Z movement.
                if (currentZSpeed > STOP_ZERO_THRESHOLD)
                    currentZSpeed -= WALK_STOP_FORCE * Time.deltaTime;
                else if (currentZSpeed < -STOP_ZERO_THRESHOLD)
                    currentZSpeed += WALK_STOP_FORCE * Time.deltaTime;
                else
                    currentZSpeed = 0f;
            }            
            else
            {
                //Is my Input the same direction I am going, e.g. I am already moving forward and holding forward.
                //if so, accelerate slowly, else, accelerate in the opposite direction abruptly
                //Forward/back
                if ((pawn.PawnInput.VerticalDirection > 0 && currentZSpeed > 0) || (pawn.PawnInput.VerticalDirection < 0 && currentZSpeed < 0))
                    currentZSpeed += (WALK_ACCELERATION) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
                else
                    currentZSpeed += (WALK_ACCELERATION_ABRUPT) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
            }

            /**********************/
            /***** X - Strafe *****/
            /**********************/
            //This is the same logic as above for Z.

            if (pawn.IsSliding)
            {
                currentXSpeed += (currentXSpeed < 0 ? pawn.Drag : -pawn.Drag) * Time.deltaTime;
            }
            else if (pawn.PawnInput.HorizontalDirection == 0f)
            {
                if (currentXSpeed > STOP_ZERO_THRESHOLD)
                    currentXSpeed -= WALK_STOP_FORCE * Time.deltaTime;
                else if (currentXSpeed < -STOP_ZERO_THRESHOLD)
                    currentXSpeed += WALK_STOP_FORCE * Time.deltaTime;
                else
                    currentXSpeed = 0f;
            }
            else
            {
                //Is my Input the same direction I am going, e.g. I am already moving forward and holding forward.
                //if so, accelerate slowly, else, accelerate in the opposite direction abruptly
                //Now for left/right
                if ((pawn.PawnInput.HorizontalDirection > 0 && currentXSpeed > 0) || (pawn.PawnInput.HorizontalDirection < 0 && currentXSpeed < 0))
                    currentXSpeed += (WALK_ACCELERATION) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
                else
                    currentXSpeed += (WALK_ACCELERATION_ABRUPT) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
            }
        }

        //If the pawns speed goes below the default max speed we remove all speed charges
        if (currentZSpeed < MAX_WALK_SPEED_FORWARD)
        {
            pawn.RemoveAllSpeedCharges();
        }

        //Clamp how we need to
        if (pawn.IsGrounded && pawn.IsCrouching && !pawn.IsSliding)
        {
            currentZSpeed = Mathf.Clamp(currentZSpeed, -MAX_CROUCH_WALK_SPEED, MAX_CROUCH_WALK_SPEED);
            currentXSpeed = Mathf.Clamp(currentXSpeed, -MAX_CROUCH_WALK_SPEED, MAX_CROUCH_WALK_SPEED);
        }
        else
        {
            currentZSpeed = Mathf.Clamp(currentZSpeed, -MAX_WALK_SPEED_FORWARD, MAX_WALK_SPEED_FORWARD + (currentZSpeed <= 0 ? 0 : (pawn.SpeedCharges * SPEED_CHARGE_MAX_VELOCITY)));
            currentXSpeed = Mathf.Clamp(currentXSpeed, -MAX_WALK_SPEED_SIDEWAYS, MAX_WALK_SPEED_SIDEWAYS);
        }

        /*******************************/
        /*** Calculate the Y (Jump/Fall) changes ***/
        /*******************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.IsGrounded)
        {
            currentYSpeed = GRAVITY_DEFAULT; //Always ensure we are trying to push the character down due to slopes.
            //Jump if the user pressed jump this frame, since holding jump will have significance
            if (pawn.PawnInput.JumpedThisFrame)
            {
                pawn.AddVaultLock(.35f);
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
        }
        //WALL JUMP
        else if(pawn.PawnInput.JumpedThisFrame && currentYSpeed >= WALL_JUMP_MINIMUM_VELOCITY && jumpBoostSensor.CollidedObjects == 1)
        {
            currentYSpeed = JUMP_FORCE;
            currentZSpeed *= -1;
            pawn.AddVaultLock(.35f);
            Debug.Log("1");
        }

        //Apply Gravity
        currentYSpeed += Physics.gravity.y * UNITY_GRAVITY_BOOST_MULTIPLIER * Time.deltaTime;

        //Now that we have all that done, calculate the velocity.
        inputCalculatedVelocity = (transform.forward * currentZSpeed) + (transform.right * currentXSpeed) + (transform.up * currentYSpeed);

        //Finally, move the controller.
        CollisionFlags flags = pawn.Move(inputCalculatedVelocity * Time.deltaTime);
      
        if ((flags & CollisionFlags.CollidedAbove) != 0)
        {
            currentYSpeed = GRAVITY_DEFAULT;
        }

    }

    /*********************/
    /*** Class Methods ***/
    /*********************/
    
    //Stop all movement on the character;
    public void HaltMovement()
    {
        inputCalculatedVelocity = Vector3.zero;
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
 }
