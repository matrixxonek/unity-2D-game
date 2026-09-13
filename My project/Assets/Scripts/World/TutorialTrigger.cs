using System;
using UnityEngine;

public enum TutorialGoal
{
    None, Move, Jump, DoubleJump, ProbeHit, Heal, Pogo, Fire, Reload, Sample, Rest, SwitchWeapon, LoadRevolver
}

public enum TutorialPrereq { None, HpBelowMax, PistolMagazineLow }

/// Strefa samouczka: po wejściu pokazuje podpowiedź, znika po wykonaniu celu.
/// Może otworzyć bramę po ukończeniu.
[RequireComponent(typeof(Collider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [TextArea] public string prompt;
    public TutorialGoal goal = TutorialGoal.None;
    public TutorialPrereq prereq = TutorialPrereq.None;

    [Tooltip("Gdy warunek wstępny nie jest spełniony (np. pełne HP), uznaj cel za wykonany.")]
    public bool completeIfPrereqUnmet;

    [Tooltip("0 = pokazuj do wykonania celu / wyjścia ze strefy.")]
    public float autoHideAfter;
    public bool hideOnExit = true;
    public string gateToOpen;

    bool done, showing, inside;
    float hideAt;
    Action handler;

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        inside = true;
        Evaluate(other.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (done || showing) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        Evaluate(other.gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        inside = false;
        if (showing && hideOnExit) Hide();
    }

    void Evaluate(GameObject playerObject)
    {
        if (done) return;

        var player = playerObject.GetComponentInParent<PlayerMovement>().gameObject;
        if (!PrereqMet(player))
        {
            if (completeIfPrereqUnmet) Complete();
            return;
        }

        if (!showing) Show();
    }

    bool PrereqMet(GameObject player)
    {
        switch (prereq)
        {
            case TutorialPrereq.HpBelowMax:
            {
                var h = player.GetComponent<Health>();
                return h != null && h.Current < h.maxHealth;
            }
            case TutorialPrereq.PistolMagazineLow:
            {
                var pistol = player.GetComponentInChildren<Pistol>();
                return pistol != null && pistol.IsOwned && pistol.InMagazine <= pistol.magazineSize - 5;
            }
            default: return true;
        }
    }

    void Show()
    {
        showing = true;
        HUD.Prompt(prompt);
        if (autoHideAfter > 0f) hideAt = Time.time + autoHideAfter;
        Subscribe();
    }

    void Hide()
    {
        showing = false;
        HUD.HidePrompt();
    }

    void Update()
    {
        if (!showing || autoHideAfter <= 0f || Time.time < hideAt) return;
        Hide();
        if (goal == TutorialGoal.None) done = true;
    }

    void Subscribe()
    {
        Unsubscribe();
        if (goal == TutorialGoal.None) return;

        handler = Complete;
        switch (goal)
        {
            case TutorialGoal.Move: PlayerEvents.Moved += handler; break;
            case TutorialGoal.Jump: PlayerEvents.Jumped += handler; break;
            case TutorialGoal.DoubleJump: PlayerEvents.DoubleJumped += handler; break;
            case TutorialGoal.ProbeHit: PlayerEvents.ProbeHit += handler; break;
            case TutorialGoal.Heal: PlayerEvents.Healed += handler; break;
            case TutorialGoal.Pogo: PlayerEvents.Pogoed += handler; break;
            case TutorialGoal.Fire: PlayerEvents.Fired += handler; break;
            case TutorialGoal.Reload: PlayerEvents.Reloaded += handler; break;
            case TutorialGoal.Sample: PlayerEvents.SampleCollected += handler; break;
            case TutorialGoal.Rest: PlayerEvents.Rested += handler; break;
            case TutorialGoal.SwitchWeapon: PlayerEvents.WeaponSwitched += handler; break;
            case TutorialGoal.LoadRevolver: PlayerEvents.RevolverLoaded += handler; break;
        }
    }

    void Unsubscribe()
    {
        if (handler == null) return;
        PlayerEvents.Moved -= handler; PlayerEvents.Jumped -= handler; PlayerEvents.DoubleJumped -= handler;
        PlayerEvents.ProbeHit -= handler; PlayerEvents.Healed -= handler; PlayerEvents.Pogoed -= handler;
        PlayerEvents.Fired -= handler; PlayerEvents.Reloaded -= handler; PlayerEvents.SampleCollected -= handler;
        PlayerEvents.Rested -= handler; PlayerEvents.WeaponSwitched -= handler; PlayerEvents.RevolverLoaded -= handler;
        handler = null;
    }

    void Complete()
    {
        if (done) return;
        done = true;
        Unsubscribe();
        if (showing) Hide();

        if (goal != TutorialGoal.None) Sfx.Play("seq_done", 0.5f, 1.2f);

        var gate = Gate.Find(gateToOpen);
        if (gate != null) gate.Open();
    }

    void OnDestroy() => Unsubscribe();
}
