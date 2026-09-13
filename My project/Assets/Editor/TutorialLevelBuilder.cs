using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

/// Buduje scenę samouczka od zera: Assets/Scenes/Tutorial.unity.
/// Wszystko deklaratywnie — zmień liczby tutaj i odbuduj (Doomsday → 2. Build Tutorial Level).
public static class TutorialLevelBuilder
{
    const string ScenePath = "Assets/Scenes/Tutorial.unity";
    const string LibPath = "Assets/Resources/SpriteLibrary.asset";
    const string UnlitMatPath = "Assets/Art/SpriteUnlit.mat";
    const string NoFrictionPath = "Assets/Settings/NoFriction.physicsMaterial2D";

    static SpriteLibrary lib;
    static Material mat;
    static PhysicsMaterial2D noFriction;
    static Transform level, enemies, pickups, triggers, world;
    static GameObject player;

    [MenuItem("Doomsday/2. Build Tutorial Level")]
    public static void Build()
    {
        lib = AssetDatabase.LoadAssetAtPath<SpriteLibrary>(LibPath);
        if (lib == null || lib.player == null)
        {
            ArtGenerator.Generate();
            lib = AssetDatabase.LoadAssetAtPath<SpriteLibrary>(LibPath);
        }

        EnsureLayers();
        mat = LoadOrCreateUnlitMaterial();
        noFriction = LoadOrCreateNoFriction();

        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        world = new GameObject("World").transform;
        level = new GameObject("Level").transform;
        enemies = new GameObject("Enemies").transform;
        pickups = new GameObject("Pickups").transform;
        triggers = new GameObject("Triggers").transform;

        player = BuildPlayer(new Vector3(1.5f, 0.55f, 0f));
        BuildCamera();
        BuildBackground();
        new GameObject("HUD").AddComponent<HUD>();
        new GameObject("GameFlow").AddComponent<GameFlow>();

        BuildLevel();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", false),
        };
        AssetDatabase.SaveAssets();
        Debug.Log("[Doomsday] Tutorial level built: " + ScenePath);
    }

    [MenuItem("Doomsday/3. Build Tutorial Level and Play")]
    public static void BuildAndPlay()
    {
        Build();
        EditorApplication.isPlaying = true;
    }

    // ------------------------------------------------------------ poziom

    static void BuildLevel()
    {
        // Granice i sufit
        Wall("WallLeft", -2f, 0f, -8f, 14f);
        Wall("WallRight", 194f, 196f, -8f, 14f);
        Ceiling("Ceiling", -2f, 196f, 9f, 4f);
        Hazard("Abyss", -10f, 210f, -12f, -9f);

        // ---- A: ruch i skok
        Ground("A_Floor", 0f, 12f, 0f, 6f);
        Ground("A_Step", 12f, 14f, 1f, 7f);
        Ground("A_Ledge", 14f, 26f, 2f, 8f);
        Bench(3.2f, 0f);
        Trigger("T_Move", 0.5f, 6f, 0f, 3f, "A / D — ruch", TutorialGoal.Move);
        Trigger("T_Jump", 8f, 12f, 0f, 3f, "SPACJA — skok (przytrzymaj, żeby skoczyć wyżej)", TutorialGoal.Jump);

        // ---- B: podwójny skok
        Ground("B_Floor", 26f, 34f, 0f, 6f);
        Ground("B_Wall", 34f, 36f, 5f, 11f);
        Ground("B_Floor2", 36f, 40f, 0f, 6f);
        AbilityPickup(29f, 1.1f);
        Trigger("T_DoubleJump", 29.5f, 34f, 0f, 5f, "SPACJA w powietrzu — podwójny skok", TutorialGoal.DoubleJump);

        // ---- C: próbnik i ładunek
        Ground("C_Floor", 40f, 62f, 0f, 6f);
        Ground("C_Bump1", 44f, 45f, 1f, 7f);
        Ground("C_Bump2", 57f, 58f, 1f, 7f);
        Walker(49f, 0.6f, false);
        Walker(54f, 0.6f, false);
        Trigger("T_Probe", 40f, 44f, 0f, 3f, "J — dźgnij próbnikiem (W+J w górę)", TutorialGoal.ProbeHit);
        Trigger("T_Charge", 46f, 50f, 0f, 3f,
            "Trafienia próbnikiem ładują ŁADUNEK (niebieski pasek) — z niego leczysz",
            TutorialGoal.None, autoHide: 4f, hideOnExit: false);

        // ---- D: leczenie (jama z zombie)
        Ground("D_Floor", 62f, 74f, -3f, 4f);
        Ground("D_Step1", 74f, 76f, -1.5f, 2.5f);
        Ground("D_Step2", 76f, 78f, 0f, 5f);
        Ground("D_Floor2", 78f, 84f, 0f, 6f);
        Walker(66f, -2.4f, true);
        Walker(70f, -2.4f, true);
        Walker(72.5f, -2.4f, true);
        ChargeCrystal(64.5f, -1.1f);
        Gate("heal", 79f, 0f, 4f, false);
        Trigger("T_Heal", 74f, 78.8f, -3f, 4f,
            "H, potem strzałki: DÓŁ, DÓŁ, GÓRA — zabieg (koszt: 3 ładunku, stój w miejscu)",
            TutorialGoal.Heal, TutorialPrereq.HpBelowMax, completeIfUnmet: true, hideOnExit: false, gate: "heal");

        // ---- E: pogo nad kolcami
        Ground("E_PitFloor", 84f, 98f, -5f, 2f);
        Spikes(84f, 98f, -5f);
        ChargeCrystal(87.5f, 0.6f);
        ChargeCrystal(91f, 0.7f);
        ChargeCrystal(94.5f, 0.6f);
        Flyer(89f, 3f, false);
        Flyer(93f, 3.2f, false);
        Ground("E_Land", 98f, 110f, 0f, 6f);
        Trigger("T_Pogo", 79.5f, 84f, 0f, 4f,
            "S + J w powietrzu — POGO: odbij się od kryształu, wroga lub kolców",
            TutorialGoal.Pogo, hideOnExit: false);

        // ---- F: pistolet i przeładowanie
        WeaponPickup(102f, 0.9f, Ability.Pistol, 12);
        Walker(107.5f, 0.6f, false);
        Trigger("T_Fire", 103f, 109f, 0f, 3f, "K — strzał z pistoletu (w kierunku patrzenia)", TutorialGoal.Fire);
        Trigger("T_Reload", 103f, 118f, 0f, 3f,
            "R, potem strzałki: DÓŁ, GÓRA — przeładowanie",
            TutorialGoal.Reload, TutorialPrereq.PistolMagazineLow, hideOnExit: false);
        Ground("F_Floor", 110f, 128f, 0f, 6f);
        Ceiling("F_LowCeiling", 116f, 128f, 2.6f, 6.4f);
        Heavy(123f, 0.9f);
        Flyer(113f, 4.2f, true);
        Ammo(127f, 0.6f, AmmoPickup.Kind.Pistol, 8);
        Trigger("T_Heavy", 110f, 116f, 0f, 3f,
            "Duży mutant uderza w ziemię — trzymaj dystans i strzelaj, albo przeskocz falę",
            TutorialGoal.None, autoHide: 4f, hideOnExit: false);

        // ---- G: kruszące platformy, próbka, ławka
        Ground("G_PitFloor", 128f, 138f, -4f, 2f);
        Spikes(128f, 138f, -4f);
        Crumble(130f, 0f, 1.8f);
        Crumble(133f, 0.4f, 1.8f);
        Crumble(136f, 0f, 1.8f);
        Trigger("T_Crumble", 124f, 128f, 0f, 3f,
            "Kryształowe platformy kruszą się pod ciężarem — nie zatrzymuj się",
            TutorialGoal.None, autoHide: 4f, hideOnExit: false);
        Ground("G_Floor", 138f, 148f, 0f, 6f);
        Ground("G_Pedestal", 140f, 141f, 1.2f, 7.2f);
        SampleNode(142.2f, 2.2f, "Maź ze ścian");
        Bench(146f, 0f);

        // ---- H: rewolwer
        Ground("H_Floor", 148f, 158f, 0f, 6f);
        WeaponPickup(150f, 0.9f, Ability.Revolver, 3);
        Walker(156.5f, 0.6f, false);
        Trigger("T_Switch", 150.5f, 154f, 0f, 3f, "TAB — zmiana broni na rewolwer", TutorialGoal.SwitchWeapon);
        Trigger("T_Load", 154f, 158f, 0f, 3f,
            "R, potem obrót: DÓŁ, LEWO, GÓRA, PRAWO… — każda strzałka ładuje 1 komorę. K przerywa i strzela tym, co masz",
            TutorialGoal.LoadRevolver, hideOnExit: false);

        // ---- I: arena bossa
        var entry = Gate("entry", 158.5f, 0f, 5f, true);
        Ground("I_WallL", 158f, 159f, 9f, 4f);
        Ground("I_Floor", 158f, 184f, 0f, 6f);
        Ground("I_WallR", 183f, 184f, 9f, 4f);
        var exit = Gate("exit", 183.5f, 0f, 5f, false);
        var boss = Boss(176f, 1.45f, 159.6f, 182.4f);
        Ammo(161f, 0.6f, AmmoPickup.Kind.Revolver, 2);
        Ammo(181f, 0.6f, AmmoPickup.Kind.Revolver, 2);
        Ammo(165f, 0.6f, AmmoPickup.Kind.Pistol, 12);
        Ammo(171f, 0.6f, AmmoPickup.Kind.Pistol, 12);
        Ammo(177f, 0.6f, AmmoPickup.Kind.Pistol, 12);
        var arena = TriggerBox("BossArena", 160.5f, 162.5f, 0f, 8f, triggers).gameObject.AddComponent<BossArena>();
        arena.boss = boss;
        arena.entryGate = entry;
        arena.exitGate = exit;
        Trigger("T_Boss", 159f, 160.5f, 0f, 8f,
            "Szarża kończy się ogłuszeniem o ścianę — to okno na rewolwer",
            TutorialGoal.None, autoHide: 4f, hideOnExit: false);

        // ---- J: wyjście
        Ground("J_Floor", 184f, 194f, 0f, 6f);
        TriggerBox("LevelExit", 191f, 193f, 0f, 5f, triggers).gameObject.AddComponent<LevelExit>();
    }

    // ------------------------------------------------------------ gracz

    static GameObject BuildPlayer(Vector3 pos)
    {
        var go = new GameObject("Player");
        go.transform.position = pos;
        go.layer = Layers.Player;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.6f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sharedMaterial = noFriction;

        var col = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.68f, 1.0f);
        col.sharedMaterial = noFriction;

        var sprite = Child(go, "Sprite", Vector3.zero);
        var sr = sprite.AddComponent<SpriteRenderer>();
        sr.sprite = lib.player;
        sr.sharedMaterial = mat;
        sr.sortingOrder = 6;

        var groundCheck = Child(go, "GroundCheck", new Vector3(0f, -0.5f, 0f));
        var hand = Child(go, "HandPoint", new Vector3(0.35f, 0.05f, 0f));

        go.AddComponent<PlayerInputReader>();
        go.AddComponent<SequenceRunner>();

        var hp = go.AddComponent<Health>();
        hp.maxHealth = 6;
        hp.invulnerabilityTime = 0.8f;

        var charge = go.AddComponent<ChargeMeter>();
        charge.maxCharge = 9;
        charge.healCost = 3;

        var pouch = go.AddComponent<AmmoPouch>();
        pouch.pistolRounds = 0;
        pouch.revolverRounds = 0;

        go.AddComponent<PlayerAbilities>();

        var move = go.AddComponent<PlayerMovement>();
        move.groundCheck = groundCheck.transform;
        move.groundLayer = Layers.GroundMask;
        move.spriteTransform = sprite.transform;
        move.jumpForce = 15f;

        go.AddComponent<ProbeAttack>();
        go.AddComponent<HealAction>();

        var pistol = Child(hand, "Pistol", Vector3.zero).AddComponent<Pistol>();
        var revolver = Child(hand, "Revolver", Vector3.zero).AddComponent<Revolver>();
        var holder = go.AddComponent<WeaponHolder>();
        holder.weapons = new Weapon[] { pistol, revolver };

        go.AddComponent<PlayerRespawn>();
        var fb = go.AddComponent<PlayerFeedback>();
        fb.sprite = sr;
        return go;
    }

    static void BuildCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(1.5f, 1.5f, -10f);

        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.055f, 0.063f, 0.095f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.GetUniversalAdditionalCameraData();

        go.AddComponent<AudioListener>();
        go.AddComponent<CameraShake>();

        var follow = go.AddComponent<CameraFollow>();
        follow.target = player.transform;
        follow.useBounds = true;
        follow.minX = 0f; follow.maxX = 194f;
        follow.minY = -6f; follow.maxY = 9.5f;
    }

    static void BuildBackground()
    {
        Parallax("BgFar", lib.bgFar, 0.9f, -30);
        Parallax("BgMid", lib.bgMid, 0.72f, -20);
        Parallax("BgNear", lib.bgNear, 0.48f, -10);
    }

    static void Parallax(string name, Sprite sprite, float factor, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(world, false);
        go.transform.position = new Vector3(90f, 3f, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(272f, 16f);
        sr.sortingOrder = order;
        go.AddComponent<ParallaxBackground>().factor = factor;
    }

    // ------------------------------------------------------------ geometria

    static GameObject Ground(string name, float x0, float x1, float yTop, float depth)
    {
        float w = x1 - x0;
        var go = New(name, level, new Vector3((x0 + x1) * 0.5f, yTop - depth * 0.5f, 0f), Layers.Ground);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, depth);

        if (depth > 1f)
        {
            Tile(go, "Top", lib.tileTop, w, 1f, new Vector2(0f, depth * 0.5f - 0.5f), 0);
            Tile(go, "Fill", lib.tileFill, w, depth - 1f, new Vector2(0f, -0.5f), -1);
        }
        else Tile(go, "Top", lib.tileTop, w, depth, Vector2.zero, 0);
        return go;
    }

    static GameObject Wall(string name, float x0, float x1, float y0, float y1)
    {
        var go = New(name, level, new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 0f), Layers.Ground);
        go.AddComponent<BoxCollider2D>().size = new Vector2(x1 - x0, y1 - y0);
        Tile(go, "Fill", lib.tileFill, x1 - x0, y1 - y0, Vector2.zero, 0);
        return go;
    }

    static GameObject Ceiling(string name, float x0, float x1, float yBottom, float depth)
    {
        var go = New(name, level, new Vector3((x0 + x1) * 0.5f, yBottom + depth * 0.5f, 0f), Layers.Ground);
        go.AddComponent<BoxCollider2D>().size = new Vector2(x1 - x0, depth);
        var lip = Tile(go, "Lip", lib.tileTop, x1 - x0, 1f, new Vector2(0f, -depth * 0.5f + 0.5f), 0);
        lip.flipY = true;
        if (depth > 1f) Tile(go, "Fill", lib.tileFill, x1 - x0, depth - 1f, new Vector2(0f, 0.5f), -1);
        return go;
    }

    static GameObject Spikes(float x0, float x1, float yBase)
    {
        float w = x1 - x0;
        var go = New("Spikes", level, new Vector3((x0 + x1) * 0.5f, yBase + 0.25f, 0f), Layers.Hazard);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(w - 0.2f, 0.3f);
        col.offset = new Vector2(0f, 0.08f);
        Tile(go, "Sprite", lib.spikes, w, 0.5f, Vector2.zero, 1);
        go.AddComponent<Hazard>();
        go.AddComponent<PogoSurface>();
        return go;
    }

    static GameObject Hazard(string name, float x0, float x1, float y0, float y1)
    {
        var go = New(name, level, new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 0f), Layers.Hazard);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(x1 - x0, y1 - y0);
        go.AddComponent<Hazard>();
        return go;
    }

    static GameObject Crumble(float x, float y, float w)
    {
        var go = New("Crumble", level, new Vector3(x, y, 0f), Layers.Ground);
        go.AddComponent<BoxCollider2D>().size = new Vector2(w, 0.36f);
        Tile(go, "Sprite", lib.crumble, w, 0.375f, Vector2.zero, 1);
        go.AddComponent<CrumblingPlatform>();
        return go;
    }

    static Gate Gate(string id, float x, float yBottom, float height, bool startOpen)
    {
        var go = New("Gate_" + id, level, new Vector3(x, yBottom + height * 0.5f, 0f), Layers.Ground);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1f, height);
        Tile(go, "Sprite", lib.gate, 1f, height, Vector2.zero, 1);
        var gate = go.AddComponent<Gate>();
        gate.id = id;
        gate.height = height + 0.2f;
        gate.startOpen = startOpen;
        return gate;
    }

    static void Bench(float x, float yFloor)
    {
        var go = New("Bench", world, new Vector3(x, yFloor + 0.31f, 0f), 0);
        Sprite(go, lib.bench, 2);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.4f, 1.6f);
        col.offset = new Vector2(0f, 0.5f);
        go.AddComponent<Bench>();
    }

    static void SampleNode(float x, float y, string name)
    {
        var go = New("Sample_" + name, world, new Vector3(x, y, 0f), 0);
        Sprite(go, lib.sampleNode, 2);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.0f;
        go.AddComponent<SampleNode>().sampleName = name;
    }

    static void ChargeCrystal(float x, float y)
    {
        var go = New("ChargeCrystal", world, new Vector3(x, y, 0f), 0);
        Sprite(go, lib.chargeCrystal, 2);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.62f, 0.88f);
        go.AddComponent<ChargeCrystal>();
        go.AddComponent<PogoSurface>();
    }

    // ------------------------------------------------------------ przeciwnicy

    static GameObject Walker(float x, float y, bool chase)
    {
        var go = EnemyBase("Walker", x, y, lib.walker, new Vector2(0.8f, 0.7f), 6, 0.1f, out var sr, out var fb);
        fb.deathColor = new Color(0.8f, 0.25f, 0.25f);
        fb.dropKind = AmmoPickup.Kind.Pistol;
        fb.dropAmount = 3;
        fb.dropChance = 0.35f;
        var walker = go.AddComponent<EnemyWalker>();
        walker.sprite = sr;
        walker.groundLayer = Layers.GroundMask;
        walker.chasePlayer = chase;
        return go;
    }

    static GameObject Heavy(float x, float y)
    {
        var go = EnemyBase("Heavy", x, y, lib.heavy, new Vector2(1.1f, 1.3f), 14, 0.12f, out var sr, out var fb);
        go.GetComponent<Rigidbody2D>().mass = 4f;
        fb.deathColor = new Color(0.55f, 0.15f, 0.15f);
        fb.knockbackResistance = 0.85f;
        fb.dropKind = AmmoPickup.Kind.Pistol;
        fb.dropAmount = 6;
        fb.dropChance = 0.8f;
        go.AddComponent<EnemyHeavy>().sprite = sr;
        return go;
    }

    static GameObject Flyer(float x, float y, bool aggressive)
    {
        var go = New("Flyer", enemies, new Vector3(x, y, 0f), Layers.Enemy);
        var sr = Sprite(go, lib.flyer, 4);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        var hp = go.AddComponent<Health>();
        hp.maxHealth = 2;
        hp.invulnerabilityTime = 0.1f;
        var fb = go.AddComponent<EnemyFeedback>();
        fb.sprite = sr;
        fb.deathColor = new Color(0.7f, 0.4f, 0.9f);
        fb.knockbackResistance = 1f;
        fb.dropChance = 0.2f;
        fb.dropAmount = 3;
        go.AddComponent<ContactDamage>().damage = 1;
        var fl = go.AddComponent<EnemyFlyer>();
        fl.aggressive = aggressive;
        fl.sprite = sr;
        return go;
    }

    static Boss Boss(float x, float y, float arenaLeft, float arenaRight)
    {
        var go = EnemyBase("Boss_ALFA", x, y, lib.boss, new Vector2(2.0f, 2.4f), 80, 0.15f, out var sr, out var fb);
        var rb = go.GetComponent<Rigidbody2D>();
        rb.mass = 10f;
        fb.big = true;
        fb.deathColor = new Color(0.6f, 0.15f, 0.3f);
        fb.knockbackResistance = 1f;
        fb.dropKind = AmmoPickup.Kind.Revolver;
        fb.dropAmount = 2;
        fb.dropChance = 1f;
        var boss = go.AddComponent<Boss>();
        boss.sprite = sr;
        boss.arenaLeft = arenaLeft;
        boss.arenaRight = arenaRight;
        return boss;
    }

    static GameObject EnemyBase(string name, float x, float y, Sprite sprite, Vector2 colliderSize, int hp, float invuln,
                                out SpriteRenderer sr, out EnemyFeedback fb)
    {
        var go = New(name, enemies, new Vector3(x, y, 0f), Layers.Enemy);
        var spriteGo = Child(go, "Sprite", Vector3.zero);
        sr = spriteGo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = 4;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sharedMaterial = noFriction;

        go.AddComponent<BoxCollider2D>().size = colliderSize;

        var health = go.AddComponent<Health>();
        health.maxHealth = hp;
        health.invulnerabilityTime = invuln;

        fb = go.AddComponent<EnemyFeedback>();
        fb.sprite = sr;
        go.AddComponent<ContactDamage>().damage = 1;
        return go;
    }

    // ------------------------------------------------------------ pickupy i triggery

    static void Ammo(float x, float y, AmmoPickup.Kind kind, int amount)
    {
        var go = New("Ammo_" + kind, pickups, new Vector3(x, y, 0f), 0);
        Sprite(go, kind == AmmoPickup.Kind.Pistol ? lib.ammoPistol : lib.ammoRevolver, 3);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.6f);
        var p = go.AddComponent<AmmoPickup>();
        p.kind = kind;
        p.amount = amount;
    }

    static void WeaponPickup(float x, float y, Ability weapon, int ammo)
    {
        var go = New("Weapon_" + weapon, pickups, new Vector3(x, y, 0f), 0);
        Sprite(go, weapon == Ability.Pistol ? lib.pickupPistol : lib.pickupRevolver, 3);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.1f, 0.9f);
        var p = go.AddComponent<WeaponPickup>();
        p.weapon = weapon;
        p.ammo = ammo;
    }

    static void AbilityPickup(float x, float y)
    {
        var go = New("Ability_DoubleJump", pickups, new Vector3(x, y, 0f), 0);
        Sprite(go, lib.pickupDoubleJump, 3);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.0f, 1.0f);
        go.AddComponent<AbilityPickup>().ability = Ability.DoubleJump;
    }

    static void Trigger(string name, float x0, float x1, float y0, float y1, string prompt, TutorialGoal goal,
                        TutorialPrereq prereq = TutorialPrereq.None, bool completeIfUnmet = false,
                        float autoHide = 0f, bool hideOnExit = true, string gate = null)
    {
        var t = TriggerBox(name, x0, x1, y0, y1, triggers).gameObject.AddComponent<TutorialTrigger>();
        t.prompt = prompt;
        t.goal = goal;
        t.prereq = prereq;
        t.completeIfPrereqUnmet = completeIfUnmet;
        t.autoHideAfter = autoHide;
        t.hideOnExit = hideOnExit;
        t.gateToOpen = gate;
    }

    static BoxCollider2D TriggerBox(string name, float x0, float x1, float y0, float y1, Transform parent)
    {
        var go = New(name, parent, new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 0f), 0);
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(x1 - x0, y1 - y0);
        return col;
    }

    // ------------------------------------------------------------ narzędzia

    static GameObject New(string name, Transform parent, Vector3 pos, int layer)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.layer = layer;
        return go;
    }

    static GameObject Child(GameObject parent, string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.layer = parent.layer;
        return go;
    }

    static SpriteRenderer Sprite(GameObject go, Sprite sprite, int order)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = order;
        return sr;
    }

    static SpriteRenderer Tile(GameObject parent, string name, Sprite sprite, float w, float h, Vector2 localPos, int order)
    {
        var go = Child(parent, name, localPos);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(w, h);
        sr.sortingOrder = order;
        return sr;
    }

    static Material LoadOrCreateUnlitMaterial()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(UnlitMatPath);
        if (m != null) return m;

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        m = new Material(shader);
        System.IO.Directory.CreateDirectory("Assets/Art");
        AssetDatabase.CreateAsset(m, UnlitMatPath);
        return m;
    }

    static PhysicsMaterial2D LoadOrCreateNoFriction()
    {
        var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(NoFrictionPath);
        if (m != null) return m;
        m = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(m, NoFrictionPath);
        return m;
    }

    static void EnsureLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        var wanted = new Dictionary<int, string>
        {
            { Layers.Ground, "Ground" }, { Layers.Player, "Player" }, { Layers.Enemy, "Enemy" }, { Layers.Hazard, "Hazard" },
        };
        foreach (var kv in wanted)
        {
            var slot = layers.GetArrayElementAtIndex(kv.Key);
            if (string.IsNullOrEmpty(slot.stringValue)) slot.stringValue = kv.Value;
        }
        tagManager.ApplyModifiedProperties();
    }
}
