
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI simonScore;
    [SerializeField] private TextMeshProUGUI shootingScore;
    [SerializeField] private TextMeshProUGUI mazeScore;
    [SerializeField] private TextMeshProUGUI targetScore; // Új UI elem a target játék pontszámához

    // Új UI elem az aktuális Simon pontszámhoz
    [SerializeField] private TextMeshProUGUI simonCurrentScore;

    [Header("Game References")]
    [SerializeField] private SimonScores sm_scr;
    [SerializeField] private ObjectSpawner_1place objectSpawner;
    [SerializeField] private ObjectSpawner targetObjectSpawner; // Új referencia az ObjectSpawner-hez
    [SerializeField] private ButtonsForMaze maze_scr;
    [SerializeField] private Gun gunscript;

    // Lokalizációs Manager
    private LoacalisationManagerScript locManager;

    private SQLiteDBScript dbManager;
    private List<double> shootingTimes = new List<double>();
    private List<string> hitPositions = new List<string>();
    private double bestTime = double.MaxValue;
    private int bestTimeIndex = -1;

    private static int lastSavedShotIndex = -1;

    // Az ObjectSpawner-hez tartozó adatok
    private List<double> targetShootingTimes = new List<double>();
    private List<string> targetHitPositions = new List<string>();
    private double targetBestTime = double.MaxValue;
    private int targetBestTimeIndex = -1;
    private static int lastSavedTargetShotIndex = -1;

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();

    // Lokalizált szövegek - magyar és angol verzióban
    private Dictionary<string, string[]> localizedTexts = new Dictionary<string, string[]>()
    {
        // 0 = angol, 1 = magyar
        {"NoHits", new string[] {"No hits recorded yet", "Még nincs találat rögzítve"}},
        {"SimonHighScore", new string[] {"Simon High Score: {0}", "Simon Legjobb Eredmény: {0}"}},
        {"CurrentScore", new string[] {"Current Score: {0}", "Aktuális Pontszám: {0}"}},
        {"MazeScore", new string[] {"Maze Score: {0}", "Labirintus Idő: {0}"}},
        {"BestShot", new string[] {"Best Shot:", "Legjobb Lövés:"}},
        {"Time", new string[] {"Time: {0:F2} sec", "Idő: {0:F2} mp"}},
        {"Position", new string[] {"Position: {0}", "Pozíció: {0}"}},
        {"PositionNA", new string[] {"Position: N/A", "Pozíció: N/A"}},
        {"LastShot", new string[] {"Last Shot:", "Utolsó Lövés:"}},
        {"TotalHits", new string[] {"Total Hits: {0}", "Összes Találat: {0}"}},
        {"SessionID", new string[] {"Session ID: {0}", "Munkamenet ID: {0}"}},
        {"PlayerID", new string[] {"Player ID: {0}", "Játékos ID: {0}"}},
        {"ScoreError", new string[] {"Score updating error...", "Hiba a pontszám frissítésekor..."}},
        {"TargetScore", new string[] {"Target Game Score", "Célzó Játék Eredmény"}},
        {"DestroyedTargets", new string[] {"Destroyed Targets: {0}/{1}", "Eltalált Célpontok: {0}/{1}"}}
    };

    void Awake()
    {
        dbManager = FindObjectOfType<SQLiteDBScript>();
        locManager = FindObjectOfType<LoacalisationManagerScript>();

        // Ellenőrizzük, hogy van-e érvényes játékos azonosító
        if (dbManager != null && dbManager.GetCurrentPlayerID() <= 0)
        {
            if (PlayerPrefs.HasKey("CurrentPlayerID"))
            {
                int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
                dbManager.SetCurrentPlayerID(savedPlayerID);
            }
        }

        ResetScoresBasedOnLevel();
    }

    void Start()
    {
        InitializeUITexts();
        UpdateAllScores();

        // Feliratkozás a Simon pontszám változásokra
        if (sm_scr != null)
        {
            sm_scr.onScoreChanged.AddListener(OnSimonScoreChanged);
            sm_scr.onHighScoreChanged.AddListener(OnSimonHighScoreChanged);
        }

        // Új munkamenetet indítunk, hogy elkülönítsük a játékokat
        if (dbManager != null)
        {
            dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            // Alaphelyzetbe állítjuk a számlálókat
            lastSavedShotIndex = -1;
            lastSavedTargetShotIndex = -1;
        }

        // Debug log - ellenőrizzük, hogy a ScoreManager megtalálta-e az összes szükséges komponenst
        Debug.Log($"ScoreManager initialized with: objectSpawner={objectSpawner != null}, targetObjectSpawner={targetObjectSpawner != null}, gunscript={gunscript != null}, dbManager={dbManager != null}, locManager={locManager != null}");
    }

    // Lokalizált szöveg lekérése
    private string GetLocalizedText(string key, params object[] args)
    {
        int langIndex = 0; // Alapértelmezetten angol

        if (locManager != null)
        {
            langIndex = locManager.getLocal();

            // Ellenőrizzük, hogy érvényes index-e (0 = angol, 1 = magyar)
            if (langIndex < 0 || langIndex > 1)
            {
                langIndex = 0; // Fallback angol nyelvre
            }
        }


        // Ellenőrizzük, hogy létezik-e a kulcs a szótárban
        if (localizedTexts.TryGetValue(key, out string[] texts) && langIndex < texts.Length)
        {
            return string.Format(texts[langIndex], args);
        }

        return $"[Missing:{key}]"; // Hiányzó kulcs jelzése
    }

    // Simon pontszám változás kezelése
    private void OnSimonScoreChanged(int newScore)
    {
        if (simonCurrentScore != null)
        {
            simonCurrentScore.SetText(GetLocalizedText("CurrentScore", newScore));
        }
    }

    // Simon highscore változás kezelése
    private void OnSimonHighScoreChanged(int newHighScore)
    {
        UpdateSimonScore();
    }

    private void ResetScoresBasedOnLevel()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        // A jelenlegi szint előtti játékok eredményeit megtartjuk,
        // a mostani és következő játékok eredményeit nullázzuk

        if (currentLevel <= 0) // Simon játék
        {
            if (sm_scr != null)
                sm_scr.ResetScore();
        }

        if (currentLevel <= 1) // Target játék (ObjectSpawner)
        {
            if (targetObjectSpawner != null)
            {
                targetObjectSpawner.hit_times?.Clear();
                targetObjectSpawner.hitPlace_fromMiddle?.Clear();
                targetObjectSpawner.destroyedTargets = 0;
            }
            targetShootingTimes.Clear();
            targetHitPositions.Clear();
            targetBestTime = double.MaxValue;
            targetBestTimeIndex = -1;
            lastSavedTargetShotIndex = -1; // Fontos: ez egy statikus változó, alaphelyzetbe kell állítani
        }

        if (currentLevel <= 2) // Labirintus
        {
            if (maze_scr != null)
                maze_scr.ResetScore();
        }

        if (currentLevel <= 3) // Lövöldözős játék
        {
            if (objectSpawner != null)
            {
                objectSpawner.hit_times?.Clear();
                objectSpawner.hitPlace_fromMiddle?.Clear();
            }
            shootingTimes.Clear();
            hitPositions.Clear();
            bestTime = double.MaxValue;
            bestTimeIndex = -1;
            hitpoints.Clear();
            lastSavedShotIndex = -1; // Fontos: ez egy statikus változó, alaphelyzetbe kell állítani
        }
    }

    private void InitializeUITexts()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        if (shootingScore != null)
            shootingScore.text = currentLevel > 3 ? shootingScore.text : GetLocalizedText("NoHits");

        if (targetScore != null)
            targetScore.text = currentLevel > 1 ? targetScore.text : GetLocalizedText("TargetScore");

        if (simonScore != null)
            simonScore.text = currentLevel > 0 ? simonScore.text : GetLocalizedText("SimonHighScore", 0);

        if (simonCurrentScore != null)
            simonCurrentScore.text = currentLevel > 0 ? simonCurrentScore.text : GetLocalizedText("CurrentScore", 0);

        if (mazeScore != null)
            mazeScore.text = currentLevel > 2 ? mazeScore.text : GetLocalizedText("MazeScore", "0:00.00");
    }

    void Update()
    {
        // ObjectSpawner_1place (lövöldözős játék) követése
        if (objectSpawner != null && objectSpawner.hit_times != null && shootingScore != null)
        {
            // Ellenőrizzük, hogy változott-e a találatok száma
            if (objectSpawner.hit_times.Count != shootingTimes.Count)
            {
                UpdateShootingScores();
            }

            // Ellenőrizzük a hitPlace_fromMiddle listát is
            if (objectSpawner.hitPlace_fromMiddle != null &&
                objectSpawner.hitPlace_fromMiddle.Count != hitPositions.Count)
            {
                UpdateShootingScores();
            }
        }

        // ObjectSpawner (target játék) követése - kiegészítve jobb debug információkkal
        if (targetObjectSpawner != null && targetObjectSpawner.hit_times != null && targetScore != null)
        {

            // Ellenőrizzük, hogy változott-e a találatok száma
            if (targetObjectSpawner.hit_times.Count != targetShootingTimes.Count)
            {
                UpdateTargetScores();
            }

            // Ellenőrizzük a hitPlace_fromMiddle listát is
            if (targetObjectSpawner.hitPlace_fromMiddle != null &&
                targetObjectSpawner.hitPlace_fromMiddle.Count != targetHitPositions.Count)
            {
                UpdateTargetScores();
            }

            // Ellenőrizzük a megsemmisített targeteket is
            if (targetObjectSpawner.destroyedTargets > 0)
            {
                UpdateTargetScores();
            }
        }
    }

    public void SwitchGameType()
    {
        if (dbManager != null)
        {
            dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            Debug.Log("Started new session for game type change");
            lastSavedShotIndex = -1;
            lastSavedTargetShotIndex = -1;
        }
    }

    private void UpdateAllScores()
    {
        UpdateSimonScore();
        UpdateTargetScores();  // Új metódus a target játék pontozásához
        UpdateMazeScore();
        UpdateShootingScores();
    }

    private void UpdateSimonScore()
    {
        if (simonScore != null && sm_scr != null)
        {
            int currentHighScore = sm_scr.GetHighScore();
            simonScore.SetText(GetLocalizedText("SimonHighScore", currentHighScore));

            // Aktuális pontszám frissítése
            if (simonCurrentScore != null)
            {
                int currentScore = sm_scr.GetCurrentScore();
                simonCurrentScore.SetText(GetLocalizedText("CurrentScore", currentScore));
            }

            // Adatbázis frissítése csak ha teljesítette a játékot
            if (dbManager != null && FindObjectOfType<SimonGameManager>() != null &&
                FindObjectOfType<SimonGameManager>().isEnded)
            {
                // Ellenőrizzük hogy van-e érvényes játékos azonosító
                if (dbManager.GetCurrentPlayerID() <= 0 && PlayerPrefs.HasKey("CurrentPlayerID"))
                {
                    int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
                    dbManager.SetCurrentPlayerID(savedPlayerID);
                }

                if (dbManager.GetCurrentPlayerID() > 0)
                {
                    dbManager.UpdateSimonScore(currentHighScore);
                }
            }
        }
    }

    private void UpdateTargetScores()
    {

        // Csak akkor frissítünk, ha létezik targetObjectSpawner
        if (targetObjectSpawner == null)
        {
            return;
        }

        // Csak akkor frissítünk, ha léteznek adatok
        if (targetObjectSpawner.hit_times == null || targetObjectSpawner.hit_times.Count == 0)
        {
            return;
        }

        bool updateUI = (targetScore != null);

        // Az adatok másolása lokális listákba
        targetShootingTimes = new List<double>(targetObjectSpawner.hit_times);
        targetHitPositions = new List<string>(targetObjectSpawner.hitPlace_fromMiddle);


        // UI frissítés
        if (updateUI)
        {
            StringBuilder displayText = new StringBuilder();
            displayText.AppendLine(GetLocalizedText("TargetScore"));
            displayText.AppendLine(GetLocalizedText("DestroyedTargets", targetObjectSpawner.destroyedTargets, targetObjectSpawner.numberToSpawn));

            if (targetShootingTimes.Count > 0)
            {
                double currentBestTime = targetShootingTimes.Min();
                if (currentBestTime < targetBestTime)
                {
                    targetBestTime = currentBestTime;
                    targetBestTimeIndex = targetShootingTimes.IndexOf(targetBestTime);
                }

                // Best shot information
                displayText.AppendLine("\n" + GetLocalizedText("BestShot"));
                displayText.AppendLine(GetLocalizedText("Time", targetBestTime));

                if (targetBestTimeIndex >= 0 && targetBestTimeIndex < targetHitPositions.Count)
                {
                    displayText.AppendLine(GetLocalizedText("Position", targetHitPositions[targetBestTimeIndex]));
                }
                else
                {
                    displayText.AppendLine(GetLocalizedText("PositionNA"));
                    Debug.LogWarning($"Best target shot position unavailable. targetBestTimeIndex={targetBestTimeIndex}, targetHitPositions.Count={targetHitPositions.Count}");
                }

                // Last shot information
                displayText.AppendLine("\n" + GetLocalizedText("LastShot"));
                displayText.AppendLine(GetLocalizedText("Time", targetShootingTimes[targetShootingTimes.Count - 1]));

                if (targetHitPositions.Count > 0)
                {
                    displayText.AppendLine(GetLocalizedText("Position", targetHitPositions[targetHitPositions.Count - 1]));
                }
                else
                {
                    displayText.AppendLine(GetLocalizedText("PositionNA"));
                }

                displayText.AppendLine("\n" + GetLocalizedText("TotalHits", targetShootingTimes.Count));

                if (dbManager != null)
                {
                    displayText.AppendLine(GetLocalizedText("SessionID", dbManager.GetCurrentShootingSessionID()));
                    displayText.AppendLine(GetLocalizedText("PlayerID", dbManager.GetCurrentPlayerID()));
                }
            }

            targetScore.SetText(displayText.ToString());
        }

        // Adatbázis frissítés - csak új lövéseket mentünk
        if (dbManager != null && targetShootingTimes.Count > 0)
        {
            // Ellenőrizzük, hogy van-e új mentendő lövés
            int newShotsCount = targetShootingTimes.Count - (lastSavedTargetShotIndex + 1);
            if (newShotsCount > 0)
            {

                // Csak az új lövéseken megyünk végig
                for (int i = lastSavedTargetShotIndex + 1; i < targetShootingTimes.Count; i++)
                {
                    if (i >= 0 && i < targetHitPositions.Count)
                    {
                        string[] coordinates = targetHitPositions[i].Split('|');
                        if (coordinates.Length == 2)
                        {
                            if (double.TryParse(coordinates[0], out double hitX) &&
                                double.TryParse(coordinates[1], out double hitY))
                            {

                                // Az új UpdateTargetScore metódust használjuk
                                bool success = dbManager.UpdateTargetScore(i + 1, targetShootingTimes[i], hitX, hitY);
                                if (success)
                                {
                                    lastSavedTargetShotIndex = i;
                                }

                            }

                        }

                    }

                }
            }
        }

    }


    private void UpdateMazeScore()
    {
        if (mazeScore != null && maze_scr != null)
        {
            // Format the time for display
            string formattedTime = string.Format("{0:mm\\:ss\\.ff}", maze_scr.score_time);
            mazeScore.SetText(GetLocalizedText("MazeScore", formattedTime));

            // Calculate time in minutes for database (original format)
            double currentTime = maze_scr.score_time.TotalMinutes;

            // Check if we have a valid time
            if (dbManager != null && maze_scr.score_time.TotalSeconds > 0)
            {
                // Pass both the numeric value and the formatted string
                dbManager.UpdateMazeTime(currentTime, formattedTime);
            }
        }
    }

    private void UpdateShootingScores()
    {

        if (objectSpawner == null)
        {
            return;
        }

        if (objectSpawner.hit_times == null || objectSpawner.hit_times.Count == 0)
        {
            if (shootingScore != null)
            {
                shootingScore.SetText(GetLocalizedText("NoHits"));
            }
            return;
        }

        bool updateUI = (shootingScore != null);

        try
        {
            // Adatok másolása és ellenőrzése
            shootingTimes = new List<double>(objectSpawner.hit_times);

            if (objectSpawner.hitPlace_fromMiddle != null)
            {
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
            }
            else
            {
                return;
            }

            // Legjobb idő frissítése
            double currentBestTime = shootingTimes.Min();
            if (currentBestTime < bestTime)
            {
                bestTime = currentBestTime;
                bestTimeIndex = shootingTimes.IndexOf(bestTime);
                Debug.Log($"New best time: {bestTime} at index {bestTimeIndex}");
            }

            // Format the display text with proper position values (csak ha van UI)
            if (updateUI)
            {
                StringBuilder displayText = new StringBuilder();
                displayText.AppendLine(GetLocalizedText("BestShot"));
                displayText.AppendLine(GetLocalizedText("Time", bestTime));

                if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
                {
                    displayText.AppendLine(GetLocalizedText("Position", hitPositions[bestTimeIndex]));
                }
                else
                {
                    displayText.AppendLine(GetLocalizedText("PositionNA"));
                }

                displayText.AppendLine("\n" + GetLocalizedText("LastShot"));
                displayText.AppendLine(GetLocalizedText("Time", shootingTimes[shootingTimes.Count - 1]));

                if (hitPositions.Count > 0)
                {
                    displayText.AppendLine(GetLocalizedText("Position", hitPositions[hitPositions.Count - 1]));
                }
                else
                {
                    displayText.AppendLine(GetLocalizedText("PositionNA"));
                }

                displayText.AppendLine("\n" + GetLocalizedText("TotalHits", shootingTimes.Count));

                if (dbManager != null)
                {
                    displayText.AppendLine(GetLocalizedText("SessionID", dbManager.GetCurrentShootingSessionID()));
                    displayText.AppendLine(GetLocalizedText("PlayerID", dbManager.GetCurrentPlayerID()));
                }

                shootingScore.SetText(displayText.ToString());
            }

            // Update database with all shots not yet saved
            if (dbManager != null)
            {
                // Ellenőrizzük, hogy van-e új mentendő lövés
                int newShotsCount = shootingTimes.Count - (lastSavedShotIndex + 1);
                if (newShotsCount > 0)
                {

                    // Csak az új lövéseken megyünk végig
                    for (int i = lastSavedShotIndex + 1; i < shootingTimes.Count; i++)
                    {
                        if (i >= 0 && i < hitPositions.Count)
                        {
                            string[] coordinates = hitPositions[i].Split('|');
                            if (coordinates.Length == 2)
                            {
                                if (double.TryParse(coordinates[0], out double hitX) &&
                                    double.TryParse(coordinates[1], out double hitY))
                                {
                                    Debug.Log($"Sending to database: shooting shot {i + 1}, time={shootingTimes[i]}, X={hitX}, Y={hitY}");

                                    // Itt meghívjuk a megfelelő játéktípussal az UpdateShootingScore-t
                                    bool success = dbManager.UpdateShootingScore(i + 1, shootingTimes[i], hitX, hitY);
                                    if (success)
                                    {
                                        lastSavedShotIndex = i;
                                    }

                                }

                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateShootingScores: {e.Message}\n{e.StackTrace}");
            if (shootingScore != null)
            {
                shootingScore.SetText(GetLocalizedText("ScoreError"));
            }
        }
    }

    // Teszt metódus a public API-hoz
    public void TestTargetScoreUpdate()
    {
        if (targetObjectSpawner != null && targetObjectSpawner.hit_times != null)
        {
            UpdateTargetScores();
        }
    }

    // Ez a metódus meghívható, amikor új játékos játszik
    public void StartNewShootingSession()
    {
        if (dbManager != null)
        {
            int currentPlayerID = dbManager.GetCurrentPlayerID();
            if (currentPlayerID > 0)
            {
                dbManager.StartNewShootingSession(currentPlayerID);
                Debug.Log($"Started new shooting session for player {currentPlayerID}");

                // Visszaállítjuk a mentett lövések számlálóit új session esetén
                lastSavedShotIndex = -1;
                lastSavedTargetShotIndex = -1;
            }
        }
    }

    public void OnLevelChanged(int newLevel)
    {

        // Új session indítása a tiszta elkülönítés érdekében
        if (dbManager != null)
        {
            dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            lastSavedShotIndex = -1;
            lastSavedTargetShotIndex = -1;
        }
    }

    // Ha egy új játékos választotta ki a játékot, ezt a metódust kell meghívni
    public void OnNewPlayerSelected(int playerID)
    {
        if (dbManager != null)
        {
            dbManager.SetCurrentPlayerID(playerID);

            // Új játékos esetén új session, visszaállítjuk a számlálókat
            lastSavedShotIndex = -1;
            lastSavedTargetShotIndex = -1;
        }
    }

    public void RefreshScores()
    {
        UpdateAllScores();
    }

    public void RestartCurrentGame()
    {
        ResetScoresBasedOnLevel();
        InitializeUITexts();
        UpdateAllScores();
    }
}