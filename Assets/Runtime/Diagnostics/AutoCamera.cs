using UnityEngine;

public class AutoCamera : MonoBehaviour
{
    void Awake()
    {
        if (Camera.main == null)
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0, 0, -10);
        }
        Time.timeScale = 1f; // ensure normal speed
    }
}