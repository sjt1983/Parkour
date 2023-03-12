using UnityEngine;

//Just a class used temporaily to give me something to shoot at!
public class Drone : Damageable
{
    public float Armor = 10;
    public float Health = 20;
    public float Timer = 0f;
    public float Y;

    public float deadY = 0;
    public float deadTimer = 0;

    protected override void Start()
    {
        base.Start();
        Timer = Random.Range(-1f, 1f);
        Y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        float x = Mathf.Sin(Timer);
        transform.position = new Vector3(transform.position.x, Y + deadY + x , transform.position.z);
        //transform.localScale = new Vector3(Health / 20, Health / 20, Health / 20);

        if (deadY < -999)
        {
            deadTimer += Time.deltaTime;
            if (deadTimer >= 5)
            {
                Health = 20;
                Armor = 10;
                deadY = 0;
                deadTimer = 0;
            }
        }
    }

    public override HitResponse ApplyDamage(float amount)
    {

        HitResponse hitResponse = new();
        hitResponse.TargetId = DamageableId;
        hitResponse.Damage = amount;
        float damageToApply = hitResponse.Damage;
       
        if (Armor > 0)
        {
            if (Armor <= damageToApply)
            {
                hitResponse.BrokeArmor = true;
                damageToApply -= Armor;
                hitResponse.ArmorDamage = Armor;
                Armor = 0;
            }
            else
            {
                Armor -= damageToApply;
                hitResponse.ArmorDamage = damageToApply;
                damageToApply = 0;
            }            
        }

        if (Health > 0 && damageToApply > 0)
        {
            Health -= damageToApply;
           
            if (Health <= 0)
            {
                hitResponse.HealthDamage = damageToApply + Health;
                Health = 0;
                hitResponse.KilledTarget = true;
            }
            else
            {
                hitResponse.HealthDamage = damageToApply;
            }
        }
                
       
        if (Health <= 0)
        {
            deadY = -1000;
        }

        return hitResponse;
    }

}
