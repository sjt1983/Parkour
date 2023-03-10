using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    static IDictionary<string, string>  dic = new Dictionary<string, string>();
    static int inc = 0;

    public static GameObject SpawnDebugSphereAtPosition(Vector3 vector3, Transform owner)
    {
        GameObject gameObjectItem = (GameObject)Instantiate(Resources.Load("Prefabs/DebugSphere"), vector3, Quaternion.identity);
        gameObjectItem.name += inc++.ToString();
        gameObjectItem.transform.parent = owner;
        dic.Add(gameObjectItem.name, "");
        return gameObjectItem;
    }

    public static void ClearAllDebugSpheres()
    {
        foreach (KeyValuePair<string, string> entry in dic)
        {
            GameObject.Destroy(GameObject.Find(entry.Key));
        }
        dic = new Dictionary<string, string>();
        inc = 0;
    }

    public static float DifferenceInBetweenTwoAngles (float angle1, float angle2)
    {
        return 180 - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 180);
    }

    public static Vector3 GenerateDirectionalForceVector(Quaternion rotation, float magnitude)
    {
        return rotation * Vector3.forward * magnitude;
    }
}
