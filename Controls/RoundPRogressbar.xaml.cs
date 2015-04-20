using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace MSA_password_kiosk_software.Controls
{
    /// <summary>
    /// Interaction logic for RoundProgressbar.xaml
    /// </summary>
    public partial class RoundProgressbar : UserControl
    {
        private int MinValue = 0;
        private int MaxValue = 100;      

        //The current angle of the outter circle.
        private int _Value;
        public int Value
        {
            get { return _Value; }
            set
            {
                //Never exceed min/max boundaries
                if (value > MaxValue)
                    value = MaxValue;
                if (value < MinValue)
                    value = MinValue;

                //Animate the circle from the previous value to the new value
                AnimateCircleAngle(_Value, value); 
                _Value = value;
            }
        }

        private string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                statusLabel.Content = value;
                statusLabel.Margin = new Thickness(grid.ActualWidth / 2 - statusLabel.ActualWidth / 2, 0, 0, 0);
                _Status = value;
            }
        }

        public void AnimateCircleColor(Color NewColor)
        {
            SolidColorBrush OldColor = new SolidColorBrush(BrushToColor(ProgressGraphic.Stroke));
            ProgressGraphic.Stroke = OldColor;

            ColorAnimation ca = new ColorAnimation(OldColor.Color, NewColor, TimeSpan.FromMilliseconds(1000));
            
            SineEase ease = new SineEase();
            ca.EasingFunction = ease;

            OldColor.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        private Color BrushToColor(Brush brush)
        {
            SolidColorBrush newBrush = (SolidColorBrush)brush;
            return newBrush.Color;
        }

        //Function that animates the circle to a certain state
        private void AnimateCircleAngle(double PercentBefore, double PercentAfter)
        {
            //Make time relative based on how much we are animating
            double time = PercentAfter - PercentBefore;

            //We multiply the percentage by 3.6 because a full circle is 360 degrees, divide 360 by 100 and you get 3.6
            //So half a circle would be 50(%) * 3.6 = 180 degrees, half a circle. 
            DoubleAnimation da = new DoubleAnimation(PercentBefore * 3.6, PercentAfter * 3.6, TimeSpan.FromSeconds(time * 0.025));

            SineEase ease = new SineEase();
            da.EasingFunction = ease;

            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.EndAngleProperty, da);
        }

        //Function that animates the circle to a certain state with a certain timing
        public void AnimateCircleAngle(double PercentBefore, double PercentAfter, double seconds)
        {
            //We multiply the percentage by 3.6 because a full circle is 360 degrees, divide 360 by 100 and you get 3.6
            //So half a circle would be 50(%) * 3.6 = 180 degrees, half a circle. 
            DoubleAnimation da = new DoubleAnimation(PercentBefore * 3.6, PercentAfter * 3.6, TimeSpan.FromSeconds(seconds));

            SineEase ease = new SineEase();
            da.EasingFunction = ease;


            da.Completed += (sender, e) =>
            {
                AnimateCircleThickness(5, 15);
                Status = "Verwerken";
            };

            Status = "Een ogenblik geduld";
            AnimateLabelPeriods(statusLabel);
            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.EndAngleProperty, da);
        }

        private bool DotsAnimated = false;
        public async void AnimateLabelPeriods(Label label, int intervalInMilliseconds = 650)
        {
            DotsAnimated = true;
            while (DotsAnimated)
            {
                for (int i = 0; i < 4; i++)
                {
                    await Task.Delay(intervalInMilliseconds);
                    label.Content += ".";
                }
                label.Content = label.Content.ToString().Split('.')[0];
            }
        }

        private void StopLabelPeriodAnimation()
        {
            DotsAnimated = false;
        }

        public void AnimateCircleThickness(double widthBefore, double widthAfter, bool RepeatForever = true, bool AutoReverse = true)
        {
            DoubleAnimation da = new DoubleAnimation(widthBefore, widthAfter, TimeSpan.FromMilliseconds(700));
            da.AutoReverse = AutoReverse;

            if(RepeatForever)
                da.RepeatBehavior = RepeatBehavior.Forever;

            SineEase ease = new SineEase();
            da.EasingFunction = ease;

            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.StrokeThicknessProperty, da);
        }

        public void StopAnimations()
        {
            DoubleAnimation da = new DoubleAnimation(ProgressGraphic.StrokeThickness, ProgressGraphic.StrokeThickness + 2, TimeSpan.FromMilliseconds(700));
            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.StrokeThicknessProperty, da);
            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.StartAngleProperty, null);
            ProgressGraphic.BeginAnimation(Microsoft.Expression.Shapes.Arc.EndAngleProperty, null);
            ProgressGraphic.EndAngle = 360; // Keep it full

            AnimateCircleThickness(ProgressGraphic.StrokeThickness, ProgressGraphic.StrokeThickness + 5, false, false);
            FadeOutStatusLabel();
        }

        private void FadeControlToOpacity(UIElement element, double newOpacity)
        {
            DoubleAnimation da = new DoubleAnimation(element.Opacity, newOpacity, TimeSpan.FromMilliseconds(1000));
            element.BeginAnimation(UIElement.OpacityProperty, da);
        }

        public void FadeOutStatusLabel()
        {
            FadeControlToOpacity(statusLabel, 0);
        }

        public RoundProgressbar()
        {
            InitializeComponent();
            Value = 0;
        }

    }
}
