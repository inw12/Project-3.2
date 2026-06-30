using UnityEngine;
public class CombatCamera : CameraState
{
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private Transform enemyPosition;
    [SerializeField] private Vector3 cameraRotation;

    [Header("Camera Settings")]
    [Tooltip("Closest the camera is allowed to get to the midpoint of the two targets")]
    [SerializeField] private float minDistance = 5f;
    [Tooltip("Farthest the camera is allowed to get from the midpoint of the two targets")]
    [SerializeField] private float maxDistance = 20f;
    [Tooltip("Extra world-space breathing room added around the targets before fitting to frame")]
    [SerializeField] private float padding;
    [Tooltip("Downward look angle in degrees (0 = level, 90 = straight down)")]
    [SerializeField] private float pitchAngle = 35f;
    [Tooltip("Horizontal approach direction in degrees (0 = camera sits behind -Z)")]
    [SerializeField] private float yawAngle = 0f;
    [Tooltip("Extra height added on top of the pitch-derived height")]
    [SerializeField] private float heightOffset = 0f;

    /// * Desired Behavior:
    ///     - Find midpoint between player and enemy
    ///     - Raycast from the camera outward
    ///     - move the camera towards the midpoint
    public override Vector3 GetTargetPosition()
    {
        // Get midpoint between Player & Enemy
        var playerPos = Player.Instance.transform.position;
        var enemyPos = enemyPosition.position;
        var midpoint = (playerPos + enemyPos) / 2f;

        // Get distance between Player & Enemy
        var distance = Vector3.Distance(playerPos, enemyPos);

        // Get 
        var camDistance = CalculateRequiredDistance(distance + padding);
        camDistance = Mathf.Clamp(camDistance, minDistance, maxDistance);

        Quaternion approachRotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        Vector3 desiredOffset = approachRotation * (Vector3.back * camDistance);
        Vector3 desiredPosition = midpoint + desiredOffset + Vector3.up * heightOffset;

        return desiredPosition;
    }

    public override Quaternion GetTargetRotation() => Quaternion.Euler(cameraRotation);

    /// <summary>
    /// Computes the camera distance needed so an object of the given world-space
    /// width fits inside both the horizontal and vertical field of view.
    /// </summary>
    private float CalculateRequiredDistance(float width)
    {
        float verticalFOVRad = mainCamera.fieldOfView * Mathf.Deg2Rad;
        float horizontalFOVRad = 2f * Mathf.Atan(Mathf.Tan(verticalFOVRad * 0.5f) * mainCamera.aspect);

        float distanceForHorizontal = (width * 0.5f) / Mathf.Tan(horizontalFOVRad * 0.5f);
        float distanceForVertical = (width * 0.5f) / Mathf.Tan(verticalFOVRad * 0.5f);

        // The tighter (larger) of the two constraints wins so nothing clips out of frame.
        return Mathf.Max(distanceForHorizontal, distanceForVertical);
    }
}