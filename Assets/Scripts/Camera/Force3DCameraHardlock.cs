using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Enforces a 3D perspective camera even if other systems flip it back to Orthographic.
/// Also disables typical 2D camera drivers (PixelPerfect, Cinemachine) via reflection,
/// so it compiles fine whether those packages are installed or not.
/// </summary>
[DefaultExecutionOrder(10000)] // run after most scripts
[RequireComponent(typeof(Camera))]
public class Force3DCameraHardlock : MonoBehaviour
{
    [Tooltip("Field of View to enforce for the main 3D camera.")]
    public float fieldOfView = 60f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Enforce3D();
        Disable2DDriversOnCamera(gameObject);
        DisableGlobal2DDrivers();
    }

    void OnEnable()
    {
        Enforce3D();
    }

    void LateUpdate()
    {
        Enforce3D();
    }

    void OnPreCull()
    {
        Enforce3D();
    }

    void Enforce3D()
    {
        if (!cam) return;
        if (cam.orthographic) cam.orthographic = false;
        if (Mathf.Abs(cam.fieldOfView - fieldOfView) > 0.01f) cam.fieldOfView = fieldOfView;

        // Reasonable transform in case someone zeroed it out
        if (cam.transform.position == Vector3.zero)
        {
            cam.transform.position = new Vector3(0f, 6f, -8f);
            cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        }
    }

    void Disable2DDriversOnCamera(GameObject go)
    {
        // Disable any Behaviour whose type name hints it's a 2D camera driver.
        var behaviours = go.GetComponentsInChildren<Behaviour>(true);
        foreach (var b in behaviours)
        {
            if (!b) continue;
            string tn = b.GetType().FullName ?? b.GetType().Name;
            // Common 2D/virtual-camera drivers:
            if (tn.Contains("PixelPerfect", StringComparison.OrdinalIgnoreCase) ||
                tn.Contains("Cinemachine", StringComparison.OrdinalIgnoreCase))
            {
                b.enabled = false;
            }
        }
    }

    void DisableGlobal2DDrivers()
    {
        // If there's a Cinemachine brain on the main camera, disable it.
        var brain = FindBehaviourOn(cam ? cam.gameObject : null, "CinemachineBrain");
        if (brain != null) brain.enabled = false;

        // If there are any virtual cameras alive, disable them so they can't fight CameraFollow.
        var allBehaviours = Resources.FindObjectsOfTypeAll<Behaviour>();
        foreach (var b in allBehaviours)
        {
            if (!b) continue;
            string tn = b.GetType().Name;
            if (tn.Equals("CinemachineVirtualCamera", StringComparison.OrdinalIgnoreCase) ||
                tn.Equals("CinemachineFreeLook", StringComparison.OrdinalIgnoreCase))
            {
                b.enabled = false;
            }
        }
    }

    Behaviour FindBehaviourOn(GameObject go, string typeName)
    {
        if (!go) return null;
        var comps = go.GetComponents<Behaviour>();
        return comps.FirstOrDefault(c => string.Equals(c.GetType().Name, typeName, StringComparison.OrdinalIgnoreCase));
    }
}
