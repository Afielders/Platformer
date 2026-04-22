using UnityEngine;
using System.Collections;

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
    private bool attack_check;
    private RaycastHit2D right_attack_check;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

       
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


        
    }

    private void FixedUpdate()
    {

        //Check if the user pressed the attack key.
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            attack_check = true;
        }
        else
        {
            attack_check = false;
        }

        //Speed the player up by adding acceleration.
        velocity.x += acceleration * direction.x;
        //Cap the player's speed by clamping its x component of the velocity.
        velocity.x = Mathf.Clamp(velocity.x, -max_speed, max_speed);

        //If the player is not holding down the A/D key...
        if(direction.x == 0)
        {
            //Apply friction by moving the veocity.x to zero.
            velocity.x = Mathf.MoveTowards(velocity.x, 0, friction);
        }


        bool is_grounded = false;
        if (velocity.y <0) //If we are falling...
        {
            //Preform the ground raycasts.
            left_ground_check = Physics2D.Raycast(rb.position - new Vector2(0.5f, 0), Vector2.down, 1, mask);
            right_ground_check = Physics2D.Raycast(rb.position + new Vector2(0.5f, 0), Vector2.down, 1, mask);
            is_grounded = (left_ground_check || right_ground_check);
        }

        float ground_offset = 0f;
        if(is_grounded)  //if the player is touching the ground...
        {
            
            //Find the amount to offset the player by.
            ground_offset = Mathf.Max(left_ground_check.distance, right_ground_check.distance) - 0.5f;

            //Disable any y velocity.
            velocity.y = 0;

            //Make the player jump since they pressed the spacebar.
            if(jump_check)
            {
                velocity.y = jump_force;
                is_grounded = false;
            }
            //Make the player attack since they pressed the Enter.
            if (attack_check)
            {
                
               
                // Do attack raycast hit detection
                right_attack_check = Physics2D.Raycast(rb.position + new Vector2(0, 0), Vector2.right, 1, attackmask);
                Debug.Log("Atttack True");
                if (right_attack_check)
                {
                    Debug.Log("Attack Hit");

                    right_attack_check.collider.gameObject.GetComponent<Enemy>().enemy_health -= 1;
                    Debug.Log(right_attack_check.collider.gameObject.GetComponent<Enemy>().enemy_health);



                }

            }


        }
        else //Otherwise the player is in the air. So...
        {
            //Apply gravity.
            velocity.y -= gravity;
            //Cap the fall speed.
            velocity.y = Mathf.Max(velocity.y, -max_fall_speed);  
        }

      

        //Move the player.
        rb.MovePosition(rb.position + (velocity * Time.fixedDeltaTime) - new Vector2(0, ground_offset));
    }

    private void OnDrawGizmos()
    {
        //Visualize the ground check ray.   
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position + new Vector2(0.5f, 0), Vector2.down);//Right Ray
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position - new Vector2(0.5f, 0), Vector2.down);//Left Ray

        if(attack_check)
        {
            //Visualize the attack check ray.   
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(GetComponent<Rigidbody2D>().position + new Vector2(0, 0), Vector2.right);//Right Ray
        }
        
    }
   

    

}
