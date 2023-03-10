
using System.Collections;
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
    private const int MAX_CLIP_AMMO = 10;

    private const float COOLDOWN_TIME = .1f;
    private float cooldownTimer = 0f;

    private bool reloading = false;

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

        if (!reloading)
        {
            if (cooldownTimer <= 0f && ammoCount > 0f && pawnInput.PrimaryUsePressedThisFrame)
            {
                pistolAnimator.SetTrigger("Shoot");
                pawnArmsAnimator.SetTrigger("Shoot");
                cooldownTimer = COOLDOWN_TIME;
                ammoCount--;
                pawnLook.Recoil(1, 3);

                GameObject obj = new GameObject();
                Bullet bullet = obj.AddComponent<Bullet>();
                obj.transform.position = pawnLook.MainCamera.position + pawnLook.MainCamera.forward * .5f; ;
                obj.transform.rotation = pawnLook.MainCamera.rotation;
                bullet.Initialize(500f);                
            }
        }

        if (pawnInput.ReloadPressedThisFrame)
        {
            reloading = true;
            pawnArmsAnimator.SetTrigger("Reload");
            StartCoroutine(PlayReloadPistolAnimation(.5f));
            StartCoroutine(DoReload(1.5f));
        }
    }

    private IEnumerator PlayReloadPistolAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        pistolAnimator.SetTrigger("Reload");
    }

    private IEnumerator DoReload(float time)
    {
        yield return new WaitForSeconds(time);
        ammoCount = MAX_CLIP_AMMO;
        reloading = false;
    }
}
