using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    //The direction the user wants to move in.
    private Vector2 direction = Vector2.zero;

    private Rigidbody2D rb;

    public float max_speed = 5f;

    private Vector2 velocity;

    public float acceleration = 1.5f;

    public float friction = 0.75f;

    public float gravity = 0.5f;

    public float max_fall_speed = 9.8f;

    //Ground Checking
    public LayerMask mask;
    private RaycastHit2D left_ground_check;
    private RaycastHit2D right_ground_check;

    //Jumping
    private bool jump_check;
    public float jump_force = 15f;

    //Attacking
    public LayerMask attackmask;
    private RaycastHit2D attack_check;
    private bool attackPressed;
    private int facing = 1; // 1 right, -1 left

    //Player Health
    public int health = 5;
    private Color player_color;

    // Knockback
    public float knockbackX = 8f;
    public float knockbackY = 6f;
    public float knockbackDuration = 0.15f;
    private float knockbackTimer = 0f;
    private int knockbackDir = 0; // -1 left, +1 right

    //Player Death behavior
    public string level1 = "Level1";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //Store the color of the player.
        player_color = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        //Get the user provided input
        //Horizontal
        if (Input.GetKey(KeyCode.D))
        {
            direction.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            direction.x = -1;
        }
        else
        {
            direction.x = 0;
        }

        //Determine which way player is facing based on horizontal input. 
        if (direction.x > 0) facing = 1;
        else if (direction.x < 0) facing = -1;

        // Buffer attack 
        if (Input.GetKeyDown(KeyCode.R))
            attackPressed = true;


        //Normalize the directuin so the player doesn't move quicker along the diaginals.
        direction = direction.normalized;

        //Check if the user pressed the jump key.
        if (Input.GetKey(KeyCode.Space))
        {
            jump_check = true;
        }
        else
        {
            jump_check = false;
        }

        //Player Death

        if(health < 1)
        {
            SceneManager.LoadScene(level1);
        }

        

    }

    private void FixedUpdate()
{
    // Horizontal movement if not in knockback
    if (knockbackTimer > 0f)
    {
        knockbackTimer -= Time.fixedDeltaTime;
    }
    else
    {
        velocity.x += acceleration * direction.x;
        velocity.x = Mathf.Clamp(velocity.x, -max_speed, max_speed);

        if (direction.x == 0)
            velocity.x = Mathf.MoveTowards(velocity.x, 0, friction);
    }

    // Ground check 
    left_ground_check = Physics2D.Raycast(rb.position - new Vector2(0.5f, 0), Vector2.down, 1, mask);
    right_ground_check = Physics2D.Raycast(rb.position + new Vector2(0.5f, 0), Vector2.down, 1, mask);
        bool is_grounded = (left_ground_check.collider != null && left_ground_check.distance <= 0.55f) || (right_ground_check.collider != null && right_ground_check.distance <= 0.55f);
        float ground_offset = 0f;

    if (is_grounded)
    {
            float leftDist = left_ground_check.collider ? left_ground_check.distance : 0f;
            float rightDist = right_ground_check.collider ? right_ground_check.distance : 0f;

            ground_offset = Mathf.Max(leftDist, rightDist) - 0.5f;

            if (velocity.y < 0f && knockbackTimer <= 0f)
                velocity.y = 0f;

            if (jump_check && knockbackTimer <= 0f)
            {
                velocity.y = jump_force;
                is_grounded = false;
                ground_offset = 0f; 
            }

    if (attackPressed)
     {
        attackPressed = false;
        attack_check = Physics2D.Raycast(rb.position, Vector2.right * facing, 1, attackmask);
            

        if (attack_check)
         { var enemy = attack_check.collider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeHitFrom(transform.position, 1);
                    }
            }
        }
    }
    else
    {
        velocity.y -= gravity;
        velocity.y = Mathf.Max(velocity.y, -max_fall_speed);
    }

    // Movement
    rb.MovePosition(rb.position + (velocity * Time.fixedDeltaTime) - new Vector2(0, ground_offset));
}
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.collider.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        health -= enemy.damage;

        GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine(ColorNormal());

        // Determine which side the enemy is on
        knockbackDir = (transform.position.x < enemy.transform.position.x) ? -1 : 1;
        knockbackTimer = knockbackDuration;

        // Inject knockback into velocity 
        velocity.x = knockbackDir * knockbackX;
        velocity.y = knockbackY;
    }

   

    IEnumerator ColorNormal()
    {
        //Wait 0.25 seconds.
        yield return new WaitForSeconds(0.25f);

        //Change back to the original color.
        GetComponent<SpriteRenderer>().color = player_color;
    }



    private void OnDrawGizmos()
    {
        //Visualize the ground check ray.   
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position + new Vector2(0.5f, 0), Vector2.down);//Right Ray
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position - new Vector2(0.5f, 0), Vector2.down);//Left Ray

        if(attackPressed)
        {
            //Visualize the attack check ray.   
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rb.position, Vector2.right * facing);
        }
        
    }
   

    

}
