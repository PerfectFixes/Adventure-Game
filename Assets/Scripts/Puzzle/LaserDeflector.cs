using UnityEngine;

public class LaserDeflector : MonoBehaviour
{
    [Header("Deflection Settings")]
    [Tooltip("The angle to deflect the laser in degrees (from the tower's forward direction)")]
    [Range(-180, 180)]
    public float deflectionAngle = -45f;
    
    [Header("Visual Settings")]
    [Tooltip("Material to apply to the laser after deflection")]
    public Material laserMaterial;
    
    // Store original colors for gizmos
    private Color gizmoIncomingColor = Color.yellow;
    private Color gizmoOutgoingColor = Color.green;

    // Called by Unity when this component is selected in editor
    private void OnDrawGizmosSelected()
    {
        // Draw a visual representation of the entry and exit rays
        Gizmos.color = gizmoIncomingColor;
        // Incoming ray (assuming from left side)
        Vector3 incoming = Vector3.left;
        Gizmos.DrawRay(transform.position, incoming.normalized * 2);
        
        // Outgoing ray based on deflection angle
        Gizmos.color = laserMaterial != null ? gizmoOutgoingColor : Color.white;
        Vector3 outgoing = GetExitDirection();
        Gizmos.DrawRay(transform.position, outgoing.normalized * 2);
    }
    
    /// <summary>
    /// Returns the exit direction of the laser based on the deflection angle
    /// </summary>
    /// <returns>The direction the laser should exit</returns>
    public Vector3 GetExitDirection()
    {
        // Create a rotation around the up vector based on the deflection angle
        // This gives us a direction relative to the tower's orientation
        Quaternion rotation = Quaternion.Euler(0, deflectionAngle, 0);
        
        // Apply this rotation to the forward direction of the tower
        // This means at 0 degrees, the laser continues straight forward
        Vector3 exitDirection = rotation * transform.forward;
        
        return exitDirection.normalized;
    }
    
    /// <summary>
    /// Public method to set the deflection angle, useful for inspectors/editors
    /// </summary>
    /// <param name="newAngle">The new deflection angle in degrees</param>
    public void SetDeflectionAngle(float newAngle)
    {
        deflectionAngle = Mathf.Clamp(newAngle, -180f, 180f);
    }
    
    /// <summary>
    /// Returns the material to be applied to the laser after it hits this deflector
    /// </summary>
    /// <returns>The material to apply, or null if no material is assigned</returns>
    public Material GetLaserMaterial()
    {
        return laserMaterial;
    }
}