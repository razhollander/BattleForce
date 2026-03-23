using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class ProceduralSpring2D : MonoBehaviour
{
    [Header("Attachments")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Spring Look")]
    public float wireThickness = 0.2f;
    public float coilWidth = 1f; // How wide the zig-zags are
    public int numberOfCoils = 8;
    public int pointsPerCoil = 15; // Higher = smoother curves

    private LineRenderer lr;

    void Update()
    {
        if (startPoint == null || endPoint == null) return;
        
        if (lr == null) lr = GetComponent<LineRenderer>();
        
        // 1. Lock the thickness
        lr.startWidth = wireThickness;
        lr.endWidth = wireThickness;
        
        int totalPoints = numberOfCoils * pointsPerCoil;
        lr.positionCount = totalPoints;
        
        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        
        // Find the direction and length between the two points
        Vector3 direction = (end - start);
        Vector3 normalizedDir = direction.normalized;
        
        // Calculate the perpendicular vector (for the coil's sideways bounce)
        // This math rotates the direction vector 90 degrees in 2D
        Vector3 perpendicular = new Vector3(-normalizedDir.y, normalizedDir.x, 0);
        
        // 2. Plot the points using a Sine Wave
        for (int i = 0; i < totalPoints; i++)
        {
            float t = (float)i / (totalPoints - 1); // Goes from 0.0 to 1.0
            
            // The straight line position
            Vector3 basePosition = Vector3.Lerp(start, end, t);
            
            // The sine wave offset
            float currentAngle = t * numberOfCoils * Mathf.PI * 2f;
            Vector3 sidewaysOffset = perpendicular * Mathf.Sin(currentAngle) * coilWidth;
            
            lr.SetPosition(i, basePosition + sidewaysOffset);
        }
    }
}