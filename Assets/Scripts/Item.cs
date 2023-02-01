using UnityEngine;

public class Item : MonoBehaviour
{
    private bool initialized = false;

    private string id;

    [SerializeField]
    public string itemName;

    public void Initialize()
    {
        id = System.Guid.NewGuid().ToString();
        initialized = true;
    }
}
