using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Customer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Order")]
    [SerializeField] private TextMeshProUGUI orderNameText;
    [SerializeField] private Image orderImage;

    [Header("Hand")]
    [SerializeField] private Transform handPoint;

    private RecipeSO currentRecipe;

    private Transform targetPosition;

    private bool hasReachedPosition;
    private bool orderCompleted;
    private bool isLeaving;
    private Transform exitPosition;

    public event Action<Customer> LeftScene;
    public Transform TargetPosition => targetPosition;

    public RecipeSO CurrentRecipe => currentRecipe;
    public bool HasReachedPosition => hasReachedPosition;

    private void Update()
    {
        if (targetPosition == null)
            return;

        if (hasReachedPosition)
            return;

        MoveToTarget();
    }

    // --------------------------------
    // ORDER
    // --------------------------------

    public void SetOrder(RecipeSO recipe)
    {
        currentRecipe = recipe;
        orderCompleted = false;

        if (orderNameText != null)
        {
            orderNameText.text = recipe.recipeName;
        }

        if (orderImage != null)
        {
            orderImage.sprite = recipe.drinkSprite;
            orderImage.enabled = true;
        }
    }

    // --------------------------------
    // MOVEMENT
    // --------------------------------

    public void SetExitPosition(Transform target)
    {
        exitPosition = target;
    }

    public void SetTargetPosition(Transform target)
    {
        targetPosition = target;
        hasReachedPosition = false;
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            targetPosition.position) < 0.01f)
        {
            transform.position = targetPosition.position;

            hasReachedPosition = true;

            if (isLeaving)
            {
                LeftScene?.Invoke(this);
                Destroy(gameObject);
                return;
            }

            Debug.Log(
                gameObject.name +
                " reached waiting position."
            );
        }
    }

    // --------------------------------
    // GLASS DETECTION
    // --------------------------------

    public void LeaveThrough(Transform spawnPoint)
    {
        if (spawnPoint == null || isLeaving)
            return;

        isLeaving = true;
        exitPosition = spawnPoint;
        targetPosition = exitPosition;
        hasReachedPosition = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLeaving || !hasReachedPosition)
            return;
        GlassContents glass =
            other.GetComponentInParent<GlassContents>();

        if (glass == null)
            return;

        Debug.Log(
            "Glass detected by " +
            gameObject.name
        );

        TryServe(glass);
    }

    // --------------------------------
    // SERVING
    // --------------------------------

    private void TryServe(GlassContents glass)
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning(
                "Customer has no recipe."
            );

            return;
        }

        if (orderCompleted)
            return;

        if (IsRecipeMatch(glass, currentRecipe))
        {
            Debug.Log(
                "CORRECT ORDER: " +
                currentRecipe.recipeName
            );

            ReceiveDrink(glass);
        }
        else
        {
            Debug.Log(
                "WRONG ORDER for " +
                gameObject.name
            );
        }
    }

    // --------------------------------
    // RECIPE COMPARISON
    // --------------------------------

    private bool IsRecipeMatch(
        GlassContents glass,
        RecipeSO recipe)
    {
        if (glass.ticationCount !=
            recipe.ticationAmount)
        {
            return false;
        }

        if (glass.sugarCount !=
            recipe.sugarSpoons)
        {
            return false;
        }

        if (glass.milkCount !=
            recipe.Milk)
        {
            return false;
        }

        return true;
    }

    // --------------------------------
    // RECEIVE DRINK
    // --------------------------------

    private void ReceiveDrink(GlassContents glass)
    {
        orderCompleted = true;

        SpawnDrinkInHand();

        HideOrderUI();

        ResetGlass(glass);

        Debug.Log(
            gameObject.name +
            " received the drink."
        );

        LeaveThrough(exitPosition);
    }

    // --------------------------------
    // DRINK SPRITE IN HAND
    // --------------------------------

    private void SpawnDrinkInHand()
    {
        if (currentRecipe.drinkSprite == null)
        {
            Debug.LogWarning(
                "Recipe has no drink sprite."
            );

            return;
        }

        GameObject drink =
            new GameObject("DeliveredDrink");

        if (handPoint == null)
        {
            Destroy(drink);
            return;
        }

        drink.transform.SetParent(handPoint);

        drink.transform.localPosition =
            Vector3.zero;

        drink.transform.localRotation =
            Quaternion.identity;

        drink.transform.localScale =
            Vector3.one*0.5f;

        SpriteRenderer renderer =
            drink.AddComponent<SpriteRenderer>();

        renderer.sprite =
            currentRecipe.drinkSprite;
    }

    // --------------------------------
    // HIDE ORDER
    // --------------------------------

    private void HideOrderUI()
    {
        if (orderNameText != null)
            orderNameText.gameObject.SetActive(false);

        if (orderImage != null)
            orderImage.gameObject.SetActive(false);
    }

    // --------------------------------
    // RESET GLASS
    // --------------------------------

    private void ResetGlass(GlassContents glass)
    {
        Glass.Instance.ResetGlassContents();
        glass.gameObject.SetActive(false);
        Glass.Instance.respawn();
      //  glass.gameObject.transform.position = new Vector3(-0.579999983f, -2.04858279f, 0f);
        glass.gameObject.SetActive(true);
    }
}