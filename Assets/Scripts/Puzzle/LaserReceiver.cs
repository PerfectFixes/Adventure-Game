using System.Collections.Generic;
using UnityEngine;

public class LaserReceiver : MonoBehaviour
{
    [Tooltip("If enabled, the receiver will check that all deflectors have been hit")]
    public bool requireAllDeflectors = true;
    
    [Tooltip("Optional visual feedback when the receiver is activated")]
    public GameObject activationEffect;
    
    [Tooltip("Color to change the receiver to when activated")]
    public Color activatedColor = Color.green;
    
    // State tracking
    private bool isPuzzleSolved = false;
    private Renderer receiverRenderer;
    private Color originalColor;
    
    private void Awake()
    {
        // Cache the renderer component
        receiverRenderer = GetComponent<Renderer>();
        if (receiverRenderer != null)
        {
            originalColor = receiverRenderer.material.color;
        }
        
        // Disable any activation effect initially
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called by the LaserEmitter when a laser hits this receiver
    /// </summary>
    /// <param name="hitDeflectors">List of deflectors the laser has passed through</param>
    /// <param name="totalDeflectors">Total number of deflectors in the scene</param>
    public void ReceiveLaser(List<LaserDeflector> hitDeflectors, int totalDeflectors)
    {
        // If puzzle is already solved, no need to process again
        if (isPuzzleSolved)
            return;
            
        bool allDeflectorsHit = (hitDeflectors.Count == totalDeflectors);
        
        // Check puzzle completion condition
        if (!requireAllDeflectors || allDeflectorsHit)
        {
            // Puzzle solved!
            isPuzzleSolved = true;
            
            // Visual feedback
            if (receiverRenderer != null)
            {
                receiverRenderer.material.color = activatedColor;
            }
            
            // Show activation effect if available
            if (activationEffect != null)
            {
                activationEffect.SetActive(true);
            }
            
            // Debug message - this would be replaced with actual game events later
            if (allDeflectorsHit)
            {
                Debug.Log("Puzzle Solved! Laser has passed through all " + totalDeflectors + " deflectors and hit the receiver.");
                IsPuzzleSolved();
            }
            else
            {
                Debug.Log("Receiver activated! Laser has reached the destination.");
            }
        }
        else
        {
            Debug.Log("Receiver activated! Laser has reached the destination but didnt hit all the deflectors.");
        }
    }
    
   
    /// Resets the receiver to its initial state (for puzzle reset)
    public void ResetReceiver()
    {
        isPuzzleSolved = false;
        
        // Reset visual elements
        if (receiverRenderer != null)
        {
            receiverRenderer.material.color = originalColor;
        }
        
        if (activationEffect != null)
        {
            activationEffect.SetActive(false);
        }
    }
    
 
    /// Returns whether the puzzle has been solved
    public bool IsPuzzleSolved()
    {
        return isPuzzleSolved;
    }
}