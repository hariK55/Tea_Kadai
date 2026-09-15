using System;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Glass : MonoBehaviour
{
    public static Glass Instance { get; private set; }
    public event Action FlowMilk;
    private Animator glassAnimator;
    private Strainer strainer;
    private bool isHit;
    private bool wasHit;
    private Vector2 origin;
    private Vector2 direction;
    private float rayDistance=0.5f;
   [SerializeField] private LayerMask detectionLayer;

    private Sugar Sugar;
    private int sugarCount = 0;

    private void Awake()
    {
        Instance = this;
        glassAnimator = GetComponent<Animator>();
    }
    private void Start()
    {
        strainer = Strainer.Instance;
        strainer.Onpour += FillTication;
        strainer.OnStopPour += OnStopPour;
        Milk.Instance.milkAdded += OnmilkAdded;

       Sugar= Sugar.Instance;
        Sugar.OnSugarAdded += Sugar_OnSugarAdded; 
 
    }

    private void Sugar_OnSugarAdded()
    {
        if(sugarCount >= 3)
        {
            Debug.Log("Maximum sugar count reached.");
            return;
        }
        sugarCount++;
        glassAnimator.SetInteger("sugarCount", sugarCount);
    }

    private void OnmilkAdded()
    {
        glassAnimator.SetBool("isMilkAdded", true);
    }

    private void Update()
    {
        DetectionRaycast();
    }

    
    private void OnDisable()
    {
        strainer.Onpour -= FillTication;
        strainer.OnStopPour -= OnStopPour;
    }

    private void FillTication()
    {
        // Handle pour event
        glassAnimator.SetBool("onTicationfill", true);
    }

    private void OnStopPour()
    {
        // Handle stop pour event
    }
    float timer;
    float max=1f;
    private void DetectionRaycast()
    {
        origin = transform.position;
        direction = Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            rayDistance,
            detectionLayer
        );

        isHit = hit.collider != null;

        // Raycast detected object for the first time
        if (isHit && !wasHit)
        {
            Debug.Log("Object below: " + hit.collider.gameObject.name);

            FlowMilk?.Invoke();

            timer = 0f;
           
        }

        // Timer while pouring
        timer += Time.deltaTime;
        if(timer< max && isHit)
        {
            transform.position = hit.collider.gameObject.transform.position + new Vector3(0f, -2f, 0f);
        }

        wasHit = isHit;
    
    }
}
