using UnityEngine;

public class CrystalFloat : MonoBehaviour
{
    [Header("Nastavení vznášení")]
    public float speed = 2f;      
    public float amplitude = 0.2f; 

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;

        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}