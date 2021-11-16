// ----------------------------------------------------------------------------
// Game Architecture with Scriptable Objects taken from Ryan Hipple's talk at Unite 2017
// ----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField]
    private GameEvent _event;

    [SerializeField]
    private UnityEvent _response;

    private void OnEnable()
    {
        _event.Subscribe(this);
    }

    private void OnDisable()
    {
        _event.Unsubscribe(this);
    }

    public void OnEventRaised()
    {
        _response.Invoke();
    }
}