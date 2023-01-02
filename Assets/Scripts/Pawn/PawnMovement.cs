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
    private const float MAX_WALK_SPEED_FORWARD = 10f;
    private const float MAX_WALK_SPEED_SIDEWAYS = 5f;

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
        if (pawn.IsGrounded && !pawn.IsCrouching)
        {
            if (pawn.PawnInput.VerticalDirection == 0f)
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
                //Forward/back first.
                if ((pawn.PawnInput.VerticalDirection > 0 && currentZSpeed > 0) || (pawn.PawnInput.VerticalDirection < 0 && currentZSpeed < 0))
                {
                    currentZSpeed += (pawn.IsCrouching ? -pawn.Drag : WALK_ACCELERATION) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
                }
                else
                {
                    currentZSpeed += (pawn.IsCrouching ? -pawn.Drag : WALK_ACCELERATION * 2) * pawn.PawnInput.VerticalDirection * Time.deltaTime;
                }
            }

            if (pawn.PawnInput.HorizontalDirection == 0f)
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
                //Now for left/right
                if ((pawn.PawnInput.HorizontalDirection > 0 && currentXSpeed > 0) || (pawn.PawnInput.HorizontalDirection < 0 && currentXSpeed < 0))
                {
                    currentXSpeed += (pawn.IsCrouching ? -pawn.Drag : WALK_ACCELERATION) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
                }
                else
                {
                    currentXSpeed += (pawn.IsCrouching ? -pawn.Drag : WALK_ACCELERATION * 2) * pawn.PawnInput.HorizontalDirection * Time.deltaTime;
                }
            }

        }

        UIManager.Instance.DebugText1 = pawn.PawnInput.VerticalDirection.ToString();
        UIManager.Instance.DebugText2 = pawn.PawnInput.HorizontalDirection.ToString();

        currentZSpeed = Mathf.Clamp(currentZSpeed, -MAX_WALK_SPEED_FORWARD, MAX_WALK_SPEED_FORWARD);
        currentXSpeed = Mathf.Clamp(currentXSpeed , -MAX_WALK_SPEED_SIDEWAYS, MAX_WALK_SPEED_SIDEWAYS);

        if (pawn.IsGrounded)
        {
            if (!pawn.IsCrouching && pawn.IsTryingToMove)
            {
                //Calculate the velocity based on input.
                inputCalculatedVelocity = Vector3.ClampMagnitude(transform.forward * currentZSpeed + (transform.right  * currentXSpeed), MAX_WALK_SPEED_FORWARD);
                lastFrameVerticalDirection = pawn.PawnInput.VerticalDirection;
                lastFrameHorizontalDirection = pawn.PawnInput.HorizontalDirection;                
            }
            else
            {
                inputCalculatedVelocity = Vector3.ClampMagnitude(transform.forward * currentZSpeed + (transform.right * currentXSpeed), MAX_WALK_SPEED_FORWARD);
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
    public bool IsMoving { get => Mathf.Abs(currentXSpeed + currentZSpeed) > .1f; }

    //Check to see if the player is moving faster than a certain speed;
    public bool IsMovingFasterThan (float targetVelocity)
    {
        return Mathf.Abs(currentXSpeed + currentZSpeed) > targetVelocity;
    }

    //Quick check to see if any X/Z movement is TRYING to happen.
    public bool IsTryingToMove()
    {
        return pawn.PawnInput.HorizontalDirection != 0f || pawn.PawnInput.VerticalDirection != 0f;
    }
 }
