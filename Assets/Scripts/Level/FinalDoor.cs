/*
 * This script contains the behavior of final door of the game at the castle.
 * It handles the key verification and the world UI above the door
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class FinalDoor : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField]
    private PlayerData _playerData;

    [Header("Door UI")]
    [SerializeField]
    private float _waitToDisplayGameOver;
    [SerializeField]
    private Canvas _doorCanvas;
    private CanvasScaler _doorCanvasScaler;

    [Header("Door Assets")]
    [SerializeField]
    private GameEvent _OnGameOver;
    [SerializeField]
    private AudioClip _succeedSound;
    [SerializeField]
    private AudioClip _failSound;
    [SerializeField]
    private GameObject _succeedFX;

    //HElp variables, to cache references
    private Animator _anim;

    private void Start()
    {
        _doorCanvasScaler = _doorCanvas.GetComponent<CanvasScaler>();

        if (!(_doorCanvas is null))
            _doorCanvasScaler = _doorCanvas.GetComponent<CanvasScaler>();
        else
            Debug.Log("Please reference the door world canvas");

        _anim = GetComponent<Animator>();
    }

    //Enable world UI from door
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            _doorCanvas.enabled = true;
            _doorCanvasScaler.enabled = true;
        }
    }

    //Disable world UI from door
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _doorCanvas.enabled = false;
            _doorCanvasScaler.enabled = false;
        }
    }

    //Key verification
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

    //Save player data and raise game over event
    private IEnumerator DisplayGameOver()
    {
        SaveManager.SavePlayerData(_playerData.data);
        yield return new WaitForSeconds(_waitToDisplayGameOver);
        _OnGameOver.Raise();
    }
}
