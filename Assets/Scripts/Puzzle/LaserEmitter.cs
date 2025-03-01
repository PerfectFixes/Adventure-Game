using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [Header("Laser Settings")]
    [Tooltip("Maximum distance the laser can travel")]
    public float maxDistance = 100f;
    
    [Tooltip("The layer mask to determine what the laser can hit")]
    public LayerMask layerMask;
    
    [Tooltip("The width of the laser beam")]
    public float laserWidth = 0.1f;
    
    [Tooltip("The color of the laser beam")]
    public Color laserColor = Color.red;
    
    [Tooltip("Maximum number of deflections allowed")]
    public int maxDeflections = 10;
    
    [Header("Emission Control")]
    [Tooltip("Master switch to enable/disable the laser")]
    public bool isEnabled = true;
    
    [Tooltip("Controls the emission mode: On=Continuous laser, Off=Pulsing laser")]
    public bool isContinuous = false;
    
    [Header("Timing Settings")]
    [Tooltip("Time between laser activations in seconds (only used in pulsing mode)")]
    public float cycleTime = 5.0f;
    
    [Tooltip("Duration the laser stays active in seconds (only used in pulsing mode)")]
    public float activeTime = 0.5f;
    
    [Tooltip("Time before the laser starts shrinking (in seconds)")]
    public float shrinkDelay = 0.1f;
    
    [Header("Visualization")]
    [Tooltip("Prefab to use for laser segments (cube recommended)")]
    public GameObject laserSegmentPrefab;
    
    [Tooltip("Material to apply to the laser prefabs")]
    public Material laserMaterial;
    
    [Tooltip("ScriptableObject for advanced laser material settings (optional)")]
    public LaserMaterial laserMaterialSettings;
    
    // Runtime variables
    private List<Vector3> hitPoints = new List<Vector3>();
    private List<GameObject> laserSegments = new List<GameObject>();
    private Transform laserParent; // Parent object for all laser segments
    private float timer = 0f;
    private float activeTimer = 0f; // Tracks how long the laser has been active
    private bool isLaserActive = false;
    
    // Puzzle tracking variables
    private List<LaserDeflector> hitDeflectors = new List<LaserDeflector>();
    private int totalDeflectorsInScene = 0;
    
    private void Awake()
    {
        // Create a parent object for all laser segments
        GameObject parentObject = new GameObject("Laser_" + gameObject.name);
        laserParent = parentObject.transform;
        // Make it a child of this emitter for organization
        laserParent.parent = transform;
        
        // Count the total number of deflectors in the scene
        totalDeflectorsInScene = FindObjectsOfType<LaserDeflector>().Length;
        Debug.Log("Found " + totalDeflectorsInScene + " deflectors in the scene.");
    }
    
    private void Start()
    {
        // Initialize the laser state based on settings
        if (isEnabled && isContinuous)
        {
            isLaserActive = true;
            UpdateLaserPath();
        }
        else
        {
            isLaserActive = false;
            DeactivateLaser();
        }
    }
    
    private void Update()
    {
        // Master switch: if disabled, ensure laser is off
        if (!isEnabled)
        {
            if (isLaserActive)
            {
                DeactivateLaser();
                isLaserActive = false;
            }
            return;
        }
        
        // Continuous mode: keep the laser always active
        if (isContinuous)
        {
            if (!isLaserActive)
            {
                isLaserActive = true;
                UpdateLaserPath();
            }
            else
            {
                // Keep updating the path (for moving objects)
                UpdateLaserPath();
            }
            return;
        }
        
        // Pulsing mode: handle laser timing cycle
        timer += Time.deltaTime;
        
        // Check if we need to toggle the laser state
        if (isLaserActive && timer >= activeTime)
        {
            // Turn off the laser
            DeactivateLaser();
            isLaserActive = false;
            activeTimer = 0f; // Reset the active timer
        }
        else if (!isLaserActive && timer >= cycleTime)
        {
            // Reset the timer and turn on the laser
            timer = 0f;
            activeTimer = 0f; // Reset the active timer when laser activates
            isLaserActive = true;
            UpdateLaserPath();
        }
        
        // If laser is active, update its state
        if (isLaserActive)
        {
            // Track how long the laser has been active
            activeTimer += Time.deltaTime;
            
            // Update the laser path to show the shrinking effect
            UpdateLaserPath();
        }
    }
   
    /// <summary>
    /// Deactivates the laser by removing all segments
    /// </summary>
    private void DeactivateLaser()
    {
        // Destroy all laser segments
        foreach (GameObject segment in laserSegments)
        {
            Destroy(segment);
        }
        laserSegments.Clear();
        hitPoints.Clear();
        hitDeflectors.Clear(); // Clear the list of hit deflectors when laser turns off
    }
    
    /// <summary>
    /// Manually activate the laser (useful for triggering from other scripts)
    /// </summary>
    public void ActivateLaser()
    {
        if (!isEnabled) return;
        
        isLaserActive = true;
        timer = 0f;
        activeTimer = 0f; // Reset the active timer
        UpdateLaserPath();
    }
    
    /// <summary>
    /// Manually deactivate the laser (useful for triggering from other scripts)
    /// </summary>
    public void ForceDeactivateLaser()
    {
        isLaserActive = false;
        DeactivateLaser();
    }
    
    /// <summary>
    /// Calculate and visualize the laser path
    /// </summary>
    private void UpdateLaserPath()
    {
        // Clear previous hit points and tracking data
        hitPoints.Clear();
        hitDeflectors.Clear();
        
        // Clean up previous laser segments
        foreach (GameObject segment in laserSegments)
        {
            Destroy(segment);
        }
        laserSegments.Clear();
        
        // If the laser is not active, don't create new segments
        if (!isLaserActive)
        {
            return;
        }
        
        // Start position is at the cylinder's position
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = transform.forward;
        
        // Add initial position
        hitPoints.Add(currentPosition);
        
        // Track deflections
        int deflectionCount = 0;
        
        // Cast the laser and handle deflections
        while (deflectionCount < maxDeflections)
        {
            // Cast a ray from current position in current direction
            if (Physics.Raycast(currentPosition, currentDirection, out RaycastHit hit, maxDistance, layerMask))
            {
                // Add hit position to the list
                hitPoints.Add(hit.point);
                
                // Check if we hit a deflector
                LaserDeflector deflector = hit.collider.GetComponent<LaserDeflector>();
                if (deflector != null)
                {
                    // Add this deflector to our hit list if not already included
                    if (!hitDeflectors.Contains(deflector))
                    {
                        hitDeflectors.Add(deflector);
                    }
                    
                    // Get the exit direction from the deflector
                    Vector3 exitDirection = deflector.GetExitDirection();
                    
                    // We want the laser to visibly change direction at the center of the tower
                    Vector3 towerCenter = deflector.transform.position;
                    
                    // Add the center point to make the laser path go through the center of the tower
                    hitPoints.Add(towerCenter);
                    
                    // Find the far side of the collider in the exit direction
                    Ray exitRay = new Ray(towerCenter, exitDirection);
                    Vector3 exitPoint;
                    
                    if (hit.collider.Raycast(exitRay, out RaycastHit exitHit, 100f))
                    {
                        // Use the actual exit point from the raycast
                        exitPoint = exitHit.point;
                    }
                    else
                    {
                        // If we can't calculate the exit point, use an approximation
                        exitPoint = towerCenter + exitDirection * (hit.collider.bounds.extents.magnitude);
                    }
                    
                    // Add the exit point
                    hitPoints.Add(exitPoint);
                    
                    // Update for next segment
                    currentPosition = exitPoint + exitDirection * 0.01f; // Small offset to avoid re-hitting
                    currentDirection = exitDirection;
                    
                    // Increment deflection count
                    deflectionCount++;
                }
                else
                {
                    // Check if we hit a laser receiver
                    LaserReceiver receiver = hit.collider.GetComponent<LaserReceiver>();
                    if (receiver != null)
                    {
                        // Notify the receiver that it's been hit, passing our hit deflectors list
                        receiver.ReceiveLaser(hitDeflectors, totalDeflectorsInScene);
                    }
                    
                    // If we hit something that's not a deflector, we're done
                    break;
                }
            }
            else
            {
                // Nothing hit - add the end position of the ray
                hitPoints.Add(currentPosition + currentDirection * maxDistance);
                break;
            }
        }
        
        // Create laser segments from the hit points
        CreateLaserSegments();
    }
    
    /// <summary>
    /// Create visual laser segments between all hit points
    /// </summary>
    private void CreateLaserSegments()
    {
        // Create a segment between each pair of points
        for (int i = 0; i < hitPoints.Count - 1; i++)
        {
            Vector3 startPoint = hitPoints[i];
            Vector3 endPoint = hitPoints[i + 1];
            
            // Create a new segment
            GameObject segment = CreateLaserSegment(startPoint, endPoint);
            laserSegments.Add(segment);
        }
    }
    
    /// <summary>
    /// Calculate the current width of the laser based on how long it has been active
    /// </summary>
    /// <returns>The current width to use for the laser</returns>
    private float CalculateCurrentLaserWidth()
    {
        // If we haven't reached the shrink delay, return full width
        if (activeTimer <= shrinkDelay)
        {
            return laserWidth;
        }
        
        // Calculate how far through the shrinking process we are
        float shrinkDuration = activeTime - shrinkDelay;
        float shrinkProgress = (activeTimer - shrinkDelay) / shrinkDuration;
        
        // Clamp to ensure we don't go below 0
        shrinkProgress = Mathf.Clamp01(shrinkProgress);
        
        // Linear interpolation from full width to 0
        return Mathf.Lerp(laserWidth, 0f, shrinkProgress);
    }
    
    /// <summary>
    /// Create a single laser segment between two points
    /// </summary>
    private GameObject CreateLaserSegment(Vector3 start, Vector3 end)
    {
        GameObject segment;
        
        if (laserSegmentPrefab != null)
        {
            // Instantiate the prefab
            segment = Instantiate(laserSegmentPrefab);
            
            // Apply material if provided
            if (laserMaterial != null)
            {
                Renderer renderer = segment.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Apply the standard material and color
                    renderer.material = laserMaterial;
                    renderer.material.color = laserColor;
                    
                    // If we also have the laser material settings, apply those
                    if (laserMaterialSettings != null)
                    {
                        renderer.material = laserMaterialSettings.GetMaterial();
                    }
                }
            }
        }
        else
        {
            // Create a default cube if no prefab is provided
            segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.GetComponent<Renderer>().material.color = laserColor;
        }
        
        // Give the segment a descriptive name
        segment.name = "LaserSegment_" + laserSegments.Count;
        
        // Make the segment a child of our laser parent for organization
        segment.transform.parent = laserParent;
        
        // Position and scale the segment to form a laser beam
        Vector3 midPoint = (start + end) / 2f;
        segment.transform.position = midPoint;
        
        // Calculate direction and look at the end point
        Vector3 direction = (end - start).normalized;
        segment.transform.forward = direction;
        
        // Calculate the current width based on the shrinking effect
        float currentWidth = CalculateCurrentLaserWidth();
        
        // Scale the segment with the calculated width
        float length = Vector3.Distance(start, end);
        segment.transform.localScale = new Vector3(currentWidth, currentWidth, length);
        
        // Disable collision on the segment
        Collider collider = segment.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        return segment;
    }
}