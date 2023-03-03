
using UnityEngine;

public class Pistol : EquippableItem
{
    private Vector3 defaultPosition = new Vector3(-0.00045f, 0, 0.00054f);
    private Quaternion defaultRotation = Quaternion.Euler(39.232f, 140.551f, -4.256f);

    public override void EquipItem()
    {
        base.EquipItem();
        gameObject.transform.localPosition = defaultPosition;
        gameObject.transform.localRotation = defaultRotation;
    }
}
