//using System;
//using System.Data;
//using System.IO;
//using Mono.Data.Sqlite;
//using UnityEngine;

//public class SQLiteDBScript : MonoBehaviour
//{
//    private string dbPath;

//    private void Start()
//    {
//        dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, "GameDB.db");
//        CreateDatabase();
//        Debug.Log(dbPath);
//    }

//    private void CreateDatabase()
//    {
//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText =
//                        "CREATE TABLE IF NOT EXISTS MainDB (" +
//                        "PlayerID INTEGER PRIMARY KEY AUTOINCREMENT, " + // Changed to AUTOINCREMENT
//                        "PlayerName TEXT NOT NULL, " +
//                        "PlayerAge INTEGER NOT NULL, " +
//                        "PlayerEmail TEXT UNIQUE NOT NULL, " +
//                        "PlayerAgeGeneration TEXT, " +
//                        "SimonScore INTEGER DEFAULT 0, " +
//                        "MazeCompleteTime REAL, " +
//                        "ShootingScores INTEGER, " +
//                        "FOREIGN KEY (ShootingScores) REFERENCES ShootingScoresDB(ShootingScores));";
//                    command.ExecuteNonQuery();

//                    command.CommandText =
//                        "CREATE TABLE IF NOT EXISTS ShootingScoresDB (" +
//                        "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
//                        "ShootingScores INTEGER NOT NULL, " +
//                        "ShootingTime REAL NOT NULL, " +
//                        "HitPointX REAL NOT NULL, " +
//                        "HitPointY REAL NOT NULL);";
//                    command.ExecuteNonQuery();
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Error creating database: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }
//    }

//    // Updated method to not require playerID parameter
//    public void InsertPlayerData(string playerName, int playerAge, string playerEmail, string ageGen, int simonScore, double mazeTime, int shootingScore)
//    {
//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText =
//                        "INSERT INTO MainDB (PlayerName, PlayerAge, PlayerEmail, PlayerAgeGeneration, SimonScore, MazeCompleteTime, ShootingScores) " +
//                        "VALUES (@PlayerName, @PlayerAge, @PlayerEmail, @PlayerAgeGeneration, @SimonScore, @MazeCompleteTime, @ShootingScores);";

//                    command.Parameters.AddWithValue("@PlayerName", playerName);
//                    command.Parameters.AddWithValue("@PlayerAge", playerAge);
//                    command.Parameters.AddWithValue("@PlayerEmail", playerEmail);
//                    command.Parameters.AddWithValue("@PlayerAgeGeneration", ageGen);
//                    command.Parameters.AddWithValue("@SimonScore", simonScore);
//                    command.Parameters.AddWithValue("@MazeCompleteTime", mazeTime);
//                    command.Parameters.AddWithValue("@ShootingScores", shootingScore);

//                    command.ExecuteNonQuery();

//                    // Get the ID of the newly inserted player and save it to PlayerPrefs
//                    command.CommandText = "SELECT last_insert_rowid()";
//                    int newPlayerId = Convert.ToInt32(command.ExecuteScalar());
//                    PlayerPrefs.SetInt("CurrentPlayerId", newPlayerId);
//                    PlayerPrefs.Save();

//                    Debug.Log($"Játékos adatok sikeresen feltöltve! PlayerID: {newPlayerId}");
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Error inserting player data: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }
//    }

//    public void InsertShootingScore(int shootingScore, double shootingTime, double hitX, double hitY)
//    {
//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText =
//                        "INSERT INTO ShootingScoresDB (ShootingScores, ShootingTime, HitPointX, HitPointY) " +
//                        "VALUES (@ShootingScores, @ShootingTime, @HitPointX, @HitPointY);";

//                    command.Parameters.AddWithValue("@ShootingScores", shootingScore);
//                    command.Parameters.AddWithValue("@ShootingTime", shootingTime);
//                    command.Parameters.AddWithValue("@HitPointX", hitX);
//                    command.Parameters.AddWithValue("@HitPointY", hitY);

//                    command.ExecuteNonQuery();
//                    Debug.Log("Lövési adatok sikeresen feltöltve!");
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Error inserting shooting score: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }
//    }

//    public void UpdateSimonScore(int newScore)
//    {
//        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText = "UPDATE MainDB SET SimonScore = @Score WHERE PlayerID = @PlayerID";
//                    command.Parameters.AddWithValue("@Score", newScore);
//                    command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
//                    command.ExecuteNonQuery();
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Error updating Simon score: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }
//    }

//    public void UpdateMazeTime(double completionTime, string formattedTime)
//    {
//        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

//        UnityEngine.Debug.Log($"Updating maze time in database: Player ID={currentPlayerId}, Time={formattedTime}");

//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    // First, try to convert the table schema to allow for text format
//                    try
//                    {
//                        command.CommandText = "ALTER TABLE MainDB MODIFY COLUMN MazeCompleteTime TEXT";
//                        command.ExecuteNonQuery();
//                        UnityEngine.Debug.Log("Database schema updated to store time as text");
//                    }
//                    catch (Exception ex)
//                    {
//                        // If alter table fails, we'll continue with the current schema
//                        UnityEngine.Debug.Log($"Continuing with existing schema: {ex.Message}");
//                    }

//                    // Now update the time - try to store as formatted string if possible
//                    command.CommandText = "UPDATE MainDB SET MazeCompleteTime = @Time WHERE PlayerID = @PlayerID";

//                    // Try to use the formatted string value first, fall back to double if needed
//                    try
//                    {
//                        command.Parameters.AddWithValue("@Time", formattedTime);
//                    }
//                    catch
//                    {
//                        // If we can't use string format, fall back to double
//                        command.Parameters.AddWithValue("@Time", completionTime);
//                    }

//                    command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
//                    int rowsAffected = command.ExecuteNonQuery();
//                    UnityEngine.Debug.Log($"Database update result: {rowsAffected} rows affected");
//                }
//            }
//            catch (Exception ex)
//            {
//                UnityEngine.Debug.LogError($"Error updating maze time: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }
//    }

//    public void UpdateShootingScore(int score, double time, double hitX, double hitY)
//    {
//        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

//        using (var connection = new SqliteConnection(dbPath))
//        {
//            connection.Open();
//            using (var transaction = connection.BeginTransaction())
//            {
//                try
//                {
//                    using (var command = connection.CreateCommand())
//                    {
//                        // Check if the ShootingScores already exists
//                        command.CommandText = "SELECT COUNT(*) FROM ShootingScoresDB WHERE ShootingScores = @Score";
//                        command.Parameters.AddWithValue("@Score", score);
//                        int exists = Convert.ToInt32(command.ExecuteScalar());

//                        if (exists > 0)
//                        {
//                            // Update existing record
//                            command.CommandText =
//                                "UPDATE ShootingScoresDB SET ShootingTime = @Time, HitPointX = @HitX, HitPointY = @HitY " +
//                                "WHERE ShootingScores = @Score";
//                        }
//                        else
//                        {
//                            // Insert new record
//                            command.CommandText =
//                                "INSERT INTO ShootingScoresDB (ShootingScores, ShootingTime, HitPointX, HitPointY) " +
//                                "VALUES (@Score, @Time, @HitX, @HitY)";
//                        }

//                        command.Parameters.AddWithValue("@Time", time);
//                        command.Parameters.AddWithValue("@HitX", hitX);
//                        command.Parameters.AddWithValue("@HitY", hitY);
//                        command.ExecuteNonQuery();

//                        // Then update the main player record
//                        command.CommandText =
//                            "UPDATE MainDB SET ShootingScores = @Score WHERE PlayerID = @PlayerID";
//                        command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
//                        command.ExecuteNonQuery();
//                    }
//                    transaction.Commit();
//                }
//                catch (Exception ex)
//                {
//                    transaction.Rollback();
//                    Debug.LogError($"Error updating shooting score: {ex.Message}");
//                }
//                finally
//                {
//                    if (connection != null && connection.State != ConnectionState.Closed)
//                        connection.Close();
//                }
//            }
//        }
//    }

//    // Add method to get current player data
//    public DataTable GetPlayerData(int playerId)
//    {
//        DataTable results = new DataTable();

//        using (var connection = new SqliteConnection(dbPath))
//        {
//            try
//            {
//                connection.Open();
//                using (var command = connection.CreateCommand())
//                {
//                    command.CommandText = "SELECT * FROM MainDB WHERE PlayerID = @PlayerID";
//                    command.Parameters.AddWithValue("@PlayerID", playerId);

//                    using (var reader = command.ExecuteReader())
//                    {
//                        results.Load(reader);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.LogError($"Error retrieving player data: {ex.Message}");
//            }
//            finally
//            {
//                if (connection != null && connection.State != ConnectionState.Closed)
//                    connection.Close();
//            }
//        }

//        return results;
//    }
//}


using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class SQLiteDBScript : MonoBehaviour
{
    private string connectionString;
    private int currentPlayerID = 0;
    private int currentShootingSessionID = 0;

    // This will help track if we have a properly initialized database connection
    private bool isDatabaseInitialized = false;

    void Awake()
    {
        // Az adatbázis fájl elérési útja
        string dbPath = Application.persistentDataPath + "/game_scores.db";
        connectionString = "URI=file:" + dbPath;

        Debug.Log("Database path: " + dbPath);

        // Adatbázis inicializálása
        InitializeDatabase();
    }

    void InitializeDatabase()
    {
        try
        {
            // Kapcsolat létrehozása
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Players tábla létrehozása
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createPlayersTable = @"
                    CREATE TABLE IF NOT EXISTS Players (
                        PlayerID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                    dbCmd.CommandText = createPlayersTable;
                    dbCmd.ExecuteNonQuery();
                }

                // PlayerDetails tábla létrehozása
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string createPlayerDetailsTable = @"
                    CREATE TABLE IF NOT EXISTS PlayerDetails (
                        PlayerID INTEGER PRIMARY KEY,
                        Age INTEGER,
                        Email TEXT,
                        Generation TEXT,
                        FOREIGN KEY(PlayerID) REFERENCES Players(PlayerID)
                    )";

                    dbCmd.CommandText = createPlayerDetailsTable;
                    dbCmd.ExecuteNonQuery();
                }

                // SimonScores tábla létrehozása
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

                // MazeScores tábla létrehozása
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

                // ShootingSessions tábla létrehozása (új)
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

                // ShootingScores tábla létrehozása (módosított)
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
                        RecordedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(SessionID) REFERENCES ShootingSessions(SessionID)
                    )";

                    dbCmd.CommandText = createShootingTable;
                    dbCmd.ExecuteNonQuery();
                }

                dbConnection.Close();
                Debug.Log("Database initialized successfully");
                isDatabaseInitialized = true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database initialization error: " + e.Message);
            isDatabaseInitialized = false;
        }
    }

    // A hiányzó InsertPlayerData metódus implementációja
    public int InsertPlayerData(string playerName, int playerAge, string playerEmail, string generation, int simonScore, double mazeTime, int shootingScore)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot insert player data: Database not initialized");
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Játékos hozzáadása
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (Name) VALUES (@name); SELECT last_insert_rowid();";

                    IDbDataParameter nameParam = dbCmd.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = playerName;
                    dbCmd.Parameters.Add(nameParam);

                    currentPlayerID = Convert.ToInt32(dbCmd.ExecuteScalar());
                    Debug.Log($"Added new player with ID: {currentPlayerID}");
                }

                // Játékos részleteinek hozzáadása
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO PlayerDetails (PlayerID, Age, Email, Generation) VALUES (@id, @age, @email, @gen)";

                    IDbDataParameter idParam = dbCmd.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = currentPlayerID;
                    dbCmd.Parameters.Add(idParam);

                    IDbDataParameter ageParam = dbCmd.CreateParameter();
                    ageParam.ParameterName = "@age";
                    ageParam.Value = playerAge;
                    dbCmd.Parameters.Add(ageParam);

                    IDbDataParameter emailParam = dbCmd.CreateParameter();
                    emailParam.ParameterName = "@email";
                    emailParam.Value = playerEmail;
                    dbCmd.Parameters.Add(emailParam);

                    IDbDataParameter genParam = dbCmd.CreateParameter();
                    genParam.ParameterName = "@gen";
                    genParam.Value = generation;
                    dbCmd.Parameters.Add(genParam);

                    dbCmd.ExecuteNonQuery();
                    Debug.Log($"Added player details for player ID: {currentPlayerID}");
                }

                // Simon játék inicializáló pontszám
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
                    Debug.Log($"Initialized Simon score for player ID: {currentPlayerID}");
                }

                // Labirintus játék inicializáló idõ
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
                    Debug.Log($"Initialized Maze time for player ID: {currentPlayerID}");
                }

                // Lövöldözõs játék session létrehozása
                currentShootingSessionID = StartNewShootingSession(currentPlayerID);
                Debug.Log($"Started new shooting session with ID: {currentShootingSessionID} for player: {currentPlayerID}");

                // Alapértelmezett lövés hozzáadása a session-höz
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = @"
                    INSERT INTO ShootingScores (SessionID, ShotNumber, ReactionTime, PositionX, PositionY) 
                    VALUES (@sessionID, @shotNumber, @time, @posX, @posY)";

                    IDbDataParameter sessionParam = dbCmd.CreateParameter();
                    sessionParam.ParameterName = "@sessionID";
                    sessionParam.Value = currentShootingSessionID;
                    dbCmd.Parameters.Add(sessionParam);

                    IDbDataParameter shotParam = dbCmd.CreateParameter();
                    shotParam.ParameterName = "@shotNumber";
                    shotParam.Value = 0; // Kezdeti lövés száma
                    dbCmd.Parameters.Add(shotParam);

                    IDbDataParameter timeParam = dbCmd.CreateParameter();
                    timeParam.ParameterName = "@time";
                    timeParam.Value = 0.0; // Kezdeti reakcióidõ
                    dbCmd.Parameters.Add(timeParam);

                    IDbDataParameter posXParam = dbCmd.CreateParameter();
                    posXParam.ParameterName = "@posX";
                    posXParam.Value = 0.0; // Kezdeti X pozíció
                    dbCmd.Parameters.Add(posXParam);

                    IDbDataParameter posYParam = dbCmd.CreateParameter();
                    posYParam.ParameterName = "@posY";
                    posYParam.Value = 0.0; // Kezdeti Y pozíció
                    dbCmd.Parameters.Add(posYParam);

                    dbCmd.ExecuteNonQuery();
                    Debug.Log($"Initialized Shooting score for session {currentShootingSessionID}");
                }

                return currentPlayerID;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error inserting player data: {e.Message}");
            return -1;
        }
    }

    // Új játékos hozzáadása és az ID lekérése
    public int AddNewPlayer(string playerName)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot add new player: Database not initialized");
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (Name) VALUES (@name); SELECT last_insert_rowid();";

                    IDbDataParameter nameParam = dbCmd.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = playerName;
                    dbCmd.Parameters.Add(nameParam);

                    currentPlayerID = Convert.ToInt32(dbCmd.ExecuteScalar());
                    Debug.Log($"Added new player with ID: {currentPlayerID}");

                    // Új lövési session létrehozása az új játékoshoz
                    StartNewShootingSession(currentPlayerID);

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

    // Új lövési session indítása egy játékoshoz
    public int StartNewShootingSession(int playerID)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot start new shooting session: Database not initialized");
            return -1;
        }

        if (playerID <= 0)
        {
            Debug.LogWarning("Cannot start new shooting session: Invalid player ID");
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO ShootingSessions (PlayerID) VALUES (@playerID); SELECT last_insert_rowid();";

                    IDbDataParameter playerParam = dbCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = playerID;
                    dbCmd.Parameters.Add(playerParam);

                    currentShootingSessionID = Convert.ToInt32(dbCmd.ExecuteScalar());
                    Debug.Log($"Started new shooting session with ID: {currentShootingSessionID} for player: {playerID}");

                    return currentShootingSessionID;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting new shooting session: " + e.Message);
            return -1;
        }
    }

    // Simon játék pontszámának frissítése
    public bool UpdateSimonScore(int score)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot update Simon score: Database not initialized");
            return false;
        }

        if (currentPlayerID <= 0)
        {
            Debug.LogWarning("Cannot update Simon score: No active player");
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

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
                    Debug.Log($"Updated Simon score for player {currentPlayerID}: {score}");
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

    // Labirintus idejének frissítése
    public bool UpdateMazeTime(double completionTime, string formattedTime)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot update Maze time: Database not initialized");
            return false;
        }

        if (currentPlayerID <= 0)
        {
            Debug.LogWarning("Cannot update Maze time: No active player");
            return false;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

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
                    Debug.Log($"Updated Maze time for player {currentPlayerID}: {formattedTime}");
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

    // Lövési pontszám frissítése (módosított)
    public bool UpdateShootingScore(int shotNumber, double reactionTime, double posX, double posY)
    {
        if (!isDatabaseInitialized)
        {
            Debug.LogWarning("Cannot update Shooting score: Database not initialized");
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
                    Debug.LogWarning("Cannot update Shooting score: No active shooting session and failed to create new one");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("Cannot update Shooting score: No active shooting session");
                return false;
            }
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = @"
                    INSERT INTO ShootingScores (SessionID, ShotNumber, ReactionTime, PositionX, PositionY) 
                    VALUES (@sessionID, @shotNumber, @time, @posX, @posY)";

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

                    dbCmd.ExecuteNonQuery();
                    Debug.Log($"Updated Shooting score for session {currentShootingSessionID}, shot {shotNumber}: time={reactionTime}, pos=({posX},{posY})");
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error updating Shooting score: " + e.Message);
            return false;
        }
    }

    // Aktuális játékos ID lekérése
    public int GetCurrentPlayerID()
    {
        return currentPlayerID;
    }

    // Aktuális lövési session ID lekérése
    public int GetCurrentShootingSessionID()
    {
        return currentShootingSessionID;
    }

    // Játékos ID beállítása (pl. meglévõ játékoshoz való csatlakozáskor)
    public void SetCurrentPlayerID(int playerID)
    {
        if (playerID <= 0)
        {
            Debug.LogWarning("Cannot set player ID: Invalid ID provided");
            return;
        }

        currentPlayerID = playerID;
        Debug.Log($"Current player ID set to: {currentPlayerID}");

        // Új lövési session indítása a beállított játékoshoz
        int sessionID = StartNewShootingSession(currentPlayerID);
        if (sessionID > 0)
        {
            currentShootingSessionID = sessionID;
            Debug.Log($"Started new shooting session ID: {currentShootingSessionID}");
        }
        else
        {
            Debug.LogWarning("Failed to start new shooting session for player ID: " + playerID);
        }
    }
}