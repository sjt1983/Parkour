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

    //How much drag we want to add per second;
    private const float SLIDING_DRAG = 3f;

    //How fast we need to be going to slide.
    private const int MINIMUM_SLIDE_VELOCITY = 2;

    //Minimum Angle to slide
    private const float PAWN_DOWNHILL_SLIDE_ANGLE = 1f;

    //Y positon of the last frame, if its higher than this frame, we can state we are sliding downhill.
    private float lastFrameY;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    void Update()
    {
        if (!initialized)
            Initialize();

        //Rules for sliding
        //Pawn has to be on the ground, crouching, and moving faster than the MINIMUM_SLIDE_VELOCITY.
        if (pawn.IsGrounded && pawn.IsCrouching && pawn.ForwardSpeed > MINIMUM_SLIDE_VELOCITY)
        {
            //Set the pawn to a sliding state for the movement script to handle.
            pawn.IsSliding = true;

            //If we are on an angle and sliding downhill, allow infinite sliding, otherwise add to the drag.
            if (GetPawnSlopeAngle() < PAWN_DOWNHILL_SLIDE_ANGLE && IsSlidingDownhill())
                pawn.Drag = 0;
            else
                pawn.Drag += SLIDING_DRAG * Time.deltaTime;
        }
        else
        {
            //If we aren't sliding, don't slide, LOL! GENIUS!
            pawn.IsSliding = false;
            pawn.Drag = 0;
        }


        
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

        //Store the lastY position so we know if we are going downhill or not.
        lastFrameY = transform.position.y;
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    private void Initialize() {
        defaultCameraLocalY = mainCameraMount.localPosition.y;
        initialized = true;
        lastFrameY = transform.position.y;
    }

    //Determines if we are sliding downhill by checking the current Y position with last frames.
    private bool IsSlidingDownhill()
    {
        return transform.position.y < lastFrameY;
    }

    //Try to raycast down and see if we are hitting a sloped surface, if we are not, just return 1 and assume we are not.
    private float GetPawnSlopeAngle()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.02f, LayerMask.GetMask("MapGeometry")))
        {
            return hit.normal.y;
        }
        return 1; 
    }
}
