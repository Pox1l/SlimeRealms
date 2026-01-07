using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldPortal : MonoBehaviour
{
    [Header("Scene")]
    public int sceneIndex;

    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject pressEHint;

    [Header("Loading System")]
    // Teï se to vyplní samo, ale nechávám public pro kontrolu
    public LevelLoader levelLoader;

    private bool playerInRange = false;

    void Awake()
    {
        gameObject.SetActive(false);
        if (pressEHint != null)
            pressEHint.SetActive(false);

        // --- Hledání podle tagu ---
        GameObject loaderObj = GameObject.FindGameObjectWithTag("LoadingCanvas");
        if (loaderObj != null)
        {
            levelLoader = loaderObj.GetComponent<LevelLoader>();
        }
        else
        {
            Debug.LogError("Chyba: Ve scénì nebyl nalezen objekt s tagem 'LoadingCanvas'!");
        }
        // --------------------------
    }

    public void EnablePortal()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (Input.GetKeyDown(interactKey))
        {
            if (levelLoader != null)
            {
                levelLoader.LoadLevel(sceneIndex);
            }
            else
            {
                // Záloha, pokud se loading screen nenašel
                Debug.LogWarning("LevelLoader chybí, naèítám napøímo.");
                SceneManager.LoadScene(sceneIndex);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (pressEHint != null)
            pressEHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (pressEHint != null)
            pressEHint.SetActive(false);
    }
}