using UnityEngine;

//Base class for all UI Views.
public abstract class BaseUI : MonoBehaviour
{
    public bool Initialized { get; private set; }

    //Set this in your views Initialization code.
    public bool ShowMouseCursor = false;

    public virtual void Initialize()
    {
        Initialized = true;
    }
}