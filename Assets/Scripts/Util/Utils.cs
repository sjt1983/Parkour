using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    static IDictionary<string, string>  dic = new Dictionary<string, string>();
    static int inc = 0;

    public static void Spawn(Vector3 vector3)
    {
        GameObject gameObjectItem = (GameObject)Instantiate(Resources.Load("Prefabs/DebugSphere"), vector3, Quaternion.identity);
        gameObjectItem.name += inc++.ToString();
        dic.Add(gameObjectItem.name, "");
    }

    public static void ClearAll()
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
}
