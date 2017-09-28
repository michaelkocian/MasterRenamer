using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace MasterRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] FileNames = new string[0];
        string GetStringFromFileNames => string.Join("\n", FileNames);
        string CurrentPath { get; set; }
        int CurrentLine => _editor.Document.GetLineByOffset(_editor.CaretOffset).LineNumber - 1;
        string SelectedFile => FileNames[CurrentLine];
        string[] NewFileNames => _editor.Text.Split(new string[] { "\n" }, options:StringSplitOptions.RemoveEmptyEntries);
        bool HasChangedNames => FileNames.Length == 0 ? false : !FileNames.SequenceEqual(NewFileNames);
        

        private MyBackgroundRenderer backgroundRenderer;


        public MainWindow()
        {
            InitializeComponent();
            backgroundRenderer = new MyBackgroundRenderer(_editor);
            _editor.TextArea.TextView.BackgroundRenderers.Add(backgroundRenderer);
            _editor.Text = "Select folder.";
            _editor.TextArea.TextEntering += TextArea_TextEntering;
            _editor.MouseRightButtonDown += _editor_MouseRightButtonDown;
            _editor.MouseLeftButtonDown += (s, a) => {
                backgroundRenderer.Line = 0;
                _editor.TextArea.TextView.InvalidateVisual();
            };
        }


        private void _editor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = _editor.GetPositionFromPoint(e.GetPosition(_editor));
            if (position.HasValue)
            {
                _editor.TextArea.Caret.Position = position.Value;
            }
            backgroundRenderer.Line = CurrentLine + 1;
            _editor.TextArea.TextView.InvalidateVisual();
        }


        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\n")
                e.Handled = true;
        }

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (HasChangedNames)
                Toast.Show("Changes thrown away.", Toast.Type.Warn);

            CurrentPath = (e.NewValue as TreeViewItem)?.Tag as string;

            this.Title = "Master Renamer - " + CurrentPath;
            if (CurrentPath == "\\\\")
            {
                _editor.Text = "Expand network shared folders (SMB).";
                FileNames = new string[0];
            }
            else if (CurrentPath.StartsWith("\\\\") && CurrentPath.Count(f => f == '\\') == 2)
            {
                _editor.Text = $"Expand server {CurrentPath}.";
                FileNames = new string[0];
            }
            else
                ShowFiles(CurrentPath);
            backgroundRenderer.Line = 0;
        }

        void ShowFiles(string path)
        {
            if (!Utils.TryWithToast(() => FileNames = Directory.GetFiles(path)))
                return;
            _editor.Text = GetStringFromFileNames;
            if (string.IsNullOrEmpty(_editor.Text))
            {
                int x = Directory.GetDirectories(CurrentPath).Length;
                if (x == 0)
                    _editor.Text = "This folder is empty";
                else
                    _editor.Text = $"This folder contains {x} folders but no file.";
            }

        }

        private object dummyNode = null;

        void network_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            e.Handled = true;
            item.Items.Clear();
            DirectoryEntry winNtDirectoryEntries = new System.DirectoryServices.DirectoryEntry("WinNT:");
            List<String> computerNames = (from DirectoryEntry availDomains in winNtDirectoryEntries.Children
                                          from DirectoryEntry pcNameEntry in availDomains.Children
                                          where pcNameEntry.SchemaClassName.ToLower().Contains("computer")
                                          select pcNameEntry.Name).ToList();

            Utils.TryWithToast(() =>
            {
                foreach (string s in computerNames)
                {
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = s;
                    subitem.Tag = @"\\" + s;
                    subitem.FontWeight = FontWeights.Normal;
                    subitem.Items.Add(dummyNode);
                    subitem.Expanded += new RoutedEventHandler(networkServer_Expanded);
                    item.Items.Add(subitem);
                }
            });
        }

        void networkServer_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            e.Handled = true;
            item.Items.Clear();
            Utils.TryWithToast(() =>
            {
                var folders = Utils.GetDirectoriesInNetworkLocation(item.Tag.ToString());
                foreach (string s in folders)
                {
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = s;
                    subitem.Tag = item.Tag + "\\" +s;
                    subitem.FontWeight = FontWeights.Normal;
                    subitem.Items.Add(dummyNode);
                    subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                    item.Items.Add(subitem);
                }
            });
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            e.Handled = true;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                Utils.TryWithToast(() =>
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
                });
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

            TreeViewItem item2 = new TreeViewItem();
            item2.Header = "Network";
            item2.Tag = "\\\\";
            item2.FontWeight = FontWeights.Normal;
            item2.Items.Add(dummyNode);
            item2.Expanded += new RoutedEventHandler(network_Expanded);
            foldersItem.Items.Add(item2);
        }

        public TreeViewItem CreateItem(string header, string Tag, RoutedEventHandler handler)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = header;
            item.Tag = Tag;
            item.FontWeight = FontWeights.Normal;
            item.Items.Add(dummyNode);
            item.Expanded += handler;
            return item;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HasChangedNames)
                Save();
        }


        private void Save()
        {
            MessageBoxResult result1 = MessageBox.Show("Do you want to save changes?",
                "Save changes", MessageBoxButton.YesNo);

            int succeeded = 0;
            int errors = 0;
            if (result1 == MessageBoxResult.Yes)
            {
                for (int i = 0; i < FileNames.Length; i++)
                {
                    if (FileNames[i] != NewFileNames[i])
                    { 
                        if(Utils.TryWithToast(() => File.Move(FileNames[i], NewFileNames[i])))
                            succeeded++;
                        else
                            errors++;
                    }
                }
           
            if (errors == 0)
                Toast.Show($"Renamed {succeeded} files.", Toast.Type.Succ);
            else
                Toast.Show($"Renamed {succeeded} files. {errors} fails.", Toast.Type.Err);

            ShowFiles(CurrentPath);
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!HasChangedNames)
            {
                Toast.Show("You haven't modifed the names.", Toast.Type.Msg);
                return;
            }

            if (NewFileNames.Length != FileNames.Length)
            {
                Toast.Show("Number of lines differs from origin.");
                return;
            }
            Save();
        }

        private void OpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            Utils.TryWithToast(() => Process.Start("explorer.exe", $"/select, \"{SelectedFile}\""));           
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Utils.TryWithToast(() => Process.Start($"\"{SelectedFile}\""));
        }

        private void Details_Click(object sender, RoutedEventArgs e)
        {
            Utils.TryWithToast(() =>
            {
                Utils.TryWithToast(() =>
                {

                });
            });
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Save();
                e.Handled = true;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentPath == null)
                Clipboard.SetText(_editor.Text);
            else
                Clipboard.SetText(SelectedFile);
        }
    }
}
