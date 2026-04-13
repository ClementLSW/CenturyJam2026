using System;
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
        StartCoroutine(LoadSceneAsync(StateManager.GameState.GAME));
    }

    private IEnumerator LoadSceneAsync(StateManager.GameState targetState)
    {
        string targetSceneName;
        switch (targetState)
        {
            case StateManager.GameState.MAINMENU:
                targetSceneName = "MainMenu";
                StateManager.Instance.SetCurrentState(StateManager.GameState.MAINMENU);
                break;
            case StateManager.GameState.GAME:
                targetSceneName = "Game";
                StateManager.Instance.SetCurrentState(StateManager.GameState.GAME);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(targetState), targetState, null);
        }
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(targetSceneName);
        while (!loadScene.isDone)
        {
            yield return null;
        }
        
    }
}
