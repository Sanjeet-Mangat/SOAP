/*
* FILE : ParameterEntry.xaml.cs
* PROJECT : PROG3080 - Assignment #1
* PROGRAMMER : Constantine Grigoriadis, Sunny Mangat, Dylan Sawchuk, Nick Whitey
* FIRST VERSION : 2014-11-28
* DESCRIPTION : contains the user control for entry of parameter values
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace client
{
    /// <summary>
    /// Interaction logic for ParameterEntry.xaml
    /// </summary>
    public partial class ParameterEntry : UserControl
    {
        /// <summary>
        /// initialises wpf components
        /// </summary>
        public ParameterEntry ()
        {
            InitializeComponent ();
        }
    }
}
