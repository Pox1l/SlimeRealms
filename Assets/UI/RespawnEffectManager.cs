using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RespawnEffectManager : MonoBehaviour
{
    /*[Header("UI Effect")]
    public Image irisImage; // Odkaz na ten Image s materiálem IrisWipe
    public float transitionDuration = 2.0f;

    [Header("Other Effects")]
    public ParticleSystem respawnParticles;

    [SerializeField]
    private int radiusID;

    void Awake()
    {
        radiusID = Shader.PropertyToID("_Radius");
    }

    void Start()
    {
        // 1. POJISTKA: Hned při startu hry ten obrázek vypneme, 
        // aby nestrašil na obrazovce, kdyby zůstal zapnutý v Inspectoru.
        if (irisImage != null)
        {
            irisImage.gameObject.SetActive(false);
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerRespawned += PlayRespawnEffects;
        }
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnPlayerRespawned -= PlayRespawnEffects;
        }
    }

    void PlayRespawnEffects()
    {
        if (respawnParticles != null) respawnParticles.Play();

        if (irisImage != null)
        {
            // Tady se spustí animace
            StartCoroutine(IrisWipeRoutine());
        }
    }

    IEnumerator IrisWipeRoutine()
    {
        // 2. AKTIVACE: Teď efekt potřebujeme, tak ho zapneme
        irisImage.gameObject.SetActive(true);

        Material mat = irisImage.material;

        // Resetujeme kruh na "zavřeno" (černá tma)
        mat.SetFloat(radiusID, 0f);

        // Krátká pauza ve tmě
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;

        // Animace zvětšování kruhu
        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / transitionDuration;

            // Plynulý přechod z 0 (tma) na 1 (průhledno)
            float currentRadius = Mathf.SmoothStep(0f, 1.0f, progress);
            mat.SetFloat(radiusID, currentRadius);

            yield return null;
        }

        // Ujistíme se, že je to úplně otevřené
        mat.SetFloat(radiusID, 1.5f);

        // 3. DEAKTIVACE: Efekt skončil, obrázek vypneme, aby nezral výkon
        irisImage.gameObject.SetActive(false);
    }*/
}