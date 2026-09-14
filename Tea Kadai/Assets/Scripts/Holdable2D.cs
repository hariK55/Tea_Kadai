using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Holdable2D : MonoBehaviour
{
    public event Action OnHold;
    public event Action OnRelease;
    [Header("Input")]
    [SerializeField] private InputActionReference pointerPosition;
    [SerializeField] private InputActionReference pointerPress;

    [SerializeField]
    private Transform centreTable;
    [Header("Settings")]
    [SerializeField] private float floatHeight = 0.5f;

    private Camera mainCamera;

    private bool isHolding;
    private Vector3 offset;

    private SpriteRenderer spriteRenderer;

    private int originalSortingOrder;
    private static Holdable2D currentlyHeldObject;

    public static Holdable2D Instance { get; private set; }

    private void Awake()
    {
        Instance= this;

        mainCamera = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    private void OnEnable()
    {
        pointerPosition.action.Enable();
        pointerPress.action.Enable();

        pointerPress.action.started += OnPressStarted;
        pointerPress.action.canceled += OnPressCanceled;
    }

    private void OnDisable()
    {
        pointerPress.action.started -= OnPressStarted;
        pointerPress.action.canceled -= OnPressCanceled;

        pointerPosition.action.Disable();
        pointerPress.action.Disable();
    }

    private void Update()
    {
        if (!isHolding)
            return;

        if(currentlyHeldObject != this)
            return;

        MoveObject();
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        // Another object is already being held
        if (currentlyHeldObject != null)
            return;

        Vector3 pointerWorldPosition = GetPointerWorldPosition();

        // Check whether the player pressed this object
        Collider2D hit = Physics2D.OverlapPoint(pointerWorldPosition);

        if (hit == null)
            return;

        if (hit.gameObject != gameObject)
            return;

        isHolding = true;

        // Register this object as the currently held object
        currentlyHeldObject = this;

        // Keep the object centered relative to where the player touched it
        offset = transform.position - pointerWorldPosition;

        // Bring glass in front
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 3;
        }

        OnHold?.Invoke();
    }
    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (!isHolding)
            return;

        transform.position = new Vector3(
            centreTable.position.x,
            centreTable.position.y,
            transform.position.z
        );

        isHolding = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        // Allow another object to be held
        if (currentlyHeldObject == this)
        {
            currentlyHeldObject = null;
        }

        OnRelease?.Invoke();
    }

    private void MoveObject()
    {
        Vector3 pointerWorldPosition = GetPointerWorldPosition();

        Vector3 targetPosition =
            pointerWorldPosition + offset;

        // Make the glass float slightly above the desk
        targetPosition.y += floatHeight;

        // Keep original Z
        targetPosition.z = transform.position.z;

        transform.position = targetPosition;
    }

    private Vector3 GetPointerWorldPosition()
    {
        Vector2 screenPosition = pointerPosition.action.ReadValue<Vector2>();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(mainCamera.transform.position.z)
                )
            );

        worldPosition.z = transform.position.z;

        return worldPosition;
    }

    public Holdable2D IsHolding()
    {
        return currentlyHeldObject;
    }
}