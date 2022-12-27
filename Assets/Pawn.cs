using UnityEngine;

//Pawn is a class that represents the avatar which is controlled by the user.
public sealed class Pawn : MonoBehaviour
{
    /** Local References **/
    [SerializeField]
    public PawnInput pawnInput;

    public bool Initialized = false;

    private void Initialize()
    {   //Name this one "SELF" so we can ignore it for raycasts for bullets
        gameObject.name = "SELF";
        Initialized = true;
    }

    public void Update()
    {
        if (!Initialized)
            Initialize();
    }   
}