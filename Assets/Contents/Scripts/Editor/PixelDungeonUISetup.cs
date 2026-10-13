using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class PixelDungeonUISetup
{
    const string Root = "Assets/Contents/PixelDungeonUI";
    const string ScenePath = "Assets/Scenes/DungeonScene.unity";
    const string FontSourcePath = Root + "/Fonts/Galmuri11.ttf";
    const string FontAssetPath = Root + "/Fonts/Galmuri11 TMP.asset";

    static readonly string[] ReferenceFields =
    {
        "playerText",
        "roomText",
        "messageText",
        "enterButton",
        "attackButton",
        "chestButton",
        "restButton",
        "nextButton",
        "returnButton"
    };

    public static void Run()
    {
        try
        {
            ConfigureTextures();
            TMP_FontAsset fontAsset = LoadOrCreateFont();
            Scene scene = EditorSceneManager.OpenScene(ScenePath);
            BuildScene(scene, fontAsset);
            VerifyClientReferences(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[PixelDungeonUISetup] DONE");
        }
        catch (Exception exception)
        {
            Debug.LogError("[PixelDungeonUISetup] FAILED\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    static void ConfigureTextures()
    {
        string dataPath = Application.dataPath.Replace("\\", "/");
        string[] files = Directory.GetFiles(Path.Combine(Application.dataPath, "Contents/PixelDungeonUI"), "*.png", SearchOption.AllDirectories);
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string file in files)
            {
                string fullPath = file.Replace("\\", "/");
                if (!fullPath.StartsWith(dataPath))
                    throw new InvalidOperationException("프로젝트 밖 텍스처입니다: " + fullPath);
                string assetPath = "Assets" + fullPath.Substring(dataPath.Length);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                    throw new InvalidOperationException("TextureImporter를 찾지 못했습니다: " + assetPath);

                float border = SpriteBorder(assetPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.spritePixelsPerUnit = 100;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMode = (int)SpriteImportMode.Single;
                settings.spritePixelsPerUnit = 100;
                settings.alphaIsTransparency = true;
                settings.filterMode = FilterMode.Point;
                settings.mipmapEnabled = false;
                if (border > 0f)
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.spriteBorder = new Vector4(border, border, border, border);
                importer.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    static float SpriteBorder(string assetPath)
    {
        string folder = Path.GetFileName(Path.GetDirectoryName(assetPath));
        string file = Path.GetFileNameWithoutExtension(assetPath);
        if (folder == "Buttons" && file.StartsWith("button_"))
            return 20f;
        if (folder != "Panels")
            return 0f;
        if (file == "panel_stone" || file == "panel_parchment" || file == "panel_wood")
            return 24f;
        if (file == "tab_title")
            return 16f;
        if (file == "bar_frame")
            return 12f;
        return 0f;
    }

    static TMP_FontAsset LoadOrCreateFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
            return existing;

        var font = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
        if (font == null)
            throw new InvalidOperationException("폰트 파일을 찾지 못했습니다: " + FontSourcePath);

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            font, 12, 2, GlyphRenderMode.RASTER_HINTED, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        if (fontAsset == null)
            throw new InvalidOperationException("TMP 폰트 에셋 생성에 실패했습니다.");

        Texture2D atlas = fontAsset.atlasTextures[0];
        atlas.filterMode = FilterMode.Point;
        atlas.name = "Galmuri11 Atlas";
        fontAsset.material.name = "Galmuri11 Material";

        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(atlas)))
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(fontAsset.material)))
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        return fontAsset;
    }

    static void BuildScene(Scene scene, TMP_FontAsset fontAsset)
    {
        Transform canvas = FindInScene(scene, "Canvas");
        if (canvas == null)
            throw new InvalidOperationException("Canvas를 찾지 못했습니다.");

        var scaler = GetOrAdd<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform background = EnsureChild(canvas, "Background");
        StretchFull(background);
        Image backgroundImage = GetOrAdd<Image>(background.gameObject);
        backgroundImage.sprite = LoadSprite(Root + "/Background/bg_dungeon.png");
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.color = Color.white;
        backgroundImage.raycastTarget = false;

        RectTransform playerPanel = CreatePanel(
            canvas, "PlayerPanel", Root + "/Panels/panel_stone.png", "PLAYER", fontAsset);
        AnchorTopLeft(playerPanel, new Vector2(40, -40), new Vector2(688, 280));
        PlaceCornerIcon(playerPanel, LoadSprite(Root + "/Icons/icon_player.png"), new Vector2(36, -56));
        PlaceBodyText(Require(scene, "PlayerText"), playerPanel, 116, 56, 32, 32);

        RectTransform roomPanel = CreatePanel(
            canvas, "RoomPanel", Root + "/Panels/panel_parchment.png", "ROOM", fontAsset);
        AnchorTopRight(roomPanel, new Vector2(-40, -40), new Vector2(640, 400));
        PlaceCornerIcon(roomPanel, LoadSprite(Root + "/Icons/icon_room.png"), new Vector2(36, -56));
        PlaceBodyText(Require(scene, "RoomText"), roomPanel, 116, 56, 32, 32);

        RectTransform messagePanel = EnsureChild(canvas, "MessagePanel");
        AnchorBottomStretch(messagePanel, 40, 40, 40, 160);
        Image messageImage = GetOrAdd<Image>(messagePanel.gameObject);
        messageImage.sprite = LoadSprite(Root + "/Panels/panel_wood.png");
        messageImage.type = Image.Type.Sliced;
        messageImage.color = Color.white;
        PlaceSideIcon(messagePanel, LoadSprite(Root + "/Icons/icon_message.png"), 40);
        RectTransform messageText = Require(scene, "MessageText");
        messageText.SetParent(messagePanel, false);
        messageText.localScale = Vector3.one;
        SetStretch(messageText, 128, 24, 40, 24);

        RectTransform buttonGrid = EnsureChild(canvas, "ButtonGrid");
        buttonGrid.anchorMin = buttonGrid.anchorMax = new Vector2(0.5f, 0.5f);
        buttonGrid.pivot = new Vector2(0.5f, 0.5f);
        buttonGrid.anchoredPosition = new Vector2(0, -120);
        buttonGrid.sizeDelta = new Vector2(744, 376);
        var grid = GetOrAdd<GridLayoutGroup>(buttonGrid.gameObject);
        grid.cellSize = new Vector2(344, 112);
        grid.spacing = new Vector2(56, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        Sprite normal = LoadSprite(Root + "/Buttons/button_normal.png");
        Sprite hover = LoadSprite(Root + "/Buttons/button_hover.png");
        Sprite pressed = LoadSprite(Root + "/Buttons/button_pressed.png");
        Sprite disabled = LoadSprite(Root + "/Buttons/button_disabled.png");
        SetupButton(scene, buttonGrid, "Button_Chest", normal, hover, pressed, disabled, "icon_chest", "상자 열기");
        SetupButton(scene, buttonGrid, "Button_Rest", normal, hover, pressed, disabled, "icon_rest", "휴식");
        SetupButton(scene, buttonGrid, "Button_Enter", normal, hover, pressed, disabled, "icon_enter", "입장");
        SetupButton(scene, buttonGrid, "Button_Attack", normal, hover, pressed, disabled, "icon_attack", "공격");
        SetupButton(scene, buttonGrid, "Button_Next", normal, hover, pressed, disabled, "icon_next", "다음 방");
        SetupButton(scene, buttonGrid, "Button_Return", normal, hover, pressed, disabled, "icon_return", "귀환");

        string[] buttonOrder =
        {
            "Button_Chest", "Button_Rest", "Button_Enter", "Button_Attack", "Button_Next", "Button_Return"
        };
        for (int i = 0; i < buttonOrder.Length; i++)
            buttonGrid.Find(buttonOrder[i]).SetSiblingIndex(i);

        ApplyCommonText(scene, fontAsset);
        StyleText(Require(scene, "PlayerText").GetComponent<TMP_Text>(), 36, "#E8E4F0", HorizontalAlignmentOptions.Left, VerticalAlignmentOptions.Top, 10);
        StyleText(Require(scene, "RoomText").GetComponent<TMP_Text>(), 32, "#3B3346", HorizontalAlignmentOptions.Left, VerticalAlignmentOptions.Top, 10);
        StyleText(Require(scene, "MessageText").GetComponent<TMP_Text>(), 36, "#FFF1B0", HorizontalAlignmentOptions.Left, VerticalAlignmentOptions.Middle, 0);

        background.SetSiblingIndex(0);
        playerPanel.SetSiblingIndex(1);
        roomPanel.SetSiblingIndex(2);
        buttonGrid.SetSiblingIndex(3);
        messagePanel.SetSiblingIndex(4);
    }

    static RectTransform CreatePanel(Transform canvas, string name, string spritePath, string title, TMP_FontAsset fontAsset)
    {
        RectTransform panel = EnsureChild(canvas, name);
        Image image = GetOrAdd<Image>(panel.gameObject);
        image.sprite = LoadSprite(spritePath);
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        RectTransform tab = EnsureChild(panel, "Tab");
        AnchorTopLeft(tab, new Vector2(32, 20), new Vector2(280, 44));
        Image tabImage = GetOrAdd<Image>(tab.gameObject);
        tabImage.sprite = LoadSprite(Root + "/Panels/tab_title.png");
        tabImage.type = Image.Type.Sliced;
        tabImage.color = Color.white;

        RectTransform labelRect = EnsureChild(tab, "Label");
        StretchFull(labelRect);
        TMP_Text label = GetOrAdd<TextMeshProUGUI>(labelRect.gameObject);
        label.text = title;
        label.font = fontAsset;
        label.fontSize = 28;
        label.enableAutoSizing = false;
        label.color = Html("#FFF1B0");
        label.horizontalAlignment = HorizontalAlignmentOptions.Center;
        label.verticalAlignment = VerticalAlignmentOptions.Middle;
        label.raycastTarget = false;
        RemoveShadow(label.gameObject);
        return panel;
    }

    static void SetupButton(
        Scene scene,
        RectTransform grid,
        string name,
        Sprite normal,
        Sprite hover,
        Sprite pressed,
        Sprite disabled,
        string iconName,
        string labelText)
    {
        RectTransform buttonRect = Require(scene, name);
        buttonRect.SetParent(grid, false);
        buttonRect.localScale = Vector3.one;

        Image image = buttonRect.GetComponent<Image>();
        if (image == null)
            throw new InvalidOperationException(name + "에 Image가 없습니다.");
        image.sprite = normal;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        Button button = buttonRect.GetComponent<Button>();
        if (button == null)
            throw new InvalidOperationException(name + "에 Button이 없습니다.");
        button.transition = Selectable.Transition.SpriteSwap;
        button.targetGraphic = image;
        SpriteState spriteState = button.spriteState;
        spriteState.highlightedSprite = hover;
        spriteState.pressedSprite = pressed;
        spriteState.selectedSprite = normal;
        spriteState.disabledSprite = disabled;
        button.spriteState = spriteState;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = Color.white;
        button.colors = colors;

        RectTransform icon = EnsureChild(buttonRect, "Icon");
        icon.anchorMin = icon.anchorMax = new Vector2(0, 0.5f);
        icon.pivot = new Vector2(0, 0.5f);
        icon.anchoredPosition = new Vector2(24, 0);
        icon.sizeDelta = new Vector2(64, 64);
        Image iconImage = GetOrAdd<Image>(icon.gameObject);
        iconImage.sprite = LoadSprite(Root + "/Icons/" + iconName + ".png");
        iconImage.type = Image.Type.Simple;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.color = Color.white;

        TMP_Text label = FindButtonLabel(buttonRect);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(104, 0);
        labelRect.offsetMax = new Vector2(-16, 0);
        label.text = labelText;
        label.fontSize = 32;
        label.enableAutoSizing = false;
        label.color = Html("#E8E4F0");
        label.horizontalAlignment = HorizontalAlignmentOptions.Left;
        label.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    static TMP_Text FindButtonLabel(RectTransform button)
    {
        TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.transform == button)
                continue;
            if (text.transform.name == "Icon" || text.transform.parent.name == "Icon")
                continue;
            return text;
        }

        throw new InvalidOperationException(button.name + "의 라벨 TMP를 찾지 못했습니다.");
    }

    static void ApplyCommonText(Scene scene, TMP_FontAsset fontAsset)
    {
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.scene != scene)
                continue;
            text.font = fontAsset;
            text.enableAutoSizing = false;
            RemoveShadow(text.gameObject);
        }
    }

    static void StyleText(
        TMP_Text text,
        float size,
        string color,
        HorizontalAlignmentOptions horizontal,
        VerticalAlignmentOptions vertical,
        float lineSpacing)
    {
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.color = Html(color);
        text.horizontalAlignment = horizontal;
        text.verticalAlignment = vertical;
        text.lineSpacing = lineSpacing;
        RemoveShadow(text.gameObject);
    }

    static void VerifyClientReferences(Scene scene)
    {
        Transform clientObject = FindInScene(scene, "GameClient");
        if (clientObject == null)
            throw new InvalidOperationException("GameClient를 찾지 못했습니다.");

        var client = clientObject.GetComponent<DungeonGameClient>();
        if (client == null)
            throw new InvalidOperationException("DungeonGameClient가 없습니다.");

        var serialized = new SerializedObject(client);
        int valid = 0;
        foreach (string field in ReferenceFields)
        {
            SerializedProperty property = serialized.FindProperty(field);
            UnityEngine.Object reference = property != null ? property.objectReferenceValue : null;
            if (reference == null)
            {
                Debug.LogError("[PixelDungeonUISetup] " + field + " = NULL");
                throw new InvalidOperationException("DungeonGameClient." + field + " 참조가 비어 있습니다.");
            }

            valid++;
            Debug.Log("[PixelDungeonUISetup] " + field + " = " + reference.name);
        }

        Debug.Log("[PixelDungeonUISetup] references OK (" + valid + "/9)");
    }

    static void PlaceBodyText(RectTransform text, RectTransform panel, float left, float top, float right, float bottom)
    {
        text.SetParent(panel, false);
        text.localScale = Vector3.one;
        SetStretch(text, left, top, right, bottom);
    }

    static void PlaceCornerIcon(RectTransform panel, Sprite sprite, Vector2 position)
    {
        RectTransform icon = EnsureChild(panel, "Icon");
        AnchorTopLeft(icon, position, new Vector2(64, 64));
        Image image = GetOrAdd<Image>(icon.gameObject);
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
    }

    static void PlaceSideIcon(RectTransform panel, Sprite sprite, float left)
    {
        RectTransform icon = EnsureChild(panel, "Icon");
        icon.anchorMin = icon.anchorMax = new Vector2(0, 0.5f);
        icon.pivot = new Vector2(0, 0.5f);
        icon.anchoredPosition = new Vector2(left, 0);
        icon.sizeDelta = new Vector2(64, 64);
        Image image = GetOrAdd<Image>(icon.gameObject);
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
    }

    static RectTransform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return (RectTransform)child;

        var created = new GameObject(name, typeof(RectTransform));
        var rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }

    static RectTransform Require(Scene scene, string name)
    {
        Transform found = FindInScene(scene, name);
        if (found == null)
            throw new InvalidOperationException(name + " 오브젝트를 찾지 못했습니다.");
        return (RectTransform)found;
    }

    static Transform FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindDeep(root.transform, name);
            if (found != null)
                return found;
        }

        return null;
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
            return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeep(root.GetChild(i), name);
            if (found != null)
                return found;
        }

        return null;
    }

    static void AnchorTopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void AnchorTopRight(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void AnchorBottomStretch(RectTransform rect, float left, float right, float bottom, float height)
    {
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2((left - right) * 0.5f, bottom);
        rect.sizeDelta = new Vector2(-(left + right), height);
    }

    static void StretchFull(RectTransform rect)
    {
        SetStretch(rect, 0, 0, 0, 0);
    }

    static void SetStretch(RectTransform rect, float left, float top, float right, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    static Sprite LoadSprite(string assetPath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
            throw new InvalidOperationException("스프라이트를 찾지 못했습니다: " + assetPath);
        return sprite;
    }

    static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    static Color Html(string hex)
    {
        if (!ColorUtility.TryParseHtmlString(hex, out Color color))
            throw new InvalidOperationException("색을 읽지 못했습니다: " + hex);
        return color;
    }

    static void RemoveShadow(GameObject gameObject)
    {
        Shadow shadow = gameObject.GetComponent<Shadow>();
        if (shadow != null)
            UnityEngine.Object.DestroyImmediate(shadow);
    }
}
