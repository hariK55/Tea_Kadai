using System;
using UnityEngine;

public class Strainer : MonoBehaviour
{
    [SerializeField] private GameObject idleStrainerObj;
    [SerializeField] private GameObject StrainerPouringObj;

    public static Strainer Instance { get; private set; }

    public event Action Onpour;
    public event Action OnStopPour;
    public event Action OntakeStrainer;
    public event Action OnputStrainer;

    private Animator strainerAnimator;

    private Collider2D strainerCollider;

    private SpriteRenderer SpriteRendererStrainer;

    private Holdable2D Holdable2D;

    private int ticationCount = 0;

    private float pourTimer = 0f;
    private bool isPouring = false;

    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask detectionLayer;
    private bool isGlassBelow = false;
    

    private bool isHit;
    private bool wasHit;


    private void Awake()
    {
        Instance = this;

        strainerAnimator=StrainerPouringObj.GetComponent<Animator>();

        strainerCollider=GetComponent<Collider2D>();

       // SpriteRendererStrainer=GetComponent<SpriteRenderer>();

        Holdable2D=GetComponent<Holdable2D>();
    }

    private void Update()
    {
        if(Holdable2D != Holdable2D.Instance.IsHolding())
        { return; }

        IsGlassBelowraycast();
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        Holdable2D.OnHold += OnHold;
        Holdable2D.OnRelease += OnRelease;
    }

    private void OnDisable()
    {
        Holdable2D.OnHold -= OnHold;
        Holdable2D.OnRelease -= OnRelease;
    }

    private void Start()
    {
       idleStrainerObj.SetActive(false);
        StrainerPouringObj.SetActive(false);
    }
    private void OnRelease()
    {
        idleStrainerObj.SetActive(false);
        StrainerPouringObj.SetActive(false);
        OnputStrainer?.Invoke();
    }

    private void OnHold()
    {
        idleStrainerObj.SetActive(true);
        OntakeStrainer?.Invoke();    }
    Vector2 origin;
    Vector2 direction;
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

        // Raycast detected object for the first time
        if (isHit && !wasHit)
        {
            Debug.Log("Object below: " + hit.collider.gameObject.name);

            Onpour?.Invoke();
            Ticationpour();

            isGlassBelow = true;
            isPouring = true;
            pourTimer = 0f;
        }

        // Timer while pouring
        if (isPouring)
        {
            pourTimer += Time.deltaTime;

            if (pourTimer >= 0.417f)
            {
                isPouring = false;
                isGlassBelow = false;

                StopTicationpour();
                OnStopPour?.Invoke();
            }
        }

        wasHit = isHit;
    }

    private void Ticationpour()
    {
          idleStrainerObj.SetActive(false);
        StrainerPouringObj.SetActive(true);
        strainerAnimator.Play("ticationPouring");

            ticationCount++;
       
    }

    private void StopTicationpour()
    {
       // transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
       // strainerAnimator.SetBool("pourTication", false);
       
        StrainerPouringObj.SetActive(false);
        idleStrainerObj.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * rayDistance;

        Gizmos.DrawLine(start, end);
    }
}
