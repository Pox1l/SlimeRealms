using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnPortal : MonoBehaviour
{
    
    public int sceneIndex = 2;

    public KeyCode interactKey = KeyCode.E;
    public GameObject pressE;  

    private bool playerInRange = false;

    void Start()
    {
        if (pressE != null)
            pressE.SetActive(false);
    }

    void Update()
    {
        
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            
            SceneManager.LoadScene(sceneIndex);
        }
    }

   
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (pressE != null)
                pressE.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pressE != null)
                pressE.SetActive(false);
        }
    }
}