using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private BaseUI[] views;

    private void Awake()
    {
        Instance = this;
        Initialize();

        //Lock the cursor to the window
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

    public void Show<T>() where T : BaseUI
    {
        foreach (BaseUI view in views)
        {
            view.gameObject.SetActive(view is T);
            if (view is T)
            {
                Cursor.visible = view.ShowMouseCursor;
            }
        }
    }

    public bool IsCursorVisible() => Cursor.visible;
}