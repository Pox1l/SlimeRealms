using UnityEngine;

public class AttackPointRotator : MonoBehaviour
{
    public Transform player;   // reference na hráèe (støed rotace)
    public Camera cam;          // kamera (pro pøepoèet myši)
    public float distance = 0.5f; // vzdálenost od hráèe

    void Update()
    {
        if (player == null) return;

        var cameraToUse = cam != null ? cam : Camera.main;
        if (cameraToUse == null) return;

        // Pøepoèet pozice myši na svìt
        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // Vektor od hráèe k myši
        Vector2 dir = (mouseWorld - player.position).normalized;

        // Nastavení pozice RangePointu
        transform.position = (Vector2)player.position + dir * distance;

        // Natoèení RangePointu podle smìru
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
