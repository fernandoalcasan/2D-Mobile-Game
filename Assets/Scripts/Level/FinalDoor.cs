using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class FinalDoor : MonoBehaviour
{
    [SerializeField]
    private float _waitToDisplayGameOver;

    [SerializeField]
    private PlayerData _playerData;

    [SerializeField]
    private Canvas _doorCanvas;
    private CanvasScaler _doorCanvasScaler;
    [SerializeField]
    private Canvas _gameOverCanvas;
    private CanvasScaler _goCanvasScaler;

    [SerializeField]
    private AudioClip _succeedSound;
    [SerializeField]
    private AudioClip _failSound;

    [SerializeField]
    private GameObject _succeedFX;
    private Animator _anim;

    private void Start()
    {
        _doorCanvasScaler = _doorCanvas.GetComponent<CanvasScaler>();

        if (!(_doorCanvas is null))
            _doorCanvasScaler = _doorCanvas.GetComponent<CanvasScaler>();
        else
            Debug.Log("Please reference the door world canvas");

        if (!(_gameOverCanvas is null))
            _goCanvasScaler = _gameOverCanvas.GetComponent<CanvasScaler>();
        else
            Debug.Log("Please reference the game over canvas");

        _anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            _doorCanvas.enabled = true;
            _doorCanvasScaler.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _doorCanvas.enabled = false;
            _doorCanvasScaler.enabled = false;
        }
    }

    public void TryToOpenDoor()
    {
        if(_playerData.data.gotCastleKey)
        {
            _anim.SetTrigger("Open");

            GameObject succeedFX = Instantiate(_succeedFX, transform.position, Quaternion.identity);
            Destroy(succeedFX, 2f);

            AudioManager.Instance.PlayOneShotSFX(_succeedSound, 1f);

            StartCoroutine(DisplayGameOver());
        }
        else
        {
            AudioManager.Instance.PlayOneShotSFX(_failSound, 1f);
        }
    }

    private IEnumerator DisplayGameOver()
    {
        yield return new WaitForSeconds(_waitToDisplayGameOver);
        _gameOverCanvas.enabled = true;
        _goCanvasScaler.enabled = true;
    }
}
