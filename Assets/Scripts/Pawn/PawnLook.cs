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

    /*** Camera Properties ***/

    //Used to clamp the camera to prevent the users neck from doing vertical 360s.
    private float cameraVerticalRotation = 0f;
    private readonly float CAMERA_MAX_VERTICAL_ROTATION = 85;

    /*** Mouse Properties ***/

    //Values of the mouse delta from the last frame, adjusted for sensitivity.
    private float adjustedMouseX;
    private float adjustedMouseY;


    /*** Crouching Properties ***/

    //Default local camera Y local position
    private float defaultCameraLocalY = 1f;

    //How tall the character should be when crouching.
    private const float CROUCH_HEIGHT = 1f;

    //How tall the character should be when crouching.
    private const float STAND_HEIGHT = 2f;

    //How fast the charatcer should crouch, essentially, meters per second.
    private const float CROUCH_SPEED = 6f;

    //Mouse Sensitivity
    public float MouseSensitivity = 15;

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

        //Calculate the mouse delta since the last frame.
        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;

        adjustedMouseX = targetMouseDelta.x * MouseSensitivity;
        adjustedMouseY = targetMouseDelta.y * MouseSensitivity;

        //Rotate player along the Y axis.
        gameObject.transform.Rotate(Vector3.up, adjustedMouseX);

        //Rotate the camera pitch.
        cameraVerticalRotation -= adjustedMouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -CAMERA_MAX_VERTICAL_ROTATION, CAMERA_MAX_VERTICAL_ROTATION);
        pawn.LookAngle = cameraVerticalRotation;
        Vector3 targetRoation = transform.eulerAngles;
        targetRoation.x = cameraVerticalRotation;
        mainCamera.transform.eulerAngles = targetRoation;

        checkToWhatPlayerIsLookingAt();

        //Move the camera mount downward or upward over time. Determine the new position and set the 
        float newCameraMountPosition = Mathf.Clamp(pawn.IsCrouching ?
                                        mainCameraMount.localPosition.y - (CROUCH_SPEED * Time.deltaTime) :
                                        mainCameraMount.localPosition.y + (CROUCH_SPEED * Time.deltaTime),
                                        defaultCameraLocalY - CROUCH_HEIGHT, defaultCameraLocalY);

        mainCameraMount.localPosition = new Vector3(mainCameraMount.localPosition.x, newCameraMountPosition, mainCameraMount.localPosition.z);

        //Resize the character controller and recenter it based on what the height should be while crouching.
        characterController.height = Mathf.Clamp(pawn.IsCrouching ?
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
        initialized = true;
    }

    //Eventually used to help pickup objects.
    private void checkToWhatPlayerIsLookingAt()
    {
        Item[] items = GameObject.FindObjectsOfType<Item>();
        Item closestItem = null;
        float closestDot = 3.1f;
        float currentDot = 0f;
        foreach (Item item in items) {
            Debug.Log(item.name);
            if (Vector3.Distance(item.gameObject.transform.position, transform.position) < 3)
            {
                currentDot = Vector3.Dot(item.gameObject.transform.position, transform.position);
                if (currentDot < closestDot)
                {
                    closestItem = item;
                    closestDot = currentDot;
                }
            }
        }
        pawn.ItemPawnIsLookingAt = closestItem;

        if (closestItem != null)
        {
            UIManager.Instance.DebugText1 = closestItem.name;
        }
    }
}