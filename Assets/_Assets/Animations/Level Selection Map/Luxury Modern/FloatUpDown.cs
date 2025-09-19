using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    public float amplitude = 10f;
    public float speed = 1f;

    RectTransform rectTransform;
    Vector3 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        rectTransform.anchoredPosition = startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amplitude;
    }
}
