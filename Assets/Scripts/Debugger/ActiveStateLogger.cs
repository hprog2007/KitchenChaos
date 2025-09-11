using UnityEngine;

public class ActiveStateLogger : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"[ActiveStateLogger] {name} Enabled", this);
        Debug.Log(UnityEngine.StackTraceUtility.ExtractStackTrace());
    }

    private void OnDisable()
    {
        
    }
}
