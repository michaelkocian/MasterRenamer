using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasterRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            _editor.Text = "select folder";

            _editor.MouseRightButtonDown += _editor_MouseRightButtonDown;
            _editor.MouseRightButtonUp += _editor_MouseRightButtonUp;

           // _editor.
                
        }

        private void _editor_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void _editor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        string[] fileNames;

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var path = (e.NewValue as System.Windows.Controls.TreeViewItem)?.Tag as string;
            fileNames = Directory.GetFiles(path);

            var toBeShown = string.Join("\n", fileNames);
            _editor.Text = toBeShown;
        }

        private object dummyNode = null;


        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result1 = MessageBox.Show("Do you want to save changes?",
             "Important Question", MessageBoxButton.YesNo);

            if (result1 == MessageBoxResult.Yes)
            {
                Save();
            }
        }

        private void Save()
        {
            var newFileNames = _editor.Text.Split('\n');
            if (newFileNames.Length != fileNames.Length)
                throw new Exception("different count of lines");

            for (int i = 0; i < fileNames.Length; i++)
            {
                if(fileNames[i] != newFileNames[i])
                    File.Move(fileNames[i], newFileNames[i]);
            }

            
        }
    }
    }
