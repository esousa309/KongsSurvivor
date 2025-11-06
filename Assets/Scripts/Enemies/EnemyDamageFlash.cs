
// Assets/Scripts/Enemies/EnemyDamageFlash.cs
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyDamageFlash : MonoBehaviour
{
    public Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);
    public float flashTime = 0.07f;

    private Health health;
    private SpriteRenderer sr;
    private Color original;
    private float lastHp = -1f;
    private float timer;

    void Awake()
    {
        health = GetComponent<Health>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) original = sr.color;
    }

    void Update()
    {
        if (health == null || sr == null) return;

        if (lastHp < 0f) lastHp = health.current;

        if (health.current < lastHp)
        {
            // took damage
            sr.color = flashColor;
            timer = flashTime;
            lastHp = health.current;
        }

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) sr.color = original;
        }
    }

    void OnDestroy()
    {
        if (sr != null) sr.color = original;
    }
}
