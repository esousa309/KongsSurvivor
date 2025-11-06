using UnityEngine;

public class TopDownBirdsEye : MonoBehaviour
{
    [Tooltip("What the camera should follow (player).")]
    public Transform target;

    [Tooltip("Height above the target (world units).")]
    public float height = 24f;

    [Tooltip("How quickly the camera catches up to the target.")]
    public float smoothTime = 0.12f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
        {
            var go = GameObject.FindWithTag("Player") ?? GameObject.Find("Kong") ?? GameObject.Find("Player");
            if (go != null) target = go.transform;
        }
        if (target == null) return;

        // Position straight above the target
        Vector3 desired = new Vector3(target.position.x, height, target.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        // Look straight down for a true bird’s-eye view
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
