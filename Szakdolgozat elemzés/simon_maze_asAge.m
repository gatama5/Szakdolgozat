% simon_maze_asAge_combined.m
% SQLite adatbázisok együttes elemzése: Simon és Maze játékok pontszámai életkor szerint

try
    % 1. Az adatbázis fájlok elérési útjainak meghatározása
    dbfile1 = 'C:\Users\nonst\AppData\LocalLow\DefaultCompany\Szakdolgozat\game_scores.db';
    dbfile2 = 'C:\Users\nonst\AppData\LocalLow\DefaultCompany\Szakdolgozat\game_scores_home.db';
    
    % Ellenőrizzük, hogy a fájlok léteznek-e
    missing_files = [];
    if ~exist(dbfile1, 'file')
        missing_files = [missing_files, {dbfile1}];
    end
    if ~exist(dbfile2, 'file')
        missing_files = [missing_files, {dbfile2}];
    end
    
    % Ha bármely fájl hiányzik, kérjünk be a felhasználótól
    if ~isempty(missing_files)
        disp('Az alábbi adatbázis fájlok nem találhatók:');
        disp(missing_files);
        
        if length(missing_files) == 2
            % Ha mindkét fájl hiányzik, kérjünk be kettőt
            disp('Kérlek válaszd ki az első adatbázis fájlt:');
            [file1, path1] = uigetfile({'*.db', 'SQLite adatbázis fájlok (*.db)'; '*.*', 'Minden fájl (*.*)'}, 'Válaszd ki az első adatbázis fájlt');
            if isequal(file1, 0)
                error('Nem választottál fájlt. Az elemzés leáll.');
            else
                dbfile1 = fullfile(path1, file1);
                disp(['Kiválasztott első adatbázis: ', dbfile1]);
            end
            
            disp('Kérlek válaszd ki a második adatbázis fájlt:');
            [file2, path2] = uigetfile({'*.db', 'SQLite adatbázis fájlok (*.db)'; '*.*', 'Minden fájl (*.*)'}, 'Válaszd ki a második adatbázis fájlt');
            if isequal(file2, 0)
                error('Nem választottál fájlt. Az elemzés leáll.');
            else
                dbfile2 = fullfile(path2, file2);
                disp(['Kiválasztott második adatbázis: ', dbfile2]);
            end
        else
            % Ha csak egy fájl hiányzik, kérjük be azt
            disp('Kérlek válaszd ki a hiányzó adatbázis fájlt:');
            [file, path] = uigetfile({'*.db', 'SQLite adatbázis fájlok (*.db)'; '*.*', 'Minden fájl (*.*)'}, 'Válaszd ki a hiányzó adatbázis fájlt');
            
            if isequal(file, 0)
                error('Nem választottál fájlt. Az elemzés leáll.');
            else
                missing_file = fullfile(path, file);
                if strcmp(missing_files{1}, dbfile1)
                    dbfile1 = missing_file;
                    disp(['Kiválasztott első adatbázis: ', dbfile1]);
                else
                    dbfile2 = missing_file;
                    disp(['Kiválasztott második adatbázis: ', dbfile2]);
                end
            end
        end
    end
    
    % 2. Adatok összegyűjtése mindkét adatbázisból
    % Létrehozzuk a tárolókat az összesített adatoknak
    allPlayerDetails = [];
    allSimonScores = [];
    allMazeScores = [];
    
    % Adatbázisok feldolgozása egyesével
    for db_idx = 1:2
        if db_idx == 1
            current_db = dbfile1;
            disp(['1. adatbázis feldolgozása: ', current_db]);
        else
            current_db = dbfile2;
            disp(['2. adatbázis feldolgozása: ', current_db]);
        end
        
        % Kapcsolódás az adatbázishoz
        disp(['Kapcsolódás az adatbázishoz: ', current_db]);
        conn = sqlite(current_db, 'readonly');
        disp('Sikeres kapcsolódás az adatbázishoz');
        
        % Adatok lekérdezése
        disp('Adatok lekérdezése...');
        
        % Táblák ellenőrzése
        tables = fetch(conn, "SELECT name FROM sqlite_master WHERE type='table'");
        disp('Elérhető táblák:');
        disp(tables);
        
        % Játékos adatok lekérdezése
        playerDetails = fetch(conn, 'SELECT PlayerID, Age FROM PlayerDetails WHERE Age IS NOT NULL AND Age > 0');
        disp(['Játékosok száma életkor adattal: ', num2str(size(playerDetails, 1))]);
        
        % Simon játék pontszámok
        simonScores = fetch(conn, 'SELECT PlayerID, Score FROM SimonScores WHERE Score > 0');
        disp(['Simon játék adatok száma: ', num2str(size(simonScores, 1))]);
        
        % Maze játék időadatok
        mazeScores = fetch(conn, 'SELECT PlayerID, CompletionTime FROM MazeScores WHERE CompletionTime > 0');
        disp(['Maze játék adatok száma: ', num2str(size(mazeScores, 1))]);
        
        % Adatok konvertálása táblákká
        if iscell(playerDetails)
            T_players = cell2table(playerDetails, 'VariableNames', {'PlayerID', 'Age'});
        elseif istable(playerDetails)
            T_players = playerDetails;
        else
            T_players = array2table(playerDetails, 'VariableNames', {'PlayerID', 'Age'});
        end
        
        if iscell(simonScores)
            T_simon = cell2table(simonScores, 'VariableNames', {'PlayerID', 'Score'});
        elseif istable(simonScores)
            T_simon = simonScores;
        else
            T_simon = array2table(simonScores, 'VariableNames', {'PlayerID', 'Score'});
        end
        
        if iscell(mazeScores)
            T_maze = cell2table(mazeScores, 'VariableNames', {'PlayerID', 'CompletionTime'});
        elseif istable(mazeScores)
            T_maze = mazeScores;
        else
            T_maze = array2table(mazeScores, 'VariableNames', {'PlayerID', 'CompletionTime'});
        end
        
        % Adatok konvertálása a megfelelő típusra
        if ~isnumeric(T_players.PlayerID)
            T_players.PlayerID = str2double(T_players.PlayerID);
        end
        
        if ~isnumeric(T_players.Age)
            T_players.Age = str2double(T_players.Age);
        end
        
        if ~isnumeric(T_simon.PlayerID)
            T_simon.PlayerID = str2double(T_simon.PlayerID);
        end
        
        if ~isnumeric(T_simon.Score)
            T_simon.Score = str2double(T_simon.Score);
        end
        
        if ~isnumeric(T_maze.PlayerID)
            T_maze.PlayerID = str2double(T_maze.PlayerID);
        end
        
        if ~isnumeric(T_maze.CompletionTime)
            T_maze.CompletionTime = str2double(T_maze.CompletionTime);
        end
        
        % Adatok hozzáadása az összesített táblákhoz
        if isempty(allPlayerDetails)
            allPlayerDetails = T_players;
        else
            allPlayerDetails = [allPlayerDetails; T_players];
        end
        
        if isempty(allSimonScores)
            allSimonScores = T_simon;
        else
            allSimonScores = [allSimonScores; T_simon];
        end
        
        if isempty(allMazeScores)
            allMazeScores = T_maze;
        else
            allMazeScores = [allMazeScores; T_maze];
        end
        
        % Kapcsolat lezárása
        close(conn);
        disp(['Adatbázis kapcsolat lezárva: ', current_db]);
    end
    
    % 3. Az összesített adatok ellenőrzése
    disp('Az összesített adatok mérete:');
    disp(['Összesített játékos adatok: ', num2str(height(allPlayerDetails)), ' sor']);
    disp(['Összesített Simon játék adatok: ', num2str(height(allSimonScores)), ' sor']);
    disp(['Összesített Maze játék adatok: ', num2str(height(allMazeScores)), ' sor']);
    
    % Duplikációk kezelése - játékos adatok esetén megtartjuk az első előfordulást
    [~, idx] = unique(allPlayerDetails.PlayerID, 'first');
    allPlayerDetails = allPlayerDetails(idx, :);
    disp(['Játékos adatok duplikációk eltávolítása után: ', num2str(height(allPlayerDetails)), ' sor']);
    
    % 4. Játékos adatok és pontszámok összekapcsolása
    simonData = innerjoin(allSimonScores, allPlayerDetails);
    mazeData = innerjoin(allMazeScores, allPlayerDetails);
    
    disp(['Simon játék összekapcsolt adatok száma: ', num2str(height(simonData))]);
    disp(['Maze játék összekapcsolt adatok száma: ', num2str(height(mazeData))]);
    
    % 5. Csoportosítás és átlagszámítás életkor szerint
    if height(simonData) > 0
        simonAvg = varfun(@mean, simonData, 'InputVariables', 'Score', 'GroupingVariables', 'Age');
        disp('Simon játék átlagok életkor szerint:');
        disp(simonAvg);
    else
        error('Nincs elegendő Simon játék adat az elemzéshez.');
    end
    
    if height(mazeData) > 0
        mazeAvg = varfun(@mean, mazeData, 'InputVariables', 'CompletionTime', 'GroupingVariables', 'Age');
        disp('Maze játék átlagok életkor szerint:');
        disp(mazeAvg);
    else
        error('Nincs elegendő Maze játék adat az elemzéshez.');
    end
    
    % 6. Eredmények ábrázolása
    figure('Position', [100, 100, 1000, 500]);
    
    % Simon játék pontszám átlagok
    subplot(1, 2, 1);
    bar(simonAvg.Age, simonAvg.mean_Score, 'FaceColor', [0.2 0.6 0.8]);
    hold on;
    plot(simonAvg.Age, simonAvg.mean_Score, '-o', 'LineWidth', 2, 'Color', [0 0.4 0.7], 'MarkerSize', 8, 'MarkerFaceColor', [0 0.4 0.7]);
    
    title('Átlagos Simon Score életkor szerint', 'FontSize', 14, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 12);
    ylabel('Átlagos pontszám', 'FontSize', 12);
    grid on;
    
    % Adatfeliratok hozzáadása
    for i = 1:height(simonAvg)
        text(simonAvg.Age(i), simonAvg.mean_Score(i) + max(simonAvg.mean_Score)*0.03, ...
            num2str(round(simonAvg.mean_Score(i), 1)), ...
            'HorizontalAlignment', 'center', 'FontWeight', 'bold');
    end
    
    % Maze játék idő átlagok
    subplot(1, 2, 2);
    bar(mazeAvg.Age, mazeAvg.mean_CompletionTime, 'FaceColor', [0.4 0.8 0.4]);
    hold on;
    plot(mazeAvg.Age, mazeAvg.mean_CompletionTime, '-o', 'LineWidth', 2, 'Color', [0.1 0.6 0.1], 'MarkerSize', 8, 'MarkerFaceColor', [0.1 0.6 0.1]);
    
    title('Átlagos Maze idő életkor szerint', 'FontSize', 14, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 12);
    ylabel('Átlagos idő (s)', 'FontSize', 12);
    grid on;
    
    % Adatfeliratok hozzáadása
    for i = 1:height(mazeAvg)
        text(mazeAvg.Age(i), mazeAvg.mean_CompletionTime(i) + max(mazeAvg.mean_CompletionTime)*0.03, ...
            num2str(round(mazeAvg.mean_CompletionTime(i), 1)), ...
            'HorizontalAlignment', 'center', 'FontWeight', 'bold');
    end
    
    % Ábra szépítés
    sgtitle('Játékeredmények életkor szerint (Kombinált adatok)', 'FontSize', 16, 'FontWeight', 'bold');
    
    % 7. Ábra mentése
    saveas(gcf, 'eletkor_szerinti_eredmenyek_kombinalt.png');
    saveas(gcf, 'eletkor_szerinti_eredmenyek_kombinalt.fig');
    disp('Ábra sikeresen mentve: eletkor_szerinti_eredmenyek_kombinalt.png és .fig');
    
    % 8. További statisztikák
    disp('További statisztikák:');
    
    % Simon játék statisztikák
    disp('Simon játék statisztikák életkoronként (kombinált adatok):');
    simonStats = varfun(@(x)[mean(x), std(x), min(x), max(x), median(x), numel(x)], ...
        simonData, 'InputVariables', 'Score', 'GroupingVariables', 'Age');
    simonStats.Properties.VariableNames(2) = {'Statisztikák [átlag, szórás, min, max, medián, N]'};
    disp(simonStats);
    
    % Maze játék statisztikák
    disp('Maze játék statisztikák életkoronként (kombinált adatok):');
    mazeStats = varfun(@(x)[mean(x), std(x), min(x), max(x), median(x), numel(x)], ...
        mazeData, 'InputVariables', 'CompletionTime', 'GroupingVariables', 'Age');
    mazeStats.Properties.VariableNames(2) = {'Statisztikák [átlag, szórás, min, max, medián, N]'};
    disp(mazeStats);
    
    % 9. Adatbázisok összehasonlítása (opcionális)
    disp('Elemzés sikeresen befejezve!');
    disp('A két adatbázis adatait sikeresen kombináltuk és elemeztük.');
    
catch e
    % Hibakezelés
    disp(['HIBA: ', e.message]);
    disp('Hiba stack trace:');
    disp(getReport(e));
    
    % Ha van még nyitott kapcsolat, zárjuk le
    if exist('conn', 'var') && ~isempty(conn)
        try
            close(conn);
            disp('Adatbázis kapcsolat lezárva a hiba után.');
        catch
            disp('Nem sikerült lezárni az adatbázis kapcsolatot.');
        end
    end
end