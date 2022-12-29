using UnityEngine;

//Base class for all UI Views.
public abstract class BaseUI : MonoBehaviour
{
    protected bool initialized;

    //Set this in your views Initialization code.
    public bool ShowMouseCursor = false;

    //Put initialization code in this method of your class
    public virtual void Initialize()
    {
        initialized = true;
    }
}