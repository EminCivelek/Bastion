using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public static class BastionSetup
{
    private const string ArtDir = "Assets/Art/Placeholder";
    private const string SquarePngPath = ArtDir + "/Square.png";
    private const string PrefabDir = "Assets/Prefabs";
    private const string SceneDir = "Assets/Scenes";
    private const string GrassTileDir = "Assets/Cainos/Pixel Art Top Down - Basic/Tile Palette/TP Grass";
    private const string ScenePath = SceneDir + "/Arena.unity";

    [MenuItem("Bastion/Setup Prototype Scene")]
    public static void SetupAll()
    {
        Sprite square = EnsureSquareSprite();
        Material spriteMat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        GameObject towerPrefab = EnsureTowerPrefab(square, spriteMat);
        GameObject projectilePrefab = EnsureProjectilePrefab(square, spriteMat);
        GameObject enemyPrefab = EnsureEnemyPrefab(square, spriteMat);

        EnemyConfig enemyBasic = EnsureEnemyConfig("EnemyBasic", enemyPrefab, 30f, 2f, 5, new Color(0.8f, 0.2f, 0.2f));
        EnemyConfig enemyFast = EnsureEnemyConfig("EnemyFast", enemyPrefab, 16f, 4f, 4, new Color(0.9f, 0.6f, 0.1f));

        TowerConfig towerArrow = EnsureTowerConfig("TowerArrow", towerPrefab, projectilePrefab, 20, 3.5f, 6f, 1.5f, 0f, 8f, TargetingType.First, new Color(0.2f, 0.5f, 0.9f));
        TowerConfig towerCannon = EnsureTowerConfig("TowerCannon", towerPrefab, projectilePrefab, 45, 2.8f, 14f, 0.6f, 1f, 5f, TargetingType.Strongest, new Color(0.6f, 0.3f, 0.7f));
        TowerConfig towerSlow = EnsureTowerConfig("TowerRapid", towerPrefab, projectilePrefab, 30, 3f, 3f, 3.5f, 0f, 9f, TargetingType.Closest, new Color(0.2f, 0.8f, 0.5f));

        WaveConfig waveTier1 = EnsureWaveConfig(enemyBasic, enemyFast);

        MetaUpgradeConfig upDamage = EnsureUpgrade("meta_damage", "Sharper Weapons", "Increase tower damage.", MetaUpgradeEffect.DamageMultiplier, 0.08f, 10, 20, 1.3f);
        MetaUpgradeConfig upGold = EnsureUpgrade("meta_gold", "Starting Gold", "More gold at the start of each run.", MetaUpgradeEffect.StartingCurrency, 10f, 8, 25, 1.3f);
        MetaUpgradeConfig upHP = EnsureUpgrade("meta_basehp", "Reinforced Base", "Increase base max HP.", MetaUpgradeEffect.BaseMaxHP, 5f, 8, 20, 1.3f);

        BuildScene(square, spriteMat, waveTier1, new[] { towerArrow, towerCannon, towerSlow }, new List<MetaUpgradeConfig> { upDamage, upGold, upHP });
        EnsureAltarSlots();
        ApplyTowerVisuals();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Bastion prototype setup complete.");
    }

    private static Sprite EnsureSquareSprite()
    {
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePngPath);
        if (existing != null) return existing;

        Directory.CreateDirectory(ArtDir);
        Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(SquarePngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(SquarePngPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(SquarePngPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(SquarePngPath);
    }

    private static GameObject EnsureTowerPrefab(Sprite sprite, Material mat)
    {
        string path = PrefabDir + "/TowerBase.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("TowerBase");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.material = mat;
        sr.color = Color.white;
        go.transform.localScale = Vector3.one * 0.8f;
        go.AddComponent<Tower>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject EnsureProjectilePrefab(Sprite sprite, Material mat)
    {
        string path = PrefabDir + "/Projectile.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("Projectile");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.material = mat;
        sr.color = Color.yellow;
        go.transform.localScale = Vector3.one * 0.2f;
        go.AddComponent<Projectile>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject EnsureEnemyPrefab(Sprite sprite, Material mat)
    {
        string path = PrefabDir + "/EnemyBase.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject go = new GameObject("EnemyBase");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.material = mat;
        sr.color = Color.white;
        go.transform.localScale = Vector3.one * 0.5f;
        go.AddComponent<Enemy>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static EnemyConfig EnsureEnemyConfig(string id, GameObject prefab, float hp, float speed, int gold, Color tint)
    {
        string path = $"Assets/Resources/Enemies/{id}.asset";
        EnemyConfig cfg = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<EnemyConfig>();
            AssetDatabase.CreateAsset(cfg, path);
        }

        cfg.enemyId = id;
        cfg.displayName = id;
        cfg.prefab = MakeTintedVariant(prefab, id, tint);
        cfg.maxHP = hp;
        cfg.moveSpeed = speed;
        cfg.baseGoldReward = gold;
        cfg.baseHitDamageToBase = 1;
        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    private static TowerConfig EnsureTowerConfig(string id, GameObject prefab, GameObject projectilePrefab, int cost, float range, float damage, float fireRate, float splash, float projectileSpeed, TargetingType targeting, Color tint)
    {
        string path = $"Assets/Resources/Towers/{id}.asset";
        TowerConfig cfg = AssetDatabase.LoadAssetAtPath<TowerConfig>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<TowerConfig>();
            AssetDatabase.CreateAsset(cfg, path);
        }

        cfg.towerId = id;
        cfg.displayName = id;
        cfg.prefab = MakeTintedVariant(prefab, id, tint);
        cfg.projectilePrefab = projectilePrefab;
        cfg.cost = cost;
        cfg.range = range;
        cfg.damage = damage;
        cfg.fireRate = fireRate;
        cfg.splashRadius = splash;
        cfg.projectileSpeed = 9f;
        cfg.targeting = targeting;
        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    private static GameObject MakeTintedVariant(GameObject basePrefab, string id, Color tint)
    {
        string path = $"{PrefabDir}/{id}.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = id;
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = tint;

        GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return variant;
    }

    private static WaveConfig EnsureWaveConfig(EnemyConfig basic, EnemyConfig fast)
    {
        string path = "Assets/Resources/Waves/Tier1.asset";
        WaveConfig cfg = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<WaveConfig>();
            AssetDatabase.CreateAsset(cfg, path);
        }

        cfg.tierId = "Tier1";
        cfg.tierIndex = 1;
        cfg.baseStartingCurrency = 50;
        cfg.waves = new List<WaveEntry>
        {
            new WaveEntry { enemy = basic, count = 6, spawnInterval = 0.9f, delayBeforeWave = 2f },
            new WaveEntry { enemy = basic, count = 8, spawnInterval = 0.7f, delayBeforeWave = 5f },
            new WaveEntry { enemy = fast, count = 6, spawnInterval = 0.6f, delayBeforeWave = 5f },
            new WaveEntry { enemy = basic, count = 10, spawnInterval = 0.6f, delayBeforeWave = 6f },
            new WaveEntry { enemy = fast, count = 10, spawnInterval = 0.5f, delayBeforeWave = 6f },
            new WaveEntry { enemy = basic, count = 16, spawnInterval = 0.4f, delayBeforeWave = 6f },
        };
        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    private static MetaUpgradeConfig EnsureUpgrade(string id, string name, string desc, MetaUpgradeEffect effect, float perLevel, int maxLevel, int baseCost, float growth)
    {
        string path = $"Assets/Resources/MetaUpgrades/{id}.asset";
        MetaUpgradeConfig cfg = AssetDatabase.LoadAssetAtPath<MetaUpgradeConfig>(path);
        if (cfg == null)
        {
            cfg = ScriptableObject.CreateInstance<MetaUpgradeConfig>();
            AssetDatabase.CreateAsset(cfg, path);
        }

        cfg.upgradeId = id;
        cfg.displayName = name;
        cfg.description = desc;
        cfg.effect = effect;
        cfg.effectPerLevel = perLevel;
        cfg.maxLevel = maxLevel;
        cfg.baseCost = baseCost;
        cfg.costGrowth = growth;
        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    private static void BuildScene(Sprite square, Material spriteMat, WaveConfig waveConfig, TowerConfig[] towers, List<MetaUpgradeConfig> upgrades)
    {
        Directory.CreateDirectory(SceneDir);
        Scene scene = EditorSceneManager.GetActiveScene();
        bool alreadyOpen = scene.IsValid() && scene.path == ScenePath;

        if (!alreadyOpen)
        {
            scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                ? EditorSceneManager.OpenScene(ScenePath)
                : EditorSceneManager.GetActiveScene();

            if (!scene.IsValid())
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera", typeof(Camera));
            cam = camGO.GetComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(4f, 3f, -10f);
        cam.tag = "MainCamera";
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        Transform pathParent = FindOrCreate("Path").transform;
        Vector2[] waypointPositions =
        {
            new Vector2(-1f, 5f), new Vector2(2f, 5f), new Vector2(2f, 2f),
            new Vector2(6f, 2f), new Vector2(6f, 4f), new Vector2(9f, 4f),
        };
        Transform[] waypoints = new Transform[waypointPositions.Length];
        for (int i = 0; i < waypointPositions.Length; i++)
        {
            Transform wp = FindOrCreateChild(pathParent, $"Waypoint_{i}");
            wp.position = waypointPositions[i];
            waypoints[i] = wp;
        }

        BuildTerrain(waypointPositions);

        GameObject spawnMarker = FindOrCreate("SpawnPoint");
        spawnMarker.transform.position = waypointPositions[0];

        GameObject baseGO = FindOrCreate("PlayerBase");
        baseGO.transform.position = waypointPositions[waypointPositions.Length - 1];
        SpriteRenderer baseSr = baseGO.GetComponent<SpriteRenderer>();
        if (baseSr == null) baseSr = baseGO.AddComponent<SpriteRenderer>();
        baseSr.sprite = square;
        baseSr.material = spriteMat;
        baseSr.color = new Color(0.3f, 0.7f, 1f);
        baseGO.transform.localScale = Vector3.one * 0.9f;
        PlayerBase playerBase = baseGO.GetComponent<PlayerBase>() ?? baseGO.AddComponent<PlayerBase>();

        GameObject towerContainerGO = FindOrCreate("Towers");

        GameObject spawnerGO = FindOrCreate("WaveSpawner");
        WaveSpawner spawner = spawnerGO.GetComponent<WaveSpawner>() ?? spawnerGO.AddComponent<WaveSpawner>();
        SetPrivateField(spawner, "spawnPoint", spawnMarker.transform);
        SetPrivateField(spawner, "path", waypoints);

        GameObject metaGO = FindOrCreate("MetaProgressionManager");
        MetaProgressionManager meta = metaGO.GetComponent<MetaProgressionManager>() ?? metaGO.AddComponent<MetaProgressionManager>();
        SetPrivateField(meta, "allUpgrades", upgrades);

        GameObject runGO = FindOrCreate("RunManager");
        RunManager run = runGO.GetComponent<RunManager>() ?? runGO.AddComponent<RunManager>();
        SetPrivateField(run, "waveConfig", waveConfig);
        SetPrivateField(run, "waveSpawner", spawner);
        SetPrivateField(run, "playerBase", playerBase);

        GameObject placementGO = FindOrCreate("PlacementManager");
        PlacementManager placement = placementGO.GetComponent<PlacementManager>() ?? placementGO.AddComponent<PlacementManager>();
        SetPrivateField(placement, "arenaCamera", cam);
        SetPrivateField(placement, "availableTowers", towers);
        SetPrivateField(placement, "towerContainer", towerContainerGO.transform);

        BuildHUD(towers, placement);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettingsScene[] scenes = { new EditorBuildSettingsScene(ScenePath, true) };
        EditorBuildSettings.scenes = scenes;
    }

    private static void BuildTerrain(Vector2[] waypointPositions)
    {
        if (GameObject.Find("TerrainGrid") != null)
            return; // terrain is hand-painted after first generation - never auto-regenerate it

        TileBase[] grassTiles = LoadCainosTiles("TX Tileset Grass ", 16);
        TileBase[] pavementTiles = LoadCainosTiles("TX Tileset Grass Pavement ", 8);

        if (grassTiles.Length == 0)
        {
            Debug.LogWarning("Bastion: Cainos grass tiles not found under " + GrassTileDir + " - skipping terrain.");
            return;
        }

        GameObject gridGO = FindOrCreate("TerrainGrid");
        Grid grid = gridGO.GetComponent<Grid>();
        if (grid == null) grid = gridGO.AddComponent<Grid>();
        grid.cellSize = Vector3.one;
        gridGO.transform.SetSiblingIndex(0);

        Tilemap groundMap = GetOrCreateTilemap(gridGO.transform, "GroundTilemap", -10);
        Tilemap pathMap = GetOrCreateTilemap(gridGO.transform, "PathTilemap", -9);
        groundMap.ClearAllTiles();
        pathMap.ClearAllTiles();

        System.Random rng = new System.Random(12345);

        const int minX = -8, maxX = 16, minY = -4, maxY = 10;
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                groundMap.SetTile(new Vector3Int(x, y, 0), grassTiles[rng.Next(grassTiles.Length)]);
            }
        }

        if (pavementTiles.Length > 0)
        {
            HashSet<Vector3Int> pathCells = new HashSet<Vector3Int>();
            for (int i = 0; i < waypointPositions.Length - 1; i++)
            {
                Vector2 a = waypointPositions[i];
                Vector2 b = waypointPositions[i + 1];

                if (Mathf.Approximately(a.y, b.y))
                {
                    int y = Mathf.RoundToInt(a.y) - 1;
                    int xMin = Mathf.RoundToInt(Mathf.Min(a.x, b.x));
                    int xMax = Mathf.RoundToInt(Mathf.Max(a.x, b.x)) - 1;
                    for (int x = xMin; x <= xMax; x++)
                    {
                        pathCells.Add(new Vector3Int(x, y, 0));
                        pathCells.Add(new Vector3Int(x, y - 1, 0));
                    }
                }
                else
                {
                    int x = Mathf.RoundToInt(a.x) - 1;
                    int yMin = Mathf.RoundToInt(Mathf.Min(a.y, b.y));
                    int yMax = Mathf.RoundToInt(Mathf.Max(a.y, b.y)) - 1;
                    for (int y = yMin; y <= yMax; y++)
                    {
                        pathCells.Add(new Vector3Int(x, y, 0));
                        pathCells.Add(new Vector3Int(x - 1, y, 0));
                    }
                }
            }

            foreach (Vector3Int cell in pathCells)
                pathMap.SetTile(cell, pavementTiles[rng.Next(pavementTiles.Length)]);
        }
    }

    private static TileBase[] LoadCainosTiles(string namePrefix, int count)
    {
        List<TileBase> tiles = new List<TileBase>();
        for (int i = 0; i < count; i++)
        {
            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>($"{GrassTileDir}/{namePrefix}{i}.asset");
            if (tile != null) tiles.Add(tile);
        }
        return tiles.ToArray();
    }

    private static Tilemap GetOrCreateTilemap(Transform parent, string name, int sortingOrder)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null) go.transform.SetParent(parent, false);

        Tilemap tilemap = go.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = go.AddComponent<Tilemap>();
        TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
        if (renderer == null) renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private static void BuildHUD(TowerConfig[] towers, PlacementManager placement)
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        RectTransform topBar = FindOrCreateUI(canvasGO.transform, "TopBar");
        topBar.anchorMin = new Vector2(0, 1);
        topBar.anchorMax = new Vector2(1, 1);
        topBar.pivot = new Vector2(0.5f, 1f);
        topBar.sizeDelta = new Vector2(0, 80);
        topBar.anchoredPosition = Vector2.zero;

        TMP_Text currencyLabel = CreateLabel(topBar, "CurrencyLabel", "Gold: 0", TextAlignmentOptions.Left, new Vector2(0, 0.5f), new Vector2(0.3f, 0.5f));
        TMP_Text waveLabel = CreateLabel(topBar, "WaveLabel", "Wave -/-", TextAlignmentOptions.Center, new Vector2(0.35f, 0.5f), new Vector2(0.65f, 0.5f));

        RectTransform hpBarBg = FindOrCreateUI(canvasGO.transform, "BaseHPBarBg");
        hpBarBg.anchorMin = new Vector2(0.7f, 0.93f);
        hpBarBg.anchorMax = new Vector2(0.98f, 0.98f);
        hpBarBg.offsetMin = Vector2.zero;
        hpBarBg.offsetMax = Vector2.zero;
        Image bgImg = hpBarBg.gameObject.GetComponent<Image>() ?? hpBarBg.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.18f);

        RectTransform hpFill = FindOrCreateUI(hpBarBg, "Fill");
        hpFill.anchorMin = new Vector2(0, 0);
        hpFill.anchorMax = new Vector2(1, 1);
        hpFill.offsetMin = Vector2.zero;
        hpFill.offsetMax = Vector2.zero;
        Image hpFillImg = hpFill.gameObject.GetComponent<Image>() ?? hpFill.gameObject.AddComponent<Image>();
        hpFillImg.color = new Color(0.3f, 0.85f, 0.4f);
        hpFillImg.type = Image.Type.Filled;
        hpFillImg.fillMethod = Image.FillMethod.Horizontal;
        hpFillImg.fillAmount = 1f;

        TMP_Text hpLabel = CreateLabel(hpBarBg, "HPLabel", "0/0", TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        hpLabel.fontSize = 24;
        hpLabel.color = Color.white;

        TowerSelectionPopupUI popup = BuildTowerPopup(canvasGO.transform);
        SetPrivateField(placement, "popup", popup);

        RectTransform resultPanel = FindOrCreateUI(canvasGO.transform, "ResultPanel");
        resultPanel.anchorMin = new Vector2(0.5f, 0.5f);
        resultPanel.anchorMax = new Vector2(0.5f, 0.5f);
        resultPanel.sizeDelta = new Vector2(500, 260);
        resultPanel.anchoredPosition = Vector2.zero;
        Image panelImg = resultPanel.gameObject.GetComponent<Image>() ?? resultPanel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.13f, 0.95f);
        resultPanel.gameObject.SetActive(false);

        TMP_Text resultLabel = CreateLabel(resultPanel, "ResultLabel", "Victory", TextAlignmentOptions.Center, new Vector2(0, 0.55f), new Vector2(1, 0.95f));
        resultLabel.fontSize = 42;
        TMP_Text resultReward = CreateLabel(resultPanel, "RewardLabel", "+0 Meta Currency", TextAlignmentOptions.Center, new Vector2(0, 0.25f), new Vector2(1, 0.55f));

        GameObject hudGO = FindOrCreate("HUDController");
        HUDController hud = hudGO.GetComponent<HUDController>() ?? hudGO.AddComponent<HUDController>();
        SetPrivateField(hud, "currencyLabel", currencyLabel);
        SetPrivateField(hud, "waveLabel", waveLabel);
        SetPrivateField(hud, "baseHPFill", hpFillImg);
        SetPrivateField(hud, "baseHPLabel", hpLabel);
        SetPrivateField(hud, "resultPanel", resultPanel.gameObject);
        SetPrivateField(hud, "resultLabel", resultLabel);
        SetPrivateField(hud, "resultRewardLabel", resultReward);
    }

    private static TowerSelectionPopupUI BuildTowerPopup(Transform canvasTransform)
    {
        RectTransform root = FindOrCreateUI(canvasTransform, "TowerPopupRoot");
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        RectTransform backdrop = FindOrCreateUI(root, "Backdrop");
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.offsetMin = Vector2.zero;
        backdrop.offsetMax = Vector2.zero;
        Image backdropImg = backdrop.gameObject.GetComponent<Image>() ?? backdrop.gameObject.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.65f);
        Button backdropBtn = backdrop.gameObject.GetComponent<Button>() ?? backdrop.gameObject.AddComponent<Button>();

        RectTransform card = FindOrCreateUI(root, "Card");
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(480, 400);
        card.anchoredPosition = Vector2.zero;
        Image cardImg = card.gameObject.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
        cardImg.color = new Color(0.11f, 0.13f, 0.17f, 0.98f);
        VerticalLayoutGroup cardVlg = card.gameObject.GetComponent<VerticalLayoutGroup>() ?? card.gameObject.AddComponent<VerticalLayoutGroup>();
        cardVlg.padding = new RectOffset(20, 20, 16, 16);
        cardVlg.spacing = 10;
        cardVlg.childControlWidth = true;
        cardVlg.childControlHeight = false;
        cardVlg.childForceExpandWidth = true;
        cardVlg.childForceExpandHeight = false;

        RectTransform title = FindOrCreateUI(card, "Title");
        LayoutElement titleLE = title.gameObject.GetComponent<LayoutElement>() ?? title.gameObject.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 46;
        TMP_Text titleLabel = title.gameObject.GetComponent<TMP_Text>() ?? title.gameObject.AddComponent<TextMeshProUGUI>();
        titleLabel.text = "Choose a Tower";
        titleLabel.fontSize = 32;
        titleLabel.alignment = TextAlignmentOptions.Center;
        titleLabel.color = Color.white;

        RectTransform rowContainer = FindOrCreateUI(card, "RowContainer");
        LayoutElement rowContainerLE = rowContainer.gameObject.GetComponent<LayoutElement>() ?? rowContainer.gameObject.AddComponent<LayoutElement>();
        rowContainerLE.preferredHeight = 280;
        VerticalLayoutGroup rowVlg = rowContainer.gameObject.GetComponent<VerticalLayoutGroup>() ?? rowContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        rowVlg.spacing = 8;
        rowVlg.childControlWidth = true;
        rowVlg.childControlHeight = false;
        rowVlg.childForceExpandWidth = true;
        rowVlg.childForceExpandHeight = false;

        root.gameObject.SetActive(false);

        TowerOptionRowUI rowPrefab = EnsureTowerOptionRowPrefab();

        GameObject popupGO = FindOrCreate("TowerSelectionPopup");
        TowerSelectionPopupUI popup = popupGO.GetComponent<TowerSelectionPopupUI>() ?? popupGO.AddComponent<TowerSelectionPopupUI>();
        SetPrivateField(popup, "root", root.gameObject);
        SetPrivateField(popup, "backdropButton", backdropBtn);
        SetPrivateField(popup, "rowContainer", rowContainer);
        SetPrivateField(popup, "rowPrefab", rowPrefab);

        return popup;
    }

    private static TowerOptionRowUI EnsureTowerOptionRowPrefab()
    {
        string path = PrefabDir + "/TowerOptionRow.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing.GetComponent<TowerOptionRowUI>();

        GameObject go = new GameObject("TowerOptionRow", typeof(RectTransform));
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 64;
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.22f, 0.28f);
        HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(14, 14, 8, 8);
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        RectTransform rt = (RectTransform)go.transform;

        GameObject nameGO = new GameObject("NameLabel", typeof(RectTransform));
        nameGO.transform.SetParent(rt, false);
        LayoutElement nameLE = nameGO.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1;
        TMP_Text nameLabel = nameGO.AddComponent<TextMeshProUGUI>();
        nameLabel.text = "Tower";
        nameLabel.fontSize = 26;
        nameLabel.alignment = TextAlignmentOptions.Left;
        nameLabel.color = Color.white;

        GameObject costGO = new GameObject("CostLabel", typeof(RectTransform));
        costGO.transform.SetParent(rt, false);
        LayoutElement costLE = costGO.AddComponent<LayoutElement>();
        costLE.preferredWidth = 70;
        TMP_Text costLabel = costGO.AddComponent<TextMeshProUGUI>();
        costLabel.text = "0g";
        costLabel.fontSize = 24;
        costLabel.alignment = TextAlignmentOptions.Right;
        costLabel.color = new Color(1f, 0.85f, 0.3f);

        GameObject btnGO = new GameObject("SelectButton", typeof(RectTransform));
        btnGO.transform.SetParent(rt, false);
        LayoutElement btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.preferredWidth = 110;
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.55f, 0.3f);
        Button btn = btnGO.AddComponent<Button>();

        GameObject btnLabelGO = new GameObject("Label", typeof(RectTransform));
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        RectTransform btnLabelRT = (RectTransform)btnLabelGO.transform;
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;
        TMP_Text btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
        btnLabel.text = "Build";
        btnLabel.fontSize = 24;
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.color = Color.white;

        TowerOptionRowUI rowUI = go.AddComponent<TowerOptionRowUI>();
        SetPrivateField(rowUI, "nameLabel", nameLabel);
        SetPrivateField(rowUI, "costLabel", costLabel);
        SetPrivateField(rowUI, "selectButton", btn);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<TowerOptionRowUI>();
    }

    private static void EnsureAltarSlots()
    {
        Cainos.PixelArtTopDown_Basic.PropsAltar[] altars = Object.FindObjectsByType<Cainos.PixelArtTopDown_Basic.PropsAltar>(FindObjectsSortMode.None);
        foreach (Cainos.PixelArtTopDown_Basic.PropsAltar altar in altars)
        {
            if (altar.GetComponent<AltarTowerSlot>() == null)
                altar.gameObject.AddComponent<AltarTowerSlot>();
        }
    }

    private static readonly (string towerId, string faction, string building)[] TowerVisuals =
    {
        ("TowerArrow", "Blue Buildings", "Archery"),
        ("TowerCannon", "Blue Buildings", "Tower"),
        ("TowerRapid", "Blue Buildings", "Barracks"),
    };

    private static void ApplyTowerVisuals()
    {
        foreach ((string towerId, string faction, string building) in TowerVisuals)
        {
            string prefabPath = $"{PrefabDir}/{towerId}.prefab";
            string spritePath = $"Assets/Tiny Swords/Buildings/{faction}/{building}.png";

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Bastion: sprite not found at {spritePath} - skipping {towerId} visual swap.");
                continue;
            }

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(existingPrefab);

            SpriteRenderer rootSr = instance.GetComponent<SpriteRenderer>();
            if (rootSr != null) Object.DestroyImmediate(rootSr);

            Transform visual = instance.transform.Find("Visual");
            GameObject visualGO;
            if (visual != null)
            {
                visualGO = visual.gameObject;
            }
            else
            {
                visualGO = new GameObject("Visual");
                visualGO.transform.SetParent(instance.transform, false);
            }

            SpriteRenderer sr = visualGO.GetComponent<SpriteRenderer>();
            if (sr == null) sr = visualGO.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;

            // These sprites have their pivot at bottom-left, not center, so offset the
            // visual child by -bounds.center to make the sprite appear centered on the
            // prefab root (which stays at the altar's exact position for range/targeting).
            Bounds bounds = sprite.bounds;
            visualGO.transform.localPosition = new Vector3(-bounds.center.x, -bounds.center.y, 0f);

            instance.transform.localScale = Vector3.one * 0.5f;

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }
    }

    private static TMP_Text CreateLabel(Transform parent, string name, string text, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = FindOrCreateUI(parent, name);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TMP_Text tmp = rt.gameObject.GetComponent<TMP_Text>() ?? rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    private static RectTransform FindOrCreateUI(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing as RectTransform ?? existing.gameObject.AddComponent<RectTransform>();

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static GameObject FindOrCreate(string name, GameObject parent = null)
    {
        GameObject existing = parent == null ? GameObject.Find(name) : FindChild(parent.transform, name);
        if (existing != null) return existing;

        GameObject go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent.transform, false);
        return go;
    }

    private static GameObject FindOrCreate(string name, RectTransform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        return t != null ? t.gameObject : null;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogError($"Field '{fieldName}' not found on {target.GetType().Name}");
            return;
        }
        field.SetValue(target, value);
        EditorUtility.SetDirty((Object)target);
    }
}
