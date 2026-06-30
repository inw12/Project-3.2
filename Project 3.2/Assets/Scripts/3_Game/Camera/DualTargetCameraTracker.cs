using UnityEngine;

/// <summary>
/// Tracks the midpoint between two targets (e.g. Player and Enemy) and dynamically
/// adjusts camera distance so both remain in frame, clamped between min/max distance.
/// Attach this directly to your Camera GameObject.
/// </summary>
[RequireComponent(typeof(Camera))]
public class DualTargetCameraTracker : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform enemyPos; // e.g. Player
    private Transform playerPos; // e.g. Enemy

    [Header("Distance Clamping")]
    [Tooltip("Closest the camera is allowed to get to the midpoint of the two targets")]
    [SerializeField] private float minDistance = 5f;
    [Tooltip("Farthest the camera is allowed to get from the midpoint of the two targets")]
    [SerializeField] private float maxDistance = 20f;

    [Header("Framing")]
    [Tooltip("Extra world-space breathing room added around the targets before fitting to frame")]
    [SerializeField] private float padding = 2f;
    [Tooltip("Downward look angle in degrees (0 = level, 90 = straight down)")]
    [SerializeField] private float pitchAngle = 35f;
    [Tooltip("Horizontal approach direction in degrees (0 = camera sits behind -Z)")]
    [SerializeField] private float yawAngle = 0f;
    [Tooltip("Extra height added on top of the pitch-derived height")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.25f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [Header("Optional Scene Bounds (clamps final camera position)")]
    [SerializeField] private bool useWorldBounds = false;
    [SerializeField] private Vector3 worldBoundsMin;
    [SerializeField] private Vector3 worldBoundsMax;

    private Camera cam;
    private Vector3 currentVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        playerPos = Player.Instance.transform;
        if (enemyPos == null || playerPos == null) return;

        Vector3 midpoint = (enemyPos.position + playerPos.position) * 0.5f;
        float targetSeparation = Vector3.Distance(enemyPos.position, playerPos.position);

        // How far back the camera needs to sit to fit both targets in frame, with padding.
        float requiredCamDistance = CalculateRequiredDistance(targetSeparation + padding);
        requiredCamDistance = Mathf.Clamp(requiredCamDistance, minDistance, maxDistance);

        Quaternion approachRotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        Vector3 desiredOffset = approachRotation * (Vector3.back * requiredCamDistance);
        Vector3 desiredPosition = midpoint + desiredOffset + Vector3.up * heightOffset;

        if (useWorldBounds)
        {
            desiredPosition = ClampToWorldBounds(desiredPosition);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);

        Quaternion desiredRotation = Quaternion.LookRotation(
            (midpoint - transform.position).normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Computes the camera distance needed so an object of the given world-space
    /// width fits inside both the horizontal and vertical field of view.
    /// </summary>
    private float CalculateRequiredDistance(float requiredWidth)
    {
        float verticalFOVRad = cam.fieldOfView * Mathf.Deg2Rad;
        float horizontalFOVRad = 2f * Mathf.Atan(Mathf.Tan(verticalFOVRad * 0.5f) * cam.aspect);

        float distanceForHorizontal = (requiredWidth * 0.5f) / Mathf.Tan(horizontalFOVRad * 0.5f);
        float distanceForVertical = (requiredWidth * 0.5f) / Mathf.Tan(verticalFOVRad * 0.5f);

        // The tighter (larger) of the two constraints wins so nothing clips out of frame.
        return Mathf.Max(distanceForHorizontal, distanceForVertical);
    }

    private Vector3 ClampToWorldBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, worldBoundsMin.x, worldBoundsMax.x);
        pos.y = Mathf.Clamp(pos.y, worldBoundsMin.y, worldBoundsMax.y);
        pos.z = Mathf.Clamp(pos.z, worldBoundsMin.z, worldBoundsMax.z);
        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyPos == null || playerPos == null) return;

        Vector3 midpoint = (enemyPos.position + playerPos.position) * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(midpoint, 0.3f);
        Gizmos.DrawLine(enemyPos.position, playerPos.position);

        if (useWorldBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = (worldBoundsMin + worldBoundsMax) * 0.5f;
            Vector3 size = worldBoundsMax - worldBoundsMin;
            Gizmos.DrawWireCube(center, size);
        }
    }
}