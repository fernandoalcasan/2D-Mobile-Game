/*
 * This script allows to create items for the shop as scriptable objects
 */

using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public new string name;
    public int cost;
    public Sprite image;

    [TextArea(3,5)]
    public string description;
    [TextArea(3, 5)]
    public string buyPhrase;

    public GameEvent OnItemBought;
}
