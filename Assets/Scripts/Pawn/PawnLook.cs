using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnLook : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnInput pawnInput;

    //Reference for the camera attached to the pawn.
    [SerializeField]
    private Transform pawnCamera;

    [SerializeField]
    private LayerMask lookLayerMask;

    /** Camera/Mouse Variables. **/

    //Used to clamp the camera to prevent the users neck from doing vertical 360s.
    private readonly float cameraVerticalRotationClamp = 85;
    private float cameraVerticalRotation = 0f;

    //Values of the mouse delta from the last fram, adjusted for sensitivity.
    private float adjustedMouseX;
    private float adjustedMouseY;

    //Mouse
    public float MouseSensitivity = 15;

    void Update()
    {
        //If the cursor is visible at all, do not try to move the camera.
        if (UIManager.Instance.IsCursorVisible())
            return;

        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;

        adjustedMouseX = targetMouseDelta.x * MouseSensitivity;
        adjustedMouseY = targetMouseDelta.y * MouseSensitivity;

        //Rotate player.
        gameObject.transform.Rotate(Vector3.up, adjustedMouseX);

        //Move camera up
        cameraVerticalRotation -= adjustedMouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalRotationClamp, cameraVerticalRotationClamp);
        Vector3 targetRoation = transform.eulerAngles;
        targetRoation.x = cameraVerticalRotation;
        pawnCamera.transform.eulerAngles = targetRoation;

        checkToWhatPlayerIsLookingAt();

    }

    //Eventually used to help pickup objects.
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