using System.Collections.Generic;
using UnityEngine;

//Utilities used for debugging.
public class DebugUtils : MonoBehaviour
{
    static IDictionary<string, string> dic = new Dictionary<string, string>();
    static int inc = 0;

    //Spawn a prefab DebugSphere with an optional owner.
    public static GameObject SpawnDebugSphereAtPosition(Vector3 vector3, Transform owner)
    {
        GameObject gameObjectItem = (GameObject)Instantiate(Resources.Load("Prefabs/DebugSphere"), vector3, Quaternion.identity);
        gameObjectItem.name += inc++.ToString();
        gameObjectItem.transform.parent = owner;
        dic.Add(gameObjectItem.name, "");
        return gameObjectItem;
    }

    //Delete all debug spheres.
    public static void ClearAllDebugSpheres()
    {
        foreach (KeyValuePair<string, string> entry in dic)
        {
            GameObject.Destroy(GameObject.Find(entry.Key));
        }
        dic = new Dictionary<string, string>();
        inc = 0;
    }
}
