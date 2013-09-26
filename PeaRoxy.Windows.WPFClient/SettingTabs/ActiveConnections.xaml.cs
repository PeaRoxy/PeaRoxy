using PeaRoxy.ClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeaRoxy.Windows.WPFClient.SettingTabs
{
    /// <summary>
    /// Interaction logic for ActiveConnections.xaml
    /// </summary>
    public partial class ActiveConnections : Base
    {
        private Dictionary<string, System.Windows.Media.ImageSource> processIconCache = new Dictionary<string, System.Windows.Media.ImageSource>();
        BitmapImage defProcessIcon;
        ContextMenu connectionContextMenu;
        public ActiveConnections()
        {
            InitializeComponent();
            defProcessIcon = new BitmapImage(new Uri("pack://application:,,,/PeaRoxy.Windows.WPFClient;component/Images/SettingsPanel/Process_def.png"));
            connectionContextMenu = new ContextMenu();
            MenuItem cItem = new MenuItem();
            cItem.Header = "Close this connection";
            cItem.Click += (RoutedEventHandler)delegate(object sender, RoutedEventArgs e)
            {
                if (connectionContextMenu.PlacementTarget.GetType().Equals(typeof(TreeViewItem)))
                {
                    TreeViewItem sel = (TreeViewItem)connectionContextMenu.PlacementTarget;
                    if (sel.Tag != null && sel.Tag.GetType().Equals(typeof(Proxy_Client)))
                        ((Proxy_Client)sel.Tag).Close("Closed By User");
                }
            };
            connectionContextMenu.Items.Add(cItem);
            cItem = new MenuItem();
            cItem.Header = "Copy";
            cItem.Click += (RoutedEventHandler)delegate(object sender, RoutedEventArgs e)
            {
                if (connectionContextMenu.PlacementTarget.GetType().Equals(typeof(TreeViewItem)))
                {
                    try
                    {
                        TreeViewItem sel = (TreeViewItem)connectionContextMenu.PlacementTarget;
                        if (sel.Tag != null && sel.Tag.GetType().Equals(typeof(Proxy_Client)))
                            System.Windows.Clipboard.SetText(((Proxy_Client)sel.Tag).RequestAddress);
                    }
                    catch (Exception) { }
                }
            };
            connectionContextMenu.Items.Add(cItem);
        }

        public void UpdateConnections(Proxy_Controller Listener)
        {
            List<Proxy_Client> clients = Listener.GetConnectedClients();
            Dictionary<string, List<Proxy_Client>> processSeperatedConnections = new Dictionary<string, List<Proxy_Client>>();
            foreach (Proxy_Client client in clients)
            {
                Platform.ConnectionInfo conInfo = client.GetExtendedInfo();
                string processName = "(Unknown)";
                if (conInfo != null && conInfo.ProcessId > 0)
                {
                    processName = conInfo.ProcessId.ToString();
                    if (conInfo.ProcessString != string.Empty)
                    {
                        processName = "[PID: " + processName + "] " + conInfo.ProcessString;
                        if (!processIconCache.ContainsKey(processName))
                        {
                            try
                            {
                                using (System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(conInfo.ProcessId))
                                {
                                    if (p != null && p.MainModule.FileName != null && p.MainModule.FileName != string.Empty)
                                    {
                                        string fileName = p.MainModule.FileName;
                                        if (System.IO.File.Exists(fileName))
                                        {
                                            using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName))
                                            {
                                                processIconCache.Add(processName, System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                                                      icon.Handle,
                                                                      System.Windows.Int32Rect.Empty,
                                                                      System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()));
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    else
                        processName = "[PID: " + processName + "] (Unknown)";
                }
                if (!processSeperatedConnections.ContainsKey(processName))
                    processSeperatedConnections.Add(processName, new List<Proxy_Client>());
                processSeperatedConnections[processName].Add(client);
            }
            for (int i = 0; i < ConnectionsListView.Items.Count; i++)
            {
                TreeViewItem processNode = (TreeViewItem)ConnectionsListView.Items[i];
                if (processSeperatedConnections.ContainsKey((string)processNode.Header))
                {
                    List<Proxy_Client> processConnections = processSeperatedConnections[(string)processNode.Header];
                    ((UserControls.Tooltip)(processNode.ToolTip)).Text = "Connections: " + processConnections.Count.ToString();
                    for (int i2 = 0; i2 < processNode.Items.Count; i2++)
                    {
                        TreeViewItem connectionNode = (TreeViewItem)processNode.Items[i2];
                        if (processConnections.Contains(connectionNode.Tag))
                        {
                            connectionNode.Resources["Type"] = ((Proxy_Client)connectionNode.Tag).Type.ToString();
                            connectionNode.Resources["Status"] = ((Proxy_Client)connectionNode.Tag).Status.ToString();
                            connectionNode.Resources["Download"] = "D: " + CommonLibrary.Common.FormatFileSizeAsString(((Proxy_Client)connectionNode.Tag).AvgReceivingSpeed) + "/s";
                            connectionNode.Resources["Upload"] = "U: " + CommonLibrary.Common.FormatFileSizeAsString(((Proxy_Client)connectionNode.Tag).AvgSendingSpeed) + "/s";
                            connectionNode.Header = (((Proxy_Client)connectionNode.Tag).RequestAddress.Length > 40 ? ((Proxy_Client)connectionNode.Tag).RequestAddress.Substring(0, 40) + "..." : ((Proxy_Client)connectionNode.Tag).RequestAddress);
                            connectionNode.ToolTip = ((Proxy_Client)connectionNode.Tag).RequestAddress;
                            processConnections.Remove((Proxy_Client)connectionNode.Tag);
                        }
                        else
                        {
                            processNode.Items.RemoveAt(i2);
                            i2--;
                        }
                    }
                    for (int i2 = 0; i2 < processConnections.Count; i2++)
                    {
                        TreeViewItem connectionNode = new TreeViewItem();
                        connectionNode.Style = (Style)FindResource("ConnectionNode");
                        connectionNode.Tag = processConnections[i2];
                        connectionNode.ContextMenu = connectionContextMenu;
                        connectionNode.Resources.Add("Type", ((Proxy_Client)connectionNode.Tag).Type.ToString());
                        connectionNode.Resources.Add("Status", ((Proxy_Client)connectionNode.Tag).Status.ToString());
                        connectionNode.Resources.Add("Download", "D: " + CommonLibrary.Common.FormatFileSizeAsString(((Proxy_Client)connectionNode.Tag).AvgReceivingSpeed) + "/s");
                        connectionNode.Resources.Add("Upload", "U: " + CommonLibrary.Common.FormatFileSizeAsString(((Proxy_Client)connectionNode.Tag).AvgSendingSpeed) + "/s");
                        connectionNode.Header = (((Proxy_Client)connectionNode.Tag).RequestAddress.Length > 40 ? ((Proxy_Client)connectionNode.Tag).RequestAddress.Substring(0, 40) + "..." : ((Proxy_Client)connectionNode.Tag).RequestAddress);
                        connectionNode.ToolTip = ((Proxy_Client)connectionNode.Tag).RequestAddress;
                        processNode.Items.Add(connectionNode);
                    }
                    processSeperatedConnections.Remove((string)processNode.Header);
                }
                else
                {
                    ConnectionsListView.Items.RemoveAt(i);
                    i--;
                }
            }
            foreach (KeyValuePair<string, List<Proxy_Client>> processConnections in processSeperatedConnections)
            {
                TreeViewItem processNode = new TreeViewItem();
                processNode.Header = processConnections.Key;
                processNode.Style = (Style)FindResource("ProcessMainNode");
                processNode.IsExpanded = true;
                processNode.ToolTip = new UserControls.Tooltip(processConnections.Key, "Connections: " + processConnections.Value.Count.ToString());
                if (processIconCache.ContainsKey(processConnections.Key))
                    processNode.Resources.Add("Img", processIconCache[processConnections.Key]);
                else
                    processNode.Resources.Add("Img", defProcessIcon);
                ConnectionsListView.Items.Add(processNode);
            }
        }

        public override void SetEnable(bool enable)
        {
            return;
        }

    }
}
