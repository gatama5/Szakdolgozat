using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class SQLiteDBScript : MonoBehaviour
{
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
        // Az adatbбzis keresztьl vitelйnek biztosнtбsa
        DontDestroyOnLoad(this.gameObject);

        // Az adatbбzis fбjl elйrйsi ъtja
        string dbPath = Application.persistentDataPath + "/game_scores.db";
        connectionString = "URI=file:" + dbPath;

        //Debug.Log("Database path: " + dbPath);

        // Adatbбzis inicializбlбsa
        InitializeDatabase();

        // PlayerPrefs-bхl az azonosнtу visszaбllнtбsa, ha van
        if (PlayerPrefs.HasKey("CurrentPlayerID"))
        {
            int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
            if (savedPlayerID > 0)
            {
                SetCurrentPlayerID(savedPlayerID);
            }
        }
    }

    void InitializeDatabase()
    {
        try
        {
            // Kapcsolat lйtrehozбsa
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // Players tбbla lйtrehozбsa
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

                // PlayerDetails tбbla lйtrehozбsa
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
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Database initialization error: " + e.Message);
            isDatabaseInitialized = false;
        }
    }

    // A hiбnyzу InsertPlayerData metуdus implementбciуja
    public int InsertPlayerData(string playerName, int playerAge, string playerEmail, string generation, int simonScore, double mazeTime, int shootingScore)
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

                // Jбtйkos hozzбadбsa
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (Name) VALUES (@name); SELECT last_insert_rowid();";

                    IDbDataParameter nameParam = dbCmd.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = playerName;
                    dbCmd.Parameters.Add(nameParam);

                    currentPlayerID = Convert.ToInt32(dbCmd.ExecuteScalar());
                }

                // Jбtйkos rйszleteinek hozzбadбsa
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
                }

                // Simon jбtйk inicializбlу pontszбm - ONLY IF NOT ZERO
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

                // Labirintus jбtйk inicializбlу idх - ONLY IF NOT ZERO
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

                // Lцvцldцzхs jбtйk session lйtrehozбsa
                currentShootingSessionID = StartNewShootingSession(currentPlayerID);

                // NE adjunk hozzб alapйrtelmezett lцvйst
                // A lцvйseket csak tйnyleges jбtйk sorбn rцgzнtsьk

                return currentPlayerID;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error inserting player data: {e.Message}");
            return -1;
        }
    }

    // Ъj jбtйkos hozzбadбsa йs az ID lekйrйse
    public int AddNewPlayer(string playerName)
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

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO Players (Name) VALUES (@name); SELECT last_insert_rowid();";

                    IDbDataParameter nameParam = dbCmd.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = playerName;
                    dbCmd.Parameters.Add(nameParam);

                    currentPlayerID = Convert.ToInt32(dbCmd.ExecuteScalar());

                    // Ъj lцvйsi session lйtrehozбsa az ъj jбtйkoshoz
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

    // Ъj lцvйsi session indнtбsa egy jбtйkoshoz
    public int StartNewShootingSession(int playerID)
    {
        if (!isDatabaseInitialized)
        {
            return -1;
        }

        if (playerID <= 0)
        {
            return -1;
        }

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                // ELLENХRIZZЬK, HOGY VAN-E MБR AKTНV SESSION
                using (IDbCommand checkCmd = dbConnection.CreateCommand())
                {
                    checkCmd.CommandText = @"
                SELECT SessionID FROM ShootingSessions 
                WHERE PlayerID = @playerID 
                ORDER BY StartedAt DESC LIMIT 1";

                    IDbDataParameter playerParam = checkCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = playerID;
                    checkCmd.Parameters.Add(playerParam);

                    object result = checkCmd.ExecuteScalar();

                    // Ha van mбr session, akkor hasznбljuk azt
                    if (result != null && result != DBNull.Value)
                    {
                        int existingSessionID = Convert.ToInt32(result);
                        currentShootingSessionID = existingSessionID;
                        return existingSessionID;
                    }
                }

                // Ha nincs mйg session, akkor hozzunk lйtre
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    dbCmd.CommandText = "INSERT INTO ShootingSessions (PlayerID) VALUES (@playerID); SELECT last_insert_rowid();";

                    IDbDataParameter playerParam = dbCmd.CreateParameter();
                    playerParam.ParameterName = "@playerID";
                    playerParam.Value = playerID;
                    dbCmd.Parameters.Add(playerParam);

                    currentShootingSessionID = Convert.ToInt32(dbCmd.ExecuteScalar());

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

    // Lцvйsi pontszбm frissнtйse (mуdosнtott a gameType paramйterrel)
    public bool UpdateShootingScore(int shotNumber, double reactionTime, double posX, double posY, string gameType = "Shooting")
    {
        if (!isDatabaseInitialized)
        {
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

                // ELLENХRIZZЬK, HOGY A LЦVЙS MБR SZEREPEL-E AZ ADATBБZISBAN
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
                    gameTypeParam.Value = gameType;
                    checkCmd.Parameters.Add(gameTypeParam);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    // Ha mбr lйtezik ilyen lцvйs, ne adjunk hozzб ъjat
                    if (count > 0)
                    {
                        return false;
                    }
                }

                // Ha mйg nem lйtezik, akkor adjuk hozzб
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

                    dbCmd.ExecuteNonQuery();
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

    // Egyszerыbb hнvбshoz tъlterhelt metуdus - Target jбtйkhoz
    public bool UpdateTargetScore(int shotNumber, double reactionTime, double posX, double posY)
    {
        return UpdateShootingScore(shotNumber, reactionTime, posX, posY, GAME_TYPE_TARGET);
    }

    // Egyszerыbb hнvбshoz tъlterhelt metуdus - Shooting jбtйkhoz
    public bool UpdateShootingScore(int shotNumber, double reactionTime, double posX, double posY)
    {
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
}