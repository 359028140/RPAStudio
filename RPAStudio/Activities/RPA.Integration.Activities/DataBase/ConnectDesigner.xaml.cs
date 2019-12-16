﻿using Plugins.Shared.Library;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using System.Activities.Expressions;
using System.Diagnostics;
using System.Activities.Presentation.View;
using System.Windows.Forms;

namespace RPA.Integration.Activities.Database
{
    public partial class ConnectDesigner
    {
        ConnectSettingDialog dialog;
        public ConnectDesigner()
        {
            InitializeComponent();
        }
        private void PathSelect(object sender, RoutedEventArgs e)
        {
            ConnectionDialog connDialog = new ConnectionDialog(this.ModelItem);
            connDialog.Show();
        }
 
    }
}