using UnityEngine;

public sealed class PawnMovement : MonoBehaviour
{

    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    //Main GameObject controlled by the player.
    [SerializeField]
    private Pawn pawn;

    /************************/
    /*** Class properties ***/
    /************************/

    //How fast we are moving forward.
    public float ForwardSpeed { get => currentZSpeed; }

    //How fast we are moving upward.
    public float UpwardSpeed { get => movementVelocity.y; }

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

    //How hard the character jumps;
    private const float WALK_ACCELERATION = 13f;

    //How fast the character stops moving.
    private const float WALK_STOP_FORCE = 20f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;

    //Current Speed the character is moving forwards or backwards.
    [SerializeField]
    private float currentZSpeed = 0;
    [SerializeField]
    private float currentXSpeed = 0;

    /*** Movement Vectors ***/

    //Used to determine X/Z Movement in 3d space based on input.
    private Vector3 inputCalculatedVelocity;

    //Actual calculated velocity after applying physics.
    private Vector3 movementVelocity;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        //This means some other script has taken control of the character, e.g. the script to vault the character.
        if (pawn.Locked)
            return;

        /*********************************/
        /*** Calculate the X/Z changes ***/
        /*********************************/

        //Determine how fast we should be going.
        //If both vectors are zero, reset speed.

        //If there is no input and the pawn is grounded, there is no speed.
        if (pawn.IsGrounded)
        {
            if (pawn.IsSliding)
            {
                currentZSpeed += (currentZSpeed < 0 ? pawn.Drag : -pawn.Drag) * Time.deltaTime;                
            }
            else if (pawn.PawnInput.VerticalDirection == 0f)
            {
                if (currentZSpeed > .3f)
                {
                    currentZSpeed -= WALK_STOP_FORCE * Time.deltaTime;
                }
                else if (currentZSpeed < -.3f)
                {
                    currentZSpeed += WALK_STOP_FORCE * Time.deltaTime;
                }
                else
                {
                    currentZSpeed = 0f;
                }
            }            
            else
            {

                //Is my Input the same direction I am going, e.g. I am already moving forward and holding forward.
                //if so, accelerate slowly, else, accelerate in the opposite direction abruptly
                //Forward/back
                if ((pawn.PawnInput.VerticalDirection > 0 && currentZSpeed > 0) || (pawn.PawnInput.VerticalDirection < 0 && currentZSpeed < 0))
                {
                    currentZSpeed += (WALK_ACCELERATION) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
                }
                else
                {
                    currentZSpeed += (WALK_ACCELERATION * 2) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
                }
            }

            if (pawn.IsSliding)
            {
                currentXSpeed += (currentXSpeed < 0 ? pawn.Drag : -pawn.Drag) * Time.deltaTime;
            }
            else if (pawn.PawnInput.HorizontalDirection == 0f)
            {
                if (currentXSpeed > .3f)
                {
                    currentXSpeed -= WALK_STOP_FORCE * Time.deltaTime;
                }
                else if (currentXSpeed < -.3f)
                {
                    currentXSpeed += WALK_STOP_FORCE * Time.deltaTime;
                }
                else
                {
                    currentXSpeed = 0f;
                }
            }
            else
            {
                //Is my Input the same direction I am going, e.g. I am already moving forward and holding forward.
                //if so, accelerate slowly, else, accelerate in the opposite direction abruptly
                //Now for left/right
                if ((pawn.PawnInput.HorizontalDirection > 0 && currentXSpeed > 0) || (pawn.PawnInput.HorizontalDirection < 0 && currentXSpeed < 0))
                {
                    currentXSpeed += (WALK_ACCELERATION) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
                }
                else
                {
                    currentXSpeed += (WALK_ACCELERATION * 2) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
                }
            }
        }


        if (pawn.IsCrouching && !pawn.IsSliding)
        {
            currentZSpeed = Mathf.Clamp(currentZSpeed, -MAX_CROUCH_WALK_SPEED, MAX_CROUCH_WALK_SPEED);
            currentXSpeed = Mathf.Clamp(currentXSpeed, -MAX_CROUCH_WALK_SPEED, MAX_CROUCH_WALK_SPEED);
        }
        else
        {
            currentZSpeed = Mathf.Clamp(currentZSpeed, -MAX_WALK_SPEED_FORWARD, MAX_WALK_SPEED_FORWARD);
            currentXSpeed = Mathf.Clamp(currentXSpeed, -MAX_WALK_SPEED_SIDEWAYS, MAX_WALK_SPEED_SIDEWAYS);
        }

        if (pawn.IsGrounded)
        { 
            inputCalculatedVelocity = Vector3.ClampMagnitude(transform.forward * currentZSpeed + (transform.right * currentXSpeed), MAX_WALK_SPEED_FORWARD);
        }

        //Take X/Z from the calculated velocity and store in the vector to move the player horizontally.
        movementVelocity.x = inputCalculatedVelocity.x;
        movementVelocity.z = inputCalculatedVelocity.z;

        /*******************************/
        /*** Calculate the Y changes ***/
        /*******************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.IsGrounded)
        {
            movementVelocity.y = -5; //Always ensure we are trying to push the character down due to slopes.
            //Only allow jumping if grounded.
            if (pawn.PawnInput.Jumping)
            {
                movementVelocity.y = JUMP_FORCE;
            }
        }
        movementVelocity.y += Physics.gravity.y * 3.5f * Time.deltaTime;

        //Finally, move the controller.
        pawn.Move(movementVelocity * Time.deltaTime);
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    //Stop all movement on the character;
    public void HaltMovement()
    {
        movementVelocity = Vector3.zero;
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
