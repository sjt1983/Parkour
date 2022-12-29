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
    private const float MAX_WALK_SPEED = 5;

    //How fast the character runs.
    private const float MAX_SPRINT_SPEED = 10;

    //How hard the character jumps;
    private const float MAX_JUMP_SPEED = 10;

    //Current Speed the character is moving forwards or backwards.
    private float currentSpeed = 0;

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

        currentSpeed = 5f;

        //Calculate the velocity based on input.
        inputCalculatedVelocity = Vector3.ClampMagnitude(((transform.forward * pawn.PawnInput.VerticalDirection) + (transform.right * pawn.PawnInput.HorizontalDirection)) * currentSpeed, MAX_SPRINT_SPEED);

        //Take X/Z from the calculated velocity and store in the vector to move the player horizontally.
        movementVelocity.x = inputCalculatedVelocity.x;
        movementVelocity.z = inputCalculatedVelocity.z;

        //Calculate the Y;
        //If the controller is on the ground already, cancel gravity
        if (pawn.CharacterController.isGrounded)
        {
            movementVelocity.y = 0.0f;

            //Only allow jumping if grounded.
            if (pawn.PawnInput.Jumping)
            {
                movementVelocity.y = MAX_JUMP_SPEED;
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
}