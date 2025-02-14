using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    public Tile[,] Tiles {get; private set; }

    public int Width => Tiles.GetLength(dimension: 0);
    public int Height => Tiles.GetLength(dimension: 1);

    private readonly List<Tile> _selection = new List<Tile>();

    private const float TweenDuration = 0.25f;


    private void Awake() => Instance = this;

    private void Start()
    {
        Tiles = new Tile[rows.Max(rows => rows.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];
                
                tile.x = x;
                tile.y = y;

                tile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }      
    }


    public async void Select(Tile tile)
    {
        if(!_selection.Contains(tile)) _selection.Add(tile);

        if (_selection.Count < 2) return;

        Debug.Log(message:$"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else 
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform; 
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
               .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play()
            .AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1.Item;
    }

    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2) return true;

        return false;
            
        
    }

    private async void Pop()
{
    if (Tiles == null) return; // Check if Tiles is initialized

    for (var y = 0; y < Height; y++)
    {
        for (var x = 0; x < Width; x++)
        {
            var tile = Tiles[x, y];

            if (tile == null || tile.icon == null || tile.Item == null) continue; // Additional null checks

            var connectedTiles = tile.GetConnectedTiles();

            if(connectedTiles.Skip(1).Count() < 2) continue;

            var deflateSequence = DOTween.Sequence();

            foreach (var connectedTile in connectedTiles)
            {
                if (connectedTile == null || connectedTile.icon == null) continue; // Null check for each connected tile
                deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));
            }

            await deflateSequence.Play().AsyncWaitForCompletion();

            if (ScoreCounter.Instance == null) continue; 

            ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count;

            Debug.Log($"Updated Score: {ScoreCounter.Instance.Score}");

            var inflateSequence = DOTween.Sequence();

            foreach (var connectedTile in connectedTiles)
            {
                if (connectedTile == null) continue; // Null check in inflate loop
                connectedTile.Item = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];

                inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
            }

            await inflateSequence.Play().AsyncWaitForCompletion();
        }
    }
}
}

