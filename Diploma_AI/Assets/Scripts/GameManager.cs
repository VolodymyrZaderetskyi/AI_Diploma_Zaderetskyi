// Attach this script to an empty GameObject in your scene.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public Warrior[] warriorPrefabs; // 6 prefabs to choose from
    public Button startButton;
    public Button CodeButton;
    public Button MLPButton;
    public Button QButton;
    public Button RFButton;
    public int warriorsPerTeam = 10;
    public float fieldMin = -4f;
    public float fieldMax = 4f;
    public float attackDistance = 0.5f;

    private List<Warrior> teamA = new List<Warrior>();
    private List<Warrior> teamB = new List<Warrior>();
    private bool gameStarted = false;
    private Types type = Types.Code;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        CodeButton.onClick.AddListener(OnCodeBtn);
        MLPButton.onClick.AddListener(OnMLPButton);
        QButton.onClick.AddListener(OnQButton);
        RFButton.onClick.AddListener(OnRFButton);
    }
    


    void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;
        ClearTable();
        SpawnTeam(0,teamA, -4f, -1f, true); // Team A on the left
        SpawnTeam(1,teamB, 1f, 4f, false);   // Team B on the right
    }

    void SpawnTeam(int teamId, List<Warrior> teamList, float minX, float maxX, bool green)
    {
        for (int i = 0; i < warriorsPerTeam; i++)
        {
            Warrior prefab = warriorPrefabs[Random.Range(0, warriorPrefabs.Length)];
            Vector3 position = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(fieldMin, fieldMax));
            Warrior warrior = Instantiate<Warrior>(prefab, position, Quaternion.identity, transform);
            warrior.transform.localPosition = position;
            Types typed = type;
            if (teamId == 0)
                typed = Types.Code;
            warrior.Setup(teamList == teamA ? teamB : teamA, green, typed, this);
            teamList.Add(warrior);
        }
    }

    public void CheckVictoryCondition()
    {
        bool teamADead = teamA.TrueForAll(w => !w.IsAlive);
        bool teamBDead = teamB.TrueForAll(w => !w.IsAlive);

        if (teamADead || teamBDead)
        {
            string winner = teamADead && teamBDead ? "Нічия" : (teamADead ? "Команда B перемогла" : "Команда A перемогла");
            Debug.Log(winner);
            gameStarted = false;
        }
    }

    private void ClearTable()
    {
        foreach (var item in teamA)
        {
            Destroy(item.gameObject);
        }

        foreach (var item in teamB)
        {
            Destroy(item.gameObject);
        }

        teamA.Clear();
        teamB.Clear();
    }

    private void OnCodeBtn()
    {
        type = Types.Code;
    }
    private void OnMLPButton()
    {
        type = Types.MLP;
    }

    private void OnQButton()
    {
        type = Types.Q;
    }

    private void OnRFButton()
    {
        type = Types.RF;
    }
}
