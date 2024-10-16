using System.Text.Json;

namespace Json_watcher
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher? _watcher;
        private string? _filePath;

        // Cache the JsonSerializerOptions instance
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "JSON and Text files (*.json;*.txt)|*.json;*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Select JSON or Text File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _filePath = openFileDialog.FileName;
                LoadJsonFile();

                // Setup the FileSystemWatcher
                SetupFileWatcher(Path.GetDirectoryName(_filePath), Path.GetFileName(_filePath));
            }
        }

        private void SetupFileWatcher(string? directory, string? fileName)
        {
            if (string.IsNullOrEmpty(directory))
            {
                MessageBox.Show("Invalid directory path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Invalid file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Dispose of any existing watcher
            _watcher?.Dispose();

            _watcher = new FileSystemWatcher(directory)
            {
                Filter = fileName,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Invoke(new Action(LoadJsonFile));
        }

        private void LoadJsonFile()
        {
            const int maxRetries = 10;
            const int delayMilliseconds = 200;
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    if (_filePath != null)
                    {
                        string fileContent = File.ReadAllText(_filePath);

                        if (Path.GetExtension(_filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            // Parse and pretty-print JSON
                            var parsedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(fileContent), _jsonSerializerOptions);
                            textDisplayWindow.Text = parsedJson;
                        }
                        else
                        {
                            // Display plain text
                            textDisplayWindow.Text = fileContent;
                        }

                        break;
                    }
                }
                catch (IOException ex)
                {
                    retries++;
                    if (retries >= maxRetries)
                    {
                        MessageBox.Show($"Failed to load JSON file after {maxRetries} attempts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Thread.Sleep(delayMilliseconds); // Wait before retrying
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load JSON file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break; // Exit loop on non-IOExceptions
                }
            }
        }


        #region Toggle dark mode
        private bool _isDarkMode = false;

        private void BtnToggleTheme_Click(object sender, EventArgs e)
        {
            ToggleTheme();
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;

            if (_isDarkMode)
            {
                // Set dark mode colors
                BackColor = Color.FromArgb(45, 45, 48);
                ForeColor = Color.White;

                textDisplayWindow.BackColor = Color.FromArgb(30, 30, 30);
                textDisplayWindow.ForeColor = Color.White;

                btnSelectFile.BackColor = Color.FromArgb(63, 63, 70);
                btnSelectFile.ForeColor = Color.White;

                btnToggleTheme.BackColor = Color.FromArgb(63, 63, 70);
                btnToggleTheme.ForeColor = Color.White;
            }
            else
            {
                // Set light mode colors
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;

                textDisplayWindow.BackColor = Color.White;
                textDisplayWindow.ForeColor = Color.Black;

                btnSelectFile.BackColor = SystemColors.Control;
                btnSelectFile.ForeColor = SystemColors.ControlText;

                btnToggleTheme.BackColor = SystemColors.Control;
                btnToggleTheme.ForeColor = SystemColors.ControlText;
            }

            // Update all controls to reflect the new theme
            foreach (Control control in Controls)
            {
                control.BackColor = BackColor;
                control.ForeColor = ForeColor;
            }
        }
        #endregion

    }
}
