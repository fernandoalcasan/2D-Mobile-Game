/*
 * This script contains the UI Manager functionality using the Singleton pattern.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //Game events to raise (Unite 2017 Event bus pattern)
    [Header("Game Events")]
    [SerializeField]
    private PlayerData _playerData;
    [SerializeField]
    private GameEvent _OnAppQuitOrPause;
    [SerializeField]
    private GameEvent _OnAppResume;

    [Header("UI elements")]
    [SerializeField]
    private Text _diamondsText;
    [SerializeField]
    private Image _healthImage;

    [Header("Scene Load Settings")]
    [SerializeField]
    private float _waitToLoadScene;

    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.Log("UIManager instance is NULL!");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        UpdateDiamonds();
        Diamond.OnDiamondCollected += UpdateDiamonds;
    }

    private void OnDisable()
    {   
        Diamond.OnDiamondCollected -= UpdateDiamonds;
    }

    public void UpdateDiamonds()
    {
        _diamondsText.text = _playerData.data.diamonds.ToString();
    }

    public void UpdateHealth()
    {
        float currentHealth = _playerData.data.health / _playerData.data.maxHealth;
        _healthImage.fillAmount = currentHealth;
    }

    public void PauseGame()
    {
        AudioListener.pause = true;
        Time.timeScale = 0f;
        _OnAppQuitOrPause.Raise();
    }

    public void ResumeGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        _OnAppResume.Raise();
    }

    public void RestartGame()
    {
        StartCoroutine(LoadAsync(1));
    }

    public void GoToMainMenu()
    {
        StartCoroutine(LoadAsync(0));
    }

    private IEnumerator LoadAsync(int scene)
    {
        yield return new WaitForSecondsRealtime(_waitToLoadScene);
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    public void RateGame()
    {
        Application.OpenURL("market://details?id=" + Application.identifier);
    }

    private void OnApplicationQuit()
    {
        _OnAppQuitOrPause.Raise();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            PauseGame();
    }
}
