
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
        InvokeRepeating("CheckLevelChanges", 0.5f, 0.5f);

    }

    //újj kód CheckLevelChanges
    private int lastCheckedLevel = -1;

    private void CheckLevelChanges()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        if (lastCheckedLevel != currentLevel)
        {
            Debug.Log($"Szintváltás észlelve: {lastCheckedLevel} -> {currentLevel}");

            // Ha szintváltás történt, indítsunk új session-t
            if (dbManager != null)
            {
                dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
                Debug.Log($"Új session indítva a {currentLevel}. szinthez");
            }

            // Nullázzuk a számlálókat, hogy ne frissüljön semmi a régi adatokból
            switch (currentLevel)
            {
                case 1: // Target játék (2. szint)
                    lastSavedShotIndex = -1;
                    shootingTimes.Clear();
                    hitPositions.Clear();
                    break;

                case 2: // Shooting játék (3. szint)
                    lastSavedTargetShotIndex = -1;
                    targetShootingTimes.Clear();
                    targetHitPositions.Clear();
                    break;
            }

            lastCheckedLevel = currentLevel;
        }
    }

    //új kód vége

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
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        // KIZÁRÓLAG a megfelelő szinten frissítünk
        if (currentLevel == 1) // Target játék (2. szint)
        {
            if (targetObjectSpawner != null && targetObjectSpawner.hit_times != null)
            {
                // Csak akkor frissítünk, ha változás van
                if (targetObjectSpawner.hit_times.Count != targetShootingTimes.Count ||
                    (targetObjectSpawner.hitPlace_fromMiddle != null &&
                     targetObjectSpawner.hitPlace_fromMiddle.Count != targetHitPositions.Count))
                {
                    // Explicit Target játék frissítés
                    Debug.Log("Target játék adatainak frissítése (2. szint)");
                    UpdateTargetScores();

                    // Biztosítjuk, hogy ne kerüljön Shooting adat mentésre
                    shootingTimes.Clear();
                    hitPositions.Clear();
                    lastSavedShotIndex = -1;
                }
            }
        }
        else if (currentLevel == 2) // Shooting játék (3. szint)
        {
            if (objectSpawner != null && objectSpawner.hit_times != null)
            {
                // Csak akkor frissítünk, ha változás van
                if (objectSpawner.hit_times.Count != shootingTimes.Count ||
                    (objectSpawner.hitPlace_fromMiddle != null &&
                     objectSpawner.hitPlace_fromMiddle.Count != hitPositions.Count))
                {
                    // Explicit Shooting játék frissítés
                    Debug.Log("Shooting játék adatainak frissítése (3. szint)");
                    UpdateShootingScores();

                    // Biztosítjuk, hogy ne kerüljön Target adat mentésre
                    targetShootingTimes.Clear();
                    targetHitPositions.Clear();
                    lastSavedTargetShotIndex = -1;
                }
            }
        }
    }

    public void SwitchGameType()
    {
        if (dbManager != null)
        {
            // Reset counters for the new game type
            lastSavedShotIndex = -1;
            lastSavedTargetShotIndex = -1;

            // Start a new session for the new game type
            dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
            Debug.Log("Started new session for game type change");
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

        // Az adatok másolása lokális listákba - Klónozzuk a listákat, hogy ne módosítsuk az eredetit
        targetShootingTimes = new List<double>(targetObjectSpawner.hit_times);

        if (targetObjectSpawner.hitPlace_fromMiddle != null)
        {
            targetHitPositions = new List<string>(targetObjectSpawner.hitPlace_fromMiddle);
        }
        else
        {
            targetHitPositions = new List<string>();
        }

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
                }

                // Last shot information
                if (targetShootingTimes.Count > 0)
                {
                    displayText.AppendLine("\n" + GetLocalizedText("LastShot"));
                    displayText.AppendLine(GetLocalizedText("Time", targetShootingTimes[targetShootingTimes.Count - 1]));

                    if (targetHitPositions.Count > 0 && targetHitPositions.Count == targetShootingTimes.Count)
                    {
                        displayText.AppendLine(GetLocalizedText("Position", targetHitPositions[targetHitPositions.Count - 1]));
                    }
                    else
                    {
                        displayText.AppendLine(GetLocalizedText("PositionNA"));
                    }
                }

                // Javítás: Explicit módon a valós találatok számát jelenítjük meg a Target játéknál
                // Amikor a 2. szinten vagyunk (ami a kódban 1), akkor a tényleges destroyedTargets értéket használjuk
                if (NextGameColliderScript.GetCurrentLevel() == 1)
                {
                    // Ha a destroyedTargets 0, de már van találat, akkor a találatok számát használjuk
                    int displayedHits = targetObjectSpawner.destroyedTargets;
                    if (displayedHits == 0 && targetShootingTimes.Count > 0)
                    {
                        displayedHits = targetShootingTimes.Count;
                    }
                    displayText.AppendLine("\n" + GetLocalizedText("TotalHits", displayedHits));
                }
                else
                {
                    displayText.AppendLine("\n" + GetLocalizedText("TotalHits", targetShootingTimes.Count));
                }

                if (dbManager != null)
                {
                    displayText.AppendLine(GetLocalizedText("SessionID", dbManager.GetCurrentShootingSessionID()));
                    displayText.AppendLine(GetLocalizedText("PlayerID", dbManager.GetCurrentPlayerID()));
                }
            }

            targetScore.SetText(displayText.ToString());
        }

        // Adatbázis frissítés - csak új lövéseket mentünk
        if (dbManager != null && targetShootingTimes.Count > 0 && NextGameColliderScript.GetCurrentLevel() == 1)
        {
            // Ellenőrizzük, hogy van-e új mentendő lövés
            int newShotsCount = targetShootingTimes.Count - (lastSavedTargetShotIndex + 1);
            if (newShotsCount > 0)
            {
                Debug.Log($"Target játék: {newShotsCount} új lövés mentése az adatbázisba");

                // Csak az új lövéseken megyünk végig
                for (int i = lastSavedTargetShotIndex + 1; i < targetShootingTimes.Count; i++)
                {
                    string hitPosition = "0|0"; // Default érték
                    if (i < targetHitPositions.Count)
                    {
                        hitPosition = targetHitPositions[i];
                    }

                    string[] coordinates = hitPosition.Split('|');
                    if (coordinates.Length == 2)
                    {
                        if (double.TryParse(coordinates[0], out double hitX) &&
                            double.TryParse(coordinates[1], out double hitY))
                        {
                            Debug.Log($"Target lövés mentése: idx={i + 1}, idő={targetShootingTimes[i]}, X={hitX}, Y={hitY}");

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
            // Adatok másolása és ellenőrzése - klónozzuk, hogy ne módosítsuk az eredetit
            shootingTimes = new List<double>(objectSpawner.hit_times);

            if (objectSpawner.hitPlace_fromMiddle != null)
            {
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
            }
            else
            {
                hitPositions = new List<string>();
                // If we don't have hit positions but we have hit times, we'll use default positions
                for (int i = 0; i < shootingTimes.Count; i++)
                {
                    hitPositions.Add("0|0");
                }
            }

            // Legjobb idő frissítése
            if (shootingTimes.Count > 0)
            {
                double currentBestTime = shootingTimes.Min();
                if (currentBestTime < bestTime)
                {
                    bestTime = currentBestTime;
                    bestTimeIndex = shootingTimes.IndexOf(bestTime);
                    Debug.Log($"New best shooting time: {bestTime} at index {bestTimeIndex}");
                }
            }

            // Format the display text with proper position values (csak ha van UI)
            if (updateUI)
            {
                StringBuilder displayText = new StringBuilder();
                displayText.AppendLine(GetLocalizedText("BestShot"));

                if (bestTimeIndex >= 0)
                {
                    displayText.AppendLine(GetLocalizedText("Time", bestTime));

                    if (bestTimeIndex < hitPositions.Count)
                    {
                        displayText.AppendLine(GetLocalizedText("Position", hitPositions[bestTimeIndex]));
                    }
                    else
                    {
                        displayText.AppendLine(GetLocalizedText("PositionNA"));
                    }
                }
                else
                {
                    displayText.AppendLine(GetLocalizedText("Time", 0));
                    displayText.AppendLine(GetLocalizedText("PositionNA"));
                }

                if (shootingTimes.Count > 0)
                {
                    displayText.AppendLine("\n" + GetLocalizedText("LastShot"));
                    displayText.AppendLine(GetLocalizedText("Time", shootingTimes[shootingTimes.Count - 1]));

                    if (hitPositions.Count > 0 && hitPositions.Count >= shootingTimes.Count)
                    {
                        displayText.AppendLine(GetLocalizedText("Position", hitPositions[hitPositions.Count - 1]));
                    }
                    else
                    {
                        displayText.AppendLine(GetLocalizedText("PositionNA"));
                    }
                }

                displayText.AppendLine("\n" + GetLocalizedText("TotalHits", shootingTimes.Count));

                if (dbManager != null)
                {
                    displayText.AppendLine(GetLocalizedText("SessionID", dbManager.GetCurrentShootingSessionID()));
                    displayText.AppendLine(GetLocalizedText("PlayerID", dbManager.GetCurrentPlayerID()));
                }

                shootingScore.SetText(displayText.ToString());
            }

            // Update database with all shots not yet saved - only for this game type
            if (dbManager != null && shootingTimes.Count > 0 && NextGameColliderScript.GetCurrentLevel() == 3)
            {
                // Ellenőrizzük, hogy van-e új mentendő lövés
                int newShotsCount = shootingTimes.Count - (lastSavedShotIndex + 1);
                if (newShotsCount > 0)
                {
                    Debug.Log($"Saving {newShotsCount} new shooting shots to database");

                    // Start a new session if needed - ONLY for Shooting game type
                    if (dbManager.GetCurrentShootingSessionID() <= 0)
                    {
                        dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
                    }

                    // Csak az új lövéseken megyünk végig
                    for (int i = lastSavedShotIndex + 1; i < shootingTimes.Count; i++)
                    {
                        string hitPosition = "0|0"; // Default value
                        if (i < hitPositions.Count)
                        {
                            hitPosition = hitPositions[i];
                        }

                        string[] coordinates = hitPosition.Split('|');
                        if (coordinates.Length == 2)
                        {
                            if (double.TryParse(coordinates[0], out double hitX) &&
                                double.TryParse(coordinates[1], out double hitY))
                            {
                                Debug.Log($"Saving Shooting shot {i + 1}: time={shootingTimes[i]}, x={hitX}, y={hitY}");

                                // EXPLICITLY use UpdateShootingScore
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
        Debug.Log($"Level changed to {newLevel} - Starting new session and resetting counters");

        // Reset the appropriate counters based on level
        if (newLevel == 2) // Target game
        {
            // Reset Target game data
            lastSavedTargetShotIndex = -1;
            targetShootingTimes.Clear();
            targetHitPositions.Clear();

            // Explicitly ignore shooting data
            shootingTimes.Clear();
            hitPositions.Clear();
            lastSavedShotIndex = -1;
        }
        else if (newLevel == 3) // Shooting game
        {
            // Reset Shooting game data
            lastSavedShotIndex = -1;
            shootingTimes.Clear();
            hitPositions.Clear();

            // Explicitly ignore target data
            targetShootingTimes.Clear();
            targetHitPositions.Clear();
            lastSavedTargetShotIndex = -1;
        }

        // Új session indítása a tiszta elkülönítés érdekében
        if (dbManager != null)
        {
            dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
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

    public void ResetGameTypeData(int gameLevel)
    {
        Debug.Log($"Játéktípus adatainak resetelése: szint={gameLevel}");

        // Csak akkor hozunk létre új session-t, ha már van adatunk
        SQLiteDBScript dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager != null && dbManager.GetCurrentShootingSessionID() > 0)
        {
            // Ellenőrizzük, van-e már adat a jelenlegi session-ben
            if (dbManager.HasDataInCurrentSession())
            {
                // Csak akkor indítunk új session-t, ha már van adat a régi session-ben
                dbManager.StartNewShootingSession(dbManager.GetCurrentPlayerID());
                Debug.Log("Új session létrehozva, mert a régi session-ben már van adat");
            }
            else
            {
                Debug.Log("Meglévő üres session újrahasznosítva");
            }
        }

        // A szint alapján reseteljük a megfelelő adatokat
        if (gameLevel == 1) // Target játék
        {
            lastSavedTargetShotIndex = -1;
            targetShootingTimes.Clear();
            targetHitPositions.Clear();

            // Megakadályozzuk a Shooting adatok frissítését
            lastSavedShotIndex = -1;
            shootingTimes.Clear();
            hitPositions.Clear();
        }
        else if (gameLevel == 2) // Shooting játék
        {
            lastSavedShotIndex = -1;
            shootingTimes.Clear();
            hitPositions.Clear();

            // Megakadályozzuk a Target adatok frissítését
            lastSavedTargetShotIndex = -1;
            targetShootingTimes.Clear();
            targetHitPositions.Clear();
        }
    }

}