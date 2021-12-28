using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerData _playerData;

    [SerializeField]
    private Text _diamondsText;
    [SerializeField]
    private Image _healthImage;

    [SerializeField]
    private Canvas _pauseCanvas;
    private CanvasScaler _pauseCanvasScaler;

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
        UpdateHealth();
        UpdateDiamonds();
        Diamond.OnDiamondCollected += UpdateDiamonds;

        if (!(_pauseCanvas is null))
            _pauseCanvasScaler = _pauseCanvas.GetComponent<CanvasScaler>();
        else
            Debug.Log("Please reference the pause canvas");
    }

    private void OnDisable()
    {   
        Diamond.OnDiamondCollected -= UpdateDiamonds;
    }

    public void UpdateDiamonds()
    {
        _diamondsText.text = _playerData.diamonds.ToString();
    }

    public void UpdateHealth()
    {
        float currentHealth = _playerData.health / _playerData.maxHealth;
        _healthImage.fillAmount = currentHealth;
    }

    public void PauseGame()
    {
        AudioListener.pause = true;
        Time.timeScale = 0f;
        _pauseCanvas.enabled = true;
        _pauseCanvasScaler.enabled = true;
    }

    public void ResumeGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
        _pauseCanvas.enabled = false;
        _pauseCanvasScaler.enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
