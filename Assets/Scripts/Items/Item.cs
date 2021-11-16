using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public new string name;
    public int cost;
    public Sprite image;

    [TextArea(3,5)]
    public string description;

    public GameEvent OnItemBought;
}
