using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ExtraSnapPointsMadeEasy.Utils
{
    internal class ReflectionUtils
    {
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
    }
}