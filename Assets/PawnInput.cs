using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnInput : MonoBehaviour
{
    //Reference to the Unity Input System
    private PlayerInputSystem playerInputSystem;
    private InputAction movementInput;

    //2D Movement Directions
    public float horizontalDirection;
    public float verticalDirection;
    private Vector2 movementDirection;

    /** Actions **/

    //Player is attempting to Interact with an object in the world, e.g. Pickup Item
    public bool Interacting;

    //Player is using their selected item, e.g. shoot gun.
    public bool PrimaryUse;

    //Player is using their selected item with alternate action, e.g. ADS/Zoom
    public bool SecondaryUse;

    //Sprinting!!!
    public bool Sprinting;

    //Crouching
    public bool Crouching;

    //Player hit the jump button.
    //We want to capture that the user jumped.
    //but as soon as something checks that the user hit the jump button, set it to false so we dont get in a jump loop.
    private bool jump = false;
    public bool Jumping
    {
        get
        {
            bool retval = jump;
            jump = false;
            return retval;
        }
        set => jump = value;
    }

    //Player attempted to reload a weapon.
    //Same deal as before, we want to ensure we capture the input but as soon as we check for it, set it to false.
    //This pattern may kind of suck and be changes later.
    private bool reloading;
    public bool Reloading
    {
        get
        {
            bool retval = reloading;
            reloading = false;
            return retval;
        }
        set => reloading = value;
    }

    private void Awake()
    {
        //Lets setup the controller
        playerInputSystem = new PlayerInputSystem();
        movementInput = playerInputSystem.PlayerControls.Move;

        //Interaction
        playerInputSystem.PlayerControls.Interact.Enable();

        //Primary Use
        playerInputSystem.PlayerControls.PrimaryUse.Enable();

        //Secondary Use
        playerInputSystem.PlayerControls.SecondaryUse.Enable();

        //Reload
        playerInputSystem.PlayerControls.Reload.Enable();

        //Jump
        playerInputSystem.PlayerControls.Jump.Enable();

        //Sprint
        playerInputSystem.PlayerControls.Sprint.Enable();

        //Crouch
        playerInputSystem.PlayerControls.Crouch.Enable();

        //Enable the controller.
        movementInput.Enable();



    }

    private void Update()
    {

        //Move the player.
        //movementInput.ReadValue returns a Vector2 to see which movement buttons are pressed.
        //We assigned those to variables to indicate certain directions are "on" (Forward/backward/strafe left/strafe right)
        //PawnMovement script reads these values and determines what to do.
        movementDirection = movementInput.ReadValue<Vector2>();
        horizontalDirection = movementDirection.x;
        verticalDirection = movementDirection.y;


        //Handle Interaction
        if (playerInputSystem.PlayerControls.Interact.WasPressedThisFrame())
        {
            Interacting = true;
        }

        if (playerInputSystem.PlayerControls.Interact.WasReleasedThisFrame())
        {
            Interacting = false;
        }

        //Handle Primary Use
        if (playerInputSystem.PlayerControls.PrimaryUse.WasPressedThisFrame())
        {
            PrimaryUse = true;
        }

        if (playerInputSystem.PlayerControls.PrimaryUse.WasReleasedThisFrame())
        {
            PrimaryUse = false;
        }

        //Handle Secondary Use
        if (playerInputSystem.PlayerControls.SecondaryUse.WasPressedThisFrame())
        {
            SecondaryUse = true;
        }

        if (playerInputSystem.PlayerControls.SecondaryUse.WasReleasedThisFrame())
        {
            SecondaryUse = false;
        }

        //Handle Interaction
        if (playerInputSystem.PlayerControls.Jump.WasPressedThisFrame())
        {
            Jumping = true;
        }
        if (playerInputSystem.PlayerControls.Jump.WasReleasedThisFrame())
        {
            Jumping = false;
        }

        //Handle Reload
        if (playerInputSystem.PlayerControls.Reload.WasPressedThisFrame())
        {
            Reloading = true;
        }

        //Handle Sprint
        if (playerInputSystem.PlayerControls.Sprint.WasPressedThisFrame())
        {
            Sprinting = true;
        }

        if (playerInputSystem.PlayerControls.Sprint.WasReleasedThisFrame())
        {
            Sprinting = false;
        }

        //Handle Sprint
        if (playerInputSystem.PlayerControls.Crouch.WasPressedThisFrame())
        {
            Crouching = true;
        }

        if (playerInputSystem.PlayerControls.Crouch.WasReleasedThisFrame())
        {
            Crouching = false;
        }
    }
}