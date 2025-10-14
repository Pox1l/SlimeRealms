using UnityEngine;
using UnityEngine.Profiling;
using System.Diagnostics;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI debugText;

    private float deltaTime = 0.0f;
    private Process currentProcess;

    void Start()
    {
        currentProcess = Process.GetCurrentProcess();
    }

    void Update()
    {
        // FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // CPU %
        float cpuUsage = 0f;
        try
        {
            cpuUsage = (float)currentProcess.TotalProcessorTime.TotalMilliseconds /
                       (Time.realtimeSinceStartup * 1000f);
            cpuUsage *= 100f / System.Environment.ProcessorCount;
        }
        catch { }

        // RAM
        float memoryMB = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);

        // Active objects
        int objects = FindObjectsOfType<GameObject>().Length;

        // Vypiš do UI
        debugText.text =
            $"FPS: {fps:0.} \n" +
            $"CPU: {cpuUsage:0.0}% \n" +
            $"RAM: {memoryMB:0.0} MB \n" +
            $"GameObjects: {objects}";
    }
}
