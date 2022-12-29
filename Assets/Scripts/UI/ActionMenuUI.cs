using UnityEngine;
using UnityEngine.UI;

//The initial menu which loads when you press the "Escape" key in game.
public class ActionMenuUI : BaseUI
{
    /*** Refernce to Unity Objects */ 
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Slider mouseSensitivity;

    /*** Unity Methods ***/

    private void Awake()
    {
        ShowMouseCursor = true;
        mouseSensitivity.value = pawn.PawnLook.MouseSensitivity;
    }

    /*** Class Methods ***/

    //Set the mouse sensitivity.
    public void OnPointerUpChangeMouseSensitivity()
    {
        pawn.PawnLook.MouseSensitivity = mouseSensitivity.value;
    }

    //Quit the game - used by a button.
    public void OnQuit()
    {
        Application.Quit();
    }

}
