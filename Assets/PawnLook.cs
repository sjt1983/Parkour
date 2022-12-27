using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnLook : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnInput pawnInput;

    //Refernce for the camera attached to the pawn.
    [SerializeField]
    private Transform pawnCamera;

    [SerializeField]
    private LayerMask lookLayerMask;

    //Used to clamp the camera to prevent the users neck from doing vertical 360s.
    private float cameraVerticalRotationClamp = 85;
    private float cameraVerticalRotation = 0f;

    /** Camera/Mouse Variables. **/

    //Values of the mouse delta from the last fram, adjusted for sensitivity.
    public float AdjustedMouseX;
    public float AdjustedMouseY;
    private float sensitivity = 15;

    private void Awake()
    {
        //Lock the cursor to the window
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;

        AdjustedMouseX = targetMouseDelta.x * sensitivity;
        AdjustedMouseY = targetMouseDelta.y * sensitivity;



        //Rotate player.
        gameObject.transform.Rotate(Vector3.up, AdjustedMouseX);

        //Move camera up
        cameraVerticalRotation -= AdjustedMouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalRotationClamp, cameraVerticalRotationClamp);
        Vector3 targetRoation = transform.eulerAngles;
        targetRoation.x = cameraVerticalRotation;
        pawnCamera.transform.eulerAngles = targetRoation;

        checkToWhatPlayerIsLookingAt();

    }

    private void checkToWhatPlayerIsLookingAt()
    {
       /* if (Physics.Raycast(pawnCamera.position, pawnCamera.forward, out var hit, 3, lookLayerMask))
        {
            GameObject gameObjectPlayerIsLookingAt = hit.collider.gameObject;
            if (gameObjectPlayerIsLookingAt.layer == 6)
            {
                pawn.ItemPawnIsLookingAt = gameObjectPlayerIsLookingAt.GetComponent<InteractableItem>();
            }
        }
        else
        {
            pawn.ItemPawnIsLookingAt = null;
        }*/
    }
}