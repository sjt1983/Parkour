using UnityEngine;
using TMPro;
//Stuff the player sees while they are playing the game.
public class ActionUI : BaseUI
{
    [SerializeField]
    Transform hitMarkerTarget;

    [SerializeField]
    private TextMeshProUGUI debug1;

    [SerializeField]
    private TextMeshProUGUI debug2;

    [SerializeField]
    private TextMeshProUGUI debug3;

    [SerializeField]
    private TextMeshProUGUI debug4;

    [SerializeField]
    private TextMeshProUGUI debug5;

    [SerializeField]
    private TextMeshProUGUI debug6;

    /****************************/
    /*** Hit Marker Variables ***/
    /****************************/

    //Last ID of the target hit.
    private int lastTargetHit = -1;

    //Damage accumulations
    private float accumulatedArmorDamage = 0f;
    private float accumulatedHealthDamage = 0f;

    //Time and timer used to reset the marker if it has been a while since the pawn shot someone.
    private float markerTimer = 0f;
    private const float MARKER_TIMEOUT = 1f;

    //Hit Markers GameObject and Component references.
    GameObject hitMarkerGameObject1, hitMarkerGameObject2;
    HitMarker hitMarker1, hitMarker2;

    /*********************/
    /*** Unity Methods ***/
    /*********************/


    private void Awake()
    {
        ShowMouseCursor = false;

        //Setup the hit markers.
        hitMarkerGameObject1 = (GameObject)Instantiate(Resources.Load("Prefabs/HitMarker"), hitMarkerTarget.transform);
        hitMarker1 = hitMarkerGameObject1.GetComponent<HitMarker>();
        hitMarkerGameObject2 = (GameObject)Instantiate(Resources.Load("Prefabs/HitMarker"), hitMarkerTarget.transform);
        hitMarker2 = hitMarkerGameObject2.GetComponent<HitMarker>();

        hitMarker1.Initialize(hitMarkerTarget.position);
        hitMarker2.Initialize(hitMarkerTarget.position);
    }

    private void Update()
    {
        markerTimer += Time.deltaTime;
    }

    //Set Debug Text 1
    public string DebugText1 { set => debug1.text = value; }

    //Set Debug Text 2
    public string DebugText2 { set => debug2.text = value; }

    //Set Debug Text 3
    public string DebugText3 { set => debug3.text = value; }

    //Set Debug Text 4
    public string DebugText4 { set => debug4.text = value; }

    //Set Debug Text 5
    public string DebugText5 { set => debug5.text = value; }

    //Set Debug Text 6
    public string DebugText6 { set => debug6.text = value; }

    public void RegisterHit(HitResponse hitResponse)
    {
        //If it has been one second after hitting someone, oro we hit a different target, reset the accumulated damage.
        if (markerTimer > MARKER_TIMEOUT || lastTargetHit != hitResponse.TargetId)
        {
            lastTargetHit = hitResponse.TargetId;
            accumulatedArmorDamage = hitResponse.ArmorDamage;
            accumulatedHealthDamage = hitResponse.HealthDamage;           
        }
        //Otherwise accumulate damage.
        else if (lastTargetHit == hitResponse.TargetId)
        {
            accumulatedArmorDamage += hitResponse.ArmorDamage;
            accumulatedHealthDamage += hitResponse.HealthDamage;            
        }

        //We hit someone to reset the timer;
        markerTimer = 0f;

        //If we did both Armor and Health damage, split the markers in two, otherwise just display the color for the type we did.
        if (hitResponse.ArmorDamage > 0 && hitResponse.HealthDamage > 0)
        {
            hitMarker1.Display(accumulatedHealthDamage, Color.red);
            hitMarker2.Display(accumulatedArmorDamage, Color.blue, true);
        }
        else if (hitResponse.ArmorDamage > 0)
            hitMarker1.Display(accumulatedArmorDamage, Color.blue);
        else
            hitMarker1.Display(accumulatedHealthDamage, Color.red);

        
    }
}
