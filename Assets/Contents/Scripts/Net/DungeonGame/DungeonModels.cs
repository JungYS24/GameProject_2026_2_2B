using System;

[Serializable]
public class DungeonPlayerData
{
    public int id;
    public string name;
    public int level;
    public int hp;
    public int maxHp;
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
public class DungeonRunData
{
    public string id;
    public string state;
    public int currentRoomIndex;
    public int runGold;
    public DungeonRoomData currentRoom;
}

[Serializable]
public class DungeonStateData
{
    public DungeonPlayerData player;
    public DungeonRunData run;
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
