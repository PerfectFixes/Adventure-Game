using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerController : MonoBehaviour
{
    [Header("Tower Settings")]
    [Tooltip("List of all towers/deflectors that can be controlled")]
    public List<Transform> towers = new List<Transform>();
    
    [Tooltip("Material to apply to the selected tower")]
    public Material highlightMaterial;
    
    [Tooltip("Movement speed of the towers")]
    public float moveSpeed = 1.0f;
    
    [Tooltip("Minimum Z position of towers")]
    public float minZPosition = -0.5f;
    
    [Tooltip("Maximum Z position of towers")]
    public float maxZPosition = 0.5f;
    
    [Header("Input Settings")]
    [Tooltip("Reference to an Input Action asset with Move and Select actions")]
    public InputActionAsset inputActions;
    
    // Input action references
    private InputAction moveAction;
    private InputAction selectAction;
    
    // Currently selected tower index
    private int currentTowerIndex = -1;
    
    // Movement value
    private float verticalInput = 0f;
    
    // Store original materials for each tower
    private Dictionary<Transform, Material> originalMaterials = new Dictionary<Transform, Material>();
    
    private void Awake()
    {
        // Store original materials and set all towers to starting position
        foreach (Transform tower in towers)
        {
            if (tower != null)
            {
                // Store the original material
                Renderer renderer = tower.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials[tower] = renderer.material;
                }
                
                // Set starting Z position
                Vector3 startPosition = tower.position;
                startPosition.z = minZPosition;
                tower.position = startPosition;
            }
        }
        
        // Set up input actions from the asset
        if (inputActions != null)
        {
            // Get the "Player" action map (you might need to adjust this name)
            var playerActionMap = inputActions.FindActionMap("Player");
            
            if (playerActionMap != null)
            {
                // Get the move and select actions
                moveAction = playerActionMap.FindAction("Move");
                selectAction = playerActionMap.FindAction("Select");
                
                // Enable the action map
                playerActionMap.Enable();
                
                // Set up callbacks for the actions
                if (moveAction != null) moveAction.performed += OnMove;
                if (moveAction != null) moveAction.canceled += OnMove;
                if (selectAction != null) selectAction.performed += OnSelect;
            }
            else
            {
                Debug.LogError("Could not find 'Player' action map in the input actions asset!");
            }
        }
        else
        {
            Debug.LogError("Input Actions asset not assigned!");
            
            // Fallback to manual creation of actions
            SetupFallbackInputActions();
        }
        
        // Select the first tower by default if we have any
        if (towers.Count > 0)
        {
            currentTowerIndex = 0;
            UpdateTowerHighlight();
        }
    }
    
    // If no Input Action asset is provided, create the actions manually
    private void SetupFallbackInputActions()
    {
        // Create an action map
        var map = new InputActionMap("Player");
        
        // Create move action (W/S keys)
        moveAction = map.AddAction("Move", binding: "<Keyboard>/w");
        moveAction.AddBinding("<Keyboard>/s");
        
        // Create select action (A/D keys)
        selectAction = map.AddAction("Select", binding: "<Keyboard>/a");
        selectAction.AddBinding("<Keyboard>/d");
        
        // Set up callbacks
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        selectAction.performed += OnSelect;
        
        // Enable the actions
        map.Enable();
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        // If it's a Vector2, get the Y component (for W/S movement)
        if (context.valueType == typeof(Vector2))
        {
            Vector2 input = context.ReadValue<Vector2>();
            verticalInput = input.y;
        }
        // If it's not a Vector2, try to determine from the control
        else
        {
            if (context.control != null)
            {
                string controlName = context.control.name.ToLower();
                
                if (controlName.Contains("w"))
                {
                    verticalInput = context.performed ? 1.0f : 0.0f;
                }
                else if (controlName.Contains("s"))
                {
                    verticalInput = context.performed ? -1.0f : 0.0f;
                }
            }
        }
    }
    
    private void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        // Determine which selection key was pressed
        string controlName = context.control.name.ToLower();
        
        // Store the previous index to check if it actually changed
        int previousIndex = currentTowerIndex;
        
        // Change the selected tower based on A/D input without wrapping
        if (controlName.Contains("a"))
        {
            // Only move left if not already at the leftmost tower
            if (currentTowerIndex > 0)
            {
                currentTowerIndex--;
            }
            // If at leftmost tower and press left, do nothing
        }
        else if (controlName.Contains("d"))
        {
            // Only move right if not already at the rightmost tower
            if (currentTowerIndex < towers.Count - 1)
            {
                currentTowerIndex++;
            }
            // If at rightmost tower and press right, do nothing
        }
        
        // Only update materials if the selected tower actually changed
        if (previousIndex != currentTowerIndex)
        {
            // Reset material on the previously selected tower
            if (previousIndex >= 0 && previousIndex < towers.Count)
            {
                Transform previousTower = towers[previousIndex];
                RestoreOriginalMaterial(previousTower);
            }
            
            // Update the tower material highlight
            UpdateTowerHighlight();
        }
    }
    
    private void Update()
    {
        // Only move if we have towers and valid input
        if (towers.Count > 0 && currentTowerIndex >= 0 && currentTowerIndex < towers.Count)
        {
            Transform currentTower = towers[currentTowerIndex];
            
            // Move the selected tower forward/backward based on W/S input
            if (currentTower != null && verticalInput != 0)
            {
                // Get the current position
                Vector3 position = currentTower.position;
                
                // Calculate the new Z position
                float newZ = position.z + (verticalInput * moveSpeed * Time.deltaTime);
                
                // Clamp the Z position within the limits
                newZ = Mathf.Clamp(newZ, minZPosition, maxZPosition);
                
                // Apply the new position
                position.z = newZ;
                currentTower.position = position;
            }
        }
    }
    
    private void UpdateTowerHighlight()
    {
        if (towers.Count > 0 && currentTowerIndex >= 0 && currentTowerIndex < towers.Count)
        {
            Transform currentTower = towers[currentTowerIndex];
            
            if (currentTower != null && highlightMaterial != null)
            {
                // Apply highlight material to the selected tower
                Renderer renderer = currentTower.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = highlightMaterial;
                }
            }
        }
    }
    
    private void RestoreOriginalMaterial(Transform tower)
    {
        if (tower != null && originalMaterials.ContainsKey(tower))
        {
            Renderer renderer = tower.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterials[tower];
            }
        }
    }
    
    private void OnEnable()
    {
        // Enable input actions
        if (moveAction != null) moveAction.Enable();
        if (selectAction != null) selectAction.Enable();
    }
    
    private void OnDisable()
    {
        // Disable input actions
        if (moveAction != null) moveAction.Disable();
        if (selectAction != null) selectAction.Disable();
        
        // Restore all tower materials
        foreach (Transform tower in towers)
        {
            RestoreOriginalMaterial(tower);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up callbacks
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
        
        if (selectAction != null)
        {
            selectAction.performed -= OnSelect;
        }
        
        // Restore all tower materials
        foreach (Transform tower in towers)
        {
            RestoreOriginalMaterial(tower);
        }
    }
}