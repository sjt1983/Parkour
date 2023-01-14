using UnityEngine;
using System.Collections.Generic;

//Base class for a Sensor.
//Used to determine if it users can do fun movement stuff.
public class Sensor : MonoBehaviour
{
    /*** Class Properties ***/
    //Number of objects the sensor is currently colliding with.
    [SerializeField ]
    public int CollidedObjects { get; set; }

    public bool IsCollidingWith(string gameObjectName)
    {
        return colliedObjectNames != null && colliedObjectNames.Keys.Count > 0 &&colliedObjectNames.ContainsKey(gameObjectName);
    }

    private IDictionary<string, bool> colliedObjectNames = new Dictionary<string, bool>();

    /*** Unity Methods ***/

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
        colliedObjectNames.Add(other.gameObject.name, true);
    }

    //When this objects collides with another, decrement count by 1;
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MapGeometry"))
            return;

        CollidedObjects--;
        colliedObjectNames.Remove(other.gameObject.name);
    }
}
