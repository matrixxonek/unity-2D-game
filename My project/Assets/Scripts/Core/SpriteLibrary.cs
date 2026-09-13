using UnityEngine;

/// Jedno miejsce z wszystkimi sprite'ami gry. Placeholdery generuje
/// Doomsday → Generate Placeholder Art; podmiana na docelową grafikę to
/// przypięcie innych sprite'ów w tym assecie.
[CreateAssetMenu(menuName = "Doomsday/Sprite Library", fileName = "SpriteLibrary")]
public class SpriteLibrary : ScriptableObject
{
    static SpriteLibrary instance;

    /// Ładowane z Resources/SpriteLibrary.
    public static SpriteLibrary I
    {
        get
        {
            if (instance == null) instance = Resources.Load<SpriteLibrary>("SpriteLibrary");
            return instance;
        }
    }

    [Header("Gracz")]
    public Sprite player;
    public Sprite playerJump;
    public Sprite probe;
    public Sprite muzzle;

    [Header("Świat")]
    public Sprite tileFill;
    public Sprite tileTop;
    public Sprite spikes;
    public Sprite crumble;
    public Sprite gate;
    public Sprite bench;
    public Sprite sampleNode;
    public Sprite chargeCrystal;

    [Header("Przeciwnicy")]
    public Sprite walker;
    public Sprite flyer;
    public Sprite heavy;
    public Sprite boss;
    public Sprite shockwave;

    [Header("Pickupy")]
    public Sprite ammoPistol;
    public Sprite ammoRevolver;
    public Sprite pickupPistol;
    public Sprite pickupRevolver;
    public Sprite pickupDoubleJump;

    [Header("UI i tło")]
    public Sprite arrow;
    public Sprite bgFar;
    public Sprite bgMid;
    public Sprite bgNear;
}
