using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.IO;

public class BuildWindow : EditorWindow
{
    bool buildMac = true;
    bool buildWindows = true;
    string version = "0.1.0";
    bool autoIncrement = true;

    bool showSteam;
    string steamUser = "";
    string steamCmdPath = "/opt/homebrew/bin/steamcmd";
    string appVdfPath = "";

    const string PrefSteamUser = "CHRONO_SteamUser";
    const string PrefSteamCmd  = "CHRONO_SteamCmd";
    const string PrefAppVdf    = "CHRONO_AppVdf";

    [MenuItem("CHRONO/Build …")]
    static void Open()
    {
        var w = GetWindow<BuildWindow>(false, "CHRONO Build", true);
        w.minSize = new Vector2(400, 300);
        w.Show();
    }

    void OnEnable()
    {
        version       = PlayerSettings.bundleVersion;
        if (string.IsNullOrEmpty(version)) version = "0.1.0";
        steamUser     = EditorPrefs.GetString(PrefSteamUser, steamUser);
        steamCmdPath  = EditorPrefs.GetString(PrefSteamCmd,  steamCmdPath);
        appVdfPath    = EditorPrefs.GetString(PrefAppVdf,
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "steam", "app_build_987820.vdf")));
    }

    void OnGUI()
    {
        GUILayout.Label("Build CHRONO", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        // ── Version ───────────────────────────────────────────────────────────
        EditorGUILayout.LabelField("Version", EditorStyles.miniLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            version = EditorGUILayout.TextField(version);
            if (GUILayout.Button("+patch", GUILayout.Width(60)))
                IncrementPatch();
        }
        autoIncrement = EditorGUILayout.Toggle("Auto-increment patch on build", autoIncrement);

        EditorGUILayout.Space(10);

        // ── Platforms ─────────────────────────────────────────────────────────
        EditorGUILayout.LabelField("Platforms", EditorStyles.miniLabel);
        buildMac     = EditorGUILayout.Toggle("macOS",   buildMac);
        buildWindows = EditorGUILayout.Toggle("Windows", buildWindows);

        EditorGUILayout.Space(8);

        bool noPlatform = !buildMac && !buildWindows;
        if (noPlatform)
            EditorGUILayout.HelpBox("Select at least one platform.", MessageType.Warning);

        using (new EditorGUI.DisabledScope(noPlatform))
        {
            if (GUILayout.Button("Build", GUILayout.Height(36)))
                DoBuild();
        }

        if (GUILayout.Button("Open Builds Folder"))
            RevealBuilds();

        EditorGUILayout.Space(10);

        // ── Steam ─────────────────────────────────────────────────────────────
        showSteam = EditorGUILayout.Foldout(showSteam, "Steam Upload", true);
        if (!showSteam) return;

        EditorGUI.BeginChangeCheck();
        steamUser    = EditorGUILayout.TextField("Steam username", steamUser);
        steamCmdPath = EditorGUILayout.TextField("steamcmd path",  steamCmdPath);
        appVdfPath   = EditorGUILayout.TextField("app_build.vdf",  appVdfPath);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetString(PrefSteamUser, steamUser);
            EditorPrefs.SetString(PrefSteamCmd,  steamCmdPath);
            EditorPrefs.SetString(PrefAppVdf,    appVdfPath);
        }

        bool hasUser = !string.IsNullOrEmpty(steamUser) && !string.IsNullOrEmpty(steamCmdPath);
        bool canPush = hasUser && !string.IsNullOrEmpty(appVdfPath);

        EditorGUILayout.Space(4);

        // First-time login to cache credentials
        EditorGUILayout.LabelField("First time? Cache your credentials:", EditorStyles.miniLabel);
        using (new EditorGUI.DisabledScope(!hasUser))
        {
            if (GUILayout.Button("Login to Steam (opens Terminal)"))
                OpenSteamLogin();
        }

        EditorGUILayout.Space(4);

        using (new EditorGUI.DisabledScope(!canPush))
        {
            if (GUILayout.Button("Push to Steam Beta", GUILayout.Height(32)))
                PushToSteam();
        }
    }

    void DoBuild()
    {
        PlayerSettings.bundleVersion = version;
        string[] scenes  = GetEnabledScenes();

        CleanMissingPrefabs(scenes);

        string buildRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
        bool allOk = true;

        if (buildMac)
            allOk &= RunBuild(BuildTarget.StandaloneOSX,
                Path.Combine(buildRoot, "Mac", "CHRONO_MAC.app"), scenes);

        if (buildWindows)
            allOk &= RunBuild(BuildTarget.StandaloneWindows64,
                Path.Combine(buildRoot, "Windows", "CHRONO_WINDOWS.exe"), scenes);

        if (allOk && autoIncrement)
            IncrementPatch();
    }

    bool RunBuild(BuildTarget target, string outPath, string[] scenes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        var report = BuildPipeline.BuildPlayer(scenes, outPath, target, BuildOptions.None);
        bool ok = report.summary.result == BuildResult.Succeeded;
        if (ok)
            UnityEngine.Debug.Log($"[CHRONO] {target} built → {outPath}");
        else
            EditorUtility.DisplayDialog("Build Failed",
                $"{target} build failed — check the Console for errors.", "OK");
        return ok;
    }

    void IncrementPatch()
    {
        var parts = version.Split('.');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int patch))
        {
            parts[2] = (patch + 1).ToString();
            version  = string.Join(".", parts);
            PlayerSettings.bundleVersion = version;
            UnityEngine.Debug.Log($"[CHRONO] Version → {version}");
        }
    }

    void RevealBuilds()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
        Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    void OpenSteamLogin()
    {
        // Interactive login — handles password prompt + Steam Guard / 2FA.
        // Once this completes successfully, credentials are cached for PushToSteam.
        OpenCommandScript("chrono_steam_login.command",
            $"\"{steamCmdPath}\" +login {steamUser} +quit");
    }

    void PushToSteam()
    {
        OpenCommandScript("chrono_steam_upload.command",
            $"\"{steamCmdPath}\" +login {steamUser} +run_app_build \"{appVdfPath}\" +quit");
    }

    void OpenCommandScript(string filename, string cmd)
    {
        string scriptPath = Path.Combine(Path.GetTempPath(), filename);
        File.WriteAllText(scriptPath,
            $"#!/bin/bash\n{cmd}\necho\nread -p 'Done — press Enter to close.'\n");
        Process.Start("chmod", $"+x \"{scriptPath}\"").WaitForExit();
        Process.Start("open", scriptPath);
    }

    // Removes any missing-prefab placeholder objects from every scene before building.
    // Also exposed as a menu item for manual cleanup.
    [MenuItem("CHRONO/Fix Missing Prefabs in Build Scenes")]
    static void CleanMissingPrefabsMenu() => CleanMissingPrefabs(GetEnabledScenes());

    static void CleanMissingPrefabs(string[] scenePaths)
    {
        // Save the currently open scene so we can restore it after iterating
        string originalScene = SceneManager.GetActiveScene().path;

        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int removed = 0;

            // Collect roots first; destroying while iterating is unsafe
            var toDestroy = new System.Collections.Generic.List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                CollectMissingPrefabs(root, toDestroy);

            foreach (var go in toDestroy)
            {
                UnityEngine.Debug.Log($"[CHRONO] Removing missing prefab '{go.name}' from {path}");
                Object.DestroyImmediate(go);
                removed++;
            }

            if (removed > 0)
                EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
    }

    static void CollectMissingPrefabs(GameObject go, System.Collections.Generic.List<GameObject> results)
    {
        if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.MissingAsset)
        {
            results.Add(go);
            return; // children belong to the same missing root; deleting the root removes them
        }
        foreach (Transform child in go.transform)
            CollectMissingPrefabs(child.gameObject, results);
    }

    static string[] GetEnabledScenes()
    {
        var list = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list.ToArray();
    }
}
