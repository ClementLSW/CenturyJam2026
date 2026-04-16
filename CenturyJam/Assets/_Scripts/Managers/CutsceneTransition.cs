using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneTransition : MonoBehaviour
{
    [SerializeField] private VideoPlayer cutscenePlayer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SceneManager.LoadScene("MainMenu");
        if (cutscenePlayer.frame >= (long)(cutscenePlayer.frameCount - 1))
        {
            Debug.Log("Video has finished playing!");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
