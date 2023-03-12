
//Response from a projectile hitting something.
public class HitResponse 
{
    //Id of the thing that was hit.
    public int TargetId { get; set; }

    //Total damage done;
    public float Damage { get; set; }

    //Amount of damage that was Armor damage;
    public float ArmorDamage { get; set; }

    //Amount of damage that was Health damage;
    public float HealthDamage { get; set; }

    //Flag to indicate armor was depleted from the hit.
    public bool BrokeArmor { get; set; }

    //Flag to indicate the target was killed with this hit.
    public bool KilledTarget { get; set; }

    public HitResponse()
    {
        Damage = 0;
        ArmorDamage = 0;
        HealthDamage = 0;
        BrokeArmor = false;
        KilledTarget = false;
    }
}
