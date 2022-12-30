using UnityEngine;

//Used to tell us if we can vault
public class VaultSensor : Sensor
{
    /*** Local Class Variables ***/
    int layerMask;

    /*** Unity Methods ***/
    private void Awake()
    {
        layerMask = LayerMask.GetMask("MapGeometry");
    }

    /*** Class Methods ***/

    //Determine what point the sensor hits
    public bool FindVaultPoint(ref Vector3 vector3)
    {
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, 2f, layerMask))
        {
            vector3 = gameObject.transform.position;
            return true;
        }

        return false;
    }    

}
