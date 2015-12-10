using Livet;
using MetroTrilithon.Lifetime;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hedspi_capcom.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IDisposableHolder
    {
        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();

        public MainWindow()
        {
            InitializeComponent();
        }

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        public void Dispose()
        {
            this.compositeDisposable.Dispose();
        }
    }
}
