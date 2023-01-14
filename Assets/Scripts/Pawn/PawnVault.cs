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

    //The state of the scripts control over the character.
    private VaultState vaultState = VaultState.ATTEMPT_VAULT;

    //The point where we want to move the player when they hit a spot to vault.
    private Vector3 vaultPoint = Vector3.zero;
    //The Raycast hit for the vault.
    private RaycastHit hitInfo;
    //The gameObject we are trying to vault
    private string gameObjectToVault;

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

    //Velocity used for the raise.
    private float raiseVelocity;

    /*** Cooldown ***/
    //How long in between vaults we allow.
    private readonly float COOLDOWN_TIME = .2f;
    //Calculated cooldown time.
    private float cooldownTimer;

    /*** Camera ***/
    //How far the difference in angle can be to vault, e.g. dont let someone who is perpendicular to a wall vault it.
    private readonly float VAULT_MAX_ANGLE_FROM_TARGET = 35f;

    //HitInfo of the low sensor when it calculates the angle of the wall we far trying to vault.
    private RaycastHit lowSensorHitInfo;
    //current angle the pawn is facing.
    private Quaternion currentPawnAngle;
    //current angle of the wall we are facing.
    private Quaternion vaultWallAngle;

    //Vars used to slerp the camera to the right angle.
    private float vaultPawnRotationSpeed = 4f;
    private float vaultPawnRotationTimer = 0.0f;

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
                vaultHighSensor.FindVaultPoint(ref hitInfo, vaultLowSensor.gameObject.transform.position))
            {
                //Cool, now lets check the angle of the wall we are trying to vault if the first few checks passed.
                if (Physics.Raycast(vaultLowSensor.transform.position, vaultLowSensor.transform.forward, out lowSensorHitInfo, 2, LayerMask.GetMask("MapGeometry")))
                {
                    //Get both angles (pawn and wall) and calculate the difference.
                    currentPawnAngle = transform.rotation;
                    vaultWallAngle = Quaternion.LookRotation(-lowSensorHitInfo.normal, transform.up);
                    Quaternion angleDifference = Quaternion.Inverse(currentPawnAngle) * vaultWallAngle;

                    //Weird Quaternion math, the angle is between 1 and 360, so a "difference" of 359 is smaller than a difference of 10
                    //so check both ends assuming player camera is facing angle 0, and I want a 45 degree angle requirment to vault,
                    //then I need to ensure the angle is less than 45 degrees AND greater than 315 degrees (45 degrees left or right from degree 0
                    if (angleDifference.eulerAngles.y <= VAULT_MAX_ANGLE_FROM_TARGET || 360f - angleDifference.eulerAngles.y <= VAULT_MAX_ANGLE_FROM_TARGET)
                    {
                        //If here, lets vault, do some housekeeping on the pawn.

                        //lock the pawn and stop it, but before we do that, see if its falling and set the proper state
                        pawn.Locked = true;
                        vaultState = pawn.IsFalling ? VaultState.DIP : VaultState.RAISE;
                        pawn.HaltMovement();
                        
                        //For some reason the position of the pawn is 1m above the bottom of the CharacterController.
                        vaultPoint = hitInfo.point + new Vector3(0, 1);

                        gameObjectToVault = hitInfo.transform.gameObject.name;
                        //Set how long we pause in the air.
                        pauseTimer = PAUSE_TIME;
                        
                        //Set the variables for the dip if were are falling
                        dipY = vaultPoint.y - DIP_AMOUNT;
                        dipVelocity = -DIP_INITIAL_GRAVITY;

                        //Initial velocity of the player when pulling them selves up.
                        raiseVelocity = RAISE_INITIAL_VELOCITY;

                        //Set the cooldown timer
                        cooldownTimer = 0f;
                        
                        //Zero out the veocity
                        movementVelocity = Vector3.zero;
                    }                   
                }                
            }
        }
        else if (vaultState == VaultState.DIP)
        {
            
            //DO the vip logic, fall, then pull yourself up, while the camera slerps.
            movementVelocity.y = dipVelocity;
            dipVelocity += DIP_GRAVITY_DECREMENT * Time.deltaTime;
            pawn.Move(movementVelocity * Time.deltaTime);
            SlerpCameraTowardsVaultPoint();

            //End logic for this state
            if (vaultLowSensor.transform.position.y <= dipY)
            {
                vaultState = VaultState.PAUSE;
            }
        }
        else if (vaultState == VaultState.PAUSE)
        {
            //Just sit for a bit but also slerp the camera.
            //SlerpCamera();
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                vaultState = VaultState.RAISE;
            }
        }
        //Now vault.
        else if (vaultState == VaultState.RAISE)
        {

            //Go forward until the forward sensor is through the wall.
            movementVelocity = vaultLowSensor.IsCollidingWith(gameObjectToVault) ? Vector3.zero : raiseVelocity * transform.forward;
            //Raise up, progressively faster
            movementVelocity.y = raiseVelocity;
            raiseVelocity += RAISE_INCREMENT * Time.deltaTime;
            raiseVelocity = Mathf.Clamp(raiseVelocity, -RAISE_MAX_VELOCITY, RAISE_MAX_VELOCITY);
            pawn.Move(movementVelocity * Time.deltaTime);

            //Move the camera
            SlerpCameraTowardsVaultPoint();

            //Release control after the raise phase
            if (gameObject.transform.position.y >= vaultPoint.y)
            {
                pawn.Locked = false;
                vaultState = VaultState.COOLDOWN;
                cooldownTimer = 0f;
                pawn.HaltMovement();
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

    //Rotates the player to be facing the vault point
    private void SlerpCameraTowardsVaultPoint()
    {
        transform.rotation = Quaternion.Slerp(currentPawnAngle, vaultWallAngle, vaultPawnRotationTimer * vaultPawnRotationSpeed);
        vaultPawnRotationTimer = vaultPawnRotationTimer + Time.deltaTime;
    }
}


/*** Local States for the script ***/
enum VaultState
{
    ATTEMPT_VAULT,
    PAUSE,
    DIP,
    RAISE,
    COOLDOWN
}
