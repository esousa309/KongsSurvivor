
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 5f;
    public float lifetime = 4f;

    private Vector2 moveDir = Vector2.right;
    private Rigidbody2D rb;

    public void Launch(Vector2 dir, float newSpeed, float newDamage)
    {
        if (dir.sqrMagnitude < 0.0001f) dir = moveDir;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        moveDir = dir.normalized;

        speed = Mathf.Max(0f, newSpeed);
        damage = newDamage;

        var ang = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;
        rb.velocity = moveDir * speed;

        CancelInvoke(nameof(Die));
        Invoke(nameof(Die), Mathf.Max(0.05f, lifetime));
    }

    void Update()
    {
        if (rb == null)
        {
            transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) return;
        if (col.GetComponent<XpOrb>() != null) return;

        var h = col.GetComponent<Health>();
        if (h == null) h = col.GetComponentInParent<Health>();
        if (h != null)
        {
            h.Damage(damage);
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
