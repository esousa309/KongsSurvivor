using UnityEngine;

/// <summary>
/// Hardens the scene against any script that flips the Main Camera to 2D.
/// Keeps the camera in Perspective with a sane FOV every frame.
/// </summary>
[DefaultExecutionOrder(9999)] // run after almost everything else
public class Force3DCameraGuard : MonoBehaviour
{
    [Tooltip("Field of View for perspective camera.")]
    public float fieldOfView = 60f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // If any system set this to orthographic, force it back.
        if (cam.orthographic)
            cam.orthographic = false;

        // Keep FOV sane (some 2D scripts zero this out)
        if (Mathf.Abs(cam.fieldOfView - fieldOfView) > 0.01f)
            cam.fieldOfView = fieldOfView;
    }
}
