using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;

namespace ExtraSnapPointsMadeEasy.Utils;

internal class BepInExUtils
{
    private static Dictionary<PluginInfo, string> PluginInfoTypeNameCache { get; } = new Dictionary<PluginInfo, string>();
    private static Dictionary<Assembly, PluginInfo> AssemblyToPluginInfoCache { get; } = new Dictionary<Assembly, PluginInfo>();
    private static Dictionary<Type, PluginInfo> TypeToPluginInfoCache { get; } = new Dictionary<Type, PluginInfo>();

    /// <summary>
    ///     Get metadata information from the current calling mod
    /// </summary>
    /// <returns></returns>
    internal static BepInPlugin GetSourceModMetadata()
    {
        Type callingType = ReflectionUtils.GetCallingType();

        return GetPluginInfoFromType(callingType)?.Metadata ??
               GetPluginInfoFromAssembly(callingType.Assembly)?.Metadata ??
               ExtraSnapPointsMadeEasy.Instance.Info.Metadata;
    }

    /// <summary>
    ///     Get <see cref="PluginInfo"/> from a <see cref="Type"/>
    /// </summary>
    /// <param name="type"><see cref="Type"/> of the plugin main class</param>
    /// <returns></returns>
    private static PluginInfo GetPluginInfoFromType(Type type)
    {
        if (TypeToPluginInfoCache.TryGetValue(type, out PluginInfo pluginInfo))
        {
            return pluginInfo;
        }

        foreach (PluginInfo info in BepInEx.Bootstrap.Chainloader.PluginInfos.Values)
        {
            string typeName = ReflectionUtils.GetPrivateProperty<string>(info, "TypeName");
            if (typeName.Equals(type.FullName))
            {
                TypeToPluginInfoCache[type] = info;
                return info;
            }
        }

        return null;
    }

    /// <summary>
    ///     Get <see cref="PluginInfo"/> from an <see cref="Assembly"/>
    /// </summary>
    /// <param name="assembly"><see cref="Assembly"/> of the plugin</param>
    /// <returns></returns>
    private static PluginInfo GetPluginInfoFromAssembly(Assembly assembly)
    {
        if (AssemblyToPluginInfoCache.TryGetValue(assembly, out PluginInfo pluginInfo))
        {
            return pluginInfo;
        }

        foreach (PluginInfo info in BepInEx.Bootstrap.Chainloader.PluginInfos.Values)
        {
            if (assembly.GetType(GetPluginInfoTypeName(info)) != null)
            {
                AssemblyToPluginInfoCache[assembly] = info;
                return info;
            }
        }

        AssemblyToPluginInfoCache[assembly] = null;
        return null;
    }

    private static string GetPluginInfoTypeName(PluginInfo info)
    {
        if (PluginInfoTypeNameCache.TryGetValue(info, out string typeName))
        {
            return typeName;
        }

        typeName = ReflectionUtils.GetPrivateProperty<string>(info, "TypeName");
        PluginInfoTypeNameCache.Add(info, typeName);
        return typeName;
    }
}