using UnityEngine;

public class Enemy : MonoBehaviour

{

    public int enemy_health = 3;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
       if(enemy_health < 1)
        {
            Destroy(gameObject);
        }
 
    }

}


