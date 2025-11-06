using UnityEngine;

[DisallowMultipleComponent]
public class OrbAutoClean : MonoBehaviour
{
    public float maxLifetime   = 20f;
    public float idleSpeed     = 0.04f;
    public float nudgeSpeed    = 5.5f;
    public float collectRadius = 1.0f;

    float age;
    Transform player;

    void Update()
    {
        age += Time.deltaTime;
        if (age > maxLifetime) { Destroy(gameObject); return; }

        if (!player)
        {
            var p = GameObject.FindWithTag("Player") ?? GameObject.Find("Kong") ?? GameObject.Find("Player");
            if (p) player = p.transform;
        }
        if (!player) return;

        // collect near player
        if ((player.position - transform.position).sqrMagnitude <= collectRadius * collectRadius)
        {
            Destroy(gameObject);
            return;
        }

        // nudge if idle
        float speed = 0f;
        var rb = GetComponent<Rigidbody>(); if (rb) speed = rb.velocity.magnitude;
        if (speed < idleSpeed)
        {
            Vector3 dir = player.position - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                transform.position += dir.normalized * (nudgeSpeed * Time.deltaTime);
            }
        }
    }
}
