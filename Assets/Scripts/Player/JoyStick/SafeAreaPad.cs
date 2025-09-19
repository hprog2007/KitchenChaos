// SafeAreaPad.cs
using UnityEngine;
[ExecuteAlways]
public class SafeAreaPad : MonoBehaviour
{
    Rect last;
    void OnEnable() => Apply();
    void Update() { if (Screen.safeArea != last) Apply(); }
    void Apply()
    {
        last = Screen.safeArea;
        var rt = (RectTransform)transform;
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(32, 32); rt.offsetMax = new Vector2(-32, -32);
    }
}
