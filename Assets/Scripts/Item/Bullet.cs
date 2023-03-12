using System.Collections.Generic;
using UnityEngine;

//Class used to represent a simple bullet.
public class Bullet : MonoBehaviour
{
    //Pawn that owns the bullet fired.
    private EquippableItem ownerItem;

    //velocity of the bullet.
    private float velocity = 0f;

    //age of the bullet
    private float age = 0f;

    //total gravity force applied to the bullet.
    private float gravity = 0f;

    //Collection of debug spheres to destroy when the bullet is destroyed.
    List<GameObject> debugSpheres = new();

    //Position we move the bullet to per frame
    private Vector3 newPosition;

    //How much gravity should be applied to the bullet per second.
    private const float BULLET_GRAVITY = -30f;

    //Initialization flag
    private bool initialized = false;

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            return;

        //How much total gravity is applied to the bullet.
        gravity += BULLET_GRAVITY * Time.deltaTime;

        //Generate the new position.
        newPosition = transform.position + (transform.forward * velocity * Time.deltaTime) + (transform.up * gravity * Time.deltaTime);

        //If debug mode, add a sphere.
        if (Globals.Debug)
            debugSpheres.Add(DebugUtils.SpawnDebugSphereAtPosition(gameObject.transform.position, null));

        //See if the bullet should hit something this frame with a linecast due to fast enough velocities missing colliders.

        if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitinfo))
        {
            Damageable damageable = hitinfo.transform.gameObject.GetComponent<Damageable>();

            if (damageable != null)
            {
                //We hit something significant, so lets apply the damage and register the hit.
                HitResponse hitResponse = damageable.ApplyDamage(7);
                ownerItem.RegisterHit(hitResponse);
            }

            GameObject.Destroy(gameObject);
        }

        age += Time.deltaTime;

        transform.position = newPosition;

        if (age > 4)
        {
            foreach (GameObject obj in debugSpheres)
            {
                GameObject.Destroy(obj);
            }
            GameObject.Destroy(gameObject);
        }
    }

    public void Initialize(EquippableItem owner, float velocity)
    {
        this.velocity = velocity;
        ownerItem = owner;
        initialized = true;

        if (Globals.Debug)
            debugSpheres.Add(DebugUtils.SpawnDebugSphereAtPosition(gameObject.transform.position, null));
    }
}
