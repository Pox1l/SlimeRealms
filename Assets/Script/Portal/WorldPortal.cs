using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldPortal : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Jméno scény, do které portál teleportuje (musí být v Build Settings).")]
    public int sceneName;

    [Header("Ovládání")]
    public KeyCode interactKey = KeyCode.F;
    public GameObject pressFHint;   // volitelné: UI text/ikonka "F"

    private bool playerInRange = false;

    void Awake()
    {
        // portál je ve scénì, ale na zaèátku schovaný
        gameObject.SetActive(false);

        if (pressFHint != null)
            pressFHint.SetActive(false);
    }

    /// <summary>
    /// Zavolej z krystalu, když má být portál odemknutý.
    /// </summary>
    public void EnablePortal()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            
                SceneManager.LoadScene(sceneName);
           
            
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        if (pressFHint != null)
            pressFHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (pressFHint != null)
            pressFHint.SetActive(false);
    }
}
