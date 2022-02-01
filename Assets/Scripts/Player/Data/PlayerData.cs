/*
 * This class allows to create scriptable objects that encapsulate player's data that is serializable.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public Data data;
}
