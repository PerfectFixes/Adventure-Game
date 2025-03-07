using System;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{

    private Animator animator;
    
    private Canvas interactCanvas;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        interactCanvas = GetComponentInChildren<Canvas>();
        interactCanvas.gameObject.SetActive(false);
    }

    public void DisplayUI()
    {
        interactCanvas.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        interactCanvas.gameObject.SetActive(false);
    }
    public void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");
    }
}
