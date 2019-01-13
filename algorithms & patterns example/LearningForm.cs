using System;
using System.Windows.Forms;
using System.Threading.Tasks;

using VoiceRecognizeMark.Learning;
using VoiceRecognizeMark.New;

namespace VoiceRecognizeMark
{
    public partial class LearningForm : Form
    {
        public LearningForm()
        {
            InitializeComponent();
        }

        private void SelectLearingPathButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                    LearningPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private async void StartLearningButton_Click(object sender, EventArgs e)
        {
            LearningProgressBar.Style = ProgressBarStyle.Marquee;
            StartLearningButton.Enabled = false;

            await Task.Run(() =>
            {
                var learning = new SystemLearningNew();
                learning.CreateLearningDatabase(LearningPathTextBox.Text, $"Learning.ldb");
            });

            StartLearningButton.Enabled = true;
            LearningProgressBar.Style = ProgressBarStyle.Blocks;
            MessageBox.Show("Завершено", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
