% Adatok betöltése
data = readtable('simon_scores_by_generation.csv');

% ANOVA teszt: pontszám generációnként
anova1(data.Score, data.Generation);

% (opcionálisan) Boxplot a vizualizációhoz
figure;
boxplot(data.Score, data.Generation);
title('Simon Scores by Generation');
ylabel('Score');
xlabel('Generation');