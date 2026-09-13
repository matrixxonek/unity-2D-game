using UnityEngine;

/// Roulettevolver — znak firmowy gry.
///
/// Bęben ma sześć komór. Każdy krok sekwencji ładuje jedną, zużywając rzadki nabój.
/// Sekwencja to ruch po okręgu (dół, lewo, góra, prawo...) — dosłownie obracanie bębna.
///
/// Ponieważ sekwencję można przerwać w dowolnym momencie, liczba załadowanych komór
/// jest decyzją gracza, a ona wprost ustala szansę na strzał: przy N komorach szansa
/// wynosi N/6. Jedna komora to dokładnie "1 na 6" z pierwotnego dokumentu — czyli
/// stan minimalny, nie stała gry.
///
/// Bęben obraca się deterministycznie po losowym ułożeniu naboi, więc na sześć
/// pociągnięć spustu gracz dostanie dokładnie tyle strzałów, ile załadował.
/// Rzadka amunicja nigdy nie przepada — nieznana jest tylko kolejność.
public class Revolver : Weapon
{
    [Header("Rewolwer")]
    public int chambers = 6;
    public int damage = 40;
    public float range = 16f;
    public float fireCooldown = 0.5f;

    [Header("Ruletka")]
    [Tooltip("WŁĄCZONE: bęben obraca się komora po komorze, pusta = klik. " +
             "Załadowane naboje i tak wystrzelisz — losowa jest tylko kolejność.\n" +
             "WYŁĄCZONE: każde pociągnięcie spustu strzela, dopóki są naboje w bębnie.")]
    public bool rouletteMode = true;

    [Tooltip("Krótszy cooldown po pustym kliku — dud nie ma karać tak samo jak strzał.")]
    public float clickCooldown = 0.15f;

    static readonly SeqInput[] Crank =
    {
        SeqInput.Down, SeqInput.Left, SeqInput.Up, SeqInput.Right
    };

    public int LoadedCount { get; private set; }
    public bool IsReloading { get; private set; }
    public bool LastPullWasClick { get; private set; }

    bool[] loaded;
    int cylinderPos;
    float nextFireTime;

    protected override void Awake()
    {
        base.Awake();
        displayName = "Rewolwer";
        requiredAbility = Ability.Revolver;
        loaded = new bool[chambers];
    }

    public override string StatusLine
    {
        get
        {
            string cylinder = $"bęben {LoadedCount}/{chambers}";
            if (rouletteMode) cylinder += $"  (szansa {LoadedCount}/{chambers})";
            return Pouch == null ? cylinder : $"{cylinder}  zapas {Pouch.revolverRounds}";
        }
    }

    public override void PullTrigger()
    {
        if (!IsOwned || IsReloading || Time.time < nextFireTime) return;

        if (LoadedCount <= 0)
        {
            Click();
            if (Pouch != null && Pouch.revolverRounds > 0) HUD.Message("Pusty bęben — R, potem obrót ▼◀▲▶", 1.4f);
            return;
        }

        if (!rouletteMode)
        {
            // Bęben pomija puste komory — każde pociągnięcie spustu strzela.
            for (int i = 1; i <= chambers; i++)
            {
                int index = (cylinderPos + i) % chambers;
                if (!loaded[index]) continue;

                cylinderPos = index;
                Fire(index);
                return;
            }
            return;
        }

        cylinderPos = (cylinderPos + 1) % chambers;

        if (loaded[cylinderPos]) Fire(cylinderPos);
        else Click();
    }

    void Fire(int chamber)
    {
        LastPullWasClick = false;
        loaded[chamber] = false;
        LoadedCount--;
        nextFireTime = Time.time + fireCooldown;

        Sfx.Play("revolver");
        CameraShake.Shake(0.45f, 0.3f);
        Owner.Knockback(new Vector2(-Owner.Facing * 3.5f, 0f), 0.08f);

        var target = FireHitscan(range, new Color(1f, 0.6f, 0.25f), out Vector2 end);
        if (target != null)
        {
            HitStop.Do(0.12f);
            target.TakeDamage(damage, transform.position);
            Particles.Burst(end, new Color(1f, 0.7f, 0.3f), 16, 7f, 0.4f, 14f, 0.12f);
        }
        else HitStop.Do(0.04f);

        PlayerEvents.RaiseFired();
    }

    void Click()
    {
        LastPullWasClick = true;
        nextFireTime = Time.time + clickCooldown;
        Sfx.Play("click");
    }

    public override void BeginReload()
    {
        if (!IsOwned || IsReloading || LoadedCount >= chambers) return;
        if (Pouch.revolverRounds <= 0)
        {
            HUD.Message("Brak naboi do rewolweru", 1.2f);
            return;
        }
        if (!Sequences.CanBegin()) return;

        // Ładujemy tylko brakujące komory, więc dobicie bębna jest krótsze niż pełne.
        int needed = Mathf.Min(chambers - LoadedCount, Pouch.revolverRounds);
        var pattern = new SeqInput[needed];
        for (int i = 0; i < needed; i++) pattern[i] = Crank[i % Crank.Length];

        IsReloading = true;
        Sequences.Begin(new SequenceRequest
        {
            label = "ŁADOWANIE BĘBNA",
            pattern = pattern,
            // Każdy krok commituje komorę, więc nie ma czego cofać przy pomyłce.
            // Zacinanie się dochodzi dopiero z modułem "Szybkoładowarka".
            resetOnMistake = false,
            onStep = _ => LoadOneChamber(),
            onComplete = () => IsReloading = false,
            onCancel = () => IsReloading = false,
        });
    }

    void LoadOneChamber()
    {
        if (LoadedCount >= chambers || !Pouch.TakeRevolver())
        {
            Sequences.Cancel();
            return;
        }

        // Losowa komora — gracz nie wie, w której kolejności wypadną strzały.
        int empties = chambers - LoadedCount;
        int pick = Random.Range(0, empties);
        for (int i = 0; i < chambers; i++)
        {
            if (loaded[i]) continue;
            if (pick-- > 0) continue;

            loaded[i] = true;
            LoadedCount++;
            PlayerEvents.RaiseRevolverLoaded();
            return;
        }
    }
}
