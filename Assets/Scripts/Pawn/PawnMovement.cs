using UnityEngine;

public sealed class PawnMovement : MonoBehaviour
{
    /*** Local References to Unity Objects ***/

    //Main GameObject controlled by the player.
    [SerializeField]
    private Pawn pawn;

    /*** Class properties ***/
    //None

    /*** Local Class Variables ***/

    ////////Default Upper Thresholds for movement
    /////////////////////////////////////////////

    //How fast the character walks.
    private const float MAX_WALK_SPEED = 5f;

    //How fast the character runs.
    private const float MAX_SPRINT_SPEED = 10f;

    //How hard the character jumps;
    private const float WALK_ACCELERATION = 12f;

    //How hard the character jumps;
    private const float JUMP_FORCE = 10;

    //Current Speed the character is moving forwards or backwards.
    private float currentXZSpeed = 0;

    ////////Movement Vectors 
    ////////////////////////

    //Used to determine X/Z Movement in 3d space based on input.
    private Vector3 inputCalculatedVelocity;

    //Actual calculated velocity after applying physics/gravity.
    private Vector3 movementVelocity;

    /*** Unity Methods ***/

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
        if (pawn.PawnInput.VerticalDirection == 0 && pawn.PawnInput.HorizontalDirection == 0 && pawn.IsGrounded)
            currentXZSpeed = 0f;
        else
            currentXZSpeed += WALK_ACCELERATION * Time.deltaTime;

        //Calculate the velocity based on input.
        inputCalculatedVelocity = Vector3.ClampMagnitude(((transform.forward * pawn.PawnInput.VerticalDirection) + (transform.right * pawn.PawnInput.HorizontalDirection)) * currentXZSpeed, MAX_WALK_SPEED);

        //Take X/Z from the calculated velocity and store in the vector to move the player horizontally.
        movementVelocity.x = inputCalculatedVelocity.x;
        movementVelocity.z = inputCalculatedVelocity.z;

        /*******************************/
        /*** Calculate the Y changes ***/
        /*******************************/

        //If the controller is on the ground already, cancel gravity
        if (pawn.CharacterController.isGrounded)
        {
            movementVelocity.y = 0.0f;

            //Only allow jumping if grounded.
            if (pawn.PawnInput.Jumping)
            {
                movementVelocity.y = JUMP_FORCE;
                movementVelocity.y += Physics.gravity.y * 2.5f * Time.deltaTime;
            }
        }
        else //Just Apply gravity
        {
            movementVelocity.y += Physics.gravity.y * 2.5f * Time.deltaTime;
        }

        //Finally, move the controller.
        pawn.CharacterController.Move(movementVelocity * Time.deltaTime);
    }

    /*** Class Methods ***/

    public void HaltMovement()
    {
        movementVelocity = Vector3.zero;
    }
}