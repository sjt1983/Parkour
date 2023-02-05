using UnityEngine;


//Class representing the pawns inventory.
public class PawnInventory : MonoBehaviour
{
    //The input for the pawn
    [SerializeField]
    private PawnInput pawnInput;

    //The transform to assign items the player picks up to
    [SerializeField]
    private Transform inventoryGameObject;

    //All the players equipment slots.
    public EquippableItem[] ItemSlots = new EquippableItem[4];

    //Current active inventory slow.
    public int activeSlot = -1;

    private void Update()
    {
        if (pawnInput.EquipSlot1PressedThisFrame)
            EquipItem(0);
        else if (pawnInput.EquipSlot2PressedThisFrame)
            EquipItem(1);
        else if (pawnInput.EquipSlot3PressedThisFrame)
            EquipItem(2);
        else if (pawnInput.EquipSlot4PressedThisFrame)
            EquipItem(3);
        else if (pawnInput.UnequipPressedThisFrame)
            UnequipItem();

        //this is our inventory UI.....for now!
        UIManager.Instance.DebugText3 = ItemSlots[0] != null ? ItemSlots[0].ItemName : "";
    }

    //Pickup an item.
    public void PickupItem(EquippableItem equippableItem)
    {
        //Set the transform.
        equippableItem.transform.parent = inventoryGameObject;

        //Find an empty slot for the item.
        for (int x = 0; x < 4; x++)
        {
            if (ItemSlots[x] == null)
            {
                ItemSlots[x] = equippableItem;
                return;
            }
        }

        Debug.Log("ADD OVERFLOW LOGIC");
    }

    //Equip an item in the active slot.
    public void EquipItem(int slot)
    {
        //If this is alredy the active slot, do nothing.
        if (activeSlot == slot)
            return; 

        //If we do have an active slot, AND it has an item, unequip the item.
        if (activeSlot != -1 && ItemSlots[activeSlot] != null)
            ItemSlots[activeSlot].UnequipItem();

        //Set the active slot.
        activeSlot = slot;

        //Equip the item in the slot, if there is one.
        if (ItemSlots[slot] != null)
            ItemSlots[slot].EquipItem();
    }

    //Unequip any active Item
    public void UnequipItem()
    {
        //aka if there is an active slot with an item.
        if (activeSlot > -1 && ItemSlots[activeSlot] != null)
            ItemSlots[activeSlot].UnequipItem();

        //Set the active slot to -1;
        activeSlot = -1;
    }

    //Drop an item.
    //NOT really implemented yet.
    public void DropItem(EquippableItem equippableItem)
    {
        equippableItem.UnequipItem();
        equippableItem.DropItem();
        equippableItem.transform.parent = null;
    }
}
