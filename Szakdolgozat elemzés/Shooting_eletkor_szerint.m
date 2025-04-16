% shooting_statistics.m
% SQLite adatbázisok együttes elemzése: Lövési eredmények elemzése életkor szerint

try
    % 1. Az adatbázis fájlok elérési útjainak meghatározása
    dbfile1 = 'C:\Users\nonst\AppData\LocalLow\DefaultCompany\Szakdolgozat\game_scores.db';
    dbfile2 = 'C:\Users\nonst\AppData\LocalLow\DefaultCompany\Szakdolgozat\game_scores_home.db';
      
    
    % 2. Adatok összegyűjtése mindkét adatbázisból
    % Létrehozzuk a tárolókat az összesített adatoknak
    allPlayerDetails = [];
    allTargetScores = [];
    allShootingScores = [];
    
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
        
        % Táblák ellenőrzése
        tables = fetch(conn, "SELECT name FROM sqlite_master WHERE type='table'");
        disp('Elérhető táblák:');
        disp(tables);
        
        % Játékos adatok lekérdezése
        playerDetails = fetch(conn, 'SELECT PlayerID, Age FROM PlayerDetails WHERE Age IS NOT NULL AND Age > 0');
        disp(['Játékosok száma életkor adattal: ', num2str(size(playerDetails, 1))]);
        
        % Target (célzás) játék adatok lekérdezése - szűrés GameType alapján
        targetScores = fetch(conn, 'SELECT PlayerID, ReactionTime, PositionX as HitPositionX, PositionY as HitPositionY FROM ShootingScores WHERE ReactionTime > 0 AND GameType = "Target"');
        disp(['Target játék adatok száma: ', num2str(size(targetScores, 1))]);
        
        % Shooting (lövés) játék adatok lekérdezése - szűrés GameType alapján
        shootingScores = fetch(conn, 'SELECT PlayerID, ReactionTime, PositionX as HitPositionX, PositionY as HitPositionY FROM ShootingScores WHERE ReactionTime > 0 AND GameType = "Shooting"');
        disp(['Shooting játék adatok száma: ', num2str(size(shootingScores, 1))]);
        
        % Adatok konvertálása táblákká
        if iscell(playerDetails)
            T_players = cell2table(playerDetails, 'VariableNames', {'PlayerID', 'Age'});
        elseif istable(playerDetails)
            T_players = playerDetails;
        else
            T_players = array2table(playerDetails, 'VariableNames', {'PlayerID', 'Age'});
        end
        
        if iscell(targetScores)
            T_target = cell2table(targetScores, 'VariableNames', {'PlayerID', 'ReactionTime', 'HitPositionX', 'HitPositionY'});
        elseif istable(targetScores)
            T_target = targetScores;
        else
            T_target = array2table(targetScores, 'VariableNames', {'PlayerID', 'ReactionTime', 'HitPositionX', 'HitPositionY'});
        end
        
        if iscell(shootingScores)
            T_shooting = cell2table(shootingScores, 'VariableNames', {'PlayerID', 'ReactionTime', 'HitPositionX', 'HitPositionY'});
        elseif istable(shootingScores)
            T_shooting = shootingScores;
        else
            T_shooting = array2table(shootingScores, 'VariableNames', {'PlayerID', 'ReactionTime', 'HitPositionX', 'HitPositionY'});
        end
        
        % Adatok konvertálása a megfelelő típusra
        if ~isnumeric(T_players.PlayerID)
            T_players.PlayerID = str2double(T_players.PlayerID);
        end
        
        if ~isnumeric(T_players.Age)
            T_players.Age = str2double(T_players.Age);
        end
        
        if ~isnumeric(T_target.PlayerID)
            T_target.PlayerID = str2double(T_target.PlayerID);
        end
        
        if ~isnumeric(T_target.ReactionTime)
            T_target.ReactionTime = str2double(T_target.ReactionTime);
        end
        
        if ~isnumeric(T_target.HitPositionX)
            T_target.HitPositionX = str2double(T_target.HitPositionX);
        end
        
        if ~isnumeric(T_target.HitPositionY)
            T_target.HitPositionY = str2double(T_target.HitPositionY);
        end
        
        if ~isnumeric(T_shooting.PlayerID)
            T_shooting.PlayerID = str2double(T_shooting.PlayerID);
        end
        
        if ~isnumeric(T_shooting.ReactionTime)
            T_shooting.ReactionTime = str2double(T_shooting.ReactionTime);
        end
        
        if ~isnumeric(T_shooting.HitPositionX)
            T_shooting.HitPositionX = str2double(T_shooting.HitPositionX);
        end
        
        if ~isnumeric(T_shooting.HitPositionY)
            T_shooting.HitPositionY = str2double(T_shooting.HitPositionY);
        end
        
        % Adatok hozzáadása az összesített táblákhoz
        if isempty(allPlayerDetails)
            allPlayerDetails = T_players;
        else
            allPlayerDetails = [allPlayerDetails; T_players];
        end
        
        if isempty(allTargetScores)
            allTargetScores = T_target;
        else
            allTargetScores = [allTargetScores; T_target];
        end
        
        if isempty(allShootingScores)
            allShootingScores = T_shooting;
        else
            allShootingScores = [allShootingScores; T_shooting];
        end
        
        % Kapcsolat lezárása
        close(conn);
        disp(['Adatbázis kapcsolat lezárva: ', current_db]);
    end
    
    % 3. Az összesített adatok ellenőrzése
    disp('Az összesített adatok mérete:');
    disp(['Összesített játékos adatok: ', num2str(height(allPlayerDetails)), ' sor']);
    disp(['Összesített Target játék adatok: ', num2str(height(allTargetScores)), ' sor']);
    disp(['Összesített Shooting játék adatok: ', num2str(height(allShootingScores)), ' sor']);
    
    % Duplikációk kezelése - játékos adatok esetén megtartjuk az első előfordulást
    [~, idx] = unique(allPlayerDetails.PlayerID, 'first');
    allPlayerDetails = allPlayerDetails(idx, :);
    disp(['Játékos adatok duplikációk eltávolítása után: ', num2str(height(allPlayerDetails)), ' sor']);
    
    % 4. Játékos adatok és pontszámok összekapcsolása
    targetData = innerjoin(allTargetScores, allPlayerDetails);
    shootingData = innerjoin(allShootingScores, allPlayerDetails);
    
    disp(['Target játék összekapcsolt adatok száma: ', num2str(height(targetData))]);
    disp(['Shooting játék összekapcsolt adatok száma: ', num2str(height(shootingData))]);
    
    % 5. Csoportosítás és átlagszámítás életkor szerint
    % Target ReactionTime átlag életkoronként
    if height(targetData) > 0
        targetReactionAvg = varfun(@mean, targetData, 'InputVariables', 'ReactionTime', 'GroupingVariables', 'Age');
        disp('Target játék reakcióidő átlagok életkor szerint:');
        disp(targetReactionAvg);
    else
        error('Nincs elegendő Target játék adat az elemzéshez.');
    end
    
    % Shooting ReactionTime átlag életkoronként
    if height(shootingData) > 0
        shootingReactionAvg = varfun(@mean, shootingData, 'InputVariables', 'ReactionTime', 'GroupingVariables', 'Age');
        disp('Shooting játék reakcióidő átlagok életkor szerint:');
        disp(shootingReactionAvg);
    else
        error('Nincs elegendő Shooting játék adat az elemzéshez.');
    end
    
    % Találati hely átlagok számolása
    % Target játék találati hely átlag
    targetPositionAvg = varfun(@mean, targetData, 'InputVariables', {'HitPositionX', 'HitPositionY'}, 'GroupingVariables', 'Age');
    disp('Target játék találati hely átlagok életkor szerint:');
    disp(targetPositionAvg);
    
    % Shooting játék találati hely átlag
    shootingPositionAvg = varfun(@mean, shootingData, 'InputVariables', {'HitPositionX', 'HitPositionY'}, 'GroupingVariables', 'Age');
    disp('Shooting játék találati hely átlagok életkor szerint:');
    disp(shootingPositionAvg);
    
    % 6. Ábra 1: Target játék reakcióidő átlag
    figure('Position', [100, 100, 800, 600]);
    bar(targetReactionAvg.Age, targetReactionAvg.mean_ReactionTime, 'FaceColor', [0.8 0.3 0.3]);
    hold on;
    plot(targetReactionAvg.Age, targetReactionAvg.mean_ReactionTime, '-o', 'LineWidth', 2, 'Color', [0.7 0.1 0.1], 'MarkerSize', 8, 'MarkerFaceColor', [0.7 0.1 0.1]);
    
    title('Átlagos Target reakcióidő életkor szerint', 'FontSize', 14, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 12);
    ylabel('Átlagos reakcióidő (s)', 'FontSize', 12);
    grid on;
    
    % Adatfeliratok hozzáadása
    for i = 1:height(targetReactionAvg)
        text(targetReactionAvg.Age(i), targetReactionAvg.mean_ReactionTime(i) + max(targetReactionAvg.mean_ReactionTime)*0.03, ...
            num2str(round(targetReactionAvg.mean_ReactionTime(i), 3)), ...
            'HorizontalAlignment', 'center', 'FontWeight', 'bold');
    end
    
    % Ábra mentése
    saveas(gcf, 'target_reakcioido_atlag.png');
    saveas(gcf, 'target_reakcioido_atlag.fig');
    disp('Target reakcióidő ábra sikeresen mentve: target_reakcioido_atlag.png és .fig');
    
    % 7. Ábra 2: Shooting játék reakcióidő átlag
    figure('Position', [100, 100, 800, 600]);
    bar(shootingReactionAvg.Age, shootingReactionAvg.mean_ReactionTime, 'FaceColor', [0.3 0.3 0.8]);
    hold on;
    plot(shootingReactionAvg.Age, shootingReactionAvg.mean_ReactionTime, '-o', 'LineWidth', 2, 'Color', [0.1 0.1 0.7], 'MarkerSize', 8, 'MarkerFaceColor', [0.1 0.1 0.7]);
    
    title('Átlagos Shooting reakcióidő életkor szerint', 'FontSize', 14, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 12);
    ylabel('Átlagos reakcióidő (s)', 'FontSize', 12);
    grid on;
    
    % Adatfeliratok hozzáadása
    for i = 1:height(shootingReactionAvg)
        text(shootingReactionAvg.Age(i), shootingReactionAvg.mean_ReactionTime(i) + max(shootingReactionAvg.mean_ReactionTime)*0.03, ...
            num2str(round(shootingReactionAvg.mean_ReactionTime(i), 3)), ...
            'HorizontalAlignment', 'center', 'FontWeight', 'bold');
    end
    
    % Ábra mentése
    saveas(gcf, 'shooting_reakcioido_atlag.png');
    saveas(gcf, 'shooting_reakcioido_atlag.fig');
    disp('Shooting reakcióidő ábra sikeresen mentve: shooting_reakcioido_atlag.png és .fig');
    
    % 8. Ábra 3: Találati helyek átlagok X és Y tengelyen életkoronként
    figure('Position', [100, 100, 1000, 800]);
    
    % Scatter plot létrehozása a találati helyekhez
    % Feltételezzük, hogy a játéktér egy 1x1-es négyzet, ahol a középpont (0.5, 0.5)
    subplot(2, 2, 1);
    scatter(targetPositionAvg.mean_HitPositionX, targetPositionAvg.mean_HitPositionY, 100, targetPositionAvg.Age, 'filled', 'MarkerEdgeColor', 'k');
    colorbar;
    colormap('cool');
    title('Target játék - Átlagos találati helyek életkor szerint', 'FontSize', 12, 'FontWeight', 'bold');
    xlabel('X pozíció', 'FontSize', 10);
    ylabel('Y pozíció', 'FontSize', 10);
    axis([0 1 0 1]);  % Feltételezett játéktér határok
    grid on;
    
    % Szöveges címkék az életkorokhoz
    for i = 1:height(targetPositionAvg)
        text(targetPositionAvg.mean_HitPositionX(i) + 0.02, targetPositionAvg.mean_HitPositionY(i), ...
            num2str(targetPositionAvg.Age(i)), 'FontWeight', 'bold');
    end
    
    % Hasonló scatter plot a shooting találati helyekhez
    subplot(2, 2, 2);
    scatter(shootingPositionAvg.mean_HitPositionX, shootingPositionAvg.mean_HitPositionY, 100, shootingPositionAvg.Age, 'filled', 'MarkerEdgeColor', 'k');
    colorbar;
    colormap('cool');
    title('Shooting játék - Átlagos találati helyek életkor szerint', 'FontSize', 12, 'FontWeight', 'bold');
    xlabel('X pozíció', 'FontSize', 10);
    ylabel('Y pozíció', 'FontSize', 10);
    axis([0 1 0 1]);  % Feltételezett játéktér határok
    grid on;
    
    % Szöveges címkék az életkorokhoz
    for i = 1:height(shootingPositionAvg)
        text(shootingPositionAvg.mean_HitPositionX(i) + 0.02, shootingPositionAvg.mean_HitPositionY(i), ...
            num2str(shootingPositionAvg.Age(i)), 'FontWeight', 'bold');
    end
    
    % X és Y pozíciók külön-külön megjelenítése életkoronként
    % X pozíciók
    subplot(2, 2, 3);
    plot(targetPositionAvg.Age, targetPositionAvg.mean_HitPositionX, '-o', 'LineWidth', 2, 'Color', [0.8 0.3 0.3], 'MarkerSize', 8, 'MarkerFaceColor', [0.8 0.3 0.3]);
    hold on;
    plot(shootingPositionAvg.Age, shootingPositionAvg.mean_HitPositionX, '-s', 'LineWidth', 2, 'Color', [0.3 0.3 0.8], 'MarkerSize', 8, 'MarkerFaceColor', [0.3 0.3 0.8]);
    title('Átlagos X találati pozíció életkor szerint', 'FontSize', 12, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 10);
    ylabel('Átlagos X pozíció', 'FontSize', 10);
    grid on;
    legend('Target játék', 'Shooting játék', 'Location', 'best');
    
    % Y pozíciók
    subplot(2, 2, 4);
    plot(targetPositionAvg.Age, targetPositionAvg.mean_HitPositionY, '-o', 'LineWidth', 2, 'Color', [0.8 0.3 0.3], 'MarkerSize', 8, 'MarkerFaceColor', [0.8 0.3 0.3]);
    hold on;
    plot(shootingPositionAvg.Age, shootingPositionAvg.mean_HitPositionY, '-s', 'LineWidth', 2, 'Color', [0.3 0.3 0.8], 'MarkerSize', 8, 'MarkerFaceColor', [0.3 0.3 0.8]);
    title('Átlagos Y találati pozíció életkor szerint', 'FontSize', 12, 'FontWeight', 'bold');
    xlabel('Életkor (év)', 'FontSize', 10);
    ylabel('Átlagos Y pozíció', 'FontSize', 10);
    grid on;
    legend('Target játék', 'Shooting játék', 'Location', 'best');
    
    % Főcím az ábrához
    sgtitle('Találati pozíciók életkor szerint (Target és Shooting játékok)', 'FontSize', 16, 'FontWeight', 'bold');
    
    % Ábra mentése
    saveas(gcf, 'talalati_helyek_eletkor_szerint.png');
    saveas(gcf, 'talalati_helyek_eletkor_szerint.fig');
    disp('Találati helyek ábra sikeresen mentve: talalati_helyek_eletkor_szerint.png és .fig');
    
    % 9. További statisztikák kiírása fájlba
    % Target játék statisztikák táblázat
    targetStatFile = fopen('target_statisztikak.txt', 'w');
    fprintf(targetStatFile, 'TARGET JÁTÉK STATISZTIKÁK ÉLETKORONKÉNT\n');
    fprintf(targetStatFile, '======================================\n\n');
    fprintf(targetStatFile, 'Életkor\tReakció idő átlag\tX pozíció átlag\tY pozíció átlag\tAdatok száma\n');
    
    % Target játék adatszám életkoronként
    targetCounts = groupcounts(targetData, 'Age');
    
    for i = 1:height(targetPositionAvg)
        age = targetPositionAvg.Age(i);
        reactionTime = targetReactionAvg.mean_ReactionTime(targetReactionAvg.Age == age);
        posX = targetPositionAvg.mean_HitPositionX(i);
        posY = targetPositionAvg.mean_HitPositionY(i);
        count = targetCounts.GroupCount(targetCounts.Age == age);
        
        fprintf(targetStatFile, '%d\t%.3f\t\t%.3f\t\t%.3f\t\t%d\n', age, reactionTime, posX, posY, count);
    end
    fclose(targetStatFile);
    disp('Target játék statisztikák mentve: target_statisztikak.txt');
    
    % Shooting játék statisztikák táblázat
    shootingStatFile = fopen('shooting_statisztikak.txt', 'w');
    fprintf(shootingStatFile, 'SHOOTING JÁTÉK STATISZTIKÁK ÉLETKORONKÉNT\n');
    fprintf(shootingStatFile, '=========================================\n\n');
    fprintf(shootingStatFile, 'Életkor\tReakció idő átlag\tX pozíció átlag\tY pozíció átlag\tAdatok száma\n');
    
    % Shooting játék adatszám életkoronként
    shootingCounts = groupcounts(shootingData, 'Age');
    
    for i = 1:height(shootingPositionAvg)
        age = shootingPositionAvg.Age(i);
        reactionTime = shootingReactionAvg.mean_ReactionTime(shootingReactionAvg.Age == age);
        posX = shootingPositionAvg.mean_HitPositionX(i);
        posY = shootingPositionAvg.mean_HitPositionY(i);
        count = shootingCounts.GroupCount(shootingCounts.Age == age);
        
        fprintf(shootingStatFile, '%d\t%.3f\t\t%.3f\t\t%.3f\t\t%d\n', age, reactionTime, posX, posY, count);
    end
    fclose(shootingStatFile);
    disp('Shooting játék statisztikák mentve: shooting_statisztikak.txt');
    
    disp('Elemzés sikeresen befejezve!');
    
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