
using UnityEngine;

public class Pistol : EquippableItem
{
    [SerializeField]
    private Animator pistolAnimator;

    [SerializeField]
    private PawnArmsAnimator pawnArmsAnimator;

    [SerializeField]
    private PawnLook pawnLook;

    private Vector3 defaultPosition = new Vector3(-0.00045f, 0, 0.00054f);
    private Quaternion defaultRotation = Quaternion.Euler(39.232f, 140.551f, -4.256f);

    private int ammoCount = 10;
    private int MAX_CLIP_AMMO = 10;

    private const float COOLDOWN_TIME = .1f;
    private float cooldownTimer = 0f;

    //Local ref to the pawn input so we can do our own Input Detection.
    private PawnInput pawnInput;
    public override void EquipItem()
    {
        base.EquipItem();
        gameObject.transform.localPosition = defaultPosition;
        gameObject.transform.localRotation = defaultRotation;
    }

    //Assign the pawn to the item.
    public override void AssignToPawn(Pawn pawn)
    {
        //Call the superclass
        base.AssignToPawn(pawn);
        pawnInput = pawn.gameObject.GetComponent<PawnInput>();
        pawnLook = pawn.gameObject.GetComponent<PawnLook>();
        pawnArmsAnimator = pawn.gameObject.GetComponentInChildren<PawnArmsAnimator>();
    }

    private void Update()
    {
        UIManager.Instance.DebugText1 = "";
        if (!IsAssignedToPawn || !Equipped)            
            return;

        UIManager.Instance.DebugText1 = ammoCount.ToString();

        cooldownTimer = Mathf.Clamp(cooldownTimer - Time.deltaTime, 0f, 1f);
        if (cooldownTimer <= 0f)
        {
            pistolAnimator.ResetTrigger("Shoot");
            pawnArmsAnimator.ResetTrigger("Shoot");
        }

        if (cooldownTimer <= 0f && ammoCount > 0f && pawnInput.PrimaryUsePressedThisFrame)
        {
            pistolAnimator.SetTrigger("Shoot");
            pawnArmsAnimator.SetTrigger("Shoot");
            cooldownTimer = COOLDOWN_TIME;
            ammoCount--;
            pawnLook.Recoil(5, 15, .1f);
        }
        else if (pawnInput.ReloadPressedThisFrame)
        {
            ammoCount = MAX_CLIP_AMMO;
        }
    }
}
