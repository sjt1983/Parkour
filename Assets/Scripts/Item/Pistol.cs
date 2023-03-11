using System.Collections;
using UnityEngine;

public class Pistol : EquippableItem
{
    /***********************************/
    /*** Local Refs to Unity objects ***/
    /***********************************/

    [SerializeField]
    private Animator pistolAnimator;

    [SerializeField]
    private PawnArmsAnimator pawnArmsAnimator;

    [SerializeField]
    private PawnLook pawnLook;

    //Local ref to the pawn input so we can do our own Input Detection.
    private PawnInput pawnInput;

    /***********************/
    /*** Class Variables ***/
    /***********************/

    //Default position and rotation for the pistol when it is in the hands of the pawn.
    private Vector3 defaultHandPosition = new Vector3(-0.00045f, 0, 0.00054f);
    private Quaternion defaultHandRotation = Quaternion.Euler(39.232f, 140.551f, -4.256f);

    //How much ammo is in the gun currently.
    private int ammoCount = 10;

    //How much ammo can be in a clip.
    private const int MAX_CLIP_AMMO = 10;

    //Cooldown time per shot.
    private const float COOLDOWN_TIME = .1f;
    private float cooldownTimer = 0f;

    //Realoding flag to prevent actions from happening while reloading.
    private bool reloading = false;



    public override void EquipItem()
    {
        base.EquipItem();
        gameObject.transform.localPosition = defaultHandPosition;
        gameObject.transform.localRotation = defaultHandRotation;
    }

    //Assign the pawn to the item.
    public override void AssignToPawn(Pawn pawn)
    {
        //Call the superclass method
        base.AssignToPawn(pawn);

        //Load references to unity objects.
        pawnInput = pawn.gameObject.GetComponent<PawnInput>();
        pawnLook = pawn.gameObject.GetComponent<PawnLook>();
        pawnArmsAnimator = pawn.gameObject.GetComponentInChildren<PawnArmsAnimator>();
    }

    private void Update()
    {
        //Never run update if the item isnt assigned to a pawn or equipped.
        if (!IsAssignedToPawn || !Equipped)            
            return;

        //Fake UI for now.
        UIManager.Instance.DebugText1 = ammoCount.ToString();

        //If the cooldown happened, reset the triggers.
        cooldownTimer = Mathf.Clamp(cooldownTimer - Time.deltaTime, 0f, 1f);
        if (cooldownTimer <= 0f)
        {
            pistolAnimator.ResetTrigger("Shoot");
            pawnArmsAnimator.ResetTrigger("Shoot");
        }

        //If we are in the process of reloading, dont allow anything to happen.
        if (!reloading)
        {
            //Shoot the gun under the conditions below, cooldown is up, have ammo, and someone tried to shoot.
            if (cooldownTimer <= 0f && ammoCount > 0f && pawnInput.PrimaryUsePressedThisFrame)
            {
                pistolAnimator.SetTrigger("Shoot");
                pawnArmsAnimator.SetTrigger("Shoot");
                cooldownTimer = COOLDOWN_TIME;
                ammoCount--;
                pawnLook.RecoilCamera(1, 3);

                //Make a bullet and send it flying!!!!
                GameObject obj = new GameObject("Bullet");
                Bullet bullet = obj.AddComponent<Bullet>();
                obj.transform.position = pawnLook.MainCamera.position + pawnLook.MainCamera.forward * .5f; ;
                obj.transform.rotation = pawnLook.MainCamera.rotation;
                bullet.Initialize(500f);                
            }
            else if (pawnInput.ReloadPressedThisFrame)
            {
                reloading = true;
                pawnArmsAnimator.SetTrigger("Reload");
                //Delay pistol clip from by a half second while the arms do their thing
                StartCoroutine(PlayReloadPistolAnimation(.5f));
                //Reload after all animations play!.
                StartCoroutine(DoReload(1.5f));
            }
        }        
    }

    //Do the Reload animation.
    private IEnumerator PlayReloadPistolAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        pistolAnimator.SetTrigger("Reload");
    }

    //Do the resetting of variables for the reload.
    private IEnumerator DoReload(float time)
    {
        yield return new WaitForSeconds(time);
        ammoCount = MAX_CLIP_AMMO;
        reloading = false;
    }
}
