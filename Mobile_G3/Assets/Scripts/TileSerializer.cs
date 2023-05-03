using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;

public class TileSerializer
{
    [System.Serializable]
    private struct TileData
    {
        public string name;
        public string transformJson;
        public string entityJson;
        public string floorJson;
        public int x;
        public int y;
    }

    private static string SerializeTile(Tile tile)
    {
        TileData data = new TileData();
        data.name = tile.name;
        data.transformJson = JsonUtility.ToJson(tile.transform);
        data.entityJson = JsonUtility.ToJson(tile.entity);
        data.floorJson = JsonUtility.ToJson(tile.floor);
        data.x = tile.coordinates.x;
        data.y = tile.coordinates.y;
        return JsonUtility.ToJson(data);
    }

    private static Tile DeserializeTile(string json)
    {
        TileData data = JsonUtility.FromJson<TileData>(json);
        Tile tile = new Tile();
        tile.name = data.name;
        tile.transform = JsonUtility.FromJson<Transform>(data.transformJson);
        tile.entity = JsonUtility.FromJson<Component>(data.entityJson);
        tile.floor = JsonUtility.FromJson<Component>(data.floorJson);
        tile.coordinates = new int2(data.x, data.y);
        return tile;
    }

    public static void SaveTiles(List<Tile> tiles, string filePath)
    {
        List<string> jsonList = new List<string>();
        foreach (Tile tile in tiles)
        {
            jsonList.Add(SerializeTile(tile));
        }
        File.WriteAllLines(filePath, jsonList.ToArray());
    }

    public static List<Tile> LoadTiles(string filePath)
    {
        List<Tile> tiles = new List<Tile>();
        string[] jsonList = File.ReadAllLines(filePath);
        foreach (string json in jsonList)
        {
            tiles.Add(DeserializeTile(json));
        }
        return tiles;
    }
}