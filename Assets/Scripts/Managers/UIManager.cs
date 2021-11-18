using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Text _diamondsText;

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

    private void OnEnable()
    {
        Diamond.OnDiamondCollected += UpdateDiamonds;
    }

    private void OnDisable()
    {   
        Diamond.OnDiamondCollected -= UpdateDiamonds;
    }

    private void UpdateDiamonds()
    {
        _diamondsText.text = "";
    }

}
