using UnityEngine;

//Base class for a Sensor.
//Used to determine if it pawns can do fun movement stuff.
public class Sensor : MonoBehaviour
{
    /************************/
    /*** Class Properties ***/
    /************************/

    //Number of objects the sensor is currently colliding with.
    public int CollidedObjects { get; set; }

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    private void Awake()
    {
        CollidedObjects = 0;
    }

    //When this objects collides with another, increment count by 1;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MapGeometry"))
            return;

        CollidedObjects++;
    }

    //When this objects collides with another, decrement count by 1;
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MapGeometry"))
            return;

        CollidedObjects--;
    }
}
