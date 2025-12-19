using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public int itemId;

    // 임시 생성자
    public Item(string name, int id)
    {
        itemName = name;
        itemId = id;
    }
}