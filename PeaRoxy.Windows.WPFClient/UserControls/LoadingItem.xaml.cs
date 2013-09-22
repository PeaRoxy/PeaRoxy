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
using System.Windows.Media.Animation;
namespace PeaRoxy.Windows.WPFClient.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingItem.xaml
    /// </summary>
    public partial class LoadingItem : UserControl
    {
        Storyboard story = new Storyboard() { RepeatBehavior = RepeatBehavior.Forever};
        public double Delay { get; set; }
        public bool IsPlaying
        {
            set
            {
                if (value)
                    story.Begin(rect, true);
                else
                {
                    story.Stop(rect);
                    rect.Opacity = 0;
                    rect.Margin = new Thickness(-5, 0, 0, 0);
                }
            }
        }

        public LoadingItem()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            story.BeginTime = TimeSpan.FromSeconds(Delay);
            PowerEase ease = new PowerEase() { Power = 0.3, EasingMode = EasingMode.EaseInOut};
            DoubleAnimation dbOpacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.8))) 
                                        { EasingFunction = ease, AutoReverse = true };
            ThicknessAnimation dbxPos = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(this.ActualWidth, 0, 0, 0), new Duration(TimeSpan.FromSeconds(1.6))) 
                                        { EasingFunction = ease, AutoReverse = true };
            story.Children.Add(dbOpacity);
            story.Children.Add(dbxPos);
            Storyboard.SetTarget(dbOpacity, rect);
            Storyboard.SetTarget(dbxPos, rect);
            Storyboard.SetTargetProperty(dbOpacity, new PropertyPath(MediaElement.OpacityProperty));
            Storyboard.SetTargetProperty(dbxPos, new PropertyPath(MediaElement.MarginProperty));
        }
    }
}
