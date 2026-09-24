using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("Recipe Database")]
    [SerializeField] private RecipeSO[] availableRecipes;

    [Header("Orders")]
    [SerializeField] private int numberOfOrders = 3;

    private List<RecipeSO> activeOrders = new List<RecipeSO>();

    private void Start()
    {
        GenerateRandomOrders();
    }

    private void GenerateRandomOrders()
    {
        activeOrders.Clear();

        if (availableRecipes == null || availableRecipes.Length == 0)
        {
            Debug.LogWarning("No RecipeSO assigned to Counter.");
            return;
        }

        for (int i = 0; i < numberOfOrders; i++)
        {
            RecipeSO randomRecipe =
                availableRecipes[Random.Range(0, availableRecipes.Length)];

            activeOrders.Add(randomRecipe);

            Debug.Log(
                "Order " + (i + 1) +
                ": " + randomRecipe.recipeName
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GlassContents glass = other.GetComponent<GlassContents>();

        if (glass == null)
            return;

        TryDeliverGlass(glass);
    }

    private void TryDeliverGlass(GlassContents glass)
    {
        RecipeSO matchedRecipe = null;

        foreach (RecipeSO recipe in activeOrders)
        {
            if (IsGlassMatch(glass, recipe))
            {
                matchedRecipe = recipe;
                break;
            }
        }

        if (matchedRecipe != null)
        {
            Debug.Log(
                "CORRECT ORDER DELIVERED: " +
                matchedRecipe.recipeName
            );

            DeliverOrder(matchedRecipe, glass);
        }
        else
        {
            Debug.Log("WRONG ORDER - Cannot deliver this glass.");
        }
    }

    private bool IsGlassMatch(GlassContents glass, RecipeSO recipe)
    {
        if (glass.ticationCount != recipe.ticationAmount)
            return false;

        if (glass.sugarCount != recipe.sugarSpoons)
            return false;

        if (glass.milkCount != recipe.Milk)
            return false;

        return true;
    }

    private void DeliverOrder(RecipeSO recipe, GlassContents glass)
    {
        // Remove the fulfilled order
        activeOrders.Remove(recipe);

        Debug.Log(
            "Order completed: " +
            recipe.recipeName
        );

        // TODO:
        // Destroy/reset the glass here if required.

        // Generate a replacement order
        GenerateReplacementOrder();
    }

    private void GenerateReplacementOrder()
    {
        if (availableRecipes == null || availableRecipes.Length == 0)
            return;

        RecipeSO randomRecipe =
            availableRecipes[Random.Range(0, availableRecipes.Length)];

        activeOrders.Add(randomRecipe);

        Debug.Log(
            "New order generated: " +
            randomRecipe.recipeName
        );
    }

    public List<RecipeSO> GetActiveOrders()
    {
        return activeOrders;
    }
}