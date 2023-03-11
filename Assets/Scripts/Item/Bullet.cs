using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    bool initialized = false;

    private float Velocity = 0f;

    private float age = 0f;

    Vector3 newPosition;

    float gravity = 0f;

    List<GameObject> debugSpheres = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            return;

        gravity += -30f * Time.deltaTime;

        newPosition = transform.position + (transform.forward * Velocity * Time.deltaTime) + (transform.up * gravity * Time.deltaTime);

        if (Globals.Debug)
            debugSpheres.Add(DebugUtils.SpawnDebugSphereAtPosition(gameObject.transform.position, null));


        if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitinfo))
        {
            Damageable damageable = hitinfo.transform.gameObject.GetComponent<Damageable>();

            if (damageable != null)
            {
                damageable.ApplyDamage(50);
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

    public void Initialize(float velocity)
    {
        Velocity = velocity;
        initialized = true;

        if (Globals.Debug)
            debugSpheres.Add(DebugUtils.SpawnDebugSphereAtPosition(gameObject.transform.position, null));
    }
}
