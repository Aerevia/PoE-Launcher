using PoELauncher.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    #region events

    public delegate void EventClick(object sender, VarEventClick e);
    public delegate void EventChecked(object sender, VarEventChecked e);
    #endregion

    public partial class RepoDispControl : UserControl, INotifyPropertyChanged
    {
        public RepoDispControl()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region properties
        private string _ApplicationName;
        public string ApplicationName
        {
            get
            {
                return _ApplicationName;
            }
            set
            {
                _ApplicationName = value;
                NotifyPropertyChanged();
            }
        }

        private string _ApplicationVersion;
        public string ApplicationVersion 
        {
            get
            {
                return _ApplicationVersion;
            }
            set
            {
                _ApplicationVersion = value;
                NotifyPropertyChanged();
            } 
        }

        private string _ApplicationDescription;

        public string ApplicationDescription
        {
            get
            {
                return _ApplicationDescription;
            }
            set
            {
                _ApplicationDescription = value;
                NotifyPropertyChanged();
            }
        }


        private bool _ApplicationEnabled;
        public bool ApplicationEnabled
        {
            get
            {
                return _ApplicationEnabled;
            }
            set
            {
                _ApplicationEnabled = value;
                enableSwitch.IsChecked = value;
                NotifyPropertyChanged();
            }
        }
        
        private bool _IsUdpateAvailable;
        public bool IsUdpateAvailable
        {
            get
            {
                return _IsUdpateAvailable;
            }
            set
            {
                _IsUdpateAvailable = value;
                NotifyPropertyChanged();
            }
        }
        private bool _IsDownloaded;
        public bool IsDownloaded
        {
            get { return _IsDownloaded; }
            set
            {
                _IsDownloaded = value;
                NotifyPropertyChanged();
            }
        }

        public int ApplicationId { get; set; }

        //public static DependencyProperty _DownloadPercent = DependencyProperty.Register("DownloadPercent", typeof(double), typeof(UserControl));
        private double _DownloadPercent;
        public double DownloadPercent
        {
            get { return _DownloadPercent; }
            set 
            {
                _DownloadPercent = value;
                NotifyPropertyChanged();
            }
        }

        public event EventClick ButtonClick;
        public event EventChecked SwitchChecked;
        #endregion

        public void ForceCheckEnableSwitch(bool isChecked)
        {
            enableSwitch.IsChecked = isChecked;
        }

        private void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            VarEventClick var = new VarEventClick();
            var.numButton = 1;
            var.idApplication = ApplicationId;
            if (ButtonClick != null)
            {
                ButtonClick(this, var);
            }
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            VarEventClick var = new VarEventClick();
            var.numButton = 2;
            var.idApplication = ApplicationId;
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
            ApplicationEnabled = enableSwitch.IsChecked.Value;
            VarEventChecked var = new VarEventChecked();
            var.idApplication = ApplicationId;
            var.enabled = ApplicationEnabled;
            if (ButtonClick != null)
            {
                SwitchChecked(this, var);
            }
        }
    }
}