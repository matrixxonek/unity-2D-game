# Prototyp — samouczek (Tutorial.unity)

Grywalny vertical slice: jeden poziom uczący wszystkich mechanik z GDD, zakończony bossem.

## Uruchomienie

W Unity: menu **Doomsday**

1. `1. Generate Placeholder Art` — generuje pixel-artowe PNG do `Assets/Art/Generated/`
   i wpina je w `Assets/Resources/SpriteLibrary.asset`.
2. `2. Build Tutorial Level` — buduje od zera `Assets/Scenes/Tutorial.unity` i ustawia ją jako
   scenę 0 w Build Settings.
3. `3. Build Tutorial Level and Play` — to samo + play mode.

Scena jest w całości generowana z [`Assets/Editor/TutorialLevelBuilder.cs`](../My%20project/Assets/Editor/TutorialLevelBuilder.cs)
(metoda `BuildLevel()`): zmień liczby, odbuduj. Nie edytuj sceny ręcznie — kolejny build ją nadpisze.

## Sterowanie

| Akcja | Klawiatura | Pad |
|---|---|---|
| Ruch | A / D | lewa gałka |
| Skok / podwójny skok | SPACJA (przytrzymaj = wyżej) | A |
| Próbnik | J (W+J w górę, S+J w powietrzu = pogo) | X |
| Strzał | K | B |
| Zmiana broni | TAB | RB |
| Przeładowanie (sekwencja) | R, potem strzałki | LB, potem d-pad |
| Zabieg (sekwencja) | H, potem strzałki | LT, potem d-pad |
| Ławka | E | Y |
| Pauza | ESC | Start |

Strzałki / d-pad są zarezerwowane na sekwencje — nie chodzą. Lewa ręka steruje postacią,
prawa wykonuje procedury.

## Sekwencje (v1.0)

| | Wzór | Uwagi |
|---|---|---|
| Przeładowanie pistoletu | DÓŁ, GÓRA | pomyłka cofa do zera |
| Ładowanie bębna | DÓŁ, LEWO, GÓRA, PRAWO, DÓŁ, LEWO | jedna strzałka = jedna komora; K przerywa i strzela tym, co jest |
| Zabieg | DÓŁ, DÓŁ, GÓRA | koszt 3 ładunku; ładunek zdobywasz trafiając próbnikiem |

Ruch, skok i obrażenia przerywają sekwencję. Załadowane komory zostają.

## Przebieg poziomu

A ruch/skok → B podwójny skok → C próbnik i ładunek → D jama z zombie + zabieg (brama otwiera się po leczeniu)
→ E pogo nad kolcami → F pistolet, przeładowanie, ciężki mutant → G kruszące platformy, próbka, ławka
→ H rewolwer i bęben → I boss ALFA → J wyjście.

Śmierć = powrót na ostatnią ławkę. Kolce = 1 obrażenie + powrót na ostatnie bezpieczne miejsce.
Arena bossa resetuje się po śmierci gracza.

## Test automatyczny

`Autopilot` ([`Assets/Scripts/Debug/Autopilot.cs`](../My%20project/Assets/Editor/../Scripts/Debug/Autopilot.cs))
przechodzi cały poziom sam i raportuje PASS/FAIL. W play mode, z dowolnego skryptu edytorowego / konsoli:

```csharp
Autopilot.Begin();          // start
Autopilot.Log.ToString();   // raport
Autopilot.Finished;         // koniec?
```

Ostatni przebieg: 34 PASS / 0 FAIL, 0 zgonów, 69 s. Nie edytuj skryptów w trakcie — domain reload
kasuje stan testu.

## Grafika

Wszystkie sprite'y to placeholdery z `ArtGenerator`. Podmiana na docelową grafikę:
przypnij inne sprite'y w `Assets/Resources/SpriteLibrary.asset` (albo podmień PNG w `Art/Generated`
zachowując nazwy i PPU 32) i odbuduj poziom.

## Gałki do strojenia

- Skok: `PlayerMovement` (siła 15, trzy grawitacje, coyote, buffer)
- Boss: `Boss` (szarża 12, telegraf 0.75 s, ogłuszenie 2.3 s), HP w builderze (80)
- Rewolwer: `Revolver.rouletteMode`, obrażenia 40
- Ładunek: `ChargeMeter` (9 max, zabieg 3), leczenie 2 HP
