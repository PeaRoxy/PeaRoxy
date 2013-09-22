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
using System.Windows.Media.Animation;
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
        bool _isVisible = false;
        public LoadingBox()
        {
            InitializeComponent();
        }

        public new bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible != value)
                {
                    DoubleAnimation da_Loading = new DoubleAnimation((!value).GetHashCode(), value.GetHashCode(), new Duration(TimeSpan.FromSeconds(0.5)));
                    this.BeginAnimation(UIElement.OpacityProperty, da_Loading);
                    foreach (UIElement item in LoadingItemsGrid.Children)
                        if (item is LoadingItem)
                            ((LoadingItem)item).IsPlaying = value;
                    _isVisible = value;
                }
            }
        }
    }
}
