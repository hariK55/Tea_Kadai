using System;
using UnityEngine;

public class BoilerController : MonoBehaviour
{
    private Animator boilerAnimator;
    [SerializeField] private GameObject boilerIdle;
    [SerializeField] private GameObject boilerNoStrainer;

    private void Awake()
    {
       // boilerAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        Strainer.Instance.OntakeStrainer += OnTakeStrainer;
        Strainer.Instance.OnputStrainer += OnPutStrainer;

    }

    private void OnPutStrainer()
    {
        boilerNoStrainer.SetActive(false);
        boilerIdle.SetActive(true);
    }

    private void OnTakeStrainer()
    {
       boilerIdle.SetActive(false);
        boilerNoStrainer.SetActive(true);
    }
}
