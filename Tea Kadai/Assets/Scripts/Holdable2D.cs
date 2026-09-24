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

    [Header("Position")]
    [SerializeField] private Transform centreTable;

    [Header("Settings")]
    [SerializeField] private float floatHeight = 0.5f;

    private Camera mainCamera;

    private bool isHolding;
    private bool canMove;

    private Vector3 offset;

    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    // Only ONE Holdable2D can be held at a time
    private static Holdable2D currentlyHeldObject;

    private GlassContents glassContents;

    private void Awake()
    {
        mainCamera = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        glassContents = GetComponent<GlassContents>();

        canMove = true;
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

        if (currentlyHeldObject != this)
            return;

        if (!canMove)
            return;

        MoveObject();
    }

    // =========================================================
    // PRESS
    // =========================================================

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        // Another object is already being held
        if (currentlyHeldObject != null)
            return;

        // This object is temporarily locked
        if (!canMove)
            return;

        Vector3 pointerWorldPosition =
            GetPointerWorldPosition();

        Collider2D hit =
            Physics2D.OverlapPoint(pointerWorldPosition);

        if (hit == null)
            return;

        if (hit.gameObject != gameObject)
            return;

        // Start holding
        isHolding = true;

        currentlyHeldObject = this;

        offset =
            transform.position -
            pointerWorldPosition;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 3;
        }

        OnHold?.Invoke();
    }

    // =========================================================
    // NORMAL RELEASE
    // =========================================================

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (!isHolding)
            return;

        // If locked, don't release.
        // Example: strainer is pouring.
        if (!canMove)
            return;

        // Normal glass behavior:
        // return to centre table.
        if (centreTable != null)
        {
            transform.position = new Vector3(
                centreTable.position.x,
                centreTable.position.y,
                transform.position.z
            );
        }

        ForceRelease();
    }

    // =========================================================
    // FORCE RELEASE
    // =========================================================

    public void ForceRelease()
    {
        if (!isHolding)
            return;

        isHolding = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder =
                originalSortingOrder;
        }

        if (currentlyHeldObject == this)
        {
            currentlyHeldObject = null;
        }

        // All release logic goes through this event
        OnRelease?.Invoke();
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveObject()
    {
        Vector3 pointerWorldPosition =
            GetPointerWorldPosition();

        Vector3 targetPosition =
            pointerWorldPosition + offset;

        targetPosition.y += floatHeight;

        targetPosition.z =
            transform.position.z;

        transform.position =
            targetPosition;
    }

    // =========================================================
    // POINTER POSITION
    // =========================================================

    private Vector3 GetPointerWorldPosition()
    {
        Vector2 screenPosition =
            pointerPosition.action.ReadValue<Vector2>();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(
                        mainCamera.transform.position.z
                    )
                )
            );

        worldPosition.z =
            transform.position.z;

        return worldPosition;
    }

    // =========================================================
    // PUBLIC FUNCTIONS
    // =========================================================

    public bool IsThisObjectBeingHeld()
    {
        return isHolding &&
               currentlyHeldObject == this;
    }

    public bool IsMovementLocked()
    {
        return !canMove;
    }

    public void cantMove()
    {
        canMove = false;
    }

    public void CanMove()
    {
        canMove = true;
    }

    public void getBack()
    {
        if (centreTable == null)
            return;

        transform.position = new Vector3(
            centreTable.position.x,
            centreTable.position.y,
            transform.position.z
        );
    }

    public void MoveToCentre()
    {
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

        // Release this object so another object can be held
        if (currentlyHeldObject == this)
        {
            currentlyHeldObject = null;
        }

        OnRelease?.Invoke();
    }
}


