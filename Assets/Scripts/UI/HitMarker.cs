using UnityEngine;
using TMPro;

//Class used to display hit markers.
public class HitMarker : MonoBehaviour
{
    //Text field from the prefab.
    [SerializeField]
    TextMeshProUGUI text;

    //Initialization flag, used to know the class was setup once.
    private bool Initialized = false;

    //Active, meaning it is currently in use (in the event of them being reused);
    private bool Active = false;

    //How long the hit marker has been alive, used for lerping.
    private float age = 0;

    //Default position for the marker and the target position it is moving to while alive.
    private Vector2 defaultPosition;
    private Vector2 targetPosition;

    //Default scale and target scale for shrinking the hit marker out of the way.
    private Vector2 defaultScale;
    private Vector2 targetScale;

    //Lerp time percentage
    private float lerpTime;

    //How much time it takes the marker to move towards its target.
    private const float moveTime = .7f;

    //how much time the marker spends shrinking.
    private const float shrinkTime = .15f;
       
    void Update()
    {
        //Do nothing if there is no reason to do anything.
        if (!Initialized && !Active)
            return;

        age += Time.deltaTime;

        if (age <= moveTime)
        {
            //Move from default to target position first.
            lerpTime = age / moveTime;
            lerpTime = EasingUtils.Exponential.Out(lerpTime);
            transform.position = Vector2.Lerp(defaultPosition, targetPosition, lerpTime);
        }
        //Then, shrink out of the way.
        else if (age > moveTime)
        {
            lerpTime = (age - moveTime) / shrinkTime;
            lerpTime = EasingUtils.Exponential.Out(lerpTime);
            transform.localScale = Vector2.Lerp(defaultScale, targetScale, lerpTime);
        }

        //Deactivate the script once its done its thing!
        if (age >= moveTime + shrinkTime)
            Active = false;

    }

    public void Initialize(Vector3 defaultPosition)
    {
        //Set some defaults that never change for the lifetime of the HitMarker!
        this.defaultPosition = defaultPosition;
        defaultScale = new Vector2(1, 1);
        targetScale = new Vector2(0, 0);
        Initialized = true;
    }

    //Shorthand for displaying a primary hit marker.
    public void Display(float amount, Color color) {
        Display(amount, color, false);
    }

    //Used to Display a hit marker!
    public void Display(float amount, Color color, bool secondaryMarker)
    {
        //Reset local variables for the script.
        age = 0;
        text.text = amount.ToString();
        text.color = color;
        transform.position = defaultPosition;
        transform.localScale = defaultScale;

        //Depending on the type of marker it needs to go to different places in case we see two of them.
        if (!secondaryMarker)
            targetPosition = defaultPosition + new Vector2(50f, 15f);
        else
            targetPosition = defaultPosition + new Vector2(50f, 75f);

        //Finally, ACTIVATE IT!
        Active = true;
    }
}
