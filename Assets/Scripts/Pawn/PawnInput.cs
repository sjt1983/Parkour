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
    public float XDirection { get; set; }
    public float ZDirection { get; set; }

    /*** Actions ***/

    //Player is attempting to Interact with an object in the world, e.g. Pickup Item
    public bool InteractPressedThisFrame { get => actionController.PlayerControls.Interact.WasPressedThisFrame(); }
    public bool InteractPressed { get => actionController.PlayerControls.Interact.IsPressed(); }

    //Player is using their selected item, e.g. shoot gun.
    public bool PrimaryUsePressedThisFrame { get => actionController.PlayerControls.PrimaryUse.WasPressedThisFrame(); }
    public bool PrimaryUsePressed { get => actionController.PlayerControls.PrimaryUse.IsPressed(); }

    //Player is using their selected item with alternate action, e.g. ADS/Zoom
    public bool SecondaryUsePressedThisFrame { get => actionController.PlayerControls.SecondaryUse.WasPressedThisFrame(); }
    public bool SecondaryUsePressed { get => actionController.PlayerControls.SecondaryUse.IsPressed(); }

    //Reload
    public bool ReloadPressedThisFrame { get => actionController.PlayerControls.Reload.WasPressedThisFrame(); }
    public bool ReloadPressed { get => actionController.PlayerControls.Reload.IsPressed(); }

    //Equipment Slots / Unequip button.
    public bool EquipSlot1PressedThisFrame { get => actionController.PlayerControls.EquipSlot1.WasPressedThisFrame(); }
    public bool EquipSlot2PressedThisFrame { get => actionController.PlayerControls.EquipSlot2.WasPressedThisFrame(); }
    public bool EquipSlot3PressedThisFrame { get => actionController.PlayerControls.EquipSlot3.WasPressedThisFrame(); }
    public bool EquipSlot4PressedThisFrame { get => actionController.PlayerControls.EquipSlot4.WasPressedThisFrame(); }
    public bool UnequipPressedThisFrame { get => actionController.PlayerControls.Unequip.WasPressedThisFrame(); }

    //Crouching
    public bool CrouchPressedThisFrame { get => actionController.PlayerControls.Crouch.WasPressedThisFrame(); }
    public bool CrouchPressed { get => actionController.PlayerControls.Crouch.IsPressed(); }

    //Player hit the jump button.
    public bool JumpPressedThisFrame { get => actionController.PlayerControls.Jump.WasPressedThisFrame(); }

    //Player is holding the jump button.
    public bool JumpPressed { get => actionController.PlayerControls.Jump.IsPressed(); }   

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

        actionController.PlayerControls.EquipSlot1.Enable();
        actionController.PlayerControls.EquipSlot2.Enable();
        actionController.PlayerControls.EquipSlot3.Enable();
        actionController.PlayerControls.EquipSlot4.Enable();
        actionController.PlayerControls.Unequip.Enable();

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
        XDirection = finalMovementDirection.x;
        ZDirection = finalMovementDirection.y;
    }
}