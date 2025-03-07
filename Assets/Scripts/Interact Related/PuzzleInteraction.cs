using System;
using UnityEngine;

public class PuzzleInteraction : MonoBehaviour
{
    private Canvas interactCanvas;
    
    [SerializeField]
    private PlayerStateControl playerStateControl;

    private void Awake()
    {
        interactCanvas = GetComponentInChildren<Canvas>();
    }

    public void StartPuzzle()
    {
        playerStateControl.SetPlayerState(PlayerStateControl.PlayerState.Puzzling);
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
