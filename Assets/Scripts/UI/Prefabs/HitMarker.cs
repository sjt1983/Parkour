using TMPro;
using UnityEngine;

public class HitMarker : MonoBehaviour
{
    [SerializeField]
    TextMeshPro text;

    private bool Initialized = false;

    private float age = 0;

    float defaultDistance = 25f;
    float defaultSize = 50f;

    Vector3 defaultPosition;
    Vector3 targetPosition;

    Vector3 defaultScale;
    Vector3 targetScale;

    float lerpTime;

    float moveTime = .7f;
    float shrinkTime = .15f;

    // Update is called once per frame
    void Update()
    {
        if (!Initialized)
            return;

        age += Time.deltaTime;

        lerpTime = age / moveTime;
        lerpTime = EasingUtils.Exponential.Out(lerpTime);
        transform.position = Vector3.Lerp(defaultPosition, targetPosition, lerpTime);

        if (age > moveTime)
        {
            lerpTime = (age - moveTime) / shrinkTime;
            lerpTime = EasingUtils.Exponential.Out(lerpTime);
            transform.localScale = Vector3.Lerp(defaultScale, targetScale, lerpTime);
        }
        else
        {
            LookAtCamera();
        }
        
        if (age >= moveTime + shrinkTime)
            GameObject.Destroy(gameObject);
    }

    public void Initialize(float amount)
    {
        text.text = amount.ToString();
        LookAtCamera();
        defaultPosition = transform.position;
        targetPosition = defaultPosition + new Vector3(-2.5f, .5f, 0);

        defaultScale = transform.localScale;
        targetScale = new Vector3(0, 0);
        Initialized = true;
    }

    private void LookAtCamera()
    {   
        transform.LookAt(transform.position + GameObject.Find("Main Camera").transform.rotation * Vector3.forward,
        GameObject.Find("Main Camera").transform.rotation * Vector3.up);
        float dist = Vector3.Distance(GameObject.Find("ArmsCamera").transform.position, transform.position);
        text.fontSize = Mathf.RoundToInt(defaultSize * Mathf.Sqrt(dist / defaultDistance));
        transform.parent = GameObject.Find("WorldSpaceCanvas").transform;
    }
}
