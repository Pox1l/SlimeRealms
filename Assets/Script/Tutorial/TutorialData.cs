using System.Collections.Generic;

[System.Serializable]
public class TutorialData
{
    public int currentStepIndex = 0;
    public bool isCompleted = false;

    // Seznamy pro uložení prùbìhu (napø. zabitých nepøátel)
    public List<string> savedEventNames = new List<string>();
    public List<int> savedEventCounts = new List<int>();
}