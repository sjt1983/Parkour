using UnityEngine;

public class PawnCrouch : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Transform cameraHolder;

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
                                        cameraHolder.localPosition.y - (CROUCH_SPEED * Time.deltaTime) :
                                        cameraHolder.localPosition.y + (CROUCH_SPEED * Time.deltaTime),
                                        CROUCH_HEAD_Y, STAND_HEAD_Y);

        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, newpos, cameraHolder.localPosition.z);
    }

    private void Initialize() {
        characterController = pawn.CharacterController;
        initialized = true;
    }
}
