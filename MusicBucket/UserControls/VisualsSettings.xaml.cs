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
using System.Windows.Shapes;
using System.Threading;

namespace MusicBucket.UserControls
{
    /// <summary>
    /// Interaktionslogik für VisualsSettings.xaml
    /// </summary>
    public partial class VisualsSettings : Window
    {
        private GridWidthAnimation _gridWidthAnim,_backup;
        public VisualsSettings(GridWidthAnimation gridWidthAnim)
        {
            InitializeComponent();
            _gridWidthAnim = gridWidthAnim;
            _backup = gridWidthAnim.Clone();
            txtSharpness.Text = _gridWidthAnim.Sharpness.ToString();
            txtFrequency.Text = _gridWidthAnim.Frequency.ToString();
            txtBouncyness.Text = _gridWidthAnim.Bouncyness.ToString();
        }

        public new GridWidthAnimation DialogResult
        {
            get
            {
                return _gridWidthAnim;
            }
        }

        private void txtBounceFunction_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                DrawBounceFunction();
            }
        }

        private void DrawBounceFunction()
        {
            Line line;
            Polyline pline;
            double offset = 14;
            double sharpness, bouncyness, currwidth, currheight;
            int frequency;
            Point pt;
            GridWidthAnimation gwa = new GridWidthAnimation();
            try
            {
                if (!(Double.TryParse(txtSharpness.Text, out sharpness) && Int32.TryParse(txtFrequency.Text, out frequency) && Double.TryParse(txtBouncyness.Text, out bouncyness)))
                {
                    bouncyness = 0.0;
                    sharpness = 0.72;
                    frequency = 3;

                }
            }
            catch
            {
                bouncyness = 0.0;
                sharpness = 0.72;
                frequency = 3;
            }
            try
            {
                gwa.Bouncyness = bouncyness;
                gwa.Sharpness = sharpness;
                gwa.Frequency = frequency;
            }
            catch
            {
                bouncyness = 0.0;
                sharpness = 0.72;
                frequency = 3;
            }
            gwa.Bouncyness = bouncyness;
            gwa.Sharpness = sharpness;
            gwa.Frequency = frequency;
            currwidth = graphicCanvas.ActualWidth;
            currheight = graphicCanvas.ActualHeight;

            graphicCanvas.Children.Clear();

            // draw x axis
            line = new Line();
            line.Stroke = Brushes.Black;
            line.X1 = 0;
            line.Y1 = currheight - offset;
            line.X2 = currwidth;
            line.Y2 = currheight - offset;
            graphicCanvas.Children.Add(line);

            // draw y axis
            line = new Line();
            line.Stroke = Brushes.Black;
            line.X1 = offset;
            line.Y1 = currheight;
            line.X2 = offset;
            line.Y2 = 0;
            graphicCanvas.Children.Add(line);

            // draw function
            pline = new Polyline();
            pline.Stroke = Brushes.BlueViolet;
            for (int k = 0; k < 128; k++)
            {
                pt = new Point();
                pt.Y = gwa.BounceFunction2((double)k / 128.0, offset, currheight - offset, gwa.Sharpness, gwa.Frequency, gwa.Bouncyness);
                pt.X = offset + (currwidth - offset) / 128.0 * k;
                pline.Points.Add(pt);
            }
            graphicCanvas.Children.Add(pline);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _gridWidthAnim = _backup.Clone();
            this.Close();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            double sharpness, bouncyness;
            int frequency;
            if ((Double.TryParse(txtSharpness.Text, out sharpness) && Int32.TryParse(txtFrequency.Text, out frequency) && Double.TryParse(txtBouncyness.Text, out bouncyness)))
            {
                try
                {
                    _gridWidthAnim.Bouncyness = bouncyness;
                    _gridWidthAnim.Sharpness = sharpness;
                    _gridWidthAnim.Frequency = frequency;

                    this.Close();
                }
                catch (Exception exc)
                {
                    mdisp.DisplayErrorMessage(exc.Message);
                }

            }
            else
            {
                mdisp.DisplayErrorMessage(String.Format(Properties.Resources.VisualsSettingsNumbersNotParsed,Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            }
        }

        private void graphicCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                DrawBounceFunction();
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                DrawBounceFunction();
            }
        }
    }
}
