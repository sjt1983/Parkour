using UnityEngine;
using UnityEngine.UI;

//The initial menu which loads when you press the "Escape" key in game.
public class ActionMenuUI : BaseUI
{
    /** Mouse Sensitivity Obnjects **/ 
    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private Slider mouseSensitivity;

    private void Awake()
    {
        ShowMouseCursor = true;
        mouseSensitivity.value = pawnLook.MouseSensitivity;
    }

    //Set the mouse sensitivity.
    public void OnPointerUpChangeMouseSensitivity()
    {
        pawnLook.MouseSensitivity = mouseSensitivity.value;
    }

    //Quit the game.
    public void OnQuit()
    {
        Application.Quit();
    }

}
