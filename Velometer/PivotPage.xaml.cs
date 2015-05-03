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
using Windows.UI.Xaml.Media.Imaging;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Velometer
{
    public sealed partial class PivotPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
       // private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        //List<double> xv = new List<double>();
        //List<double> yv = new List<double>();
        //List<double> zv = new List<double>();
        List<double> v = new List<double>();
        List<double> gv = new List<double>();
       // double[] v; double[] gv;
        List<double> Latitude = new List<double>();
        List<double> Longitude = new List<double>();
        //double[] Latitude; double[] Longitude;
        //计数变量
        int an = 0;
        int gn = 0;
        bool unitflag = true; bool gpsunitflag = true;
        bool run_state = false;
        Accelerometer accelerometer;
        AccelerometerReading accelerometerReading = null;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        Geolocator gpswatcher = new Geolocator()
        {
            //移动阈值
            MovementThreshold = 5,
                            //精度：高
            DesiredAccuracy = PositionAccuracy.High
        };
        Geoposition pos;

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
            //给eclipse注册touch事件
            zero.PointerPressed += Get_Acce_Data;
            //给speed文本框注册touch事件
            speed.PointerPressed += text_speed_touched;
            //给gps speed文本框注册touch事件
            speed_GPS.PointerPressed += text_gpsspeed_touched;
            //dispatcherTimer.Start();
            dispatcherTimer.Tick += new EventHandler<object>(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            //采样间隔：1s
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

        private void dispatcherTimer_Tick(object sender, object e)
        {
            //AcceShowData();
            ShowGPSData();
        }

        #region accelerometer
        //摸圆圈儿触发
        public async void Get_Acce_Data(object sender, RoutedEventArgs e)
        {
            Init();
            //修改zero圆圈的颜色
            /*
            var img = new Uri("ms-appx://Assets/zero.png", UriKind.Absolute);
            ImageBrush ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(img);
            zero.Fill = ib;
            */
            //if (run_state == false)
            //{
                run_state = true;
                accelerometer = Accelerometer.GetDefault();
                if (accelerometer == null)
                {
                    await new MessageDialog("您的手机不支持加速度传感器").ShowAsync();
                    return;
                }
                else
                {
                    //xv.Add(0); yv.Add(0); zv.Add(0); 
                    v.Add(0);
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
                    dispatcherTimer.Start();
                }

                Get_GPS_Data();
           // }
           // else
           // {
           //     run_state = false;
           //     dispatcherTimer.Stop();
          //      Init();
           // }
        }

        async void accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    accelerometerReading = args.Reading;
                    AcceShowData();
                });
        }

        void accelerometer_Shaken(Accelerometer sender, AccelerometerShakenEventArgs args)
        {
            throw new NotImplementedException();
        }

        void AcceShowData()
        {
            double xa = 0;
            double ya = 0;
            double za = 0;
            accelerometerReading = accelerometer.GetCurrentReading();
            //读取数据
                xa = 9.81 * accelerometerReading.AccelerationX;
                ya = 9.81 * accelerometerReading.AccelerationY;
                za = 9.81 * accelerometerReading.AccelerationZ;

            //处理，计算数据
            double dt = 1;//accelerometer.MinimumReportInterval;
            //if (ya != 0)
            //{
                an++;
                v.Add(v[an - 1] + dt * xa);
                // xv.Add(xv[an - 1] + dt * xa);
                //yv.Add(yv[an - 1] + dt * xa);
                //zv.Add(zv[an - 1] + dt * xa);
                //v.Add(vector_add(xv[an], yv[an], zv[an]));
                if (unitflag == true)
                {
                    speed.Text = v[an].ToString("0.00");
                    unit1.Text = "m/s";
                }
                if (unitflag==false)
                {
                    double vkm = v[an] / 3.6;
                    speed.Text = vkm.ToString("0.0");
                    unit1.Text = "km/h";
                }
            //}
            //显示加速传感器得到的速度数值
            // xspeed.Text = "x: " + xv[an].ToString("0.00");
            //yspeed.Text = "y: " + yv[an].ToString("0.00");
            //zspeed.Text = "z: " + zv[an].ToString("0.00");
            //else
           // { }
            xspeed.Text = xa.ToString("0.00");
            yspeed.Text = ya.ToString("0.00");
            zspeed.Text = za.ToString("0.00");
            /*
            xspeed.Text = accelerometerReading.AccelerationX.ToString("0.00");
            yspeed.Text = accelerometerReading.AccelerationY.ToString("0.00");
            zspeed.Text = accelerometerReading.AccelerationZ.ToString("0.00");
             * */
        }

        #endregion

        #region gps
        async void Get_GPS_Data()
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
                //位置变化
                gpswatcher.PositionChanged += gpswatcherPositionChanged;
                //传感器状态变化
                gpswatcher.StatusChanged += gpswatcherStatusChanged;
                //ShowGPSData();
                pos = await gpswatcher.GetGeopositionAsync();
            }
        }

        async void gpswatcherPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                pos = e.Position;
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
            pos = await gpswatcher.GetGeopositionAsync();
            try
            {
                double temp1 = pos.Coordinate.Point.Position.Latitude;
                double temp2 = pos.Coordinate.Point.Position.Longitude;

                    if (((Latitude[gn] != temp1) || (Longitude[gn] != temp2)) == true)
                {
                    //南北
                    Latitude.Add(temp1);
                    //东西
                    Longitude.Add(temp2);
                    gn++;
                        //计算速度啦
                        double temp = 0;
                        temp = GetDistance(Latitude[gn-1], Longitude[gn - 1], Latitude[gn], Longitude[gn]);
                        gv.Add(temp);
                        if (gn > 1)
                        {
                            if (gpsunitflag == true)
                            {
                                speed_GPS.Text = gv[gn].ToString("0.00");
                                unit2.Text = "m/s";
                            }
                            else
                            {
                                double gpstemp = gv[gn] / 3.6;
                                speed_GPS.Text = gpstemp.ToString("0.0");
                                unit2.Text = "km/h";
                            }
                        }
                    
                }
                else
                { }
            }
            catch (System.UnauthorizedAccessException)
            {
                speed_GPS.Text = "无数据";
            }

        }

#endregion

        double vector_add(double x, double y)
        {
            double result=0;
            result = x * x + y * y;
            result = Math.Sqrt(result);
            return result;
        }

        private const double EARTH_RADIUS = 6378137;//地球半径,单位m
        private static double rad(double d)
        {
            return d * Math.PI / 180.0;
        }

        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double radLat1 = rad(lat1);
            double radLat2 = rad(lat2);
            double a = radLat1 - radLat2;
            double b = rad(lng1) - rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }

        void Init()
        {
            //xv = new List<double>();
            //yv = new List<double>();
            //zv = new List<double>();
            v = new List<double>();
            gv = new List<double>();
            Latitude = new List<double>();
            Longitude = new List<double>();
            an = 0;
            gn = 0;
           speed.Text = "0.00";
           speed_GPS.Text = "0.00";
           xspeed.Text = "";
           yspeed.Text = "";
           zspeed.Text = "";
            //换回圈圈颜色
            /*
           var img = new Uri("ms-appx://Assets/go.png", UriKind.Absolute);
           ImageBrush ib = new ImageBrush();
           ib.ImageSource = new BitmapImage(img);
           zero.Fill = ib;
             */ 
        }

        #region touchAndDeal
        private void text_speed_touched(object sender, RoutedEventArgs e)
        {
            if (unitflag==true)
            { unitflag = false; }
            else
            { unitflag = true; }
        }

        private void text_gpsspeed_touched(object sender, RoutedEventArgs e)
        {
            if (gpsunitflag == true)
            { gpsunitflag = false; }
            else
            { gpsunitflag = true; }
        }

        #endregion
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
