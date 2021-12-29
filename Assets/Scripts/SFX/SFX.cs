using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SFX", menuName = "Audio/SFX")]
public class SFX : ScriptableObject
{
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume;
}
