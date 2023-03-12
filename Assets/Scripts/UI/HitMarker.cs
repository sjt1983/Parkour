using UnityEngine;
using TMPro;

public class HitMarker : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    private bool Initialized = false;

    private float age = 0;

    private Vector2 defaultPosition;
    private Vector2 targetPosition;

    private Vector2 defaultScale;
    private Vector2 targetScale;

    private float lerpTime;

    private const float moveTime = .7f;
    private const float shrinkTime = .15f;

    // Update is called once per frame
    void Update()
    {

        if (!Initialized)
            return;

        age += Time.deltaTime;

        lerpTime = age / moveTime;
        lerpTime = EasingUtils.Exponential.Out(lerpTime);
        transform.position = Vector2.Lerp(defaultPosition, targetPosition, lerpTime);

        if (age > moveTime)
        {
            lerpTime = (age - moveTime) / shrinkTime;
            lerpTime = EasingUtils.Exponential.Out(lerpTime);
            transform.localScale = Vector2.Lerp(defaultScale, targetScale, lerpTime);
        }

        if (age >= moveTime + shrinkTime)
            GameObject.Destroy(gameObject);

       
    }

    public void Initialize(float amount)
    {
        text.text = amount.ToString();
        defaultPosition = transform.position; ;
        targetPosition = defaultPosition + new Vector2(5f, 125f);

        defaultScale = transform.localScale;
        targetScale = new Vector2(0, 0);
        Initialized = true;
    }
}
