using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AI; // PŘIDÁNO: Knihovna pro NavMesh

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Systém a UI")]
    public TutorialSaveSystem saveSystem;
    public GameObject tutorialPanel;
    public TextMeshProUGUI instructionTextUI;

    [Header("Hráč a Vlastní Cesta")]
    [Tooltip("Hráč se najde automaticky podle tagu 'Player'")]
    public Transform playerTransform;

    public GameObject pathPrefab;
    public float pathSpacing = 1f;
    private List<GameObject> pathPool;

    private NavMeshPath navPath; // PŘIDÁNO: Proměnná pro uchování vypočítané cesty

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea] public string instructionText;

        [Header("Zóny")]
        public List<Transform> targetPoints;
        public float hideDistance = 3f;

        [Header("Podmínky pro splnění")]
        public string requiredEventName;
        public int requiredEventCount = 1;
    }

    [Header("Kroky tutoriálu")]
    public List<TutorialStep> steps;

    private TutorialData currentData;
    private int currentEventProgress = 0;

    private void Awake()
    {
        Instance = this;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Objekt s tagem 'Player' nebyl v této scéně nalezen!");
        }

        pathPool = new List<GameObject>();
        navPath = new NavMeshPath(); // PŘIDÁNO: Inicializace NavMeshPath
    }

    private void Start()
    {
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        if (!currentData.isCompleted && steps.Count > 0)
        {
            ShowCurrentStep();
        }
        else
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            DeactivatePath();
        }
    }

    private void Update()
    {
        if (currentData.isCompleted || currentData.currentStepIndex >= steps.Count) return;

        UpdateCustomPath();
    }

    public void TriggerEvent(string eventName)
    {
        if (currentData.isCompleted || currentData.currentStepIndex >= steps.Count) return;

        TutorialStep currentStep = steps[currentData.currentStepIndex];

        if (currentStep.requiredEventName == eventName)
        {
            currentEventProgress++;
            UpdateInstructionText();

            if (currentEventProgress >= currentStep.requiredEventCount)
            {
                AdvanceStep();
            }
        }
    }

    private void AdvanceStep()
    {
        currentEventProgress = 0;
        currentData.currentStepIndex++;

        if (saveSystem != null) saveSystem.Save(currentData);

        if (currentData.currentStepIndex < steps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void ShowCurrentStep()
    {
        tutorialPanel.SetActive(true);
        UpdateInstructionText();
    }

    private void UpdateInstructionText()
    {
        TutorialStep step = steps[currentData.currentStepIndex];
        if (instructionTextUI != null)
        {
            if (step.requiredEventCount > 1)
            {
                instructionTextUI.text = $"{step.instructionText} ({currentEventProgress}/{step.requiredEventCount})";
            }
            else
            {
                instructionTextUI.text = step.instructionText;
            }
        }
    }

    private void UpdateCustomPath()
    {
        TutorialStep step = steps[currentData.currentStepIndex];

        if (playerTransform == null || step.targetPoints == null || step.targetPoints.Count == 0 || pathPrefab == null)
        {
            DeactivatePath();
            return;
        }

        bool isPlayerInAnyZone = false;
        foreach (Transform target in step.targetPoints)
        {
            if (target != null && Vector3.Distance(playerTransform.position, target.position) <= step.hideDistance)
            {
                isPlayerInAnyZone = true;
                break;
            }
        }

        if (isPlayerInAnyZone)
        {
            DeactivatePath();
            return;
        }

        int poolIndex = 0;
        foreach (Transform target in step.targetPoints)
        {
            if (target == null) continue;

            // ZMĚNA: Výpočet cesty pomocí NavMesh
            if (NavMesh.CalculatePath(playerTransform.position, target.position, NavMesh.AllAreas, navPath))
            {
                float distanceToNextPrefab = pathSpacing;

                // ZMĚNA: Průchod jednotlivými úseky NavMesh cesty
                for (int j = 0; j < navPath.corners.Length - 1; j++)
                {
                    Vector3 currentCorner = navPath.corners[j];
                    Vector3 nextCorner = navPath.corners[j + 1];
                    Vector3 direction = (nextCorner - currentCorner).normalized;
                    float segmentDist = Vector3.Distance(currentCorner, nextCorner);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    while (segmentDist >= distanceToNextPrefab)
                    {
                        currentCorner += direction * distanceToNextPrefab;
                        segmentDist -= distanceToNextPrefab;

                        if (poolIndex >= pathPool.Count)
                        {
                            GameObject newObj = Instantiate(pathPrefab, transform);
                            newObj.SetActive(false);
                            pathPool.Add(newObj);
                        }

                        pathPool[poolIndex].SetActive(true);
                        pathPool[poolIndex].transform.position = currentCorner;
                        pathPool[poolIndex].transform.rotation = Quaternion.Euler(0, 0, angle);
                        poolIndex++;

                        distanceToNextPrefab = pathSpacing;
                    }
                    distanceToNextPrefab -= segmentDist; // Zbytek vzdálenosti do dalšího úseku
                }
            }
        }

        for (int i = poolIndex; i < pathPool.Count; i++)
        {
            pathPool[i].SetActive(false);
        }
    }

    private void DeactivatePath()
    {
        foreach (var obj in pathPool)
        {
            obj.SetActive(false);
        }
    }

    private void CompleteTutorial()
    {
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        DeactivatePath();
    }

    private void OnDrawGizmosSelected()
    {
        if (steps == null) return;

        Gizmos.color = Color.yellow;

        foreach (var step in steps)
        {
            if (step.targetPoints != null)
            {
                foreach (var target in step.targetPoints)
                {
                    if (target != null)
                    {
                        Gizmos.DrawWireSphere(target.position, step.hideDistance);
                    }
                }
            }
        }
    }
}