using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BattleLogger : MonoBehaviour
{
    public static BattleLogger Instance;
    private List<string> _rows = new List<string>();

    private string _filePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            _filePath = Path.Combine(Application.persistentDataPath, "output.csv");

            _rows.Add("team_id,position_x,position_z,health,armor,weapon_level,distance_to_enemy,action,result,alive");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogAgentData(int teamId, Vector3 position, float health, float armor, int weaponLevel, float distanceToEnemy, int action, string result, bool isAlive)
    {
        string row = $"{teamId},{position.x:F2},{position.z:F2},{health:F1},{armor:F1},{weaponLevel},{distanceToEnemy:F2},{action},{result},{(isAlive ? 1 : 0)}";
        _rows.Add(row);
    }

    public void SaveToFile()
    {
        File.WriteAllLines(_filePath, _rows.ToArray());
        Debug.Log($"«бережено лог у файл: {_filePath}");
    }
}

