using System.Collections.Generic;

// 装备
public class Equipment
{
    public string id;
    public int level;
    public List<string> skills;       // 技能ID列表
}

// 任务目标
public class QuestObjective
{
    public string description;
    public int requiredAmount;
    public bool isComplete;
}

// 任务
public class Quest
{
    public string questId;
    public string title;
    public List<QuestObjective> objectives;   // 嵌套列表
    public Dictionary<string, int> rewards;   // 奖励：物品ID -> 数量
}

// 角色数据（最外层）
public class PlayerData
{
    public string playerName;
    public int level;
    public List<Equipment> equipments;            // 装备列表
    public Dictionary<string, Quest> quests;      // 任务字典
    public Dictionary<string, List<string>> tags; // 复杂字典：标签组 -> 标签列表
}