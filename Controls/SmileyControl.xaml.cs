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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSA_password_kiosk_software.Controls
{
    /// <summary>
    /// Interaction logic for SmileyControl.xaml
    /// </summary>
    public partial class SmileyControl : UserControl
    {
        private Color _FaceColor;

        public Color FaceColor
        {
            get { return _FaceColor; }
            set
            {
                _FaceColor = value; 
            }
        }
        

        private State _EmotionState;

        public State EmotionState
        {
            get { return _EmotionState; }
            set
            {
                displayEmotion(value);
                _EmotionState = value;
            }
        }

        public void displayEmotion(State state)
        {
            switch (state)
            {
                case State.Sad:
                    faceImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/sad.png"));
                    RoundProgressbar.FadeControlToOpacity(faceImage, 1);
                    break;
                case State.Neutral:
                    faceImage.Source = new BitmapImage(new Uri("Pack://application:,,,/Resources/neutral.png"));
                    FaceColor = Colors.Yellow;
                    RoundProgressbar.FadeControlToOpacity(faceImage, 1);
                    break;
                case State.Happy:
                    faceImage.Source = new BitmapImage(new Uri("Pack://application:,,,/Resources/happy.png"));
                    FaceColor = Colors.Green;
                    RoundProgressbar.FadeControlToOpacity(faceImage, 1);
                    break;
            }
        }

        public SmileyControl()
        {
            InitializeComponent();
        }
    }

    public enum State
    {
        Sad,
        Neutral,
        Happy
    }
}
