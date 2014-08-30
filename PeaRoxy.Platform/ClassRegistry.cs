// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassRegistry.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This class can be used to register and retrieve the platform dependent objects in a multi-platform environment so
    ///     you can keep your code same between different platforms. This is the base class for platform dependent libraries.
    /// </summary>
    public abstract class ClassRegistry
    {
        private static readonly Dictionary<string, object> Items = new Dictionary<string, object>();

        /// <summary>
        ///     This method can be used to retrieve a registered object
        /// </summary>
        /// <typeparam name="TType">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="TType" /> of the object you want to access.
        /// </returns>
        /// <exception cref="Exception">
        ///     The selected type is not valid or there is no registration for this type.
        /// </exception>
        public static TType GetClass<TType>()
        {
            if (!typeof(TType).IsSubclassOf(typeof(PlatformDependentClassBaseType)))
            {
                throw new Exception("Not supported class.");
            }

            if (Items.ContainsKey(typeof(TType).ToString()))
            {
                return (TType)Items[typeof(TType).ToString()];
            }

            throw new Exception("No active class registered.");
        }

        /// <summary>
        ///     This method can be used to register the corresponding classes of the current platform
        /// </summary>
        public abstract void RegisterPlatform();

        protected static void RegisterClass<TType>(TType pclass)
        {
            if (!typeof(TType).IsSubclassOf(typeof(PlatformDependentClassBaseType)))
            {
                throw new Exception("Not supported class.");
            }

            if (Items.ContainsKey(typeof(TType).ToString()))
            {
                Items[typeof(TType).ToString()] = pclass;
            }
            else
            {
                Items.Add(typeof(TType).ToString(), pclass);
            }
        }

        /// <summary>
        ///     An empty class that should be the base class of all platform-independent abstract classes.
        /// </summary>
        public class PlatformDependentClassBaseType
        {
        }
    }
}