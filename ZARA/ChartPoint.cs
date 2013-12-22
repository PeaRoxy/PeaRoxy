// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartPoint.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   The chart point.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ZARA
{
    #region

    using System;

    #endregion

    /// <summary>
    ///     The chart point.
    /// </summary>
    public class ChartPoint
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartPoint"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public ChartPoint(double data)
        {
            this.Data = data;
            this.Time = new TimeSpan(Environment.TickCount);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public double Data { get; set; }

        /// <summary>
        ///     Gets or sets the time.
        /// </summary>
        public TimeSpan Time { get; set; }

        #endregion
    }
}