using UnityEngine;

//Script to control the pawn head bob when the pawn is moving.
public class PawnHeadBob : MonoBehaviour
{
    /*****************************************/
    /*** Local References to Unity Objects ***/
    /*****************************************/

    //This guy again.
    [SerializeField]
    private Pawn pawn;

    //Main Camera to head bob.
    [SerializeField]
    private Transform mainCamera;

    /***********************/
    /*** Class Variables ***/
    /***********************/

    //How fast the bobbing should occur.
    [SerializeField]
    float walkBobSpeed = 14f;

    //How much the bobbing should occur.
    [SerializeField]
    float walkBobAmount = .05f;

    //How fast the bobbing should occur, crouch edition.
    [SerializeField]
    float crouchBobSpeed = 5f;

    //How much the bobbing should occur, crouch edition.
    [SerializeField]
    float crouchBobAmount = .08f;

    //Default Y position of the camera when standing still.
    private float defaultYPosition = 0;

    //these both work toward head bobbing which I googled how to do.
    private float timer;
    private float increment;

    /*****************/
    /* Unity Methods */
    /*****************/
    void Awake()
    {
        //Set the home Y position for the camera.
        defaultYPosition = mainCamera.localPosition.y;
    }

    void Update()
    {
        //Don't head bob in the air.
        if (!pawn.IsGrounded && !pawn.IsWallRunning)
            return;

        //DO the head bobbing if we are moving fast enough.
        if (pawn.IsMovingFasterThan(3f) && pawn.IsTryingToMove)
        {
            //Simple SIN wave timer to move the camera vertically.
            timer += Time.deltaTime * (pawn.IsCrouching ? crouchBobSpeed : walkBobSpeed);
            increment = Mathf.Sin(timer) * (pawn.IsCrouching ? crouchBobAmount : walkBobAmount);

            mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, defaultYPosition + increment, mainCamera.localPosition.z);
        }
        //ELSE, Lerp the camera back into place.
        else 
        {
            mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, Mathf.Lerp(mainCamera.localPosition.y, defaultYPosition, .1f), mainCamera.localPosition.z);

            if (Mathf.Abs(mainCamera.localPosition.y - defaultYPosition) < .001f)
              mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, defaultYPosition, mainCamera.localPosition.z);
        }      
    }

}
