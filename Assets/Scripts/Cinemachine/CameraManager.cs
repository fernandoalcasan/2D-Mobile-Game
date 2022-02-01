/*
 * This script makes sure to subscribe to the Camera shift event in order to 
 * control the Cinemachine state driven camera that's being used in the game. 
 */
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
