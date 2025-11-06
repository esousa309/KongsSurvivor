using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileVisibilityBooster : MonoBehaviour
{
    public float trailTime  = 0.6f;
    public float trailWidth = 0.7f;

    void Awake()
    {
        var tr = GetComponent<TrailRenderer>();
        if (tr == null) tr = gameObject.AddComponent<TrailRenderer>();

        tr.time = trailTime;
        tr.minVertexDistance = 0.015f;
        tr.numCapVertices = 6;
        tr.numCornerVertices = 3;

        var curve = new AnimationCurve(
            new Keyframe(0f, trailWidth),
            new Keyframe(1f, trailWidth * 0.25f)
        );
        tr.widthCurve = curve;

        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.35f, 0.65f, 1f), 0f),
                new GradientColorKey(new Color(0.95f, 0.98f, 1f), 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        tr.colorGradient = g;

        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        tr.receiveShadows = false;
        tr.alignment = LineAlignment.View;
        tr.textureMode = LineTextureMode.Stretch;
    }
}
