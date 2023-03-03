using System.Collections;
using UnityEngine;

//Class representing the pawns inventory.
public class PawnInventory : MonoBehaviour
{
    [SerializeField]
    private PawnArmsAnimator pawnArmsAnimator;

    //The input for the pawn
    [SerializeField]
    private PawnInput pawnInput;

    //The transform to assign items the player picks up to
    [SerializeField]
    private Transform inventoryGameObject;

    //The transform to assign items the player picks up to
    [SerializeField]
    private Transform itemBone;

    //All the players equipment slots.
    public EquippableItem[] ItemSlots = new EquippableItem[4];

    //Current active inventory slow.
    public int activeSlot = -1;

    //Flag to indicate a current "item switch" is happening.
    private bool equippingItem = false;

    private const float ITEM_SWITCH_DELAY = .25f;
        
    private void Update()
    {
        //For now, we only need to handle the equipment buttons being pressed.
        if (!equippingItem)
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
        }

    }

    //Pickup an item.
    public void PickupItem(EquippableItem equippableItem)
    {
        //Find an empty slot for the item.
        for (int x = 0; x < 4; x++)
        {
            if (ItemSlots[x] == null)
            {
                //Set the transform.
                equippableItem.transform.parent = inventoryGameObject;
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

        //Equip the item in the slot, if there is one.
        if (ItemSlots[slot] != null)
        {
            equippingItem = true;
            pawnArmsAnimator.HandleEquipItem(ItemSlots[slot]);

            //We need to take into account animations before doing the switch because switching triggers showing/hiding of meshes.
            StartCoroutine(DoEquipItem(slot, ITEM_SWITCH_DELAY));
        }            
    }

    //Coroutine for doing the item switch
    private IEnumerator DoEquipItem(int slot, float time)
    {
        yield return new WaitForSeconds(time);

        //If we do have an active slot, AND it has an item, unequip the item.
        if (activeSlot != -1 && ItemSlots[activeSlot] != null)
        {
            ItemSlots[activeSlot].UnequipItem();
        }
        ItemSlots[slot].transform.parent = itemBone;
        ItemSlots[slot].EquipItem();

        activeSlot = slot;
        equippingItem = false;
    }

    //Unequip any active Item
    public void UnequipItem()
    {
        equippingItem = true;
        pawnArmsAnimator.HandleEquipItem(null);

        //We need to take into account animations before doing the switch because switching triggers showing/hiding of meshes.
        StartCoroutine(DoUnequipItem(ITEM_SWITCH_DELAY));
    }

    //Coroutine for unequipping an item.
    private IEnumerator DoUnequipItem(float time)
    {
        yield return new WaitForSeconds(time);

        //If there is an active slot with an item.
        if (activeSlot > -1 && ItemSlots[activeSlot] != null)
        {
            ItemSlots[activeSlot].UnequipItem();
            ItemSlots[activeSlot].transform.parent = inventoryGameObject;
            activeSlot = -1;
            equippingItem = false;
        }
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
