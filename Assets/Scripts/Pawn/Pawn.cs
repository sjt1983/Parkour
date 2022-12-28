using UnityEngine;

//Pawn is a class that represents the avatar which is controlled by the user.
public sealed class Pawn : MonoBehaviour
{
    /** Local References **/
    [SerializeField]
    public PawnInput pawnInput;

    //Flag to prevent any logic from executing until after Initialization.
    public bool Initialized = false;

    private void Initialize()
    {   
        Initialized = true;
    }

    public void Update()
    {
        if (!Initialized)
            Initialize();
    }   
}