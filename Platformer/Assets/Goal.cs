using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{

    public string level = "";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneManager.LoadScene(level);
    }

}
