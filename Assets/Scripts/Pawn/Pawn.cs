using UnityEngine;

//Pawn is a class that represents the avatar which is controlled by the user.
public sealed class Pawn : MonoBehaviour
{
    /*** Local References to Unity Objects ***/
    [SerializeField]
    private PawnInput pawnInput;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private PawnMovement pawnMovement;

    [SerializeField]
    private CharacterController characterController;

    /*** Class properties ***/

    //Flag to indicate that a certain script has control over the pawn and that nothing else should touch it.
    public bool Locked { get; set; }
    /*** Additional Properties ***/

    //Quick reference for seeing if the Character Controller is grounded.
    public bool IsGrounded { get { return characterController.isGrounded; } }

    //Quick reference for moving the pawn's character controller.
    public void Move(Vector3 movementVelocity)
    {
        characterController.Move(movementVelocity * Time.deltaTime);
    }

    //Quick reference for getting the PawnInput object.
    public PawnInput PawnInput { get => pawnInput; }

    //Quick reference for getting the PawnLook object.
    public PawnLook PawnLook { get => pawnLook; }

    //Quick reference for getting the PawnLook object.
    public PawnMovement PawnMovement { get => pawnMovement; }

    public CharacterController CharacterController { get => characterController; }

    //Flag to prevent any logic from executing until after Initialization.
    private bool initialized = false;

    /*** Unity Methods ***/

    public void Update()
    {
        if (!initialized)
            Initialize();
    }

    /*** Class Methods ***/

    //Do any class initializations here.
    private void Initialize()
    {
        initialized = true;
    }

    public void HaltMovement()
    {
        pawnMovement.HaltMovement();
    }
  
}