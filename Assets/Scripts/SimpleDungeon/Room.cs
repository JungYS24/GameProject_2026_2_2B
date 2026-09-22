using UnityEngine;

[System.Serializable]
public class Room
{
    public Vector2Int center;
    public int size;
    public RoomType type;

    public Room(Vector2Int center, int size, RoomType type)
    {
        this.center = center;
        this.size = size;
        this.type = type;
    }

    public Color Color => type switch
    {
        RoomType.Start => new Color(0.35f, 1f, 0.35f),
        RoomType.Treasure => new Color(1f, 0.8f, 0.2f),
        RoomType.Boss => new Color(1f, 0.3f, 0.3f),
        _ => Color.white
    };
}
