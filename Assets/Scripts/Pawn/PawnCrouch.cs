using UnityEngine;

//Script used to handle crouching behavior.
public class PawnCrouch : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    //The one and only pawn.
    [SerializeField]
    private Pawn pawn;
    
    //Main Camera Mount
    [SerializeField]
    private Transform mainCameraMount;

    //Character Controller
    [SerializeField]
    private CharacterController characterController;

    /********************************/
    /*** Private Class Properties ***/
    /********************************/
    
    //You know what this flag do.
    private bool initialized = false;

    //Default local camera Y local position
    private float defaultCameraLocalY = 1f;

    //How tall the character should be when crouching.
    private const float CROUCH_HEIGHT = 1f;

    //How tall the character should be when crouching.
    private const float STAND_HEIGHT = 2f;

    //How fast the charatcer should crouch, essentially, meters per second.
    private const float CROUCH_SPEED = 6f;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    void Update()
    {
        if (!initialized)
            Initialize();

                       
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

    private void Initialize() {
        defaultCameraLocalY = mainCameraMount.localPosition.y;
        initialized = true;
    }

 
}
