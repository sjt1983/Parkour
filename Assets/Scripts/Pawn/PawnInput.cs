using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnInput : MonoBehaviour
{
    /*** References to Unity Objects ***/
    //none!

    /*** Class Properties ***/

    //2D Movement Directions
    //////////////////////////////////
    ///
    public float HorizontalDirection { get; set; }
    public float VerticalDirection { get; set; }

    ///Actions
    //////////////////////////

    //Player is attempting to Interact with an object in the world, e.g. Pickup Item
    public bool Interacting { get; set; }

    //Player is using their selected item, e.g. shoot gun.
    public bool PrimaryUse { get; set; }

    //Player is using their selected item with alternate action, e.g. ADS/Zoom
    public bool SecondaryUse { get; set; }

    //Sprinting!!!
    public bool Sprinting { get; set; }

    //Crouching
    public bool Crouching { get; set; }

    private bool MenuOpen = false;

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

    /*** Local Class Variables ***/

    //Reference to the Player Input System
    private PlayerInputSystem playerInputSystem;

    //Quick Reference to the movement controls (WSAD)
    private InputAction movementInput;

    //Calculated direction the player is moving
    private Vector2 movementDirection;

    /* Unity Methods */

    private void Awake()
    {
        UIManager.Instance.Initialize();
        UIManager.Instance.Show<ActionUI>();
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

        //Menu
        playerInputSystem.PlayerControls.Menu.Enable();
        playerInputSystem.PlayerControls.Menu.performed += ctx =>
        {
            MenuOpen = !MenuOpen;
            if (MenuOpen)
            {
                UIManager.Instance.Show<ActionMenuUI>();                
            }
            else
            {
                UIManager.Instance.Show<ActionUI>();
            }            

        };

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
        HorizontalDirection = movementDirection.x;
        VerticalDirection = movementDirection.y;

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