using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OkMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32 ")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32 ")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public MainWindow()
        {
            InitializeComponent();
        }

        private TextBlock ContainerTB
        {
            get;
            set;
        }

        private TextBlock ContainerTBBtc
        {
            get;
            set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 30; i++)
            {
                var bd = new Border();
                bd.Width = 4;
                bd.Height = 0;
                bd.Background = new SolidColorBrush(Colors.White);
                bd.BorderBrush = null;
                bd.VerticalAlignment = VerticalAlignment.Bottom;
                bd.Opacity = 0.7;
                spTrade.Children.Add(bd);
            }
            ContainerTB = new TextBlock(); ;
            ContainerTB.Width = 0;

            ContainerTBBtc = new TextBlock(); ;
            ContainerTBBtc.Width = 0;
            var quoteTimer = new Timer(1000);
            var binding = new Binding
            {
                Source = ContainerTB,
                Path = new PropertyPath("Width"),
                Converter = new DoubleToStringConverter()
            };

            var bindingBtc = new Binding
            {
                Source = ContainerTBBtc,
                Path = new PropertyPath("Width"),
                Converter = new DoubleToStringConverter()
            };
            BindingOperations.SetBinding(tbEthPrice, TextBlock.TextProperty, binding);
            BindingOperations.SetBinding(tbBtcPrice, TextBlock.TextProperty, bindingBtc);
            quoteTimer.Elapsed += (qsender, qe) =>
            {
                quoteTimer.Stop();
                try
                {
                    var jss = new JavaScriptSerializer();
                    var client = new WebClient();
                    var ethPrice = 0.0;
                    var btcPrice = 0.0;
                    var progress = 0.5;
                    dynamic trades = null;
                    using (var data = client.OpenRead("https://www.okex.com/api/futures/v3/instruments/ETH-USDT-201225/trades?after=2517062044057601&limit=1"))
                    {
                        using (var reader = new StreamReader(data))
                        {
                            var s = reader.ReadToEnd();
                            var dict = jss.Deserialize<dynamic>(s);
                            ethPrice = double.Parse(dict[0]["price"]);
                        }
                    }
                    using (var data = client.OpenRead("https://www.okex.com/api/futures/v3/instruments/BTC-USDT-201225/trades?after=2517062044057601&limit=1"))
                    {
                        using (var reader = new StreamReader(data))
                        {
                            var s = reader.ReadToEnd();
                            var dict = jss.Deserialize<dynamic>(s);
                            btcPrice = double.Parse(dict[0]["price"]);
                        }
                    }

                    tbEthPrice.Dispatcher.Invoke(() =>
                    {
                        //Topmost = true;
                        Activate();

                        if (trades != null)
                        {
                            var j = 0;
                            var max = 0.0;
                            foreach (var i in trades)
                            {
                                if ((double)i[5] > max)
                                    max = (double)i[5];
                            }
                            if (max == 0)
                                max = 20;
                            foreach (var i in trades)
                            {
                                var bd = spTrade.Children[j] as Border;
                                if ((double)i[4] >= (double)i[1])
                                    (bd.Background as SolidColorBrush).Color = Colors.DodgerBlue;
                                else
                                    (bd.Background as SolidColorBrush).Color = Colors.Crimson;

                                bd.Height = (double)i[5] / max * 20;
                                j++;
                            }
                        }
                        var stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[2];
                        var powerAnim = new DoubleAnimation();
                        powerAnim.Duration = TimeSpan.FromMilliseconds(1000);
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[2].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[1];
                        powerAnim = new DoubleAnimation();
                        if (progress < 0.5)
                            powerAnim.Duration = TimeSpan.FromMilliseconds(100);
                        else
                            powerAnim.Duration = new TimeSpan(0, 0, 1);
                        powerAnim.To = 0.45;
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress - 0.05;
                        if (powerAnim.To < 0)
                            powerAnim.To = 0;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[3];
                        powerAnim = new DoubleAnimation();
                        if (progress > 0.5)
                            powerAnim.Duration = TimeSpan.FromMilliseconds(100);
                        else
                            powerAnim.Duration = new TimeSpan(0, 0, 1);
                        powerAnim.To = 0.55;
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress + 0.05;
                        if (powerAnim.To > 1)
                            powerAnim.To = 1;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[3].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        var borderAnim = new ColorAnimation();
                        if (ethPrice > ContainerTB.Width)
                            borderAnim.To = Color.FromArgb(255, 137, 249, 96);
                        else if (ethPrice < ContainerTB.Width)
                            borderAnim.To = Color.FromArgb(255, 255, 65, 65);
                        else
                            borderAnim.To = Colors.CadetBlue;
                        borderAnim.Duration = TimeSpan.FromMilliseconds(300);
                        (bdFlag.BorderBrush as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);
                        var priceAnim = new DoubleAnimation { To = ethPrice, Duration = new TimeSpan(0, 0, 1) };
                        ContainerTB.BeginAnimation(TextBlock.WidthProperty, priceAnim);
                    });
                    tbEthPrice.Dispatcher.Invoke(() =>
                    {
                        if (trades != null)
                        {
                            var j = 0;
                            var max = 0.0;
                            foreach (var i in trades)
                            {
                                if ((double)i[5] > max)
                                    max = (double)i[5];
                            }
                            if (max == 0)
                                max = 20;
                            foreach (var i in trades)
                            {
                                var bd = spTrade.Children[j] as Border;
                                if ((double)i[4] >= (double)i[1])
                                    (bd.Background as SolidColorBrush).Color = Colors.DodgerBlue;
                                else
                                    (bd.Background as SolidColorBrush).Color = Colors.Crimson;

                                bd.Height = (double)i[5] / max * 20;
                                j++;
                            }
                        }
                        var stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[2];
                        var powerAnim = new DoubleAnimation();
                        powerAnim.Duration = TimeSpan.FromMilliseconds(1000);
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[2].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[1];
                        powerAnim = new DoubleAnimation();
                        if (progress < 0.5)
                            powerAnim.Duration = TimeSpan.FromMilliseconds(100);
                        else
                            powerAnim.Duration = new TimeSpan(0, 0, 1);
                        powerAnim.To = 0.45;
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress - 0.05;
                        if (powerAnim.To < 0)
                            powerAnim.To = 0;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        stop = (bdAskPower.Background as LinearGradientBrush).GradientStops[3];
                        powerAnim = new DoubleAnimation();
                        if (progress > 0.5)
                            powerAnim.Duration = TimeSpan.FromMilliseconds(100);
                        else
                            powerAnim.Duration = new TimeSpan(0, 0, 1);
                        powerAnim.To = 0.55;
                        if (!double.IsNaN(progress))
                            powerAnim.To = progress + 0.05;
                        if (powerAnim.To > 1)
                            powerAnim.To = 1;
                        (bdAskPower.Background as LinearGradientBrush).GradientStops[3].BeginAnimation(GradientStop.OffsetProperty, powerAnim);

                        var borderAnim = new ColorAnimation();
                        if (btcPrice > ContainerTBBtc.Width)
                            borderAnim.To = Color.FromArgb(255, 137, 249, 96);
                        else if (btcPrice < ContainerTBBtc.Width)
                            borderAnim.To = Color.FromArgb(255, 255, 65, 65);
                        else
                            borderAnim.To = Colors.CadetBlue;
                        borderAnim.Duration = TimeSpan.FromMilliseconds(300);
                        (bdFlag.BorderBrush as SolidColorBrush).BeginAnimation(SolidColorBrush.ColorProperty, borderAnim);
                        var priceAnim = new DoubleAnimation { To = btcPrice, Duration = new TimeSpan(0, 0, 1) };
                        ContainerTBBtc.BeginAnimation(TextBlock.WidthProperty, priceAnim);
                    });
                }
                catch
                { }
                quoteTimer.Start();
            };
            quoteTimer.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double price = (double)value;
            return price.ToString("0.00");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}