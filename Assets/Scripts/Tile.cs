using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;
    private Item _item;

    public Item Item
    {
        get => _item;

        set
        {
            if(_item == value) return;

            _item = value;

            icon.sprite = _item.sprite;
        }
    }

    public Image icon;

    public Button button;

    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => x < Board.Instance.Width -1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Bottom => y < Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    public Tile[] Neighbors => new []
    {
        Left,
        Top,
        Right,
        Bottom,
    };

    private void Start() => button.onClick.AddListener(call:() => Board.Instance.Select(this));

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
{
    var result = new List<Tile> { this };
    // Using 'value' for debugging; not ideal but works if no other identifier is available
    Debug.Log($"Starting GetConnectedTiles from Tile at ({x}, {y}) with Item Value: {Item.value}");

    if (exclude == null)
    {
        exclude = new List<Tile> { this };
    }
    else
    {
        if (!exclude.Contains(this))
            exclude.Add(this);
    }

    foreach (var neighbor in Neighbors)
    {
        if (neighbor == null || exclude.Contains(neighbor) || neighbor.Item != Item) continue;

        // Using 'value' for debugging
        Debug.Log($"Connecting to neighbor at ({neighbor.x}, {neighbor.y}) with Item Value: {neighbor.Item.value}");
        result.AddRange(neighbor.GetConnectedTiles(exclude));
    }

    return result;
}
    
}
