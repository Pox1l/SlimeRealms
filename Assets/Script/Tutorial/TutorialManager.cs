using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.AI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Systém a UI")]
    public TutorialSaveSystem saveSystem;
    public TextMeshProUGUI instructionTextUI;

    [Header("Hráč a Vlastní Cesta")]
    [Tooltip("Hráč se najde automaticky podle tagu 'Player'")]
    public Transform playerTransform;

    public GameObject pathPrefab;
    public float pathSpacing = 1f;
    [Tooltip("Maximální vzdálenost, na kterou se šipky vykreslí (šetří výkon)")]
    public float maxPathDistance = 15f;

    private Dictionary<GameObject, List<GameObject>> pathPools;
    private Dictionary<GameObject, int> poolUsage;

    private NavMeshPath navPath;

    [System.Serializable]
    public class TutorialRequirement
    {
        public string eventName;
        public string displayName;

        [Header("Nastavení počítadla")]
        public bool showProgressText = true;
        public int requiredCount = 1;

        [Header("Zóny a Ukazatel pro tento úkol")]
        public GameObject customPathPrefab;
        public List<Transform> targetPoints;
    }

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea] public string instructionText;
        public float hideDistance = 3f;

        [Header("Podmínky pro splnění")]
        public List<TutorialRequirement> requirements;
    }

    [Header("Kroky tutoriálu")]
    public List<TutorialStep> steps;

    private TutorialData currentData;
    private Dictionary<string, int> eventProgress = new Dictionary<string, int>();

    public bool IsCompleted => currentData == null || currentData.isCompleted || currentData.currentStepIndex >= steps.Count;

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

        pathPools = new Dictionary<GameObject, List<GameObject>>();
        poolUsage = new Dictionary<GameObject, int>();
        navPath = new NavMeshPath();
    }

    private void Start()
    {
        if (saveSystem != null) currentData = saveSystem.Load();
        else currentData = new TutorialData();

        eventProgress.Clear();
        if (currentData.savedEventNames != null)
        {
            for (int i = 0; i < currentData.savedEventNames.Count; i++)
            {
                eventProgress[currentData.savedEventNames[i]] = currentData.savedEventCounts[i];
            }
        }

        if (saveSystem != null) saveSystem.Save(currentData);

        if (!IsCompleted)
        {
            foreach (var req in steps[currentData.currentStepIndex].requirements)
            {
                if (!eventProgress.ContainsKey(req.eventName))
                {
                    eventProgress[req.eventName] = 0;
                }
            }
            ShowCurrentStep();

            if (UIManager.Instance != null && UIManager.Instance.tutorialPanelGame != null)
            {
                UIManager.Instance.tutorialPanelGame.SetActive(true);
            }
        }
        else
        {
            DeactivatePath();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideTutorialPanelGame();
            }
        }
    }

    private void Update()
    {
        if (IsCompleted) return;
        UpdateCustomPath();
    }

    private void SyncDataForSave()
    {
        if (currentData.savedEventNames == null) currentData.savedEventNames = new List<string>();
        if (currentData.savedEventCounts == null) currentData.savedEventCounts = new List<int>();

        currentData.savedEventNames.Clear();
        currentData.savedEventCounts.Clear();

        foreach (var kvp in eventProgress)
        {
            currentData.savedEventNames.Add(kvp.Key);
            currentData.savedEventCounts.Add(kvp.Value);
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (IsCompleted) return;

        TutorialStep currentStep = steps[currentData.currentStepIndex];

        bool isEventRequired = false;
        foreach (var req in currentStep.requirements)
        {
            if (req.eventName == eventName)
            {
                isEventRequired = true;
                break;
            }
        }

        if (!isEventRequired) return;

        if (eventProgress.ContainsKey(eventName))
        {
            eventProgress[eventName]++;
        }
        else
        {
            eventProgress[eventName] = 1;
        }

        SyncDataForSave();
        if (saveSystem != null) saveSystem.Save(currentData);

        UpdateInstructionText();

        bool allCompleted = true;
        foreach (var req in currentStep.requirements)
        {
            int current = eventProgress.ContainsKey(req.eventName) ? eventProgress[req.eventName] : 0;
            if (current < req.requiredCount)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            AdvanceStep();
        }
    }

    private void AdvanceStep()
    {
        currentData.currentStepIndex++;

        eventProgress.Clear();
        SyncDataForSave();

        if (saveSystem != null) saveSystem.Save(currentData);

        if (currentData.currentStepIndex < steps.Count)
        {
            foreach (var req in steps[currentData.currentStepIndex].requirements)
            {
                eventProgress[req.eventName] = 0;
            }
            ShowCurrentStep();
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void ShowCurrentStep()
    {
        UpdateInstructionText();
    }

    private void UpdateInstructionText()
    {
        TutorialStep step = steps[currentData.currentStepIndex];
        if (instructionTextUI != null)
        {
            string finalText = step.instructionText;

            if (step.requirements != null && step.requirements.Count > 0)
            {
                finalText += "\n";
                foreach (var req in step.requirements)
                {
                    int current = eventProgress.ContainsKey(req.eventName) ? eventProgress[req.eventName] : 0;
                    current = Mathf.Min(current, req.requiredCount);

                    string nameToDisplay = string.IsNullOrEmpty(req.displayName) ? req.eventName : req.displayName;

                    if (current >= req.requiredCount)
                    {
                        finalText += $"\n<color=green><s>{nameToDisplay}: DONE</s></color>";
                    }
                    else
                    {
                        if (req.showProgressText)
                        {
                            finalText += $"\n{nameToDisplay}: {current}/{req.requiredCount}";
                        }
                        else
                        {
                            finalText += $"\n{nameToDisplay}";
                        }
                    }
                }
            }

            instructionTextUI.text = finalText;
        }
    }

    private void UpdateCustomPath()
    {
        TutorialStep step = steps[currentData.currentStepIndex];

        if (playerTransform == null || step.requirements == null || pathPrefab == null)
        {
            DeactivatePath();
            return;
        }

        bool isPlayerInAnyZone = false;

        foreach (var req in step.requirements)
        {
            int current = eventProgress.ContainsKey(req.eventName) ? eventProgress[req.eventName] : 0;
            if (current >= req.requiredCount) continue;

            if (req.targetPoints == null) continue;

            foreach (Transform target in req.targetPoints)
            {
                if (target != null && Vector3.Distance(playerTransform.position, target.position) <= step.hideDistance)
                {
                    isPlayerInAnyZone = true;
                    break;
                }
            }
            if (isPlayerInAnyZone) break;
        }

        if (isPlayerInAnyZone)
        {
            DeactivatePath();
            return;
        }

        poolUsage.Clear();
        foreach (var key in pathPools.Keys)
        {
            poolUsage[key] = 0;
        }

        foreach (var req in step.requirements)
        {
            int current = eventProgress.ContainsKey(req.eventName) ? eventProgress[req.eventName] : 0;
            if (current >= req.requiredCount) continue;

            if (req.targetPoints == null) continue;

            GameObject currentPrefab = req.customPathPrefab != null ? req.customPathPrefab : pathPrefab;

            if (!pathPools.ContainsKey(currentPrefab))
            {
                pathPools[currentPrefab] = new List<GameObject>();
                poolUsage[currentPrefab] = 0;
            }

            foreach (Transform target in req.targetPoints)
            {
                if (target == null) continue;

                if (NavMesh.CalculatePath(playerTransform.position, target.position, NavMesh.AllAreas, navPath))
                {
                    float distanceToNextPrefab = pathSpacing;
                    float totalDistanceRendered = 0f;

                    for (int j = 0; j < navPath.corners.Length - 1; j++)
                    {
                        Vector3 currentCorner = navPath.corners[j];
                        Vector3 nextCorner = navPath.corners[j + 1];
                        Vector3 direction = (nextCorner - currentCorner).normalized;
                        float segmentDist = Vector3.Distance(currentCorner, nextCorner);
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                        while (segmentDist >= distanceToNextPrefab && totalDistanceRendered < maxPathDistance)
                        {
                            currentCorner += direction * distanceToNextPrefab;
                            segmentDist -= distanceToNextPrefab;
                            totalDistanceRendered += pathSpacing;

                            int usageIndex = poolUsage[currentPrefab];

                            if (usageIndex >= pathPools[currentPrefab].Count)
                            {
                                GameObject newObj = Instantiate(currentPrefab, transform);
                                newObj.SetActive(false);
                                pathPools[currentPrefab].Add(newObj);
                            }

                            GameObject objToPlace = pathPools[currentPrefab][usageIndex];
                            objToPlace.SetActive(true);
                            objToPlace.transform.position = currentCorner;
                            objToPlace.transform.rotation = Quaternion.Euler(0, 0, angle);

                            poolUsage[currentPrefab]++;
                            distanceToNextPrefab = pathSpacing;
                        }

                        if (totalDistanceRendered >= maxPathDistance)
                        {
                            break;
                        }

                        distanceToNextPrefab -= segmentDist;
                    }
                }
            }
        }

        foreach (var kvp in pathPools)
        {
            GameObject prefabKey = kvp.Key;
            List<GameObject> pool = kvp.Value;
            int usedCount = poolUsage.ContainsKey(prefabKey) ? poolUsage[prefabKey] : 0;

            for (int i = usedCount; i < pool.Count; i++)
            {
                pool[i].SetActive(false);
            }
        }
    }

    private void DeactivatePath()
    {
        if (pathPools == null) return;

        foreach (var pool in pathPools.Values)
        {
            foreach (var obj in pool)
            {
                obj.SetActive(false);
            }
        }
    }

    private void CompleteTutorial()
    {
        currentData.isCompleted = true;
        if (saveSystem != null) saveSystem.Save(currentData);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideTutorialPanelGame();
        }
        DeactivatePath();
    }

    private void OnDrawGizmosSelected()
    {
        if (steps == null) return;

        Gizmos.color = Color.yellow;

        foreach (var step in steps)
        {
            if (step.requirements != null)
            {
                foreach (var req in step.requirements)
                {
                    if (req.targetPoints != null)
                    {
                        foreach (var target in req.targetPoints)
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
    }
}