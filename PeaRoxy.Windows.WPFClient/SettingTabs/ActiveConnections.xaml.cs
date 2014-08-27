// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveConnections.xaml.cs" company="PeaRoxy.com">
//   PeaRoxy by PeaRoxy.com is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License .
//   Permissions beyond the scope of this license may be requested by sending email to PeaRoxy's Dev Email .
// </copyright>
// <summary>
//   Interaction logic for ActiveConnections.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using PeaRoxy.ClientLibrary;
    using PeaRoxy.CommonLibrary;
    using PeaRoxy.Platform;
    using PeaRoxy.Windows.WPFClient.UserControls;

    #endregion

    /// <summary>
    ///     Interaction logic for ActiveConnections.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class ActiveConnections
    {
        #region Fields

        /// <summary>
        /// The connection context menu.
        /// </summary>
        private readonly ContextMenu connectionContextMenu;

        /// <summary>
        /// The default process icon.
        /// </summary>
        private readonly BitmapImage defaultProcessIcon;

        /// <summary>
        /// The process icon cache.
        /// </summary>
        private readonly Dictionary<string, ImageSource> processIconCache = new Dictionary<string, ImageSource>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveConnections"/> class.
        /// </summary>
        public ActiveConnections()
        {
            this.InitializeComponent();
            this.defaultProcessIcon =
                new BitmapImage(
                    new Uri(
                        "pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/SettingsPanel/Process_def.png"));
            this.connectionContextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem { Header = "Close this connection" };
            menuItem.Click += (RoutedEventHandler)delegate
                {
                    TreeViewItem placementTarget = this.connectionContextMenu.PlacementTarget as TreeViewItem;
                    if (placementTarget == null)
                    {
                        return;
                    }

                    try
                    {
                        ProxyClient client = placementTarget.Tag as ProxyClient;
                        if (client != null)
                        {
                            client.Close("Closed By User");
                        }
                    }
                    catch
                    {
                    }
                };

            this.connectionContextMenu.Items.Add(menuItem);
            menuItem = new MenuItem { Header = "Copy" };
            menuItem.Click += (RoutedEventHandler)delegate
                {
                    TreeViewItem placementTarget = this.connectionContextMenu.PlacementTarget as TreeViewItem;
                    if (placementTarget == null)
                    {
                        return;
                    }

                    try
                    {
                        ProxyClient client = placementTarget.Tag as ProxyClient;
                        if (client != null)
                        {
                            Clipboard.SetText(client.RequestAddress);
                        }
                    }
                    catch
                    {
                    }
                };

            this.connectionContextMenu.Items.Add(menuItem);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The set enable.
        /// </summary>
        /// <param name="enable">
        /// The enable.
        /// </param>
        public override void SetEnable(bool enable)
        {
        }

        /// <summary>
        /// The update connections.
        /// </summary>
        /// <param name="listener">
        /// The listener.
        /// </param>
        public void UpdateConnections(ProxyController listener)
        {
            IEnumerable<ProxyClient> clients = listener.GetConnectedClients();
            Dictionary<string, List<ProxyClient>> processSeperatedConnections =
                new Dictionary<string, List<ProxyClient>>();
            foreach (ProxyClient client in clients)
            {
                ConnectionInfo conInfo = client.GetExtendedInfo();
                string processName = "(Unknown)";
                if (conInfo != null && conInfo.ProcessId > 0)
                {
                    processName = conInfo.ProcessId.ToString(CultureInfo.InvariantCulture);
                    if (conInfo.ProcessName == string.Empty)
                    {
                        processName = "[PID: " + processName + "] (Unknown)";
                    }
                    else
                    {
                        processName = "[PID: " + processName + "] " + conInfo.ProcessName;
                        if (!this.processIconCache.ContainsKey(processName))
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(conInfo.ProcessPath))
                                {
                                    string fileName = conInfo.ProcessPath;
                                    if (File.Exists(fileName))
                                    {
                                        using (Icon icon = Icon.ExtractAssociatedIcon(fileName))
                                        {
                                            if (icon != null)
                                            {
                                                BitmapSource iconImage = Imaging.CreateBitmapSourceFromHIcon(
                                                    icon.Handle,
                                                    Int32Rect.Empty,
                                                    BitmapSizeOptions.FromWidthAndHeight(16, 16));
                                                this.processIconCache.Add(processName, iconImage);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                if (!processSeperatedConnections.ContainsKey(processName))
                {
                    processSeperatedConnections.Add(processName, new List<ProxyClient>());
                }

                processSeperatedConnections[processName].Add(client);
            }

            for (int i = 0; i < this.ConnectionsListView.Items.Count; i++)
            {
                TreeViewItem processNode = (TreeViewItem)this.ConnectionsListView.Items[i];
                if (processSeperatedConnections.ContainsKey((string)processNode.Header))
                {
                    List<ProxyClient> processConnections = processSeperatedConnections[(string)processNode.Header];
                    ((Tooltip)processNode.ToolTip).Text = "Connections: " + processConnections.Count;
                    for (int i2 = 0; i2 < processNode.Items.Count; i2++)
                    {
                        TreeViewItem connectionNode = (TreeViewItem)processNode.Items[i2];
                        ProxyClient connectedClient = connectionNode.Tag as ProxyClient;
                        if (connectedClient != null && processConnections.Contains(connectedClient))
                        {
                            connectionNode.Resources["Type"] = connectedClient.RequestType.ToString();
                            connectionNode.Resources["Status"] = connectedClient.Status.ToString();
                            connectionNode.Resources["Download"] = "D: "
                                                                   + Common.FormatFileSizeAsString(
                                                                       connectedClient.AverageReceivingSpeed) + "/s";
                            connectionNode.Resources["Upload"] = string.Format("U: {0}/s", Common.FormatFileSizeAsString(connectedClient.AverageSendingSpeed));
                            connectionNode.Header = connectedClient.RequestAddress.Length > 40
                                                        ? connectedClient.RequestAddress.Substring(
                                                            0, 
                                                            40) + "..."
                                                        : connectedClient.RequestAddress;
                            connectionNode.ToolTip = connectedClient.RequestAddress;
                            processConnections.Remove(connectedClient);
                        }
                        else
                        {
                            processNode.Items.RemoveAt(i2);
                            i2--;
                        }
                    }

                    foreach (ProxyClient connectedClient in processConnections)
                    {
                        TreeViewItem connectionNode = new TreeViewItem
                                                          {
                                                              Style =
                                                                  (Style)
                                                                  this.FindResource("ConnectionNode"),
                                                              Tag = connectedClient,
                                                              ContextMenu = this.connectionContextMenu
                                                          };
                        connectionNode.Resources.Add("Type", connectedClient.RequestType.ToString());
                        connectionNode.Resources.Add("Status", connectedClient.Status.ToString());
                        connectionNode.Resources.Add(
                            "Download", 
                            string.Format("D: {0}/s", Common.FormatFileSizeAsString(connectedClient.AverageReceivingSpeed)));
                        connectionNode.Resources.Add(
                            "Upload", 
                            string.Format("U: {0}/s", Common.FormatFileSizeAsString(connectedClient.AverageSendingSpeed)));
                        connectionNode.Header = connectedClient.RequestAddress.Length > 40
                                                    ? connectedClient.RequestAddress.Substring(0, 40)
                                                      + "..."
                                                    : connectedClient.RequestAddress;
                        connectionNode.ToolTip = connectedClient.RequestAddress;
                        processNode.Items.Add(connectionNode);
                    }

                    processSeperatedConnections.Remove((string)processNode.Header);
                }
                else
                {
                    this.ConnectionsListView.Items.RemoveAt(i);
                    i--;
                }
            }

            foreach (KeyValuePair<string, List<ProxyClient>> processConnections in processSeperatedConnections)
            {
                TreeViewItem processNode = new TreeViewItem
                                               {
                                                   Header = processConnections.Key,
                                                   Style = (Style)this.FindResource("ProcessMainNode"),
                                                   IsExpanded = true,
                                                   ToolTip =
                                                       new Tooltip(
                                                       processConnections.Key,
                                                       "Connections: " + processConnections.Value.Count)
                                               };
                processNode.Resources.Add(
                    "Image",
                    this.processIconCache.ContainsKey(processConnections.Key) ? this.processIconCache[processConnections.Key] : this.defaultProcessIcon);

                this.ConnectionsListView.Items.Add(processNode);
            }
        }

        #endregion
    }
}