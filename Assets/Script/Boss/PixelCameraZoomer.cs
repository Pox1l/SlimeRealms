using UnityEngine;
using UnityEngine.U2D;
using System.Collections;

public class PixelCameraZoomer : MonoBehaviour
{
    // Už není [RequireComponent], protože skript může být jinde než kamera
    [HideInInspector]
    public PixelPerfectCamera pixelCam;

    public Vector2Int normalRes = new Vector2Int(320, 180);
    public Vector2Int combatRes = new Vector2Int(480, 270);
    public float zoomDuration = 2.0f;

    private Coroutine zoomRoutine;

    void Awake()
    {
        // 👇 Hledá objekt s tagem "MainCamera"
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");

        if (camObj != null)
        {
            pixelCam = camObj.GetComponent<PixelPerfectCamera>();
        }
        else
        {
            Debug.LogError("PixelCameraZoomer: Nenašel jsem objekt s tagem 'MainCamera'!");
        }
    }

    public void ZoomToCombat()
    {
        if (pixelCam == null) return;
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(AnimateRes(pixelCam.refResolutionX, pixelCam.refResolutionY, combatRes));
    }

    public void ZoomToNormal()
    {
        if (pixelCam == null) return;
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        zoomRoutine = StartCoroutine(AnimateRes(pixelCam.refResolutionX, pixelCam.refResolutionY, normalRes));
    }

    IEnumerator AnimateRes(int startX, int startY, Vector2Int target)
    {
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            pixelCam.refResolutionX = (int)Mathf.Lerp(startX, target.x, t);
            pixelCam.refResolutionY = (int)Mathf.Lerp(startY, target.y, t);
            yield return null;
        }
        pixelCam.refResolutionX = target.x;
        pixelCam.refResolutionY = target.y;
    }
}