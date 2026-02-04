using UnityEngine;
using FMODUnity;  // Nutné pro FMOD
using FMOD.Studio; // Nutné pro práci s Instancemi

public class CrystalFloat : MonoBehaviour
{
    [Header("Nastavení vznášení")]
    public float speed = 2f;
    public float amplitude = 0.2f;

    [Header("Audio (FMOD)")]
    public EventReference humSound; // Smyèka jemného bzuèení krystalu

    private Vector3 startPos;
    private EventInstance humInstance;

    void Start()
    {
        startPos = transform.position;

        // Vytvoøení instance zvuku pro smyèku
        if (!humSound.IsNull)
        {
            humInstance = RuntimeManager.CreateInstance(humSound);
            // Pøipnutí zvuku ke krystalu pro 3D efekt
            RuntimeManager.AttachInstanceToGameObject(humInstance, gameObject);
            humInstance.start();
        }
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnDisable()
    {
        // Zastavení zvuku pøi vypnutí krystalu
        if (humInstance.isValid())
        {
            humInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void OnDestroy()
    {
        // Úklid pamìti
        if (humInstance.isValid())
        {
            humInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            humInstance.release();
        }
    }
}