using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TSP_Transponder.Utilities
{
    class Animations
    {
        private static Dictionary<FrameworkElement, Storyboard> m_storyboards = new Dictionary<FrameworkElement, Storyboard>();

        // Custom Opacity Anim
        public static void AnimOpacity(FrameworkElement element, double start, double end, double duration = 1000, double delay = 0.0, Storyboard storyboard = null)
        {
            element.Visibility = Visibility.Visible;

            // Generate Thickness animation
            DoubleAnimation anim = new DoubleAnimation();

            // Declare durations
            anim.BeginTime = TimeSpan.FromMilliseconds(delay);
            anim.Duration = TimeSpan.FromMilliseconds(duration);

            // Set position targets
            anim.To = end;
            anim.From = start;

            // Set easing
            anim.EasingFunction = new CubicEase();

            Storyboard sb;
            if (storyboard == null)
            {
                if (m_storyboards.ContainsKey(element))
                {
                    sb = m_storyboards[element];
                    sb.Stop();
                }
                else
                {
                    sb = new Storyboard();
                }
            }
            else
            {
                sb = storyboard;
                sb.Stop();
            }

            // Set storyboards and animate!
            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, new PropertyPath(FrameworkElement.OpacityProperty));
            sb.Children.Add(anim);

            sb.Completed += (s, v) =>
            {
                element.Opacity = end;
                if (end == 0)
                {
                    element.Visibility = Visibility.Hidden;
                }
                else
                {
                    element.Visibility = Visibility.Visible;
                }

                sb.Stop();
                sb.Children.Clear();
            };

            sb.Begin();
        }

        // Custom Slide Animate
        public static void AnimMargin(FrameworkElement element, Thickness start, Thickness end, double duration = 1000, double delay = 0.0, Storyboard storyboard = null)
        {
            element.Visibility = Visibility.Visible;

            // Generate Thickness animation
            ThicknessAnimation anim = new ThicknessAnimation();

            // Declare durations
            anim.BeginTime = TimeSpan.FromMilliseconds(delay);
            anim.Duration = TimeSpan.FromMilliseconds(duration);

            // Set position targets
            anim.To = end;
            anim.From = start;

            // Set easing
            anim.EasingFunction = new CubicEase();

            Storyboard sb;
            if (storyboard == null)
            {
                if (m_storyboards.ContainsKey(element))
                {
                    sb = m_storyboards[element];
                    sb.Stop();
                }
                else
                {
                    sb = new Storyboard();
                }
            }
            else
            {
                sb = storyboard;
                sb.Stop();
            }

            // Set storyboards and animate!
            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, new PropertyPath(FrameworkElement.MarginProperty));
            sb.Children.Add(anim);

            sb.Completed += (s, v) =>
            {
                element.Margin = end;
                if (sb != null)
                {
                    sb.Stop();
                    if (storyboard != null)
                    {
                        sb = null;
                    }
                }
            };

            sb.Begin();
        }
        
        // Custom Translate Animate
        public static void AnimTranslate(FrameworkElement element, Point? start, Point end, double duration = 1000, EasingMode easing = EasingMode.EaseInOut, double delay = 0.0, Storyboard storyboard = null)
        {
            Storyboard sb;
            if (storyboard == null)
            {
                if (m_storyboards.ContainsKey(element))
                {
                    sb = m_storyboards[element];
                    sb.Stop();
                }
                else
                {
                    sb = new Storyboard();
                }
            }
            else
            {
                sb = storyboard;
                sb.Stop();
            }

            Point compStart = new Point();
            Point compEnd = end;
            element.Visibility = Visibility.Visible;
            if (start == null)
            {
                TranslateTransform existingTransform = (element.RenderTransform as TranslateTransform);
                compStart = new Point(existingTransform.X, existingTransform.Y);
            }
            else
            {
                compStart = ((Point)start);
            }

            element.RenderTransform = new TranslateTransform(compStart.X, compStart.Y);

            // Set easing
            BackEase ssAnimEasing = new BackEase();
            ssAnimEasing.Amplitude = 0.2;
            ssAnimEasing.EasingMode = easing;

            //Translate X
            DoubleAnimation translateXAnimation = new DoubleAnimation() { From = compStart.X, To = compEnd.X, AutoReverse = false };
            translateXAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            translateXAnimation.BeginTime = TimeSpan.FromMilliseconds(delay);
            translateXAnimation.EasingFunction = ssAnimEasing;
            Storyboard.SetTarget(translateXAnimation, element);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));

            //Translate Y
            DoubleAnimation translateYAnimation = new DoubleAnimation() { From = compStart.X, To = compEnd.Y, AutoReverse = false };
            translateYAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            translateYAnimation.BeginTime = TimeSpan.FromMilliseconds(delay);
            translateYAnimation.EasingFunction = ssAnimEasing;
            Storyboard.SetTarget(translateYAnimation, element);
            Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));

            // Set storyboards and animate!
            sb.Children.Add(translateXAnimation);
            sb.Children.Add(translateYAnimation);

            sb.Completed += (s, v) =>
            {
                element.RenderTransform = new TranslateTransform(compEnd.X, compEnd.Y);

                if (sb != null)
                {
                    sb.Stop();
                    if (storyboard != null)
                    {
                        sb = null;
                    }
                }
            };

            sb.Begin();
        }
        
    }
}
