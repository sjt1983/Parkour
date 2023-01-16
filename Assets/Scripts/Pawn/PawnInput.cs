using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PawnInput : MonoBehaviour
{
    /***********************************/
    /*** References to Unity Objects ***/
    /***********************************/
    //none!

    /************************/
    /*** Class Properties ***/
    /************************/

    //2D Movement Directions
    public float HorizontalDirection { get; set; }
    public float VerticalDirection { get; set; }

    
    /*** Actions ***/

    //Player is attempting to Interact with an object in the world, e.g. Pickup Item
    public bool Interacting { get; set; }

    //Player is using their selected item, e.g. shoot gun.
    public bool PrimaryUse { get; set; }

    //Player is using their selected item with alternate action, e.g. ADS/Zoom
    public bool SecondaryUse { get; set; }

    //Crouching
    public bool Crouching { get; set; }

    //Jumping 

    //Player hit the jump button.
    //We want to capture that the user jumped.
    //but as soon as something checks that the user hit the jump button, set it to false so we dont get in a jump loop.
    private bool jump = false;
    public bool JumpedThisFrame
    {
        get
        {
            bool retval = jump;
            jump = false;
            return retval;
        }
        set => jump = value;
    }

    //Simple notification for scripts to know if the jump button is pressed.
    public bool JumpIsPressed { get => actionController.PlayerControls.Jump.IsPressed(); }

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

    /*************************/
    /*** Private variables ***/
    /*************************/

    //Flag to indicate the menu is open
    private bool menuOpen = false;

    //Reference to the Player Input System
    private ActionController actionController;

    //Quick Reference to the movement controls (WSAD)
    private InputAction movementInput;

    //Calculated direction the player is moving
    private Vector2 movementDirection;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    private void Awake()
    {
        UIManager.Instance.Initialize();
        UIManager.Instance.Show<ActionUI>();
        //Lets setup the controller
        actionController = new ActionController();
        movementInput = actionController.PlayerControls.Move;

        //Interaction
        actionController.PlayerControls.Interact.Enable();

        //Primary Use
        actionController.PlayerControls.PrimaryUse.Enable();

        //Secondary Use
        actionController.PlayerControls.SecondaryUse.Enable();

        //Reload
        actionController.PlayerControls.Reload.Enable();

        //Jump
        actionController.PlayerControls.Jump.Enable();

        //Crouch
        actionController.PlayerControls.Crouch.Enable();

        //Menu
        actionController.PlayerControls.Menu.Enable();
        actionController.PlayerControls.Menu.performed += ctx =>
        {
            menuOpen = !menuOpen;
            if (menuOpen)
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
        if (actionController.PlayerControls.Interact.WasPressedThisFrame())
        {
            Interacting = true;
        }

        if (actionController.PlayerControls.Interact.WasReleasedThisFrame())
        {
            Interacting = false;
        }

        //Handle Primary Use
        if (actionController.PlayerControls.PrimaryUse.WasPressedThisFrame())
        {
            PrimaryUse = true;
        }

        if (actionController.PlayerControls.PrimaryUse.WasReleasedThisFrame())
        {
            PrimaryUse = false;
        }

        //Handle Secondary Use
        if (actionController.PlayerControls.SecondaryUse.WasPressedThisFrame())
        {
            SecondaryUse = true;
        }

        if (actionController.PlayerControls.SecondaryUse.WasReleasedThisFrame())
        {
            SecondaryUse = false;
        }

        //Handle Jumping
        if (actionController.PlayerControls.Jump.WasPressedThisFrame())
        {
            JumpedThisFrame = true;
        }
        else { 
            JumpedThisFrame = false;
        }

        //Handle Reload
        if (actionController.PlayerControls.Reload.WasPressedThisFrame())
        {
            Reloading = true;
        }

        //Handle Crouch
        if (actionController.PlayerControls.Crouch.WasPressedThisFrame())
        {
            Crouching = true;
        }

        if (actionController.PlayerControls.Crouch.WasReleasedThisFrame())
        {
            Crouching = false;
        }
    }
}