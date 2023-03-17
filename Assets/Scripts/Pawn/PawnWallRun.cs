using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnWallRun : MonoBehaviour
{
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private PawnInput pawnInput;

    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private PawnMovement pawnMovement;

    [SerializeField]
    private Sensor wallRunSensor;

    private bool checkLeft = true;

    //The layer mask we can hit when looking to vault.
    private LayerMask layerMask;

    private WallRunState wallRunState = WallRunState.CHECKING;

    private Vector3 lerpFromPosition;
    private Vector3 lerpToPosition;
    private float lerpTimer = 0f;

    private Vector3 forwardVector;
    private Vector3 raycastOrigin;

    private float raycastTimer = 0f;
    private void Awake()
    {
        layerMask = LayerMask.GetMask("MapGeometry");
    }

    // Update is called once per frame
    void Update()
    {
        if (pawnMovement.CurrentYSpeed <= 0 && !pawn.IsGrounded && pawn.SpeedCharges > 0 && pawnInput.ParkourPressed && wallRunState == WallRunState.CHECKING)
        {
            raycastOrigin = transform.position + -transform.up * .5f + (transform.forward * 1f);
            checkLeft = !checkLeft;
            if (Physics.Raycast(raycastOrigin, (checkLeft ? -transform.right : transform.right), out RaycastHit hitInfo, .55f, layerMask))
            {
                lerpTimer = 0f;
                Quaternion rotation = Quaternion.FromToRotation(checkLeft ? Vector3.right : -Vector3.right, hitInfo.normal);
                pawnMovement.LastGroundedFrameAngle = rotation.eulerAngles.y;
                forwardVector = ParkourUtils.GenerateDirectionalForceVector(rotation, pawnMovement.CurrentGroundedZSpeed);
                pawnMovement.MovementLocked = true;
                lerpFromPosition = transform.position;
                lerpToPosition = hitInfo.point + new Vector3(0, 1, 0);
                wallRunState = WallRunState.ADJUSTING;
                pawnLook.TargetZAngle = checkLeft ? -15 : 15;
            }
        }
        else if (wallRunState == WallRunState.ADJUSTING)
        {
            lerpTimer += Time.deltaTime;
            float t = lerpTimer / .2f;
            transform.position = Vector3.Lerp(lerpFromPosition, lerpToPosition, t);

            if (Vector3.Distance(lerpToPosition, transform.position) < .5)
            {
                wallRunState = WallRunState.CLAMPED;
            }
        }
        else if (wallRunState == WallRunState.CLAMPED)
        {
            raycastTimer += Time.deltaTime;

            if (raycastTimer > .15 && !Physics.Raycast(transform.position, (checkLeft ? -transform.right : transform.right), out RaycastHit hitInfo, 1f, layerMask))
            {
                pawnMovement.MovementLocked = false;
                wallRunState = WallRunState.CHECKING;
                pawnLook.TargetZAngle = 0;
                raycastTimer = 0f;
            }

            CollisionFlags flags = pawn.Move(forwardVector * Time.deltaTime);
            if ((flags & CollisionFlags.CollidedSides) != 0)
            {
                pawnMovement.MovementLocked = false;
                wallRunState = WallRunState.CHECKING;
                pawnLook.TargetZAngle = 0;
            }

            if (pawnInput.JumpPressedThisFrame)
            {
                pawnMovement.TransferState(forwardVector + ((checkLeft ? Vector3.right : -Vector3.right) * 10), 10);
                pawnMovement.MovementLocked = false;
                wallRunState = WallRunState.CHECKING;
                pawnLook.TargetZAngle = 0;
            }

            if (pawnInput.ZDirection <= 0)
            {
               // pawnMovement.TransferState(forwardVector + ((checkLeft ? transform.right : -transform.right) * 2), 0);
                pawnMovement.MovementLocked = false;
                wallRunState = WallRunState.CHECKING;
                pawnLook.TargetZAngle = 0;
            }
        }
    }
}

enum WallRunState
{
    CHECKING,
    ADJUSTING,
    CLAMPED
}
