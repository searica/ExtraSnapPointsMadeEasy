using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace Logging;

/// <summary>
///     Helper class for properly logging from static contexts.
/// </summary>
internal static class Log
{
    /// <summary>
    ///     Log level to control output to BepInEx log
    /// </summary>
    internal enum LogLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    #region Verbosity

    internal static ConfigEntry<LogLevel> Verbosity { get; set; }
    internal static LogLevel VerbosityLevel => Verbosity.Value;
    internal static bool IsVerbosityLow => Verbosity.Value >= LogLevel.Low;
    internal static bool IsVerbosityMedium => Verbosity.Value >= LogLevel.Medium;
    internal static bool IsVerbosityHigh => Verbosity.Value >= LogLevel.High;

    #endregion Verbosity

    private static ManualLogSource logSource;

    internal static void Init(ManualLogSource logSource)
    {
        Log.logSource = logSource;
    }

    internal static void LogDebug(object data) => logSource.LogDebug(data);

    internal static void LogError(object data) => logSource.LogError(data);

    internal static void LogFatal(object data) => logSource.LogFatal(data);

    internal static void LogMessage(object data) => logSource.LogMessage(data);

    internal static void LogWarning(object data) => logSource.LogWarning(data);

    internal static void LogInfo(object data, LogLevel level = LogLevel.Low)
    {
        if (Verbosity is null || VerbosityLevel >= level)
        {
            logSource.LogInfo(data);
        }
    }

    internal static void LogGameObject(GameObject prefab, bool includeChildren = false)
    {
        LogInfo("***** " + prefab.name + " *****");
        foreach (Component compo in prefab.GetComponents<Component>())
        {
            LogComponent(compo);
        }

        if (!includeChildren) { return; }

        LogInfo("***** " + prefab.name + " (children) *****");
        foreach (Transform child in prefab.transform)
        {
            if (!child) { continue; }

            LogInfo($" - {child.name}");
            foreach (Component compo in child.GetComponents<Component>())
            {
                LogComponent(compo);
            }
        }
    }

    internal static void LogComponent(Component compo)
    {
        if (!compo) { return; }
        try
        {
            LogInfo($"--- {compo.GetType().Name}: {compo.name} ---");
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
            LogWarning("Could not get type name for component!");
            return;
        }

        try
        {
            List<PropertyInfo> properties = AccessTools.GetDeclaredProperties(compo.GetType());
            foreach (var property in properties)
            {
                try
                {
                    LogInfo($" - {property.Name} = {property.GetValue(compo)}");
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                    LogWarning($"Could not get property: {property.Name} for component!");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
            LogWarning("Could not get properties for component!");
        }

        try
        {
            List<FieldInfo> fields = AccessTools.GetDeclaredFields(compo.GetType());
            foreach (var field in fields)
            {
                try
                {
                    LogInfo($" - {field.Name} = {field.GetValue(compo)}");
                }
                catch (Exception ex)
                {
                    LogError(ex.ToString());
                    LogWarning($"Could not get field: {field.Name} for component!");
                }
            }
        }
        catch (Exception ex)
        {
            LogError(ex.ToString());
            LogWarning("Could not get fields for component!");
        }

    }
}