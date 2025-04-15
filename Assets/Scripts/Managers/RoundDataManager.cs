using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundDataManager : MonoBehaviour
{
    public Canvas worldSpaceCanvas;
    public Transform collectibleHeadersParent;
    public CollectibleHeader collectibleHeaderPrefab;
    
    public PlayerScoreboard scoreboardPrefab;
    public Transform scoreboardParent;
    
    private Dictionary<int, PlayerGameData> playerGameData;
    private Dictionary<int, PlayerScoreboard> playerScoreboards;

    private ShotInfo currentShotInfo;
    private int currentShotPointsAccrued;
    private int currentShotMultipleAccrued = 1;
    
    private static RoundDataManager instance;
    public static RoundDataManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private GameManager game;
    public GameManager Game => game;
    
    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
    }
    
    public void BallShot(BallShotEvent e)
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        gameData.shotsTaken++;
        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public void CreateNewPlayerRecord(int _index)
    {
        PlayerGameData data = new PlayerGameData();
        
        //TODO: Name might be introduced later
        data.name = $"Player {_index + 1}";
        data.totalPoints = 0;
        data.shotsTaken = 0;
        data.projectileViewsRemaining = GameMode.Instance.numProjectileViews;
        data.shotHistory = new List<ShotInfo>();
        
        if (playerGameData == null)
        {
            playerGameData = new Dictionary<int, PlayerGameData>();
        }

        playerGameData.Add(_index, data);
    }

    public void CreatePlayers(int numPlayers)
    {
        playerScoreboards = new Dictionary<int, PlayerScoreboard>();
        for (int i = 0; i < numPlayers; i++)
        {
            CreateNewPlayerRecord(i);
            PlayerScoreboard scoreboard = Instantiate(scoreboardPrefab, scoreboardParent);
            scoreboard.SetPlayer(i);
            scoreboard.SetScore(0);
            playerScoreboards.Add(i, scoreboard);
        }
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        PlayerScoreboard scoreboard = playerScoreboards[Game.CurrentPlayerTurn];

        switch (e.Type)
        {
            case CollectibleType.Multiple:
                currentShotMultipleAccrued *= e.Value;
                break;
            case CollectibleType.Points:
                int pointsFromThisHit = currentShotMultipleAccrued * e.Value;
                gameData.totalPoints += pointsFromThisHit;
                currentShotPointsAccrued += pointsFromThisHit;
                scoreboard.TickToScore(gameData.totalPoints, pointsFromThisHit);
                break;
        }

        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public int GetPointsForPlayer(int index)
    {
        if (playerGameData.ContainsKey(index))
        {
            return playerGameData[index].totalPoints;
        }

        return -1;
    }
    
    public List<PlayerGameData> GetSortedPlayerData(List<PlayerGameData> players)
    {
        players.Sort((player1, player2) => player2.totalPoints.CompareTo(player1.totalPoints));
        return players;
    }
    
    public List<PlayerGameData> GetPlayerRankings()
    {
        // Convert the dictionary values to a list and sort them by totalPoints in descending order
        return playerGameData.Values
            .OrderByDescending(player => player.totalPoints)
            .ToList();
    }

    public void SetCurrentShotTaker()
    {
        int currentShotTaker = Game.CurrentPlayerTurn;
        for (int i = 0; i < playerScoreboards.Keys.Count; i++)
        {
            playerScoreboards[i].SetCurrentShotTaker(i == currentShotTaker);
        }
    }
    
    public int GetTrajectoryViewsRemaining()
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        return gameData.projectileViewsRemaining;
    }

    public void TrajectoryViewUsed()
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        gameData.projectileViewsRemaining--;
        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public void AddPlayerShotHistory(ShotInfo shotInfo)
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        gameData.shotHistory.Add(shotInfo);
        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public List<ShotInfo> GetTrajectoryHistory()
    {
        return playerGameData[Game.CurrentPlayerTurn].shotHistory;
    }

    public void StartLoggingShotInfo()
    {
        currentShotPointsAccrued = 0;
        currentShotInfo = new ShotInfo();
        currentShotInfo.angle = Game.AngleInput.CalculateProjectedAngle();
        currentShotInfo.spin = Game.SpinInput.SpinVector;
        currentShotInfo.power = (int)Game.PowerInput.Power;
    }

    public void FinishLoggingShotInfo(List<Vector3> capturedTrajectory)
    {
        currentShotInfo.points = currentShotPointsAccrued;
        currentShotInfo.trajectory = capturedTrajectory;
        AddPlayerShotHistory(currentShotInfo);
        currentShotPointsAccrued = 0;
        currentShotMultipleAccrued = 1;
    }
}
