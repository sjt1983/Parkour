using UnityEngine;

//Just a class used temporaily to give me something to shoot at!
public class Drone : Damageable
{ 

    public float Health = 3;
    public float Timer = 0f;
    public float Y;

    public float deadY = 0;
    public float deadTimer = 0;

    private void Start()
    {
        Timer = Random.Range(-1f, 1f);
        Y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        float x = Mathf.Sin(Timer);
        UIManager.Instance.DebugText2 = x.ToString();
        transform.position = new Vector3(transform.position.x, Y + deadY + x * (2 + 3 - Health), transform.position.z);

        transform.localScale = new Vector3(Health / 3, Health / 3, Health / 3);

        if (deadY < -999)
        {
            deadTimer += Time.deltaTime;
            if (deadTimer >= 30)
            {
                Health = 3;
                deadY = 0;
                deadTimer = 0;
            }
        }
    }

    public override void ApplyDamage(float amount)
    {
        Health--;

        if (Health <= 0)
        {
            deadY = -1000;
        }
    }

}
