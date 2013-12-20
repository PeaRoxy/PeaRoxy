// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassRegistry.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The class registry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Platform
{
    #region

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    ///     The class registry.
    /// </summary>
    public abstract class ClassRegistry
    {
        #region Static Fields

        /// <summary>
        ///     The _items.
        /// </summary>
        private static readonly Dictionary<string, object> Items = new Dictionary<string, object>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get class.
        /// </summary>
        /// <typeparam name="TType">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="TType" />.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public static TType GetClass<TType>()
        {
            // ReSharper disable once PossibleNullReferenceException
            if (typeof(TType).BaseType.ToString() != typeof(PlatformDependentClassBaseType).ToString())
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
        ///     The register platform.
        /// </summary>
        public abstract void RegisterPlatform();

        #endregion

        #region Methods

        /// <summary>
        /// The register class.
        /// </summary>
        /// <param name="pclass">
        /// The pclass.
        /// </param>
        /// <typeparam name="TType">
        /// </typeparam>
        /// <exception cref="Exception">
        /// </exception>
        protected static void RegisterClass<TType>(TType pclass)
        {
            // ReSharper disable once PossibleNullReferenceException
            if (typeof(TType).BaseType.ToString() != typeof(PlatformDependentClassBaseType).ToString())
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

        #endregion

        /// <summary>
        ///     The platform dependent class base type.
        /// </summary>
        public class PlatformDependentClassBaseType
        {
        }
    }
}