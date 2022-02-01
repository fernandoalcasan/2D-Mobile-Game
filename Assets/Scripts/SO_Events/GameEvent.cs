/* ----------------------------------------------------------------------------
* Game Architecture with Scriptable Objects taken from Ryan Hipple's talk at Unite 2017
* This script helps to implement the Event bus pattern as events in scriptable objects
* ----------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Game_Event", menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private readonly List<GameEventListener> _listeners = new List<GameEventListener>();

    public void Raise()
    {
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i].OnEventRaised();
        }
    }

    public void Subscribe(GameEventListener listener)
    {
        if(!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void Unsubscribe(GameEventListener listener)
    {
        if (_listeners.Contains(listener))
            _listeners.Remove(listener);
    }
}