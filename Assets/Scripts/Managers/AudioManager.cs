using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _sfx;
    [SerializeField]
    private AudioSource _ui;

    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance is null)
                Debug.LogError("AudioManager instance is NULL!");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        _ui.ignoreListenerPause = true;
    }

    public void PlayOneShotSFX(AudioClip audio, float volume)
    {
        _sfx.PlayOneShot(audio, volume);
    }

    public void PlayOneShotUI(AudioClip audio, float volume)
    {
        _ui.PlayOneShot(audio, volume);
    }
}
