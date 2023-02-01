using UnityEngine;


//Class representing the pawns inventory.
public class PawnInventory : MonoBehaviour
{
    public Item ItemSlot1 { get; set; }

    private void Update()
    {
        //this is our inventory UI.....for now!
        UIManager.Instance.DebugText3 = ItemSlot1 != null ? ItemSlot1.ItemName : "";
    }
}
