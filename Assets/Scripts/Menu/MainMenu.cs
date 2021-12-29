using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private float _timeToLoadOrExitGame;

    public void LoadGame()
    {
        StartCoroutine(LoadWithDelay(1));
    }

    public void QuitGame()
    {
        StartCoroutine(ExitWithDelay());
    }

    private IEnumerator LoadWithDelay(int scene)
    {
        yield return new WaitForSeconds(_timeToLoadOrExitGame);
        SceneManager.LoadScene(scene);
    }

    private IEnumerator ExitWithDelay()
    {
        yield return new WaitForSeconds(_timeToLoadOrExitGame);
        Application.Quit();
    }
}
