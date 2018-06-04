using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using swf = System.Windows.Forms;

namespace PoELauncher
{
    /// <summary>
    /// Logique d'interaction pour CustomWindow.xaml
    /// </summary>
    public partial class CustomWindow : Window
    {
        string toolName, toolPath, toolDesc;
        public CustomWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameInputBox.Text != String.Empty && DescInputBox.Text != String.Empty && FileOK.IsChecked == true)
            {
                toolName = NameInputBox.Text;
                toolDesc = DescInputBox.Text;
                //MessageBox.Show(toolName + "\n" + toolPath + "\n" + toolDesc);
                var tool = new CustomTool() { name = toolName, path = toolPath, desc = toolDesc };
                WriteCustomXML(tool);
                this.Close();
            }
            else
            {
                MessageBox.Show("Fill");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void WriteCustomXML(CustomTool tool)
        {
            string xmlPath = @"data/data_custom.xml";

            XDocument xDocument = XDocument.Load(xmlPath);
            XElement root = xDocument.Element(@"tools");
            IEnumerable<XElement> rows = root.Descendants("tool");
            if (rows.Count()==0)
            {
                root.Add(new XElement("tool",
                    new XElement("name", tool.name),
                    new XElement("desc", tool.desc),
                    new XElement("path", tool.path)));
            }
            else
            {
                XElement lastRow = rows.Last();
                lastRow.AddAfterSelf(
                   new XElement("tool",
                   new XElement("name", tool.name),
                   new XElement("desc", tool.desc),
                   new XElement("path", tool.path)));
            }
            xDocument.Save(xmlPath);
        }

        public class CustomTool
        {
            public string name { get; set; }
            public string path { get; set; }
            public string desc { get; set; }
        }
        
        private void PathBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            swf.OpenFileDialog pathbox1 = new swf.OpenFileDialog();
            pathbox1.Filter = "exe (*.exe)|*.exe";
            pathbox1.FilterIndex = 1;
            pathbox1.FileOk += (s,eF) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    FileOK.IsChecked = true;
                }));
            };
            pathbox1.ShowDialog();
            toolPath = pathbox1.FileName;  
        }
    }
}
