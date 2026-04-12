using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{

    //The direction the user wants to move in.
    private Vector2 direction = Vector2.zero;

    private Rigidbody2D rb;

    public float max_speed = 5f;
    private Vector2 velocity;
    public float acceleration = 1.5f;
    public float fritction = 0.75f;

    public float gravity = 0.5f;
    public float max_fall_speed = 9.8f;

    public LayerMask mask;
    private RaycastHit2D left_ground_check;
    private RaycastHit2D right_ground_check;




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
    }

    private void FixedUpdate()
    {

        //Accelerate the player.
        velocity.x += acceleration * direction.x;
        //Cap the speed to max_speed.
        velocity.x = Mathf.Clamp(velocity.x, -max_speed, max_speed);

        //Friction
        if(direction.x == 0)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, fritction);
        }

        //Apply gravity to velocity.
        velocity.y -= gravity;

        //Preform the raycasts.
        left_ground_check = Physics2D.Raycast(rb.position, Vector2.down, 1, mask);
        right_ground_check = Physics2D.Raycast(rb.position, Vector2.down, 1, mask);

        float ground_offset = 0f;

        //if the player is touching the gorund...
        if(left_ground_check || right_ground_check)
        {
            //Disable velocity.
            velocity.y = 0;

            //Find the amount to offset the player by.
            ground_offset = Mathf.Max(left_ground_check.distance, right_ground_check.distance) - 0.5f;
        }
        else //Otherwise the player is in the air. So...
        {
            //Apply gravity to velocity.
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
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position + new Vector2(-0.5f, 0), Vector2.down);
        Gizmos.DrawRay(GetComponent<Rigidbody2D>().position + new Vector2(0.5f, 0), Vector2.down);

    }
   

    

}
