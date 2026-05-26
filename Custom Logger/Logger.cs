using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Logger", menuName = "Scriptable Objects/Logger")]
public class Logger : ScriptableObject
{
    [Serializable]
    public class LoggerData
    {
        public LogType logType;
        public Color logColor;
        
        public bool show;
        public bool showMessages;
        public bool showWarnings;
        public bool showErrors;
    }

    public bool showDebug;
    
    public LoggerData[] logDatas;
    
    private static Logger _instance;

    private static Logger Instance
    {
        get
        {
            if (_instance is null)
                _instance = Resources.Load<Logger>("Logger");
            
            return _instance;
        }
    }

    public static void PrintMessage(object obj, string message, LogType logType)
    {
        var data = Instance.logDatas[(int)logType];
        
        if (!Instance.showDebug || !data.show || !data.showMessages)
            return;
        
        var color = ColorUtility.ToHtmlStringRGB(data.logColor);
        Debug.Log($"<color=#{color}>{obj}:</color> {message}.");
    }
    
    public static void PrintWarning(object obj, string message, LogType logType)
    {
        var data = Instance.logDatas[(int)logType];
        
        if (!Instance.showDebug || !data.show || !data.showWarnings)
            return;
        
        var color = ColorUtility.ToHtmlStringRGB(data.logColor);
        Debug.LogWarning($"<color=#{color}>{obj}:</color> {message}.");
    }

    public static void PrintError(object obj, string message, LogType logType)
    {
        var data = Instance.logDatas[(int)logType];
        
        if (!Instance.showDebug || !data.show || !data.showErrors)
            return;
        
        var color = ColorUtility.ToHtmlStringRGB(data.logColor);
        Debug.LogError($"<color=#{color}>{obj}:</color> {message}.");
    }
}


public enum LogType
{
    Terrain,
    Player,
    Enemy
}