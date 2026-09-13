using UnityEngine;

/// Deweloperski HUD na OnGUI — świadomie brzydki i bez Canvasu.
/// Ma pokazać, czy sekwencje da się czytać w trakcie gry; docelowe UI to osobny temat.
public class DebugHUD : MonoBehaviour
{
    Health health;
    ChargeMeter charge;
    AmmoPouch pouch;
    WeaponHolder weapons;
    SequenceRunner sequences;

    GUIStyle body, big, dim;

    void Awake()
    {
        health = GetComponent<Health>();
        charge = GetComponent<ChargeMeter>();
        pouch = GetComponent<AmmoPouch>();
        weapons = GetComponent<WeaponHolder>();
        sequences = GetComponent<SequenceRunner>();
    }

    void BuildStyles()
    {
        if (body != null) return;

        body = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
        big = new GUIStyle(GUI.skin.label) { fontSize = 30, richText = true, alignment = TextAnchor.MiddleCenter };
        dim = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
        dim.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
    }

    void OnGUI()
    {
        BuildStyles();

        GUI.Box(new Rect(10, 10, 430, 132), GUIContent.none);
        GUILayout.BeginArea(new Rect(22, 20, 410, 120));

        GUILayout.Label($"<b>HP</b>        {Bar(health.Current, health.maxHealth, '#', '-')}  " +
                        $"{health.Current}/{health.maxHealth}", body);

        GUILayout.Label($"<b>ŁADUNEK</b>   {Bar(charge.Current, charge.maxCharge, '|', '.')}  " +
                        $"{charge.Current}/{charge.maxCharge}   (zabieg: {charge.healCost})", body);

        var active = weapons.Active;
        GUILayout.Label(active == null
            ? "<b>BROŃ</b>      (brak)"
            : $"<b>BROŃ</b>      {active.displayName} — {active.StatusLine}", body);

        GUILayout.Label($"<b>PRÓBKI</b>    {SampleNode.CollectedCount}/{SampleNode.TotalCount}", body);

        GUILayout.EndArea();

        DrawSequence();
        DrawControls();
    }

    void DrawSequence()
    {
        if (!sequences.IsRunning) return;

        var pattern = sequences.Pattern;
        var line = "";
        for (int i = 0; i < pattern.Length; i++)
        {
            bool done = i < sequences.Progress;
            string glyph = Glyph(pattern[i]);
            line += done ? $"<color=#7CE07C>{glyph}</color>  "
                         : i == sequences.Progress ? $"<color=#FFD24A>{glyph}</color>  "
                         : $"<color=#606060>{glyph}</color>  ";
        }

        float w = 560f, h = 84f;
        var rect = new Rect((Screen.width - w) * 0.5f, Screen.height - 190f, w, h);
        GUI.Box(rect, GUIContent.none);

        var labelColor = sequences.MistakeThisFrame ? "#FF6B6B" : "#DDDDDD";
        GUI.Label(new Rect(rect.x, rect.y + 6, w, 24),
                  $"<color={labelColor}><b>{sequences.Label}</b></color>",
                  new GUIStyle(big) { fontSize = 16 });
        GUI.Label(new Rect(rect.x, rect.y + 32, w, 44), line, big);
    }

    void DrawControls()
    {
        var rect = new Rect(10, Screen.height - 78, 720, 70);
        GUILayout.BeginArea(rect);
        GUILayout.Label("A/D ruch    SPACJA skok (podwójny)    J próbnik    S+J w dół = pogo    W+J w górę", dim);
        GUILayout.Label("K strzał    TAB zmiana broni    R przeładowanie    H zabieg    E ławka", dim);
        GUILayout.Label("STRZAŁKI — sekwencje (ruch i skok przerywają)", dim);
        GUILayout.EndArea();
    }

    static string Bar(int value, int max, char full, char empty)
    {
        var s = "";
        for (int i = 0; i < max; i++) s += i < value ? full : empty;
        return s;
    }

    static string Glyph(SeqInput input) => input switch
    {
        SeqInput.Up => "▲",
        SeqInput.Down => "▼",
        SeqInput.Left => "◀",
        _ => "▶",
    };
}
