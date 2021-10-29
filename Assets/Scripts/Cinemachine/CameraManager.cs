using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraManager : MonoBehaviour
{
    private Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
        if (_anim is null)
            Debug.LogError("Animator is NULL");
    }

    private void OnEnable()
    {
        CameraStateShift.OnCameraShift += ChangeCameraState;
    }

    private void OnDisable()
    {
        CameraStateShift.OnCameraShift -= ChangeCameraState;
    }

    private void ChangeCameraState(int animHash)
    {
        _anim.Play(animHash);
    }

}
