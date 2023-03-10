
using UnityEngine;

public class Bullet : MonoBehaviour
{
    bool initialized = false;

    private float Velocity = 0f;

    private float age = 0f;

    Vector3 newPosition;

    float gravity = 0f;

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            return;

        gravity += -30f * Time.deltaTime;

        newPosition = transform.position + (transform.forward * Velocity * Time.deltaTime) + (transform.up * gravity * Time.deltaTime);
        Utils.SpawnDebugSphereAtPosition(gameObject.transform.position);
        if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitinfo))
        {
           // GameObject.Destroy(gameObject);
        }

        age += Time.deltaTime;
        if (age > 4)
        {
            //GameObject.Destroy(gameObject);
        }
        transform.position = newPosition;
    }

    public void Initialize(float velocity)
    {
        Velocity = velocity;        
        Utils.SpawnDebugSphereAtPosition(gameObject.transform.position);
        initialized = true;
    }
}
