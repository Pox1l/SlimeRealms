using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public float speed = 2f;
    public float scaleAmount = 0.1f;
    Vector3 baseScale;

    void Start() => baseScale = transform.localScale;

    void Update()
    {
        float pulse = Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = baseScale + new Vector3(pulse, pulse, 0);
    }
}