using UnityEngine;

public class PawnCrouch : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Transform mainCamera;

    private CharacterController characterController;

    private bool initialized = false;

    private const float STAND_HEAD_Y = 1;
    private const float CROUCH_HEAD_Y = .5f;
    private const float CROUCH_SPEED = 2f;

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            Initialize();

        if (pawn.IsGrounded && pawn.IsCrouching)
        {
            pawn.Drag = 5f;
        }
        else
        {
            pawn.Drag = 0;
        }

        
        float newpos = Mathf.Clamp(pawn.IsCrouching ?
                                        mainCamera.localPosition.y - (CROUCH_SPEED * Time.deltaTime) :
                                        mainCamera.localPosition.y + (CROUCH_SPEED * Time.deltaTime),
                                        CROUCH_HEAD_Y, STAND_HEAD_Y);

        mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, newpos, mainCamera.localPosition.z);
    }

    private void Initialize() {
        characterController = pawn.CharacterController;
        initialized = true;
    }
}
