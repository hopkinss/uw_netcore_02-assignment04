using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Linq;
using Assignment04.ViewModel;

namespace Assignment04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
            this.Title = "Shawn Hopkins, Assignment 04: ZipCode TextBox";
        }

        public async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadZipAsync();
        }
    }
}
