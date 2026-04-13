using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        StartCoroutine(LoadSceneAsync("Game"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);
        while (!loadScene.isDone)
        {
            yield return null;
        }
        
    }
}
