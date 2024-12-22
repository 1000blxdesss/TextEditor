using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            
            comboBox1.Items.Add("UTF-8");
            comboBox1.Items.Add("ASCII");
            comboBox1.Items.Add("Unicode");
            comboBox1.SelectedIndex = -1; 
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                IEncodingStrategy encodingStrategy = null;
                switch (comboBox1.SelectedItem.ToString())
                {
                    case "UTF-8":
                        encodingStrategy = new EncodingUTF();
                        break;
                    case "ASCII":
                        encodingStrategy = new EncodingASCII();
                        break;
                    case "Unicode":
                        encodingStrategy = new EncodingUnicode();
                        break;
                    default:
                        MessageBox.Show("Unsupported Encoding Selected.");
                        return;
                }

                editor.EncodeChange(encodingStrategy);
            }
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
            // Создаем издателя и наблюдателя для RichTextBox
            RichTextBoxSubject richTextBoxSubject = new RichTextBoxSubject(RichText);
            TabNameObserver tabNameObserver = new TabNameObserver(Page);
            richTextBoxSubject.Attach(tabNameObserver);
        }

        public void OpenDocument(Document document)
        {
            string tabName = Path.GetFileName(document.FilePath);

            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Text.StartsWith(tabName))
                {
                    MessageBox.Show("Already open!");
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
            // Создаем издателя и наблюдателя для RichTextBox
            RichTextBoxSubject richTextBoxSubject = new RichTextBoxSubject(RichText);
            TabNameObserver tabNameObserver = new TabNameObserver(TempPage);
            richTextBoxSubject.Attach(tabNameObserver);
        }
        private void RichText_TextChanged(object sender, EventArgs e)
        {
            if (isUndoRedoOperation)
            {
                isUndoRedoOperation = false;
                return;
            }

            //TabPage currentTab = tabControl.SelectedTab;
            //if (currentTab != null && !currentTab.Text.EndsWith("*"))
            //{
            //    currentTab.Text += "*";
            //}

            if (undoList.Count == MaxUndoSteps)
            {
                undoList.Pop();
            }

            undoList.Push(GetRichTextBox().Text);
            redoList.Clear();
        }
        public void EncodeChange(IEncodingStrategy encodingStrategy)
        {
            TabPage Tpage = tabControl.SelectedTab;
            if (Tpage != null)
            {
                RichTextBox richTextBox = GetRichTextBox();
                if (richTextBox != null)
                {
                    string currentText = richTextBox.Text;
                    string encodedText = encodingStrategy.Encode(currentText);
                    richTextBox.Text = encodedText;
                }
                    
            }
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
    public interface IEncodingStrategy
    {
        string Encode(string text);
    }
    public class EncodingUTF : IEncodingStrategy
    {
        public string Encode(string text)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            return BitConverter.ToString(utf8Bytes).Replace("-"," ");
        }
    }

    public class EncodingASCII : IEncodingStrategy
    {
        public string Encode(string text)
        {
            byte[] asciiByteArray = Encoding.ASCII.GetBytes(text);
            string asciiEncoded = String.Join(" ", asciiByteArray);
            return asciiEncoded;
        }
    }

    public class EncodingUnicode : IEncodingStrategy
    {
        public string Encode(string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }
    }
    public interface IObserver
    {
        void Update();
    }

    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify();
    }
    //издатель
    public class RichTextBoxSubject : ISubject
    {
        private List<IObserver> _observers = new List<IObserver>();
        private RichTextBox _richTextBox;

        public RichTextBoxSubject(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            _richTextBox.TextChanged += (sender, e) => Notify();
        }

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify()
        {
            foreach (var observer in _observers)
            {
                observer.Update();
            }
        }
    }
    //Наблюдатель
    public class TabNameObserver : IObserver
    {
        private TabPage _tabPage;

        public TabNameObserver(TabPage tabPage)
        {
            _tabPage = tabPage;
        }

        public void Update()
        {
            if (!_tabPage.Text.EndsWith("*"))
            {
                _tabPage.Text += "*";
            }
        }
    }


}
