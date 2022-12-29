using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnLook : MonoBehaviour
{
    /*** Local References to Unity Objects ***/

    //Main GameObject controlled by the player.
    [SerializeField]
    private Pawn pawn;

    //Reference for the camera attached to the pawn.
    [SerializeField]
    private Transform pawnCamera;

    //Used to indicate which objects the player can "see" for pickup.
    [SerializeField]
    private LayerMask lookLayerMask;

    /*** Class properties ***/

    ///Camera Properties
    //////////////////////////////////////////////

    //Used to clamp the camera to prevent the users neck from doing vertical 360s.
    private float cameraVerticalRotation = 0f;
    private readonly float CAMERA_MAX_VERTICAL_ROTATION = 85;

    ///Mouse Properties
    //////////////////////////////////////////////

    //Values of the mouse delta from the last frame, adjusted for sensitivity.
    private float adjustedMouseX;
    private float adjustedMouseY;

    //Mouse Sensitivity
    public float MouseSensitivity = 15;

    /*** Unity Methods ***/

    void Update()
    {
        //If the cursor is visible at all, do not try to move the camera.
        if (UIManager.Instance.IsCursorVisible())
            return;

        //Calculate the mouse delta since the last frame.
        Vector2 targetMouseDelta = Mouse.current.delta.ReadValue() * Time.smoothDeltaTime;

        adjustedMouseX = targetMouseDelta.x * MouseSensitivity;
        adjustedMouseY = targetMouseDelta.y * MouseSensitivity;

        //Rotate player along the Y axis.
        gameObject.transform.Rotate(Vector3.up, adjustedMouseX);

        //Rotate the camera pitch.
        cameraVerticalRotation -= adjustedMouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -CAMERA_MAX_VERTICAL_ROTATION, CAMERA_MAX_VERTICAL_ROTATION);
        Vector3 targetRoation = transform.eulerAngles;
        targetRoation.x = cameraVerticalRotation;
        pawnCamera.transform.eulerAngles = targetRoation;

        checkToWhatPlayerIsLookingAt();

    }

    /*** Class Methods ***/

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