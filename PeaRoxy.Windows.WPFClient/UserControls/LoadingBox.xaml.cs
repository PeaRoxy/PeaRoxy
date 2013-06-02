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
    /// Interaction logic for LoadingBox.xaml
    /// </summary>
    public partial class LoadingBox : UserControl
    {
        public bool isVisible { get; private set; }
        public LoadingBox()
        {
            InitializeComponent();
        }
        public void Hide()
        {
            if (isVisible)
            {

            }
        }
    }
}
