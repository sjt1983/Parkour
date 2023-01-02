using UnityEngine;

public class PawnCrouch : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Transform mainCameraMount;

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

        UIManager.Instance.DebugText1 = pawn.IsGrounded.ToString();

        if (pawn.IsGrounded && pawn.IsCrouching && pawn.ForwardSpeed > 5)
        {
            pawn.IsSliding = true;
            pawn.Drag = 3f;
        }
        else
        {
            pawn.IsSliding = false;
            pawn.Drag = 0;
        }
        
        float newpos = Mathf.Clamp(pawn.IsCrouching ?
                                        mainCameraMount.localPosition.y - (CROUCH_SPEED * Time.deltaTime) :
                                        mainCameraMount.localPosition.y + (CROUCH_SPEED * Time.deltaTime),
                                        CROUCH_HEAD_Y, STAND_HEAD_Y);

        mainCameraMount.localPosition = new Vector3(mainCameraMount.localPosition.x, newpos, mainCameraMount.localPosition.z);
    }

    private void Initialize() {        
        initialized = true;
    }
}
