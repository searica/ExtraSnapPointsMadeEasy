using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ExtraSnapPointsMadeEasy.Utils;

internal class ReflectionUtils
{
    public const BindingFlags AllBindings =
        BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.GetField
        | BindingFlags.SetField
        | BindingFlags.GetProperty
        | BindingFlags.SetProperty;

    /// <summary>
    ///     Get the <see cref="Type.ReflectedType"/> of the first caller outside of this assembly
    /// </summary>
    /// <returns>The reflected type of the first caller outside of this assembly</returns>
    internal static Type GetCallingType()
    {
        return (new StackTrace().GetFrames() ?? Array.Empty<StackFrame>())
            .First(x => x.GetMethod().ReflectedType?.Assembly != typeof(ExtraSnapPointsMadeEasy).Assembly)
            .GetMethod()
            .ReflectedType;
    }

    /// <summary>
    ///     Get the value of a private property of any class instance
    /// </summary>
    /// <typeparam name="T">Generic property type</typeparam>
    /// <param name="instance">Instance of the class</param>
    /// <param name="name">Name of the property</param>
    /// <returns>The value of the property</returns>
    internal static T GetPrivateProperty<T>(object instance, string name)
    {
        PropertyInfo var = instance.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);

        if (var == null)
        {
            Log.LogError("Property " + name + " does not exist on type: " + instance.GetType());
            return default(T);
        }

        return (T)var.GetValue(instance);
    }

    internal static MethodInfo GetMethod(Type type, string name, Type[] types)
    {
        foreach (MethodInfo method in type.GetMethods(AllBindings))
        {
            if (method.Name == name && HasMatchingParameterTypes(0, types, method.GetParameters()))
            {
                return method;
            }
        }
        return default;
    }

    internal static MethodInfo GetGenericMethod(Type type, string name, int genericParameterCount, Type[] types)
    {
        foreach (MethodInfo method in type.GetMethods(AllBindings))
        {
            if (method.IsGenericMethod
                && method.ContainsGenericParameters
                && method.Name == name
                && HasMatchingParameterTypes(genericParameterCount, types, method.GetParameters()))
            {
                return method;
            }
        }

        return default;
    }

    private static bool HasMatchingParameterTypes(int genericParameterCount, Type[] types, ParameterInfo[] parameters)
    {
        if (parameters.Length < genericParameterCount || parameters.Length != types.Length)
        {
            return false;
        }

        int count = 0;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType.IsGenericParameter)
            {
                count++;
            }
            else if (types[i] != parameters[i].ParameterType)
            {
                return false;
            }
        }

        if (count != genericParameterCount)
        {
            return false;
        }

        return true;
    }
}
