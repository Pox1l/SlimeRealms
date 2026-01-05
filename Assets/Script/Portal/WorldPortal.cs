using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldPortal : MonoBehaviour
{
    [Header("Scene")]
    public int sceneIndex;

    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject pressEHint;   

    private bool playerInRange = false;

    void Awake()
    {
        gameObject.SetActive(false);
        if (pressEHint != null)
            pressEHint.SetActive(false);
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
            SceneManager.LoadScene(sceneIndex);
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
