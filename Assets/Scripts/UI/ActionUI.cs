using UnityEngine;
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

    //Set Debug Text 1
    public string DebugText1 { set => debug1.text = value; }

    //Set Debug Text 2
    public string DebugText2 { set => debug2.text = value; }

    //Set Debug Text 3
    public string DebugText3 { set => debug3.text = value; }
}
