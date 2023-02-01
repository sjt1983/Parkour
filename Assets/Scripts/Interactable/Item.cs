//Represents the payload of an item,
public class Item 
{
    //The name
    public string ItemName { get; set; }

    //Returns a clone of the existing item.
    public Item Clone()
    {
        Item newItem = new()
        {
            ItemName = ItemName
        }; //teehee
        return newItem;
    }
}
