#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;

public static class CoinDropSetup
{
    static readonly int[]   ZonePoints = { 100, 200, 500, 1000, 500, 200, 100 };
    static readonly Color[] ZoneColors =
    {
        new Color(0.30f, 0.50f, 1.00f),
        new Color(0.30f, 0.80f, 0.30f),
        new Color(1.00f, 0.80f, 0.20f),
        new Color(1.00f, 0.25f, 0.25f),
        new Color(1.00f, 0.80f, 0.20f),
        new Color(0.30f, 0.80f, 0.30f),
        new Color(0.30f, 0.50f, 1.00f),
    };

    [MenuItem("CoinDrop/Setup Scene")]
    public static void SetupScene()
    {
        CleanupPrevious();
        EnsureFolders();
        SetupCamera();
        SetupLighting();
        SetupBloom();
        CreateBoundaries();
        var coinPrefab = CreateCoinPrefab();
        CreatePegs();
        CreateScoreZones();
        CreateManagers(coinPrefab);
        CreateAudio();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[CoinDrop] Setup complete! Press Play to test.");
    }

    // ── cleanup ──────────────────────────────────────────────────────────

    static void CleanupPrevious()
    {
        string[] names = {
            "Main Camera", "Wall_Left", "Wall_Right", "Wall_Bottom",
            "Pegs", "ScoreZones", "GameManager", "CoinDropper",
            "Canvas", "EventSystem", "Fill Light", "BGM", "GameSFX"
        };
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) Object.DestroyImmediate(go);
        }
    }

    // ── folders ──────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
    }

    // ── camera ───────────────────────────────────────────────────────────

    static void SetupCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        cam.orthographic      = true;
        cam.orthographicSize  = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.clearFlags        = CameraClearFlags.SolidColor;
        cam.backgroundColor   = new Color(0.06f, 0.06f, 0.14f);
    }

    // ── lighting ─────────────────────────────────────────────────────────

    static void SetupLighting()
    {
        // Warm main directional light
        var dirGO = GameObject.Find("Directional Light");
        if (dirGO == null) { dirGO = new GameObject("Directional Light"); dirGO.AddComponent<Light>(); }
        var dir = dirGO.GetComponent<Light>();
        dir.type      = LightType.Directional;
        dir.color     = new Color(1.0f, 0.93f, 0.72f);
        dir.intensity = 1.4f;
        dir.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        dir.shadows   = LightShadows.Soft;

        // Cool fill light
        var fillGO    = new GameObject("Fill Light");
        var fill      = fillGO.AddComponent<Light>();
        fill.type     = LightType.Directional;
        fill.color    = new Color(0.38f, 0.48f, 1.0f);
        fill.intensity = 0.45f;
        fill.transform.rotation = Quaternion.Euler(-20f, 150f, 0f);
        fill.shadows  = LightShadows.None;

        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.08f, 0.18f);
    }

    // ── bloom ────────────────────────────────────────────────────────────

    static void SetupBloom()
    {
        var volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null) return;
        var profile = volume.sharedProfile;
        if (profile == null) return;

        if (!profile.TryGet<Bloom>(out var bloom))
            bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.Override(2.2f);
        bloom.threshold.Override(0.75f);
        bloom.scatter.Override(0.65f);
    }

    // ── boundaries ───────────────────────────────────────────────────────

    static void CreateBoundaries()
    {
        var mat = MakeMaterial("Wall", new Color(0.35f, 0.35f, 0.42f));
        SpawnWall("Wall_Left",   new Vector3(-4.7f,  0f,   0f), new Vector3(0.4f, 14f,  1f), mat);
        SpawnWall("Wall_Right",  new Vector3( 4.7f,  0f,   0f), new Vector3(0.4f, 14f,  1f), mat);
        SpawnWall("Wall_Bottom", new Vector3( 0f,  -6.2f,  0f), new Vector3(9.4f,  0.4f, 1f), mat);
    }

    static void SpawnWall(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        var rb = go.GetComponent<Rigidbody>();
        if (rb) Object.DestroyImmediate(rb);
    }

    // ── pegs ─────────────────────────────────────────────────────────────

    static void CreatePegs()
    {
        var parent  = new GameObject("Pegs");
        var mat     = MakeMaterial("Peg", new Color(0.75f, 0.80f, 1.0f));
        // Subtle peg emission so they glow a little
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.3f));

        for (int row = 0; row < 6; row++)
        {
            bool even   = (row % 2 == 0);
            int  count  = even ? 7 : 6;
            float startX = even ? -3f : -2.5f;
            float y      = 3f - row * 1.1f;
            for (int i = 0; i < count; i++)
            {
                var peg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                peg.name = $"Peg_{row}_{i}";
                peg.transform.SetParent(parent.transform);
                peg.transform.position   = new Vector3(startX + i, y, 0f);
                peg.transform.localScale = Vector3.one * 0.28f;
                peg.GetComponent<Renderer>().sharedMaterial = mat;
                var rb = peg.GetComponent<Rigidbody>();
                if (rb) Object.DestroyImmediate(rb);
            }
        }
    }

    // ── score zones ──────────────────────────────────────────────────────

    static void CreateScoreZones()
    {
        var parent   = new GameObject("ScoreZones");
        int   count  = ZonePoints.Length;
        float width  = 8f / count;
        float startX = -4f + width * 0.5f;
        float y      = -5.5f;

        for (int i = 0; i < count; i++)
        {
            var zoneGO = new GameObject($"Zone_{ZonePoints[i]}");
            zoneGO.transform.SetParent(parent.transform);
            zoneGO.transform.position = new Vector3(startX + i * width, y, 0f);

            var col      = zoneGO.AddComponent<BoxCollider>();
            col.size     = new Vector3(width - 0.05f, 0.8f, 2f);
            col.isTrigger = true;

            var sz    = zoneGO.AddComponent<ScoreZone>();
            sz.Points = ZonePoints[i];

            var visMat = MakeMaterial($"Zone_{i}", ZoneColors[i]);
            visMat.EnableKeyword("_EMISSION");
            visMat.SetColor("_EmissionColor", ZoneColors[i] * 0.5f);

            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "Visual";
            vis.transform.SetParent(zoneGO.transform);
            vis.transform.localPosition = Vector3.zero;
            vis.transform.localScale    = new Vector3(width - 0.05f, 0.8f, 0.5f);
            vis.GetComponent<Renderer>().sharedMaterial = visMat;
            Object.DestroyImmediate(vis.GetComponent<BoxCollider>());

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(zoneGO.transform);
            labelGO.transform.localPosition = new Vector3(0f, 0f, -0.3f);
            var tm           = labelGO.AddComponent<TextMesh>();
            tm.text          = ZonePoints[i].ToString();
            tm.fontSize      = 28;
            tm.characterSize = 0.09f;
            tm.alignment     = TextAlignment.Center;
            tm.anchor        = TextAnchor.MiddleCenter;
            tm.color         = Color.white;
        }
    }

    // ── coin prefab ──────────────────────────────────────────────────────

    static GameObject CreateCoinPrefab()
    {
        var coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Coin";
        coin.transform.localScale = Vector3.one * 0.35f;
        var mat = MakeMaterial("Coin", new Color(1f, 0.85f, 0.1f));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.1f) * 1.5f);
        coin.GetComponent<Renderer>().sharedMaterial = mat;

        var rb = coin.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY;

        coin.AddComponent<Coin>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(coin, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(coin);
        return prefab;
    }

    // ── audio ────────────────────────────────────────────────────────────

    static void CreateAudio()
    {
        var bgmGO = new GameObject("BGM");
        bgmGO.AddComponent<AudioSource>();
        bgmGO.AddComponent<ProceduralBGM>();

        var sfxGO = new GameObject("GameSFX");
        sfxGO.AddComponent<GameSFX>();
    }

    // ── managers & UI ────────────────────────────────────────────────────

    static void CreateManagers(GameObject coinPrefab)
    {
        var gmGO = new GameObject("GameManager");
        var gm   = gmGO.AddComponent<GameManager>();

        var dropperGO = new GameObject("CoinDropper");
        var dropper   = dropperGO.AddComponent<CoinDropper>();

        var (scoreText, coinText, gameOverPanel, finalScoreText, retryButton) = CreateUI();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("scoreText").objectReferenceValue      = scoreText;
        gmSO.FindProperty("coinCountText").objectReferenceValue  = coinText;
        gmSO.FindProperty("gameOverPanel").objectReferenceValue  = gameOverPanel;
        gmSO.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
        gmSO.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(retryButton.onClick, gm.RestartGame);

        var dropperSO = new SerializedObject(dropper);
        dropperSO.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
        dropperSO.ApplyModifiedProperties();
    }

    static (TextMeshProUGUI score, TextMeshProUGUI coins,
            GameObject panel, TextMeshProUGUI finalScore, Button retry) CreateUI()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // Score（左上・黄色・アウトライン太め）
        var scoreText = MakeUIText(canvasGO.transform, "ScoreText", "Score: 0",
            new Color(1f, 0.95f, 0.2f),
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20));

        // Coins（右上・シアン）
        var coinText = MakeUIText(canvasGO.transform, "CoinCountText", "Coins: 10",
            new Color(0.3f, 1f, 0.95f),
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20));

        var (gameOverPanel, finalScoreText, retryButton) = MakeGameOverPanel(canvasGO.transform);
        return (scoreText, coinText, gameOverPanel, finalScoreText, retryButton);
    }

    static (GameObject panel, TextMeshProUGUI finalScore, Button retry) MakeGameOverPanel(Transform parent)
    {
        var panelGO    = new GameObject("GameOverPanel");
        panelGO.transform.SetParent(parent, false);
        var panelImg   = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);
        var panelRect  = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        MakeLabel(panelGO.transform, "GameOverTitle", "GAME OVER",
            new Color(1f, 0.85f, 0.1f), 110, new Vector2(0f, 130f), new Vector2(900f, 140f));

        var finalScore = MakeLabel(panelGO.transform, "FinalScoreText", "Score: 0",
            Color.white, 64, new Vector2(0f, 0f), new Vector2(600f, 90f));

        var (panel2, btn) = MakeRetryButton(panelGO.transform);

        panelGO.SetActive(false);
        return (panelGO, finalScore, btn);
    }

    static (GameObject go, Button btn) MakeRetryButton(Transform parent)
    {
        var go      = new GameObject("RetryButton");
        go.transform.SetParent(parent, false);
        var img     = go.AddComponent<Image>();
        img.color   = new Color(0.15f, 0.55f, 1f);
        var btn     = go.AddComponent<Button>();
        var rect    = go.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.pivot            = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -130f);
        rect.sizeDelta        = new Vector2(320f, 90f);

        var labelGO  = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp      = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text     = "RETRY";
        tmp.fontSize = 52;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color    = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var lr = labelGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;

        return (go, btn);
    }

    static TextMeshProUGUI MakeLabel(Transform parent, string name, string text,
        Color color, float fontSize, Vector2 pos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = Color.black;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0.5f);
        rect.anchorMax        = new Vector2(0.5f, 0.5f);
        rect.pivot            = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta        = size;
        return tmp;
    }

    static TextMeshProUGUI MakeUIText(Transform parent, string name, string text,
        Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text         = text;
        tmp.fontSize     = 46;
        tmp.fontStyle    = FontStyles.Bold;
        tmp.color        = color;
        tmp.outlineWidth = 0.3f;
        tmp.outlineColor = Color.black;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin        = anchorMin;
        rect.anchorMax        = anchorMax;
        rect.pivot            = pivot;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta        = new Vector2(320f, 65f);
        return tmp;
    }

    // ── material helper ──────────────────────────────────────────────────

    static Material MakeMaterial(string assetName, Color color)
    {
        var path = $"Assets/Materials/{assetName}.mat";
        var mat  = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        return mat;
    }
}
#endif
