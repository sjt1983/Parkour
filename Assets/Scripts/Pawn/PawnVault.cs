using UnityEngine;

public class PawnVault : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private PawnInventory pawnInventory;

    [SerializeField]
    private VaultSensor vaultHighSensor;

    [SerializeField]
    private Sensor vaultLowSensor;

    [SerializeField]
    private Transform mainCamera;

    [SerializeField]
    private PawnArmsAnimator pawnArmsAnimator;

    /************************/
    /*** Class Properties ***/
    /************************/
    //None!

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    //Flag for initialization, do it first! LOL
    private bool initialized = false;

    //The state of the scripts control over the character.
    private VaultState vaultState = VaultState.ATTEMPT_VAULT;

    //The point where we want to move the player when they hit a spot to vault.
    private Vector3 vaultPoint = Vector3.zero;
    //The Raycast hit for the vault.
    private RaycastHit hitInfo;

    //The Vector to apply to the character when the script moves it.
    private Vector3 movementVelocity;

    /*******************************************/
    /*** Special Variables for Vault Tuning. ***/
    /*******************************************/

    /*** Pause ***/
    //How long we pause before vaulting
    private readonly float PAUSE_TIME = .3f;
    //Calculated Pause Time;
    private float pauseTimer;

    /*** Raise ***/
    //MAX Velocity for raising;
    private readonly float RAISE_HEIGHT_BUFFER = .1f;

    //SmoothDamp smooth reference float.
    private float smoothY;

    //Camera Dip params for wall jumping.
    private const float VAULT_DIP_ANGLE = 8f;
    private const float VAULT_DIP_SMOOTH_TIME = .1f;
    private const float VAULT_DIP_RAISE_TIME = .1f;
    private const float VAULT_MINIMUM_FORWARD_SPEED = 5f;
    
    //Extra height buffer to ensure SmoothDamp hits the target, because .2999999999999999999999 < 3
    private const float VAULT_SMOOTH_HEIGHT_BUFFER = .05f;
    //Used to smooth out the SmoothDamp on the raise
    private const float VAULT_SMOOTH_TIME = .15f;

    /*** Forward ***/
    //Amount of time we should move forward before releasing control
    private readonly float FORWARD_TIME = .1f;
    //Calculated forward time.
    private float forwardTimer;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    void Update()
    {
        if (!initialized)
            Initialize();

        //Check to see if we SHOULD vault.
        if (vaultState == VaultState.ATTEMPT_VAULT)
        {
            //Do some basic checks and see if we have a potential vault point.
            if (!pawn.IsGrounded && pawn.PawnInput.JumpIsPressed && vaultHighSensor.CollidedObjects == 0 
                 && pawn.VaultLockTimer <= 0 && vaultHighSensor.FindVaultPoint(ref hitInfo, vaultLowSensor.transform.position))
            {               
                //If here, lets vault, do some housekeeping on the pawn.
                //lock the pawn and stop it, but before we do that, see if its falling and set the proper state
                pawn.MovementLocked = true;
                vaultState = pawn.IsFalling ? VaultState.PAUSE : VaultState.RAISE;
                        
                //For some reason the position of the pawn is 1m above the bottom of the CharacterController.
                vaultPoint = hitInfo.point + new Vector3(0, 1 + RAISE_HEIGHT_BUFFER);

                //Set how long we pause in the air.
                pauseTimer = PAUSE_TIME;

                //Reset the forward timer.
                forwardTimer = 0f;

                //Zero out the veocity
                movementVelocity = Vector3.zero;

                //Set the IK Points.
                pawnArmsAnimator.SetActiveIk(hitInfo.point + (-transform.right * .4f) + (transform.up * .2f), Quaternion.identity, hitInfo.point + (transform.right * .4f) + (transform.up * .2f), Quaternion.identity);

                //Lock the item.
                pawn.ItemLocked = true;
            }
        }
        else if (vaultState == VaultState.PAUSE)
        {
            pawnLook.Dip(VAULT_DIP_ANGLE, VAULT_DIP_SMOOTH_TIME, VAULT_DIP_RAISE_TIME, true);
            //Just sit for a bit but also slerp the camera.
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
                vaultState = VaultState.RAISE;
        }
        //Now vault.
        else if (vaultState == VaultState.RAISE)
        {
            pawnLook.Dip(VAULT_DIP_ANGLE, VAULT_DIP_SMOOTH_TIME, VAULT_DIP_RAISE_TIME, true);
            transform.position = new Vector3(transform.position.x, Mathf.SmoothDamp(transform.position.y, vaultPoint.y + VAULT_SMOOTH_HEIGHT_BUFFER, ref smoothY, VAULT_SMOOTH_TIME), transform.position.z);

            //Release control after the raise phase
            if (transform.position.y >= vaultPoint.y)
            {
                transform.position = new Vector3(transform.position.x, vaultPoint.y, transform.position.z);
                vaultState = VaultState.FORWARD;
            }
        }
        else if (vaultState == VaultState.FORWARD)
        {
            pawnArmsAnimator.ClearIk();
            //Go forward if we are sure we will collide with something.
            movementVelocity = transform.forward * ((pawn.CurrentZSpeed > VAULT_MINIMUM_FORWARD_SPEED ? pawn.CurrentZSpeed : VAULT_MINIMUM_FORWARD_SPEED) * Time.deltaTime);
            movementVelocity.y = 0;
            pawn.Move(movementVelocity);
            forwardTimer += Time.deltaTime;
            if (forwardTimer > FORWARD_TIME)
            {
                vaultState = VaultState.ATTEMPT_VAULT;
                pawn.HaltMovement(false, true, false);
                pawn.ItemLocked = false;
                pawn.MovementLocked = false;
            }
        }
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    private void Initialize()
    {
        initialized = true;
    }
}


/*** Local States for the script ***/
enum VaultState
{
    ATTEMPT_VAULT,
    PAUSE,
    RAISE,
    FORWARD
}
