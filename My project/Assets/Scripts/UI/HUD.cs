using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// HUD budowany w kodzie (uGUI, legacy Text — bez importów TMP).
/// Jedna instancja na scenę; statyczne skróty HUD.Message / HUD.Prompt są bezpieczne,
/// gdy HUD-u nie ma.
public class HUD : MonoBehaviour
{
    public static HUD I { get; private set; }

    static readonly Color PipFull = new Color(0.95f, 0.32f, 0.36f);
    static readonly Color PipEmpty = new Color(0.25f, 0.26f, 0.32f);
    static readonly Color ChargeColor = new Color(0.4f, 0.9f, 1f);
    static readonly Color PanelBg = new Color(0.05f, 0.06f, 0.09f, 0.72f);
    static readonly Color TextMain = new Color(0.92f, 0.93f, 0.95f);
    static readonly Color TextDim = new Color(0.6f, 0.62f, 0.68f);

    Font font;
    Sprite dot, arrowSprite;

    Health health;
    ChargeMeter charge;
    AmmoPouch pouch;
    WeaponHolder weapons;
    SequenceRunner sequences;

    Image[] hpPips;
    Transform hpRow;
    Image chargeFill;
    Text chargeText, weaponText, samplesText;

    GameObject promptPanel;
    Text promptText;

    CanvasGroup messageGroup;
    Text messageText;
    float messageUntil, messageDuration;

    GameObject seqPanel;
    Text seqLabel;
    Image[] seqArrows;

    GameObject bossPanel;
    Text bossNameText;
    Image bossFill;

    Image fadeImage;
    GameObject endPanel, pausePanel, titlePanel;
    Text endTitle, endBody, titleText, subtitleText;
    CanvasGroup titleGroup;

    void Awake()
    {
        I = this;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dot = Visuals.Dot;
        var lib = SpriteLibrary.I;
        arrowSprite = lib != null && lib.arrow != null ? lib.arrow : dot;
        Build();
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    void Start()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player == null) return;
        health = player.GetComponent<Health>();
        charge = player.GetComponent<ChargeMeter>();
        pouch = player.GetComponent<AmmoPouch>();
        weapons = player.GetComponent<WeaponHolder>();
        sequences = player.GetComponent<SequenceRunner>();
    }

    // ------------------------------------------------------------------ API

    public static void Message(string text, float seconds) { if (I != null) I.ShowMessage(text, seconds); }
    public static void Prompt(string text) { if (I != null) I.ShowPrompt(text); }
    public static void HidePrompt() { if (I != null) I.promptPanel.SetActive(false); }

    public void ShowPrompt(string text)
    {
        promptText.text = text;
        promptPanel.SetActive(true);
        Sfx.Play("message", 0.35f, 1f, 0f);
    }

    public void ShowMessage(string text, float seconds)
    {
        messageText.text = text;
        messageDuration = Mathf.Max(0.3f, seconds);
        messageUntil = Time.unscaledTime + messageDuration;
        messageGroup.alpha = 1f;
    }

    public void ShowBoss(string name)
    {
        bossNameText.text = name;
        bossFill.fillAmount = 1f;
        bossPanel.SetActive(true);
    }

    public void SetBoss(float fraction) => bossFill.fillAmount = Mathf.Clamp01(fraction);
    public void HideBoss() => bossPanel.SetActive(false);

    public IEnumerator Fade(float target, float duration)
    {
        float start = fadeImage.color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetFade(Mathf.Lerp(start, target, t / duration));
            yield return null;
        }
        SetFade(target);
    }

    public void SetFade(float alpha)
    {
        fadeImage.color = new Color(0f, 0f, 0f, alpha);
        fadeImage.raycastTarget = false;
    }

    public IEnumerator ShowTitle(string title, string subtitle, float seconds)
    {
        titleText.text = title;
        subtitleText.text = subtitle;
        titlePanel.SetActive(true);
        titleGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(seconds);
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.unscaledDeltaTime;
            titleGroup.alpha = 1f - t / 0.6f;
            yield return null;
        }
        titlePanel.SetActive(false);
    }

    public bool EndShown { get; private set; }

    public void ShowEndCard(string title, string body)
    {
        EndShown = true;
        endTitle.text = title;
        endBody.text = body;
        endPanel.SetActive(true);
    }

    public void SetPaused(bool paused) => pausePanel.SetActive(paused);

    // --------------------------------------------------------------- Update

    void Update()
    {
        if (health != null) UpdateStats();
        UpdateMessage();
        UpdateSequence();
    }

    void UpdateStats()
    {
        if (hpPips == null || hpPips.Length != health.maxHealth) BuildPips(health.maxHealth);
        for (int i = 0; i < hpPips.Length; i++)
            hpPips[i].color = i < health.Current ? PipFull : PipEmpty;

        if (charge != null)
        {
            chargeFill.fillAmount = charge.maxCharge > 0 ? charge.Current / (float)charge.maxCharge : 0f;
            chargeFill.color = charge.CanAffordHeal ? ChargeColor : new Color(0.3f, 0.5f, 0.6f);
            chargeText.text = $"ŁADUNEK {charge.Current}/{charge.maxCharge}   zabieg = {charge.healCost}";
        }

        var active = weapons != null ? weapons.Active : null;
        weaponText.text = active == null ? "<color=#8a8f99>brak broni palnej</color>"
                                         : $"<b>{active.displayName.ToUpper()}</b>   {active.StatusLine}";

        samplesText.text = SampleNode.TotalCount > 0
            ? $"PRÓBKI {SampleNode.CollectedCount}/{SampleNode.TotalCount}"
            : "";
    }

    void UpdateMessage()
    {
        if (messageGroup.alpha <= 0f) return;
        float remaining = messageUntil - Time.unscaledTime;
        messageGroup.alpha = remaining <= 0f ? 0f : Mathf.Clamp01(remaining / 0.4f);
    }

    void UpdateSequence()
    {
        bool running = sequences != null && sequences.IsRunning;
        seqPanel.SetActive(running);
        if (!running) return;

        var pattern = sequences.Pattern;
        bool mistake = Time.unscaledTime - sequences.LastMistakeTime < 0.15f;
        seqLabel.text = sequences.Label;
        seqLabel.color = mistake ? new Color(1f, 0.4f, 0.4f) : TextMain;

        for (int i = 0; i < seqArrows.Length; i++)
        {
            bool visible = i < pattern.Length;
            seqArrows[i].gameObject.SetActive(visible);
            if (!visible) continue;

            seqArrows[i].rectTransform.localEulerAngles = new Vector3(0f, 0f, ArrowAngle(pattern[i]));
            bool done = i < sequences.Progress;
            bool current = i == sequences.Progress;
            seqArrows[i].color = done ? new Color(0.5f, 0.95f, 0.5f)
                               : current ? (mistake ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.85f, 0.3f))
                               : new Color(0.4f, 0.42f, 0.48f);
            float scale = current ? 1.15f + 0.08f * Mathf.Sin(Time.unscaledTime * 10f) : 1f;
            seqArrows[i].rectTransform.localScale = Vector3.one * scale;
        }
    }

    static float ArrowAngle(SeqInput input)
    {
        switch (input)
        {
            case SeqInput.Left: return 90f;
            case SeqInput.Down: return 180f;
            case SeqInput.Right: return -90f;
            default: return 0f;
        }
    }

    // ---------------------------------------------------------------- Build

    void Build()
    {
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel statystyk (lewy górny róg)
        var stats = Panel("Stats", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                          new Vector2(28f, -28f), new Vector2(560f, 152f), PanelBg);
        hpRow = Panel("HpRow", stats, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                      new Vector2(18f, -16f), new Vector2(520f, 34f), null);
        var chargeBg = MakeImage(stats, "ChargeBg", dot, new Color(0.15f, 0.17f, 0.22f));
        Place(chargeBg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -60f), new Vector2(420f, 16f));
        chargeFill = MakeImage(stats, "ChargeFill", dot, ChargeColor);
        Place(chargeFill.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -60f), new Vector2(420f, 16f));
        chargeFill.type = Image.Type.Filled;
        chargeFill.fillMethod = Image.FillMethod.Horizontal;
        chargeText = MakeText(stats, "ChargeText", 18, TextAnchor.MiddleLeft, TextDim);
        Place(chargeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -80f), new Vector2(520f, 22f));
        weaponText = MakeText(stats, "WeaponText", 22, TextAnchor.MiddleLeft, TextMain);
        Place(weaponText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -106f), new Vector2(530f, 26f));
        samplesText = MakeText(stats, "SamplesText", 18, TextAnchor.MiddleLeft, new Color(0.5f, 0.95f, 0.6f));
        Place(samplesText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -132f), new Vector2(520f, 20f));

        // Podpowiedź samouczka (góra, środek)
        var prompt = Panel("Prompt", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                           new Vector2(0f, -36f), new Vector2(1100f, 64f), PanelBg);
        promptPanel = prompt.gameObject;
        promptText = MakeText(prompt, "PromptText", 30, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.55f));
        Stretch(promptText.rectTransform, 16f, 4f);
        promptPanel.SetActive(false);

        // Komunikat (środek ekranu, wyżej)
        var msg = Panel("Message", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(0f, 150f), new Vector2(1400f, 70f), null);
        messageGroup = msg.gameObject.AddComponent<CanvasGroup>();
        messageGroup.alpha = 0f;
        messageText = MakeText(msg, "MessageText", 40, TextAnchor.MiddleCenter, TextMain);
        messageText.fontStyle = FontStyle.Bold;
        Stretch(messageText.rectTransform, 0f, 0f);
        AddShadow(messageText);

        // Panel sekwencji (dół, środek)
        var seq = Panel("Sequence", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                        new Vector2(0f, 120f), new Vector2(760f, 150f), PanelBg);
        seqPanel = seq.gameObject;
        seqLabel = MakeText(seq, "SeqLabel", 26, TextAnchor.MiddleCenter, TextMain);
        seqLabel.fontStyle = FontStyle.Bold;
        Place(seqLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(700f, 32f));
        var arrowRow = Panel("Arrows", seq, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                             new Vector2(0f, 14f), new Vector2(700f, 80f), null);
        var layout = arrowRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 22f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        seqArrows = new Image[8];
        for (int i = 0; i < seqArrows.Length; i++)
        {
            var a = MakeImage(arrowRow, "Arrow" + i, arrowSprite, Color.white);
            a.rectTransform.sizeDelta = new Vector2(64f, 64f);
            a.preserveAspect = true;
            seqArrows[i] = a;
            a.gameObject.SetActive(false);
        }
        seqPanel.SetActive(false);

        // Pasek bossa
        var boss = Panel("Boss", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                         new Vector2(0f, -118f), new Vector2(900f, 58f), null);
        bossPanel = boss.gameObject;
        bossNameText = MakeText(boss, "BossName", 26, TextAnchor.MiddleCenter, new Color(1f, 0.55f, 0.55f));
        bossNameText.fontStyle = FontStyle.Bold;
        Place(bossNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(900f, 30f));
        AddShadow(bossNameText);
        var bossBg = MakeImage(boss, "BossBg", dot, new Color(0.1f, 0.1f, 0.13f, 0.85f));
        Place(bossBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(880f, 20f));
        bossFill = MakeImage(boss, "BossFill", dot, new Color(0.9f, 0.25f, 0.3f));
        Place(bossFill.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(880f, 20f));
        bossFill.type = Image.Type.Filled;
        bossFill.fillMethod = Image.FillMethod.Horizontal;
        bossFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        bossPanel.SetActive(false);

        // Podpowiedź sterowania (dół, lewo)
        var hint = MakeText(transform, "Hint", 17, TextAnchor.LowerLeft, new Color(0.55f, 0.57f, 0.63f, 0.85f));
        Place(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(26f, 18f), new Vector2(900f, 70f));
        hint.text = "A/D ruch   SPACJA skok   J próbnik (S+J w dół = pogo, W+J w górę)   K strzał   TAB broń   R przeładuj   H zabieg   E ławka   ESC pauza\n" +
                    "STRZAŁKI = sekwencje   (pad: gałka / d-pad, A skok, X próbnik, B strzał, LB przeładuj, LT zabieg, Y ławka, RB broń)";

        // Ściemnienie
        fadeImage = MakeImage(transform, "Fade", dot, new Color(0f, 0f, 0f, 0f));
        Stretch(fadeImage.rectTransform, 0f, 0f);
        fadeImage.raycastTarget = false;

        // Karta tytułowa
        var title = Panel("Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                          Vector2.zero, new Vector2(1600f, 260f), null);
        titlePanel = title.gameObject;
        titleGroup = titlePanel.AddComponent<CanvasGroup>();
        titleText = MakeText(title, "TitleText", 84, TextAnchor.MiddleCenter, TextMain);
        titleText.fontStyle = FontStyle.Bold;
        Place(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1600f, 110f));
        AddShadow(titleText);
        subtitleText = MakeText(title, "Subtitle", 30, TextAnchor.MiddleCenter, TextDim);
        Place(subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(1600f, 40f));
        titlePanel.SetActive(false);

        // Pauza
        var pause = Panel("Pause", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
        pausePanel = pause.gameObject;
        var pauseText = MakeText(pause, "PauseText", 72, TextAnchor.MiddleCenter, TextMain);
        pauseText.fontStyle = FontStyle.Bold;
        pauseText.text = "PAUZA\n<size=28><color=#9a9fa8>ESC — wróć do gry</color></size>";
        Stretch(pauseText.rectTransform, 0f, 0f);
        pausePanel.SetActive(false);

        // Ekran końcowy
        var end = Panel("End", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.04f, 0.92f));
        endPanel = end.gameObject;
        endTitle = MakeText(end, "EndTitle", 78, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        endTitle.fontStyle = FontStyle.Bold;
        Place(endTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 100f));
        endBody = MakeText(end, "EndBody", 32, TextAnchor.UpperCenter, TextMain);
        Place(endBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, 50f), new Vector2(1400f, 400f));
        endPanel.SetActive(false);
    }

    void BuildPips(int count)
    {
        foreach (Transform child in hpRow) Destroy(child.gameObject);
        hpPips = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var pip = MakeImage(hpRow, "Pip" + i, dot, PipFull);
            Place(pip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                  new Vector2(i * 40f, 0f), new Vector2(32f, 30f));
            hpPips[i] = pip;
        }
    }

    RectTransform Panel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color? bg) =>
        Panel(name, transform, anchorMin, anchorMax, pivot, pos, size, bg);

    RectTransform Panel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color? bg)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        if (bg.HasValue)
        {
            var img = go.AddComponent<Image>();
            img.sprite = dot;
            img.color = bg.Value;
            img.raycastTarget = false;
        }
        return rt;
    }

    Text MakeText(Transform parent, string name, int size, TextAnchor align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Image MakeImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt, float padX, float padY)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
    }

    static void AddShadow(Text t)
    {
        var s = t.gameObject.AddComponent<Shadow>();
        s.effectColor = new Color(0f, 0f, 0f, 0.8f);
        s.effectDistance = new Vector2(2f, -2f);
    }
}
