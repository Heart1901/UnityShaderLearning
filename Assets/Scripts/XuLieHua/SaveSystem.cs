using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveSystem : MonoBehaviour
{
    private string savePath => Path.Combine(Application.persistentDataPath, "player.json");

    // 保存复杂对象到文件
    public void SavePlayerData(PlayerData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("保存成功！\n" + json);
    }

    // 从文件加载复杂对象
    public PlayerData LoadPlayerData()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("存档不存在，返回空数据");
            return null;
        }

        string json = File.ReadAllText(savePath);
        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json);
        Debug.Log("加载成功！\n" + json);
        Debug.Log("path:" + savePath);
        return data;
    }
    
    
    void Start()
    {
        // 构建一个包含所有嵌套结构的复杂对象
        var player = new PlayerData
        {
            playerName = "英雄66",
            level = 10,
            equipments = new List<Equipment>
            {
                new Equipment { id = "sword_01", level = 5, skills = new List<string> { "slash", "charge" } },
                new Equipment { id = "armor_02", level = 3, skills = new List<string> { "block" } }
            },
            quests = new Dictionary<string, Quest>
            {
                ["q001"] = new Quest
                {
                    questId = "q001",
                    title = "消灭史莱姆",
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "击杀史莱姆", requiredAmount = 5, isComplete = false }
                    },
                    rewards = new Dictionary<string, int> { ["gold"] = 100, ["exp"] = 50 }
                }
            },
            tags = new Dictionary<string, List<string>>
            {
                ["combat"] = new List<string> { "warrior", "mage" },
                ["craft"] = new List<string> { "blacksmith" }
            }
        };

        var saver = GetComponent<SaveSystem>();
        // saver.SavePlayerData(player);

        // 稍后加载
        PlayerData loaded = saver.LoadPlayerData();
        Debug.Log("加载后的玩家名字：" + loaded.playerName);
    }
}