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

    /************************/
    /*** Class properties ***/
    /************************/

    //Flag to indicate that a certain script has control over the pawn and that nothing else should touch it.
    public bool Locked { get; set; }

    public float LookAngle { get; set; }

    public float Drag { get; set; }

    /*****************************/
    /*** Additional Properties ***/
    /*****************************/

    //Quick reference for seeing if the Character Controller is grounded.
    public bool IsGrounded { get => groundedTimer > 0; }

    public void UnGround()
    {
        groundedTimer = 0f;
    }

    //Quick reference for moving the pawn's character controller.
    public void Move(Vector3 movementVelocity)
    {
        characterController.Move(movementVelocity);
    }

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

    private float groundedTimer = 0f;

    //Flag to prevent any logic from executing until after Initialization.
    private bool initialized = false;

    /*** Unity Methods ***/

    public void Update()
    {
        if (!initialized)
            Initialize();

        if (characterController.isGrounded)
        {
            // cooldown interval to allow reliable jumping even whem coming down ramps
            groundedTimer = 0.25f;
        }
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }

    }

    /*** Class Methods ***/

    //Do any class initializations here.
    private void Initialize()
    {
        Drag = 0f;
        IsSliding = false;
        initialized = true;
    }

    //Stop all movement on the character;
    public void HaltMovement()
    {
        pawnMovement.HaltMovement();
    }

    //Quick check to see if the pawn is TRYING to move alonmg x/z due to input.
    public bool IsTryingToMove { get => pawnMovement.IsTryingToMove(); }

    //Check to see if the player is moving faster than a certain speed;t
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return pawnMovement.IsMovingFasterThan(targetVelocity);
    }

}