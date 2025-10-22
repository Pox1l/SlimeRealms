using UnityEngine;

public class CrystalInteraction : MonoBehaviour
{
    public GameObject crystalUI;
    private CrystalUIController crystalController;
    private bool isPlayerNearby = false;

    void Start()
    {
        crystalController = crystalUI.GetComponent<CrystalUIController>();
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ToggleUI(true);
        }
    }

    private void ToggleUI(bool show)
    {
        crystalUI.SetActive(show);
        Time.timeScale = show ? 0f : 1f;

        if (show && crystalController != null)
        {
            
            crystalController.mainPanel.SetActive(true);
            crystalController.UpdateRequirements();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            //ToggleUI(false);
        }
    }
}
