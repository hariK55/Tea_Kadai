using System;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public static Glass Instance { get; private set; }

    public event Action FlowMilk;

    private Animator glassAnimator;
    private Strainer strainer;
    private Holdable2D holdable;
    private GlassContents glassContents;

    private bool isHit;
    private bool wasHit;

    private Vector2 origin;
    private Vector2 direction;

    [SerializeField]
    private float rayDistance = 0.5f;

    [SerializeField]
    private LayerMask detectionLayer;

    [Header("Milk Flow")]
    [SerializeField]
    private float milkFlowDuration = 3f;

    private float timer;

    private bool isMilkFlowing;

    private Sugar Sugar;

    private void Awake()
    {
        Instance = this;

        glassAnimator = GetComponent<Animator>();

        glassContents = GetComponent<GlassContents>();

        holdable = GetComponent<Holdable2D>();
    }

    private void Start()
    {
        strainer = Strainer.Instance;

        if (strainer != null)
        {
            strainer.Onpour += FillTication;
            strainer.OnStopPour += OnStopPour;
        }

        if (Milk.Instance != null)
        {
            Milk.Instance.milkAdded += OnmilkAdded;
        }

        Sugar = Sugar.Instance;

        if (Sugar != null)
        {
            Sugar.OnSugarAdded += Sugar_OnSugarAdded;
        }
    }

    private void Update()
    {
        DetectionRaycast();

        HandleMilkFlowTimer();
    }

    private void Sugar_OnSugarAdded()
    {
        if (glassContents.sugarCount >= 3)
        {
            Debug.Log("Maximum sugar count reached.");
            return;
        }

        glassContents.sugarCount++;

        glassAnimator.SetInteger(
            "sugarCount",
            glassContents.sugarCount
        );
    }

    private void OnmilkAdded()
    {
        if (glassContents.milkCount >= 2)
        {
            Debug.Log("Maximum milk count reached.");
            return;
        }

        glassContents.milkCount++;

        glassAnimator.SetBool(
            "isMilkAdded",
            true
        );

        glassAnimator.SetInteger(
            "addMilk",
            glassContents.milkCount
        );
    }

    private void FillTication()
    {
        // Don't add tication if the recipe doesn't allow it
        if (!canAddTication())
        {
            Debug.Log("Cannot add tication.");
            return;
        }

        glassAnimator.SetBool(
            "onTicationfill",
            true
        );

        glassContents.ticationCount++;

        glassAnimator.SetInteger(
            "ticationCount",
            glassContents.ticationCount
        );
    }

    private void OnStopPour()
    {
        // Handle stop pouring here if required
    }

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

        if (!isHit)
        {
            wasHit = false;
            return;
        }

        int hitLayer = hit.collider.gameObject.layer;

        int milkLayerIndex =
            LayerMask.NameToLayer("Milk");

        int sinkLayerIndex =
            LayerMask.NameToLayer("Sink");

        // ==========================================
        // MILK TANK DETECTION
        // ==========================================

        if (isHit &&
            !wasHit &&
            hitLayer == milkLayerIndex)
        {
            Debug.Log(
                "Milk tank detected: " +
                hit.collider.gameObject.name
            );

            if (!canAddMilk())
            {
                Debug.Log("Maximum milk count reached.");
                return;
            }

            // Prevent starting another milk flow
            if (isMilkFlowing)
                return;

            // Lock glass movement
            holdable.cantMove();

               // =================================================
            // PLACE STRAINER ABOVE GLASS
            // =================================================

            Transform glassTransform =
                hit.collider.transform;

            transform.position = new Vector3(
                glassTransform.position.x,
                glassTransform.position.y-2.6f
                ,
                transform.position.z
            );


            // Start milk flow
            FlowMilk?.Invoke();

            // Start timer
            timer = 0f;

            isMilkFlowing = true;
        }

        // ==========================================
        // SINK DETECTION
        // ==========================================

        if (isHit &&
            !wasHit &&
            hitLayer == sinkLayerIndex)
        {
            Debug.Log(
                "Sink detected: " +
                hit.collider.gameObject.name
            );

            ResetGlassContents();
        }

        wasHit = isHit;
    }

    private void HandleMilkFlowTimer()
    {
        if (!isMilkFlowing)
            return;

        timer += Time.deltaTime;

        if (timer >= milkFlowDuration)
        {
            isMilkFlowing = false;

            timer = 0f;

            Debug.Log("Milk flow finished.");

            // Return glass to centre table
            holdable.MoveToCentre();

            // Allow glass to move again
            holdable.CanMove();
        }
    }

    public bool canAddMilk()
    {
        if (
            (glassContents.milkCount < 2 &&
             glassContents.ticationCount == 0)
            ||
            (glassContents.milkCount == 0 &&
             glassContents.ticationCount == 1)
           )
        {
            return true;
        }

        return false;
    }

    public bool canAddTication()
    {
        if (
            (glassContents.milkCount == 0 &&
             glassContents.ticationCount < 2)
            ||
            (glassContents.milkCount == 1 &&
             glassContents.ticationCount == 0)
           )
        {
            return true;
        }

        return false;
    }

    public void ResetGlassContents()
    {
        glassContents.ticationCount = 0;
        glassContents.sugarCount = 0;
        glassContents.milkCount = 0;

        glassAnimator.SetInteger(
            "ticationCount",
            glassContents.ticationCount
        );

        glassAnimator.SetInteger(
            "sugarCount",
            glassContents.sugarCount
        );

        glassAnimator.SetInteger(
            "addMilk",
            glassContents.milkCount
        );

        glassAnimator.SetBool(
            "isMilkAdded",
            false
        );

        glassAnimator.SetTrigger("reset");
    }

    public void respawn()
    {
        holdable.cantMove();
        holdable.MoveToCentre();
        holdable.CanMove();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            Vector3.down * rayDistance
        );
    }
}