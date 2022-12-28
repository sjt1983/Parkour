using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    //Singleton instance for quick access by all domains.
    public static UIManager Instance { get; private set; }

    //Any UI View needs to be added to this manager.
    [SerializeField]
    private BaseUI[] views;

    private void Awake()
    {
        Instance = this;
        Initialize();

        //Lock the cursor to the window and hide the cursor, when we show Views, we can show the cursor if the view tells us to.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Initialize()
    {
        foreach (BaseUI view in views)
        {
            view.Initialize();
        }
    }

    //Shows a view
    public void Show<T>() where T : BaseUI
    {
        //Loop thru the collection of views and see if they match 'T' and show it, then check to see we should show the cursor.
        foreach (BaseUI view in views)
        {
            view.gameObject.SetActive(view is T);
            if (view is T)
            {
                Cursor.visible = view.ShowMouseCursor;
            }
        }
    }

    //Simple getter to see if the cursor is enabled for utility.
    //e.g. you're in a menu and you don't want the camera to move, return from update if IsCursorVisible.
    public bool IsCursorVisible() => Cursor.visible;
}