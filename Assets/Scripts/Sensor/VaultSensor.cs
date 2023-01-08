using UnityEngine;

//Used to tell us if we can vault
public class VaultSensor : Sensor
{
    /*****************************/
    /*** Local Class Variables ***/
    /*****************************/

    //The layer mask we can hit when looking to vault.
     private LayerMask layerMask;

    //How far down from the sensor we should raycast.
    private readonly float SENSOR_RANGE = 1f;

    /*********************/
    /*** Unity Methods ***/
    /*********************/

    private void Awake()
    {
        layerMask = LayerMask.GetMask("MapGeometry");
    }

    /*********************/
    /*** Class Methods ***/
    /*********************/

    //Determine what point the sensor hits.
    //Used the Bool with return object pattern.
    public bool FindVaultPoint(ref Vector3 potentialVaultPoint, Vector3 vaultLowPointPosition)
    {
        RaycastHit hitInfo;
        //Raycast down from the sensor, only looking at map geometry.
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hitInfo, SENSOR_RANGE, layerMask))
        {
            potentialVaultPoint = hitInfo.point;

            //If the point we are trying to vault at is lower than our shoulders, we dislocate both shoulders.
            //JK we just dont allow the vault.

            if (potentialVaultPoint.y > vaultLowPointPosition.y)
                return true;
        }

        return false;
    }    

}
