using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader {
    
    public enum Scene {
        Level_Selection_Map,
        L1_Classic_City_Scene,
        LoadingScene
    }
    
    private static Scene targetScene;

    public static void Load(Scene targetScene) {
        Time.timeScale = 1f;

        SceneLoader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());

    }

    public static void LoaderCallback() {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
