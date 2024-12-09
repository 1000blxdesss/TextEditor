//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Windows.Forms;
//using System.Xml.Linq;

//namespace TabbedEditor
//{
//    public partial class Form1 : Form
//    {
//        private Stack<string> undoList = new Stack<string>();
//        private Stack<string> redoList = new Stack<string>();
//        private Stack<string> recentFiles = new Stack<string>();
//        private ToolStripMenuItem toolStripMenuItem;
//        private const int MaxUndoSteps = 5;
//        private const int MaxRecentFiles = 5;
//        private bool isUndoRedoOperation = false;

//        public Form1()
//        {
//            InitializeComponent();
//            toolStripMenuItem = recentToolStripMenuItem;
//            this.recentToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.recentToolStripMenuItem_DropDownItemClicked);
//        }

//        // Для операций Copy, Paste, Cut
//        private RichTextBox GetRichTextBox()
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                return Tpage.Controls[0] as RichTextBox;
//            }
//            return null;
//        }

//        public void recentToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
//        {
//            // Получаем текст кликнутого элемента (имя файла)
//            string clickedFileName = e.ClickedItem.Text;

//            // Выводим его для проверки
//            MessageBox.Show($"Вы выбрали файл: {clickedFileName}");

//            // Логика для открытия файла
//            OpenRecentFile(clickedFileName);
//        }

//        private void OpenRecentFile(string filePath)
//        {
//            if (File.Exists(filePath))
//            {
//                string filetext = File.ReadAllText(filePath); // Считываем текст из файла

//                // Создаем новую вкладку с именем файла
//                TabPage TempPage = new TabPage(Path.GetFileName(filePath));

//                // Создаем новый RichTextBox
//                RichTextBox RichText = new RichTextBox();

//                // Подписываемся на событие TextChanged
//                RichText.TextChanged += RichText_TextChanged;

//                // Устанавливаем Dock для RichTextBox, чтобы он занимал всю вкладку
//                RichText.Dock = DockStyle.Fill;

//                // Вставляем текст из файла в RichTextBox
//                RichText.Text = filetext;

//                // Добавляем RichTextBox на вкладку
//                TempPage.Controls.Add(RichText);

//                // Добавляем новую вкладку в TabControl
//                tabControl1.Controls.Add(TempPage);

//                // Устанавливаем новую вкладку активной
//                tabControl1.SelectedTab = TempPage;
//            }
//            else
//            {
//                MessageBox.Show("File not found!");
//            }
//        }


//        private void RichText_TextChanged(object sender, EventArgs e)
//        {
//            if (isUndoRedoOperation)
//            {
//                isUndoRedoOperation = false;
//                return;
//            }

//            // Получаем текущую вкладку
//            TabPage currentTab = tabControl1.SelectedTab;

//            if (currentTab != null && !currentTab.Text.EndsWith("*"))
//            {
//                // Добавляем "*" к названию вкладки, если его там еще нет
//                currentTab.Text += "*";
//            }

//            if (undoList.Count == MaxUndoSteps)
//            {
//                undoList.Pop();
//            }

//            undoList.Push(GetRichTextBox().Text);
//            redoList.Clear(); // Очищаем стек redo при новом изменении
//        }

//        // File->Open
//        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            Stream stream;

//            OpenFileDialog opendialog = new OpenFileDialog();
//            if (opendialog.ShowDialog() == DialogResult.OK)
//            {
//                if ((stream = opendialog.OpenFile()) != null)
//                {
//                    string strfilename = opendialog.FileName; // Полный путь к файлу
//                    string filetext = File.ReadAllText(strfilename); // Считываем текст из файла

//                    // Добавляем путь к файлу в стек recentFiles
//                    if (recentFiles.Count == MaxRecentFiles)
//                    {
//                        recentFiles.Pop();
//                    }
//                    recentFiles.Push(strfilename);
//                    //ToolStripMenuItem newItem = new ToolStripMenuItem { Text = strfilename };
//                    //recentToolStripMenuItem.DropDownItems.Add(newItem);

//                    // Создаем новую вкладку с именем файла
//                    TabPage TempPage = new TabPage(opendialog.SafeFileName);

//                    // Создаем новый RichTextBox
//                    RichTextBox RichText = new RichTextBox();

//                    // Подписываемся на событие TextChanged
//                    RichText.TextChanged += RichText_TextChanged;

//                    // Устанавливаем Dock для RichTextBox, чтобы он занимал всю вкладку
//                    RichText.Dock = DockStyle.Fill;

//                    // Вставляем текст из файла в RichTextBox
//                    RichText.Text = filetext;

//                    // Добавляем RichTextBox на вкладку
//                    TempPage.Controls.Add(RichText);

//                    // Добавляем новую вкладку в TabControl
//                    tabControl1.Controls.Add(TempPage);

//                    // Устанавливаем новую вкладку активной
//                    tabControl1.SelectedTab = TempPage;
//                }
//            }
//        }

//        // File->New option
//        private void openToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            // Создаем новую вкладку с заголовком "New Document"
//            TabPage Page = new TabPage("New Document");

//            // Создаем новый RichTextBox
//            RichTextBox RichText = new RichTextBox();

//            // Устанавливаем свойство Dock для RichTextBox, чтобы он заполнял всю доступную область
//            RichText.Dock = DockStyle.Fill;

//            // Подписываемся на событие TextChanged
//            RichText.TextChanged += RichText_TextChanged;

//            // Добавляем RichTextBox в коллекцию контролов вкладки
//            Page.Controls.Add(RichText);

//            // Добавляем вкладку в TabControl
//            tabControl1.Controls.Add(Page);
//        }

//        // Cut option
//        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                GetRichTextBox().Cut();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Copy option
//        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                GetRichTextBox().Copy();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Paste option
//        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                GetRichTextBox().Paste();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // File->Save
//        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                SaveFileDialog save = new SaveFileDialog();
//                if (save.ShowDialog() == DialogResult.OK)
//                {
//                    using (Stream s = File.Open(save.FileName, FileMode.CreateNew))
//                    {
//                        // Создаем StreamWriter для записи в поток
//                        using (StreamWriter sw = new StreamWriter(s))
//                        {
//                            // Записываем текст из RichTextBox в файл
//                            sw.Write(GetRichTextBox()?.Text);

//                            // Обновляем заголовок текущей вкладки на имя сохраненного файла
//                            tabControl1.SelectedTab.Text = Path.GetFileName(save.FileName);
//                        }
//                    }
//                }
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Select All option
//        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                GetRichTextBox().SelectAll();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Undo option
//        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            RichTextBox richTextBox = GetRichTextBox();
//            if (richTextBox != null && undoList.Count > 0)
//            {
//                isUndoRedoOperation = true;
//                // Сохраняем текущее состояние текста в стек redo
//                redoList.Push(richTextBox.Text);
//                // Восстанавливаем предыдущее состояние текста из стека undo
//                richTextBox.Text = undoList.Pop();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Redo option
//        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            RichTextBox richTextBox = GetRichTextBox();
//            if (richTextBox != null && redoList.Count > 0)
//            {
//                isUndoRedoOperation = true;
//                // Сохраняем текущее состояние текста в стек undo
//                undoList.Push(richTextBox.Text);
//                // Восстанавливаем следующее состояние текста из стека redo
//                richTextBox.Text = redoList.Pop();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Clear option
//        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                GetRichTextBox().Clear();
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }

//        // Close Tab button
//        private void closetabb_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                TabPage CurrentTab = tabControl1.SelectedTab;
//                tabControl1.TabPages.Remove(CurrentTab);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("No Open Tabs to Close");
//            }
//        }

//        // Close Tab button
//        private void toolStripButton1_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                TabPage CurrentTab = tabControl1.SelectedTab;
//                tabControl1.TabPages.Remove(CurrentTab);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("No Open Tabs to Close");
//            }
//        }

//        private void Form1_Load(object sender, EventArgs e)
//        {
//            // Инициализация формы
//        }

//        private void undoClearToolStripMenuItem_Click(object sender, EventArgs e)
//        {

//        }

//        private void recentToolStripMenuItem_Click(object sender, EventArgs e)
//        {

//        }



//        private void toolStrip1_DoubleClick(object sender, EventArgs e)
//        {
//            openToolStripMenuItem_Click(sender, e);
//        }

//        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            TabPage Tpage = tabControl1.SelectedTab;
//            if (Tpage != null)
//            {
//                SaveFileDialog save = new SaveFileDialog();
//                if (save.ShowDialog() == DialogResult.OK)
//                {
//                    using (Stream s = File.Open(save.FileName, FileMode.CreateNew))
//                    {
//                        // Создаем StreamWriter для записи в поток
//                        using (StreamWriter sw = new StreamWriter(s))
//                        {
//                            // Записываем текст из RichTextBox в файл
//                            sw.Write(GetRichTextBox()?.Text);

//                            // Обновляем заголовок текущей вкладки на имя сохраненного файла
//                            tabControl1.SelectedTab.Text = Path.GetFileName(save.FileName);
//                        }
//                    }
//                }
//            }
//            else
//            {
//                MessageBox.Show("Open a new Document First!");
//            }
//        }
//    }
//}