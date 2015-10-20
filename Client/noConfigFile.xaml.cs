/*
* FILE : noConfigFile.xaml.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : contains the popup shown when the config file doesnt exist or cantain neccesarry values
*/

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

namespace client
{
    /// <summary>
    /// Interaction logic for noConfigFile.xaml
    /// </summary>
    public partial class noConfigFile : Window
    {
        /// <summary>
        /// initialises wpf components
        /// </summary>
        public noConfigFile ()
        {
            InitializeComponent ();
        }

        /// <summary>
        /// closes the popup
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void closeButton_Click (object sender, RoutedEventArgs e)
        {
            Close ();
        }
    }
}
