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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Draw3DSurface
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel();

            hview.Camera.UpDirection = new Vector3D(-0.061, -0.106, 0.992);
            hview.Camera.LookDirection = new Vector3D(9.608, 16.596, -13.317);
            hview.Camera.Position = new Point3D(-4.608, -6.596, 3.317);
        }
    }
}
