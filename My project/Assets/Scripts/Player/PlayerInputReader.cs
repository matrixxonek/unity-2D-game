using UnityEngine;
using UnityEngine.InputSystem;

/// Jedyne miejsce, w którym czytamy klawiaturę i pada.
///
/// Bronie i akcje pytają o stan tutaj, zamiast każda nasłuchiwać samodzielnie —
/// inaczej przy dwóch broniach naraz to samo wciśnięcie obsłuży się dwa razy.
///
/// Podział rąk jest celowy: lewa steruje postacią (WASD + spacja / gałka), prawa
/// wykonuje procedury (strzałki / d-pad). Strzałki NIE chodzą — są na sekwencje.
[DefaultExecutionOrder(-100)]
public class PlayerInputReader : MonoBehaviour
{
    public float Move { get; private set; }
    public bool HoldUp { get; private set; }
    public bool HoldDown { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool ProbePressed { get; private set; }
    public bool FirePressed { get; private set; }
    public bool SwitchWeaponPressed { get; private set; }
    public bool ReloadPressed { get; private set; }
    public bool HealPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool PausePressed { get; private set; }
    public bool ConfirmPressed { get; private set; }

    /// Sterowanie skryptowe (autopilot testowy). Gdy ustawione, klawiatura i pad są ignorowane.
    public class ScriptedInput
    {
        public float move;
        public bool holdUp, holdDown, jumpHeld;
        public bool jump, probe, fire, switchWeapon, reload, heal, interact, pause, confirm;
        public SeqInput? arrow;
    }

    public static ScriptedInput Scripted;

    bool arrowAvailable;
    SeqInput arrow;

    void Update()
    {
        if (Scripted != null)
        {
            ReadScripted(Scripted);
            return;
        }

        var k = Keyboard.current;
        var g = Gamepad.current;

        float kbMove = 0f;
        if (k != null)
            kbMove = (k.dKey.isPressed ? 1f : 0f) - (k.aKey.isPressed ? 1f : 0f);

        float padMove = 0f;
        if (g != null)
        {
            float x = g.leftStick.x.ReadValue();
            padMove = Mathf.Abs(x) < 0.3f ? 0f : Mathf.Sign(x);
        }
        Move = kbMove != 0f ? kbMove : padMove;

        HoldUp = (k != null && k.wKey.isPressed) || (g != null && g.leftStick.y.ReadValue() > 0.5f);
        HoldDown = (k != null && k.sKey.isPressed) || (g != null && g.leftStick.y.ReadValue() < -0.5f);

        JumpPressed = Pressed(k?.spaceKey, g?.buttonSouth);
        JumpHeld = Held(k?.spaceKey, g?.buttonSouth);
        ProbePressed = Pressed(k?.jKey, g?.buttonWest);
        FirePressed = Pressed(k?.kKey, g?.buttonEast);
        SwitchWeaponPressed = Pressed(k?.tabKey, g?.rightShoulder);
        ReloadPressed = Pressed(k?.rKey, g?.leftShoulder);
        HealPressed = Pressed(k?.hKey, g?.leftTrigger);
        InteractPressed = Pressed(k?.eKey, g?.buttonNorth);
        PausePressed = Pressed(k?.escapeKey, g?.startButton);
        ConfirmPressed = Pressed(k?.enterKey, g?.buttonSouth);

        arrowAvailable = true;
        if (Pressed(k?.downArrowKey, g?.dpad.down)) arrow = SeqInput.Down;
        else if (Pressed(k?.leftArrowKey, g?.dpad.left)) arrow = SeqInput.Left;
        else if (Pressed(k?.upArrowKey, g?.dpad.up)) arrow = SeqInput.Up;
        else if (Pressed(k?.rightArrowKey, g?.dpad.right)) arrow = SeqInput.Right;
        else arrowAvailable = false;
    }

    void ReadScripted(ScriptedInput s)
    {
        Move = s.move;
        HoldUp = s.holdUp;
        HoldDown = s.holdDown;

        JumpPressed = s.jump; s.jump = false;
        JumpHeld = s.jumpHeld || JumpPressed;
        ProbePressed = s.probe; s.probe = false;
        FirePressed = s.fire; s.fire = false;
        SwitchWeaponPressed = s.switchWeapon; s.switchWeapon = false;
        ReloadPressed = s.reload; s.reload = false;
        HealPressed = s.heal; s.heal = false;
        InteractPressed = s.interact; s.interact = false;
        PausePressed = s.pause; s.pause = false;
        ConfirmPressed = s.confirm; s.confirm = false;

        arrowAvailable = s.arrow.HasValue;
        arrow = s.arrow ?? SeqInput.Up;
        s.arrow = null;
    }

    static bool Pressed(UnityEngine.InputSystem.Controls.ButtonControl a,
                        UnityEngine.InputSystem.Controls.ButtonControl b) =>
        (a != null && a.wasPressedThisFrame) || (b != null && b.wasPressedThisFrame);

    static bool Held(UnityEngine.InputSystem.Controls.ButtonControl a,
                     UnityEngine.InputSystem.Controls.ButtonControl b) =>
        (a != null && a.isPressed) || (b != null && b.isPressed);

    /// Zwraca strzałkę wciśniętą w tej klatce (jedną). Konsumuje ją, żeby dwie
    /// sekwencje naraz nie zjadły tego samego wejścia.
    public bool TryConsumeArrow(out SeqInput pressed)
    {
        pressed = arrow;
        if (!arrowAvailable) return false;
        arrowAvailable = false;
        return true;
    }
}
