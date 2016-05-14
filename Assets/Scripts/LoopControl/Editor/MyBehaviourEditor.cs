using UnityEditor;
using UnityEngine;
using StateControl;

public class MyBehaviourEditor : EditorWindow
{
    //==================================================
    //  EDITORPREFS
    //==================================================

    bool logMessages
    {
        get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogMessages, false); }
        set { EditorPrefs.SetBool(MyStrings.menu_Logging_LogMessages, value); }
    }
    bool logWarnings
    {
        get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogWarnings, false); }
        set { EditorPrefs.SetBool(MyStrings.menu_Logging_LogWarnings, value); }
    }
    bool logErrors
    {
        get { return EditorPrefs.GetBool(MyStrings.menu_Logging_LogErrors, false); }
        set { EditorPrefs.SetBool(MyStrings.menu_Logging_LogErrors, value); }
    }

    //==================================================
    //  MENU ITEMS
    //==================================================

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/MyBehaviour")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        GetWindow(typeof(MyBehaviourEditor));
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }


    string myString = "Hello World";
    bool groupEnabled;

    float myFloat = 1.23f;

    public static bool Hello = false;

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Load Game"))
        {
            StateController.instance.LoadState();
        }
        if (GUILayout.Button("Save Game"))
        {
            StateController.instance.SaveState();
        }
        GUILayout.EndVertical();

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        logMessages = EditorGUILayout.Toggle("Log Messages", logMessages);
        logWarnings = EditorGUILayout.Toggle("Log Warnings", logWarnings);
        logErrors = EditorGUILayout.Toggle("Log Errors", logErrors);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();


    }
}
