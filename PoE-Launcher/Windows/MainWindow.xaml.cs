using Octokit;
using PoELauncher.Extensions;
using PoELauncher.Properties;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace PoELauncher
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {  
        public SolidColorBrush btnCloseColor { get; set; }
        public SolidColorBrush btnMinimizeColor { get; set; }

        Dictionary<int, GithubRepository> repositories = new Dictionary<int, GithubRepository>();

        Dictionary<int, RepoDispControl> dispControls = new Dictionary<int, RepoDispControl>();

        List<string> buttonsLinksList = new List<string>();

        private int customToolsStartId;
        private bool githubError = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        /// <summary>
        /// Initialization method
        /// </summary>
        private void InitializeApp()
        {
            try
            {
                if (Settings.Default.SettingsUpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.SettingsUpgradeRequired = false;
                    Settings.Default.Save();
                }

                btnCloseColor = new SolidColorBrush(Color.FromArgb(255, 231, 76, 60));
                btnMinimizeColor = new SolidColorBrush(Color.FromArgb(255, 241, 196, 15));

                readXML("data/data.xml");

                filter_All.IsSelected = true;

                try
                {
                    foreach (KeyValuePair<int, GithubRepository> repo in repositories)
                    {
                        var latest = Task.Run(() => GetVersion(repo.Value.Owner, repo.Value.App)).Result;
                        Settings.Default.ToolSettings[repo.Key].LatestVersion = latest.TagName;
                        Settings.Default.Save();
                        foreach (var asset in latest.Assets)
                        {
                            string downloadLink = asset.BrowserDownloadUrl;
                            if (downloadLink.Contains(".rar") || downloadLink.Contains(".zip") || downloadLink.Contains(".exe"))
                            {
                                repo.Value.latestVersionLink = downloadLink;
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to get data from Github, check your internet or try again later");
                    githubError = true;
                }
            }
            catch (Exception e)
            {
                    if (e.InnerException != null)
                    {
                        MessageBox.Show(e.InnerException.ToString());
                    }
                throw;
            }
        }

        /// <summary>
        /// Async task to get the latest version of a tool using octokit.net
        /// </summary>
        /// <param name="owner">Owner name on GitHub</param>
        /// <param name="app">App name on GitHub</param>
        /// <returns></returns>
        private static async Task<Release> GetVersion(string owner, string app)
        {
            var github = new GitHubClient(new ProductHeaderValue("CheckUpdate"));

            var latestRelease = await github.Repository.Release.GetLatest(owner, app).ConfigureAwait(false);

            return latestRelease;
        }

        /// <summary>
        /// Reads the main XML file and adds tools to the display
        /// </summary>
        /// <param name="path"></param>
        private void readXML(string path)
        {
            var toolSettingsList = new List<ToolSettings>();
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(path);
            XmlNodeList repos = myXmlDocument.SelectNodes("/data/repos/repo");
            int id = 0;

            foreach (XmlNode repo in repos)
            {
                string owner = repo["owner"].InnerText;
                string app = repo["app"].InnerText;
                string name = repo["name"].InnerText;
                string desc = repo["desc"].InnerText;
                string exe = repo["exe"].InnerText;
                Settings.Default.Reload();
                List<ToolSettings> settingsList = Settings.Default.ToolSettings;
                if (settingsList==null)
                {
                    Settings.Default.ToolSettings = new List<ToolSettings>();
                    Settings.Default.Save();
                }
                Settings.Default.Reload();
                settingsList = Settings.Default.ToolSettings;
                if (settingsList.Count <= id)
                {
                    Settings.Default.ToolSettings.Add(new ToolSettings());
                    Settings.Default.Save();
                }

                bool enabled = Settings.Default.ToolSettings[id].Enabled;
                bool downloaded = Settings.Default.ToolSettings[id].Downloaded;
                string currentVer = Settings.Default.ToolSettings[id].CurrentVersion;
                repositories.Add(id, new GithubRepository { Owner = owner, App = app, Name = name, Description = desc, Exe = exe });

                RepoDispControl dispControl = new RepoDispControl()
                {
                    applicationId = id,
                    applicationName = name,
                    applicationVersion = currentVer,
                    applicationDescription = desc,
                    applicationEnabled = enabled,
                    IsSwitchEnabled = downloaded
                };

                dispControl.ButtonClick += new EventClick(ButtonClick);
                dispControl.SwitchChecked += new EventChecked(SwitchChecked);
                dispControl.pourcentageDl = Convert.ToDouble(downloaded);
                dispControls.Add(id, dispControl);
                panelRepo.Children.Add(dispControl);
                id++;
            }
            XmlNodeList links = myXmlDocument.SelectNodes("/data/links/link");

            foreach (XmlNode link in links)
            {
                string name = link["name"].InnerText;
                string url = link["url"].InnerText;

                Button linkButton = new Button();
                linkButton.Content = name;
                linkButton.Click += (s, e) =>
                {
                    System.Diagnostics.Process.Start(url);
                };
                buttonsLinksList.Add(url);
                linkButtonsPanel.Children.Add(linkButton);
            }
            customToolsStartId = id + 1;
            readCustomXML("data/data_custom.xml");
        }

        /// <summary>
        /// Reads the custom tools xml file and add the found tools to the display
        /// </summary>
        /// <param name="filePath">Path to the XML file</param>
        private void readCustomXML(string filePath)
        {
            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(filePath);
            XmlNodeList repos = myXmlDocument.SelectNodes("/data/tools/tool");
            int id = customToolsStartId;
            foreach (XmlNode repo in repos)
            {
                string name = repo["name"].InnerText;
                string desc = repo["desc"].InnerText;
                string path = repo["path"].InnerText;

                RepoDispControl dispControl = new RepoDispControl()
                {
                    applicationId = id,
                    applicationName = name,
                    applicationDescription = desc,
                    applicationEnabled = true,
                };

                dispControl.ButtonClick += new EventClick(ButtonClickCustom);
                dispControl.SwitchChecked += new EventChecked(SwitchCheckedCustom);
                dispControl.pourcentageDl = 1.0;
                dispControl.IsSwitchEnabled = true;
                dispControls.Add(id, dispControl);
                panelRepo.Children.Add(dispControl);
                id++;
            }
        }

        /// <summary>
        /// Async Task to download a file.
        /// </summary>
        /// <param name="id">Id of the tool to download</param>
        /// <returns></returns>
        private async Task download(int id)
        {
            string downloadLink = repositories[id].latestVersionLink, fileName;
            string extension = System.IO.Path.GetExtension(downloadLink);

            fileName = repositories[id].App + extension;

            using (var client = new HttpClientDownloadWithProgress(downloadLink, fileName))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        dispControls[id].pourcentageDl = Convert.ToDouble(progressPercentage);
                    }));
                };

                client.DownloadCompleted += () =>
                {
                    try
                    {
                        unzip(id, extension);
                        Settings.Default.ToolSettings[id].Downloaded = true; // = new ToolSettings { Downloaded = true };
                        Settings.Default.ToolSettings[id].CurrentVersion = Settings.Default.ToolSettings[id].LatestVersion;
                        Settings.Default.Save();
                        dispControls[id].applicationVersion = Settings.Default.ToolSettings[id].CurrentVersion;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            dispControls[id].IsSwitchEnabled = true;
                            dispControls[id].forceCheckEnableSwitch(true);
                        }));
                    }
                    catch (Exception )
                    {
                        MessageBox.Show("Error while extracting the downloaded file.");
                    }
                };

                await client.StartDownload();
            }
        }

        /// <summary>
        /// Extracts the downloaded file
        /// </summary>
        /// <param name="id">Id of the downloaded app</param>
        /// <param name="extension">Extension of the downloaded app</param>
        private void unzip(int id, string extension)
        {
            string app = repositories[id].App;
            string filePath = System.AppDomain.CurrentDomain.BaseDirectory + app + extension;
            string extractPath = System.AppDomain.CurrentDomain.BaseDirectory + app+@"\";

            uninstallApp(id);
            switch (extension)
            {
                case ".rar":
                    using (var archive = RarArchive.Open(filePath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(extractPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                    break;
                case ".zip":
                    ZipFile.ExtractToDirectory(filePath, extractPath);
                    File.Delete(filePath);
                    break;
                case ".exe":
                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    File.Move(filePath, extractPath + app + extension);
                    break;
                default:
                    MessageBox.Show("File type not supported");
                    break;
            }
            //MessageBox.Show(app + " succesfully extracted");
        }

        /// <summary>
        /// Delete an installed app
        /// </summary>
        /// <param name="id">Id of the app</param>
        private void uninstallApp(int id)
        {
            string app = repositories[id].App;
            if (Directory.Exists(app))
            {
                try
                {
                    Directory.Delete(app, true);
                    Settings.Default.ToolSettings[id].Downloaded = false;
                    Settings.Default.Save();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        dispControls[id].IsSwitchEnabled = false;
                        dispControls[id].forceCheckEnableSwitch(false);
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #region Window

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            btnCloseColor.Color = (Color)ColorConverter.ConvertFromString("#FFE53935");

            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            btnCloseColor.Color = (Color)ColorConverter.ConvertFromString("#FFF44336");
            Mouse.OverrideCursor = null;
        }

        private void Minimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Minimize_MouseEnter(object sender, MouseEventArgs e)
        {
            btnMinimizeColor.Color = (Color)ColorConverter.ConvertFromString("#FFFDD835");
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Minimize_MouseLeave(object sender, MouseEventArgs e)
        {
            btnMinimizeColor.Color = (Color)ColorConverter.ConvertFromString("#FFFFEB3B");
            Mouse.OverrideCursor = null;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void CanvasTitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();

            CheckBoxStartPoE.IsChecked = Settings.Default.LaunchPoE;
            CheckBoxCloseOnExit.IsChecked = Settings.Default.CloseToolsOnExit;

            List<ToolSettings> settings = Settings.Default.ToolSettings;
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    dispControls[settings.IndexOf(setting)].IsSwitchEnabled = setting.Downloaded;
                    dispControls[settings.IndexOf(setting)].applicationEnabled = setting.Enabled;
                    dispControls[settings.IndexOf(setting)].applicationVersion = setting.CurrentVersion;
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.LaunchPoE = CheckBoxStartPoE.IsChecked.Value;
            Properties.Settings.Default.CloseToolsOnExit = CheckBoxCloseOnExit.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region repoDisp

        private void ButtonClick(object sender, VarEventClick e)
        {
            int id = e.idApplication;
            BackgroundWorker workerDownload = new BackgroundWorker();

            workerDownload.DoWork += async delegate (object s, DoWorkEventArgs args)
            {
                await download(id);
                args.Result = id;
            };

            if (e.numButton == 2)
            {
                uninstallApp(id);
            }
            else
            {
                if (!githubError)
                {
                    workerDownload.RunWorkerAsync(id);
                }
                else
                {
                    MessageBox.Show("Impossible to download tools if the app couldn't get the data from github at launch.");
                }
            }

            e.numButton = 0;
        }

        private void ButtonClickCustom(object sender, VarEventClick e)
        {
            MessageBox.Show(e.numButton.ToString());
            int id = e.idApplication;
            if (e.numButton == 2)
            {
                uninstallApp(id);
                //MessageBox.Show("delete");
            }
            e.numButton = 0;
        }

        private void SwitchChecked(object sender, VarEventChecked e)
        {
            Settings.Default.ToolSettings[e.idApplication].Enabled = e.enabled; // = new ToolSettings { Enabled = e.enabled, Downloaded = Settings.Default.ToolSettings[e.idApplication].Downloaded,CurrentVersion };
            Settings.Default.Save();
        }

        private void SwitchCheckedCustom(object sender, VarEventChecked e)
        {
        }
        #endregion

        #region ClickEvents
        private void MaterialButtonLauncher_Click(object sender, RoutedEventArgs e)
        {
            foreach (var repo in repositories)
            {
                if (Settings.Default.ToolSettings[repo.Key].Enabled)
                {
                    var allFiles = Directory.GetFiles(repo.Value.App, "*.exe", SearchOption.AllDirectories);
                    foreach (string file in allFiles)
                    {
                        if (file.Contains(repo.Value.Exe))
                        {
                            Process.Start(file);
                        }
                    }
                }
            }
            if (Settings.Default.LaunchPoE)
            {
                string processName = "PathOfExileSteam";

                Process gameProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("steam://rungameid/238960"),
                };
                gameProcess.Start();
                Process[] process = Process.GetProcessesByName(processName);
                Timer timerProcess = new Timer(3000);
                timerProcess.Elapsed += TimerProcess_Elapsed;
                timerProcess.Start();
            }
        }

        private void TimerProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            string processName = "PathOfExileSteam";
            //string processName = "notepad";
            if (!(Process.GetProcessesByName(processName).Length > 0))
            {
                foreach (var tool in repositories.Values)
                {
                    foreach (var process in Process.GetProcessesByName(tool.Exe))
                    {
                        process.Kill();
                    }
                }
            }
        }

        private void AllWebsitesButtonLauncher_Click(object sender, RoutedEventArgs e)
        {
            foreach (string link in buttonsLinksList)
            {
                System.Diagnostics.Process.Start(link);
            }
        }

        private void AddCustomToolButton_Click(object sender, RoutedEventArgs e)
        {
            CustomWindow AddCustomWindow = new CustomWindow();
            AddCustomWindow.ShowInTaskbar = true;
            AddCustomWindow.Topmost = true;
            AddCustomWindow.Left = mainWindow.Left + ((mainWindow.Width - AddCustomWindow.Width) / 2);
            AddCustomWindow.Top = mainWindow.Top + ((mainWindow.Height - AddCustomWindow.Height) / 2);
            AddCustomWindow.ShowDialog();
        }

        private void CheckBoxStartPoE_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LaunchPoE = CheckBoxStartPoE.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void CheckBoxCloseOnExit_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CloseToolsOnExit = CheckBoxStartPoE.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        #endregion

        private void filter_listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (filter_listbox.SelectedIndex)
            {
                case -1:
                    filter_All.IsSelected = true;
                    break;
                case 0:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        control.Visibility = Visibility.Visible;
                    }
                    break;
                case 1:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        if (!control.IsSwitchEnabled)
                        {
                            control.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 2:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        if (control.IsSwitchEnabled)
                        {
                            control.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 3:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        if (!control.applicationEnabled)
                        {
                            control.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 4:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        if (control.applicationEnabled)
                        {
                            control.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                case 5:
                    foreach (RepoDispControl control in dispControls.Values)
                    {
                        if (control.applicationId < customToolsStartId)
                        {
                            control.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            control.Visibility = Visibility.Visible;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void filter_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter_listbox.SelectedIndex = 0;
            foreach (RepoDispControl control in dispControls.Values)
            {
                if (control.applicationName.ToLower().Contains(filter_Search.Text.ToLower()))
                {
                    control.Visibility = Visibility.Visible;
                }
                else
                {
                    control.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
