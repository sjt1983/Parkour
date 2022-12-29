using UnityEngine;

public class PawnVault : MonoBehaviour
{
    /*** Local References to Unity Objects ***/

    [SerializeField]
    private Pawn pawn;
    
    [SerializeField]
    private VaultSensor vaultSensor;

    /*** Class Properties ***/
    //None!

    /*** Local Class Variables ***/

    //Flag for initialization, do it first! LOL
    private bool initialized = false;

    //The state of the scripts control over the character.
    private VaultState vaultState = VaultState.NONE;

    //The point where we want to move the player when they hit a spot to vault.
    private Vector3 vaultPoint = Vector3.zero;

    //The Vector to apply to the character when the script moves it upwards.
    private Vector3 movementVelocity;

    //How long in between vaults we allow.
    private float cooldownTimer = 1;

    /*** Unity Methods ***/

    void Update()
    {
        if (!initialized)
            Initialize();

        if (vaultState == VaultState.NONE)
        {
            if (!pawn.IsGrounded && vaultSensor.FindVaultPoint(ref vaultPoint) && vaultSensor.CollidedObjects == 0)
            {
                pawn.Locked = true;
                vaultState = VaultState.VERTICAL;
                vaultPoint += new Vector3(0, 1);
            }
        }
        else if (vaultState == VaultState.VERTICAL)
        {
            movementVelocity.y = 10f;
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
                vaultState = VaultState.NONE;
            }
        }
    }

    /*** Class Methods ***/
    private void Initialize()
    {
        initialized = true;
    }
}

/*** Local States for the script ***/
enum VaultState
{
    NONE,
    VERTICAL,
    COOLDOWN
}
