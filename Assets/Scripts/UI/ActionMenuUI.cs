using UnityEngine;
using UnityEngine.UI;

public class ActionMenuUI : BaseUI
{
    [SerializeField]
    private PawnLook pawnLook;

    [SerializeField]
    private Slider mouseSensitivity;

    private void Awake()
    {
        ShowMouseCursor = true;
        mouseSensitivity.value = pawnLook.MouseSensitivity;
    }

    public void OnPointerUpChangeMouseSensitivity()
    {
        pawnLook.MouseSensitivity = mouseSensitivity.value;
    }

    public void OnQuit()
    {
        Application.Quit();
    }

}
