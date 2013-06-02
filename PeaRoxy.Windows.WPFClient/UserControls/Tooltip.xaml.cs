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

namespace PeaRoxy.Windows.WPFClient.UserControls
{
    /// <summary>
    /// Interaction logic for Tooltip.xaml
    /// </summary>
    public partial class Tooltip : UserControl
    {
        public Tooltip(string title,string text)
        {
            InitializeComponent();
            Text = text;
            Title = title;
        }
        public Tooltip()
        {
            InitializeComponent();
        }
        public string Text
        {
            get
            {
                return lbl_data.Text;
            }
            set
            {
                lbl_data.Text = "";

                string[] textBoldParts = value.Replace("[N]", Environment.NewLine).Split(new char[] { '[' });
                foreach (string b in textBoldParts)
                {
                    string b_buggyMS = b;
                    int stEnd = b_buggyMS.IndexOf("]");
                    if (stEnd > -1)
                    {
                        lbl_data.Inlines.Add(new Bold(new Run(b_buggyMS.Substring(0, stEnd))));
                        b_buggyMS = b_buggyMS.Substring(stEnd + 1);
                    }
                    lbl_data.Inlines.Add(new Run(b_buggyMS));
                }
            }
        }
        public string Title
        {
            get
            {
                return (string)lbl_title.Content;
            }
            set
            {
                lbl_title.Content = value;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (lbl_data.Text == string.Empty && lbl_data.Inlines.Count == 0)
                text_co.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
