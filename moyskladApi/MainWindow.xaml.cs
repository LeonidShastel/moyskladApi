using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
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

namespace moyskladApi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Api api = new Api();
        public MainWindow()
        {
            InitializeComponent();
            foreach(string option in api.GetTemplate()) 
                optionsOwner.Items.Add(option);
            foreach (string printerName in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                optionsPrinters.Items.Add(printerName);
            }
            optionsOwner.SelectedIndex = 0;
            optionsPrinters.SelectedIndex = 0;
            api.selectedTemplate = 0;
            
        }

        private async void textBoxProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && textBoxProductCode.Text != "" && optionsOwner.SelectedIndex!=-1)
            {
                await Task.Run(()=>api.GetOwner(textBoxProductCode.Text));
            }
            else if (e.Key == Key.Enter && textBoxProductCode.Text == "")
                MessageBox.Show("Не введен номер заказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void optionsOwner_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            api.selectedTemplate = optionsOwner.SelectedIndex;
        }

        private void optionsPrinters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            api.selectedPrinter = optionsPrinters.SelectedValue.ToString();
        }
    }
}
