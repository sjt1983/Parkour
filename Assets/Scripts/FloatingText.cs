using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    Transform mainCamera;
    Transform unit;
    Transform worldSpaceCanvas;

    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.Find("ArmsCamera").transform;
        unit = transform.parent;
        worldSpaceCanvas = GameObject.FindObjectOfType<Canvas>().transform;
        transform.SetParent(worldSpaceCanvas);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        transform.position = unit.position + offset;
    }
}
