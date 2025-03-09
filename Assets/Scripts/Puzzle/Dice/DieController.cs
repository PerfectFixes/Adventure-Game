using UnityEngine;

public class DieController : MonoBehaviour
{
    [Header("Die Settings")]
    [Tooltip("Current face value (1-6)")]
    [SerializeField] private int currentValue = 1;
    
    [Tooltip("Animation speed for rotation")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("Color of this die")]
    [SerializeField] private Color dieColor = Color.white;
    
    // Rotation targets for each face value (1-6)
    private readonly Quaternion[] faceRotations = new Quaternion[6];
    
    // Outline component reference
    private MonoBehaviour outlineComponent;
    
    // Target rotation when animating
    private Quaternion targetRotation;
    private bool isRotating = false;
    
    public int CurrentValue => currentValue;
    public Color DieColor => dieColor;
    
    private void Awake()
    {
        // Initialize face rotations (each rotation represents a face value 1-6)
        InitializeFaceRotations();
        
        // Set the die to its initial value
        targetRotation = faceRotations[currentValue - 1];
        transform.rotation = targetRotation;
        
        // Try to find or add outline component
        outlineComponent = FindOutlineComponent(gameObject);
        if (outlineComponent != null)
        {
            EnableOutline(false);
        }
    }
    
    private void Update()
    {
        // Handle rotation animation
        if (isRotating)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Check if rotation is complete
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }
    
    // Set the die to a specific value (1-6)
    public void SetValue(int value)
    {
        value = Mathf.Clamp(value, 1, 6);
        
        if (currentValue != value)
        {
            currentValue = value;
            targetRotation = faceRotations[currentValue - 1];
            isRotating = true;
            
            Debug.Log($"Die {gameObject.name} value changed to: {currentValue}");
        }
    }
    
    // Increment the die value (wraps from 6 to 1)
    public void IncrementValue()
    {
        int newValue = currentValue + 1;
        if (newValue > 6) newValue = 1;
        SetValue(newValue);
    }
    
    // Decrement the die value (wraps from 1 to 6)
    public void DecrementValue()
    {
        int newValue = currentValue - 1;
        if (newValue < 1) newValue = 6;
        SetValue(newValue);
    }
    
    // Set outline visibility
    public void SetSelected(bool selected, Color outlineColor, float outlineWidth)
    {
        if (outlineComponent != null)
        {
            SetOutlineProperties(outlineComponent, outlineColor, outlineWidth);
            EnableOutline(selected);
        }
    }
    
    private void InitializeFaceRotations()
    {
        // These rotations correspond to the die faces 1-6
        // You may need to adjust these based on your die model's orientation
        faceRotations[0] = Quaternion.Euler(0, 0, 0);      // 1
        faceRotations[1] = Quaternion.Euler(0, 0, 90);     // 2
        faceRotations[2] = Quaternion.Euler(90, 0, 0);     // 3
        faceRotations[3] = Quaternion.Euler(-90, 0, 0);    // 4
        faceRotations[4] = Quaternion.Euler(0, 0, -90);    // 5
        faceRotations[5] = Quaternion.Euler(180, 0, 0);    // 6
    }
    
    // Helper method to find any Outline component (reused from your TowerController script)
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
    
    // Helper method to set outline properties (reused from your TowerController script)
    private void SetOutlineProperties(MonoBehaviour outline, Color color, float width)
    {
        try
        {
            // Use reflection to set properties
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
    private void EnableOutline(bool enabled)
    {
        if (outlineComponent != null)
        {
            outlineComponent.enabled = enabled;
        }
    }
}