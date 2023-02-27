using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : EquippableItem
{
    private Vector3 defaultPosition = new Vector3(-0.00045f, 0, 0.00027f);
    private Quaternion defaultRotation = new Quaternion(39.232f, 140.551f, -4.256f, 0);

    public override void EquipItem()
    {
        base.EquipItem();
        gameObject.transform.localPosition = defaultPosition;
        gameObject.transform.localRotation = defaultRotation;
    }
}
