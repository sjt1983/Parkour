using UnityEngine;
using UnityEngine.UI;
using TMPro;

//The initial menu which loads when you press the "Escape" key in game.
public class ActionMenuUI : BaseUI
{
    /*******************************/
    /*** Refernce to Unity Objects */
    /*******************************/

    //The pawn!
    [SerializeField]
    private Pawn pawn;

    [SerializeField]
    private Slider mouseSensitivitySlider;

    [SerializeField]
    private TextMeshProUGUI mouseSensitivityValue;

    [SerializeField]
    private Slider fovSlider;

    [SerializeField]
    private TextMeshProUGUI fovValue;

    private Camera mainCamera;

    /*** Unity Methods ***/

    private void Awake()
    {
        ShowMouseCursor = true;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        mouseSensitivitySlider.value = pawn.PawnLook.MouseSensitivity;
        mouseSensitivityValue.text = pawn.PawnLook.MouseSensitivity.ToString();

        fovSlider.value = mainCamera.fieldOfView;
        fovValue.text = mainCamera.fieldOfView.ToString();
    }

    /*** Class Methods ***/

    //Set the mouse sensitivity.
    public void OnPointerUpChangeMouseSensitivity()
    {
        pawn.PawnLook.MouseSensitivity = mouseSensitivitySlider.value;
        mouseSensitivityValue.text = pawn.PawnLook.MouseSensitivity.ToString();
    }

    //Set the mouse sensitivity.
    public void OnPointerUpChangeFOV()
    {
        mainCamera.fieldOfView = fovSlider.value;
        fovValue.text = mainCamera.fieldOfView.ToString();
    }

    //Quit the game - used by a button.
    public void OnQuit()
    {
        Application.Quit();
    }

}
