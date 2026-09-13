# The Doomsday Recipe — dokument projektowy

> Zastępuje wcześniejszy dokument koncepcyjny (`gra.pdf`). Tamten zakładał grę
> poziomową z systemem RPG i kilkoma minigrami; ten opisuje metroidvanię w duchu
> Hollow Knight. Sekcja [Co zostało wycięte](#co-zostało-wycięte-i-dlaczego)
> tłumaczy różnice.

---

## 1. Pitch

Obłąkany naukowiec wraca do miasta, które sam zniszczył, żeby zebrać z niego
materiał na bombę, która tym razem zadziała. Nie jest wojownikiem — jest starym
człowiekiem z jednym narzędziem i prowizorycznym sprzętem, więc każda jego
zdolność to procedura, którą trzeba wykonać, a nie przycisk do wciśnięcia.

**Gatunek:** metroidvania 2D
**Referencja:** Hollow Knight (struktura świata, ekonomia leczenia, progresja przez moduły)
**Hook:** walka i przetrwanie oparte na sekwencjach wejść — sterowanie jako charakterystyka postaci

---

## 2. Fabuła

**Intro.** Korporacja zatrudnia naukowca do zbudowania bomby niszczącej planetę.
Bomba spada na miasto, załoga ogląda wybuch z rakiety — i widzi tylko mały błysk.
Planeta nie wybucha. Naukowiec zostaje zwolniony za spartaczoną robotę i popada
w obłęd.

**Zawiązanie.** Po latach knucia stwierdza, że do nowej bomby brakuje mu próbek
z zainfekowanego miasta. Miasto zostało odizolowane grubym murem; z powodu
promieniowania na powierzchni jego mieszkańcy zeszli pod ziemię i rozbudowali
sieć korytarzy. Z czasem zaczęły się tam pojawiać zmutowane istoty.

**Rdzeń.** Gracz eksploruje podziemia, zdobywa umiejętności ruchu otwierające
kolejne obszary i zbiera próbki potrzebne do receptury.

**Zakończenie.** Naukowiec wraca do korporacji z gotowym planem, żeby udowodnić
swój geniusz. Korporacja go przyjmuje, bomba powstaje. Moment przed startem
rakiety naukowiec zostaje z niej kreskówkowo wykopany przez pracownika korpo.
Zostaje na ziemi i ginie od własnej bomby — grzyb atomowy odbijający się w jego
oczach. W napisach: zmutowany świat, zmutowany naukowiec, i weseli członkowie
korporacji cali i zdrowi na innej planecie.

---

## 3. Hook: sekwencje wejść

Naukowiec nie ma naturalnej zdolności walki. Wszystko, co robi, jest procedurą
obsługi prowizorycznego sprzętu. To jest charakter postaci wyrażony schematem
sterowania.

**W wersji 1.0 istnieją dokładnie dwie sekwencje: przeładowanie i leczenie.**
Mają być wyszlifowane do perfekcji, zanim cokolwiek innego dostanie sekwencję.
Dwie rzeczy robione znakomicie są rozpoznawalne; pięć rozrzuconych po grze to
maniera.

### Zasady projektowe sekwencji

Obowiązują bez wyjątku — każda z nich odpowiada za to, że mechanika buduje
napięcie zamiast być podatkiem:

1. **Zawsze w świecie, nigdy w menu.** Sekwencja dzieje się w miejscu, w którym
   stoisz, i kosztuje bezbronność. To jej cena.
2. **Przerwanie nie kasuje postępu.** Oberwiesz przy trzecim wejściu z sześciu —
   masz załadowane trzy. To jedyna rzecz oddzielająca napięcie od kary.
3. **Długość skaluje się z mocą, nie z częstotliwością.** Pistolet używany co
   kilka sekund dostaje jedno–dwa wejścia. Rewolwer kładący bossa dostaje pełną
   ceremonię. Moc = ceremonia.
4. **Nigdy w trakcie parkouru.** Sekwencje wykonuje się na ziemi. Skakanie
   i sekwencje to dwa różne tryby uwagi i nie mieszają się.
5. **Diegetyczność.** Rewolwer ma sześć komór, więc do sześciu wejść. Załadowałeś
   dwie i strzelasz dwoma — to decyzja, nie ograniczenie.

Sekwencje to pamięć mięśniowa. Po kilku godzinach gracz wykonuje je odruchowo
i to jest zamierzone: mistrzostwo ma być widoczne. Tym różnią się od losowej
frustracji — tamta nigdy nie przestaje boleć.

---

## 4. Rdzeń rozgrywki

### 4.1 Próbnik

Podstawowe i jedyne narzędzie walki wręcz: teleskopowy pręt z chwytakiem na
końcu, służący do pobierania materiału bez dotykania go ręką. Przywiózł ze sobą
dokładnie jedno narzędzie — to, którego potrzebował do misji.

Pełni trzy funkcje jednocześnie:

| Kontekst | Działanie |
|---|---|
| Przeciwnik | Dźgnięcie — podstawowy atak |
| Złoże / próbka | Pobranie materiału (ten sam przycisk, bez osobnej interakcji) |
| Atak w dół | **Pogo** — odbicie od przeciwnika lub przeszkody |

Pogo jest jednocześnie walką i trawersowaniem — to znak firmowy gatunku i główny
powód, dla którego naskok z pierwotnego dokumentu awansuje z „opcji obrażeń" na
filar rozgrywki.

Długi zasięg czyta się w sylwetce i naturalnie skaluje: wydłużenie teleskopu to
czytelny upgrade, w odróżnieniu od „+3 do obrażeń".

### 4.2 Ładunek i leczenie

Trafienie próbnikiem **pobiera ładunek** z celu (biomasa, promieniowanie).
Ładunek zasila prowizoryczny zabieg polowy, czyli leczenie.

**Żeby się wyleczyć, trzeba najpierw kogoś dźgnąć.** Nie da się uciec i odespać —
trzeba wejść w walkę, żeby z niej wyjść. To silnik napięcia całej gry.

Leczenie jest sekwencją: kilka wejść, wykonywane w miejscu, przerywalne, obrywasz
w trakcie — tracisz to, co włożyłeś (ale nie ładunek, patrz zasada 2).

Brak mikstur i przedmiotów leczących. **Ładunek służy wyłącznie leczeniu.**

> **Do obserwacji w testach:** ładunek można bankować bez limitu, więc gracz może
> nabić pełny pasek na słabych przeciwnikach przed bossem. Hollow Knight na to
> pozwala i działa, więc na start nie komplikujemy — ale jeśli okaże się to
> problemem, rozwiązaniem jest cap lub powolny zanik poza walką.

### 4.3 Broń palna

Bronie palne to narzędzia do konkretnych problemów, nie podstawowy atak —
amunicja jest rzadka, a przeładowanie kosztuje czas.

**Pistolet.** Duży magazynek, amunicja znajdowana w świecie. Przeładowanie to
krótka sekwencja (1–2 wejścia). Nagroda za eksplorację.

**Rewolwer (roulettevolver).** Ogromne obrażenia, ale **szansa 1 na 6, że
wypali** — losowość jako mechanika hazardu u człowieka, który stawia wszystko na
jedną bombę. Własna, bardzo rzadka amunicja, osobna od pistoletowej.
Przeładowanie to pełna ceremonia: do sześciu wejść, po jednym na komorę,
przerywalne.

Rzadkość amunicji rewolwerowej tworzy decyzję eksploracyjną: trzymam te trzy
naboje na bossa czy ratuję się teraz?

### 4.4 Ekonomia zasobów

Trzy niezależne waluty, każda z innym źródłem i innym przeznaczeniem:

| Zasób | Źródło | Wydawany na |
|---|---|---|
| Ładunek | Trafienia próbnikiem | Leczenie |
| Amunicja pistoletowa | Znajdowana w świecie (częsta) | Pistolet |
| Amunicja rewolwerowa | Znajdowana w świecie (bardzo rzadka) | Rewolwer |

Materiały na moduły i ulepszenia to osobna kategoria — patrz sekcja 5.

---

## 5. Progresja

Trzy niezależne tory. Żaden nie jest liniowym paskiem statystyk.

### 5.1 Poziomy próbnika

Surowa moc. Kilka dyskretnych poziomów, ulepszanych w laboratorium za rzadki
materiał znajdowany po jednym na obszar (odpowiednik pale ore). Wyższy poziom to
większe obrażenia i dłuższy zasięg — a dłuższy zasięg zmienia geometrię pogo,
więc bywa też bramką dostępu.

### 5.2 Moduły

Odpowiednik charmów. Znajdujesz materiały w świecie, w laboratorium budujesz
z nich moduł. Moduł jest **permanentny**; założyć możesz ograniczoną liczbę
(system slotów kosztowych — odpowiednik notchy). Zero grindu, zero zarządzania
plecakiem, pełne poczucie budowania.

**Kluczowe: część modułów modyfikuje sekwencje.** Dzięki temu system progresji
*jest* systemem sekwencji — nie masz dwóch niepowiązanych mechanik, tylko jedną,
która rośnie. Build gracza odpowiada na pytanie „jakim jestem operatorem tego
sprzętu", a nie „jakie mam statystyki".

Przykłady:

| Moduł | Efekt | Koszt / wada |
|---|---|---|
| Autoloader | Przeładowanie pistoletu z 3 wejść do 1 | Magazynek o połowę mniejszy |
| Szybkoładowarka | Rewolwer ładuje 3 komory na jedno wejście | Przy pomyłce się zacina |
| Stabilizator | Sekwencję leczenia można wykonać w powietrzu | Wysoki koszt slotów |
| Sprzężenie | Leczenie o połowę krótsze | Leczy o połowę mniej |
| Teleskop wzmocniony | Dłuższy zasięg próbnika | Wolniejszy zamach |

### 5.3 Umiejętności ruchu (bramki)

Klasyczne gate'y metroidvanii. Kolejność orientacyjna:

1. **Double jump** — wcześnie
2. **Odbijanie od ścian** — wcześnie
3. **Chwyt liny** (przeskakiwanie jak Tarzan) — środek gry, otwiera pionowe szyby
4. **Odporność na kwas** — środek/późno, otwiera obszary zalane kwasem
5. **Wydłużone nurkowanie** — późno, otwiera przejścia podwodne

Jetpack z pierwotnego dokumentu **odpada** — swobodny lot rozbraja całą strukturę
bramek, na której stoi gatunek.

---

## 6. Struktura świata

### 6.1 Hub: laboratorium

Korytarz o niskim promieniowaniu, w którym naukowiec urządza laboratorium.
Odpowiednik Dirtmouth. Mieści:

- **Ławkę** — zapis stanu, punkt odrodzenia, przepinanie modułów
- **Stół warsztatowy** — budowa modułów, ulepszanie próbnika
- **Ścianę wariata** — zdjęcia, sznurek, notatki. Diegetyczny dziennik zadań
  pokazujący, których próbek jeszcze brakuje

Ławki występują też w świecie, rozstawione jak w Hollow Knight.

### 6.2 Biomy

W wersji 1.0 **trzy obszary**, nie pięć. Powód nie jest tylko ilościowy: obszary
metroidvanii muszą się łączyć w wielu miejscach, więc N obszarów to około N²
decyzji o połączeniach i skrótach. Przy pięciu to się rozjeżdża.

| Biom | Charakter | Status |
|---|---|---|
| Grzybi | Organiczny, zarodniki, świecące grzyby | v1 |
| Industrialny | Mechaniczny, zerwane kable, ruchome platformy | v1 |
| Zamarznięty | Awaria systemu chłodniczego, kontrast wizualny | v1 |
| Slime | Organiczna maź | odłożony — dubluje się z grzybim |
| Zarośnięty roślinami | Roślinność | odłożony |

Obszary muszą łączyć się bezpośrednio, nie tylko przez hub, i zawierać skróty
otwierane od drugiej strony.

### 6.3 Podróż: pojazd wiertniczy

Pojazd wiertniczy **nie jest minigrą**. To stacje szybkiej podróży (odpowiednik
Stagways) odblokowywane przez odnalezienie i uruchomienie. Ta sama fikcja
podziemnego pojazdu, zero kosztu drugiej gry.

### 6.4 Mapy

Mapa każdego obszaru jest niedostępna, dopóki nie zdobędziesz jej od ocalałego
kartografa — którego trzeba okłamać (patrz sekcja 7).

---

## 7. Ocalali i dialogi

W tunelach żyją jeszcze ludzie. Rozmowa daje wybór jednej z dwóch odpowiedzi:

- **Prawda** — mówisz, że chcesz zniszczyć planetę. Uciekają. **Ten NPC znika ze
  świata na stałe** wraz ze wszystkim, co mógł ci jeszcze dać.
- **Kłamstwo** — mówisz, że chcesz ich uratować. Dostajesz przedmiot, materiał
  albo oznaczenie na mapie.

To jest mechanika, która wypowiada temat zamiast go ilustrować: gra **nagradza**
granie potworem i **karze** szczerość. W metroidvanii zyskuje dodatkowo, bo do
NPC-ów się wraca — trwała utrata jest realnym kosztem, a nie linijką dialogu.

Licznik okłamanych ludzi widoczny w napisach końcowych.

---

## 8. Próbki i cel gry

Próbki są **skutkiem eksploracji, nie listą zadań.** Gracz wchodzi do obszaru
z ciekawości; próbka jest za bramką umiejętnościową albo u bossa.

**Jedna unikalna próbka na biom + drop z bossa.** Nie pięć próbek w każdym
biomie — to byłoby 25 obiektów do zebrania i grzyb w biomie industrialnym nie
miałby sensu. Każdy biom dostaje w ten sposób własną tożsamość.

Lista brakujących próbek mieszka na ścianie wariata w laboratorium.

---

## 9. Przeciwnicy

| Przeciwnik | Zachowanie | Zalecana odpowiedź |
|---|---|---|
| Mysz | Szybka, słaba | Pogo |
| Mucha | Lata przy suficie, popycha, drobne obrażenia | Pistolet |
| Zwykły zombie | Szybki, zmutowany człowiek | Próbnik |
| Zmutowany zombie | Wolny, atak obszarowy uderzeniem w ziemię, dużo HP | Pistolet z dystansu |
| Większy zmutowany | Umiejętności zależne od biomu | Zależnie od biomu |
| Slime | Powolny, bardzo dużo HP | Rewolwer |
| Boss | Umiejętności specjalne, jeden na biom | Rewolwer + pełne opanowanie sekwencji |

Mrówki z pierwotnego dokumentu były przeciwnikami minigry z wiertłem — do
przeprojektowania na zwykłego przeciwnika biomu albo wycięcia.

**Balans obrażeń — zasada, nie liczby:** pogo musi być wyraźnie słabsze od broni
palnej, inaczej gracz zoptymalizuje grę do skakania po głowach i zignoruje cały
arsenał. Pogo ma być narzędziem trawersowania i utrzymania tempa, nie
najefektywniejszym źródłem obrażeń. Konkretne wartości do wytunowania na vertical
slice.

---

## 10. Środowisko

**Transport i trawersowanie**
- Liny do przeskakiwania
- Winda
- Tyrolka (zjazd w dół tyrolką, powrót windą — jednokierunkowy skrót)
- Woda, w której można zanurkować na ograniczony czas
- Kwas zadający obrażenia natychmiast

**Przeszkody**
- Kryształy w kształcie kolców
- Zerwane iskrzące kable
- Spadające krople kwasu
- Zwisające kontenery na łańcuchach jako huśtające się platformy
- Kryształowe platformy kruszące się pod ciężarem gracza

Woda i kwas pełnią podwójną rolę: przeszkody i bramki (patrz 5.3).

---

## 11. Co zostało wycięte i dlaczego

| Element | Powód |
|---|---|
| Struktura poziomowa | Niespójna z metroidvanią; biomy są połączonym światem, nie leveliami |
| Minigra z wiertłem | Druga gra w środku gry, na ścieżce krytycznej, powtarzana 10+ razy |
| Sloty ekwipunku (Head/Chest/Legs/Boots/Weapon) | System z action-RPG; loot i porównywanie statystyk przerywają eksplorację |
| Mikstury leczenia i przedmioty zużywalne | Zamieniają leczenie w zasób do gromadzenia; wymuszają farmienie przed bossem |
| Sekwencja przy craftingu | Laboratorium to strefa bezpieczna, friction nic tam nie buduje |
| Puzzle ze światełkami w windzie | Osobna mechanika udająca hook |
| Naprawa latarki sekwencją | j.w.; latarka ewentualnie wraca jako moduł, bez sekwencji |
| Jetpack | Swobodny lot rozbraja strukturę bramek |
| 2 z 5 biomów | Slime dubluje się z grzybim; koszt połączeń rośnie kwadratowo |
| 5 próbek na biom | 25 obiektów do zebrania; wypełniacz zamiast tożsamości obszaru |

Sekcja „Elementy frustrujące" z pierwotnego dokumentu nie została wycięta, tylko
**przeprojektowana** — jej intencja żyje w sekwencjach (sekcja 3), ale z twardymi
zasadami, które zamieniają frustrację w napięcie i mistrzostwo.

Ciemność jako mechanika (latarka) jest odłożona świadomie: walczy z czytelnością
mapy, a mapa jest w metroidvanii wszystkim.

---

## 12. Plan produkcji

**Następny krok to vertical slice, nie kolejny biom.**

Jeden pokój zawierający:
- ruch: chodzenie, skok, double jump
- próbnik: dźgnięcie, pogo, pobranie próbki
- jeden typ przeciwnika
- ładunek i sekwencja leczenia
- pistolet z sekwencją przeładowania
- jedna ławka
- jeden ocalały z wyborem dialogowym

**Cel:** sprawdzić, czy skakanie, pogo i sekwencje *dobrze się czują*. Jeśli tak —
reszta to praca. Jeśli nie — dobrze, że nie zbudowaliśmy tego trzy razy.

Rewolwer i moduły dochodzą dopiero po tym, jak vertical slice zostanie uznany za
przyjemny.

---

## 13. Do rozstrzygnięcia

- Konkretne wartości obrażeń i HP (do wytunowania na vertical slice)
- Liczba slotów na moduły i koszty poszczególnych modułów
- Czy ładunek wymaga capu lub zaniku poza walką (patrz 4.2)
- Czy mrówki wracają jako przeciwnik biomu, czy wypadają
- Docelowa długość gry — wpływa na to, ile modułów i obszarów trzeba zaprojektować
- Struktura zapisu stanu: co dokładnie persystuje między sesjami (ławki, otwarte
  skróty, zabici bossowie, NPC-e, którzy uciekli)
