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

namespace PoELauncher
{
    /// <summary>
    /// Logique d'interaction pour RepoDispControl.xaml
    /// </summary>
    /// 

    #region events
    public class VarEventClick : EventArgs
    {
        public int numButton;
        public int idApplication;
    }

    public class VarEventChecked : EventArgs
    {
        public int idApplication;
        public bool enabled;
    }

    public delegate void EventClick(object sender, VarEventClick e);
    public delegate void EventChecked(object sender, VarEventChecked e);
    #endregion

    public partial class RepoDispControl : UserControl
    {

        public RepoDispControl()
        {
            InitializeComponent();
            enableSwitch.IsEnabled = false;
        }

        #region properties
        public string applicationName { get; set; }
        public string applicationVersion { get; set; }
        public string applicationDescription { get; set; }

        private bool ApplicationEnabled;
        public bool applicationEnabled
        {
            get
            {
                return ApplicationEnabled;
            }
            set
            {
                ApplicationEnabled = value;
                enableSwitch.IsChecked = value;
            }
        }
        
        private bool IsUdpateAvailable;
        public bool isUdpateAvailable
        {
            get
            {
                return IsUdpateAvailable;
            }
            set
            {
                IsUdpateAvailable = value;
            }
        }

        public bool IsSwitchEnabled
        {
            get { return enableSwitch.IsEnabled; }
            set
            {
                enableSwitch.IsEnabled = value;
            }
        }

        public int applicationId { get; set; }

        public static DependencyProperty PourcentageDl = DependencyProperty.Register("pourcentageDl", typeof(double), typeof(UserControl));
        public double pourcentageDl
        {
            get { return (double)GetValue(PourcentageDl); }
            set { SetValue(PourcentageDl, value); }
        }

        public event EventClick ButtonClick;
        public event EventChecked SwitchChecked;
        #endregion

        public void forceCheckEnableSwitch(bool isChecked)
        {
            enableSwitch.IsChecked = isChecked;
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            VarEventClick var = new VarEventClick();
            var.numButton = 1;
            var.idApplication = applicationId;
            if (ButtonClick != null)
            {
                ButtonClick(this, var);
            }
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            VarEventClick var = new VarEventClick();
            var.numButton = 2;
            var.idApplication = applicationId;
            if (ButtonClick != null)
            {
                ButtonClick(this, var);
            }
        }

        private void enableSwitch_Checked(object sender, RoutedEventArgs e)
        {
            enableSwitch_IsCheckedChanged(true);
        }

        private void enableSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            enableSwitch_IsCheckedChanged(false);
        }

        private void enableSwitch_IsCheckedChanged(bool isChecked)
        {
            applicationEnabled = enableSwitch.IsChecked.Value;
            VarEventChecked var = new VarEventChecked();
            var.idApplication = applicationId;
            var.enabled = applicationEnabled;
            if (ButtonClick != null)
            {
                SwitchChecked(this, var);
            }
        }

    }
}

