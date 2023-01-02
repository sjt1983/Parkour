using UnityEngine;

public class PawnHeadBob : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Transform mainCamera;

    [SerializeField]
    float walkBobSpeed = 14f;

    [SerializeField]
    float walkBobAmount = .01f;

    [SerializeField]
    float crouchBobSpeed = 5f;

    [SerializeField]
    float crouchBobAmount = .01f;

    private float defaultYPosition = 0;
    private float defaultXPosition = 0;
    private float timer;

    private float increment;

    void Awake()
    {
        defaultYPosition = mainCamera.localPosition.y;
        defaultXPosition = mainCamera.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pawn.IsGrounded)
            return;

        if (pawn.IsMovingFasterThan(.1f) && pawn.IsTryingToMove)
        {
            bobTheHead();
        }
        else if (mainCamera.localPosition.y != defaultYPosition)
        {
            bobTheHead();

            if (Mathf.Abs(mainCamera.localPosition.y - defaultYPosition) < .001f)
              mainCamera.localPosition = new Vector3(mainCamera.localPosition.x, defaultYPosition, mainCamera.localPosition.z);
        }      
    }

    private void bobTheHead()
    {
        timer += Time.deltaTime * (pawn.IsCrouching ? crouchBobSpeed : walkBobSpeed);
        increment = Mathf.Sin(timer) * (pawn.IsCrouching ? crouchBobAmount : walkBobAmount);

        mainCamera.localPosition = new Vector3(
            mainCamera.localPosition.x,
            defaultYPosition + increment,
            mainCamera.localPosition.z);
    }
}
