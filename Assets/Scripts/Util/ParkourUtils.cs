using UnityEngine;

//Utility class for various functions I found neat
public class ParkourUtils : MonoBehaviour
{

    //Get the euler difference between two angles.
    public static float DifferenceInBetweenTwoAngles (float angle1, float angle2)
    {
        return 180 - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 180);
    }

    //Returns a new Vector pointing forward given a rotation and magnitude.
    public static Vector3 GenerateDirectionalForceVector(Quaternion rotation, float magnitude)
    {
        return rotation * Vector3.forward * magnitude;
    }
}
