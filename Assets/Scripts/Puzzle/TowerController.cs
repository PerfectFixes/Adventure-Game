using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerController : MonoBehaviour
{
    [Header("Tower Settings")]
    [Tooltip("List of all towers/deflectors that can be controlled")]
    public List<Transform> towers = new List<Transform>();
    
    [Tooltip("Movement speed of the towers")]
    public float moveSpeed = 1.0f;
    
    [Tooltip("Minimum Z position of towers")]
    public float minZPosition = -0.5f;
    
    [Tooltip("Maximum Z position of towers")]
    public float maxZPosition = 0.5f;
    
    [Header("Outline Settings")]
    [Tooltip("Outline color for the selected tower")]
    public Color outlineColor = Color.yellow;
    
    [Tooltip("Outline width for the selected tower")]
    [Range(0f, 10f)]
    public float outlineWidth = 5f;
    
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
    
    // Store monobehaviours for each tower that handle outlines
    private Dictionary<Transform, MonoBehaviour> towerOutlines = new Dictionary<Transform, MonoBehaviour>();
    
    private void Awake()
    {
        // Set all towers to starting position and set up outlines
        foreach (Transform tower in towers)
        {
            if (tower != null)
            {
                // Set starting Z position
                Vector3 startPosition = tower.position;
                startPosition.z = minZPosition;
                tower.position = startPosition;
                
                // Try to find an Outline component (from Quick Outline package)
                MonoBehaviour outlineComponent = FindOutlineComponent(tower.gameObject);
                
                if (outlineComponent != null)
                {
                    // Store the outline component
                    towerOutlines[tower] = outlineComponent;
                    
                    // Configure and disable initially
                    SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
                    EnableOutline(outlineComponent, false);
                }
                else
                {
                    Debug.LogWarning("No Outline component found on " + tower.name + ". Make sure you've added the Quick Outline component to this object.");
                }
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
            UpdateTowerOutline();
        }
    }
    
    // Helper method to find any Outline component on the given game object
    private MonoBehaviour FindOutlineComponent(GameObject obj)
    {
        // Try to find a component named "Outline" using GetComponents
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component.GetType().Name == "Outline")
            {
                return component;
            }
        }
        
        // If no Outline component found, try to add one
        // Note: This will only work if Quick Outline is properly imported
        try
        {
            // Use reflection to create the component without direct reference
            System.Type outlineType = System.Type.GetType("Outline, Assembly-CSharp");
            if (outlineType != null)
            {
                return obj.AddComponent(outlineType) as MonoBehaviour;
            }
            else
            {
                // Try alternative namespace
                outlineType = System.Type.GetType("QuickOutline.Outline, Assembly-CSharp");
                if (outlineType != null)
                {
                    return obj.AddComponent(outlineType) as MonoBehaviour;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error adding Outline component: " + e.Message);
        }
        
        return null;
    }
    
    // Helper method to set outline properties using reflection (works with any Quick Outline implementation)
    private void SetOutlineProperties(MonoBehaviour outline, Color color, float width)
    {
        try
        {
            // Use reflection to set properties without direct reference
            System.Reflection.PropertyInfo colorProperty = outline.GetType().GetProperty("OutlineColor");
            System.Reflection.PropertyInfo widthProperty = outline.GetType().GetProperty("OutlineWidth");
            
            if (colorProperty != null)
            {
                colorProperty.SetValue(outline, color);
            }
            
            if (widthProperty != null)
            {
                widthProperty.SetValue(outline, width);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error setting outline properties: " + e.Message);
        }
    }
    
    // Helper method to enable/disable the outline
    private void EnableOutline(MonoBehaviour outline, bool enabled)
    {
        if (outline != null)
        {
            outline.enabled = enabled;
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
        
        // Only update outlines if the selected tower actually changed
        if (previousIndex != currentTowerIndex)
        {
            // Disable outline on the previously selected tower
            if (previousIndex >= 0 && previousIndex < towers.Count)
            {
                Transform previousTower = towers[previousIndex];
                DisableTowerOutline(previousTower);
            }
            
            // Update the tower outline
            UpdateTowerOutline();
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
    
    private void UpdateTowerOutline()
    {
        if (towers.Count > 0 && currentTowerIndex >= 0 && currentTowerIndex < towers.Count)
        {
            Transform currentTower = towers[currentTowerIndex];
            
            if (currentTower != null && towerOutlines.ContainsKey(currentTower))
            {
                // Get the outline component
                MonoBehaviour outline = towerOutlines[currentTower];
                
                // Update outline properties in case they were changed in the Inspector
                SetOutlineProperties(outline, outlineColor, outlineWidth);
                
                // Enable the outline
                EnableOutline(outline, true);
            }
        }
    }
    
    private void DisableTowerOutline(Transform tower)
    {
        if (tower != null && towerOutlines.ContainsKey(tower))
        {
            // Disable the outline component
            MonoBehaviour outline = towerOutlines[tower];
            EnableOutline(outline, false);
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
        
        // Disable all outlines
        foreach (Transform tower in towers)
        {
            DisableTowerOutline(tower);
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
        
        // Disable all outlines
        foreach (Transform tower in towers)
        {
            DisableTowerOutline(tower);
        }
    }
}