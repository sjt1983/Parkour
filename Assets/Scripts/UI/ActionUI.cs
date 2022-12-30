using UnityEngine;
using UnityEngine.UI;
using TMPro;
//Stuff the player sees while they are playing the game.
public class ActionUI : BaseUI
{
    [SerializeField]
    private TextMeshProUGUI debug1;

    [SerializeField]
    private TextMeshProUGUI debug2;

    [SerializeField]
    private TextMeshProUGUI debug3;

    /*** Unity Methods ***/
    private void Awake()
    {
        ShowMouseCursor = false;
    }

    public string DebugText1
    {
        set
        {
            debug1.text = value;
        }
    }

    public string DebugText2
    {
        set
        {
            debug2.text = value;
        }
    }

    public string DebugText3
    {
        set
        {
            debug3.text = value;
        }
    }
}
