using UnityEngine;

/// Pistolet — narzędzie do konkretnych problemów, nie podstawowy atak.
/// Amunicja znajdowana w świecie, przeładowanie krótkie (zasada 3: moc = ceremonia,
/// a pistolet nie jest szczególnie mocny).
public class Pistol : Weapon
{
    [Header("Pistolet")]
    public int magazineSize = 12;
    public int damage = 3;
    public float range = 14f;
    public float fireCooldown = 0.18f;

    [Header("Przeładowanie")]
    public SeqInput[] reloadPattern = { SeqInput.Down, SeqInput.Up };

    public int InMagazine { get; private set; }
    public bool IsReloading { get; private set; }

    float nextFireTime;

    protected override void Awake()
    {
        base.Awake();
        displayName = "Pistolet";
        requiredAbility = Ability.Pistol;
        InMagazine = magazineSize;
    }

    public override string StatusLine => Pouch == null
        ? $"{InMagazine}/{magazineSize}"
        : $"{InMagazine}/{magazineSize}  zapas {Pouch.pistolRounds}";

    public override void PullTrigger()
    {
        if (!IsOwned || IsReloading || Time.time < nextFireTime) return;

        if (InMagazine <= 0)
        {
            Sfx.Play("click", 0.7f);
            BeginReload();
            return;
        }

        InMagazine--;
        nextFireTime = Time.time + fireCooldown;

        Sfx.Play("shoot");
        CameraShake.Shake(0.07f, 0.1f);

        var target = FireHitscan(range, new Color(1f, 0.86f, 0.42f), out _);
        target?.TakeDamage(damage, transform.position);

        PlayerEvents.RaiseFired();
    }

    public override void BeginReload()
    {
        if (!IsOwned || IsReloading || InMagazine >= magazineSize) return;
        if (Pouch.pistolRounds <= 0)
        {
            HUD.Message("Brak zapasu amunicji", 1.2f);
            return;
        }
        if (!Sequences.CanBegin()) return;

        IsReloading = true;
        Sequences.Begin(new SequenceRequest
        {
            label = "PRZEŁADOWANIE",
            pattern = reloadPattern,
            resetOnMistake = true,
            onComplete = () =>
            {
                IsReloading = false;
                InMagazine += Pouch.TakePistol(magazineSize - InMagazine);
                Sfx.Play("reload");
                PlayerEvents.RaiseReloaded();
            },
            onCancel = () => IsReloading = false,
        });
    }
}
