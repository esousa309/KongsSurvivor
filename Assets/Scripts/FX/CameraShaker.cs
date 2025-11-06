
// Assets/Scripts/FX/CameraShaker.cs
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance;

    public float decay = 1.5f;

    float timeLeft;
    float strength;
    Vector3 originalPos;
    Transform camT;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        var cam = Camera.main;
        if (cam == null) return;
        camT = cam.transform;
        originalPos = camT.localPosition;
    }

    void LateUpdate()
    {
        if (camT == null)
        {
            var cam = Camera.main;
            if (cam == null) return;
            camT = cam.transform;
            originalPos = camT.localPosition;
        }

        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime * decay;
            camT.localPosition = originalPos + (Vector3)(Random.insideUnitCircle * strength * timeLeft);
            if (timeLeft <= 0f) camT.localPosition = originalPos;
        }
    }

    public static void Shake(float duration, float power)
    {
        if (Instance == null) new GameObject("CameraShaker", typeof(CameraShaker));
        Instance.timeLeft = Mathf.Max(Instance.timeLeft, duration);
        Instance.strength = Mathf.Max(Instance.strength, power);
    }
}
