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
    private readonly float VAULT_MAX_ANGLE_FROM_TARGET = 35f;
    private RaycastHit lowSensorHitInfo;
    private Quaternion currentPawnAngle;
    private Quaternion vaultWallAngle;
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
            if (!pawn.IsGrounded && pawn.PawnInput.JumpPressed && vaultHighSensor.CollidedObjects == 0 &&
                vaultHighSensor.FindVaultPoint(ref hitInfo, vaultLowSensor.gameObject.transform.position))
            {
                if (Physics.Raycast(vaultLowSensor.transform.position, vaultLowSensor.transform.forward, out lowSensorHitInfo, 2, LayerMask.GetMask("MapGeometry")))
                {
                    currentPawnAngle = transform.rotation;
                    vaultWallAngle = Quaternion.LookRotation(-lowSensorHitInfo.normal, transform.up);

                    Quaternion angleDifference = Quaternion.Inverse(currentPawnAngle) * vaultWallAngle;

                    if (angleDifference.eulerAngles.y <= VAULT_MAX_ANGLE_FROM_TARGET || 360f - angleDifference.eulerAngles.y <= VAULT_MAX_ANGLE_FROM_TARGET)
                    {
                        vaultPawnRotationTimer = 0;

                        pawn.Locked = true;
                        UIManager.Instance.DebugText1 = "ON";
                        vaultState = pawn.IsFalling ? VaultState.DIP : VaultState.RAISE;
                        pawn.HaltMovement();

                        //For some reason the position is 1m above the bottom of the CharacterController.
                        vaultPoint = hitInfo.point + new Vector3(0, 1);

                        gameObjectToVault = hitInfo.transform.gameObject.name;
                        pauseTimer = PAUSE_TIME;

                        dipY = vaultPoint.y - DIP_AMOUNT;
                        dipVelocity = -DIP_INITIAL_GRAVITY;

                        raiseVelocity = RAISE_INITIAL_VELOCITY;
                        cooldownTimer = 0f;

                        movementVelocity = Vector3.zero;
                    }

                   
                }                
            }
        }
        else if (vaultState == VaultState.DIP)
        {
            UIManager.Instance.DebugText2 = "ON";
            movementVelocity.y = dipVelocity;
            dipVelocity += DIP_GRAVITY_DECREMENT * Time.deltaTime;
            pawn.Move(movementVelocity * Time.deltaTime);
            SlerpCamera();
            if (vaultLowSensor.transform.position.y <= dipY)
            {
                UIManager.Instance.DebugText2 = "";
                vaultState = VaultState.PAUSE;
            }
        }
        else if (vaultState == VaultState.PAUSE)
        {
            SlerpCamera();
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                vaultState = VaultState.RAISE;
            }
        }
        //Now vault.
        else if (vaultState == VaultState.RAISE)
        {
            SlerpCamera();
            movementVelocity = vaultLowSensor.IsCollidingWith(gameObjectToVault) ? Vector3.zero : raiseVelocity * transform.forward;
            movementVelocity.y = raiseVelocity;
            raiseVelocity += RAISE_INCREMENT * Time.deltaTime;
            raiseVelocity = Mathf.Clamp(raiseVelocity, -RAISE_MAX_VELOCITY, RAISE_MAX_VELOCITY);
            pawn.Move(movementVelocity * Time.deltaTime);

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
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer > COOLDOWN_TIME)
            {
                vaultState = VaultState.ATTEMPT_VAULT;                
                UIManager.Instance.DebugText1 = "";
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

    private void SlerpCamera()
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
