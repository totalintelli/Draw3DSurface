// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   No color coding, use coloured lights
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Distributions.Fitting;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Draw3DSurface
{
    // http://reference.wolfram.com/mathematica/tutorial/ThreeDimensionalSurfacePlots.html

    public enum ColorCoding
    {
        /// <summary>
        /// No color coding, use coloured lights
        /// </summary>
        ByLights,

        /// <summary>
        /// Color code by gradient in y-direction using a gradient brush with white ambient light
        /// </summary>
        ByGradientY
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }

        public Func<double, double, double> Function { get; set; }
        public Point3D[,] Data { get; set; }
        public double[,] ColorValues { get; set; }

        public ColorCoding ColorCoding { get; set; }

        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                        group.Children.Add(new AmbientLight(Colors.White));
                        break;
                    case ColorCoding.ByLights:
                        group.Children.Add(new AmbientLight(Colors.Gray));
                        group.Children.Add(new PointLight(Colors.Red, new Point3D(0, -1000, 0)));
                        group.Children.Add(new PointLight(Colors.Blue, new Point3D(0, 0, 1000)));
                        group.Children.Add(new PointLight(Colors.Green, new Point3D(1000, 1000, 0)));
                        break;
                }
                return group;
            }
        }

        public Brush SurfaceBrush
        {
            get
            {
                // Brush = BrushHelper.CreateGradientBrush(Colors.White, Colors.Blue);
                // Brush = GradientBrushes.RainbowStripes;
                // Brush = GradientBrushes.BlueWhiteRed;
                switch (ColorCoding)
                {
                    case ColorCoding.ByGradientY:
                        return BrushHelper.CreateGradientBrush(Colors.Red, Colors.White, Colors.Blue);
                    case ColorCoding.ByLights:
                        return Brushes.White;
                }
                return null;
            }
        }

        public MainViewModel()
        {
            // 매트랩 예제 데이터를 가지고 데이터를 설정한다.
            SetSampleData();

            //// hdata.csv 파일에 있는 데이터를 가지고 데이터를 설정한다.
            //SetHData();
        }

        /// <summary>
        /// hdata.csv 파일에 있는 데이터를 가지고 데이터를 설정한다.
        /// </summary>
        private void SetHData()
        {
            List<double> xList = new List<double>();
            List<double> yList = new List<double>();

            StreamReader sr = new StreamReader(@"d:\hdata.csv", Encoding.GetEncoding("euc-kr"));
            while (!sr.EndOfStream)
            {
                string s = sr.ReadLine();
                string[] temp = s.Split(',');        // Split() 메서드를 이용하여 ',' 구분하여 잘라냄
                xList.Add(Convert.ToDouble(temp[1]));
                yList.Add(Convert.ToDouble(temp[2]));
            }
            double[] x = xList.ToArray();
            double[] y = yList.ToArray();

            MinX = xList.Min();
            MaxX = xList.Max();
            MinY = yList.Min();
            MaxY = yList.Max();

            Rows = x.Length;
            Columns = y.Length;

            double[][] observations = new double[Rows][];
            observations = MakeObservations(x, y);

            // Estimate a new Multivariate Normal Distribution from the observations
            MultivariateNormalDistribution dist = MultivariateNormalDistribution.Estimate(observations, new NormalOptions()
            {
                Regularization = 1e-10 // this value will be added to the diagonal until it becomes positive-definite
            });
            ColorCoding = ColorCoding.ByGradientY;
            UpdateModel(observations);
        }

        /// <summary>
        /// 매트랩 예제 데이터를 가지고 데이터를 설정한다.
        /// </summary>
        private void SetSampleData()
        {
            double[] x = new double[] { -3, -2.8, -2.6, -2.4, -2.2, -2.0, -1.8, -1.6, -1.4, -1.2, -1.0 };
            double[] y = new double[] { -3, -2.8, -2.6, -2.4, -2.2, -2.0, -1.8, -1.6, -1.4, -1.2, -1.0 };

            MinX = -3;
            MaxX = -1;
            MinY = -3;
            MaxY = -1;
            Rows = x.Length;
            Columns = y.Length;

            double[][] observations = new double[Rows][];
            observations = MakeObservations(x, y);

            // Estimate a new Multivariate Normal Distribution from the observations
            MultivariateNormalDistribution dist = MultivariateNormalDistribution.Estimate(observations, new NormalOptions()
            {
                Regularization = 1e-10 // this value will be added to the diagonal until it becomes positive-definite
            });
            ColorCoding = ColorCoding.ByGradientY;
            UpdateModel(observations);
        }

        /// <summary>
        /// hdata를 (x, y) 형태의 측정값으로 만든다.
        /// </summary>
        /// <param name="x">hdata의 한 열</param>
        /// <param name="y">hdata의 다른 한 열</param>
        /// <returns></returns>
        private double[][] MakeObservations(double[] x, double[] y)
        {
            int ROWS = x.Length;
            int COLUMNS = y.Length;
            double[][] observations = new double[ROWS * COLUMNS][];
            int k = 0; // 측정값 인덱스
            for (int i = 0; i < ROWS; i++)
            {
                double oneY = y[i];
                for (int j = 0; j < COLUMNS; j++)
                {
                    double oneX = x[j];
                    observations[k] = new double[] { oneX, oneY };
                    k++;
                }
            }
            return observations;
        }

        private void UpdateModel(double[][] values)
        {
            Data = CreateDataArray(values);
            switch (ColorCoding)
            {
                case ColorCoding.ByGradientY:
                    ColorValues = FindGradientY(Data);
                    break;
                case ColorCoding.ByLights:
                    ColorValues = null;
                    break;
            }
            RaisePropertyChanged("Data");
            RaisePropertyChanged("ColorValues");
            RaisePropertyChanged("SurfaceBrush");
        }

        public Point GetPointFromIndex(int i, int j)
        {
            double x = MinX + (double)j / (Columns - 1) * (MaxX - MinX);
            double y = MinY + (double)i / (Rows - 1) * (MaxY - MinY);
            return new Point(x, y);
        }

        public Point3D[,] CreateDataArray(double[][] observations)
        {
            var data = new Point3D[Rows, Columns];
            // Estimate a new Multivariate Normal Distribution from the observations
            MultivariateNormalDistribution dist = MultivariateNormalDistribution.Estimate(observations);

            // Probability density를 구한다.
            double[,] probabilityDensities = Calculation.GetProbabilityDensity(observations, dist);
            int k = 0; // 측정값의 인덱스
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    data[i, j] = new Point3D(observations[k][0], observations[k][1], probabilityDensities[k,j]);
                    k++;
                }
            }
            return data;
        }

        // http://en.wikipedia.org/wiki/Numerical_differentiation
        public double[,] FindGradientY(Point3D[,] data)
        {
            int n = data.GetUpperBound(0) + 1;
            int m = data.GetUpperBound(0) + 1;
            var K = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    // Finite difference approximation
                    var p10 = data[i + 1 < n ? i + 1 : i, j - 1 > 0 ? j - 1 : j];
                    var p00 = data[i - 1 > 0 ? i - 1 : i, j - 1 > 0 ? j - 1 : j];
                    var p11 = data[i + 1 < n ? i + 1 : i, j + 1 < m ? j + 1 : j];
                    var p01 = data[i - 1 > 0 ? i - 1 : i, j + 1 < m ? j + 1 : j];

                    //double dx = p01.X - p00.X;
                    //double dz = p01.Z - p00.Z;
                    //double Fx = dz / dx;

                    double dy = p10.Y - p00.Y;
                    double dz = p10.Z - p00.Z;

                    K[i, j] = dz / dy;
                }
            return K;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

    }
}