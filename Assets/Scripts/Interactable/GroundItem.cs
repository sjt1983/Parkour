using UnityEngine;

//Class used to represent an item laying on the ground.
//It is interactible
public class GroundItem : Interactable
{
    //The name of the item, probably temporary.
    [SerializeField]
    public string itemName;

    //The GUID of the item
    private string id;

    //THe Item Payload (Data)
    private Item item;

    //Initialize the item.
    public void Awake()
    {
        Item item = new Item();
        item.ItemName = itemName;
        id = System.Guid.NewGuid().ToString();
        gameObject.name = id + "-" + item.ItemName;
        this.item = item;
    }

    //What to do when we interact.
    //Clone the payload to pickup the inventory since the item on the GameObject will die immediately after.
    public override void Interact(Pawn pawn)
    {
        pawn.PickupItem(item.Clone());
        GameObject.Destroy(gameObject);
    }
}
