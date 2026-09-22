using Microsoft.Win32;
using Nika_Vizard_Station.Services;
using Nika_Vizard_Station.Services.ColorChanging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageMagick;
using static Nika_Vizard_Station.CurveHelper;
using Point = System.Windows.Point;
using System.Timers;


namespace Nika_Vizard_Station
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PixelInformation[][] OriginalImage { get; set; }
        public PixelInformation[][] Preview { get; set; }
        public PixelInformation[][] MaskImage { get; set; }
        public bool LastAligment { get; set; }
        public byte[] OrigByteArray { get; set; }
        public byte[] SharpenByteArray { get; set; }
        public byte[] SharpenBaseByteArray { get; set; }
        public byte[] PreviewByteArray { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public CurveSortBy CurveFilter { get; set; } = CurveSortBy.All;
        public List<Point> RedCurve { get; set; } = GetInitialPoints();
        public List<Point> GreenCurve { get; set; } = GetInitialPoints();
        public List<Point> BlueCurve { get; set; } = GetInitialPoints();
        public List<Point> RGBCurve { get; set; } = GetInitialPoints();
        public System.Drawing.Bitmap Bitmap { get; set; }

        public double[] RedCurveValue { get; set; }
        public double[] GreenCurveValue { get; set; }
        public double[] BlueCurveValue { get; set; }
        public double[] RGBCurveValue { get; set; }
        public ColorEditingSettings CEsettings { get; set; }
        public PixelSortingSettings PSsettings { get; set; }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public List<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();
        public double CurrentTime { get; set; } = 0;
        public double StartTime { get; set; } = 0;
        public double EndTime { get; set; } = 10;
        public Forms CurrentForm { get; set; }
        public List<Forms> AnimationLayers { get; set; } = new List<Forms>();

        public bool IsCurveChanged { get; set; }
        public bool IsMatrixChanged { get; set; }
        public BitmapSource OriginalBitmap { get; set; }
        public BitmapSource PreviewlBitmap { get; set; }
        public List<(double Time, BitmapSource Cache)> Cache { get; set; } = new List<(double Time, BitmapSource Cache)>();
        public bool IsPlay { get; set; }
        public List<Button> LeftButtons { get; set; }
        public BitmapSource OriginBitmapSource { get; set; }
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }
        public struct PixelInformation
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
            public byte Hue;
            public byte Lightness;
            public byte Saturation;
            public byte Luminance;
        }
        public enum Forms
        {
            ColorEditing,
            PixelSorting
        }


        public MainWindow()
        {

            PSsettings = new PixelSortingSettings
            {
                MaskStart = 70,
                MaskEnd = 185,
                SortFilter = SortBy.Red,
                MaskFilter = SortBy.Lightness,
            };

            CEsettings = new ColorEditingSettings();
            CEsettings.Contrast = 1;
            CEsettings.Saturation = 1;
            CEsettings.Sharpen = 9;

            InitializeComponent();

            LeftButtons = new List<Button>
            {
                PSbutton,
                CEbutton,
                SaveImage,
                SaveGif,
                MakeBeauty
            };
        }


        private void SaveCurrentImage()
        {
            var source = MainImage.Source as BitmapSource;
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            OriginalBitmap = source;
            Width = source.PixelWidth;
            Height = source.PixelHeight;
            OriginalImage = BitmapHelper.CopyPixels(source, Width * 4, 0);
            Preview = BitmapHelper.CopyPixels(source, Width * 4, 0);

            OrigByteArray = new byte[Height * Width * 4];
            PreviewByteArray = new byte[Height * Width * 4];
            SharpenByteArray = new byte[Height * Width * 4];
            SharpenBaseByteArray = new byte[Height * Width * 4];
            source.CopyPixels(OrigByteArray, Width * 4, 0);
            source.CopyPixels(PreviewByteArray, Width * 4, 0);
            source.CopyPixels(SharpenByteArray, Width * 4, 0);
            source.CopyPixels(SharpenBaseByteArray, Width * 4, 0);

            PSsettings.Mask = MaskHelper.GenerateMask(OriginalImage, PSsettings);
            MaskImage = MaskHelper.CreateMaskImage(PSsettings.Mask);

            LastAligment = true;
        }


        private void LoadPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;|All files (*.*)|*.*";
            if (dlg.ShowDialog() == false)
                return;

            Bitmap = new System.Drawing.Bitmap(dlg.FileName);
            MainImage.Source = new BitmapImage(new Uri(dlg.FileName));
            OriginBitmapSource = new BitmapImage(new Uri(dlg.FileName));

            SaveCurrentImage();

            ImageBack.Visibility = Visibility.Collapsed;
            SettingsTabControl.SelectedIndex = 0;
            foreach (var buttons in LeftButtons)
            {
                buttons.IsEnabled = true;
            }
        }

        #region LeftMenu

        private void MakeBeauty_Click(object sender, RoutedEventArgs e)
        {
            SettingsTabControl.SelectedIndex = 1;
        }
        private void Sorting_Click(object sender, RoutedEventArgs e)
        {

            SettingsTabControl.SelectedIndex = 2;
            CurrentForm = Forms.PixelSorting;
            if (!AnimationLayers.Any(x => x == Forms.PixelSorting))
            {
                SetColorSettingsToDefault();

                PSsettings.MaskStart = 70;
                PSsettings.MaskEnd = 185;
                PSsettings.SortFilter = SortBy.Red;
                PSsettings.MaskFilter = SortBy.Lightness;
            }
            PSsettings.Mask = MaskHelper.GenerateMask(OriginalImage, PSsettings);
        }

        private void Curve_Click(object sender, RoutedEventArgs e)
        {

            CurrentForm = Forms.ColorEditing;

            SettingsTabControl.SelectedIndex = 3;
            if (!AnimationLayers.Any(x => x == Forms.ColorEditing))
            {
                SetColorSettingsToDefault();
            }

            var colorSettings = AnimationService.GetColorSettins(KeyPoints, CurrentTime, CEsettings);

            switch (CurveFilter)
            {
                case CurveSortBy.All:
                    CurvePoint1.Value = colorSettings.RGBCurveValue[0];
                    CurvePoint2.Value = colorSettings.RGBCurveValue[1];
                    CurvePoint3.Value = colorSettings.RGBCurveValue[2];
                    CurvePoint4.Value = colorSettings.RGBCurveValue[3];
                    CurvePoint5.Value = colorSettings.RGBCurveValue[4];
                    break;

                case CurveSortBy.Red:
                    CurvePoint1.Value = colorSettings.RedCurveValue[0];
                    CurvePoint2.Value = colorSettings.RedCurveValue[1];
                    CurvePoint3.Value = colorSettings.RedCurveValue[2];
                    CurvePoint4.Value = colorSettings.RedCurveValue[3];
                    CurvePoint5.Value = colorSettings.RedCurveValue[4];
                    break;

                case CurveSortBy.Green:
                    CurvePoint1.Value = colorSettings.GreenCurveValue[0];
                    CurvePoint2.Value = colorSettings.GreenCurveValue[1];
                    CurvePoint3.Value = colorSettings.GreenCurveValue[2];
                    CurvePoint4.Value = colorSettings.GreenCurveValue[3];
                    CurvePoint5.Value = colorSettings.GreenCurveValue[4];
                    break;

                case CurveSortBy.Blue:
                    CurvePoint1.Value = colorSettings.BlueCurveValue[0];
                    CurvePoint2.Value = colorSettings.BlueCurveValue[1];
                    CurvePoint3.Value = colorSettings.BlueCurveValue[2];
                    CurvePoint4.Value = colorSettings.BlueCurveValue[3];
                    CurvePoint5.Value = colorSettings.BlueCurveValue[4];
                    break;
            }

            var points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.RGBCurveValue[0]),
                                new Point(63, 255-colorSettings.RGBCurveValue[1]),
                                new Point(128, 255-colorSettings.RGBCurveValue[2]),
                                new Point(191, 255-colorSettings.RGBCurveValue[3]),
                                new Point(255, 255-colorSettings.RGBCurveValue[4])});
            RGBCurve = points;

            points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.RedCurveValue[0]),
                                new Point(63, 255-colorSettings.RedCurveValue[1]),
                                new Point(128, 255-colorSettings.RedCurveValue[2]),
                                new Point(191, 255-colorSettings.RedCurveValue[3]),
                                new Point(255, 255-colorSettings.RedCurveValue[4])});
            RedCurve = points;

            points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.GreenCurveValue[0]),
                                new Point(63, 255-colorSettings.GreenCurveValue[1]),
                                new Point(128, 255-colorSettings.GreenCurveValue[2]),
                                new Point(191, 255-colorSettings.GreenCurveValue[3]),
                                new Point(255, 255-colorSettings.GreenCurveValue[4])});
            GreenCurve = points;

            points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.BlueCurveValue[0]),
                                new Point(63, 255-colorSettings.BlueCurveValue[1]),
                                new Point(128, 255-colorSettings.BlueCurveValue[2]),
                                new Point(191, 255-colorSettings.BlueCurveValue[3]),
                                new Point(255, 255-colorSettings.BlueCurveValue[4])});
            BlueCurve = points;

            DrawPolyline();

            Brightness.Value = colorSettings.Brightness;
            Contrast.Value = colorSettings.Contrast;
            Saturation.Value = colorSettings.Saturation;
            Hue.Value = colorSettings.Hue;
            Invert.Value = colorSettings.Invert;
            Fade.Value = colorSettings.Fade;
            Temperature.Value = colorSettings.Temperature;
            Sharpen.Value = colorSettings.Sharpen;

        }
        #endregion

        #region SortPixel
        private void SortByRed_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => ApplyPixelSortingSettings(PSsettings, "sort"));

            MaskCheckBox.IsChecked = false;
            LastAligment = PSsettings.IsHorizontal;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (MaskCheckBox.IsChecked == true)
            {
                MainImage.Source = BitmapHelper.FromArray(MaskImage.GetByteArray(true), Width, Height, 4);
            }
            else
            {
                if (AutoShowCheckBox.IsChecked == true)
                    Task.Run(() => ApplyPixelSortingSettings(PSsettings, "sort"));
                else
                    MainImage.Source = BitmapHelper.FromArray(Preview.GetByteArray(LastAligment), Width, Height, 4);
            }
        }

        private void Mask_ValueChanged(object sender, DragDeltaEventArgs e)
        {
            Cache.Clear();

            if (IsMaskValueAnimated.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.PixelSorting))
                    AnimationLayers.Add(Forms.PixelSorting);


                AnimationService.AddKeyPoint(KeyPoints, MaskStart.Value, CurrentTime, Fields.MaskValue1);
                AnimationService.AddKeyPoint(KeyPoints, MaskEnd.Value, CurrentTime, Fields.MaskValue2);
            }

            PSsettings.MaskStart = (int)Math.Min(MaskStart.Value, MaskEnd.Value);
            PSsettings.MaskEnd = (int)Math.Max(MaskStart.Value, MaskEnd.Value);

            if (AutoShowCheckBox.IsChecked == true && MaskCheckBox.IsChecked == false)
                Task.Run(() => ApplyPixelSortingSettings(PSsettings, "default"));
            else
                Task.Run(() => ApplyPixelSortingSettings(PSsettings, "mask"));
        }

        private void SortingFilter_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            string groupName = "SortFilter";
            var activeRadioButton = SortFilter.Children.OfType<RadioButton>()
                                                   .FirstOrDefault(r => r.GroupName == groupName && r.IsChecked == true);

            if (activeRadioButton.Content != null)
            {
                PSsettings.SortFilter = Enum.Parse<SortBy>(activeRadioButton.Content.ToString());

                if (AutoShowCheckBox.IsChecked == true && MaskCheckBox.IsChecked == false)
                    Task.Run(() => ApplyPixelSortingSettings(PSsettings, "sort"));
            }
        }

        private void MaskSortingFilter_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();


            string groupName = "MaskFilter";
            var activeRadioButton = MaskFilter.Children.OfType<RadioButton>()
                                                   .FirstOrDefault(r => r.GroupName == groupName && r.IsChecked == true);

            if (activeRadioButton.Content != null)
            {
                PSsettings.MaskFilter = Enum.Parse<SortBy>(activeRadioButton.Content.ToString());
                var percentMin = MaskStart.Value / MaskStart.Maximum;
                var percentMax = MaskEnd.Value / MaskEnd.Maximum;

                if ((byte)PSsettings.MaskFilter < 3)
                {
                    MaskStart.Maximum = 255;
                    MaskEnd.Maximum = 255;
                }
                if ((byte)PSsettings.MaskFilter == 4)
                {
                    MaskStart.Maximum = 360;
                    MaskEnd.Maximum = 360;
                }
                if ((byte)PSsettings.MaskFilter > 4)
                {
                    MaskStart.Maximum = 100;
                    MaskEnd.Maximum = 100;
                }
                MaskStart.Value = percentMin * MaskStart.Maximum;
                MaskEnd.Value = percentMax * MaskEnd.Maximum;
                PSsettings.MaskStart = (int)Math.Min(MaskStart.Value, MaskEnd.Value);
                PSsettings.MaskEnd = (int)Math.Max(MaskStart.Value, MaskEnd.Value);

                if (AutoShowCheckBox.IsChecked == true && MaskCheckBox.IsChecked == false)
                    Task.Run(() => ApplyPixelSortingSettings(PSsettings, "default"));
                else
                    Task.Run(() => ApplyPixelSortingSettings(PSsettings, "mask"));
            }
        }

        private void HorizontalSorting_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            PSsettings.IsHorizontal = true;

            if (AutoShowCheckBox?.IsChecked == true && MaskCheckBox.IsChecked == false)
                Task.Run(() => ApplyPixelSortingSettings(PSsettings, ""));
        }

        private void VerticalSorting_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            PSsettings.IsHorizontal = false;

            if (AutoShowCheckBox?.IsChecked == true && MaskCheckBox.IsChecked == false)
                Task.Run(() => ApplyPixelSortingSettings(PSsettings, ""));
        }

        private async Task ApplyPixelSortingSettings(PixelSortingSettings settings, string layer)
        {
            if (!await semaphore.WaitAsync(0))
            {
                return;
            }


            switch (layer)
            {
                case "sort":
                    Preview = SortingHelper.SortImage(OriginalImage, settings);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MainImage.Source = BitmapHelper.FromArray(Preview.GetByteArray(PSsettings.IsHorizontal), Width, Height, 4);
                    });
                    break;

                case "mask":
                    settings.Mask = MaskHelper.GenerateMask(OriginalImage, settings);
                    MaskImage = MaskHelper.CreateMaskImage(settings.Mask);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (MaskCheckBox.IsChecked == true)
                        {
                            MainImage.Source = BitmapHelper.FromArray(MaskImage.GetByteArray(true), Width, Height, 4);
                        }
                    });
                    break;

                default:
                    settings.Mask = MaskHelper.GenerateMask(OriginalImage, settings);
                    MaskImage = MaskHelper.CreateMaskImage(settings.Mask);
                    Preview = SortingHelper.SortImage(OriginalImage, settings);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MainImage.Source = BitmapHelper.FromArray(Preview.GetByteArray(PSsettings.IsHorizontal), Width, Height, 4);
                    });
                    break;
            }
            semaphore.Release();
        }

        private async void SaveSorting_Click(object sender, RoutedEventArgs e)
        {
            await ApplyPixelSortingSettings(PSsettings, "");

            SaveCurrentImage();

            if (AnimationLayers.Count != 0)
            {
                Task.Run(() => ApplyAllLayers(CurrentTime));
            }
        }

        private void RevertSorting_Click(object sender, RoutedEventArgs e)
        {
            Preview = OriginalImage;
            MainImage.Source = BitmapHelper.FromArray(OriginalImage.GetByteArray(true), Width, Height, 4);
        }

        #endregion

        #region ColorChanging
        #region Curves
        private void DrawPolyline()
        {
            if (CEsettings.RedCurveValue[0] == 0 &&
               CEsettings.RedCurveValue[1] == 63 &&
               CEsettings.RedCurveValue[2] == 128 &&
               CEsettings.RedCurveValue[3] == 191 &&
               CEsettings.RedCurveValue[4] == 255)
            {
                PolylineRed.Visibility = Visibility.Hidden;
            }
            else
            {
                PointCollection RedPointCollection = new PointCollection(RedCurve);
                PolylineRed.Visibility = Visibility.Visible;

                StartRedLine.Point = RedCurve.FirstOrDefault();
                PolylineRedSegment.Points = RedPointCollection;
            }

            if (CEsettings.GreenCurveValue[0] == 0 &&
               CEsettings.GreenCurveValue[1] == 63 &&
               CEsettings.GreenCurveValue[2] == 128 &&
               CEsettings.GreenCurveValue[3] == 191 &&
               CEsettings.GreenCurveValue[4] == 255)
            {
                PolylineGreen.Visibility = Visibility.Hidden;
            }
            else
            {
                PointCollection GreenPointCollection = new PointCollection(GreenCurve);
                PolylineGreen.Visibility = Visibility.Visible;

                StartGreenLine.Point = GreenCurve.FirstOrDefault();
                PolylineGreenSegment.Points = GreenPointCollection;
            }

            if (CEsettings.BlueCurveValue[0] == 0 &&
               CEsettings.BlueCurveValue[1] == 63 &&
               CEsettings.BlueCurveValue[2] == 128 &&
               CEsettings.BlueCurveValue[3] == 191 &&
               CEsettings.BlueCurveValue[4] == 255)
            {
                PolylineBlue.Visibility = Visibility.Hidden;
            }
            else
            {
                PointCollection BluePointCollection = new PointCollection(BlueCurve);
                PolylineBlue.Visibility = Visibility.Visible;

                StartBlueLine.Point = BlueCurve.FirstOrDefault();
                PolylineBlueSegment.Points = BluePointCollection;
            }

            PointCollection RGBpointCollection = new PointCollection(RGBCurve);
            StartRGBLine.Point = RGBCurve.FirstOrDefault();
            PolylineSegment.Points = RGBpointCollection;
        }

        private void CurvePoint1_ValueChanged(object sender, DragDeltaEventArgs e)
        {
            Cache.Clear();

            if (IsCurveAnimated.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.ColorEditing))
                    AnimationLayers.Add(Forms.ColorEditing);

                var sliderMappings = new Dictionary<string, (double value, Dictionary<CurveSortBy, Fields> fields)>
                {
                    ["CurvePoint1"] = (CurvePoint1.Value, new Dictionary<CurveSortBy, Fields>
                    {
                        [CurveSortBy.All] = Fields.RGBCurvePoint1,
                        [CurveSortBy.Red] = Fields.RedCurvePoint1,
                        [CurveSortBy.Green] = Fields.GreenCurvePoint1,
                        [CurveSortBy.Blue] = Fields.BlueCurvePoint1
                    }),
                    ["CurvePoint2"] = (CurvePoint2.Value, new Dictionary<CurveSortBy, Fields>
                    {
                        [CurveSortBy.All] = Fields.RGBCurvePoint2,
                        [CurveSortBy.Red] = Fields.RedCurvePoint2,
                        [CurveSortBy.Blue] = Fields.BlueCurvePoint2,
                        [CurveSortBy.Green] = Fields.GreenCurvePoint2
                    }),
                    ["CurvePoint3"] = (CurvePoint3.Value, new Dictionary<CurveSortBy, Fields>
                    {
                        [CurveSortBy.All] = Fields.RGBCurvePoint3,
                        [CurveSortBy.Red] = Fields.RedCurvePoint3,
                        [CurveSortBy.Blue] = Fields.BlueCurvePoint3,
                        [CurveSortBy.Green] = Fields.GreenCurvePoint3
                    }),
                    ["CurvePoint4"] = (CurvePoint4.Value, new Dictionary<CurveSortBy, Fields>
                    {
                        [CurveSortBy.All] = Fields.RGBCurvePoint4,
                        [CurveSortBy.Red] = Fields.RedCurvePoint4,
                        [CurveSortBy.Blue] = Fields.BlueCurvePoint4,
                        [CurveSortBy.Green] = Fields.GreenCurvePoint4
                    }),
                    ["CurvePoint5"] = (CurvePoint5.Value, new Dictionary<CurveSortBy, Fields>
                    {
                        [CurveSortBy.All] = Fields.RGBCurvePoint5,
                        [CurveSortBy.Red] = Fields.RedCurvePoint5,
                        [CurveSortBy.Blue] = Fields.BlueCurvePoint5,
                        [CurveSortBy.Green] = Fields.GreenCurvePoint5
                    }),
                };



                var slider = (Slider)sender;

                if (sliderMappings.TryGetValue(slider.Name, out var mapping))
                {
                    if (mapping.fields.TryGetValue(CurveFilter, out var field))
                    {

                        AnimationService.AddKeyPoint(KeyPoints, mapping.value, CurrentTime, field);
                    }
                }
            }

            if (!IsCurveChanged)
                IsCurveChanged = true;

            GetPoints();

            Task.Run(() => ApplyColorEditingSettings(CEsettings, "curve"));

            DrawPolyline();
        }
        private void GetPoints()
        {
            var points = SetPathData(new List<Point> {
            new Point(0, 255-CurvePoint1.Value),
            new Point(63, 255-CurvePoint2.Value),
            new Point(128, 255-CurvePoint3.Value),
            new Point(191, 255-CurvePoint4.Value),
            new Point(255, 255-CurvePoint5.Value)});

            switch (CurveFilter)
            {
                case CurveSortBy.All:
                    RGBCurve = points;

                    CEsettings.RGBCurveValue[0] = CurvePoint1.Value;
                    CEsettings.RGBCurveValue[1] = CurvePoint2.Value;
                    CEsettings.RGBCurveValue[2] = CurvePoint3.Value;
                    CEsettings.RGBCurveValue[3] = CurvePoint4.Value;
                    CEsettings.RGBCurveValue[4] = CurvePoint5.Value;
                    break;

                case CurveSortBy.Red:
                    RedCurve = points;

                    CEsettings.RedCurveValue[0] = CurvePoint1.Value;
                    CEsettings.RedCurveValue[1] = CurvePoint2.Value;
                    CEsettings.RedCurveValue[2] = CurvePoint3.Value;
                    CEsettings.RedCurveValue[3] = CurvePoint4.Value;
                    CEsettings.RedCurveValue[4] = CurvePoint5.Value;
                    break;

                case CurveSortBy.Green:
                    GreenCurve = points;

                    CEsettings.GreenCurveValue[0] = CurvePoint1.Value;
                    CEsettings.GreenCurveValue[1] = CurvePoint2.Value;
                    CEsettings.GreenCurveValue[2] = CurvePoint3.Value;
                    CEsettings.GreenCurveValue[3] = CurvePoint4.Value;
                    CEsettings.GreenCurveValue[4] = CurvePoint5.Value;
                    break;

                case CurveSortBy.Blue:
                    BlueCurve = points;

                    CEsettings.BlueCurveValue[0] = CurvePoint1.Value;
                    CEsettings.BlueCurveValue[1] = CurvePoint2.Value;
                    CEsettings.BlueCurveValue[2] = CurvePoint3.Value;
                    CEsettings.BlueCurveValue[3] = CurvePoint4.Value;
                    CEsettings.BlueCurveValue[4] = CurvePoint5.Value;
                    break;
            }
            return;
        }

        private byte[] ChangeImageByCurve(byte[] imageArray)
        {
            byte[] preview;
            var firstArray = ChangeImageSeperatly(imageArray, RedCurve, GreenCurve, BlueCurve);
            preview = ChangeImage(firstArray, RGBCurve);

            return preview;
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

            if (CurveFilterAll.IsChecked == true)
            {
                CurvePoint1.Value = CEsettings.RGBCurveValue[0];
                CurvePoint2.Value = CEsettings.RGBCurveValue[1];
                CurvePoint3.Value = CEsettings.RGBCurveValue[2];
                CurvePoint4.Value = CEsettings.RGBCurveValue[3];
                CurvePoint5.Value = CEsettings.RGBCurveValue[4];

                CurveFilter = CurveSortBy.All;
                DrawPolyline();
            }
            if (CurveFilterRed.IsChecked == true)
            {
                CurvePoint1.Value = CEsettings.RedCurveValue[0];
                CurvePoint2.Value = CEsettings.RedCurveValue[1];
                CurvePoint3.Value = CEsettings.RedCurveValue[2];
                CurvePoint4.Value = CEsettings.RedCurveValue[3];
                CurvePoint5.Value = CEsettings.RedCurveValue[4];

                CurveFilter = CurveSortBy.Red;
                DrawPolyline();
            }
            if (CurveFilterGreen.IsChecked == true)
            {
                CurvePoint1.Value = CEsettings.GreenCurveValue[0];
                CurvePoint2.Value = CEsettings.GreenCurveValue[1];
                CurvePoint3.Value = CEsettings.GreenCurveValue[2];
                CurvePoint4.Value = CEsettings.GreenCurveValue[3];
                CurvePoint5.Value = CEsettings.GreenCurveValue[4];

                CurveFilter = CurveSortBy.Green;
                DrawPolyline();
            }
            if (CurveFilterBlue.IsChecked == true)
            {

                CurvePoint1.Value = CEsettings.BlueCurveValue[0];
                CurvePoint2.Value = CEsettings.BlueCurveValue[1];
                CurvePoint3.Value = CEsettings.BlueCurveValue[2];
                CurvePoint4.Value = CEsettings.BlueCurveValue[3];
                CurvePoint5.Value = CEsettings.BlueCurveValue[4];

                CurveFilter = CurveSortBy.Blue;
                DrawPolyline();
            }
        }
        #endregion


        private async Task ApplyColorEditingSettings(ColorEditingSettings settings, string layer)
        {
            if (!await semaphore.WaitAsync(0))
            {
                return;
            }

            Bitmap btm;
            byte[] array;
            switch (layer)
            {
                case "sharpen":

                    var sharpenArray = ColorEditingService.ApplySharpen(SharpenBaseByteArray, Width, Height, settings.Sharpen);
                    SharpenByteArray = sharpenArray;

                    if (IsCurveChanged)
                    {
                        array = ChangeImageByCurve(sharpenArray);
                    }
                    else
                    {
                        array = sharpenArray;
                    }

                    PreviewByteArray = array;

                    if (IsMatrixChanged)
                    {
                        btm = ColorEditingService.AdjustColorMatrix(BitmapHelper.CreateBitmapFromByteArray(array, Width, Height),
                            settings.Brightness, settings.Contrast, settings.Saturation,
                            settings.Hue, settings.Invert, settings.Fade, settings.Temperature);

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MainImage.Source = BitmapHelper.ImageSourceFromBitmap(btm);
                        });
                    }
                    else
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MainImage.Source = BitmapHelper.FromArray(array, Width, Height, 4);
                        });
                    }


                    break;

                case "curve":
                    array = ChangeImageByCurve(SharpenByteArray);
                    PreviewByteArray = array;

                    if (IsMatrixChanged)
                    {
                        btm = ColorEditingService.AdjustColorMatrix(BitmapHelper.CreateBitmapFromByteArray(array, Width, Height),
                            settings.Brightness, settings.Contrast, settings.Saturation,
                            settings.Hue, settings.Invert, settings.Fade, settings.Temperature);

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MainImage.Source = BitmapHelper.ImageSourceFromBitmap(btm);
                        });
                    }
                    else
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MainImage.Source = BitmapHelper.FromArray(array, Width, Height, 4);
                        });
                    }

                    break;

                case "matrix":
                    btm = ColorEditingService.AdjustColorMatrix(BitmapHelper.CreateBitmapFromByteArray(PreviewByteArray, Width, Height),
                        settings.Brightness, settings.Contrast, settings.Saturation,
                        settings.Hue, settings.Invert, settings.Fade, settings.Temperature);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MainImage.Source = BitmapHelper.ImageSourceFromBitmap(btm);
                    });
                    break;
            }

            semaphore.Release();
        }

        private async Task<BitmapSource> AnimApplyColorEditingSettings(ColorEditingSettings settings, string layer, BitmapSource btm)
        {
            var sharpenBase = new byte[btm.PixelHeight * btm.PixelWidth * 4];
            var sharpenArray = new byte[btm.PixelHeight * btm.PixelWidth * 4];

            btm.CopyPixels(sharpenBase, btm.PixelWidth * 4, 0);
            btm.CopyPixels(sharpenArray, btm.PixelWidth * 4, 0);

            BitmapSource result;

            byte[] array;
            switch (layer)
            {
                case "sharpen":

                    sharpenArray = ColorEditingService.ApplySharpen(sharpenBase, btm.PixelWidth, btm.PixelHeight, settings.Sharpen);

                    if (IsCurveChanged)
                    {
                        array = ChangeImageByCurve(sharpenArray);
                    }
                    else
                    {
                        array = sharpenArray;
                    }

                    if (IsMatrixChanged)
                    {
                        var bitmap = ColorEditingService.AdjustColorMatrix(BitmapHelper.CreateBitmapFromByteArray(array, btm.PixelWidth, btm.PixelHeight),
                            settings.Brightness, settings.Contrast, settings.Saturation,
                            settings.Hue, settings.Invert, settings.Fade, settings.Temperature);

                        return (BitmapSource)BitmapHelper.ImageSourceFromBitmap(bitmap);
                    }
                    else
                    {
                        return BitmapHelper.FromArray(array, btm.PixelWidth, btm.PixelHeight, 4);
                    }

                case "curve":
                    array = ChangeImageByCurve(sharpenArray);

                    if (IsMatrixChanged)
                    {
                        var bitmap = ColorEditingService.AdjustColorMatrix(BitmapHelper.CreateBitmapFromByteArray(array, btm.PixelWidth, btm.PixelHeight),
                            settings.Brightness, settings.Contrast, settings.Saturation,
                            settings.Hue, settings.Invert, settings.Fade, settings.Temperature);

                        return (BitmapSource)BitmapHelper.ImageSourceFromBitmap(bitmap);
                    }
                    else
                    {
                        return BitmapHelper.FromArray(array, btm.PixelWidth, btm.PixelHeight, 4);
                    }
            }
            return BitmapHelper.FromArray(sharpenBase, btm.PixelWidth, btm.PixelHeight, 4);
        }

        private async Task<BitmapSource> AnimApplyPixelSortingSettings(PixelSortingSettings settings, BitmapSource btm)
        {
            PixelInformation[][] image = new PixelInformation[btm.PixelWidth][];
            image = BitmapHelper.CopyPixels(btm, btm.PixelWidth * 4, 0);
            settings.Mask = MaskHelper.GenerateMask(image, settings);
            var result = SortingHelper.SortImage(image, settings);
            return BitmapHelper.FromArray(result.GetByteArray(settings.IsHorizontal), btm.PixelWidth, btm.PixelHeight, 4);
        }

        private void ColorMatrix_ValueChanged(object sender, DragDeltaEventArgs e)
        {
            Cache.Clear();

            if (!IsMatrixChanged)
                IsMatrixChanged = true;

            var slider = (Slider)sender;
            var mappings = new Dictionary<string, (CheckBox checkBox, double value, Fields field)>
            {
                ["Brightness"] = (IsBrightnessAnimated, Brightness.Value, Fields.Brightness),
                ["Contrast"] = (IsContrastAnimated, Contrast.Value, Fields.Contrast),
                ["Saturation"] = (IsSaturationAnimated, Saturation.Value, Fields.Saturation),
                ["Hue"] = (IsHueAnimated, Hue.Value, Fields.Hue),
                ["Invert"] = (IsInvertAnimated, Invert.Value, Fields.Invert),
                ["Fade"] = (IsFadeAnimated, Fade.Value, Fields.Fade),
                ["Temperature"] = (IsTemperatureAnimated, Temperature.Value, Fields.Temperature),
            };

            if (mappings.TryGetValue(slider.Name, out var mapping) && mapping.checkBox.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.ColorEditing))
                    AnimationLayers.Add(Forms.ColorEditing);


                AnimationService.AddKeyPoint(KeyPoints, mapping.value, CurrentTime, mapping.field);
            }

            CEsettings.Brightness = (float)Brightness.Value;
            CEsettings.Contrast = (float)Contrast.Value;
            CEsettings.Saturation = (float)Saturation.Value;
            CEsettings.Hue = (float)Hue.Value;
            CEsettings.Invert = (float)Invert.Value;
            CEsettings.Fade = (float)Fade.Value;
            CEsettings.Temperature = (float)Temperature.Value;

            Task.Run(() => ApplyColorEditingSettings(CEsettings, "matrix"));
        }

        private void Sharpen_ValueChanged(object sender, DragDeltaEventArgs e)
        {
            Cache.Clear();

            var slider = (Slider)sender;
            if (IsSharpenAnimated.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.ColorEditing))
                    AnimationLayers.Add(Forms.ColorEditing);


                AnimationService.AddKeyPoint(KeyPoints, slider.Value, CurrentTime, Fields.Sharpen);
            }
            CEsettings.Sharpen = (float)Sharpen.Value;

            Task.Run(() => ApplyColorEditingSettings(CEsettings, "sharpen"));
        }

        private void DoubleSharpen_Click(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            SharpenBaseByteArray = SharpenByteArray;
            Sharpen.Value = 9.01;
            ApplyColorEditingSettings(CEsettings, "sharpen");
        }

        private void RevertSharpen_Click(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            SharpenBaseByteArray = OrigByteArray;
            SharpenByteArray = OrigByteArray;
            Sharpen.Value = 9;
            CEsettings.Sharpen = 9;
            ApplyColorEditingSettings(CEsettings, "curve");
        }

        #endregion



        private async void SaveColor_Click(object sender, RoutedEventArgs e)
        {
            if (CEsettings.Sharpen == 9)
            {
                await ApplyColorEditingSettings(CEsettings, "curve");
            }
            else
            {
                await ApplyColorEditingSettings(CEsettings, "sharpen");
            }

            SaveCurrentImage();

            if (AnimationLayers.Count != 0)
            {
                Task.Run(() => ApplyAllLayers(CurrentTime));
            }

            SetColorSettingsToDefault();
        }
        private void SetColorSettingsToDefault()
        {
            CEsettings = new ColorEditingSettings();
            CEsettings.Contrast = 1;
            CEsettings.Saturation = 1;
            CEsettings.Sharpen = 9;

            CurveFilter = CurveSortBy.All;

            CurvePoint1.Value = 0;
            CurvePoint2.Value = 63;
            CurvePoint3.Value = 128;
            CurvePoint4.Value = 191;
            CurvePoint5.Value = 255;

            RedCurve = GetInitialPoints();
            BlueCurve = GetInitialPoints();
            GreenCurve = GetInitialPoints();
            RGBCurve = GetInitialPoints();

            DrawPolyline();

            CurveFilterAll.IsChecked = true;

            if (!AnimationLayers.Any(x => x == Forms.ColorEditing))
            {
                IsCurveChanged = false;
                IsMatrixChanged = false;
            }

            Brightness.Value = 0;
            Contrast.Value = 1;
            Saturation.Value = 1;
            Hue.Value = 0;
            Invert.Value = 0;
            Fade.Value = 0;
            Temperature.Value = 0;
            Sharpen.Value = 9;
            SharpenBaseByteArray = OrigByteArray;
            SharpenByteArray = OrigByteArray;
            ApplyColorEditingSettings(CEsettings, "curve");
        }
        private void RevertColor_Click(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            SetColorSettingsToDefault();
            MainImage.Source = BitmapHelper.FromArray(OrigByteArray, Width, Height, 4);
            SaveCurrentImage();
        }

        #region Animation
        private void Timeline_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            int wholePart = (int)Math.Floor(Timeline.Value);
            double fractionPart = Timeline.Value - wholePart;
            CurrentTime = wholePart + (Math.Round(fractionPart * 0.23, 2));

            if (CurrentForm == Forms.ColorEditing)
            {
                var colorSettings = AnimationService.GetColorSettins(KeyPoints, CurrentTime, CEsettings);

                switch (CurveFilter)
                {
                    case CurveSortBy.All:
                        CurvePoint1.Value = colorSettings.RGBCurveValue[0];
                        CurvePoint2.Value = colorSettings.RGBCurveValue[1];
                        CurvePoint3.Value = colorSettings.RGBCurveValue[2];
                        CurvePoint4.Value = colorSettings.RGBCurveValue[3];
                        CurvePoint5.Value = colorSettings.RGBCurveValue[4];
                        break;

                    case CurveSortBy.Red:
                        CurvePoint1.Value = colorSettings.RedCurveValue[0];
                        CurvePoint2.Value = colorSettings.RedCurveValue[1];
                        CurvePoint3.Value = colorSettings.RedCurveValue[2];
                        CurvePoint4.Value = colorSettings.RedCurveValue[3];
                        CurvePoint5.Value = colorSettings.RedCurveValue[4];
                        break;

                    case CurveSortBy.Green:
                        CurvePoint1.Value = colorSettings.GreenCurveValue[0];
                        CurvePoint2.Value = colorSettings.GreenCurveValue[1];
                        CurvePoint3.Value = colorSettings.GreenCurveValue[2];
                        CurvePoint4.Value = colorSettings.GreenCurveValue[3];
                        CurvePoint5.Value = colorSettings.GreenCurveValue[4];
                        break;

                    case CurveSortBy.Blue:
                        CurvePoint1.Value = colorSettings.BlueCurveValue[0];
                        CurvePoint2.Value = colorSettings.BlueCurveValue[1];
                        CurvePoint3.Value = colorSettings.BlueCurveValue[2];
                        CurvePoint4.Value = colorSettings.BlueCurveValue[3];
                        CurvePoint5.Value = colorSettings.BlueCurveValue[4];
                        break;
                }

                var points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.RGBCurveValue[0]),
                                new Point(63, 255-colorSettings.RGBCurveValue[1]),
                                new Point(128, 255-colorSettings.RGBCurveValue[2]),
                                new Point(191, 255-colorSettings.RGBCurveValue[3]),
                                new Point(255, 255-colorSettings.RGBCurveValue[4])});
                RGBCurve = points;

                points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.RedCurveValue[0]),
                                new Point(63, 255-colorSettings.RedCurveValue[1]),
                                new Point(128, 255-colorSettings.RedCurveValue[2]),
                                new Point(191, 255-colorSettings.RedCurveValue[3]),
                                new Point(255, 255-colorSettings.RedCurveValue[4])});
                RedCurve = points;

                points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.GreenCurveValue[0]),
                                new Point(63, 255-colorSettings.GreenCurveValue[1]),
                                new Point(128, 255-colorSettings.GreenCurveValue[2]),
                                new Point(191, 255-colorSettings.GreenCurveValue[3]),
                                new Point(255, 255-colorSettings.GreenCurveValue[4])});
                GreenCurve = points;

                points = SetPathData(new List<Point> {
                                new Point(0, 255-colorSettings.BlueCurveValue[0]),
                                new Point(63, 255-colorSettings.BlueCurveValue[1]),
                                new Point(128, 255-colorSettings.BlueCurveValue[2]),
                                new Point(191, 255-colorSettings.BlueCurveValue[3]),
                                new Point(255, 255-colorSettings.BlueCurveValue[4])});
                BlueCurve = points;

                DrawPolyline();

                Brightness.Value = colorSettings.Brightness;
                Contrast.Value = colorSettings.Contrast;
                Saturation.Value = colorSettings.Saturation;
                Hue.Value = colorSettings.Hue;
                Invert.Value = colorSettings.Invert;
                Fade.Value = colorSettings.Fade;
                Temperature.Value = colorSettings.Temperature;
                Sharpen.Value = colorSettings.Sharpen;

            }
            if (CurrentForm == Forms.PixelSorting)
            {
                var sortingSettings = AnimationService.GetPixelSortingSettins(KeyPoints, CurrentTime, PSsettings);

                MaskStart.Value = sortingSettings.MaskStart;
                MaskEnd.Value = sortingSettings.MaskEnd;
            }

            if (!IsPlay)
                Task.Run(() => ApplyAllLayers(CurrentTime));
        }

        private object lockObject = new object();
        private async Task ApplyAllLayers(double time)
        {
            if (!semaphore.Wait(0))
            {
                return;
            }

            lock (lockObject)
            {
                if (AnimationLayers.Count == 0)
                {
                    semaphore.Release();
                    return;
                }

                BitmapSource tmpBitmap;
                var temp = new byte[Height * Width * 4];
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    OriginalBitmap.CopyPixels(temp, Width * 4, 0);
                }).Wait();
                tmpBitmap = BitmapHelper.FromArray(temp, Width, Height, 4);

                if (tmpBitmap.PixelHeight > 1000 && tmpBitmap.PixelHeight > 500)
                {
                    var scaleTransform = new ScaleTransform
                    {
                        ScaleX = 0.5, // уменьшение вдвое
                        ScaleY = 0.5
                    };
                    tmpBitmap = new TransformedBitmap(tmpBitmap, scaleTransform);
                }

                bool isFormAdded = false;
                if (!AnimationLayers.Any(x => x == CurrentForm))
                {
                    AnimationLayers.Add(CurrentForm);
                    isFormAdded = true;
                }

                var cacheBtm = Cache.FirstOrDefault(x => x.Time == time).Cache;

                foreach (var layer in AnimationLayers)
                {
                    if (layer == Forms.ColorEditing)
                    {

                        var colorSettings = AnimationService.GetColorSettins(KeyPoints, time, CEsettings);

                        if (cacheBtm != null)
                        {
                            CEsettings = colorSettings;
                            continue;
                        }

                        if (colorSettings.Sharpen == 9)
                        {
                            tmpBitmap = AnimApplyColorEditingSettings(colorSettings, "curve", tmpBitmap).Result;
                        }
                        else
                        {
                            tmpBitmap = AnimApplyColorEditingSettings(colorSettings, "sharpen", tmpBitmap).Result;
                        }
                        CEsettings = colorSettings;
                    }

                    if (layer == Forms.PixelSorting)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            MaskCheckBox.IsChecked = false;
                        }).Wait();
                        var sortingSettings = AnimationService.GetPixelSortingSettins(KeyPoints, time, PSsettings);

                        PSsettings = sortingSettings;
                        if (cacheBtm != null)
                            continue;

                        tmpBitmap = AnimApplyPixelSortingSettings(sortingSettings, tmpBitmap).Result;
                    }
                }

                var result = new byte[tmpBitmap.PixelHeight * tmpBitmap.PixelWidth * 4];
                tmpBitmap.CopyPixels(result, tmpBitmap.PixelWidth * 4, 0);
                var width = tmpBitmap.PixelWidth;
                var height = tmpBitmap.PixelHeight;
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cacheBtm != null)
                    {
                        MainImage.Source = cacheBtm;
                    }
                    else
                    {
                        var btm = BitmapHelper.FromArray(result, width, height, 4);
                        var cache = Cache.FirstOrDefault(x => x.Time == time).Cache;
                        if (cache != null)
                            Cache.Remove((time, cache));

                        Cache.Add((time, btm));
                        MainImage.Source = btm;
                    }
                }).Wait();

                if (isFormAdded)
                    AnimationLayers.Remove(CurrentForm);

                semaphore.Release();
            }
        }
        #endregion

        private void IsSortAnimated_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            if (IsSortAnimated.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.PixelSorting))
                    AnimationLayers.Add(Forms.PixelSorting);

                SortingByBlue.IsEnabled = false;
                SortingByGreen.IsEnabled = false;
                SortingByHue.IsEnabled = false;
                SortingByLightness.IsEnabled = false;
                SortingByLuminance.IsEnabled = false;
                SortingByRed.IsEnabled = false;
                SortingBySaturation.IsEnabled = false;


                AnimationService.AddKeyPoint(KeyPoints, 1, 0, Fields.Sorting);
            }
            else
            {
                SortingByBlue.IsEnabled = true;
                SortingByGreen.IsEnabled = true;
                SortingByHue.IsEnabled = true;
                SortingByLightness.IsEnabled = true;
                SortingByLuminance.IsEnabled = true;
                SortingByRed.IsEnabled = true;
                SortingBySaturation.IsEnabled = true;
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.Sorting);
            }
        }

        private void IsMaskAnimated_Checked(object sender, RoutedEventArgs e)
        {
            Cache.Clear();

            if (IsMaskAnimated.IsChecked == true)
            {
                if (!AnimationLayers.Any(x => x == Forms.PixelSorting))
                    AnimationLayers.Add(Forms.PixelSorting);

                MaskSortingByBlue.IsEnabled = false;
                MaskSortingByGreen.IsEnabled = false;
                MaskSortingByHue.IsEnabled = false;
                MaskSortingByLightness.IsEnabled = false;
                MaskSortingByLuminance.IsEnabled = false;
                MaskSortingByRed.IsEnabled = false;
                MaskSortingBySaturation.IsEnabled = false;


                AnimationService.AddKeyPoint(KeyPoints, 1, 0, Fields.Mask);
            }
            else
            {
                MaskSortingByBlue.IsEnabled = true;
                MaskSortingByGreen.IsEnabled = true;
                MaskSortingByHue.IsEnabled = true;
                MaskSortingByLightness.IsEnabled = true;
                MaskSortingByLuminance.IsEnabled = true;
                MaskSortingByRed.IsEnabled = true;
                MaskSortingBySaturation.IsEnabled = true;
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.Sorting);

            }
        }

        private void IsMaskValueAnimated_Checked(object sender, RoutedEventArgs e)
        {
            if (IsMaskValueAnimated.IsChecked == false)
            {
                Cache.Clear();

                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.MaskValue1);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.MaskValue2);
            }
        }

        private void IsCurveAnimated_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var mappings = new Dictionary<string, (CheckBox checkBox, Fields field)>
            {
                ["IsBrightnessAnimated"] = (IsBrightnessAnimated, Fields.Brightness),
                ["IsContrastAnimated"] = (IsContrastAnimated, Fields.Contrast),
                ["IsSaturationAnimated"] = (IsSaturationAnimated, Fields.Saturation),
                ["IsHueAnimated"] = (IsHueAnimated, Fields.Hue),
                ["IsInvertAnimated"] = (IsInvertAnimated, Fields.Invert),
                ["IsFadeAnimated"] = (IsFadeAnimated, Fields.Fade),
                ["IsTemperatureAnimated"] = (IsTemperatureAnimated, Fields.Temperature),
                ["IsSharpenAnimated"] = (IsSharpenAnimated, Fields.Sharpen),
            };

            if (mappings.TryGetValue(checkBox.Name, out var mapping) && mapping.checkBox.IsChecked == false)
            {
                Cache.Clear();

                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, mapping.field);
            }
            if (checkBox.Name == "IsCurveAnimated" && checkBox.IsChecked == false)
            {
                Cache.Clear();

                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RGBCurvePoint1);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RGBCurvePoint2);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RGBCurvePoint3);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RGBCurvePoint4);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RGBCurvePoint5);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RedCurvePoint1);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RedCurvePoint2);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RedCurvePoint3);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RedCurvePoint4);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.RedCurvePoint5);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.BlueCurvePoint1);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.BlueCurvePoint2);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.BlueCurvePoint3);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.BlueCurvePoint4);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.BlueCurvePoint5);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.GreenCurvePoint1);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.GreenCurvePoint2);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.GreenCurvePoint3);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.GreenCurvePoint4);
                AnimationService.RemoveKeyPoint(KeyPoints, AnimationLayers, Fields.GreenCurvePoint5);
            }
        }


        private void Image_OnLoaded(object sender, RoutedEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image == null) return;

            var rect = new RectangleGeometry(new Rect(0, 0, image.ActualWidth, image.ActualHeight), 5, 5);
            image.Clip = rect;
        }

        private void MainImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image == null) return;

            var rect = new RectangleGeometry(new Rect(0, 0, image.ActualWidth, image.ActualHeight), 5, 5);
            image.Clip = rect;
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var path = (Path)sender;
            var mappings = new Dictionary<Path, RadioButton>
            {
                [BluePath] = SortingByBlue,
                [RedPath] = SortingByRed,
                [GreenPath] = SortingByGreen,
                [HuePath] = SortingByHue,
                [LightnessPath] = SortingByLightness,
                [LuminancePath] = SortingByLuminance,
                [SaturationPath] = SortingBySaturation,
            };

            if (mappings.TryGetValue(path, out var mapping))
            {
                mapping.IsChecked = true;
            }
        }

        private void MaskPath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var path = (Path)sender;
            var mappings = new Dictionary<Path, RadioButton>
            {
                [BlueMaskPath] = MaskSortingByBlue,
                [RedMaskPath] = MaskSortingByRed,
                [GreenMaskPath] = MaskSortingByGreen,
                [HueMaskPath] = MaskSortingByHue,
                [LightnessMaskPath] = MaskSortingByLightness,
                [LuminanceMaskPath] = MaskSortingByLuminance,
                [SaturationMaskPath] = MaskSortingBySaturation,
            };

            if (mappings.TryGetValue(path, out var mapping))
            {
                mapping.IsChecked = true;
            }

        }

        private int index;
        private void BluePath_MouseEnter(object sender, MouseEventArgs e)
        {
            var element = (Path)sender;
            index = Panel.GetZIndex(element);
            Panel.SetZIndex(element, 3);
        }

        private void BluePath_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = (Path)sender;
            Panel.SetZIndex(element, index);
        }

        private void AutoShowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (AutoShowCheckBox.IsChecked == true)
                Apply.IsEnabled = false;
            else
                Apply.IsEnabled = true;
        }

        private void Begining_Click(object sender, RoutedEventArgs e)
        {
            int wholePart;
            double fractionPart;
            var btn = (Button)sender;
            if (btn.Name == "End")
                Timeline.Value = TimelineEnd.Value;

            if (btn.Name == "Begining")
                Timeline.Value = TimelineStart.Value;

            if (btn.Name == "PreviousFrame")
            {
                Timeline.Value = Timeline.Value - (10f / 241f);
                if (Timeline.Value < 0)
                    Timeline.Value = 0;
            }

            if (btn.Name == "NextFrame")
            {

                Timeline.Value = Timeline.Value + (10f / 241f);

                if (Timeline.Value > 10)
                    Timeline.Value = 10;
            }

            wholePart = (int)Math.Floor(Timeline.Value);
            fractionPart = Timeline.Value - wholePart;
            CurrentTime = wholePart + (Math.Round(fractionPart * 0.23, 2));

            Task.Run(() => ApplyAllLayers(CurrentTime));
        }

        private void PlayButton(object sender, RoutedEventArgs e)
        {
            if (TimelineStart.Value > TimelineEnd.Value)
                TimelineStart.Value = 0;

            IsPlay = true;
            Play.Visibility = Visibility.Hidden;
            Pause.Visibility = Visibility.Visible;

            int wholePart;
            double fractionPart;
            wholePart = (int)Math.Floor(Timeline.Value);
            fractionPart = Timeline.Value - wholePart;
            CurrentTime = wholePart + (Math.Round(fractionPart * 0.23, 2));
            var time = CurrentTime;
            var isPlay = IsPlay;

            wholePart = (int)Math.Floor(TimelineEnd.Value);
            fractionPart = TimelineEnd.Value - wholePart;
            EndTime = wholePart + (Math.Round(fractionPart * 0.23, 2));

            if (time >= EndTime)
            {
                Timeline.Value = TimelineStart.Value;
                wholePart = (int)Math.Floor(Timeline.Value);
                fractionPart = Timeline.Value - wholePart;
                CurrentTime = wholePart + (Math.Round(fractionPart * 0.23, 2));
                time = CurrentTime;
            }
            Task.Run(() => PlayState(time, EndTime));

        }
        private void PauseButton(object sender, RoutedEventArgs e)
        {
            IsPlay = false;
            Pause.Visibility = Visibility.Hidden;
            Play.Visibility = Visibility.Visible;
        }
        public async void PlayState(double time, double endTime)
        {
            time += 0.01;
            time = Math.Round(time, 2);
            var fractialPart = Math.Round(time - Math.Floor(time), 2);
            if (fractialPart > 0.23)
            {
                time = time + 0.76;
            }

            if (time > endTime)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsPlay = false;
                    Pause.Visibility = Visibility.Hidden;
                    Play.Visibility = Visibility.Visible;
                });
                return;
            }
            bool isPlay = false;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                isPlay = IsPlay;
            });

            if (isPlay)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Timeline.Value = (int)Math.Floor(time) + (time - (int)Math.Floor(time)) / 0.24;
                });
                var timer = Stopwatch.StartNew();
                await ApplyAllLayers(time);
                timer.Stop();
                if (timer.ElapsedMilliseconds < (1000 / 24))
                    Thread.Sleep((int)(1000 / 24) - (int)timer.ElapsedMilliseconds);

                PlayState(time, endTime);
            }
            else
            {

            }
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                this.MaxHeight = SystemParameters.WorkArea.Height + 12;
                this.MaxWidth = SystemParameters.WorkArea.Width + 12;
                MainGrid.Margin = new Thickness(6);
                this.WindowState = WindowState.Maximized;
                RoundBorder.Rect = new System.Windows.Rect(0, 0, ActualWidth, ActualHeight);
            }
            else
            {
                RoundBorder.RadiusX = 22;
                RoundBorder.RadiusY = 22;
                RoundBorder.Rect = new System.Windows.Rect(0, 0, ActualWidth, ActualHeight);
                MainGrid.Margin = new Thickness(0);
                this.WindowState = WindowState.Normal;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RoundBorder.Rect = new System.Windows.Rect(0, 0, ActualWidth, ActualHeight);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*",
                DefaultExt = "png",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                var fileExtension = System.IO.Path.GetExtension(dialog.FileName)?.ToLower() ?? string.Empty;

                BitmapEncoder encoder = fileExtension switch
                {
                    ".jpg" => new JpegBitmapEncoder(),
                    ".jpeg" => new JpegBitmapEncoder(),
                    ".png" => new PngBitmapEncoder(),
                    _ => throw new InvalidOperationException("Unsupported file format")
                };

                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)MainImage.Source));

                using var fileStream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create);
                encoder.Save(fileStream);
            }
        }
        private void SaveGif_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "GIF files |*.gif;",
                DefaultExt = "gif",
                AddExtension = true
            };
            if (dialog.ShowDialog() == false)
                return;


            if (TimelineStart.Value > TimelineEnd.Value)
                TimelineStart.Value = 0;

            var wholePart = (int)Math.Floor(TimelineStart.Value);
            var fractionPart = TimelineStart.Value - wholePart;
            StartTime = wholePart + (Math.Round(fractionPart * 0.23, 2));

            wholePart = (int)Math.Floor(TimelineEnd.Value);
            fractionPart = TimelineEnd.Value - wholePart;
            EndTime = wholePart + (Math.Round(fractionPart * 0.23, 2));

            var time = StartTime;
            Task.Run(() => SaveGifAsync(time, dialog.FileName, null));

            LoadCanvas.Visibility = Visibility.Visible;
        }

        private async void SaveGifAsync(double time, string fileName, BitmapSource frame)
        {

            //using (var gifWriter = new GifWriter(fileName))
            //{
            //    gifWriter.DefaultFrameDelay = 500;

            //    while (time < EndTime)
            //    {
            //        await Application.Current.Dispatcher.InvokeAsync(() =>
            //        {
            //            frame = Cache.FirstOrDefault(x => x.Time == time).Cache;
            //        });

            //        if (frame == null)
            //        {
            //            await ApplyAllLayers(time);
            //            await Application.Current.Dispatcher.InvokeAsync(() =>
            //            {
            //                frame = Cache.FirstOrDefault(x => x.Time == time).Cache;
            //            });

            //            if (frame == null)
            //            {
            //                await Application.Current.Dispatcher.InvokeAsync(() =>
            //                {
            //                    frame = (BitmapSource)MainImage.Source;
            //                });
            //            }
            //        }
            //        var arr = new byte[height * width * 4];
            //        var btm = new Bitmap(width, height);
            //        await Application.Current.Dispatcher.InvokeAsync(() =>
            //        {
            //            frame.CopyPixels(arr, width * 4, 0);
            //            btm = BitmapHelper.CreateBitmapFromByteArray(arr, width, height);
            //        });

            //        System.Drawing.Image image = btm;
            //        gifWriter.WriteFrame(image, 50);

            //        time += 0.01;
            //        time = Math.Round(time, 2);

            //        var fractialPart = Math.Round(time - Math.Floor(time), 2);
            //        if (fractialPart > 0.23)
            //        {
            //            time = time + 0.76;
            //        }

            //        await Application.Current.Dispatcher.InvokeAsync(() =>
            //        {
            //            var realTime = (int)Math.Floor(time) + (time - (int)Math.Floor(time)) / 0.24;
            //            var realEndTime = (int)Math.Floor(EndTime) + (EndTime - (int)Math.Floor(EndTime)) / 0.24;
            //            var realStartTime = (int)Math.Floor(StartTime) + (StartTime - (int)Math.Floor(StartTime)) / 0.24;
            //            loadBar.Value = (realTime / (realEndTime - realStartTime) * 100);
            //        });
            //    }
            //    await Application.Current.Dispatcher.InvokeAsync(() =>
            //    {
            //        LoadCanvas.Visibility = Visibility.Collapsed;
            //    });

            //var listToSave = Cache.OrderBy(x => x.Time);

            //foreach (var frame in listToSave)
            //{
            //    var arr = new byte[Height * Width * 4];
            //    frame.Cache.CopyPixels(arr, Width * 4, 0);
            //    var btm = BitmapHelper.CreateBitmapFromByteArray(arr, Width, Height);
            //    System.Drawing.Image image = btm;
            //    gifWriter.WriteFrame(image, 50);
            //}
            //listToSave = Cache.OrderByDescending(x => x.Time);
            //foreach (var frame in listToSave)
            //{
            //    var arr = new byte[Height * Width * 4];
            //    frame.Cache.CopyPixels(arr, Width * 4, 0);
            //    var btm = BitmapHelper.CreateBitmapFromByteArray(arr, Width, Height);
            //    System.Drawing.Image image = btm;
            //    gifWriter.WriteFrame(image, 50);
            //}

            using (var collection = new MagickImageCollection())
            {

                while (time < EndTime)
                {
                    int height = 0;
                    int width = 0;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        frame = Cache.FirstOrDefault(x => x.Time == time).Cache;
                    });

                    if (frame == null)
                    {
                        await ApplyAllLayers(time);
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            frame = Cache.FirstOrDefault(x => x.Time == time).Cache;
                        });

                        if (frame == null)
                        {
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                frame = (BitmapSource)MainImage.Source;
                            });
                        }
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        height = frame.PixelHeight;
                        width = frame.PixelWidth;
                    });

                    var arr = new byte[height * width * 4];
                    var btm = new Bitmap(width, height);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        frame.CopyPixels(arr, width * 4, 0);
                        btm = BitmapHelper.CreateBitmapFromByteArray(arr, width, height);
                    });

                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        btm.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        memoryStream.Position = 0;
                        var magickImage = new MagickImage(memoryStream);

                        magickImage.Resize(1000, 1000);

                        collection.Add(magickImage);
                        collection[collection.Count - 1].AnimationDelay = 15; 
                    }

                    time += 0.01;
                    time = Math.Round(time, 2);

                    var fractialPart = Math.Round(time - Math.Floor(time), 2);
                    if (fractialPart > 0.23)
                    {
                        time = time + 0.76;
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var realTime = (int)Math.Floor(time) + (time - (int)Math.Floor(time)) / 0.24;
                        var realEndTime = (int)Math.Floor(EndTime) + (EndTime - (int)Math.Floor(EndTime)) / 0.24;
                        var realStartTime = (int)Math.Floor(StartTime) + (StartTime - (int)Math.Floor(StartTime)) / 0.24;
                        loadBar.Value = (realTime / (realEndTime - realStartTime) * 65);
                    });
                }

                QuantizeSettings settings = new QuantizeSettings
                {
                    Colors = 256 // Ограничение до 256 цветов
                };
                collection.Quantize(settings);

                // Установите метод оптимизации кадров
                collection.Optimize();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    loadBar.Value = 85;
                });
                // Сохраните GIF
                collection.Write(fileName);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LoadCanvas.Visibility = Visibility.Collapsed;
                });

            }
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            MainImage.Source = OriginBitmapSource;
            var temp = new byte[Height * Width * 4];
            OriginBitmapSource.CopyPixels(temp, Width * 4, 0);
            var tmpBitmap = BitmapHelper.FromArray(temp, Width, Height, 4);

            SaveCurrentImage();
            var rnd = new Random();
            Cache.Clear();
            KeyPoints.Clear();
            AnimationLayers.Clear();
            SetCheckBoxes();
            if (RandomAnimation.IsChecked == true)
            {

                if (PSRandomEnable.IsChecked == true)
                {
                    RandomService.AnimGeneratePSsettings(RandomPower.Value, KeyPoints);
                    if (KeyPoints.Any(x => (int)x.FieldName > 27 && (int)x.FieldName < 32))
                    {
                        AnimationLayers.Add(Forms.PixelSorting);
                    }
                }
                else
                {
                    PSsettings = new PixelSortingSettings
                    {
                        MaskStart = 70,
                        MaskEnd = 185,
                        SortFilter = SortBy.Red,
                        MaskFilter = SortBy.Lightness,
                    };
                }
                if (CERandomEnable.IsChecked == true)
                {
                    IsMatrixChanged = true;
                    IsCurveChanged = true;
                    RandomService.AnimGenerateCEsettings(RandomPower.Value, KeyPoints);
                    if (KeyPoints.Any(x => (int)x.FieldName < 28))
                    {
                        AnimationLayers.Add(Forms.ColorEditing);
                    }


                }
                else
                {
                    CEsettings = new ColorEditingSettings();
                    CEsettings.Contrast = 1;
                    CEsettings.Saturation = 1;
                    CEsettings.Sharpen = 9;
                }

                var ceSettings = AnimationService.GetColorSettins(KeyPoints, CurrentTime, CEsettings);
                RGBCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.RGBCurveValue[0]),
                    new Point(63, 255-ceSettings.RGBCurveValue[1]),
                    new Point(128, 255-ceSettings.RGBCurveValue[2]),
                    new Point(191, 255-ceSettings.RGBCurveValue[3]),
                    new Point(255, 255-ceSettings.RGBCurveValue[4])});

                RedCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.RedCurveValue[0]),
                    new Point(63, 255-ceSettings.RedCurveValue[1]),
                    new Point(128, 255-ceSettings.RedCurveValue[2]),
                    new Point(191, 255-ceSettings.RedCurveValue[3]),
                    new Point(255, 255-ceSettings.RedCurveValue[4])});

                GreenCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.GreenCurveValue[0]),
                    new Point(63, 255-ceSettings.GreenCurveValue[1]),
                    new Point(128, 255-ceSettings.GreenCurveValue[2]),
                    new Point(191, 255-ceSettings.GreenCurveValue[3]),
                    new Point(255, 255-ceSettings.GreenCurveValue[4])});

                BlueCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.BlueCurveValue[0]),
                    new Point(63, 255-ceSettings.BlueCurveValue[1]),
                    new Point(128, 255-ceSettings.BlueCurveValue[2]),
                    new Point(191, 255-ceSettings.BlueCurveValue[3]),
                    new Point(255, 255-ceSettings.BlueCurveValue[4])});

                SetCheckBoxes();
                int wholePart = (int)Math.Floor(Timeline.Value);
                double fractionPart = Timeline.Value - wholePart;
                CurrentTime = wholePart + (Math.Round(fractionPart * 0.23, 2));
                Task.Run(() => ApplyAllLayers(CurrentTime));
            }
            else
            {

                ColorEditingSettings ceSettings;
                PixelSortingSettings psSettings;
                if (PSRandomEnable.IsChecked == true)
                {
                    psSettings = RandomService.GeneratePSsettings(RandomPower.Value);
                    tmpBitmap = AnimApplyPixelSortingSettings(psSettings, tmpBitmap).Result;
                }
                if (CERandomEnable.IsChecked == true)
                {
                    ceSettings = RandomService.GenerateCEsettings(RandomPower.Value);
                    IsMatrixChanged = true;
                    IsCurveChanged = true;
                    RGBCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.RGBCurveValue[0]),
                    new Point(63, 255-ceSettings.RGBCurveValue[1]),
                    new Point(128, 255-ceSettings.RGBCurveValue[2]),
                    new Point(191, 255-ceSettings.RGBCurveValue[3]),
                    new Point(255, 255-ceSettings.RGBCurveValue[4])});

                    RedCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.RedCurveValue[0]),
                    new Point(63, 255-ceSettings.RedCurveValue[1]),
                    new Point(128, 255-ceSettings.RedCurveValue[2]),
                    new Point(191, 255-ceSettings.RedCurveValue[3]),
                    new Point(255, 255-ceSettings.RedCurveValue[4])});

                    GreenCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.GreenCurveValue[0]),
                    new Point(63, 255-ceSettings.GreenCurveValue[1]),
                    new Point(128, 255-ceSettings.GreenCurveValue[2]),
                    new Point(191, 255-ceSettings.GreenCurveValue[3]),
                    new Point(255, 255-ceSettings.GreenCurveValue[4])});

                    BlueCurve = SetPathData(new List<Point> {
                    new Point(0, 255-ceSettings.BlueCurveValue[0]),
                    new Point(63, 255-ceSettings.BlueCurveValue[1]),
                    new Point(128, 255-ceSettings.BlueCurveValue[2]),
                    new Point(191, 255-ceSettings.BlueCurveValue[3]),
                    new Point(255, 255-ceSettings.BlueCurveValue[4])});


                    if (rnd.NextDouble() > 0.5)
                    {
                        tmpBitmap = AnimApplyColorEditingSettings(ceSettings, "curve", tmpBitmap).Result;
                    }
                    else
                    {
                        tmpBitmap = AnimApplyColorEditingSettings(ceSettings, "sharpen", tmpBitmap).Result;
                    }
                    IsMatrixChanged = false;
                    IsCurveChanged = false;
                }
                MainImage.Source = tmpBitmap;
            }
        }

        private void RevertRandom_Click(object sender, RoutedEventArgs e)
        {
            SetColorSettingsToDefault();
            MainImage.Source = BitmapHelper.FromArray(OriginalImage.GetByteArray(true), Width, Height, 4);
        }
        private void SaveRandom_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentImage();
        }
        private void SetCheckBoxes()
        {
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsCurveAnimated.IsChecked = false;
            IsBrightnessAnimated.IsChecked = false;
            IsContrastAnimated.IsChecked = false;
            IsSaturationAnimated.IsChecked = false;
            IsHueAnimated.IsChecked = false;
            IsInvertAnimated.IsChecked = false;
            IsFadeAnimated.IsChecked = false;
            IsTemperatureAnimated.IsChecked = false;
            IsSharpenAnimated.IsChecked = false;
            IsSortAnimated.IsChecked = false;
            IsMaskAnimated.IsChecked = false;
            IsMaskValueAnimated.IsChecked = false;
            IsMaskValueAnimated.IsChecked = false;
            var mappings = new Dictionary<Fields, CheckBox>
            {
                [Fields.RGBCurvePoint1] = IsCurveAnimated,
                [Fields.RGBCurvePoint2] = IsCurveAnimated,
                [Fields.RGBCurvePoint3] = IsCurveAnimated,
                [Fields.RGBCurvePoint4] = IsCurveAnimated,
                [Fields.RGBCurvePoint5] = IsCurveAnimated,
                [Fields.RedCurvePoint1] = IsCurveAnimated,
                [Fields.RedCurvePoint2] = IsCurveAnimated,
                [Fields.RedCurvePoint3] = IsCurveAnimated,
                [Fields.RedCurvePoint4] = IsCurveAnimated,
                [Fields.RedCurvePoint5] = IsCurveAnimated,
                [Fields.BlueCurvePoint1] = IsCurveAnimated,
                [Fields.BlueCurvePoint2] = IsCurveAnimated,
                [Fields.BlueCurvePoint3] = IsCurveAnimated,
                [Fields.BlueCurvePoint4] = IsCurveAnimated,
                [Fields.BlueCurvePoint5] = IsCurveAnimated,
                [Fields.GreenCurvePoint1] = IsCurveAnimated,
                [Fields.GreenCurvePoint2] = IsCurveAnimated,
                [Fields.GreenCurvePoint3] = IsCurveAnimated,
                [Fields.GreenCurvePoint4] = IsCurveAnimated,
                [Fields.GreenCurvePoint5] = IsCurveAnimated,
                [Fields.Brightness] = IsBrightnessAnimated,
                [Fields.Contrast] = IsContrastAnimated,
                [Fields.Saturation] = IsSaturationAnimated,
                [Fields.Hue] = IsHueAnimated,
                [Fields.Invert] = IsInvertAnimated,
                [Fields.Fade] = IsFadeAnimated,
                [Fields.Temperature] = IsTemperatureAnimated,
                [Fields.Sharpen] = IsSharpenAnimated,
                [Fields.Sorting] = IsSortAnimated,
                [Fields.Mask] = IsMaskAnimated,
                [Fields.MaskValue1] = IsMaskValueAnimated,
                [Fields.MaskValue2] = IsMaskValueAnimated,
            };

            foreach (var key in KeyPoints)
            {
                if (mappings.TryGetValue(key.FieldName, out var mapping))
                {
                    mapping.IsChecked = true;
                }
            }
        }

    }
}

