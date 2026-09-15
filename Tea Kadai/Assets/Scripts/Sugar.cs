using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sugar : MonoBehaviour
{
    public static Sugar Instance { get; private set; }
    public event Action OnSugarAdded;

    [SerializeField] private InputActionReference pointerPosition;
    [SerializeField] private InputActionReference pointerButton;

    private Collider2D sugarCollider;
    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        sugarCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        pointerPosition.action.Enable();
        pointerButton.action.Enable();
    }

    private void OnDisable()
    {
        pointerPosition.action.Disable();
        pointerButton.action.Disable();
    }

    private void Update()
    {
        if (!pointerButton.action.WasPressedThisFrame())
            return;

        if (pointerButton.action.WasPressedThisFrame())
        {
            Debug.Log("POINTER BUTTON PRESSED");
        }

        Vector2 screenPosition = pointerPosition.action.ReadValue<Vector2>();

        Vector3 worldPosition =mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(mainCamera.transform.position.z)));

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit != null)
        {
            Debug.Log("HIT: " + hit.gameObject.name);
            Debug.Log("SugarBox script object: " + gameObject.name);
        }
        else
        {
            Debug.Log("NOTHING HIT");
        }
        if (hit != null )
        {
            if(hit.gameObject == gameObject)
            {
                Debug.Log("Sugar Box Pressed!");

                OnSugarAdded?.Invoke();
            }
        }
    }

  
}