using UnityEngine;

public class SceneConfig : MonoBehaviour
{
    public static SceneConfig Instance { get; private set; }

    [SerializeField] private SceneConfigSO sceneConfigSO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
