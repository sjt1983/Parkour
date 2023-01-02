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

    //None

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    /*** Default Upper Thresholds for movement ***/    

    //How fast the character walks.
    private const float MAX_WALK_SPEED = 10f;

    //How hard the character jumps;
    private const float WALK_ACCELERATION = 12f;

    //How fast the character stops moving.
    private const float WALK_STOP_FORCE = 20f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;

    //Current Speed the character is moving forwards or backwards.
    private float currentXZSpeed = 0;

    //Last frame directions, used for keeping momentum.
    private float lastFrameHorizontalDirection;
    private float lastFrameVerticalDirection;

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
        if (!pawn.IsTryingToMove && pawn.IsGrounded && !pawn.IsCrouching)
        {           
            if (currentXZSpeed > 0)
                currentXZSpeed -= WALK_STOP_FORCE * Time.deltaTime;
            else
                currentXZSpeed = 0;
        }
        else
        {
            currentXZSpeed += (pawn.IsCrouching ? -pawn.Drag : WALK_ACCELERATION) * Time.deltaTime;
        }

        currentXZSpeed = Mathf.Clamp(currentXZSpeed, 0f, MAX_WALK_SPEED);

        if (pawn.IsGrounded)
        {
            if (!pawn.IsCrouching && pawn.IsTryingToMove)
            {
                //Calculate the velocity based on input.
                inputCalculatedVelocity = Vector3.ClampMagnitude(((transform.forward * pawn.PawnInput.VerticalDirection) + (transform.right * pawn.PawnInput.HorizontalDirection)) * currentXZSpeed, MAX_WALK_SPEED);
                lastFrameVerticalDirection = pawn.PawnInput.VerticalDirection;
                lastFrameHorizontalDirection = pawn.PawnInput.HorizontalDirection;                
            }
            else
            {
                inputCalculatedVelocity = Vector3.ClampMagnitude(((transform.forward * lastFrameVerticalDirection) + (transform.right * lastFrameHorizontalDirection)) * currentXZSpeed, MAX_WALK_SPEED);
            }
        }

        //Take X/Z from the calculated velocity and store in the vector to move the player horizontally.
        movementVelocity.x = inputCalculatedVelocity.x;
        movementVelocity.z = inputCalculatedVelocity.z;

        /*******************************/
        /*** Calculate the Y changes ***/
        /*******************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.CharacterController.isGrounded)
        {
            movementVelocity.y = 0;
            //Only allow jumping if grounded.
            if (pawn.PawnInput.Jumping)
            {
                movementVelocity.y = JUMP_FORCE;
            }
        }
        movementVelocity.y += Physics.gravity.y * 2.5f * Time.deltaTime;

        //Finally, move the controller.
        pawn.CharacterController.Move(movementVelocity * Time.deltaTime);
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    //Stop all movement on the character;
    public void HaltMovement()
    {
        movementVelocity = Vector3.zero;
    }

    //Quick check to see if there is significant movement
    public bool IsMoving { get => Mathf.Abs(currentXZSpeed) > .1f; }

    //Check to see if the player is moving faster than a certain speed;
    public bool IsMovingFasterThan (float targetVelocity)
    {
        return Mathf.Abs(currentXZSpeed) > targetVelocity;
    }

    //Quick check to see if any X/Z movement is TRYING to happen.
    public bool IsTryingToMove()
    {
        return pawn.PawnInput.HorizontalDirection != 0f || pawn.PawnInput.VerticalDirection != 0f;
    }
 }
