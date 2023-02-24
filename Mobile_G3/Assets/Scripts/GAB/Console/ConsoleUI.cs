using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private int maxLineCount = 10;
    private string myLog;
    private int lineCount;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A)) Debug.Log(lineCount + " : lineCount", this);
        if(Input.GetKeyDown(KeyCode.E)) Debug.LogError("Random Error", this);
        if(Input.GetKeyDown(KeyCode.W)) Debug.LogWarning("Random Warning", this);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;

    }

    public void Log(string logString, string stackTrace, LogType logType)
    {
        logString = logType switch
        {
            LogType.Error => "<color=red>" + logString + "</color>",
            LogType.Assert => "<color=white>" + logString + "</color>",
            LogType.Warning => "<color=yellow>" + logString + "</color>",
            LogType.Log => "<color=white>" + logString + "</color>",
            LogType.Exception => "<color=red>" + logString + "</color>",
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
        };

        myLog = myLog + "\n" + logString;
        lineCount++;

        if (lineCount > maxLineCount)
        {
            lineCount--;
            myLog = DeleteLines(myLog, 1);
        }

        text.text = myLog;
    }

    private string DeleteLines(string message, int linesCountToRemove)
    {
        return message.Split(Environment.NewLine.ToCharArray(), linesCountToRemove + 1).Skip(linesCountToRemove)
            .FirstOrDefault();
    }
}
