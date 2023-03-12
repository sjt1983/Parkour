using UnityEngine;

//An item which can be equipped and used by the player.
//All objects must inherit from this/
//All objects should do very little to touch the player.
public abstract class EquippableItem: Interactable 
{
    //The main mesh renderer for the model to show/hide, should be easily found during Runtime.
    [SerializeField]
    protected Renderer[] mainMeshRenderer;

    [SerializeField]
    protected AnimatorOverrideController animatorOverrideController;

    //The pawn which currently owns the item.
    protected Pawn pawn;

    //The name of the item
    public string ItemName;

    //Quick property check to see if the item is assigned to a pawn or not.
    public bool IsAssignedToPawn => pawn != null;

    //Flag to indicate it is currently equipped, e.g. the active item slot.
    //SUPER IMPORTANT PROPERTY - scripts needs to determine if the item is equipped to know whether or not to execute.
    public bool Equipped { get; set; }

    //Flag for enabling the item, which is another condition which can be checked. Certain actions disable certain items (items programmed to support it)
    //example, you cannot use a pogostick while vaulting but you can shoot a gun.
    public bool ItemEnabled { get; set; }

    public void Awake()
    {
        Equipped = false;
        ItemEnabled = false;
    }

    //Assign to the selected pawn.
    public virtual void AssignToPawn(Pawn pawn)
    {

        this.pawn = pawn;
    }

    //When interacting with items on the ground, we pick them up.
    //Then, hide the item.
    public override void Interact(Pawn pawn)
    {
        pawn.PickupItem(this);

        HideMeshes(9);
    }

    //Equip the item, which sets Equipped to true, shows the mesh, eventually does neat animation stuff.
    public virtual void EquipItem()
    {
        Equipped = true;
        ShowMeshes(9);
    }

    //Unequip the item, which should stop the script and hide the mesh.
    public virtual void UnequipItem()
    {
        Equipped = false;
        HideMeshes(9);
    }

    //Returns this weapons animator override controller
    public AnimatorOverrideController AnimatorOverrideController => animatorOverrideController; 

    //Drop the item back on the ground.
    public virtual void DropItem()
    {
        pawn = null;
        Equipped = false;
        ShowMeshes(1);
    }

    //Hide the meshes, and also assign them to the passed-in layer.
    private void HideMeshes(int layerToAssignMeshes)
    {
        foreach (Renderer r in mainMeshRenderer)
        {
            r.enabled = false;
            r.gameObject.layer = layerToAssignMeshes;
        }
    }

    //Show the meshes, and also assign them to the passed-in layer.
    private void ShowMeshes(int layerToAssignMeshes)
    {
        foreach (Renderer r in mainMeshRenderer)
        {
            r.enabled = true;
            r.gameObject.layer = layerToAssignMeshes;
        }
    }

    public void RegisterHit(HitResponse hitResponse)
    {
        pawn.RegisterHit(hitResponse);
    }
}
