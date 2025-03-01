using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Canvas interactionCanvas;
    
    // References
    [SerializeField] private Animator doorAnimator; // Can be assigned in inspector if needed
    
    // State tracking
    private bool playerInRange = false;
    private bool isDoorOpen = false;
    
    private void Awake()
    {
        // Find animator if not already assigned
        if (doorAnimator == null)
        {
            // First try to get the animator on this GameObject
            doorAnimator = GetComponent<Animator>();
            
            // If not found, check the parent GameObject
            if (doorAnimator == null && transform.parent != null)
            {
                doorAnimator = transform.parent.GetComponent<Animator>();
            }
            
            // If still not found, search in children of parent
            if (doorAnimator == null && transform.parent != null)
            {
                doorAnimator = transform.parent.GetComponentInChildren<Animator>(true);
            }
            
            // Log a warning if animator still not found
            if (doorAnimator == null)
            {
                Debug.LogWarning($"No Animator found for door {gameObject.name}. Please assign one in the inspector or add to parent.");
            }
        }
        
        // If no canvas is assigned in the inspector, try to find it
        if (interactionCanvas == null)
        {
            // Check if there's a Canvas as a child
            interactionCanvas = GetComponentInChildren<Canvas>(true);
        }
        
        // Ensure canvas is disabled at start
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Enable the input action
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        // Disable the input action
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
        
        // Hide the canvas when disabled
        if (interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player entering the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Only show interaction canvas if the door isn't open yet
            if (!isDoorOpen && interactionCanvas != null)
            {
                interactionCanvas.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player leaving the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // Hide interaction canvas when player leaves
            if (interactionCanvas != null)
            {
                interactionCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Only handle interaction if player is in range and door isn't already open
        if (playerInRange && !isDoorOpen)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        // Trigger the animation
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("OpenDoor");
            isDoorOpen = true;
            
            // Hide the interaction canvas
            if (interactionCanvas != null)
            {
                interactionCanvas.gameObject.SetActive(false);
            }
        }
    }

    // Called by animation event when door is fully opened if needed
    public void OnDoorFullyOpened()
    {
        // Additional functionality can be added here
    }

    // Public method to reset door state if needed (e.g., for doors that can close)
    public void ResetDoor()
    {
        isDoorOpen = false;
        
        // Show interaction canvas if player is still in range
        if (playerInRange && interactionCanvas != null)
        {
            interactionCanvas.gameObject.SetActive(true);
        }
    }
}