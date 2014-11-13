using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WP8Nico.nomula
{
    public partial class ProgressSlider : UserControl
    {
        private double _value, _minimum, _maximum, _progressValue, _progressMaximum;
        public event MouseButtonEventHandler MouseLeftButtonDown, MouseLeftButtonUp;
        public event RoutedPropertyChangedEventHandler<double> ValueChanged, ProgressValueChanged;

        public ProgressSlider()
        {
            InitializeComponent();
        }

        public double Value
        {
            get
            {
                return slider1.Value;
            }
            set
            {
                _value = value;
                slider1.Value = _value;
            }
        }

        public double Minimum
        {
            get
            {
                return slider1.Minimum;
            }
            set
            {
                _minimum = value;
                slider1.Minimum = _minimum;
            }
        }

        public double Maximum
        {
            get
            {
                return slider1.Maximum;
            }
            set
            {
                _maximum = value;
                slider1.Maximum = _maximum;
            }
        }

        /// <summary>
        /// 現在の進行状況を取得・設定します。
        /// </summary>
        public double ProgressValue
        {
            get
            {
                return slider2.Value;
            }
            set
            {
                _progressValue = value;
                slider2.Value = _progressValue;
            }
        }

        public double ProgressMaximum
        {
            get
            {
                return slider2.Maximum;
            }
            set
            {
                _progressMaximum = value;
                slider2.Maximum = _progressMaximum;
            }
        }

        private void slider1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(sender, e);
        }

        protected virtual void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseLeftButtonDown != null)
                MouseLeftButtonDown(this, e);
        }

        private void slider1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonUp(sender, e);
        }

        protected virtual void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseLeftButtonUp != null)
                MouseLeftButtonUp(this, e);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnValueChanged(sender, e);
        }

        protected virtual void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        private void slider_ProgressValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnProgressValueChanged(sender, e);
        }

        protected virtual void OnProgressValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ProgressValueChanged != null)
                ProgressValueChanged(this, e);
        }
    }
}
