using System;
using UnityEngine;

public class Strainer : MonoBehaviour
{
    [Header("Strainer Objects")]
    [SerializeField] private GameObject idleStrainerObj;
    [SerializeField] private GameObject StrainerPouringObj;

    public static Strainer Instance { get; private set; }

    public event Action Onpour;
    public event Action OnStopPour;
    public event Action OntakeStrainer;
    public event Action OnputStrainer;

    private Animator strainerAnimator;
    private Collider2D strainerCollider;
    private Holdable2D holdable2D;

    private int ticationCount = 0;

    [Header("Pour Settings")]
    [SerializeField] private float pourDuration = 0.417f;

    [SerializeField] private float strainerHeightAboveGlass = 1.5f;

    private float pourTimer = 0f;
    private bool isPouring = false;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask detectionLayer;

    private bool isGlassBelow = false;

    private bool isHit;
    private bool wasHit;

    private Vector2 origin;
    private Vector2 direction;


    private void Awake()
    {
        Instance = this;

        strainerAnimator =
            StrainerPouringObj.GetComponent<Animator>();

        strainerCollider =
            GetComponent<Collider2D>();

        holdable2D =
            GetComponent<Holdable2D>();
    }


    private void OnEnable()
    {
        if (holdable2D != null)
        {
            holdable2D.OnHold += OnHold;
            holdable2D.OnRelease += OnRelease;
        }
    }


    private void OnDisable()
    {
        if (holdable2D != null)
        {
            holdable2D.OnHold -= OnHold;
            holdable2D.OnRelease -= OnRelease;
        }
    }


    private void Start()
    {
        idleStrainerObj.SetActive(false);
        StrainerPouringObj.SetActive(false);
    }


    private void Update()
    {
        // Only run while THIS strainer is being held
        if (!holdable2D.IsThisObjectBeingHeld())
            return;

        // During pouring, only handle the timer
        if (isPouring)
        {
            HandlePourTimer();
            return;
        }

        IsGlassBelowraycast();
    }


    // =========================================================
    // HOLD
    // =========================================================

    private void OnHold()
    {
        idleStrainerObj.SetActive(true);

        OntakeStrainer?.Invoke();

        Debug.Log("Strainer picked up.");
    }


    // =========================================================
    // RELEASE
    // =========================================================

    private void OnRelease()
    {
        idleStrainerObj.SetActive(false);
        StrainerPouringObj.SetActive(false);

        OnputStrainer?.Invoke();

        Debug.Log("Strainer released.");
    }


    // =========================================================
    // GLASS DETECTION
    // =========================================================

    private void IsGlassBelowraycast()
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


        // Glass detected for the first time
        if (isHit && !wasHit)
        {
            Debug.Log(
                "Object below: " +
                hit.collider.gameObject.name
            );


            if (Glass.Instance == null)
            {
                Debug.LogWarning(
                    "Glass.Instance is null."
                );

                wasHit = isHit;
                return;
            }


            if (!Glass.Instance.canAddTication())
            {
                Debug.Log(
                    "Cannot add tication."
                );

                wasHit = isHit;
                return;
            }


            // =================================================
            // PLACE STRAINER ABOVE GLASS
            // =================================================

            Transform glassTransform =
                hit.collider.transform;

            transform.position = new Vector3(
                glassTransform.position.x,
                glassTransform.position.y +
                strainerHeightAboveGlass,
                transform.position.z
            );


            // =================================================
            // LOCK STRAINER
            // =================================================

            holdable2D.cantMove();


            // =================================================
            // START POURING
            // =================================================

            Onpour?.Invoke();

            Ticationpour();


            isGlassBelow = true;

            isPouring = true;

            pourTimer = 0f;
        }


        wasHit = isHit;
    }


    // =========================================================
    // POUR TIMER
    // =========================================================

    private void HandlePourTimer()
    {
        pourTimer += Time.deltaTime;


        if (pourTimer >= pourDuration)
        {
            isPouring = false;

            isGlassBelow = false;


            // Stop pouring animation
            StopTicationpour();


            // Tell Glass that pouring stopped
            OnStopPour?.Invoke();


            // Allow movement again
            holdable2D.CanMove();


            pourTimer = 0f;


            // Automatically release
            // This calls OnRelease()
            holdable2D.ForceRelease();
            holdable2D.getBack();

            Debug.Log(
                "Pouring finished. " +
                "Strainer automatically released."
            );
        }
    }


    // =========================================================
    // START POURING
    // =========================================================

    private void Ticationpour()
    {
        idleStrainerObj.SetActive(false);

        StrainerPouringObj.SetActive(true);

        strainerAnimator.Play(
            "ticationPouring"
        );

        ticationCount++;

        Debug.Log(
            "Tication pouring started."
        );
    }


    // =========================================================
    // STOP POURING
    // =========================================================

    private void StopTicationpour()
    {
        StrainerPouringObj.SetActive(false);

        idleStrainerObj.SetActive(true);

        Debug.Log(
            "Tication pouring stopped."
        );
    }


    // =========================================================
    // DEBUG RAYCAST
    // =========================================================

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 start =
            transform.position;

        Vector3 end =
            start +
            Vector3.down * rayDistance;

        Gizmos.DrawLine(
            start,
            end
        );
    }
}