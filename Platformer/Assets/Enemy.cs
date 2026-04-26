using System.Net.NetworkInformation;
using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour

{
    // Enemy stats
    public float speed = 2f;
    public int enemy_health = 3;
    public int damage = 1;



    // Death variables
    private bool dying = false;
    public float deathFlashDuration = 0.5f;
    public float deathFlashInterval = 0.08f;
    public float deathFreezeDelay = 0.12f; // how long the last-hit knockback is allowed to move
    public LayerMask groundMask;
    public float groundRayLength = 0.7f;
    public Vector2 groundRayOffset = new Vector2(0.3f, 0f);
    public float maxFallWait = 1.0f; 


    // Patrol movement variables
    public LayerMask wallMask;
    public float wallCheckDistance = 0.2f;
    public Vector2 wallCheckOffset = new Vector2(0.4f, 0f);
    private Rigidbody2D rb;
    private int dir = 1;

    // Knockback variables
    public float knockbackX = 6f;
    public float knockbackY = 2f;
    public float knockbackDuration = 0.15f;
    private float knockbackTimer = 0f;
    private int knockbackDir = 0; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
           
    }

    // Update is called once per frame
    void Update()
    {
        
      
 



    }

    void FixedUpdate()
    {
        // Don't do anything if we're in the middle of dying
        if (rb == null) return;
        if (dying) return;

        // Handle Knockback
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(knockbackDir * knockbackX, rb.linearVelocity.y);
            return; // skip normal movement while in knockback
        }

        // Patrol movement
        if (rb == null) return;

        // Wall check (raycast forward)
        Vector2 origin = rb.position + new Vector2(wallCheckOffset.x * dir, wallCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, wallCheckDistance, wallMask);

        if (hit.collider != null)
        {
            Debug.Log("Hit wall: " + hit.collider.name);

            dir *= -1; // flip direction

            // Nudge away from wall 
            rb.position += Vector2.right * dir * 0.05f;
        }

        // Move after deciding direction 
        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
    }

    void OnDrawGizmos()
    {
        // visualize wall ray
        Gizmos.color = Color.yellow;

        int drawDir = 1; 
        Vector3 origin = transform.position + (Vector3)wallCheckOffset * drawDir;
        Gizmos.DrawRay(origin, Vector3.right * wallCheckDistance * drawDir);
    }

    public void TakeHitFrom(Vector2 attackerPos, int damage)
    {
        if (dying) return;

        // Apply knockback direction immediately
        knockbackDir = (transform.position.x < attackerPos.x) ? -1 : 1;
        knockbackTimer = knockbackDuration;

        if (rb != null)
            rb.linearVelocity = new Vector2(knockbackDir * knockbackX, knockbackY);

        // Now apply damage
        enemy_health -= damage;

        if (enemy_health <= 0)
        {
            StartCoroutine(DeathRoutine()); // will freeze later, not instantly
        }
    }

    private bool IsGrounded()
    {
        Vector2 pos = rb.position;

        var left = Physics2D.Raycast(pos - new Vector2(groundRayOffset.x, 0f), Vector2.down, groundRayLength, groundMask);
        var right = Physics2D.Raycast(pos + new Vector2(groundRayOffset.x, 0f), Vector2.down, groundRayLength, groundMask);

        return left.collider != null || right.collider != null;
    }

    private IEnumerator DeathRoutine()
    {
        dying = true;

        // Let last-hit knockback play for a moment
        yield return new WaitForSeconds(deathFreezeDelay);

        // Let it fall until it touches ground (or timeout)
        float waitTime = 0f;
        while (rb != null && !IsGrounded() && waitTime < maxFallWait)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // Now freeze + flash
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (var c in GetComponentsInChildren<Collider2D>())
            if (c.isTrigger) c.enabled = false;

        // Flash
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;

            float flashTime = 0f;
            bool on = false;

            while (flashTime < deathFlashDuration)
            {
                on = !on;
                sr.color = on ? Color.white : Color.red;
                yield return new WaitForSeconds(deathFlashInterval);
                flashTime += deathFlashInterval;
            }

            sr.color = original;
        }

        Destroy(gameObject);
    }
}


