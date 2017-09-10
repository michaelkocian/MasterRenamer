using ICSharpCode.AvalonEdit;
using System;
using System.Diagnostics;
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

        int CurrentLine { get { return _editor.Document.GetLineByOffset(_editor.CaretOffset).LineNumber - 1; } }
        string SelectedFile { get{ return fileNames[CurrentLine]; } }

        public MainWindow()
        {
            InitializeComponent();
            _editor.Text = "select folder";
            _editor.TextArea.TextEntering += TextArea_TextEntering;
            _editor.MouseRightButtonDown += _editor_MouseRightButtonDown;
        }

        private void _editor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = _editor.GetPositionFromPoint(e.GetPosition(_editor));
            if (position.HasValue)
            {
                _editor.TextArea.Caret.Position = position.Value;
            }
        }


        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\n")
                e.Handled = true;
        }

        string[] fileNames;
        string path;

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            path = (e.NewValue as System.Windows.Controls.TreeViewItem)?.Tag as string;
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
            Save();
        }

        


        private void Save()
        {
            MessageBoxResult result1 = MessageBox.Show("Do you want to save changes?",
                "Save changes", MessageBoxButton.YesNo);

            if (result1 == MessageBoxResult.Yes)
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", $"/select, \"{SelectedFile}\"");
        }
    }
}
