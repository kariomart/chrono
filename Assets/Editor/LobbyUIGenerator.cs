using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class LobbyUIGenerator {

    [MenuItem("CHRONO/Generate Lobby UI")]
    static void Generate() {
        var mainMenu = Object.FindFirstObjectByType<MainMenu>(FindObjectsInactive.Include);
        if (mainMenu == null) {
            EditorUtility.DisplayDialog("CHRONO", "No MainMenu found in scene. Open the menu scene first.", "OK");
            return;
        }

        // Prevent duplicates
        var existing = GameObject.Find("LobbyUI");
        if (existing != null) {
            if (!EditorUtility.DisplayDialog("CHRONO", "LobbyUI already exists. Recreate it?", "Yes", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<EventSystem>() == null) {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // Find or create Canvas
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Root — left-side panel, full height, ~320px wide
        var root = MakePanel(canvas.transform, "LobbyUI", Color.clear);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0, 0);
        rootRt.anchorMax = new Vector2(0, 1);
        rootRt.pivot     = new Vector2(0, 0.5f);
        rootRt.sizeDelta = new Vector2(320, 0);
        rootRt.anchoredPosition = Vector2.zero;

        // ── OnlinePanel ──────────────────────────────────────────────────────
        var onlinePanel = MakePanel(root.transform, "OnlinePanel", new Color(0.05f, 0.05f, 0.05f, 0.93f));
        Fill(onlinePanel);

        MakeLabel(onlinePanel.transform, "Title", "PLAY ONLINE", 20, FontStyles.Bold,
            new Vector2(0, 0.82f), new Vector2(1, 1f));

        var hostBtn  = MakeButton(onlinePanel.transform, "HostButton",      "Host Game",
            new Vector2(0.08f, 0.63f), new Vector2(0.92f, 0.78f));
        var joinBtn  = MakeButton(onlinePanel.transform, "JoinButton",      "Join Game",
            new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.59f));
        var localBtn = MakeButton(onlinePanel.transform, "LocalPlayButton", "Local Play",
            new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.40f));

        var statusA = MakeLabel(onlinePanel.transform, "StatusText", "", 11, FontStyles.Normal,
            new Vector2(0.04f, 0.01f), new Vector2(0.96f, 0.22f));
        statusA.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);

        // ── HostWaitPanel ─────────────────────────────────────────────────────
        var hostWaitPanel = MakePanel(root.transform, "HostWaitPanel", new Color(0.05f, 0.05f, 0.05f, 0.93f));
        Fill(hostWaitPanel);
        hostWaitPanel.SetActive(false);

        MakeLabel(hostWaitPanel.transform, "Title", "HOSTING", 20, FontStyles.Bold,
            new Vector2(0, 0.87f), new Vector2(1, 1f));
        var hostP1Label = MakeLabel(hostWaitPanel.transform, "P1Label", "P1  ---", 13, FontStyles.Normal,
            new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.86f));
        var hostP2Label = MakeLabel(hostWaitPanel.transform, "P2Label", "P2  ---", 13, FontStyles.Normal,
            new Vector2(0.05f, 0.63f), new Vector2(0.95f, 0.74f));
        var lobbyIdDisplay = MakeLabel(hostWaitPanel.transform, "LobbyIdDisplay", "—", 8, FontStyles.Normal,
            new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.62f));
        lobbyIdDisplay.GetComponent<TextMeshProUGUI>().color = new Color(0.45f, 0.45f, 0.45f);
        var copyBtn          = MakeButton(hostWaitPanel.transform, "CopyButton",      "Copy ID",
            new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.53f));
        var inviteBtn        = MakeButton(hostWaitPanel.transform, "InviteButton",    "Invite Friends",
            new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.42f));
        var hostRefreshBtn   = MakeButton(hostWaitPanel.transform, "RefreshButton",   "Refresh",
            new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.31f));
        var startGameBtn     = MakeButton(hostWaitPanel.transform, "StartGameButton", "Start Game",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.21f));
        // Start Game is disabled until 2 players are present.
        startGameBtn.GetComponent<UnityEngine.UI.Button>().interactable = false;
        var hostBackBtn      = MakeButton(hostWaitPanel.transform, "BackButton",      "Back",
            new Vector2(0.08f, 0.01f), new Vector2(0.92f, 0.10f));

        // ── JoinPanel ─────────────────────────────────────────────────────────
        var joinPanel = MakePanel(root.transform, "JoinPanel", new Color(0.05f, 0.05f, 0.05f, 0.93f));
        Fill(joinPanel);
        joinPanel.SetActive(false);

        MakeLabel(joinPanel.transform, "Title", "JOIN GAME", 20, FontStyles.Bold,
            new Vector2(0, 0.82f), new Vector2(1, 1f));
        MakeLabel(joinPanel.transform, "InputLabel", "Enter Lobby ID:", 12, FontStyles.Normal,
            new Vector2(0.05f, 0.67f), new Vector2(0.95f, 0.77f));
        var inputField = MakeInputField(joinPanel.transform,
            new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.66f));
        var confirmBtn  = MakeButton(joinPanel.transform, "ConfirmButton", "Join",
            new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.49f));
        var joinBackBtn = MakeButton(joinPanel.transform, "BackButton", "Back",
            new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.31f));

        // ── JoinWaitPanel ─────────────────────────────────────────────────────
        var joinWaitPanel = MakePanel(root.transform, "JoinWaitPanel", new Color(0.05f, 0.05f, 0.05f, 0.93f));
        Fill(joinWaitPanel);
        joinWaitPanel.SetActive(false);

        MakeLabel(joinWaitPanel.transform, "Title", "JOINED", 20, FontStyles.Bold,
            new Vector2(0, 0.86f), new Vector2(1, 1f));
        var joinP1Label = MakeLabel(joinWaitPanel.transform, "P1Label", "P1  ---", 13, FontStyles.Normal,
            new Vector2(0.05f, 0.73f), new Vector2(0.95f, 0.84f));
        var joinP2Label = MakeLabel(joinWaitPanel.transform, "P2Label", "P2  ---", 13, FontStyles.Normal,
            new Vector2(0.05f, 0.61f), new Vector2(0.95f, 0.72f));
        var waitingLabel = MakeLabel(joinWaitPanel.transform, "WaitingLabel", "Waiting for host to start…",
            11, FontStyles.Italic, new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.60f));
        waitingLabel.GetComponent<TextMeshProUGUI>().color = new Color(0.65f, 0.65f, 0.65f);
        var joinRefreshBtn  = MakeButton(joinWaitPanel.transform, "RefreshButton", "Refresh",
            new Vector2(0.08f, 0.37f), new Vector2(0.92f, 0.47f));
        var joinWaitBackBtn = MakeButton(joinWaitPanel.transform, "BackButton", "Back",
            new Vector2(0.08f, 0.01f), new Vector2(0.92f, 0.11f));

        // ── Wire MainMenu inspector fields ────────────────────────────────────
        var so = new SerializedObject(mainMenu);
        so.FindProperty("onlinePanel")     .objectReferenceValue = onlinePanel;
        so.FindProperty("hostWaitPanel")   .objectReferenceValue = hostWaitPanel;
        so.FindProperty("joinPanel")       .objectReferenceValue = joinPanel;
        so.FindProperty("joinWaitPanel")   .objectReferenceValue = joinWaitPanel;
        so.FindProperty("joinCodeDisplay") .objectReferenceValue = lobbyIdDisplay.GetComponent<TextMeshProUGUI>();
        so.FindProperty("joinCodeInput")   .objectReferenceValue = inputField;
        so.FindProperty("statusText")      .objectReferenceValue = statusA.GetComponent<TextMeshProUGUI>();
        so.FindProperty("lobbyP1Text")     .objectReferenceValue = hostP1Label.GetComponent<TextMeshProUGUI>();
        so.FindProperty("lobbyP2Text")     .objectReferenceValue = hostP2Label.GetComponent<TextMeshProUGUI>();
        so.FindProperty("joinLobbyP1Text") .objectReferenceValue = joinP1Label.GetComponent<TextMeshProUGUI>();
        so.FindProperty("joinLobbyP2Text") .objectReferenceValue = joinP2Label.GetComponent<TextMeshProUGUI>();
        so.FindProperty("startGameButton") .objectReferenceValue = startGameBtn.GetComponent<UnityEngine.UI.Button>();
        so.ApplyModifiedProperties();

        // ── Wire button onClick events ────────────────────────────────────────
        UnityEventTools.AddPersistentListener(copyBtn         .GetComponent<Button>().onClick, mainMenu.OnCopyLobbyId);
        UnityEventTools.AddPersistentListener(inviteBtn       .GetComponent<Button>().onClick, mainMenu.OnInviteFriends);
        UnityEventTools.AddPersistentListener(hostRefreshBtn  .GetComponent<Button>().onClick, mainMenu.OnRefreshLobbyButton);
        UnityEventTools.AddPersistentListener(startGameBtn    .GetComponent<Button>().onClick, mainMenu.OnStartGameButton);
        UnityEventTools.AddPersistentListener(hostBackBtn     .GetComponent<Button>().onClick, mainMenu.OnBackToOnlinePanel);
        UnityEventTools.AddPersistentListener(joinBackBtn     .GetComponent<Button>().onClick, mainMenu.OnBackToOnlinePanel);
        UnityEventTools.AddPersistentListener(joinRefreshBtn  .GetComponent<Button>().onClick, mainMenu.OnRefreshLobbyButton);
        UnityEventTools.AddPersistentListener(joinWaitBackBtn .GetComponent<Button>().onClick, mainMenu.OnBackToOnlinePanel);
        UnityEventTools.AddPersistentListener(hostBtn         .GetComponent<Button>().onClick, mainMenu.OnHostButton);
        UnityEventTools.AddPersistentListener(joinBtn         .GetComponent<Button>().onClick, mainMenu.OnShowJoinPanel);
        UnityEventTools.AddPersistentListener(localBtn        .GetComponent<Button>().onClick, mainMenu.OnLocalPlayButton);
        UnityEventTools.AddPersistentListener(confirmBtn      .GetComponent<Button>().onClick, mainMenu.OnJoinButton);

        // ── Version label ─────────────────────────────────────────────────────
        // Sibling of LobbyUI so it's visible regardless of which panel is active.
        var verGo = new GameObject("VersionLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(verGo, "Create VersionLabel");
        verGo.transform.SetParent(canvas.transform, false);
        var verTmp = verGo.GetComponent<TextMeshProUGUI>();
        verTmp.text           = "v0.0.0";
        verTmp.fontSize       = 10;
        verTmp.color          = new Color(0.55f, 0.55f, 0.55f);
        verTmp.alignment      = TextAlignmentOptions.BottomRight;
        verTmp.raycastTarget  = false;
        var verRt = verGo.GetComponent<RectTransform>();
        verRt.anchorMin        = Vector2.zero;
        verRt.anchorMax        = Vector2.one;
        verRt.offsetMin        = new Vector2(8, 8);
        verRt.offsetMax        = new Vector2(-8, -8);
        verGo.AddComponent<VersionDisplay>();

        EditorUtility.SetDirty(mainMenu);
        Selection.activeGameObject = root;
        Debug.Log("[CHRONO] Lobby UI generated. Find it under Canvas/LobbyUI.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject MakePanel(Transform parent, string name, Color color) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static void Fill(GameObject go) {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeLabel(Transform parent, string name, string text,
                                 float size, FontStyles style,
                                 Vector2 anchorMin, Vector2 anchorMax) {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        Anchor(go, anchorMin, anchorMax);
        return go;
    }

    static GameObject MakeButton(Transform parent, string name, string label,
                                  Vector2 anchorMin, Vector2 anchorMax) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.18f, 0.18f, 0.18f);

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = new Color(0.18f, 0.18f, 0.18f);
        colors.highlightedColor = new Color(0.28f, 0.28f, 0.28f);
        colors.pressedColor     = new Color(0.10f, 0.10f, 0.10f);
        btn.colors = colors;

        Anchor(go, anchorMin, anchorMax);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        Anchor(labelGo, Vector2.zero, Vector2.one);

        return go;
    }

    static TMP_InputField MakeInputField(Transform parent,
                                          Vector2 anchorMin, Vector2 anchorMax) {
        var go = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        Undo.RegisterCreatedObjectUndo(go, "Create InputField");
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.13f, 0.13f, 0.13f);
        Anchor(go, anchorMin, anchorMax);

        // Text Area with mask
        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(go.transform, false);
        var areaRt = area.GetComponent<RectTransform>();
        areaRt.anchorMin = Vector2.zero;
        areaRt.anchorMax = Vector2.one;
        areaRt.offsetMin = new Vector2(8, 4);
        areaRt.offsetMax = new Vector2(-8, -4);

        // Placeholder
        var ph = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        ph.transform.SetParent(area.transform, false);
        Anchor(ph, Vector2.zero, Vector2.one);
        var phTmp = ph.GetComponent<TextMeshProUGUI>();
        phTmp.text      = "Enter lobby ID…";
        phTmp.fontSize  = 13;
        phTmp.color     = new Color(0.5f, 0.5f, 0.5f);
        phTmp.fontStyle = FontStyles.Italic;

        // Text
        var txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(area.transform, false);
        Anchor(txt, Vector2.zero, Vector2.one);
        var txtTmp = txt.GetComponent<TextMeshProUGUI>();
        txtTmp.fontSize = 13;
        txtTmp.color    = Color.white;

        var field = go.GetComponent<TMP_InputField>();
        field.textViewport = areaRt;
        field.textComponent = txtTmp;
        field.placeholder   = phTmp;

        return field;
    }

    static void Anchor(GameObject go, Vector2 min, Vector2 max) {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
