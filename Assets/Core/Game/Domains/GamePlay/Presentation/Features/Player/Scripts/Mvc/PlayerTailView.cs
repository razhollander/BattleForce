using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

public class PlayerTailView : MonoBehaviour
{
    private static readonly int spiralPropertyID = Shader.PropertyToID("_SpiralAmount"); 
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Tail Physics")]
    [Tooltip("How strongly the tail reacts to rotation. Higher = more bend.")]
    public float bendSensitivity = 0.005f;
    
    [Tooltip("Maximum allowed bend to prevent the spiral from breaking or clipping.")]
    public float maxBend = 1.5f;
    
    [Tooltip("How fast the tail physically snaps to the new bend or returns to straight.")]
    public float tailFlexibility = 10f;
    
    private float previousRotationZ;
    private float currentBend = 0f;

    void Update()
    {
        // 1. Get the current rotation on the Z axis
        float currentRotationZ = transform.eulerAngles.z;

        // 2. Calculate the difference (delta) in rotation since last frame.
        // We use Mathf.DeltaAngle because it safely handles the jump between 360 and 0 degrees.
        float deltaRotation = Mathf.DeltaAngle(previousRotationZ, currentRotationZ);

        // 3. Calculate rotational speed (degrees per second). 
        // Dividing by deltaTime ensures the bend is consistent regardless of frame rate.
        float angularVelocity = deltaRotation / Time.deltaTime;

        // 4. Determine the target bend based on how fast we are turning.
        // We multiply by negative sensitivity so the tail lags *behind* the rotation.
        float targetBend = angularVelocity * -bendSensitivity; 

        // 5. Clamp the target so the tail doesn't curl into a tight, broken circle.
        targetBend = Mathf.Clamp(targetBend, -maxBend, maxBend);

        // 6. Smoothly transition the current bend towards the target bend.
        // If targetBend is 0 (player stopped), this smoothly pulls currentBend back to 0.
        currentBend = Mathf.Lerp(currentBend, targetBend, Time.deltaTime * tailFlexibility);
        LogService.LogError(currentBend.ToString());
        // 7. Send the final calculated bend to the shader.
        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetFloat(spiralPropertyID, -currentBend);
        }

        // 8. Store the current rotation to compare against in the next frame.
        previousRotationZ = currentRotationZ;
    }

    public void InitEntryPoint()
    {
        // Cache the shader property ID for performance
      
        
        // Record our starting rotation
        previousRotationZ = transform.eulerAngles.z;
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
}