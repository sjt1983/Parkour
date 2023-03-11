using UnityEngine;

//Class to represent things on the map which the Pawn can interact with.
public abstract class Interactable : MonoBehaviour
{
    //Interact with the item
    public abstract void Interact(Pawn pawn);
    
}
