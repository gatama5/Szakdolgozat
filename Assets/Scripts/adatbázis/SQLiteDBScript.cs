
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class SQLiteDBScript : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI playerIdText;

    private string connectionString;
    private int currentPlayerID = 0;
    private int currentShootingSessionID = 0;

    // Jбtйktнpus konstansok
    public const string GAME_TYPE_TARGET = "Target";
    public const string GAME_TYPE_SHOOTING = "Shooting";

    // This will help track if we have a properly initialized database connection
    private bool isDatabaseInitialized = false;

    void Awake()
    {
        // Az adatbázis keresztül vitelének biztosítása
        DontDestroyOnLoad(this.gameObject);

        // Az adatbázis fájl elérési útja
        string dbPath = Application.persistentDataPath + "/game_scores.db";
        connectionString = "URI=file:" + dbPath;

        //Debug.Log("Database path: " + dbPath);

        // Adatbázis inicializálása
        InitializeDatabase();

        // PlayerPrefs-ből az azonosító visszaállítása, ha van
        if (PlayerPrefs.HasKey("CurrentPlayerID"))
        {
            int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
            if (savedPlayerID > 0)
            {
                SetCurrentPlayerID(savedPlayerID);
            }
        }

        // HOZZÁADNI: Frissítsük a TextMeshPro mezőt az aktuális ID-vel
        UpdatePlayerIdText();
    }

    void InitializeDatabase()
    {
        try
        {
            // Kapcsolat létrehozása
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Players tábla létrehozása név nélkül (vagy opcionális névvel)
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createPlayersTable = @"
                CREATE TABLE IF NOT EXISTS Players (
                    PlayerID INTEGER PRIMARY KEY,
                    Name TEXT, -- Eltávolítottuk a NOT NULL kényszert
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

                    dbCmd.CommandText = createPlayersTable;
                    dbCmd.ExecuteNonQuery();
                }

                // PlayerDetails tábla létrehozása - csak kor és generáció
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createPlayerDetailsTable = @"
                CREATE TABLE IF NOT EXISTS PlayerDetails (
                    PlayerID INTEGER PRIMARY KEY,
                    Age INTEGER,
                    Generation TEXT,
                    FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                )";

                    dbCmd.CommandText = createPlayerDetailsTable;
                    dbCmd.ExecuteNonQuery();
                }

                // SimonScores tбbla lйtrehozбsa
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createSimonTable = @"
                    CREATE TABLE IF NOT EXISTS SimonScores (
                        ScoreID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID INTEGER,
                        Score INTEGER,
                        RecordedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                    )";

                    dbCmd.CommandText = createSimonTable;
                    dbCmd.ExecuteNonQuery();
                }

                // MazeScores tбbla lйtrehozбsa
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createMazeTable = @"
                    CREATE TABLE IF NOT EXISTS MazeScores (
                        ScoreID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID INTEGER,
                        CompletionTime REAL,
                        FormattedTime TEXT,
                        RecordedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                    )";

                    dbCmd.CommandText = createMazeTable;
                    dbCmd.ExecuteNonQuery();
                }

                // ShootingSessions tбbla lйtrehozбsa (ъj)
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createSessionsTable = @"
                    CREATE TABLE IF NOT EXISTS ShootingSessions (
                        SessionID INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerID INTEGER,
                        StartedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                    )";

                    dbCmd.CommandText = createSessionsTable;
                    dbCmd.ExecuteNonQuery();
                }

                // ShootingScores tбbla lйtrehozбsa (mуdosнtott)
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createShootingTable = @"
                    CREATE TABLE IF NOT EXISTS ShootingScores (
                        ScoreID INTEGER PRIMARY KEY AUTOINCREMENT,
                        SessionID INTEGER,
                        ShotNumber INTEGER,
                        ReactionTime REAL,
                        PositionX REAL,
                        PositionY REAL,
                        GameType TEXT DEFAULT 'Shooting',
                        RecordedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(SessionID) REFERENCES ShootingSessions(SessionID)
                    )";

                    dbCmd.CommandText = createShootingTable;
                    dbCmd.ExecuteNonQuery();
                }

                // Ha a GameType oszlop mйg nem lйtezik, adjuk hozzб
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    // Ellenхrizzьk, hogy a GameType oszlop mбr lйtezik-e
                    bool gameTypeColumnExists = false;

                    try
                    {
                        dbCmd.CommandText = "SELECT GameType FROM ShootingScores LIMIT 1";
                        using (IDataReader reader = dbCmd.ExecuteReader())
                        {
                            gameTypeColumnExists = true;
                        }
                    }
                    catch (SqliteException)
                    {
                        // Az oszlop nem lйtezik, ez vбrhatу
                        gameTypeColumnExists = false;
                    }

                    // Ha nem lйtezik, adjuk hozzб
                    if (!gameTypeColumnExists)
                    {
                        try
                        {
                            dbCmd.CommandText = "ALTER TABLE ShootingScores ADD COLUMN GameType TEXT DEFAULT 'Shooting'";
                            dbCmd.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error adding GameType column: {e.Message}");
                        }
                    }
                }

                dbConnection.Close();
                isDatabaseInitialized = true;
                RemoveUnneededColumns();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database initialization error: " + e.Message);
            isDatabaseInitialized = false;
        }
    }

    // A hiбnyzу InsertPlayerData metуdus implementбciуja
    public int InsertPlayerData(int playerAge, string generation, int simonScore, double mazeTime, int shootingScore)
    {
        if (!isDatabaseInitialized)
        {
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // ID generálás új formátumban: MMDDHHMM
                string dateStr = DateTime.Now.ToString("MMdd");  // Hónap és nap (4 számjegy)
                string timeStr = DateTime.Now.ToString("HHmm");  // Óra és perc (4 számjegy)
                string idStr = dateStr + timeStr;  // MMDDHHMM (8 számjegy)

                Debug.Log($"Generated ID string: {idStr}");

                int dateTimeID;
                bool parseSuccess = int.TryParse(idStr, out dateTimeID);
                if (!parseSuccess)
                {
                    Debug.LogWarning($"Failed to parse '{idStr}' to int, using fallback");
                    // Fallback megoldás
                    dateTimeID = (int)(DateTime.Now.Ticks % 1000000000);  // 9 számjegyű maximum
                }

                Debug.Log($"Final ID: {dateTimeID}");

                // Játékos hozzáadása név nélkül
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (PlayerID) VALUES (@id)";

                    IDbDataParameter idParam = dbCmd.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = dateTimeID;
                    dbCmd.Parameters.Add(idParam);

                    dbCmd.ExecuteNonQuery();
                    currentPlayerID = dateTimeID;
                }

                // Játékos részleteinek hozzáadása - csak kor és generáció
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO PlayerDetails (PlayerID, Age, Generation) VALUES (@id, @age, @gen)";

                    IDbDataParameter idParam = dbCmd.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = currentPlayerID;
                    dbCmd.Parameters.Add(idParam);

                    IDbDataParameter ageParam = dbCmd.CreateParameter();
                    ageParam.ParameterName = "@age";
                    ageParam.Value = playerAge;
                    dbCmd.Parameters.Add(ageParam);

                    IDbDataParameter genParam = dbCmd.CreateParameter();
                    genParam.ParameterName = "@gen";
                    genParam.Value = generation;
                    dbCmd.Parameters.Add(genParam);

                    dbCmd.ExecuteNonQuery();
                }

                // Simon játék inicializáló pontszám - ONLY IF NOT ZERO
                if (simonScore > 0)
                {
                    using (IDbCommand dbCmd = dbConnection.CreateCommand())
                    {
                        dbCmd.CommandText = "INSERT INTO SimonScores (PlayerID, Score) VALUES (@id, @score)";

                        IDbDataParameter idParam = dbCmd.CreateParameter();
                        idParam.ParameterName = "@id";
                        idParam.Value = currentPlayerID;
                        dbCmd.Parameters.Add(idParam);

                        IDbDataParameter scoreParam = dbCmd.CreateParameter();
                        scoreParam.ParameterName = "@score";
                        scoreParam.Value = simonScore;
                        dbCmd.Parameters.Add(scoreParam);

                        dbCmd.ExecuteNonQuery();
                    }
                }

                // Labirintus játék inicializáló idő - ONLY IF NOT ZERO
                if (mazeTime > 0)
                {
                    string formattedTime = string.Format("{0:mm\\:ss\\.ff}", TimeSpan.FromMinutes(mazeTime));
                    using (IDbCommand dbCmd = dbConnection.CreateCommand())
                    {
                        dbCmd.CommandText = "INSERT INTO MazeScores (PlayerID, CompletionTime, FormattedTime) VALUES (@id, @time, @formatted)";

                        IDbDataParameter idParam = dbCmd.CreateParameter();
                        idParam.ParameterName = "@id";
                        idParam.Value = currentPlayerID;
                        dbCmd.Parameters.Add(idParam);

                        IDbDataParameter timeParam = dbCmd.CreateParameter();
                        timeParam.ParameterName = "@time";
                        timeParam.Value = mazeTime;
                        dbCmd.Parameters.Add(timeParam);

                        IDbDataParameter formattedParam = dbCmd.CreateParameter();
                        formattedParam.ParameterName = "@formatted";
                        formattedParam.Value = formattedTime;
                        dbCmd.Parameters.Add(formattedParam);

                        dbCmd.ExecuteNonQuery();
                    }
                }

                // Lövöldözős játék session létrehozása
                currentShootingSessionID = StartNewShootingSession(currentPlayerID);

                // Az új ID mentése a PlayerPrefs-be is
                PlayerPrefs.SetInt("CurrentPlayerID", currentPlayerID);
                PlayerPrefs.Save();

                return currentPlayerID;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error inserting player data: {e.Message}");
            return -1;
        }
    }

    public int AddNewPlayer()
    {
        if (!isDatabaseInitialized)
        {
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // ID generálás új formátumban: MMDDHHMM
                string dateStr = DateTime.Now.ToString("MMdd");  // Hónap és nap (4 számjegy)
                string timeStr = DateTime.Now.ToString("HHmm");  // Óra és perc (4 számjegy)
                string idStr = dateStr + timeStr;  // MMDDHHMM (8 számjegy)

                int dateTimeID;
                if (!int.TryParse(idStr, out dateTimeID))
                {
                    // Fallback megoldás
                    dateTimeID = (int)(DateTime.Now.Ticks % 1000000000);  // 9 számjegyű maximum
                }

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (PlayerID) VALUES (@id)";

                    IDbDataParameter idParam = dbCmd.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = dateTimeID;
                    dbCmd.Parameters.Add(idParam);

                    dbCmd.ExecuteNonQuery();
                    currentPlayerID = dateTimeID;

                    // Mentsük a PlayerPrefs-be is
                    PlayerPrefs.SetInt("CurrentPlayerID", currentPlayerID);
                    PlayerPrefs.Save();

                    // Új lövési session létrehozása az új játékoshoz
                    if (currentShootingSessionID <= 0)
                    {
                        currentShootingSessionID = StartNewShootingSession(currentPlayerID);
                        Debug.Log($"Új játékos létrehozva: ID={currentPlayerID}, session ID={currentShootingSessionID}");
                    }

                    return currentPlayerID;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error adding new player: " + e.Message);
            return -1;
        }
    }
    private void UpdatePlayerIdText()
    {
        if (playerIdText != null)
        {
            playerIdText.text = $"{currentPlayerID}";
        }
    }

    // SQLiteDBScript osztályba:
    private void RemoveUnneededColumns()
    {
        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Ellenőrizzük, hogy létezik-e a Players tábla
                bool tableExists = false;
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Players'";
                    object result = checkCmd.ExecuteScalar();
                    tableExists = result != null && result != DBNull.Value;
                }

                if (tableExists)
                {
                    // Ellenőrizzük, hogy létezik-e Name oszlop
                    bool nameColumnExists = false;
                    try
                    {
                        using (IDbCommand checkCmd = dbConnection.CreateCommand())
                        {
                            checkCmd.CommandText = "SELECT Name FROM Players LIMIT 1";
                            using (IDataReader reader = checkCmd.ExecuteReader())
                            {
                                nameColumnExists = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        nameColumnExists = false;
                    }

                    // Ha létezik a Name oszlop, eltávolítjuk
                    if (nameColumnExists)
                    {
                        Debug.Log("Removing Name column from Players table...");

                        // SQLite nem támogatja közvetlenül az oszlopok törlését, ezért ideiglenes táblát kell használnunk
                        using (IDbCommand dbCmd = dbConnection.CreateCommand())
                        {
                            // 1. Ideiglenes tábla létrehozása a kívánt struktúrával
                            dbCmd.CommandText = @"
                        CREATE TABLE Players_temp (
                            PlayerID INTEGER PRIMARY KEY,
                            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";
                            dbCmd.ExecuteNonQuery();

                            // 2. Adatok átmásolása az új struktúrába
                            dbCmd.CommandText = @"
                        INSERT INTO Players_temp (PlayerID, CreatedAt)
                        SELECT PlayerID, CreatedAt FROM Players";
                            dbCmd.ExecuteNonQuery();

                            // 3. Régi tábla eldobása
                            dbCmd.CommandText = "DROP TABLE Players";
                            dbCmd.ExecuteNonQuery();

                            // 4. Ideiglenes tábla átnevezése
                            dbCmd.CommandText = "ALTER TABLE Players_temp RENAME TO Players";
                            dbCmd.ExecuteNonQuery();

                            Debug.Log("Name column successfully removed from Players table");
                        }
                    }

                    // Hasonló ellenőrzés és módosítás a PlayerDetails táblában az Email oszlopra
                    bool emailColumnExists = false;
                    try
                    {
                        using (IDbCommand checkCmd = dbConnection.CreateCommand())
                        {
                            checkCmd.CommandText = "SELECT Email FROM PlayerDetails LIMIT 1";
                            using (IDataReader reader = checkCmd.ExecuteReader())
                            {
                                emailColumnExists = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        emailColumnExists = false;
                    }

                    if (emailColumnExists)
                    {
                        Debug.Log("Removing Email column from PlayerDetails table...");

                        using (IDbCommand dbCmd = dbConnection.CreateCommand())
                        {
                            // 1. Ideiglenes tábla létrehozása a kívánt struktúrával
                            dbCmd.CommandText = @"
                        CREATE TABLE PlayerDetails_temp (
                            PlayerID INTEGER PRIMARY KEY,
                            Age INTEGER,
                            Generation TEXT,
                            FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                        )";
                            dbCmd.ExecuteNonQuery();

                            // 2. Adatok átmásolása az új struktúrába
                            dbCmd.CommandText = @"
                        INSERT INTO PlayerDetails_temp (PlayerID, Age, Generation)
                        SELECT PlayerID, Age, Generation FROM PlayerDetails";
                            dbCmd.ExecuteNonQuery();

                            // 3. Régi tábla eldobása
                            dbCmd.CommandText = "DROP TABLE PlayerDetails";
                            dbCmd.ExecuteNonQuery();

                            // 4. Ideiglenes tábla átnevezése
                            dbCmd.CommandText = "ALTER TABLE PlayerDetails_temp RENAME TO PlayerDetails";
                            dbCmd.ExecuteNonQuery();

                            Debug.Log("Email column successfully removed from PlayerDetails table");
                        }
                    }
                }

                dbConnection.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error removing columns: {e.Message}");
        }
    }

    public int StartNewShootingSession(int playerID)
    {
        // Ellenőrizzük a paramétereket
        if (!isDatabaseInitialized || playerID <= 0)
        {
            Debug.LogError("Nem lehet új session-t indítani: érvénytelen paraméterek");
            return -1;
        }

        try
        {
            // Debug infó
            Debug.Log($"StartNewShootingSession hívva: játékos ID = {playerID}, jelenlegi session = {currentShootingSessionID}");

            // Ellenőrizzük, hogy van-e már aktív session ehhez a játékoshoz
            if (currentShootingSessionID > 0)
            {
                using (IDbConnection dbConnection = new SqliteConnection(connectionString))
                {
                    dbConnection.Open();

                    // Ellenőrizzük a meglévő session-t
                    using (IDbCommand checkCmd = dbConnection.CreateCommand())
                    {
                        checkCmd.CommandText = "SELECT COUNT(*) FROM ShootingSessions WHERE SessionID = @sessionID AND PlayerID = @playerID";

                        IDbDataParameter sessionParam = checkCmd.CreateParameter();
                        sessionParam.ParameterName = "@sessionID";
                        sessionParam.Value = currentShootingSessionID;
                        checkCmd.Parameters.Add(sessionParam);

                        IDbDataParameter playerParam = checkCmd.CreateParameter();
                        playerParam.ParameterName = "@playerID";
                        playerParam.Value = playerID;
                        checkCmd.Parameters.Add(playerParam);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        // Ha a session a jelenlegi játékoshoz tartozik és érvényes, használjuk azt
                        if (count > 0)
                        {
                            Debug.Log($"Meglévő session újrafelhasználása: ID={currentShootingSessionID}");
                            return currentShootingSessionID;
                        }
                    }
                }
            }

            // Új session létrehozása egy játékos számára
            // Ellenőrizzük, hogy van-e már session a jelenlegi játékoshoz, amit újra lehet használni
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT SessionID FROM ShootingSessions WHERE PlayerID = @playerID ORDER BY StartedAt DESC LIMIT 1";

                    IDbDataParameter playerParam = checkCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = playerID;
                    checkCmd.Parameters.Add(playerParam);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        currentShootingSessionID = Convert.ToInt32(result);
                        Debug.Log($"Meglévő session használata a játékos számára: ID={currentShootingSessionID}");
                        return currentShootingSessionID;
                    }
                }

                // Ha nincs meglévő session, hozzunk létre újat
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO ShootingSessions (PlayerID) VALUES (@playerID); SELECT last_insert_rowid();";

                    IDbDataParameter playerParam = dbCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = playerID;
                    dbCmd.Parameters.Add(playerParam);

                    currentShootingSessionID = Convert.ToInt32(dbCmd.ExecuteScalar());
                    Debug.Log($"Új shooting session létrehozva: ID={currentShootingSessionID} Player ID={playerID}");
                    return currentShootingSessionID;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Hiba a session létrehozásakor: {e.Message}");
            return -1;
        }
    }

    // Simon jбtйk pontszбmбnak frissнtйse
    public bool UpdateSimonScore(int score)
    {
        if (!isDatabaseInitialized)
        {
            return false;
        }

        if (currentPlayerID <= 0)
        {
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // ELLENХRIZZЬK, HOGY LЙTEZIK-E MБR UGYANILYEN PONTSZБM
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM SimonScores WHERE PlayerID = @playerID AND Score = @score";

                    IDbDataParameter playerParam = checkCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = currentPlayerID;
                    checkCmd.Parameters.Add(playerParam);

                    IDbDataParameter scoreParam = checkCmd.CreateParameter();
                    scoreParam.ParameterName = "@score";
                    scoreParam.Value = score;
                    checkCmd.Parameters.Add(scoreParam);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    // Ha mбr lйtezik ilyen pontszбm, ne adjunk hozzб ъjat
                    if (count > 0)
                    {
                        return false;
                    }
                }

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO SimonScores (PlayerID, Score) VALUES (@playerID, @score)";

                    IDbDataParameter playerParam = dbCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = currentPlayerID;
                    dbCmd.Parameters.Add(playerParam);

                    IDbDataParameter scoreParam = dbCmd.CreateParameter();
                    scoreParam.ParameterName = "@score";
                    scoreParam.Value = score;
                    dbCmd.Parameters.Add(scoreParam);

                    dbCmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating Simon score: " + e.Message);
            return false;
        }
    }

    // Labirintus idejйnek frissнtйse
    public bool UpdateMazeTime(double completionTime, string formattedTime)
    {
        if (!isDatabaseInitialized)
        {
            return false;
        }

        if (currentPlayerID <= 0)
        {
            return false;
        }

        // NE ENGEDЙLYEZZЬK A NULLБS IDХKET
        if (completionTime <= 0)
        {
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // ELLENХRIZZЬK, HOGY LЙTEZIK-E MБR UGYANILYEN IDХ
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM MazeScores WHERE PlayerID = @playerID AND CompletionTime = @time";

                    IDbDataParameter playerParam = checkCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = currentPlayerID;
                    checkCmd.Parameters.Add(playerParam);

                    IDbDataParameter timeParam = checkCmd.CreateParameter();
                    timeParam.ParameterName = "@time";
                    timeParam.Value = completionTime;
                    checkCmd.Parameters.Add(timeParam);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    // Ha mбr lйtezik ilyen idх, ne adjunk hozzб ъjat
                    if (count > 0)
                    {
                        return false;
                    }
                }

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO MazeScores (PlayerID, CompletionTime, FormattedTime) VALUES (@playerID, @time, @formatted)";

                    IDbDataParameter playerParam = dbCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = currentPlayerID;
                    dbCmd.Parameters.Add(playerParam);

                    IDbDataParameter timeParam = dbCmd.CreateParameter();
                    timeParam.ParameterName = "@time";
                    timeParam.Value = completionTime;
                    dbCmd.Parameters.Add(timeParam);

                    IDbDataParameter formattedParam = dbCmd.CreateParameter();
                    formattedParam.ParameterName = "@formatted";
                    formattedParam.Value = formattedTime;
                    dbCmd.Parameters.Add(formattedParam);

                    dbCmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating Maze time: " + e.Message);
            return false;
        }
    }
    private bool UpdateShootingScore(int shotNumber, double reactionTime, double posX, double posY, string gameType)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogError("Cannot update score: Database not initialized");
            return false;
        }

        if (currentShootingSessionID <= 0)
        {
            // Attempt to restart a shooting session if we have a valid player
            if (currentPlayerID > 0)
            {
                currentShootingSessionID = StartNewShootingSession(currentPlayerID);
                if (currentShootingSessionID <= 0)
                {
                    Debug.LogError("Cannot update score: No active shooting session and failed to create new one");
                    return false;
                }
            }
            else
            {
                Debug.LogError("Cannot update score: No active shooting session");
                return false;
            }
        }

        // Ellenőrizzük, hogy ez a lövés már mentve van-e
        if (IsAlreadySaved(shotNumber, reactionTime, gameType))
        {
            Debug.Log($"Duplikált lövés kihagyása: Session={currentShootingSessionID}, Shot={shotNumber}, Type={gameType}");
            return false;
        }

        Debug.Log($"Adatok mentése: Session={currentShootingSessionID}, Shot={shotNumber}, Time={reactionTime}, X={posX}, Y={posY}, Type={gameType}");

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Ha még nem létezik, akkor adjuk hozzá
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = @"
                INSERT INTO ShootingScores (SessionID, ShotNumber, ReactionTime, PositionX, PositionY, GameType) 
                VALUES (@sessionID, @shotNumber, @time, @posX, @posY, @gameType)";

                    IDbDataParameter sessionParam = dbCmd.CreateParameter();
                    sessionParam.ParameterName = "@sessionID";
                    sessionParam.Value = currentShootingSessionID;
                    dbCmd.Parameters.Add(sessionParam);

                    IDbDataParameter shotParam = dbCmd.CreateParameter();
                    shotParam.ParameterName = "@shotNumber";
                    shotParam.Value = shotNumber;
                    dbCmd.Parameters.Add(shotParam);

                    IDbDataParameter timeParam = dbCmd.CreateParameter();
                    timeParam.ParameterName = "@time";
                    timeParam.Value = reactionTime;
                    dbCmd.Parameters.Add(timeParam);

                    IDbDataParameter posXParam = dbCmd.CreateParameter();
                    posXParam.ParameterName = "@posX";
                    posXParam.Value = posX;
                    dbCmd.Parameters.Add(posXParam);

                    IDbDataParameter posYParam = dbCmd.CreateParameter();
                    posYParam.ParameterName = "@posY";
                    posYParam.Value = posY;
                    dbCmd.Parameters.Add(posYParam);

                    IDbDataParameter gameTypeParam = dbCmd.CreateParameter();
                    gameTypeParam.ParameterName = "@gameType";
                    gameTypeParam.Value = gameType;
                    dbCmd.Parameters.Add(gameTypeParam);

                    int rowsAffected = dbCmd.ExecuteNonQuery();
                    Debug.Log($"Adatok mentve az adatbázisba: {rowsAffected} sor érintett, GameType: {gameType}");
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Hiba az adatok mentésekor: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }



    public bool UpdateTargetScore(int shotNumber, double reactionTime, double posX, double posY)
    {
        // Ne ellenőrizzük a szintet, csak a típust használjuk megfelelően
        // Minden target játék adatot a GAME_TYPE_TARGET típussal mentünk el
        return UpdateShootingScore(shotNumber, reactionTime, posX, posY, GAME_TYPE_TARGET);
    }

    public bool UpdateShootingScore(int shotNumber, double reactionTime, double posX, double posY)
    {
        // Ne ellenőrizzük a szintet, csak a típust használjuk megfelelően
        // Minden shooting játék adatot a GAME_TYPE_SHOOTING típussal mentünk el
        return UpdateShootingScore(shotNumber, reactionTime, posX, posY, GAME_TYPE_SHOOTING);
    }

    // Aktuбlis jбtйkos ID lekйrйse
    public int GetCurrentPlayerID()
    {
        return currentPlayerID;
    }

    // Aktuбlis lцvйsi session ID lekйrйse
    public int GetCurrentShootingSessionID()
    {
        return currentShootingSessionID;
    }

    // Jбtйkos ID beбllнtбsa (pl. meglйvх jбtйkoshoz valу csatlakozбskor)
    public void SetCurrentPlayerID(int playerID)
    {
        if (playerID <= 0)
        {
            Debug.LogWarning("Cannot set player ID: Invalid ID provided");
            return;
        }

        currentPlayerID = playerID;
        PlayerPrefs.SetInt("CurrentPlayerID", playerID);
        PlayerPrefs.Save();

        // Ъj lцvйsi session indнtбsa a beбllнtott jбtйkoshoz
        int sessionID = StartNewShootingSession(currentPlayerID);
        if (sessionID > 0)
        {
            currentShootingSessionID = sessionID;
        }
        else
        {
            Debug.LogWarning("Failed to start new shooting session for player ID: " + playerID);
        }
    }

    public bool IsAlreadySaved(int shotNumber, double reactionTime, string gameType)
    {
        if (!isDatabaseInitialized || currentShootingSessionID <= 0)
        {
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM ShootingScores WHERE SessionID = @sessionID AND ShotNumber = @shotNumber AND GameType = @gameType";

                    IDbDataParameter sessionParam = checkCmd.CreateParameter();
                    sessionParam.ParameterName = "@sessionID";
                    sessionParam.Value = currentShootingSessionID;
                    checkCmd.Parameters.Add(sessionParam);

                    IDbDataParameter shotParam = checkCmd.CreateParameter();
                    shotParam.ParameterName = "@shotNumber";
                    shotParam.Value = shotNumber;
                    checkCmd.Parameters.Add(shotParam);

                    IDbDataParameter gameTypeParam = checkCmd.CreateParameter();
                    gameTypeParam.ParameterName = "@gameType";
                    gameTypeParam.Value = gameType == "Target" ? GAME_TYPE_TARGET : GAME_TYPE_SHOOTING;
                    checkCmd.Parameters.Add(gameTypeParam);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Hiba az IsAlreadySaved ellenőrzés során: {e.Message}");
            return false;
        }
    }


    public bool HasDataInCurrentSession()
    {
        if (!isDatabaseInitialized || currentShootingSessionID <= 0)
        {
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM ShootingScores WHERE SessionID = @sessionID";

                    IDbDataParameter sessionParam = checkCmd.CreateParameter();
                    sessionParam.ParameterName = "@sessionID";
                    sessionParam.Value = currentShootingSessionID;
                    checkCmd.Parameters.Add(sessionParam);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Hiba a HasDataInCurrentSession ellenőrzés során: {e.Message}");
            return false;
        }
    }

}




