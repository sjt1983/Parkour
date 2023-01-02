using UnityEngine;

public class PawnVault : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    [SerializeField]
    private Pawn pawn;
    
    [SerializeField]
    private VaultSensor vaultSensor;

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

    //The Vector to apply to the character when the script moves it upwards.
    private Vector3 movementVelocity;

    //How long in between vaults we allow.
    private float cooldownTimer = .2f;

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
            if (!pawn.IsGrounded && vaultSensor.FindVaultPoint(ref vaultPoint, pawn.LookAngle) && vaultSensor.CollidedObjects == 0 && pawn.PawnInput.JumpPressed)
            {
                pawn.Locked = true;
                pawn.HaltMovement();
                vaultState = VaultState.EXECUTE_VAULT;
                vaultPoint += new Vector3(0, 1);
            }
        }
        //Now vault.
        else if (vaultState == VaultState.EXECUTE_VAULT)
        {
            movementVelocity = 15f * transform.forward;
            movementVelocity.y = 15f;
            pawn.Move(movementVelocity);

            if (pawn.gameObject.transform.position.y >= vaultPoint.y)
            {
                pawn.Locked = false;
                vaultState = VaultState.COOLDOWN;
                cooldownTimer = 0f;
            }
        }
        else if (vaultState == VaultState.COOLDOWN)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer > .3f) {
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
}

/*** Local States for the script ***/
enum VaultState
{
    ATTEMPT_VAULT,
    EXECUTE_VAULT,
    COOLDOWN
}
