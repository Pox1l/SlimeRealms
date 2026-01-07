using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnPortal : MonoBehaviour
{
    public int sceneIndex = 1;

    public KeyCode interactKey = KeyCode.E;
    public GameObject pressE;

    // Pøidána reference na loader
    public LevelLoader levelLoader;

    private bool playerInRange = false;

    void Start()
    {
        if (pressE != null)
            pressE.SetActive(false);

        // Hledání LoadingCanvas podle tagu (stejnì jako v pøedchozím skriptu)
        GameObject loaderObj = GameObject.FindGameObjectWithTag("LoadingCanvas");
        if (loaderObj != null)
        {
            levelLoader = loaderObj.GetComponent<LevelLoader>();
        }
        else
        {
            Debug.LogWarning("Chyba: LoadingCanvas s tagem nebyl nalezen!");
        }
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            // Pokud máme loader, použijeme ho, jinak klasický load
            if (levelLoader != null)
            {
                levelLoader.LoadLevel(sceneIndex);
            }
            else
            {
                SceneManager.LoadScene(sceneIndex);
            }
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