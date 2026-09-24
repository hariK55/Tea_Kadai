using UnityEngine;
using UnityEngine.InputSystem;

public class GlassPlate : MonoBehaviour
{
    [Header("Glass")]
    [SerializeField] private GameObject emptyGlassPrefab;
    [SerializeField] private Transform centreTable;

    [Header("Input System")]
    [SerializeField] private InputActionReference pointerPosition;
    [SerializeField] private InputActionReference pointerButton;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        pointerButton.action.Enable();
        pointerPosition.action.Enable();
    }

    private void OnDisable()
    {
        pointerButton.action.Disable();
        pointerPosition.action.Disable();
    }

    private void Update()
    {
        // Only react when player taps/clicks
        if (!pointerButton.action.WasPressedThisFrame())
            return;

        Vector2 screenPosition =
            pointerPosition.action.ReadValue<Vector2>();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                Mathf.Abs(mainCamera.transform.position.z)
            )
        );

        worldPosition.z = 0f;

        // Check if the plate was clicked
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        if (hit.gameObject != gameObject)
            return;

        SpawnEmptyGlass();
    }

    private void SpawnEmptyGlass()
    {
        if (emptyGlassPrefab == null)
        {
            Debug.LogError("GlassPlate: Empty Glass Prefab is not assigned.");
            return;
        }

        if (centreTable == null)
        {
            Debug.LogError("GlassPlate: Centre Table is not assigned.");
            return;
        }

        // Spawn fresh empty glass at table center
        GameObject newGlass = Instantiate(
            emptyGlassPrefab,
            centreTable.position,
            Quaternion.identity
        );

        Debug.Log("Empty glass spawned at table center.");
    }
}