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

    private const float ACTION_LOCK_MINIMUM_TIME = .25f;

    //Flag to prevent any logic from executing until after Initialization.
    private bool initialized = false;

    public float VaultLockTimer { set; get; }

    /************************/
    /*** Class properties ***/
    /************************/

    //Flag to indicate that a certain script has control over the pawn and that nothing else should touch it.
    public bool MovementLocked { get; set; }

    //The angle the camera is looking at.
    public float LookAngle { get; set; }

    //Quick check to see if the pawn is TRYING to move along x/z due to input.
    public bool IsTryingToMove { get => pawnMovement.IsTryingToMove(); }

    //How many Speed Charges the pawn has, which increases max velocity
    public int SpeedCharges { get; private set; }

    public void AddSpeedCharge()
    {
        SpeedCharges += 1;
    }

    public void RemoveAllSpeedCharges()
    {
        SpeedCharges = 0;
    }

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

    //Quick reference for if the pawn is sliding. Set by the PawnCrouch script.
    public bool IsFalling { get => pawnMovement.UpwardSpeed < 0f; }

    //Quick Reference for Forward Speed
    public float CurrentZSpeed { get => pawnMovement.ForwardSpeed; }

    //Quick Reference for Upward Speed.
    public float UpwardSpeed { get => pawnMovement.UpwardSpeed; }

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        if (!initialized)
            Initialize();

        //Deduct the Vault Lock timer on the pawn.
        VaultLockTimer = Mathf.Clamp(VaultLockTimer - Time.deltaTime, 0, 555);
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    //Do any class initializations here.
    private void Initialize()
    {
        SpeedCharges = 0;
        IsSliding = false;
        VaultLockTimer = 0f;
        initialized = true;
    }

    //Stop all movement on the character;
    public void HaltMovement(bool haltX, bool haltY, bool haltZ)
    {
        pawnMovement.HaltMovement(haltX, haltY, haltZ);
    }

    //Quick reference for moving the pawn's character controller.
    public CollisionFlags Move(Vector3 movementVelocity)
    {
        return characterController.Move(movementVelocity);
    }

    //Check to see if the player is moving faster than a certain speed
    public bool IsMovingFasterThan(float targetVelocity)
    {
        return pawnMovement.IsMovingFasterThan(targetVelocity);
    }

    //Add a lock on actions from happening.
    public void AddVaultLock(float time)
    {
        VaultLockTimer = time < .25f ? ACTION_LOCK_MINIMUM_TIME : time;
    }

}