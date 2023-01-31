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
    public bool Crouching { get => actionController.PlayerControls.Crouch.IsPressed(); }

    //Jumping 

    //Player hit the jump button.
    public bool JumpedThisFrame { get => actionController.PlayerControls.Jump.WasPressedThisFrame(); }

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
    private Vector2 inputMovementDirection;

    //Vector used to Smooth out character movement.
    private Vector2 smoothMovementDirection;

    //Final smoothed out vector
    private Vector2 finalMovementDirection;

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

        actionController.PlayerControls.Interact.Enable();
        actionController.PlayerControls.PrimaryUse.Enable();
        actionController.PlayerControls.SecondaryUse.Enable();
        actionController.PlayerControls.Reload.Enable();
        actionController.PlayerControls.Jump.Enable();
        actionController.PlayerControls.Crouch.Enable();

        actionController.PlayerControls.Menu.Enable();
        actionController.PlayerControls.Menu.performed += ctx =>
        {
            menuOpen = !menuOpen;
            if (menuOpen)
                UIManager.Instance.Show<ActionMenuUI>();                
            else
                UIManager.Instance.Show<ActionUI>();

        };

        //Enable the controller.
        movementInput.Enable();
    }

    private void Update()
    {
        //Move the player.
        //Standard 2d movement reads with SmoothDamp
        inputMovementDirection = movementInput.ReadValue<Vector2>();
        finalMovementDirection = Vector2.SmoothDamp(finalMovementDirection, inputMovementDirection, ref smoothMovementDirection, .2f);
        HorizontalDirection = finalMovementDirection.x;
        VerticalDirection = finalMovementDirection.y;

        //Handle Interaction
        if (actionController.PlayerControls.Interact.WasPressedThisFrame())
            Interacting = true;
        else if (actionController.PlayerControls.Interact.WasReleasedThisFrame())
            Interacting = false;

        //Handle Primary Use
        if (actionController.PlayerControls.PrimaryUse.WasPressedThisFrame())
            PrimaryUse = true;
        else if (actionController.PlayerControls.PrimaryUse.WasReleasedThisFrame())
            PrimaryUse = false;

        //Handle Secondary Use
        if (actionController.PlayerControls.SecondaryUse.WasPressedThisFrame())
            SecondaryUse = true;
        else if (actionController.PlayerControls.SecondaryUse.WasReleasedThisFrame())
            SecondaryUse = false;

        //Handle Reload
        if (actionController.PlayerControls.Reload.WasPressedThisFrame())
            Reloading = true;

    }
}