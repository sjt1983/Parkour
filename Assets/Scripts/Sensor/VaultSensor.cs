using UnityEngine;

//Used to tell us if we can vault
public class VaultSensor : Sensor
{
    /*** Local Class Variables ***/
    int layerMask;
    float sensorRange = 0f;

    /*** Unity Methods ***/
    private void Awake()
    {
        layerMask = LayerMask.GetMask("MapGeometry");
    }

    /*** Class Methods ***/

    //Determine what point the sensor hits
    public bool FindVaultPoint(ref Vector3 vector3, float lookAngle)
    {
        //The look angle is somewhere between -85 and 85 degrees.
        //
        if (lookAngle < 0)
        {
            sensorRange = 1.5f;
        }
        else
        {
            sensorRange = .5f * ((85f - lookAngle) / 85f);
        }

        if (Physics.Raycast(gameObject.transform.position, Vector3.down, sensorRange, layerMask))
        {
            vector3 = gameObject.transform.position;
            return true;
        }

        return false;
    }    

}
