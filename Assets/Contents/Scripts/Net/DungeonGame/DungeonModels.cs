using System;

[Serializable]
public class DungeonPlayerData
{
    public int id;
    public string name;
    public int level;
    public int hp;
    public int maxhp;
    public int attack;
    public int gold;
}

[Serializable]
public class DungeonRoomData
{
    public int index;
    public string type;
    public string state;
    public string monsterName;
    public int monsterHp;
    public int monsterAttack;
    public int rewardGold;
}

[Serializable]
public class DungeonStateData
{
    public DungeonPlayerData player;
    public DungeonRoomData room;
}

[Serializable]
public class DungeonResponse
{
    public bool success;
    public string code;
    public string message;
    public DungeonStateData data;
}

[Serializable]
public class DungeonActionRequest
{
    public string action;
}
