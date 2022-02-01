/*
 * This script contains the Audio Manager functionality using the Singleton pattern.
 */

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //References to Audio Sources
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

    //Set the UI audio source to ignore the audio listener pause when game pauses
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
