using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenLogger : MonoBehaviour {

    private string log;

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string ignoredString, LogType ignoredType) {
        log = logString;
    }

    void OnGUI() {
        GUI.color = Color.magenta;
        GUILayout.Label(log);
    }
}
