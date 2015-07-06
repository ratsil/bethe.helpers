using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Windows.Threading;
using System.Collections.Generic;

namespace controls.sl
{
	/// <summary>
	/// Represents a Color Picker control which allows a user to select a color.
	/// </summary>
	public class ColorPicker : Grid
	{
		private Rectangle SelectedColorSample;
		private Canvas Rainbow;
		private Grid ColorField;
		private Grid ColorSelector;
		private Grid HueSelector;
		private Grid ChooserArea;

		public ColorPicker()
		{
			this.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			this.RowDefinitions.Add(new RowDefinition());
			this.MinWidth = 200;

			SelectedColorSample = CreateColorSample();
			Rainbow = CreateRainbow();
			ColorField = CreateColorField();

			ChooserArea = new Grid();
			ChooserArea.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			ChooserArea.ColumnDefinitions.Add(new ColumnDefinition());
			ChooserArea.Children.Add(Rainbow);
			ChooserArea.Children.Add(ColorField);
			Grid.SetColumn(ColorField, 1);

			this.Children.Add(SelectedColorSample);
			this.Children.Add(ChooserArea);
			Grid.SetRow(ChooserArea, 1);

			Expanded = false;
			UpdateSelectedColorSample();
		}

		private Rectangle CreateColorSample()
		{
			var result = new Rectangle();
			result.MinHeight = 20;
			result.HorizontalAlignment = HorizontalAlignment.Stretch;
			result.VerticalAlignment = VerticalAlignment.Stretch;
			result.MouseLeftButtonDown += ColorSample_MouseLeftButtonDown;
			result.Fill = new SolidColorBrush(Colors.Black);
			return result;
		}

		private Canvas CreateRainbow()
		{
			HueSelector = CreateHueSelector();

			var result = new Canvas();
			result.Background = new LinearGradientBrush(
				new GradientStopCollection()
            {
                new GradientStop() { Offset = 0.00, Color = Color.FromArgb(255, 255, 0, 0)},
                new GradientStop() { Offset = 0.17, Color = Color.FromArgb(255, 255, 255, 0) },
                new GradientStop() { Offset = 0.33, Color = Color.FromArgb(255, 0, 255, 0) },
                new GradientStop() { Offset = 0.50, Color = Color.FromArgb(255, 0, 255, 255) },
                new GradientStop() { Offset = 0.66, Color = Color.FromArgb(255, 0, 0, 255) },
                new GradientStop() { Offset = 0.83, Color = Color.FromArgb(255, 255, 0, 255) },
                new GradientStop() { Offset = 1.00, Color = Color.FromArgb(255, 255, 0, 0) },
            },
				90);
			result.HorizontalAlignment = HorizontalAlignment.Stretch;
			result.VerticalAlignment = VerticalAlignment.Stretch;
			result.MinWidth = 20;
			result.MouseLeftButtonDown += Rainbow_MouseLeftButtonDown;
			result.MouseMove += Rainbow_MouseMove;
			result.MouseLeftButtonUp += Rainbow_MouseLeftButtonUp;
			result.Children.Add(HueSelector);
			return result;
		}

		private Grid CreateHueSelector()
		{
			var result = new Grid();
			var triangle1 = new Polygon()
			{
				Points = new PointCollection()
            {
                new Point(0, 0),
                new Point(10, 5),
                new Point(0, 10)
            },
				Fill = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			};
			var triangle2 = new Polygon()
			{
				Points = new PointCollection()
            {
                new Point(10, 0),
                new Point(0, 5),
                new Point(10, 10)
            },
				Fill = new SolidColorBrush(Colors.Black),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top
			};

			result.Children.Add(triangle1);
			result.Children.Add(triangle2);
			result.VerticalAlignment = VerticalAlignment.Top;
			result.HorizontalAlignment = HorizontalAlignment.Left;
			result.IsHitTestVisible = false;
			return result;
		}

		private Grid CreateColorField()
		{
			var whiteGradient = new Rectangle()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			var blackGradient = new Rectangle()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};
			var canvas = new Canvas()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};
			ColorSelector = CreateColorSelector();
			canvas.Children.Add(ColorSelector);

			whiteGradient.Fill = new LinearGradientBrush(
				new GradientStopCollection()
            {
                new GradientStop() { Color = Color.FromArgb(255, 255, 255, 255), Offset = 0 },
                new GradientStop() { Color = Color.FromArgb(0, 255, 255, 255), Offset = 1 }
            },
				0);

			blackGradient.Fill = new LinearGradientBrush(
				new GradientStopCollection()
            {
                new GradientStop() { Color = Color.FromArgb(0, 0, 0, 0), Offset = 0 },
                new GradientStop() { Color = Color.FromArgb(255, 0, 0, 0), Offset = 1 },
            },
				90);

			var result = new Grid();
			result.Background = new SolidColorBrush(Colors.Red);
			result.HorizontalAlignment = HorizontalAlignment.Stretch;
			result.VerticalAlignment = VerticalAlignment.Stretch;
			result.MinWidth = 180;
			result.MinHeight = result.MinWidth;
			result.Children.Add(whiteGradient);
			result.Children.Add(blackGradient);
			result.Children.Add(canvas);
			result.MouseLeftButtonDown += ColorField_MouseLeftButtonDown;
			result.MouseMove += ColorField_MouseMove;
			result.MouseLeftButtonUp += ColorField_MouseLeftButtonUp;
			return result;
		}

		private Grid CreateColorSelector()
		{
			var result = new Grid()
			{
				IsHitTestVisible = false
			};
			var ellipse1 = new Ellipse()
			{
				Width = 10,
				Height = 10,
				StrokeThickness = 3,
				Stroke = new SolidColorBrush(Colors.White)
			};
			var ellipse2 = new Ellipse()
			{
				Width = 10,
				Height = 10,
				StrokeThickness = 1,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			result.Children.Add(ellipse1);
			result.Children.Add(ellipse2);
			result.HorizontalAlignment = HorizontalAlignment.Left;
			result.VerticalAlignment = VerticalAlignment.Top;
			result.IsHitTestVisible = false;
			return result;
		}

		private void ColorSample_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Expanded = !Expanded;
		}

		public bool Expanded
		{
			get
			{
				return ChooserArea.Visibility == Visibility.Visible;
			}
			set
			{
				var visibility = value ? Visibility.Visible : Visibility.Collapsed;
				if (ChooserArea.Visibility == visibility)
				{
					return;
				}
				ChooserArea.Visibility = visibility;
				if (value)
				{
#if SILVERLIGHT
					Dispatcher.BeginInvoke(() => SelectedColor = SelectedColor);
#else
                Dispatcher.BeginInvoke(new Action(() => SelectedColor = SelectedColor), DispatcherPriority.Render);
#endif
				}
			}
		}

		private void Rainbow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			RainbowMouseCaptured = Rainbow.CaptureMouse();
			UpdateHuePos(e.GetPosition(Rainbow).Y);
		}

		private void Rainbow_MouseMove(object sender, MouseEventArgs e)
		{
			if (ColorFieldMouseCaptured || !RainbowMouseCaptured)
			{
				return;
			}

			UpdateHuePos(e.GetPosition(Rainbow).Y);
		}

		private void Rainbow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Rainbow.ReleaseMouseCapture();
			RainbowMouseCaptured = false;
			SelectedColor = GetColor();
		}

		private void ColorField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ColorFieldMouseCaptured = ColorField.CaptureMouse();
			Point coordinates = e.GetPosition(ColorField);
			UpdateSampleXY(coordinates.X, coordinates.Y);
		}

		private void ColorField_MouseMove(object sender, MouseEventArgs e)
		{
			if (RainbowMouseCaptured || !ColorFieldMouseCaptured)
			{
				return;
			}

			Point coordinates = e.GetPosition(ColorField);
			UpdateSampleXY(coordinates.X, coordinates.Y);
		}

		private void ColorField_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ColorField.ReleaseMouseCapture();
			ColorFieldMouseCaptured = false;
			SelectedColor = GetColor();
		}

		private Color GetColor()
		{
			double yComponent = 1 - (m_sampleY / ColorFieldHeight);
			double xComponent = m_sampleX / ColorFieldWidth;
			double hueComponent = (m_huePos / RainbowHeight) * 360;

			return ColorSpace.ConvertHsvToRgb(hueComponent, xComponent, yComponent);
		}

		private Color GetColorFromHue()
		{
			double huePos = m_huePos / RainbowHeight * 255;
			Color c = ColorSpace.GetColorFromPosition(huePos);
			return c;
		}

		private void UpdateSampleXY(double x, double y)
		{
			if (x < 0)
			{
				m_sampleX = 0;
			}
			else if (x >= ColorFieldWidth)
			{
				m_sampleX = ColorFieldWidth;
			}
			else
			{
				m_sampleX = x;
			}

			if (y < 0)
			{
				m_sampleY = 0;
			}
			else if (y >= ColorFieldHeight)
			{
				m_sampleY = ColorFieldHeight;
			}
			else
			{
				m_sampleY = y;
			}
			UpdateSelectedColorSample();
			UpdateColorSelector();
		}

		private void UpdateHuePos(double y)
		{
			if (y < 0)
			{
				m_huePos = 0;
			}
			else if (y >= RainbowHeight)
			{
				m_huePos = RainbowHeight;
			}
			else
			{
				m_huePos = y;
			}

			if (SelectedColor == Colors.Black || SelectedColor == Colors.White)
			{
				SelectedColor = GetColorFromHue();
				return;
			}

			UpdateHueSelector();
			UpdateColorFieldBackground();
			UpdateSelectedColorSample();
		}

		private void UpdateSelectedColorSample()
		{
			var color = SelectedColor;
			if (Expanded)
			{
				color = GetColor();
			}
			SelectedColorSample.Fill = new SolidColorBrush(color);
			FireSelectedColorChangingEvent(color);
		}

		private void UpdateColorFieldBackground()
		{
			Color c = GetColorFromHue();
			ColorField.Background = new SolidColorBrush(c);
		}

		private void UpdateHueSelector()
		{
			Canvas.SetTop(HueSelector, m_huePos - HueSelector.ActualHeight / 2);
			HueSelector.Width = Rainbow.ActualWidth;
		}

		private void UpdateColorSelector()
		{
			Canvas.SetLeft(ColorSelector, m_sampleX - ColorSelector.ActualWidth / 2);
			Canvas.SetTop(ColorSelector, m_sampleY - ColorSelector.ActualHeight / 2);
		}

		private double ColorFieldHeight
		{
			get
			{
				return ColorField.ActualHeight;
			}
		}

		private double ColorFieldWidth
		{
			get
			{
				return ColorField.ActualWidth;
			}
		}

		private double RainbowHeight
		{
			get
			{
				return Rainbow.ActualHeight;
			}
		}

		/// <summary>
		/// Event fired when the selected color changes.  This event occurs when the 
		/// left-mouse button is lifted after clicking.
		/// </summary>
		public event SelectedColorChangedHandler SelectedColorChanged;

		/// <summary>
		/// Event fired when the selected color is changing.  This event occurs when the 
		/// left-mouse button is pressed and the user is moving the mouse.
		/// </summary>
		public event SelectedColorChangingHandler SelectedColorChanging;

		private bool RainbowMouseCaptured;
		private bool ColorFieldMouseCaptured;
		private double m_huePos;
		private double m_sampleX;
		private double m_sampleY;

		#region SelectedColor Dependency Property
		/// <summary>
		/// Gets or sets the currently selected color in the Color Picker.
		/// </summary>
		public Color SelectedColor
		{
			get
			{
				return (Color)GetValue(SelectedColorProperty);
			}
			set
			{
				UpdateValuesFromColor(value);
				UpdateColorFieldBackground();
				UpdateColorSelector();
				UpdateHueSelector();
				SetValue(SelectedColorProperty, value);
				UpdateSelectedColorSample();
			}
		}

		private void UpdateValuesFromColor(Color value)
		{
			HSV hsv = ColorSpace.ConvertRgbToHsv(value);
			m_huePos = (hsv.Hue / 360 * RainbowHeight);
			m_sampleY = (1 - hsv.Value) * ColorFieldHeight;
			m_sampleX = hsv.Saturation * ColorFieldWidth;
		}

		/// <summary>
		/// SelectedColor Dependency Property.
		/// </summary>
		static public readonly DependencyProperty SelectedColorProperty =
			DependencyProperty.Register(
				"SelectedColor",
				typeof(Color),
				typeof(ColorPicker),
				new PropertyMetadata(Colors.Black, new PropertyChangedCallback(SelectedColorPropertyChanged)));

		private static void SelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ColorPicker p = d as ColorPicker;
			if (p != null && p.SelectedColorChanged != null)
			{
				SelectedColorEventArgs args = new SelectedColorEventArgs((Color)e.NewValue);
				p.SelectedColorChanged(p, args);
			}
		}

		private void FireSelectedColorChangingEvent(Color selectedColor)
		{
			if (SelectedColorChanging != null)
			{
				SelectedColorEventArgs args = new SelectedColorEventArgs(selectedColor);
				SelectedColorChanging(this, args);
			}
		}

		#endregion
	}
}
/// <summary>
/// Contains helper methods for use by the ColorPicker control.
/// </summary>
internal static class ColorSpace
{
    private const byte MIN = 0;
    private const byte MAX = 255;

    static public Color GetColorFromPosition(double position)
    {
        int gradientStops = 6;
        position *= gradientStops;
        byte mod = (byte)(position % MAX);
        byte diff = (byte)(MAX - mod);

        switch ((int)(position / MAX))
        {
            case 0: return Color.FromArgb(MAX, MAX, mod, MIN);
            case 1: return Color.FromArgb(MAX, diff, MAX, MIN);
            case 2: return Color.FromArgb(MAX, MIN, MAX, mod);
            case 3: return Color.FromArgb(MAX, MIN, diff, MAX);
            case 4: return Color.FromArgb(MAX, mod, MIN, MAX);
            case 5: return Color.FromArgb(MAX, MAX, MIN, diff);
            case 6: return Color.FromArgb(MAX, MAX, mod, MIN);
            default: return Colors.Black;
        }
    }

    static public string GetHexCode(Color c)
    {
        return string.Format("#{0}{1}{2}", 
            c.R.ToString("X2"), 
            c.G.ToString("X2"), 
            c.B.ToString("X2"));
    }

    /// <summary>
    /// Converts from Hue/Sat/Val (HSV) color space to Red/Green/Blue color space.
    /// Algorithm ported from: http://www.colorjack.com/software/dhtml+color+picker.html
    /// </summary>
    /// <param name="h">The Hue value.</param>
    /// <param name="s">The Saturation value.</param>
    /// <param name="v">The Value value.</param>
    /// <returns></returns>
    static public Color ConvertHsvToRgb(double h, double s, double v)
    {
        h = h / 360;
        if (s > 0)
        {
            if (h >= 1)
                h = 0;
            h = 6 * h;
            int hueFloor = (int)Math.Floor(h);
            byte a = (byte)Math.Round(MAX * v * (1.0 - s));
            byte b = (byte)Math.Round(MAX * v * (1.0 - (s * (h - hueFloor))));
            byte c = (byte)Math.Round(MAX * v * (1.0 - (s * (1.0 - (h - hueFloor)))));
            byte d = (byte)Math.Round(MAX * v);

            switch (hueFloor)
            {
                case 0: return Color.FromArgb(MAX, d, c, a);
                case 1: return Color.FromArgb(MAX, b, d, a);
                case 2: return Color.FromArgb(MAX, a, d, c);
                case 3: return Color.FromArgb(MAX, a, b, d);
                case 4: return Color.FromArgb(MAX, c, a, d);
                case 5: return Color.FromArgb(MAX, d, a, b);
                default: return Color.FromArgb(0, 0, 0, 0);
            }
        }
        else
        {
            byte d = (byte)(v * MAX);
            return Color.FromArgb(255, d, d, d);
        }
    }

    /// <summary>
    /// Converts from the Red/Green/Blue color space to the Hue/Sat/Val (HSV) color space.
    /// Algorithm ported from: http://www.codeproject.com/KB/recipes/colorspace1.aspx
    /// </summary>
    /// <param name="c">The color to convert.</param>
    /// <returns></returns>
    static public HSV ConvertRgbToHsv(Color c)
    {
        // normalize red, green and blue values

        double r = (c.R / 255.0);
        double g = (c.G / 255.0);
        double b = (c.B / 255.0);

        // conversion start

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));

        if (max == min)
        {
            return new HSV(0, 0, max);
        }

        double h = 0.0;
        if (max == r && g >= b)
        {
            h = 60 * (g - b) / (max - min);
        }
        else if (max == r && g < b)
        {
            h = 60 * (g - b) / (max - min) + 360;
        }
        else if (max == g)
        {
            h = 60 * (b - r) / (max - min) + 120;
        }
        else if (max == b)
        {
            h = 60 * (r - g) / (max - min) + 240;
        }

        double s = (max == 0) ? 0.0 : (1.0 - (min / max));

        return new HSV(h, s, max);
    }
}

/// <summary>
/// Data structure that represents a HSV value.
/// </summary>
internal struct HSV
{
    private readonly double m_hue;
    private readonly double m_saturation;
    private readonly double m_value;

    public HSV(double hue, double saturation, double value)
    {
        m_hue = hue;
        m_saturation = saturation;
        m_value = value;
    }

    public double Hue
    {
        get { return m_hue; }
    }

    public double Saturation
    {
        get { return m_saturation; }
    }

    public double Value
    {
        get { return m_value; }
    }
}

/// <summary>
/// Delegate for the SelectedColorChanged event.
/// </summary>
/// <param name="sender">The object instance that fired the event.</param>
/// <param name="e">The selected color event arguments for the event.</param>
public delegate void SelectedColorChangedHandler(object sender, SelectedColorEventArgs e);

/// <summary>
/// Delegate for the SelectedColorChanging event.
/// </summary>
/// <param name="sender">The object instance that fired the event.</param>
/// <param name="e">The selected color event arguments for the event.</param>
public delegate void SelectedColorChangingHandler(object sender, SelectedColorEventArgs e);


/// <summary>
/// Event data for the SelectedColorChanged event.
/// </summary>
public class SelectedColorEventArgs : EventArgs
{
	/// <summary>
	/// The currently selected color.
	/// </summary>
	public readonly Color SelectedColor;

	/// <summary>
	/// Create a new instance of the SelectedColorEventArgs class.
	/// </summary>
	/// <param name="color">The currently selected color.</param>
	public SelectedColorEventArgs(Color color)
	{
		this.SelectedColor = color;
	}


}
