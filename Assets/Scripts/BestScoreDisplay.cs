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

    // Lokalizált szövegek - magyar és angol verzióban
    private Dictionary<string, string[]> localizedTexts = new Dictionary<string, string[]>()
    {
        // 0 = angol, 1 = magyar
        {"BestScores", new string[] {"Best Scores", "Legjobb Eredmények"}},
        {"Name", new string[] {"Name", "Név"}},
        {"Age", new string[] {"Age", "Kor"}},
        {"Generation", new string[] {"Generation", "Generáció"}},
        {"SimonScore", new string[] {"Simon Score", "Simon Pontszám"}},
        {"ShootingScore", new string[] {"Shooting Score", "Lövés Pontszám"}},
        {"MazeTime", new string[] {"Maze Time", "Labirintus Idõ"}},
        {"NoScores", new string[] {"No scores available", "Nincs elérhetõ pontszám"}},
        {"NoData", new string[] {"No data file found", "Nem található adatfájl"}},
        {"BestShot", new string[] {"Best Shot", "Legjobb Lövés"}},
        {"Seconds", new string[] {"seconds", "másodperc"}},
        {"NotAvailable", new string[] {"N/A", "N/A"}},
    };

    private void Awake()
    {
        // Elérési út beállítása
        backupFolderPath = Path.Combine(Application.persistentDataPath, backupFolderName);

        // Lokalizációs manager lekérése
        locManager = FindObjectOfType<LoacalisationManagerScript>();
        if (locManager == null)
        {
            Debug.LogWarning("LoacalisationManagerScript not found!");
        }
        else
        {
            // Az aktuális nyelv megjegyzése
            previousLanguage = locManager.getLocal();
        }
    }


    private void Start()
    {
        // Elsõ indításkor frissítsük az adatokat
        if (autoRefreshOnStart)
        {
            RefreshBestScores();
        }
    }

    private void Update()
    {
        // Csak akkor ellenõrizzük, ha van lokalizációs manager
        if (locManager != null)
        {
            // Lekérdezzük az aktuális nyelvi beállítást
            int currentLanguage = locManager.getLocal();

            // Ha változott a nyelv, frissítjük a megjelenítést
            if (currentLanguage != previousLanguage)
            {
                LogMessage($"Nyelvváltás észlelve: {previousLanguage} -> {currentLanguage}");
                RefreshBestScores();
                previousLanguage = currentLanguage;
            }
        }
    }

    // Lokalizált szöveg lekérése
    private string GetLocalizedText(string key, params object[] args)
    {
        int langIndex = 0; // Alapértelmezetten angol

        if (locManager != null)
        {
            langIndex = locManager.getLocal();

            // Ellenõrizzük, hogy érvényes index-e (0 = angol, 1 = magyar)
            if (langIndex < 0 || langIndex > 1)
            {
                langIndex = 0; // Fallback angol nyelvre
            }
        }

        // Ellenõrizzük, hogy létezik-e a kulcs a szótárban
        if (localizedTexts.TryGetValue(key, out string[] texts) && langIndex < texts.Length)
        {
            return string.Format(texts[langIndex], args);
        }

        return $"[Missing:{key}]"; // Hiányzó kulcs jelzése
    }

    // Publikus metódus a pontszámok frissítésére
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
            // JSON fájl beolvasása
            string jsonData = File.ReadAllText(summaryFilePath);
            AllPlayersWrapper wrapper = JsonUtility.FromJson<AllPlayersWrapper>(jsonData);

            if (wrapper == null || wrapper.players == null || wrapper.players.Count == 0)
            {
                bestScoresText.text = GetLocalizedText("NoScores");
                return;
            }

            // Legjobb eredmények keresése
            PlayerData bestSimonPlayer = GetBestSimonPlayer(wrapper.players);
            PlayerData bestMazePlayer = GetBestMazePlayer(wrapper.players);
            PlayerData bestShootingPlayer = GetBestShootingPlayer(wrapper.players);

            // Eredmények megjelenítése
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"<b>{GetLocalizedText("BestScores")}</b>\n");

            // Simon Játék
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

            // Labirintus Játék
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

            // Lövöldözõs Játék
            sb.AppendLine($"<color=#4da6ff><b>{GetLocalizedText("ShootingScore")}:</b></color>");
            if (bestShootingPlayer != null && bestShootingPlayer.shootingSessions != null && bestShootingPlayer.shootingSessions.Count > 0)
            {
                sb.AppendLine($"{GetLocalizedText("Name")}: {bestShootingPlayer.playerName}");
                sb.AppendLine($"{GetLocalizedText("Age")}: {bestShootingPlayer.playerAge}");
                sb.AppendLine($"{GetLocalizedText("Generation")}: {bestShootingPlayer.playerAgeGeneration}");

                // Legjobb lövés reactionTime
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

    // Legjobb Simon játékos meghatározása
    private PlayerData GetBestSimonPlayer(List<PlayerData> players)
    {
        return players
            .Where(p => p.simonData != null && p.simonData.highScore > 0)
            .OrderByDescending(p => p.simonData.highScore)
            .FirstOrDefault();
    }

    // Legjobb Labirintus játékos meghatározása
    private PlayerData GetBestMazePlayer(List<PlayerData> players)
    {
        return players
            .Where(p => p.mazeData != null && p.mazeData.completionTime > 0)
            .OrderBy(p => p.mazeData.completionTime)  // Legrövidebb idõ a legjobb
            .FirstOrDefault();
    }

    // Legjobb Lövöldözõs játékos meghatározása 
    private PlayerData GetBestShootingPlayer(List<PlayerData> players)
    {
        // Játékosok szûrése, akiknek van lövés adata
        var playersWithShots = players
            .Where(p => p.shootingSessions != null &&
                        p.shootingSessions.Any(s => s.shots != null && s.shots.Count > 0))
            .ToList();

        if (playersWithShots.Count == 0)
            return null;

        // Minden játékos legjobb lövésének meghatározása
        var playerBestShots = new List<Tuple<PlayerData, ShootingData>>();

        foreach (var player in playersWithShots)
        {
            var bestShot = GetBestShot(player.shootingSessions);
            if (bestShot != null)
            {
                playerBestShots.Add(new Tuple<PlayerData, ShootingData>(player, bestShot));
            }
        }

        // Sorba rendezés a legjobb lövés alapján
        if (playerBestShots.Count > 0)
        {
            return playerBestShots
                .OrderBy(t => t.Item2.reactionTime)  // Leggyorsabb reakcióidõ a legjobb
                .FirstOrDefault()?.Item1;
        }

        return null;
    }

    // Legjobb lövés meghatározása
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

        // Leggyorsabb reakcióidejû lövés meghatározása
        return allShots
            .OrderBy(s => s.reactionTime)
            .FirstOrDefault();
    }

    // Debug üzenetek kezelése
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

    // Ez a metódus meghívható egy UI gombról
    public void RefreshButtonClicked()
    {
        RefreshBestScores();
    }
}