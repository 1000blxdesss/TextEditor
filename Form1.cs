using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TabbedEditor
{
    public partial class Form1 : Form
    {
        private Editor editor;
        private RecentList recentList;
        private const int MaxRecentFiles = 5;

        public Form1()
        {
            InitializeComponent();
            editor = new Editor(tabControl1);
            recentList = new RecentList(recentToolStripMenuItem, MaxRecentFiles, editor);
            recentToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recentToolStripMenuItem_DropDownItemClicked);
        }

        private void recentToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string clickedFileName = e.ClickedItem.Text;
            MessageBox.Show($"Вы выбрали файл: {clickedFileName}");
            recentList.OpenRecentFile(clickedFileName);
        }

        // File->Open
        //private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog opendialog = new OpenFileDialog();
        //    if (opendialog.ShowDialog() == DialogResult.OK)
        //    {
        //        string strfilename = opendialog.FileName;
        //        string filetext = File.ReadAllText(strfilename);
        //        recentList.AddRecentFile(strfilename);
        //        editor.OpenDocument(strfilename, filetext);
        //    }
        //}
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opendialog = new OpenFileDialog();
            if (opendialog.ShowDialog() == DialogResult.OK)
            {
                string strfilename = opendialog.FileName;
                string filetext = File.ReadAllText(strfilename);
                Document document = new Document(strfilename, filetext);
                recentList.AddRecentFile(strfilename);
                editor.OpenDocument(document);
            }
        }
        // File->New option
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.NewDocument();
        }

        // Cut option
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Cut();
        }

        // Copy option
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Copy();
        }

        // Paste option
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Paste();
        }

        // File->Save
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            editor.SaveDocument();
        }

        // Select All option
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.SelectAll();
        }

        // Undo option
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Undo();
        }

        // Redo option
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Redo();
        }

        // Clear option
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.Clear();
        }

        // Close Tab button
        private void closetabb_Click(object sender, EventArgs e)
        {
            editor.CloseTab();
        }

        // Close Tab button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            editor.CloseTab();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // :)
        }

        private void toolStrip1_DoubleClick(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }
        private void recentToolStripMenuItem_Click(object sender, EventArgs e)
        { }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editor.SaveDocumentAs();
        }
    }

    public class Editor
    {
        private TabControl tabControl;
        private Stack<string> undoList = new Stack<string>();
        private Stack<string> redoList = new Stack<string>();
        private const int MaxUndoSteps = 5;
        private bool isUndoRedoOperation = false;

        public Editor(TabControl tabControl)
        {
            this.tabControl = tabControl;
        }

        private RichTextBox GetRichTextBox()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                return Tpage.Controls[0] as RichTextBox;
            }
            return null;
        }

        public void NewDocument()
        {
            TabPage Page = new TabPage("New Document");
            RichTextBox RichText = new RichTextBox();
            RichText.Dock = DockStyle.Fill;
            RichText.TextChanged += RichText_TextChanged;
            Page.Controls.Add(RichText);
            tabControl.Controls.Add(Page);
        }

        //public void OpenDocument(string filePath, string fileText)
        //{
        //    string tabName = Path.GetFileName(filePath);

        //    // Проверка на одинаковые вкладки
        //    foreach (TabPage tab in tabControl.TabPages)
        //    {
        //        if (tab.Text.StartsWith(tabName))
        //        {
        //            MessageBox.Show("A tab with this name is already open.");
        //            tabControl.SelectedTab = tab;
        //            return;
        //        }
        //    }

        //    TabPage TempPage = new TabPage(tabName);
        //    RichTextBox RichText = new RichTextBox();
        //    RichText.Dock = DockStyle.Fill;
        //    RichText.Text = fileText;
        //    RichText.TextChanged += RichText_TextChanged;
        //    TempPage.Controls.Add(RichText);
        //    tabControl.Controls.Add(TempPage);
        //    tabControl.SelectedTab = TempPage;
        //}
        public void OpenDocument(Document document)
        {
            string tabName = Path.GetFileName(document.FilePath);

            // Проверка на одинаковые вкладки
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Text.StartsWith(tabName))
                {
                    MessageBox.Show("A tab with this name is already open.");
                    tabControl.SelectedTab = tab;
                    return;
                }
            }

            TabPage TempPage = new TabPage(tabName);
            RichTextBox RichText = new RichTextBox();
            RichText.Dock = DockStyle.Fill;
            RichText.Text = document.Content;
            RichText.TextChanged += RichText_TextChanged;
            TempPage.Controls.Add(RichText);
            tabControl.Controls.Add(TempPage);
            tabControl.SelectedTab = TempPage;
        }
        private void RichText_TextChanged(object sender, EventArgs e)
        {
            if (isUndoRedoOperation)
            {
                isUndoRedoOperation = false;
                return;
            }

            TabPage currentTab = tabControl.SelectedTab;
            if (currentTab != null && !currentTab.Text.EndsWith("*"))
            {
                currentTab.Text += "*";
            }

            if (undoList.Count == MaxUndoSteps)
            {
                undoList.Pop();
            }

            undoList.Push(GetRichTextBox().Text);
            redoList.Clear();
        }

        public void Cut()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                GetRichTextBox().Cut();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void Copy()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                GetRichTextBox().Copy();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void Paste()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                GetRichTextBox().Paste();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void SaveDocument()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (Stream s = File.Open(save.FileName, FileMode.CreateNew))
                    {
                        using (StreamWriter sw = new StreamWriter(s))
                        {
                            sw.Write(GetRichTextBox()?.Text);
                            tabControl.SelectedTab.Text = Path.GetFileName(save.FileName);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void SaveDocumentAs()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                if (save.ShowDialog() == DialogResult.OK)
                {
                    using (Stream s = File.Open(save.FileName, FileMode.CreateNew))
                    {
                        using (StreamWriter sw = new StreamWriter(s))
                        {
                            sw.Write(GetRichTextBox()?.Text);
                            tabControl.SelectedTab.Text = Path.GetFileName(save.FileName);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void SelectAll()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                GetRichTextBox().SelectAll();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void Undo()
        {
            RichTextBox richTextBox = GetRichTextBox();
            if (richTextBox != null && undoList.Count > 0)
            {
                isUndoRedoOperation = true;
                redoList.Push(richTextBox.Text);
                richTextBox.Text = undoList.Pop();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void Redo()
        {
            RichTextBox richTextBox = GetRichTextBox();
            if (richTextBox != null && redoList.Count > 0)
            {
                isUndoRedoOperation = true;
                undoList.Push(richTextBox.Text);
                richTextBox.Text = redoList.Pop();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void Clear()
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                GetRichTextBox().Clear();
            }
            else
            {
                MessageBox.Show("Open a new Document First!");
            }
        }

        public void CloseTab()
        {
            try
            {
                TabPage CurrentTab = tabControl.SelectedTab;
                tabControl.TabPages.Remove(CurrentTab);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Open Tabs to Close");
            }
        }
    }

    public class Document
    {
        public string FilePath { get; set; }
        public string Content { get; set; }

        public Document(string filePath, string content)
        {
            FilePath = filePath;
            Content = content;
        }
    }

    public class RecentList
    {
        private Stack<string> recentFiles = new Stack<string>();
        private ToolStripMenuItem toolStripMenuItem;
        private int maxRecentFiles;
        private Editor editor;

        public RecentList(ToolStripMenuItem toolStripMenuItem, int maxRecentFiles, Editor editor)
        {
            this.toolStripMenuItem = toolStripMenuItem;
            this.maxRecentFiles = maxRecentFiles;
            this.editor = editor;
        }

        public void AddRecentFile(string filePath)
        {
            if (recentFiles.Count == maxRecentFiles)
            {
                recentFiles.Pop();
            }
            recentFiles.Push(filePath);
            UpdateRecentFilesMenu();
        }

        //public void OpenRecentFile(string filePath)
        //{
        //    if (File.Exists(filePath))
        //    {
        //        string filetext = File.ReadAllText(filePath);
        //        editor.OpenDocument(filePath, filetext);
        //    }
        //    else
        //    {
        //        MessageBox.Show("File not found!");
        //    }
        //}
        public void OpenRecentFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string filetext = File.ReadAllText(filePath);
                Document document = new Document(filePath, filetext);
                editor.OpenDocument(document);
            }
            else
            {
                MessageBox.Show("File not found!");
            }
        }
        private void UpdateRecentFilesMenu()
        {
            toolStripMenuItem.DropDownItems.Clear();
            foreach (var file in recentFiles)
            {
                ToolStripMenuItem newItem = new ToolStripMenuItem { Text = file };
                toolStripMenuItem.DropDownItems.Add(newItem);
            }
        }
    }
}

