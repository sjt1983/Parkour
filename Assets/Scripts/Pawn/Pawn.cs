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
    private PawnInventory pawnInventory;

    [SerializeField]
    private CharacterController characterController;

    /***************************/
    /* Private class variables */
    /***************************/

    //Minimum time to lock the pawn from doing stuff
    private const float ACTION_LOCK_MINIMUM_TIME = .25f;

    //Flag to prevent any logic from executing until after Initialization.
    private bool initialized = false;

    //How long we should lock the vaulting action after doing it.
    public float VaultLockTimer { set; get; }

    /************************/
    /*** Class properties ***/
    /************************/

    //Flag to indicate that a certain script has control over the pawn and that nothing else should touch it.
    public bool MovementLocked { get; set; }

    //Indicated the effects of items should be ignored.
    public bool ItemLocked { get; set; }

    //The angle the camera is looking at.
    public float LookAngle { get; set; }

    //Quick check to see if the pawn is TRYING to move along x/z due to input.
    public bool IsTryingToMove { get => pawnMovement.IsTryingToMove(); }

    //How many Speed Charges the pawn has, which increases max velocity
    public int SpeedCharges { get; private set; }

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
    public bool IsCrouching { get => pawnInput.CrouchPressed; }

    //Quick reference for if the pawn is sliding. Set by the PawnCrouch script.
    public bool IsSliding { get; set; }

    //Quick reference for if the pawn is sliding. Set by the PawnCrouch script.
    public bool IsFalling { get => pawnMovement.UpwardSpeed < 0f; }

    //Quick reference for if the pawn is landing penalized
    public bool IsLandingPenalized { get; set; }

    //Quick Reference for Forward Speed
    public float CurrentZSpeed { get => pawnMovement.ForwardSpeed; }

    //Quick Reference for Upward Speed.
    public float UpwardSpeed { get => pawnMovement.UpwardSpeed; }

    //Current forward vector the pawn is moving in
    public Vector3 ForwardVector { get; set; }

    //Current right vector the pawn is moving in
    public Vector3 RightVector { get; set; }

    //Item the pawn is looking at
    public EquippableItem ItemPawnIsLookingAt { get; set; }
    
    /*********************/
    /*** Unity Methods ***/
    /*********************/

    public void Update()
    {
        if (!initialized)
            Initialize();

        //Deduct the Vault Lock timer on the pawn.
        VaultLockTimer = Mathf.Clamp(VaultLockTimer - Time.deltaTime, 0, 555);

        Interact();
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
        IsLandingPenalized = false;
        ItemLocked = false;
        ForwardVector = Vector3.zero;
        RightVector = Vector3.zero;

        initialized = true;

    }

    //Add a speed charge to the pawn to increase the max speed
    public void AddSpeedCharge()
    {
        SpeedCharges += 1;
    }

    //Remove all speed charges on the pawn,
    public void RemoveAllSpeedCharges()
    {
        SpeedCharges = 0;
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

    //Have the pawn pickup the item and put it in their inventory.
    public void PickupItem(EquippableItem item)
    {
        item.AssignToPawn(this);
        pawnInventory.PickupItem(item);
    }

    //Have the pawn interact with the item if they are looking at one AND interacrtoing.
    private void Interact()
    {
        if (pawnInput.InteractPressedThisFrame && ItemPawnIsLookingAt != null)
            ItemPawnIsLookingAt.Interact(this);
    }

    //Register a Hit with the pawn.
    public void RegisterHit(HitResponse hitResponse)
    {
        UIManager.Instance.RegisterHit(hitResponse);
    }

}