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
        public LoadingItem()
        {
            InitializeComponent();
        }

        public double Delay { get; set; }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard story = new Storyboard();
            story.BeginTime = TimeSpan.FromSeconds(Delay);
            PowerEase ease = new PowerEase();
            ease.Power = 0.3;
            ease.EasingMode = EasingMode.EaseInOut;
            story.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation dbOpacity = new DoubleAnimation();
            dbOpacity.AutoReverse = true;
            dbOpacity.EasingFunction = ease;
            dbOpacity.From = 0.0;
            dbOpacity.To = 1;
            dbOpacity.Duration = new Duration(TimeSpan.FromSeconds(0.8));

            ThicknessAnimation dbxPos = new ThicknessAnimation();
            dbxPos.EasingFunction = ease;
            dbxPos.AutoReverse = true;
            dbxPos.From = rect.Margin;
            dbxPos.To = new Thickness(this.ActualWidth, 0, 0, 0);
            dbxPos.Duration = new Duration(TimeSpan.FromSeconds(1.6));



            story.Children.Add(dbOpacity);
            story.Children.Add(dbxPos);
            Storyboard.SetTargetName(dbOpacity, rect.Name);
            Storyboard.SetTargetProperty(dbOpacity, new PropertyPath(MediaElement.OpacityProperty));
            Storyboard.SetTargetName(dbxPos, rect.Name);
            Storyboard.SetTargetProperty(dbxPos, new PropertyPath(MediaElement.MarginProperty));
            story.Begin(this);
        }
    }
}
