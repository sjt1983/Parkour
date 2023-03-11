using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnLook : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    //Main GameObject controlled by the player.
    [SerializeField]
    private Pawn pawn;

    //Reference for the camera attached to the pawn.
    [SerializeField]
    private Transform mainCamera;

    //Main Camera Mount
    [SerializeField]
    private Transform mainCameraMount;

    //Character Controller
    [SerializeField]
    private CharacterController characterController;

    //Used to indicate which objects the player can "see" for pickup.
    [SerializeField]
    private LayerMask lookLayerMask;

    /************************/
    /*** Class properties ***/
    /************************/

    /*** Public ***/

    public Transform MainCamera { get => mainCamera; }

    /*************************/
    /*** Camera Properties ***/
    /*************************/

    //Used to clamp the camera to prevent the users neck from doing vertical 360s.
    private float cameraVerticalRotation = 0f;
    private readonly float CAMERA_MAX_VERTICAL_ROTATION = 85;

    private Vector3 targetRotation = Vector3.zero;

    /************************/
    /*** Mouse Properties ***/
    /************************/

    //Values of the mouse delta from the last frame, adjusted for sensitivity.
    private float adjustedMouseX;
    private float adjustedMouseY;

    //Mouse Sensitivity
    public float MouseSensitivity = 15;

    /****************************/
    /*** Crouching Properties ***/
    /****************************/

    //Default local camera Y local position
    private float defaultCameraLocalY = 1f;

    //How tall the character should be when crouching.
    private const float CROUCH_HEIGHT = 1f;

    //How tall the character should be when crouching.
    private const float STAND_HEIGHT = 2f;

    //How fast the charatcer should crouch, essentially, meters per second.
    private const float CROUCH_SPEED = 6f;

    /**********************************************************/
    /************** Looking At Item Properties ****************/
    /**********************************************************/

    //How close someone needs to be to the item.
    private const float ITEM_LOOK_DISTANCE_METERS = 3f;

    //How well they need to be looking at the item, aka the dot product
    private const float ITEM_LOOK_DOT_MAX = .95f;

    /***************************************/
    /*********** Dip Properties ************/
    /***************************************/
    //Flag to determine if we should dip;
    private DipState dipState = DipState.WAITING;

    //SmoothDamp dip holder
    private float dipSmoothDamp = 0f;

    //The target amount to dip the camera.
    private float dipTargetAmount = 0f;

    //Local versions of the parameters for dipping to ensure the script can be interrupted.
    private float dipAngle, dipTime, dipRaiseTime;

    //Semaphore for blocking a dip reset because we are already resetting it.
    bool dipResetBlock = false;

    /******************************************/
    /********** Recoil Properties *************/
    /******************************************/

    private float recoilTargetX = 0f;
    private float recoilTargetY = 0f;
    private float recoilX = 0f;
    private float recoilY = 0f;
    private float recoilTimer = 0f;
    private RecoilState recoilState = RecoilState.WAITING;    

    //You know what this flag do.
    private bool initialized = false;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    void Update()
    {
        if (!initialized)
            Initialize();

        //If the cursor is visible at all, do not try to move the camera.
        if (UIManager.Instance.IsCursorVisible)
            return;

        //Handle the head dip if needed.
        DoDip();
        DoRecoil();

        //Calculate the mouse delta since the last frame.
        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;

        adjustedMouseX = (targetMouseDelta.x * MouseSensitivity);
        adjustedMouseY = targetMouseDelta.y * MouseSensitivity;

        //Rotate player along the Y axis.
        gameObject.transform.Rotate(Vector3.up, adjustedMouseX);

        UIManager.Instance.DebugText2 = dipTargetAmount.ToString();

        //Rotate the camera pitch.
        cameraVerticalRotation -= adjustedMouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -CAMERA_MAX_VERTICAL_ROTATION, CAMERA_MAX_VERTICAL_ROTATION);
        pawn.LookAngle = cameraVerticalRotation;
        targetRotation = transform.eulerAngles;
        targetRotation.x = cameraVerticalRotation + dipTargetAmount + -recoilY;

        mainCamera.transform.eulerAngles = targetRotation;

        CheckToWhatPlayerIsLookingAt();

        //Do stuff with the camera when crouching
        //Move the camera mount downward or upward over time. Determine the new position and set the 
        float newCameraMountPosition = Mathf.Clamp(pawn.IsCrouching && pawn.IsGrounded ?
                                        mainCameraMount.localPosition.y - (CROUCH_SPEED * Time.deltaTime) :
                                        mainCameraMount.localPosition.y + (CROUCH_SPEED * Time.deltaTime),
                                        defaultCameraLocalY - CROUCH_HEIGHT, defaultCameraLocalY);

        mainCameraMount.localPosition = new Vector3(mainCameraMount.localPosition.x, newCameraMountPosition, mainCameraMount.localPosition.z);

        //Resize the character controller and recenter it based on what the height should be while crouching.
        characterController.height = Mathf.Clamp(pawn.IsCrouching && pawn.IsGrounded ?
                                        characterController.height - (CROUCH_SPEED * Time.deltaTime) :
                                        characterController.height + (CROUCH_SPEED * Time.deltaTime),
                                        CROUCH_HEIGHT, STAND_HEIGHT);
        characterController.center = Vector3.down * (2f - characterController.height) / 2.0f;
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    private void Initialize()
    {
        defaultCameraLocalY = mainCameraMount.localPosition.y;
        dipState = DipState.WAITING;
        initialized = true;
    }

    //Used to see which item the player is looking at.
    private void CheckToWhatPlayerIsLookingAt()
    {
        //Grab all equippable items.
        EquippableItem[] items = GameObject.FindObjectsOfType<EquippableItem>();
        EquippableItem closestItem = null;
        float closestDot = 0f;
        float currentDot;

        //Loop thru each Item to see if its being looked at.
        foreach (EquippableItem item in items) {

            //If this item is owned by a pawn, we cannot be looking at it.
            if (item.IsAssignedToPawn)
                continue;

            //Is it in range?
            if (Vector3.Distance(item.gameObject.transform.position, mainCamera.transform.position) <= ITEM_LOOK_DISTANCE_METERS)
            {
                //Are we looking at it, and in the event of multiples meeting the above criteria, prefer the closest.
                currentDot = Vector3.Dot(mainCamera.transform.forward, (item.gameObject.transform.position - mainCamera.position).normalized);
                if (currentDot < ITEM_LOOK_DOT_MAX)
                    continue;

                //The higher the dot product, the closer we are to looking at it over others.
                if (currentDot > closestDot)
                {
                    closestItem = item;
                    closestDot = currentDot;
                }
            }
        }

        //Cool, set the item we are looking at, even if its null (Can be looking at nothing)
        pawn.ItemPawnIsLookingAt = closestItem;        
    }

    //Shorthand Dip for skipping the forceReset param and assume false.
    public void DipCamera(float dipAngle, float dipSmoothTime, float dipRaiseSmoothTime)
    {
        DipCamera(dipAngle, dipSmoothTime, dipRaiseSmoothTime, false);            
    }

    //Dip and raise the camera to make vertical transicitons (jumping, landing, etc) look less basic
    public void DipCamera(float dipAngle, float dipSmoothTime, float dipRaiseSmoothTime, bool forceReset)
    {
        //If we are just waiting, calibrate the engine!!!!1
        if (dipState == DipState.WAITING)
        {
            this.dipAngle = dipAngle;
            this.dipTime = dipSmoothTime;
            this.dipRaiseTime = dipRaiseSmoothTime;
            dipState = DipState.LOWERING;
        }
        //If we are mid dip and someone wants to override the dip, then do so.
        else if (dipState != DipState.WAITING && !dipResetBlock && forceReset)
        {
            dipResetBlock = true;
            this.dipAngle = dipAngle;
            this.dipTime = dipSmoothTime;
            this.dipRaiseTime = dipRaiseSmoothTime;
            dipState = DipState.LOWERING;
        }
    }
    
    //DO the actual dip!!
    private void DoDip()
    {
        //First state, do nothing
        if (dipState == DipState.WAITING)
        {
            dipTargetAmount = 0f;
        }
        //Lowering state, the camera will sort of "Drag" down when you jump or land.
        else if (dipState == DipState.LOWERING)
        {
            //Smoothdamp may not actually hit the value we pass in, it may be millionths off, so append .1 to the value to ensure we hit out target.
            dipTargetAmount = Mathf.SmoothDamp(dipTargetAmount, dipAngle + .1f, ref dipSmoothDamp, dipTime);
            if (dipTargetAmount >= dipAngle)
            {
                dipTargetAmount = dipAngle;
                dipState = DipState.RAISING;
            }
        }
        //Raising state, basically, SmoothDamp back to angle '0'.
        else if (dipState == DipState.RAISING) {

            //Smoothdamp may not actually hit the value we pass in, it may be millionths off, so append -.1 to the value to ensure we hit out target.
            dipTargetAmount = Mathf.SmoothDamp(dipTargetAmount, -.1f, ref dipSmoothDamp, dipRaiseTime);
            if (dipTargetAmount <= 0)
            {
                dipTargetAmount = 0;
                dipResetBlock = false;
                dipState = DipState.WAITING;
            }
        }
    }

    //Set how much the camera should recoil.
    public void RecoilCamera(float x, float y)
    {
        recoilState = RecoilState.RAISING;
        recoilTimer = 0;
        recoilTargetX = x * (Random.Range(1, 3) == 1 ? -1 : 1);
        recoilTargetY = y;
    } 
    
    //Perform the recoil, if necessary
    private void DoRecoil()
    {
        //If we are waiting for recoil to happen, do nothing.
        if (recoilState == RecoilState.WAITING)
            return;

        //For raising, very quickly lerp up.
        if (recoilState == RecoilState.RAISING)
        {
            recoilTimer += Time.deltaTime;

            recoilX = Mathf.Lerp(0, recoilTargetX, recoilTimer);
            recoilY = Mathf.Lerp(0, recoilTargetY, recoilTimer);
            if (recoilTimer >= .05)
            {
                recoilState = RecoilState.LOWERING;
                recoilTimer = 0;
            }
        }
        //For lowering, lets be fancy in how we lerp back.
        else if (recoilState == RecoilState.LOWERING)
        {
            recoilTimer += Time.deltaTime;
            float t = recoilTimer / 1;
            t = EasingUtils.Exponential.Out(t);
            recoilX = Mathf.Lerp(recoilTargetX, 0, t);
            recoilY = Mathf.Lerp(recoilTargetY, 0, t);
            if (recoilTimer >= 1)
            {
                recoilState = RecoilState.WAITING;
            }
        }
    }
}

enum RecoilState
{
    WAITING,
    LOWERING,
    RAISING
}

enum DipState
{
    WAITING,
    LOWERING,
    RAISING
}