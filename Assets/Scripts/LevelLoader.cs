using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator sceneTransition;

    public void GoToStart()
    {
        StartCoroutine(LoadLevel(0, 1f));
    }

    public void GoToPrep()
    {
        StartCoroutine(LoadLevel(1, 1f));
    }

    public void GoToGame()
    {
        StartCoroutine(LoadLevel(2, 1f));
    }

    public void GoToBoss()
    {
        StartCoroutine(LoadLevel(3, 1f));
    }

    public void GoToDeath()
    {
        StartCoroutine(LoadLevel(4, 1f));
    }

    public void GoToVictory()
    {
        StartCoroutine(LoadLevel(5, 1f));
    }

    IEnumerator LoadLevel(int idx, float seconds)
    {
        GameManager.ResumeGame();
        sceneTransition.SetTrigger("Start");
        yield return new WaitForSecondsRealtime(seconds);
        SceneManager.LoadScene(idx);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
