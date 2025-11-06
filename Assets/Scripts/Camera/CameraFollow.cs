using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 20f, -20f);
    public float smoothTime = 0.15f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
        {
            var playerGO = GameObject.FindWithTag("Player") ?? GameObject.Find("Kong") ?? GameObject.Find("Player");
            if (playerGO != null) target = playerGO.transform;
        }

        if (!target) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target);
    }
}
