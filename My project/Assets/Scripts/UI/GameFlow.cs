using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Przepływ gry: intro, pauza, śmierć/odrodzenie, zwycięstwo, restart.
public class GameFlow : MonoBehaviour
{
    public static GameFlow I { get; private set; }
    public static bool IsPaused { get; private set; }

    [Header("Intro")]
    public string title = "THE DOOMSDAY RECIPE";
    public string subtitle = "prototyp — samouczek";

    public float ElapsedTime => Time.time - startTime;
    public int Deaths => respawn != null ? respawn.Deaths : 0;

    PlayerInputReader input;
    PlayerRespawn respawn;
    PlayerMovement movement;
    CameraFollow cameraFollow;
    float startTime;
    bool ended, dying;

    void Awake()
    {
        I = this;
        IsPaused = false;
        Time.timeScale = 1f;

        // Edytor bez fokusu też ma tykać — ułatwia testy.
        Application.runInBackground = true;

        Physics2D.IgnoreLayerCollision(Layers.Enemy, Layers.Enemy, true);
        Physics2D.IgnoreLayerCollision(Layers.Enemy, Layers.Hazard, true);
    }

    void OnDestroy()
    {
        if (I == this) I = null;
        Time.timeScale = 1f;
        IsPaused = false;
    }

    void Start()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            input = player.GetComponent<PlayerInputReader>();
            respawn = player.GetComponent<PlayerRespawn>();
            movement = player;
        }
        cameraFollow = FindFirstObjectByType<CameraFollow>();
        startTime = Time.time;

        cameraFollow?.SnapToTarget();
        StartCoroutine(Intro());
    }

    IEnumerator Intro()
    {
        if (HUD.I == null) yield break;
        HUD.I.SetFade(1f);
        StartCoroutine(HUD.I.ShowTitle(title, subtitle, 2.4f));
        yield return HUD.I.Fade(0f, 1.0f);
    }

    void Update()
    {
        if (input == null) return;

        if (ended)
        {
            if (input.ConfirmPressed) Restart();
            return;
        }

        if (input.PausePressed) TogglePause();
    }

    public void OnPlayerDied(PlayerRespawn r)
    {
        if (dying || ended) return;
        StartCoroutine(DeathRoutine(r));
    }

    IEnumerator DeathRoutine(PlayerRespawn r)
    {
        dying = true;
        movement?.Knockback(Vector2.zero, 10f);
        Sfx.Play("boss_hit", 0.7f, 0.6f);
        CameraShake.Shake(0.4f, 0.4f);

        yield return new WaitForSecondsRealtime(0.35f);
        if (HUD.I != null) yield return HUD.I.Fade(1f, 0.45f);

        r.RespawnAtCheckpoint();
        cameraFollow?.SnapToTarget();
        HUD.Message("Wracasz na ławkę", 1.6f);

        yield return new WaitForSecondsRealtime(0.25f);
        if (HUD.I != null) yield return HUD.I.Fade(0f, 0.5f);
        dying = false;
    }

    public void Victory()
    {
        if (ended) return;
        ended = true;
        StartCoroutine(VictoryRoutine());
    }

    IEnumerator VictoryRoutine()
    {
        Sfx.Play("victory");
        movement?.Knockback(Vector2.zero, 100f);
        var playerHealth = movement != null ? movement.GetComponent<Health>() : null;
        if (playerHealth != null) playerHealth.Invulnerable = true;
        yield return new WaitForSecondsRealtime(0.6f);
        if (HUD.I != null) yield return HUD.I.Fade(1f, 1.2f);

        int minutes = (int)(ElapsedTime / 60f);
        int seconds = (int)(ElapsedTime % 60f);
        string body =
            $"Czas: {minutes:0}:{seconds:00}\n" +
            $"Śmierci: {Deaths}\n" +
            $"Próbki: {SampleNode.CollectedCount}/{SampleNode.TotalCount}\n\n" +
            "<color=#9a9fa8>ENTER — od nowa</color>";

        HUD.I?.ShowEndCard("PLAN BOMBY: 1/3 GOTOWY", body);
    }

    void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        HUD.I?.SetPaused(IsPaused);
        Sfx.Play("message", 0.5f, IsPaused ? 0.7f : 1.2f, 0f);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
