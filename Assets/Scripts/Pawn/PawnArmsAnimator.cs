using UnityEngine;

//Used to animate the pawns arms.
public class PawnArmsAnimator : MonoBehaviour
{
    //Reference to the pawn.
    [SerializeField]
    private Pawn pawn;
    
    //Reverence to the animator.
    [SerializeField]
    private Animator pawnArmsAnimator;

    /*** Item switch animations/pawn properties need to happen on a timer and in-sync ***/

    //Which item we will equip
    private EquippableItem itemToEquip;
    //Which state the equipping script is in.
    private EquipState equipState = EquipState.WAITING;

    /*** I do a neat trick where, instead of creating animations for each weapon to "Switch" I just move the arms backwards diagonally down ***/
    //default position of the pawns arms.
    private Vector3 defaultPawnArmsPosition;
    //target position of the pawns arms,
    private Vector3 targetPawnArmsPosition;
    //How fast we should lerp the arms when switching weapons. This should be half the total time you want to switch weapons.
    private const float WEAPON_SWITCH_LERP_TIME = .25f;
    //Tracker for how long we hagve lerped for.
    private float lerpTimer = 0f;

    //The default animation controller, we override this for each weapon.
    RuntimeAnimatorController defaultRuntimeAnimationController;

    private void Start()
    {
        //Initialize the class.
        defaultRuntimeAnimationController = pawnArmsAnimator.runtimeAnimatorController;
        //Set the default arms position
        defaultPawnArmsPosition = transform.localPosition;
        //Set the target arms position
        targetPawnArmsPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - .5f, transform.localPosition.z - .5f);
    }

    private void Update()
    {
        //Determine if we should be playing the run animation or not.
        if (pawn.IsGrounded && pawn.IsMovingFasterThan(2f) && !pawn.IsSliding)
            pawnArmsAnimator.SetBool("Running", true);
        else
            pawnArmsAnimator.SetBool("Running", false);

        //See if we need to equip the item.
        DoEquipItem();

        UIManager.Instance.DebugText3 = equipState.ToString();
    }
    
    //Trigger an animation.
    public void SetTrigger(string trigger)
    {
        pawnArmsAnimator.SetTrigger(trigger);
    }

    //Reset an animation trigger.
    public void ResetTrigger(string trigger)
    {
        pawnArmsAnimator.ResetTrigger(trigger);
    }

    //Triggers the animation for equipping a new item.
    public void HandleEquipItem(EquippableItem equippableItem)
    {
        itemToEquip = equippableItem;
        equipState = EquipState.LOWER;
    }

    //Do the actual equipping animation.
    private void DoEquipItem()
    {
        //Waiting just means we are waiting for someome to trigger the equip animation
        if (equipState == EquipState.WAITING)
        {
            lerpTimer = 0f;
        }
        //Lowering means we are moving the arms out of the way.
        else if (equipState == EquipState.LOWER) {
            lerpTimer += Time.deltaTime;
            //Lerp the arms out the way!
            transform.localPosition = Vector3.Lerp(defaultPawnArmsPosition, targetPawnArmsPosition, lerpTimer / WEAPON_SWITCH_LERP_TIME);
            //Basically, if we are "close enough" just set the position manually and move on!
                        
            if (lerpTimer >= WEAPON_SWITCH_LERP_TIME)
            {
                lerpTimer = 0f;
                pawnArmsAnimator.runtimeAnimatorController = itemToEquip != null ? itemToEquip.AnimatorOverrideController : defaultRuntimeAnimationController;
                equipState = EquipState.RAISE;
            }
        }
        //Raising is bring our arms back up.
        else if (equipState == EquipState.RAISE)
        {
            lerpTimer += Time.deltaTime;
            //Lerp the arms back!
            transform.localPosition = Vector3.Lerp(targetPawnArmsPosition, defaultPawnArmsPosition, lerpTimer / WEAPON_SWITCH_LERP_TIME);
            //Basically, if we are "close enough" just set the position manually and move on!
            if (lerpTimer >= WEAPON_SWITCH_LERP_TIME)
            {
                transform.localPosition = defaultPawnArmsPosition;
                equipState = EquipState.WAITING;
            }
        }
    }
}

enum EquipState
{
    WAITING,
    LOWER,
    RAISE
}
