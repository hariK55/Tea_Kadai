using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private Customer customerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;

    [Header("Waiting Positions")]
    [SerializeField] private Transform[] waitingPositions;

    [Header("Recipes")]
    [SerializeField] private RecipeSO[] recipes;

    [Header("Customer Settings")]
    [SerializeField] private int maximumCustomers = 4;

    private List<Customer> customers = new List<Customer>();

    private void Start()
    {
        int customersToSpawn = Mathf.Min(2, waitingPositions == null ? 0 : waitingPositions.Length);
        for (int i = 0; i < customersToSpawn; i++)
            SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        CleanupCustomers();

        if (customers.Count >= maximumCustomers)
        {
            Debug.Log("Maximum customers reached.");
            return;
        }

        if (customerPrefab == null || leftSpawnPoint == null || rightSpawnPoint == null)
        {
            Debug.LogWarning("Assign a customer prefab and both spawn points.");
            return;
        }

        Transform freePosition = GetFreePosition();

        if (freePosition == null)
        {
            Debug.Log("No free customer position.");
            return;
        }

        if (recipes == null || recipes.Length == 0)
        {
            Debug.LogWarning("No recipes assigned.");
            return;
        }

        Transform spawnPoint;

        if (Random.value < 0.5f)
            spawnPoint = leftSpawnPoint;
        else
            spawnPoint = rightSpawnPoint;

        Customer customer =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

        customers.Add(customer);
        customer.LeftScene += HandleCustomerLeft;

        customer.SetExitPosition(spawnPoint);
        customer.SetTargetPosition(freePosition);

        RecipeSO randomRecipe =
            recipes[Random.Range(0, recipes.Length)];

        customer.SetOrder(randomRecipe);

        customer.name =
            "Customer_" + customers.Count;
    }

    private Transform GetFreePosition()
    {
        foreach (Transform position in waitingPositions)
        {
            bool occupied = false;

            foreach (Customer customer in customers)
            {
                if (customer == null)
                    continue;

                if (customer.TargetPosition == position)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
                return position;
        }

        return null;
    }

    private void HandleCustomerLeft(Customer customer)
    {
        if (customer != null)
            customer.LeftScene -= HandleCustomerLeft;

        customers.Remove(customer);
        SpawnCustomer();
    }

    private void CleanupCustomers()
    {
        customers.RemoveAll(customer => customer == null);
    }
}