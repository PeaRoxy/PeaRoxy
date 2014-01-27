// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Base.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The base.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.ComponentModel;
    using System.Windows.Controls;

    #endregion

    /// <summary>
    ///     The base.
    /// </summary>
    public abstract class Base : UserControl, ISynchronizeInvoke
    {
        #region Fields

        /// <summary>
        ///     The is loading.
        /// </summary>
        private bool isLoading = true;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets a value indicating whether invoke required.
        /// </summary>
        public bool InvokeRequired
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether this control is in loading state
        /// </summary>
        protected bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The begin invoke.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="IAsyncResult"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This operation is not supported
        /// </exception>
        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The end invoke.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// This operation is not supported
        /// </exception>
        public object EndInvoke(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The invoke.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Invoke(Delegate method, object[] args)
        {
            return this.Dispatcher.Invoke(method, args);
        }

        /// <summary>
        ///     The load settings.
        /// </summary>
        public virtual void LoadSettings()
        {
        }

        /// <summary>
        ///     The save settings.
        /// </summary>
        public virtual void SaveSettings()
        {
        }

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public virtual void SetEnable(bool enable)
        {
            this.IsEnabled = enable;
        }

        #endregion
    }
}