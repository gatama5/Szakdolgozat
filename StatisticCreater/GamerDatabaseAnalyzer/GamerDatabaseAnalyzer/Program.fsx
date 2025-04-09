//let connectionString = "Data Source=C:\\Users\\nonst\\AppData\\LocalLow\\DefaultCompany\\Szakdolgozat\\game_scores_home.db"

#r "nuget: Microsoft.Data.Sqlite"
open System
open System.IO
open Microsoft.Data.Sqlite

let dbPath = "C:\\Users\\nonst\\AppData\\LocalLow\\DefaultCompany\\Szakdolgozat\\game_scores_home.db"

try
    let connectionString = sprintf "Data Source=%s" dbPath
    use connection = new SqliteConnection(connectionString)
    connection.Open()
    printfn "Adatbázis sikeresen megnyitva"
    
    // Próbáljunk egy egyszerű lekérdezést először
    let query = "
        SELECT Players.Name, AVG(ShootingScores.ReactionTime)
        FROM ShootingScores
        JOIN ShootingSessions ON ShootingScores.SessionID = ShootingSessions.SessionID
        JOIN Players ON ShootingSessions.PlayerID = Players.PlayerID
        GROUP BY Players.Name"
    
    use command = connection.CreateCommand()
    command.CommandText <- query
    
    try
        use reader = command.ExecuteReader()
        printfn "Lekérdezés sikeres. Az eredmények:"
        
        while reader.Read() do
            let name = reader.GetString(0)
            let avgReaction = reader.GetDouble(1)
            printfn "Játékos: %s | Átlagos reakcióidő: %.2f ms" name avgReaction
    with
        | ex -> printfn "Hiba a lekérdezés során: %s" ex.Message
    
except
    | ex -> printfn "Hiba az adatbázis megnyitásakor: %s" ex.Message

//#r "nuget: XPlot.Plotly"
//#r "nuget: FsLab"
//#r "nuget: Microsoft.Data.Sqlite"
//open System
//open Microsoft.Data.Sqlite
//open XPlot.Plotly

////elérési út ellenőrzése
//let dbPath = "C:\\Users\\nonst\\AppData\\LocalLow\\DefaultCompany\\Szakdolgozat\\game_scores_home.db"

//if System.IO.File.Exists(dbPath) then
//    printfn "Az adatbázisfájl megtalálható: %s" dbPath
//else
//    printfn "Az adatbázisfájl nem található: %s" dbPath

//let connectionString = "Data Source=C:\\Users\\nonst\\AppData\\LocalLow\\DefaultCompany\\Szakdolgozat\\game_scores_home.db"
//let connection = new SqliteConnection(connectionString)
//connection.Open()
//// Adatok lekérdezése
//let query = "
//    SELECT Players.Name, AVG(ShootingScores.ReactionTime)
//    FROM ShootingScores
//    JOIN ShootingSessions ON ShootingScores.SessionID = ShootingSessions.SessionID
//    JOIN Players ON ShootingSessions.PlayerID = Players.PlayerID
//    GROUP BY Players.Name"
//let command = connection.CreateCommand()
//command.CommandText <- query
//// Adatok összegyűjtése grafikonhoz
//let mutable names = []
//let mutable reactions = []
//let reader = command.ExecuteReader()
//while reader.Read() do
//    let name = reader.GetString(0)
//    let avgReaction = reader.GetDouble(1)
    
//    // Adatok hozzáadása a listákhoz
//    names <- name :: names
//    reactions <- avgReaction :: reactions
    
//    // Kiírás konzolra is
//    printfn "Játékos: %s | Átlagos reakcióidő: %.2f ms" name avgReaction
//// Listák megfordítása - új változónevekkel
//let namesArray = List.rev names |> Array.ofList
//let reactionsArray = List.rev reactions |> Array.ofList
//// Grafikon létrehozása egyszerűbb módon
//let trace = 
//    Bar(
//        x = namesArray,
//        y = reactionsArray
//    )
//let chart = [trace] |> Chart.Plot
//chart |> Chart.Show
//// Minden erőforrás megfelelő felszabadítása
//reader.Dispose()
//command.Dispose()
//connection.Dispose()