using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    public float amplitude = 10f;
    public float speed = 1f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amplitude;
    }
}
