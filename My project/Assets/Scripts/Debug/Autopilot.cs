using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// Automatyczne przejście samouczka — test regresji całej pętli gry.
/// Steruje graczem przez PlayerInputReader.Scripted i zapisuje PASS/FAIL do Log.
/// Uruchomienie: Autopilot.Begin() w play mode (np. z konsoli MCP).
public class Autopilot : MonoBehaviour
{
    public static Autopilot I { get; private set; }
    public static readonly StringBuilder Log = new StringBuilder();
    public static bool Finished { get; private set; }
    public static int Passes { get; private set; }
    public static int Failures { get; private set; }
    public static string Stage { get; private set; } = "";

    PlayerInputReader.ScriptedInput input;
    Transform t;
    PlayerMovement move;
    Rigidbody2D rb;
    Health hp;
    ChargeMeter charge;
    PlayerAbilities abilities;
    PlayerRespawn respawn;
    WeaponHolder holder;
    SequenceRunner sequences;
    readonly HashSet<string> seen = new HashSet<string>();
    float nextProbe, nextFire;

    public static void Begin()
    {
        if (I != null) Destroy(I.gameObject);
        Log.Length = 0;
        Finished = false; Passes = 0; Failures = 0; Stage = "";
        new GameObject("Autopilot").AddComponent<Autopilot>();
    }

    public static void Stop()
    {
        if (I != null) Destroy(I.gameObject);
    }

    void Awake() => I = this;

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm == null) { Fail("brak gracza w scenie"); Finished = true; return; }

        t = pm.transform;
        move = pm;
        rb = pm.GetComponent<Rigidbody2D>();
        hp = pm.GetComponent<Health>();
        charge = pm.GetComponent<ChargeMeter>();
        abilities = pm.GetComponent<PlayerAbilities>();
        respawn = pm.GetComponent<PlayerRespawn>();
        holder = pm.GetComponent<WeaponHolder>();
        sequences = pm.GetComponent<SequenceRunner>();

        input = new PlayerInputReader.ScriptedInput();
        PlayerInputReader.Scripted = input;
        hp.Damaged += OnPlayerDamaged;

        PlayerEvents.Moved += OnMoved; PlayerEvents.Jumped += OnJumped; PlayerEvents.DoubleJumped += OnDoubleJumped;
        PlayerEvents.ProbeHit += OnProbeHit; PlayerEvents.Pogoed += OnPogoed; PlayerEvents.Healed += OnHealed;
        PlayerEvents.Fired += OnFired; PlayerEvents.Reloaded += OnReloaded; PlayerEvents.WeaponSwitched += OnSwitched;
        PlayerEvents.RevolverLoaded += OnRevolverLoaded; PlayerEvents.SampleCollected += OnSample; PlayerEvents.Rested += OnRested;

        StartCoroutine(Run());
    }

    void OnPlayerDamaged(DamageInfo info)
    {
        string source = "?";
        float best = 3f;
        foreach (var c in Physics2D.OverlapCircleAll(t.position, 3f))
        {
            if (c.transform.IsChildOf(t)) continue;
            if (c.GetComponentInParent<ContactDamage>() == null && c.GetComponentInParent<Hazard>() == null && c.GetComponentInParent<Shockwave>() == null) continue;
            float d = Vector2.Distance(c.transform.position, t.position);
            if (d < best) { best = d; source = c.transform.root.name + "/" + c.name; }
        }
        Say(Pos($"obrażenia -{info.amount} (HP {hp.Current}) od: {source}{(info.hasSource ? $" @{info.source.x:0.0},{info.source.y:0.0}" : "")}"));
    }

    void OnDestroy()
    {
        if (hp != null) hp.Damaged -= OnPlayerDamaged;
        if (PlayerInputReader.Scripted == input) PlayerInputReader.Scripted = null;
        PlayerEvents.Moved -= OnMoved; PlayerEvents.Jumped -= OnJumped; PlayerEvents.DoubleJumped -= OnDoubleJumped;
        PlayerEvents.ProbeHit -= OnProbeHit; PlayerEvents.Pogoed -= OnPogoed; PlayerEvents.Healed -= OnHealed;
        PlayerEvents.Fired -= OnFired; PlayerEvents.Reloaded -= OnReloaded; PlayerEvents.WeaponSwitched -= OnSwitched;
        PlayerEvents.RevolverLoaded -= OnRevolverLoaded; PlayerEvents.SampleCollected -= OnSample; PlayerEvents.Rested -= OnRested;
        if (I == this) I = null;
    }

    void OnMoved() => seen.Add("Moved");
    void OnJumped() => seen.Add("Jumped");
    void OnDoubleJumped() => seen.Add("DoubleJumped");
    void OnProbeHit() => seen.Add("ProbeHit");
    void OnPogoed() => seen.Add("Pogoed");
    void OnHealed() => seen.Add("Healed");
    void OnFired() => seen.Add("Fired");
    void OnReloaded() => seen.Add("Reloaded");
    void OnSwitched() => seen.Add("WeaponSwitched");
    void OnRevolverLoaded() => seen.Add("RevolverLoaded");
    void OnSample() => seen.Add("SampleCollected");
    void OnRested() => seen.Add("Rested");

    // ------------------------------------------------------------ scenariusz

    IEnumerator Run()
    {
        yield return Wait(1.8f);
        Stage = "start";
        Check(HUD.I != null, "HUD istnieje");
        Check(GameFlow.I != null, "GameFlow istnieje");
        yield return WaitUntil(() => move.IsGrounded, 3f);
        Check(move.IsGrounded, Pos("gracz stoi na ziemi po starcie"));

        // Ławka na starcie: punkt odrodzenia na wypadek zgonu w pierwszych etapach.
        yield return MoveTo(3.2f, 4f);
        yield return Wait(0.2f);
        input.interact = true;
        yield return Wait(0.4f);
        Check(seen.Contains("Rested"), "odpoczynek na pierwszej ławce");

        yield return StageWithRetry("A ruch/skok", 3.2f, StageA);
        yield return StageWithRetry("B double jump", 26f, StageB);
        yield return StageWithRetry("C próbnik", 40f, StageC);
        yield return StageWithRetry("D leczenie", 61f, StageD);
        yield return StageWithRetry("E pogo", 82.5f, StageE);
        yield return StageWithRetry("F pistolet", 99f, StageF);
        yield return StageWithRetry("G platformy/próbka/ławka", 125f, StageG);
        yield return StageWithRetry("H rewolwer", 148.5f, StageH);
        yield return StageWithRetry("I boss", 156f, StageI, 8);
        yield return StageWithRetry("J wyjście", 184.5f, StageJ);

        Stage = "done";
        Say($"KONIEC: PASS={Passes} FAIL={Failures} śmierci={respawn.Deaths} czas={Time.time:0}s");
        Finished = true;
        PlayerInputReader.Scripted = null;
    }

    IEnumerator StageA()
    {
        yield return MoveTo(9f, 6f);
        Check(seen.Contains("Moved"), "zdarzenie Moved");
        yield return RunAndJump(1, 15.5f, 10f, 11.2f, 13.2f);
        yield return MoveTo(16f, 4f);
        Check(t.position.x > 14.5f && t.position.y > 2.2f, Pos("na półce A_Ledge"));
        Check(seen.Contains("Jumped"), "zdarzenie Jumped");
    }

    IEnumerator StageB()
    {
        yield return MoveTo(29f, 10f);
        yield return Wait(0.4f);
        Check(abilities.doubleJump, "double jump odblokowany");
        yield return MoveTo(32.6f, 5f);
        for (int attempt = 0; attempt < 3 && t.position.x < 36.2f; attempt++)
        {
            yield return DoubleJump(1, 37.5f, 5f);
            if (t.position.x < 36.2f) yield return MoveTo(32.6f, 5f);
        }
        Check(t.position.x > 36.2f, Pos("za ścianą B"));
        Check(seen.Contains("DoubleJumped"), "zdarzenie DoubleJumped");
    }

    IEnumerator StageC()
    {
        yield return MoveTo(46f, 8f);
        yield return FightWalkers(60f, 30f);
        Check(seen.Contains("ProbeHit"), "zdarzenie ProbeHit");
        Check(charge.Current > 0, $"ładunek > 0 (jest {charge.Current})");
    }

    IEnumerator StageD()
    {
        yield return MoveTo(64.5f, 10f);
        yield return WaitUntil(() => move.IsGrounded, 3f);
        int chargeBefore = charge.Current;
        input.holdUp = true;
        yield return ProbeSpam(1.0f);
        input.holdUp = false;
        Check(charge.Current > chargeBefore || charge.Current == charge.maxCharge, $"kryształ dał ładunek ({chargeBefore} -> {charge.Current})");
        yield return FightWalkers(73.5f, 35f);
        yield return RunAndJump(1, 77.4f, 12f, 73.4f, 75.5f);
        yield return MoveTo(77.6f, 5f);
        yield return WaitUntil(() => move.IsGrounded, 2f);
        if (hp.Current < hp.maxHealth)
        {
            Say($"HP {hp.Current}/{hp.maxHealth}, ładunek {charge.Current} — leczę");
            if (!charge.CanAffordHeal)
            {
                yield return MoveTo(64.5f, 10f);
                input.holdUp = true; yield return ProbeSpam(1.0f); input.holdUp = false;
                yield return RunAndJump(1, 77.4f, 12f, 73.4f, 75.5f);
                yield return MoveTo(77.6f, 5f);
            }
            yield return Wait(0.3f);
            input.heal = true;
            yield return Wait(0.25f);
            yield return Arrows(0.2f, SeqInput.Down, SeqInput.Down, SeqInput.Up);
            yield return Wait(0.4f);
            Check(seen.Contains("Healed"), $"zdarzenie Healed (HP teraz {hp.Current})");
        }
        else Say("HP pełne — brama otwiera się bez zabiegu");
        var healGate = Gate.Find("heal");
        yield return WaitUntil(() => healGate != null && healGate.IsOpen, 4f);
        Check(healGate != null && healGate.IsOpen, "brama 'heal' otwarta");
    }

    IEnumerator StageE()
    {
        yield return MoveTo(82.5f, 8f);
        yield return PogoAcross(98.6f, 40f);
        Check(seen.Contains("Pogoed"), "zdarzenie Pogoed");
        Check(t.position.x > 97.5f, Pos("przeszedł kolce"));
    }

    IEnumerator StageF()
    {
        yield return MoveTo(102.3f, 10f);
        yield return Wait(0.4f);
        Check(abilities.pistol, "pistolet odblokowany");
        yield return FaceRight();
        input.fire = true;
        yield return Wait(0.3f);
        yield return ShootUntil(() => Nearest<EnemyWalker>(107.5f, 6f) == null, 15f, 1, false, 30);
        Check(seen.Contains("Fired"), "zdarzenie Fired");
        yield return MoveTo(112.5f, 8f);
        yield return ShootUntil(() => Nearest<EnemyHeavy>(123f, 8f) == null, 60f, 1, true, 60);
        Check(Nearest<EnemyHeavy>(123f, 8f) == null, "ciężki mutant pokonany");

        var pistol = holder.Active as Pistol;
        if (pistol != null && pistol.InMagazine < pistol.magazineSize && !seen.Contains("Reloaded"))
        {
            yield return WaitUntil(() => move.IsGrounded, 2f);
            input.reload = true; yield return Wait(0.25f);
            yield return Arrows(0.2f, SeqInput.Down, SeqInput.Up);
            yield return Wait(0.3f);
        }
        Check(seen.Contains("Reloaded"), "zdarzenie Reloaded");
    }

    IEnumerator StageG()
    {
        yield return MoveTo(127.2f, 15f);
        for (int attempt = 0; attempt < 4 && t.position.x < 138.2f; attempt++)
        {
            yield return HopAcross(139f, 12f, 127.6f);
            if (t.position.x < 138.2f) { Say(Pos("retry kruszących platform")); yield return MoveTo(127.2f, 10f); }
        }
        Check(t.position.x > 138.2f, Pos("przeszedł kruszące platformy"));
        yield return MoveTo(140.5f, 8f);
        yield return WaitUntil(() => move.IsGrounded, 2f);
        Check(t.position.y > 1.5f, Pos("na piedestale próbki"));
        yield return FaceRight();
        yield return ProbeSpam(1.5f);
        Check(seen.Contains("SampleCollected"), "próbka zebrana");
        yield return MoveTo(146f, 8f);
        yield return Wait(0.3f);
        input.interact = true;
        yield return Wait(0.4f);
        Check(Mathf.Abs(respawn.Checkpoint.x - 146f) < 1f, $"checkpoint na drugiej ławce (x={respawn.Checkpoint.x:0.0})");
    }

    IEnumerator StageH()
    {
        yield return MoveTo(150.2f, 6f);
        yield return Wait(0.4f);
        Check(abilities.revolver, "rewolwer odblokowany");
        if (!(holder.Active is Revolver)) { input.switchWeapon = true; yield return Wait(0.3f); }
        Check(seen.Contains("WeaponSwitched") || holder.Active is Revolver, "zmiana broni");
        Check(holder.Active is Revolver, $"aktywna broń: {(holder.Active != null ? holder.Active.displayName : "brak")}");
        yield return FaceRight();
        input.reload = true;
        yield return Wait(0.25f);
        yield return Arrows(0.2f, SeqInput.Down, SeqInput.Left, SeqInput.Up);
        yield return Wait(0.3f);
        var rev = holder.Active as Revolver;
        Check(seen.Contains("RevolverLoaded"), "zdarzenie RevolverLoaded");
        Check(rev != null && rev.LoadedCount >= 1, $"komory załadowane: {(rev != null ? rev.LoadedCount : -1)}");
        yield return ShootUntil(() => Nearest<EnemyWalker>(156.5f, 6f) == null, 12f, 1, false, 8);
    }

    IEnumerator StageI()
    {
        yield return MoveTo(161.6f, 10f);
        yield return Wait(0.6f);
        var arena = FindFirstObjectByType<BossArena>();
        Check(arena != null && arena.Started, "arena bossa rozpoczęta");
        yield return BossFight(180f);
        var boss = FindFirstObjectByType<Boss>();
        Check(boss == null || boss.Health.IsDead, "boss pokonany");
        var exitGate = Gate.Find("exit");
        yield return WaitUntil(() => exitGate != null && exitGate.IsOpen, 5f);
        Check(exitGate != null && exitGate.IsOpen, "brama wyjściowa otwarta");
    }

    IEnumerator StageJ()
    {
        var bossLeft = FindFirstObjectByType<Boss>();
        if (bossLeft != null && !bossLeft.Health.IsDead) { Fail("boss żyje — pomijam wyjście"); yield break; }
        yield return MoveTo(192f, 20f);
        yield return Wait(3f);
        Check(HUD.I != null && HUD.I.EndShown, "ekran końcowy pokazany");
    }

    // ------------------------------------------------------------ kroki

    IEnumerator Wait(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end) yield return null;
    }

    IEnumerator WaitUntil(Func<bool> cond, float timeout)
    {
        float end = Time.time + timeout;
        while (!cond() && Time.time < end) yield return null;
    }

    IEnumerator FaceRight()
    {
        input.move = 1f; yield return null; yield return null;
        input.move = 0f; yield return null;
    }

    IEnumerator MoveTo(float x, float timeout)
    {
        float end = Time.time + timeout;
        float lastX = t.position.x, lastProgress = Time.time;
        while (Mathf.Abs(t.position.x - x) > 0.25f && Time.time < end)
        {
            input.move = Mathf.Sign(x - t.position.x);
            if (Mathf.Abs(t.position.x - lastX) > 0.2f) { lastX = t.position.x; lastProgress = Time.time; }
            else if (Time.time - lastProgress > 0.7f && move.IsGrounded)
            {
                // Zablokowany o coś — skok, a jeśli nadal blokada, podwójny.
                input.jump = true; input.jumpHeld = true;
                yield return Wait(0.3f);
                input.jumpHeld = false;
                yield return Wait(0.08f);
                if (!move.IsGrounded && abilities.doubleJump)
                {
                    input.jump = true; input.jumpHeld = true;
                    yield return Wait(0.3f);
                    input.jumpHeld = false;
                }
                lastProgress = Time.time;
            }
            yield return null;
        }
        input.move = 0f;
        yield return null;
    }

    /// Nawigacja po całym poziomie (po śmierci): zna miejsca wymagające pogo.
    IEnumerator NavigateTo(float x, float timeout)
    {
        if (x > 98f && t.position.x < 84f)
        {
            yield return MoveTo(82.5f, timeout);
            yield return PogoAcross(98.6f, 40f);
        }
        if (x > 138f && t.position.x < 128f)
        {
            yield return MoveTo(127.2f, timeout);
            for (int a = 0; a < 4 && t.position.x < 138.2f; a++)
            {
                yield return HopAcross(139f, 12f, 127.6f);
                if (t.position.x < 138.2f) yield return MoveTo(127.2f, 10f);
            }
        }
        yield return MoveTo(x, timeout);
    }

    /// Uruchamia etap; po śmierci gracza wraca na start etapu i powtarza (max 2 razy).
    IEnumerator StageWithRetry(string name, float startX, Func<IEnumerator> body, int attempts = 3)
    {
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Stage = name;
            int deathsBefore = respawn.Deaths;
            bool done = false;
            var co = StartCoroutine(RunBody(body, () => done = true));
            while (!done && respawn.Deaths == deathsBefore) yield return null;
            if (done || (HUD.I != null && HUD.I.EndShown)) yield break;

            // Śmierć w trakcie etapu: przerwij natychmiast, wróć i powtórz.
            StopCoroutine(co);
            ClearInput();
            Say(Pos($"śmierć w etapie '{name}' (#{respawn.Deaths}) — wracam na x={startX}"));
            yield return WaitUntil(() => !hp.IsDead && move.IsGrounded, 6f);
            yield return Wait(0.6f);
            yield return NavigateTo(startX, 60f);
        }
    }

    IEnumerator RunBody(Func<IEnumerator> body, Action onDone)
    {
        yield return body();
        onDone();
    }

    void ClearInput()
    {
        input.move = 0f;
        input.holdUp = input.holdDown = input.jumpHeld = false;
        input.jump = input.probe = input.fire = input.switchWeapon = input.reload = input.heal = input.interact = false;
        input.arrow = null;
    }

    /// Biegnie w kierunku dir; skacze, gdy stoi na ziemi i minął kolejny próg x.
    IEnumerator RunAndJump(int dir, float toX, float timeout, params float[] jumpAt)
    {
        float end = Time.time + timeout;
        int next = 0;
        while (Time.time < end && (dir > 0 ? t.position.x < toX : t.position.x > toX))
        {
            input.move = dir;
            bool passed = next < jumpAt.Length && (dir > 0 ? t.position.x >= jumpAt[next] : t.position.x <= jumpAt[next]);
            if (passed && move.IsGrounded)
            {
                next++;
                input.jump = true; input.jumpHeld = true;
                yield return Wait(0.28f);
                input.jumpHeld = false;
            }
            yield return null;
        }
        input.move = 0f;
        input.jumpHeld = false;
    }

    IEnumerator DoubleJump(int dir, float toX, float timeout)
    {
        float end = Time.time + timeout;
        input.move = dir;
        input.jump = true; input.jumpHeld = true;
        yield return Wait(0.3f);
        input.jumpHeld = false;
        yield return Wait(0.1f);
        input.jump = true; input.jumpHeld = true;
        yield return Wait(0.3f);
        input.jumpHeld = false;
        while (Time.time < end && !(move.IsGrounded && (dir > 0 ? t.position.x >= toX - 1f : t.position.x <= toX + 1f)))
        {
            if (move.IsGrounded) break;
            yield return null;
        }
        yield return Wait(0.1f);
        input.move = 0f;
    }

    IEnumerator ProbeSpam(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end)
        {
            if (Time.time >= nextProbe) { input.probe = true; nextProbe = Time.time + 0.3f; }
            yield return null;
        }
    }

    IEnumerator Arrows(float gap, params SeqInput[] seq)
    {
        foreach (var s in seq)
        {
            input.arrow = s;
            yield return Wait(gap);
        }
    }

    /// Idzie w prawo do untilX, po drodze dźga każdego zombie w zasięgu.
    IEnumerator FightWalkers(float untilX, float timeout)
    {
        float end = Time.time + timeout;
        float lastX = t.position.x, lastProgress = Time.time;
        while (Time.time < end && t.position.x < untilX - 0.3f)
        {
            var enemy = Nearest<EnemyWalker>(t.position.x, 9f);
            bool fighting = enemy != null && Mathf.Abs(enemy.transform.position.y - t.position.y) < 2f;

            // Brak postępu bez walki = przeszkoda terenowa. Przeskocz.
            if (!fighting)
            {
                if (Mathf.Abs(t.position.x - lastX) > 0.2f) { lastX = t.position.x; lastProgress = Time.time; }
                else if (Time.time - lastProgress > 0.6f && move.IsGrounded)
                {
                    input.move = 1f;
                    input.jump = true; input.jumpHeld = true;
                    yield return Wait(0.25f);
                    input.jumpHeld = false;
                    lastProgress = Time.time;
                    continue;
                }
            }
            else { lastX = t.position.x; lastProgress = Time.time; }

            if (fighting)
            {
                float dx = enemy.transform.position.x - t.position.x;
                if (Mathf.Abs(dx) > 1.3f) input.move = Mathf.Sign(dx);
                else if (Mathf.Abs(dx) < 0.95f) input.move = -Mathf.Sign(dx);   // za blisko — odskocz
                else input.move = 0f;
                if (Mathf.Abs(dx) < 1.5f && Time.time >= nextProbe)
                {
                    // Odwróć się w stronę wroga, potem dźgnij.
                    input.move = Mathf.Sign(dx) * 0.05f;
                    yield return null;
                    input.move = 0f;
                    input.probe = true;
                    nextProbe = Time.time + 0.3f;
                }
            }
            else input.move = 1f;
            yield return null;
        }
        input.move = 0f;
    }

    /// Strzela w kierunku dir aż done(); przeładowuje sekwencją; opcjonalnie skacze nad falami.
    IEnumerator ShootUntil(Func<bool> done, float timeout, int dir, bool dodge, int maxShots)
    {
        float end = Time.time + timeout;
        int shots = 0;
        input.move = dir * 0.05f; yield return null; input.move = 0f;
        while (Time.time < end && !done() && shots < maxShots)
        {
            if (dodge && ShockwaveNear(2.8f) && move.IsGrounded)
            {
                input.jump = true; input.jumpHeld = true;
                yield return Wait(0.3f);
                input.jumpHeld = false;
                continue;
            }

            var pistol = holder.Active as Pistol;
            if (pistol != null && pistol.IsReloading)
            {
                yield return Arrows(0.2f, SeqInput.Down, SeqInput.Up);
                continue;
            }

            if (hp.Current <= 2 && charge.CanAffordHeal && move.IsGrounded && !ShockwaveNear(5f))
            {
                input.heal = true; yield return Wait(0.2f);
                yield return Arrows(0.2f, SeqInput.Down, SeqInput.Down, SeqInput.Up);
                yield return Wait(0.2f);
                continue;
            }

            if (Time.time >= nextFire)
            {
                input.fire = true;
                nextFire = Time.time + 0.25f;
                shots++;
            }
            yield return null;
        }
        input.move = 0f;
    }

    /// Przelot nad kolcami: skok z krawędzi, potem pogo od wszystkiego, co jest pod nogami.
    IEnumerator PogoAcross(float toX, float timeout)
    {
        float end = Time.time + timeout;
        float startX = t.position.x;
        int retries = 0;
        while (Time.time < end && !(move.IsGrounded && t.position.x >= toX - 0.6f))
        {
            input.move = 1f;

            if (move.IsGrounded && t.position.x < 84.4f)
            {
                // Odbieg z krawędzi.
                if (t.position.x > 83.4f)
                {
                    input.jump = true; input.jumpHeld = true;
                    yield return Wait(0.3f);
                    input.jumpHeld = false;
                }
                yield return null;
                continue;
            }

            if (!move.IsGrounded && rb.linearVelocity.y < 4f && Time.time >= nextProbe && PogoableBelow())
            {
                input.holdDown = true;
                input.probe = true;
                nextProbe = Time.time + 0.3f;
                yield return null;
                input.holdDown = false;
                continue;
            }

            // Wrócił na bezpieczne miejsce po kolcach.
            if (move.IsGrounded && t.position.x < startX + 1f && retries < 6 && Time.time - respawnSeenAt > 0.5f)
            {
                retries++;
                respawnSeenAt = Time.time;
                Say(Pos($"pogo retry {retries}"));
            }
            yield return null;
        }
        input.holdDown = false;
        input.move = 0f;
    }
    float respawnSeenAt = -99f;

    bool PogoableBelow()
    {
        Vector2 center = (Vector2)t.position + Vector2.down * 0.9f;
        foreach (var c in Physics2D.OverlapBoxAll(center, new Vector2(1.0f, 1.5f), 0f))
        {
            if (c.transform.IsChildOf(t)) continue;
            if (c.GetComponentInParent<PogoSurface>() != null) return true;
            if (c.GetComponentInParent<ChargeCrystal>() != null) return true;
            var h = c.GetComponentInParent<Health>();
            if (h != null && !h.IsDead) return true;
        }
        return false;
    }

    /// Kruszące platformy: biegnij i skacz natychmiast po każdym lądowaniu.
    IEnumerator HopAcross(float toX, float timeout, float startJumpX)
    {
        float end = Time.time + timeout;
        float lastJump = -99f;
        while (Time.time < end && t.position.x < toX)
        {
            input.move = 1f;
            if (move.IsGrounded && t.position.x >= startJumpX && Time.time - lastJump > 0.15f)
            {
                lastJump = Time.time;
                input.jump = true; input.jumpHeld = true;
                yield return Wait(0.16f);
                input.jumpHeld = false;
                continue;
            }
            // Zbyt nisko = wpadł, Hazard cofnie; przerwij próbę.
            if (t.position.y < -1.5f) break;
            yield return null;
        }
        input.move = 0f;
        yield return Wait(0.3f);
    }

    IEnumerator Face(float dx)
    {
        input.move = Mathf.Sign(dx) * 0.05f;
        yield return null;
        input.move = 0f;
    }

    /// Wykonuje sekwencję widoczną na HUD, cokolwiek to jest.
    IEnumerator FollowSequence()
    {
        float end = Time.time + 4f;
        while (sequences.IsRunning && Time.time < end)
        {
            var pattern = sequences.Pattern;
            if (pattern == null || sequences.Progress >= pattern.Length) break;
            input.arrow = pattern[sequences.Progress];
            yield return Wait(0.2f);
        }
    }

    IEnumerator Hop(bool doubleJump)
    {
        input.jump = true; input.jumpHeld = true;
        yield return Wait(0.3f);
        input.jumpHeld = false;
        if (!doubleJump) yield break;
        yield return Wait(0.1f);
        input.jump = true; input.jumpHeld = true;
        yield return Wait(0.3f);
        input.jumpHeld = false;
    }

    IEnumerator BossFight(float timeout)
    {
        float end = Time.time + timeout;
        int deathsAtStart = respawn.Deaths;
        var pistol = t.GetComponentInChildren<Pistol>();
        var revolver = t.GetComponentInChildren<Revolver>();
        var pouch = t.GetComponent<AmmoPouch>();

        while (Time.time < end)
        {
            var boss = FindFirstObjectByType<Boss>();
            if (boss == null || boss.Health.IsDead) break;
            if (hp.IsDead) yield break;   // StageWithRetry przejmie

            float dx = boss.transform.position.x - t.position.x;
            float dist = Mathf.Abs(dx);
            int away = dx > 0 ? -1 : 1;

            // 1. Fala uderzeniowa — skok.
            if (ShockwaveNear(3f) && move.IsGrounded)
            {
                yield return Hop(false);
                continue;
            }

            // 2. Szarża/skok bossa w naszą stronę — podwójny skok nad nim.
            if (boss.IsCharging && dist < 7.5f && move.IsGrounded)
            {
                input.move = 0f;
                yield return Hop(true);
                yield return WaitUntil(() => move.IsGrounded, 1.5f);
                continue;
            }

            bool safeWindow = move.IsGrounded && !boss.IsCharging && !ShockwaveNear(5f) && dist > 6f;

            // 3. Leczenie w bezpiecznym oknie.
            if (hp.Current <= 2 && charge.CanAffordHeal && safeWindow)
            {
                input.move = 0f; yield return null;
                input.heal = true; yield return Wait(0.2f);
                yield return FollowSequence();
                yield return Wait(0.2f);
                continue;
            }

            // 4. Ogłuszony: podejdź i dźgaj; jeśli rewolwer załadowany — strzel z bliska.
            if (boss.IsStunned)
            {
                if (revolver.IsOwned && revolver.LoadedCount > 0 && dist < 10f)
                {
                    if (!(holder.Active is Revolver)) { input.switchWeapon = true; yield return Wait(0.1f); }
                    yield return Face(dx);
                    if (Time.time >= nextFire) { input.fire = true; nextFire = Time.time + 0.2f; }
                    yield return null;
                    continue;
                }
                if (dist > 1.6f) input.move = Mathf.Sign(dx);
                else
                {
                    yield return Face(dx);
                    if (Time.time >= nextProbe) { input.probe = true; nextProbe = Time.time + 0.3f; }
                }
                yield return null;
                continue;
            }

            bool pistolUsable = pistol.IsOwned && (pistol.InMagazine > 0 || pouch.pistolRounds > 0);
            bool revolverLoaded = revolver.IsOwned && revolver.LoadedCount > 0;
            bool revolverLoadable = revolver.IsOwned && pouch.revolverRounds > 0 && revolver.LoadedCount < revolver.chambers;

            // 5. Bez amunicji — po pickup na arenie.
            if (!pistolUsable && !revolverLoaded && !revolverLoadable)
            {
                var ammo = NearestPickup();
                if (ammo != null)
                {
                    input.move = Mathf.Sign(ammo.transform.position.x - t.position.x);
                    yield return null;
                    continue;
                }
                // Tylko próbnik: trzymaj dystans i czekaj na ogłuszenie.
                input.move = dist < 5f ? away : 0f;
                yield return null;
                continue;
            }

            // 6. Przeładowania w bezpiecznym oknie.
            if (safeWindow && revolverLoadable && !revolverLoaded && pouch.revolverRounds >= 2)
            {
                if (!(holder.Active is Revolver)) { input.switchWeapon = true; yield return Wait(0.1f); }
                input.move = 0f; yield return null;
                input.reload = true; yield return Wait(0.2f);
                yield return FollowSequence();
                continue;
            }
            if (safeWindow && pistol.IsOwned && pistol.InMagazine == 0 && pouch.pistolRounds > 0)
            {
                if (!(holder.Active is Pistol)) { input.switchWeapon = true; yield return Wait(0.1f); }
                input.move = 0f; yield return null;
                input.reload = true; yield return Wait(0.2f);
                yield return FollowSequence();
                continue;
            }

            // 7. Wybór broni do ostrzału z dystansu: pistolet (rewolwer zostawiamy na ogłuszenie).
            if (pistolUsable && pistol.InMagazine > 0 && !(holder.Active is Pistol))
            {
                input.switchWeapon = true; yield return Wait(0.1f); continue;
            }

            // 8. Dystans 5–8 w granicach areny.
            float targetX = t.position.x;
            if (dist < 4.5f) targetX = t.position.x + away * 2.5f;
            else if (dist > 8.5f) targetX = t.position.x - away * 2f;
            targetX = Mathf.Clamp(targetX, 160.5f, 181.5f);
            float dxTarget = targetX - t.position.x;
            if (Mathf.Abs(dxTarget) > 0.3f)
            {
                input.move = Mathf.Sign(dxTarget);
                yield return null;
                continue;
            }
            input.move = 0f;

            // 9. Strzał (tylko gdy jesteśmy zwróceni do bossa i boss nie jest w powietrzu).
            if (Time.time >= nextFire && dist < 13f && holder.Active is Pistol && pistol.InMagazine > 0)
            {
                yield return Face(dx);
                if (Mathf.Abs(boss.transform.position.y - t.position.y) < 1.8f)
                {
                    input.fire = true;
                    nextFire = Time.time + 0.25f;
                }
            }
            yield return null;
        }
        input.move = 0f;
    }

    AmmoPickup NearestPickup()
    {
        AmmoPickup best = null;
        float bestD = 30f;
        foreach (var p in FindObjectsByType<AmmoPickup>(FindObjectsSortMode.None))
        {
            float d = Mathf.Abs(p.transform.position.x - t.position.x);
            if (p.transform.position.x < 159f || p.transform.position.x > 183f) continue;
            if (d < bestD) { bestD = d; best = p; }
        }
        return best;
    }

    // ------------------------------------------------------------ narzędzia

    bool ShockwaveNear(float range)
    {
        foreach (var w in FindObjectsByType<Shockwave>(FindObjectsSortMode.None))
        {
            float dx = w.transform.position.x - t.position.x;
            bool approaching = Mathf.Sign(dx) == -Mathf.Sign(w.direction) || Mathf.Abs(dx) < 0.8f;
            if (Mathf.Abs(dx) < range && approaching && Mathf.Abs(w.transform.position.y - t.position.y) < 2f) return true;
        }
        return false;
    }

    T Nearest<T>(float x, float range) where T : Component
    {
        T best = null;
        float bestD = range;
        foreach (var e in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            var h = e.GetComponent<Health>();
            if (h != null && h.IsDead) continue;
            float d = Mathf.Abs(e.transform.position.x - x);
            if (d < bestD) { bestD = d; best = e; }
        }
        return best;
    }

    string Pos(string what) => $"{what} (x={t.position.x:0.0}, y={t.position.y:0.0})";

    void Say(string s)
    {
        Log.AppendLine($"[{Time.time,6:0.0}] {s}");
        Debug.Log("[Autopilot] " + s);
    }

    void Check(bool ok, string what)
    {
        if (ok) { Passes++; Say("PASS " + what); }
        else Fail(what);
    }

    void Fail(string what)
    {
        Failures++;
        Say("FAIL " + what);
    }
}
