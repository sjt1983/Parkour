using UnityEngine;

//Class used to deleniate objects which can receive damage.
public abstract class Damageable : MonoBehaviour
{    
    //Apply damage to the entity
    public abstract void ApplyDamage(float amount);
}
