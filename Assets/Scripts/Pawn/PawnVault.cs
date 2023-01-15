using UnityEngine;

public class PawnVault : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private VaultSensor vaultHighSensor;

    [SerializeField]
    private Sensor vaultLowSensor;

    [SerializeField]
    private Transform mainCamera;

    /************************/
    /*** Class Properties ***/
    /************************/
    //None!

    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    //Flag for initialization, do it first! LOL
    private bool initialized = false;

    private readonly float GLOBAL_SPEED = .9f;

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
    private readonly float PAUSE_TIME = .1f;
    //Calculated Pause Time;
    private float pauseTimer;

    /*** Dip ***/
    //How far we dip
    private readonly float DIP_AMOUNT = 1.5f;
    //How fast we dip initially.
    private readonly float DIP_INITIAL_GRAVITY = 2f;
    //How much the dip slows down.
    private readonly float DIP_GRAVITY_DECREMENT = .6f;
    //The coordinate we should dip to.
    private float dipY;
    //Velocity used for the dip
    private float dipVelocity;

    /*** Raise ***/
    //Initial Velocity at which we raise.
    private readonly float RAISE_INITIAL_VELOCITY = 8f;
    //How much the velocity increment per second.
    private readonly float RAISE_INCREMENT = 2f;
    //MAX Velocity for raising;
    private readonly float RAISE_MAX_VELOCITY = 10f;
    //MAX Velocity for raising;
    private readonly float RAISE_HEIGHT_BUFFER = 1.2f;

    //Velocity used for the raise.
    private float raiseVelocity;

    /*** Forward ***/
    //How long in between vaults we allow.
    private readonly float FORWARD_TIME = .2f;
    //Calculated forward time.
    private float forwardTimer;
    private readonly float FORWARD_VELOCITY = 4f;

    /*** Cooldown ***/
    //How long in between vaults we allow.
    private readonly float COOLDOWN_TIME = .2f;
    //Calculated cooldown time.
    private float cooldownTimer;

    /*** Camera Zob ***/

    //Vars used for moving the head back and forth.
    //How fast the camera should move back and forth.
    private float VAULT_CAMERA_ZOB_SPEED = 5f;
    //Where the camera starts
    private float zobZDefault;
    
    //Original Position of the camera
    private Vector3 zobDefautVector;

    //Flag to indicate we should zob
    private bool zobFlag;
    //How far away from the default Z we should go.
    private float ZOB_DISTANCE = .45f;

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
            if (!pawn.IsGrounded && pawn.PawnInput.JumpPressed && vaultHighSensor.CollidedObjects == 0 &&
                vaultHighSensor.FindVaultPoint(ref hitInfo, vaultLowSensor.transform.position))
            {               
                //If here, lets vault, do some housekeeping on the pawn.
                //lock the pawn and stop it, but before we do that, see if its falling and set the proper state
                pawn.Locked = true;
                vaultState = pawn.IsFalling ? VaultState.DIP : VaultState.RAISE;
                pawn.HaltMovement();
                        
                //For some reason the position of the pawn is 1m above the bottom of the CharacterController.
                vaultPoint = hitInfo.point + new Vector3(0, 1 + RAISE_HEIGHT_BUFFER);

                //Set how long we pause in the air.
                pauseTimer = PAUSE_TIME;
                        
                //Set the variables for the dip if were are falling
                dipY = vaultPoint.y - DIP_AMOUNT;
                dipVelocity = -DIP_INITIAL_GRAVITY;

                //Initial velocity of the player when pulling them selves up.
                raiseVelocity = RAISE_INITIAL_VELOCITY;

                //ZOB settings
                zobFlag = true;
                zobZDefault = mainCamera.localPosition.z;
                zobDefautVector = mainCamera.localPosition;

                //Set the cooldown timer, time before the player can vault again
                cooldownTimer = 0f;

                forwardTimer = 0f;

                //Zero out the veocity
                movementVelocity = Vector3.zero;                                   
            }
        }
        else if (vaultState == VaultState.DIP)
        {            
            //Do the dip logic, fall, then pull yourself up, while the camera slerps.
            movementVelocity.y = dipVelocity * Time.deltaTime * GLOBAL_SPEED; ;
            dipVelocity += DIP_GRAVITY_DECREMENT * Time.deltaTime * GLOBAL_SPEED;
            pawn.Move(movementVelocity);
            ZobCamera();

            //End logic for this state
            if (vaultLowSensor.transform.position.y <= dipY)
            {
                vaultState = VaultState.PAUSE;
            }
        }
        else if (vaultState == VaultState.PAUSE)
        {
            //Just sit for a bit but also slerp the camera.
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                vaultState = VaultState.RAISE;
            }
        }
        //Now vault.
        else if (vaultState == VaultState.RAISE)
        {            
            //Go forward if we are sure we will collide with something.
            movementVelocity = transform.forward * (FORWARD_VELOCITY * Time.deltaTime * GLOBAL_SPEED);
            //Raise up, progressively faster
            movementVelocity.y = raiseVelocity * Time.deltaTime * GLOBAL_SPEED;
            raiseVelocity += RAISE_INCREMENT * Time.deltaTime * GLOBAL_SPEED;
            raiseVelocity = Mathf.Clamp(raiseVelocity, -RAISE_MAX_VELOCITY, RAISE_MAX_VELOCITY);
            pawn.Move(movementVelocity);

            //Move the camera
            ZobCamera();

            //Release control after the raise phase
            if (transform.position.y >= vaultPoint.y)
            {
                pawn.Locked = false;
                vaultState = VaultState.FORWARD;
                cooldownTimer = 0f;
                pawn.HaltMovement();
            }
        }
        else if (vaultState == VaultState.FORWARD)
        {
            //Go forward if we are sure we will collide with something.
            movementVelocity = transform.forward * (FORWARD_VELOCITY * Time.deltaTime * GLOBAL_SPEED);
            movementVelocity.y = 0;
            pawn.Move(movementVelocity);

            forwardTimer += Time.deltaTime;
            if (forwardTimer > FORWARD_TIME)
            {
                vaultState = VaultState.COOLDOWN;
            }
        }
        else if (vaultState == VaultState.COOLDOWN)
        {
            //After cooldown, reset the script state
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer > COOLDOWN_TIME)
            {
                vaultState = VaultState.ATTEMPT_VAULT;
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

    //Gives the illusion of head movement when vaulting.
    private void ZobCamera()
    {
        //Zob
        if (zobFlag)
        {
            mainCamera.transform.localPosition -= new Vector3(0, 0, VAULT_CAMERA_ZOB_SPEED * Time.deltaTime * GLOBAL_SPEED);
            if (mainCamera.transform.localPosition.z < zobZDefault - ZOB_DISTANCE)
            {
                zobFlag = false;
            }
        }
        else
        {
            mainCamera.transform.localPosition += new Vector3(0, 0, VAULT_CAMERA_ZOB_SPEED * Time.deltaTime * GLOBAL_SPEED);

            if (mainCamera.transform.localPosition.z > zobZDefault)
            {
                mainCamera.transform.localPosition = zobDefautVector;
                zobFlag = false;
            }
        }
    }
}


/*** Local States for the script ***/
enum VaultState
{
    ATTEMPT_VAULT,
    PAUSE,
    DIP,
    RAISE,
    FORWARD,
    COOLDOWN
}
