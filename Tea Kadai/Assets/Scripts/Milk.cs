using System;
using UnityEngine;

public class Milk : MonoBehaviour
{
    public static Milk Instance { get; private set;  }
    public event Action milkAdded;
    private Animator animatorMilk;

    private void Awake()
    {
        Instance = this;
        animatorMilk= GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Glass.Instance.FlowMilk += OnFlowMilk;
    }

    private void OnFlowMilk()
    {
        animatorMilk.SetTrigger("isMilkFlow");
        milkAdded?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
