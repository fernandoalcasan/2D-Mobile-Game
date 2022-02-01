/*
 * This script allows to store audios with a respective volume as scriptable objects
 */

using UnityEngine;

[CreateAssetMenu(fileName = "SFX", menuName = "Audio/SFX")]
public class SFX : ScriptableObject
{
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume;
}
