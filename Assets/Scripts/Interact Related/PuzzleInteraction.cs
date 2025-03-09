using System;
using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
    private Canvas interactCanvas;
    private bool hasInteracted = false;
    
    [SerializeField]
    private PlayerStateControl playerStateControl;
    
    [SerializeField]
    private PlayerStateControl.PlayerState puzzleType;

    private void Awake()
    {
        interactCanvas = GetComponentInChildren<Canvas>();
        interactCanvas.gameObject.SetActive(false);
    }

    public void StartPuzzle()
    {
        if (hasInteracted)
        {
            return;
        }
        hasInteracted = true;
        playerStateControl.SetPlayerState(puzzleType);
    }

    public void DisplayUI()
    {
        interactCanvas.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        interactCanvas.gameObject.SetActive(false);
    }
}