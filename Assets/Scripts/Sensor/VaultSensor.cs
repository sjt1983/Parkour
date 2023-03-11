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
    private readonly float SENSOR_RANGE = 3f;

    //How far to the left and right the secondary sensors fire.
    private readonly float SECONDARY_SENSOR_SPREAD = .25f;

    //How far up from the hitpoint we cast the secondary rays, and how far down we cast.
    private readonly float SECONDARY_SENSOR_HEIGHT_BUFFER = 1.25f;

    //How far down the secondary raycast should travel
    private readonly float SECONDARY_SENSOR_RANGE = 2f;

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
    public bool FindVaultPoint(ref RaycastHit inHitInfo, Vector3 vaultLowPointPosition)
    {
        RaycastHit hitInfo;
        //Raycast down from the sensor, only looking at map geometry.
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hitInfo, SENSOR_RANGE, layerMask))
        { 
            //If the point we are trying to vault at is lower than our shoulders, we dislocate both shoulders.
            //JK we just dont allow the vault.

            if (hitInfo.point.y > vaultLowPointPosition.y && hitInfo.point.y <= vaultLowPointPosition.y + 1)
            {
                //This may be inefficient but for now we will cast two more rays, to the left and right of center.
                //Old algorithm for vaulting required there be a "Wall" in front of the player to climb which gave us an angle to ensure
                //we cannot vault a surface we are not reasonably facing.
                //I want the pawn to be able to vault on a platform, but not at an angle that is very close to parallel to the edge.
                //Luckily we can use the first raycast's hitInfo to make the raycast as short as possible.

                float adjustedY = transform.position.y - hitInfo.point.y - SECONDARY_SENSOR_HEIGHT_BUFFER;
                Vector3 secondaryLeftSensorPosition = transform.position + (-transform.right * SECONDARY_SENSOR_SPREAD) + (-transform.up * adjustedY);      
                Vector3 secondaryRightSensorPosition = transform.position + (transform.right * SECONDARY_SENSOR_SPREAD) + (-transform.up * adjustedY);

                if (Physics.Raycast(secondaryLeftSensorPosition, Vector3.down, SECONDARY_SENSOR_RANGE, layerMask) &&
                    Physics.Raycast(secondaryRightSensorPosition, Vector3.down, SECONDARY_SENSOR_RANGE, layerMask))
                {
                    inHitInfo = hitInfo;
                    return true;
                }
            }                
        }

        return false;
    }    

}
