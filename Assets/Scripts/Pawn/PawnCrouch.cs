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
    
    //Target camera *LOCAL* position when the character is standing.
    private const float STANDING_HEAD_Y = 1f;

    //Target camera *LOCAL* position when the character is crouching.
    private const float CROUCHING_HEAD_Y = 0f;

    //How fast the charatcer should crouch, essentially, meters per second.
    private const float CROUCH_SPEED = 6f;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    void Update()
    {
        if (!initialized)
            Initialize();

        //Rules for sliding
        //Pawn has to be on the ground, crouching, and moving faster than 5 m/s.
        if (pawn.IsGrounded && pawn.IsCrouching && pawn.ForwardSpeed > 5)
        {
            //Set the pawn to a sliding state for the movement script to handle.
            pawn.IsSliding = true;
            //Set the drag if we are on a sloped surface.
            pawn.Drag = pawn.IsOnSlopedSurface ? 0f : 3f;
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
                                        CROUCHING_HEAD_Y, STANDING_HEAD_Y);

        mainCameraMount.localPosition = new Vector3(mainCameraMount.localPosition.x, newCameraMountPosition, mainCameraMount.localPosition.z);

       /* characterController.height = Mathf.Clamp(pawn.IsCrouching ?
                                        characterController.height - (CROUCH_SPEED * Time.deltaTime) :
                                        characterController.height + (CROUCH_SPEED * Time.deltaTime),
                                        CROUCH_HEAD_Y, STAND_HEAD_Y);*/
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    private void Initialize() {
        initialized = true;
    }
}
