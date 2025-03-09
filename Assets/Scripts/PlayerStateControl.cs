using System;
using System.Collections;
using UnityEngine;

public class PlayerStateControl : MonoBehaviour
{
    [SerializeField]
    private GameObject playerGameObject;
    
    [SerializeField]
    private GameObject playerCamera;

    [SerializeField]
    private GameObject laserPuzzleObject;
    
    [SerializeField]
    private GameObject laserPuzzleCamera;

    [SerializeField] 
    private Animator fadeAnimator;

    public enum PlayerState
    {
        Moving,
        LaserPuzzle,
        CubePuzzle,
    }

    public PlayerState currentState = PlayerState.Moving;

    private void Awake()
    {
        playerGameObject.SetActive(true);
        laserPuzzleObject.SetActive(false);
    }

    private IEnumerator SwitchCamera(string cameraName)
    {
       
        if (cameraName == "Player")
        {
            fadeAnimator.SetBool("isFinished", false);
            fadeAnimator.SetTrigger("StartFade");
            yield return new WaitForSeconds(1f);
           //Turn Off/On camera 
            playerCamera.SetActive(true);
            laserPuzzleCamera.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            //Turn Off/On the gameobject 
            playerGameObject.SetActive(true);
            laserPuzzleObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            fadeAnimator.SetBool("isFinished", true);
        }
        else
        {
            fadeAnimator.SetBool("isFinished", false);
            fadeAnimator.SetTrigger("StartFade");
            yield return new WaitForSeconds(1f);
            //Turn Off/On camera 
            playerCamera.SetActive(false);
            laserPuzzleCamera.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            //Turn Off/On the gameobject 
            playerGameObject.SetActive(false);
            laserPuzzleObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            fadeAnimator.SetBool("isFinished", true);
        }
       
    }
    public void SetPlayerState(PlayerState state)
    {
        if (PlayerState.Moving == state)
        {
            StartCoroutine(SwitchCamera("Player"));
        }
        else if (PlayerState.LaserPuzzle == state) 
        {
            StartCoroutine(SwitchCamera("Puzzle"));
        }
        
    }
}
