using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    /*** References to Unity Objects ***/

    //We need the ActionUI for Debug Text
    [SerializeField]
    ActionUI actionUI;

    //Any UI View needs to be added to this manager.
    [SerializeField]
    private BaseUI[] views;

    /*** Class Properties ***/

    //Singleton instance for quick access by all domains.
    public static UIManager Instance { get; private set; }

    /*** Unity Methods ***/

    private void Awake()
    {
        Instance = this;
        Initialize();

        //Lock the cursor to the window and hide the cursor, when we show Views, we can show the cursor if the view tells us to.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /*** Class Methods ***/

    public void Initialize()
    {
        foreach (BaseUI view in views)  
            view.Initialize();   
    }

    //Shows a view
    public void Show<T>() where T : BaseUI
    {
        //Loop thru the collection of views and see if they match 'T' and show it, then check to see we should show the cursor.
        foreach (BaseUI view in views)
        {
            view.gameObject.SetActive(view is T);
            if (view is T)
                Cursor.visible = view.ShowMouseCursor;
        }
    }

    //Simple getter to see if the cursor is enabled for utility.
    //e.g. you're in a menu and you don't want the camera to move, return from update if IsCursorVisible.
    public bool IsCursorVisible { get => Cursor.visible; }

    //Debug Texts
    public string DebugText1 { set => actionUI.DebugText1 = value; }

    public string DebugText2 { set => actionUI.DebugText2 = value; }

    public string DebugText3 { set => actionUI.DebugText3 = value; }

    public string DebugText4 { set => actionUI.DebugText4 = value; }

    public string DebugText5 { set => actionUI.DebugText5 = value; }

    public string DebugText6 { set => actionUI.DebugText6 = value; }
}