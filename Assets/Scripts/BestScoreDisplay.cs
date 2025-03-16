using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class BestScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI bestScoresText;

    [Header("Settings")]
    [SerializeField] private string backupFolderName = "PlayerDataBackup";
    [SerializeField] private string summaryFileName = "AllPlayersData.json";
    [SerializeField] private bool autoRefreshOnStart = true;
    [SerializeField] private bool logDebugMessages = true;

    private string backupFolderPath;
    private LoacalisationManagerScript locManager;
    private int previousLanguage = -1;

    // Wrapper class for JSON deserialization
    [Serializable]
    private class AllPlayersWrapper
    {
        public List<PlayerData> players;
    }

    [Serializable]
    private class PlayerData
    {
        public int playerId;
        public string playerName;
        public int playerAge;
        public string playerEmail;
        public string playerAgeGeneration;
        public SimonGameData simonData;
        public MazeGameData mazeData;
        public List<ShootingSessionData> shootingSessions;
    }

    [Serializable]
    private class SimonGameData
    {
        public int highScore;
        public string recordedAt;
    }

    [Serializable]
    private class MazeGameData
    {
        public double completionTime;
        public string formattedTime;
        public string recordedAt;
    }

    [Serializable]
    private class ShootingSessionData
    {
        public int sessionId;
        public string startedAt;
        public List<ShootingData> shots;
    }

    [Serializable]
    private class ShootingData
    {
        public int shotNumber;
        public double reactionTime;
        public double positionX;
        public double positionY;
        public string recordedAt;
    }

    // Lokaliz�lt sz�vegek - magyar �s angol verzi�ban
    private Dictionary<string, string[]> localizedTexts = new Dictionary<string, string[]>()
    {
        // 0 = angol, 1 = magyar
        {"BestScores", new string[] {"Best Scores", "Legjobb Eredm�nyek"}},
        {"Name", new string[] {"Name", "N�v"}},
        {"Age", new string[] {"Age", "Kor"}},
        {"Generation", new string[] {"Generation", "Gener�ci�"}},
        {"SimonScore", new string[] {"Simon Score", "Simon Pontsz�m"}},
        {"ShootingScore", new string[] {"Shooting Score", "L�v�s Pontsz�m"}},
        {"MazeTime", new string[] {"Maze Time", "Labirintus Id�"}},
        {"NoScores", new string[] {"No scores available", "Nincs el�rhet� pontsz�m"}},
        {"NoData", new string[] {"No data file found", "Nem tal�lhat� adatf�jl"}},
        {"BestShot", new string[] {"Best Shot", "Legjobb L�v�s"}},
        {"Seconds", new string[] {"seconds", "m�sodperc"}},
        {"NotAvailable", new string[] {"N/A", "N/A"}},
    };

    private void Awake()
    {
        // El�r�si �t be�ll�t�sa
        backupFolderPath = Path.Combine(Application.persistentDataPath, backupFolderName);

        // Lokaliz�ci�s manager lek�r�se
        locManager = FindObjectOfType<LoacalisationManagerScript>();
        if (locManager == null)
        {
            Debug.LogWarning("LoacalisationManagerScript not found!");
        }
        else
        {
            // Az aktu�lis nyelv megjegyz�se
            previousLanguage = locManager.getLocal();
        }
    }


    private void Start()
    {
        // Els� ind�t�skor friss�ts�k az adatokat
        if (autoRefreshOnStart)
        {
            RefreshBestScores();
        }
    }

    private void Update()
    {
        // Csak akkor ellen�rizz�k, ha van lokaliz�ci�s manager
        if (locManager != null)
        {
            // Lek�rdezz�k az aktu�lis nyelvi be�ll�t�st
            int currentLanguage = locManager.getLocal();

            // Ha v�ltozott a nyelv, friss�tj�k a megjelen�t�st
            if (currentLanguage != previousLanguage)
            {
                LogMessage($"Nyelvv�lt�s �szlelve: {previousLanguage} -> {currentLanguage}");
                RefreshBestScores();
                previousLanguage = currentLanguage;
            }
        }
    }

    // Lokaliz�lt sz�veg lek�r�se
    private string GetLocalizedText(string key, params object[] args)
    {
        int langIndex = 0; // Alap�rtelmezetten angol

        if (locManager != null)
        {
            langIndex = locManager.getLocal();

            // Ellen�rizz�k, hogy �rv�nyes index-e (0 = angol, 1 = magyar)
            if (langIndex < 0 || langIndex > 1)
            {
                langIndex = 0; // Fallback angol nyelvre
            }
        }

        // Ellen�rizz�k, hogy l�tezik-e a kulcs a sz�t�rban
        if (localizedTexts.TryGetValue(key, out string[] texts) && langIndex < texts.Length)
        {
            return string.Format(texts[langIndex], args);
        }

        return $"[Missing:{key}]"; // Hi�nyz� kulcs jelz�se
    }

    // Publikus met�dus a pontsz�mok friss�t�s�re
    public void RefreshBestScores()
    {
        if (bestScoresText == null)
        {
            LogMessage("bestScoresText is not assigned!", true);
            return;
        }

        string summaryFilePath = Path.Combine(backupFolderPath, summaryFileName);

        if (!File.Exists(summaryFilePath))
        {
            LogMessage($"Summary file not found at: {summaryFilePath}", true);
            bestScoresText.text = GetLocalizedText("NoData");
            return;
        }

        try
        {
            // JSON f�jl beolvas�sa
            string jsonData = File.ReadAllText(summaryFilePath);
            AllPlayersWrapper wrapper = JsonUtility.FromJson<AllPlayersWrapper>(jsonData);

            if (wrapper == null || wrapper.players == null || wrapper.players.Count == 0)
            {
                bestScoresText.text = GetLocalizedText("NoScores");
                return;
            }

            // Legjobb eredm�nyek keres�se
            PlayerData bestSimonPlayer = GetBestSimonPlayer(wrapper.players);
            PlayerData bestMazePlayer = GetBestMazePlayer(wrapper.players);
            PlayerData bestShootingPlayer = GetBestShootingPlayer(wrapper.players);

            // Eredm�nyek megjelen�t�se
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"<b>{GetLocalizedText("BestScores")}</b>\n");

            // Simon J�t�k
            sb.AppendLine($"<color=#4da6ff><b>{GetLocalizedText("SimonScore")}:</b></color>");
            if (bestSimonPlayer != null && bestSimonPlayer.simonData != null && bestSimonPlayer.simonData.highScore > 0)
            {
                sb.AppendLine($"{GetLocalizedText("Name")}: {bestSimonPlayer.playerName}");
                sb.AppendLine($"{GetLocalizedText("Age")}: {bestSimonPlayer.playerAge}");
                sb.AppendLine($"{GetLocalizedText("Generation")}: {bestSimonPlayer.playerAgeGeneration}");
                sb.AppendLine($"{GetLocalizedText("SimonScore")}: {bestSimonPlayer.simonData.highScore}");
            }
            else
            {
                sb.AppendLine($"{GetLocalizedText("NotAvailable")}");
            }

            sb.AppendLine();

            // Labirintus J�t�k
            sb.AppendLine($"<color=#4da6ff><b>{GetLocalizedText("MazeTime")}:</b></color>");
            if (bestMazePlayer != null && bestMazePlayer.mazeData != null && !string.IsNullOrEmpty(bestMazePlayer.mazeData.formattedTime))
            {
                sb.AppendLine($"{GetLocalizedText("Name")}: {bestMazePlayer.playerName}");
                sb.AppendLine($"{GetLocalizedText("Age")}: {bestMazePlayer.playerAge}");
                sb.AppendLine($"{GetLocalizedText("Generation")}: {bestMazePlayer.playerAgeGeneration}");
                sb.AppendLine($"{GetLocalizedText("MazeTime")}: {bestMazePlayer.mazeData.formattedTime}");
            }
            else
            {
                sb.AppendLine($"{GetLocalizedText("NotAvailable")}");
            }

            sb.AppendLine();

            // L�v�ld�z�s J�t�k
            sb.AppendLine($"<color=#4da6ff><b>{GetLocalizedText("ShootingScore")}:</b></color>");
            if (bestShootingPlayer != null && bestShootingPlayer.shootingSessions != null && bestShootingPlayer.shootingSessions.Count > 0)
            {
                sb.AppendLine($"{GetLocalizedText("Name")}: {bestShootingPlayer.playerName}");
                sb.AppendLine($"{GetLocalizedText("Age")}: {bestShootingPlayer.playerAge}");
                sb.AppendLine($"{GetLocalizedText("Generation")}: {bestShootingPlayer.playerAgeGeneration}");

                // Legjobb l�v�s reactionTime
                var bestShot = GetBestShot(bestShootingPlayer.shootingSessions);
                if (bestShot != null)
                {
                    sb.AppendLine($"{GetLocalizedText("BestShot")}: {bestShot.reactionTime:F2} {GetLocalizedText("Seconds")}");
                }
                else
                {
                    sb.AppendLine($"{GetLocalizedText("BestShot")}: {GetLocalizedText("NotAvailable")}");
                }
            }
            else
            {
                sb.AppendLine($"{GetLocalizedText("NotAvailable")}");
            }

            bestScoresText.text = sb.ToString();
        }
        catch (Exception ex)
        {
            LogMessage($"Error loading best scores: {ex.Message}", true);
            bestScoresText.text = $"Error: {ex.Message}";
        }
    }

    // Legjobb Simon j�t�kos meghat�roz�sa
    private PlayerData GetBestSimonPlayer(List<PlayerData> players)
    {
        return players
            .Where(p => p.simonData != null && p.simonData.highScore > 0)
            .OrderByDescending(p => p.simonData.highScore)
            .FirstOrDefault();
    }

    // Legjobb Labirintus j�t�kos meghat�roz�sa
    private PlayerData GetBestMazePlayer(List<PlayerData> players)
    {
        return players
            .Where(p => p.mazeData != null && p.mazeData.completionTime > 0)
            .OrderBy(p => p.mazeData.completionTime)  // Legr�videbb id� a legjobb
            .FirstOrDefault();
    }

    // Legjobb L�v�ld�z�s j�t�kos meghat�roz�sa 
    private PlayerData GetBestShootingPlayer(List<PlayerData> players)
    {
        // J�t�kosok sz�r�se, akiknek van l�v�s adata
        var playersWithShots = players
            .Where(p => p.shootingSessions != null &&
                        p.shootingSessions.Any(s => s.shots != null && s.shots.Count > 0))
            .ToList();

        if (playersWithShots.Count == 0)
            return null;

        // Minden j�t�kos legjobb l�v�s�nek meghat�roz�sa
        var playerBestShots = new List<Tuple<PlayerData, ShootingData>>();

        foreach (var player in playersWithShots)
        {
            var bestShot = GetBestShot(player.shootingSessions);
            if (bestShot != null)
            {
                playerBestShots.Add(new Tuple<PlayerData, ShootingData>(player, bestShot));
            }
        }

        // Sorba rendez�s a legjobb l�v�s alapj�n
        if (playerBestShots.Count > 0)
        {
            return playerBestShots
                .OrderBy(t => t.Item2.reactionTime)  // Leggyorsabb reakci�id� a legjobb
                .FirstOrDefault()?.Item1;
        }

        return null;
    }

    // Legjobb l�v�s meghat�roz�sa
    private ShootingData GetBestShot(List<ShootingSessionData> sessions)
    {
        var allShots = new List<ShootingData>();

        foreach (var session in sessions)
        {
            if (session.shots != null && session.shots.Count > 0)
            {
                allShots.AddRange(session.shots);
            }
        }

        // Leggyorsabb reakci�idej� l�v�s meghat�roz�sa
        return allShots
            .OrderBy(s => s.reactionTime)
            .FirstOrDefault();
    }

    // Debug �zenetek kezel�se
    private void LogMessage(string message, bool isError = false)
    {
        if (logDebugMessages)
        {
            if (isError)
                Debug.LogError($"[BestScoreDisplay] {message}");
            else
                Debug.Log($"[BestScoreDisplay] {message}");
        }
    }

    // Ez a met�dus megh�vhat� egy UI gombr�l
    public void RefreshButtonClicked()
    {
        RefreshBestScores();
    }
}