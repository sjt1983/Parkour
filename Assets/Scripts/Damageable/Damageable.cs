using UnityEngine;

//Class used to deleniate objects which can receive damage.
public abstract class Damageable : MonoBehaviour
{
    //The ID of the Damageble target, used for accumulation
    public int DamageableId { get; private set; }

    //ID Seed Start
    private static int GeneratedId = 1;

    protected virtual void Start()
    {
        DamageableId = GeneratedId++;
        Debug.Log("Generated ID " + DamageableId);
    }

    //Apply damage to the entity
    public abstract HitResponse ApplyDamage(float amount);
}
