using UnityEngine;

//Pawn is a class that represents the avatar which is controlled by the user.
public sealed class Pawn : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/
 
    [SerializeField]
    private PawnInput pawnInput;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private PawnMovement pawnMovement;

    [SerializeField]
    private CharacterController characterController;

    /***************************/
    /* Private class variables */
    /***************************/

    //Holds the anlge of the slope beneath the player.
    private float slopeBeneathPawn = 1.0f;

    //Flag to prevent any logic from executing until after Initialization.
    private bool initialized = false;

    /************************/
    /*** Class properties ***/
    /************************/

    //Flag to indicate that a certain script has control over the pawn and that nothing else should touch it.
    public bool Locked { get; set; }

    //The angle the camera is looking at.
    public float LookAngle { get; set; }

    //How much Drag is on the player, as in the force to slow them down over time.
    public float Drag { get; set; }

    //Quick check to see if the pawn is TRYING to move alonmg x/z due to input.
    public bool IsTryingToMove { get => pawnMovement.IsTryingToMove(); }

    public bool IsOnSlopedSurface { get => slopeBeneathPawn < 1f; }

    /*****************************/
    /*** Additional Properties ***/
    /*****************************/

    //Quick reference for seeing if the Character Controller is grounded.
    public bool IsGrounded { get => characterController.isGrounded; }

    //Quick reference for getting the PawnInput object.
    public PawnInput PawnInput { get => pawnInput; }

    //Quick reference for getting the PawnLook object.
    public PawnLook PawnLook { get => pawnLook; }

    //Quick reference for getting the PawnLook object.
    public PawnMovement PawnMovement { get => pawnMovement; }

    //Quick reference for if the Pawn is moving.
    public bool IsMoving { get => pawnMovement.IsMoving; }

    //Quick reference for if the pawn is crouching.
    public bool IsCrouching { get => pawnInput.Crouching; }

    //Quick reference for if the pawn is sliding. Set by the PawnCrouch script.
    public bool IsSliding { get; set; }

    public float ForwardSpeed { get => pawnMovement.ForwardSpeed; }

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        if (!initialized)
            Initialize();

        calculateSlopeBeneathPawn();
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    //Do any class initializations here.
    private void Initialize()
    {
        Drag = 0f;
        IsSliding = false;
        initialized = true;
    }

    //Sets the slope angle of the surface beneath the player.
    private void calculateSlopeBeneathPawn()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.02f);
        slopeBeneathPawn = hit.normal.y;
    }

    //Stop all movement on the character;
    public void HaltMovement()
    {
        pawnMovement.HaltMovement();
    }

    //Quick reference for moving the pawn's character controller.
    public void Move(Vector3 movementVelocity)
    {
        characterController.Move(movementVelocity);
    }

    //Check to see if the player is moving faster than a certain speed
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return pawnMovement.IsMovingFasterThan(targetVelocity);
    }

}