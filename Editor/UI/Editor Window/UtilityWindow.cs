#region
using System;
using System.Linq;
using Lumina.Essentials.Editor.UI.Management;
using UnityEditor;
using UnityEngine;
using static Lumina.Essentials.Editor.UI.Management.EditorGUIUtils;
using static Lumina.Essentials.Editor.UI.Management.VersionManager;
#endregion

namespace Lumina.Essentials.Editor.UI
{
/// <summary>
///     The Utility Window that provides various features to enhance the user's workflow.
///     Includes the Setup Window, Settings Window, and Utilities Window.
///     This class is split into three partials, one for each panel.
/// </summary>
internal sealed partial class UtilityWindow : EditorWindow
{
    readonly static Vector2 winSize = new (370, 650);
    readonly static float buttonSize = winSize.x * 0.5f - 6;

    #region Panels
    /// <summary> The panel that will be displayed. </summary>
    int selectedTab;

    /// <summary> The labels of the tabs. </summary>
    readonly string[] tabLabels =
    { "Setup", "Settings", "Utilities" };

    /// <summary> Used to invoke the setup panel when necessary. (Not the main panel) </summary>
    Action currentPanel;
    #endregion

    [MenuItem("Tools/Lumina/Open Utility Panel")]
    internal static void OpenUtilityWindow()
    {
        // Get existing open window or if none, make a new one:
        var window = (UtilityWindow) GetWindow(typeof(UtilityWindow), true, "Lumina's Essentials Utility Panel");
        window.minSize = winSize;
        window.maxSize = window.minSize;
        window.Show();
    }

    void OnEnable()
    {
        // Initialize all the variables
        Initialization();

        Repaint();
        return;

        void Initialization()
        { 
            // Set the last open version to the current version
            LastOpenVersion = CurrentVersion;
            SafeMode        = true;

            // Initialize the installed modules
            CheckForInstalledModules(AvailableModules);

            // Set SetupRequired to true if there are no modules installed.
            SetupRequired = !InstalledModules.Values.Any(module => module);

            // Display the Toolbar.
            currentPanel = DisplayToolbar;

            // Displays the header and footer images.
            const string headerGUID = "7a1204763dac9b142b9cd974c88fdc8d";
            const string footerGUID = "22cbfe0e1e5aa9a46a9bd08709fdcac6";
            string       headerPath = AssetDatabase.GUIDToAssetPath(headerGUID);
            string       footerPath = AssetDatabase.GUIDToAssetPath(footerGUID);

            if (headerPath == null || footerPath == null) return;
            headerImg = AssetDatabase.LoadAssetAtPath<Texture2D>(headerPath);
            footerImg = AssetDatabase.LoadAssetAtPath<Texture2D>(footerPath);

            // If the user is not up-to-date, display a warning.
            if (!EditorPrefs.GetBool("UpToDate")) 
                EssentialsDebugger.LogWarning("There is a new version available!" + "\nPlease update to the latest version for the latest features.");
        }
    }

    /// <summary>
    ///     Displays the toolbar at the top of the window that toggles between the two panels.
    /// </summary>
    void OnGUI()
    {
        // Initialize GUIStyles
        SetGUIStyles();

        // If the user is in play mode, display a message telling them that the utility panel is not available while in play mode.
        if (EditorApplication.isPlaying)
        {
            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            GUILayout.Label("The Utility Panel \nis disabled while in play mode.", wrapCenterLabelStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(40);
            GUILayout.EndHorizontal();
            return;
        }

        // Don't show the toolbar if the user in the setup panel
        currentPanel();
    }

    /// <summary>
    ///     The toolbar at the top of the window with the tabs.
    /// </summary>
    void DisplayToolbar()
    {
        var areaRect = new Rect(0, 0, 370, 30);
        selectedTab = GUI.Toolbar(areaRect, selectedTab, tabLabels);

        GUILayout.Space(30);

        switch (selectedTab)
        {
            case 1:
                DrawSettingsGUI();
                break;

            case 2:
                DrawUtilitiesGUI();
                break;

            default:
                DrawSetupGUI();
                break;
        }
    }

    /// <summary>
    ///     Displays the main panel that shows the current version, latest version, etc.
    /// </summary>
    void DrawSetupGUI()
    {
        var areaRect = new Rect(0, 30, 370, 118);
        GUI.DrawTexture(areaRect, headerImg, ScaleMode.StretchToFill, false);
        GUILayout.Space(areaRect.y + 90);

        #region Main Labels (Version, Update Check, etc.)
        // Display current version in bold
        GUILayout.Label($"  Essentials Version: {CurrentVersion}", mainLabelStyle);

        // Display the latest version in bold
        GUILayout.Label($"  Latest Version: {LatestVersion}", mainLabelStyle);

        // Display the time since the last update check
        GUILayout.Label($"  Last Update Check: {VersionUpdater.LastUpdateCheck}", mainLabelStyle);

        // End of Main Labels
        #endregion
        GUILayout.Space(3);

        #region Setup Lumina Essentials Button
        GUILayout.Space(3);

        if (SetupRequired)
        {
            GUI.backgroundColor = Color.red;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("SETUP REQUIRED", setupLabelStyle);
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
        else { GUILayout.Space(8); }

        GUI.color = Color.green;

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("<b>Setup Essentials...</b>\n(add/remove Modules)", buttonSetup, GUILayout.Width(200)))

                // Select Setup Panel (not main panel)
                currentPanel = DrawModulesGUI;

            GUILayout.FlexibleSpace();
        }

        GUI.color = new (0.89f, 0.87f, 0.87f);

        GUI.backgroundColor = Color.white;
        GUILayout.Space(4);

        // End of Setup Lumina Essentials Button
        #endregion
        GUILayout.Space(3);

        #region Text Box (Description)
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.color = new (1f, 0.75f, 0.55f);
                if (GUILayout.Button("Something!", buttonSetup, GUILayout.Width(200))) EssentialsDebugger.Log("Placeholder button. Doesn't do anything yet.");

                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
            }

            GUILayout.Label
            ("Thank you for using Lumina's Essentials! \n"     + "This window will help you get started with Lumina's Essentials. \n" +
             "Please select the \"Setup\" tab to get started." + "\n"                                                                 + "" + "\n" +
             "Check out the \"Utilities\" tab to access the various workflow-enhancing features that this package provides.", wrapCenterLabelStyle);
        }
        #endregion
        GUILayout.Space(3);

        #region Grid of Buttons (Open Documentation, Open Changelog, etc.)
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button(openDocumentationContent, GUILayout.Width(buttonSize), GUILayout.Height(40))) Application.OpenURL("https://github.com/ltsLumina/Unity-Essentials");

            if (GUILayout.Button
                (openChangeLogContent, GUILayout.Width(buttonSize), GUILayout.Height(40))) Application.OpenURL("https://github.com/ltsLumina/Unity-Essentials/releases/latest");
        }

        using (new GUILayout.HorizontalScope())
        {
            // Display the button to check for updates
            if (GUILayout.Button(checkForUpdatesContent, GUILayout.Width(buttonSize), GUILayout.Height(40)))
            {
                VersionUpdater.CheckForUpdates();

                // if there is a new version available, open the GitHub repository's releases page
                if (!EditorPrefs.GetBool("UpToDate"))
                {
                    EssentialsDebugger.LogWarning("There is a new version available!" + "\nPlease update to the latest version to ensure functionality.");
                    Application.OpenURL("https://github.com/ltsLumina/Unity-Essentials/releases/latest");
                }
            }

            if (GUILayout.Button
                (openKnownIssuesContent, GUILayout.Width(buttonSize), GUILayout.Height(40))) Application.OpenURL("https://github.com/ltsLumina/Lumina-Essentials/issues");
        }
        #endregion
        GUILayout.Space(3);

        // Footer/Developed by Lumina
        if (GUILayout.Button(footerImg, btImgStyle)) Application.OpenURL("https://github.com/ltsLumina/");
    }

    /// <summary>
    ///     Displays the setup panel that allows the user to select which modules to install.
    /// </summary>
    void DrawModulesGUI()
    {
        DrawModulesHeader();
        DrawModulesInstallGUI();
        DrawModulesHelpBox();
        DrawModulesButtons();
    }

    /// <summary>
    ///     Displays the settings panel that allows the user to change various settings.
    /// </summary>
    void DrawSettingsGUI()
    {
        // Begin scroll view
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(370), GUILayout.Height(650));

        DrawSettingsHeader();
        DrawResetSettingsButton();
        DrawSettingsLabels();
        DrawAdvancedSettingsGUI();

        // End scroll view
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    ///     Displays the utilities panel that provides various features to enhance the user's workflow.
    /// </summary>
    void DrawUtilitiesGUI()
    {
        DrawUtilitiesHeader();
        DrawUtilitiesButtonsGUI();
        DrawConfigureImagesGUI();
    }
}
}
