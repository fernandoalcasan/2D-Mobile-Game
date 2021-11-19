using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private PlayerData _playerData;

    [SerializeField]
    private Text _diamondsText;
    [SerializeField]
    private Image _healthImage;

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
    }

    private void OnDisable()
    {   
        Diamond.OnDiamondCollected -= UpdateDiamonds;
    }

    private void UpdateDiamonds()
    {
        _diamondsText.text = "" + _playerData.diamonds;
    }

    public void UpdateHealth()
    {
        float currentHealth = _playerData.health / _playerData.maxHealth;
        _healthImage.fillAmount = currentHealth;
    }
}
