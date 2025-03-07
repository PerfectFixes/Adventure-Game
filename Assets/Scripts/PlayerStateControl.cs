using System;
using UnityEngine;

public class PlayerStateControl : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGameObject;

    [SerializeField]
    private GameObject laserPuzzleObject;
    public enum PlayerState
    {
        Moving,
        Puzzling,
    }

    public PlayerState currentState = PlayerState.Moving;
    private void Awake()
    {
        SetPlayerState(PlayerState.Moving);
    }

  

    public void SetPlayerState(PlayerState state)
    {
        if (PlayerState.Moving == state)
        {
            playerGameObject.SetActive(true);
            laserPuzzleObject.SetActive(false);
        }
        else if (PlayerState.Puzzling == state) 
        {
            playerGameObject.SetActive(false);
            laserPuzzleObject.SetActive(true);
        }
        
    }
}
