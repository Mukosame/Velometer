using Velometer.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.ApplicationModel;
using Windows.UI.Popups;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Velometer
{
    public sealed partial class PivotPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
       // private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        List<double> xv = new List<double>();
        List<double> yv = new List<double>();
        List<double> zv = new List<double>();
        //double[] yv; double[] zv;
        List<double> v = new List<double>();
        List<double> gv = new List<double>();
       // double[] v; double[] gv;
        List<double> Latitude = new List<double>();
        List<double> Longitude = new List<double>();
        //double[] Latitude; double[] Longitude;
        //计数变量
        int an = 0;
        int gn = 0;
        Accelerometer accelerometer;
        AccelerometerReading accelerometerReading = null;

        Geolocator gpswatcher = new Geolocator()
        {
            //移动阈值
            MovementThreshold = 20
        };

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (!e.Handled && Frame.CurrentSourcePageType.FullName == "Velometer.PivotPage")
                Application.Current.Exit();
        }

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }


        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>


        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>


        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region accelerometer
        private async void Get_Acce_Data(object sender, RoutedEventArgs e)
        {
            accelerometer = Accelerometer.GetDefault();
            if (accelerometer == null)
            {
                await new MessageDialog("您的手机不支持加速度传感器").ShowAsync();
                return;
            }
            else
            {
                xv.Add(0); yv.Add(0); zv.Add(0); v.Add(0);
                //最小时间间隔
                accelerometer.ReportInterval = accelerometer.MinimumReportInterval;
                //加速度变化
                accelerometer.ReadingChanged += accelerometer_ReadingChanged;
                //手机晃动
                accelerometer.Shaken += accelerometer_Shaken;
                //读取加速度数据
                accelerometerReading = accelerometer.GetCurrentReading();
                //显示数据
                AcceShowData();
            }
        }

        private async void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    accelerometerReading = args.Reading;
                    //AcceShowData();
                });
        }

        void accelerometer_Shaken(Accelerometer sender, AccelerometerShakenEventArgs args)
        {
            throw new NotImplementedException();
        }

        void AcceShowData()
        {
            //读取数据
            double xa = accelerometerReading.AccelerationX;
            double ya = accelerometerReading.AccelerationY;
            double za = accelerometerReading.AccelerationZ;
            //处理，计算数据
            double dt = accelerometer.MinimumReportInterval;
            if (ya != 0)
            {
                an++;
                v.Add(v[an - 1] + dt * accelerometerReading.AccelerationY);
                // xv.Add(xv[an - 1] + dt * xa);
                //yv.Add(yv[an - 1] + dt * xa);
                //zv.Add(zv[an - 1] + dt * xa);
                //v.Add(vector_add(xv[an], yv[an], zv[an]));
                speed.Text = v[an].ToString("0.00");
            }
            //显示加速传感器得到的速度数值
            // xspeed.Text = "x: " + xv[an].ToString("0.00");
            //yspeed.Text = "y: " + yv[an].ToString("0.00");
            //zspeed.Text = "z: " + zv[an].ToString("0.00");
            else
            { }
            xspeed.Text = accelerometerReading.AccelerationX.ToString("0.00");
            yspeed.Text = accelerometerReading.AccelerationY.ToString("0.00");
            zspeed.Text = accelerometerReading.AccelerationZ.ToString("0.00");
        }

        #endregion

        #region gps
        private async void Get_GPS_Data(object sender, RoutedEventArgs e)
        {
            if (gpswatcher == null)
            {
                await new MessageDialog("您的设备不支持GPS传感器").ShowAsync();
                return;
            }
            else
            {
                gv.Add(0);
                Latitude.Add(0); Longitude.Add(0);
                //精度：高
                gpswatcher.DesiredAccuracy = PositionAccuracy.High;
                //位置变化
                gpswatcher.PositionChanged += gpswatcherPositionChanged;
                //传感器状态变化
                gpswatcher.StatusChanged += gpswatcherStatusChanged;
                ShowGPSData();
            }
        }

        async void gpswatcherPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ShowGPSData();
            });
        }

        private async void gpswatcherStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case PositionStatus.Disabled:
                    //location is unsupported on this device
                    await new MessageDialog("请手动开启定位设置").ShowAsync();
                    break;
                case PositionStatus.NoData:
                    // data unavailable
                    await new MessageDialog("无数据").ShowAsync();
                    break;
                case PositionStatus.NotAvailable:
                    await new MessageDialog("您的设备不支持GPS装置").ShowAsync();
                    break;
                default:
                    break;
            }
        }

        async void ShowGPSData()
        {
            try
            {
                Geoposition pos = await gpswatcher.GetGeopositionAsync();
                //南北
                Latitude.Add( pos.Coordinate.Point.Position.Latitude);
                //东西
                Longitude.Add( pos.Coordinate.Point.Position.Longitude);
                gn++;
            }
            catch (System.UnauthorizedAccessException)
            {
                speed_GPS.Text = "无数据";
            }
            //计算速度啦
            double temp=0;

            gv.Add(temp);
            speed_GPS.Text = gv[gn].ToString("0.00");
        }

#endregion

        double vector_add(double x, double y, double z)
        {
            double result=0;
            result = x * x + y * y + z * z;
            result = Math.Sqrt(result);
            return result;
        }

        void Init()
        {
            xv = new List<double>();
            yv = new List<double>();
            zv = new List<double>();
            v = new List<double>();
            gv = new List<double>();
            Latitude = new List<double>();
            Longitude = new List<double>();
            an = 0;
            gn = 0;
           speed.Text = "";
           xspeed.Text = "";
           yspeed.Text = "";
           zspeed.Text = "";
        }

        #region AddInfo
        //Info Page
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            //this.NavigationService.Navigate(new Uri("/Info_Page.xaml", UriKind.Relative));
            Frame.Navigate(typeof(ItemPage));
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
    new Uri(string.Format("ms-windows-store:reviewapp?appid=" + "d37af074-862b-4145-95cd-4c6b200da41f")));
        }
        #endregion
    }
}
