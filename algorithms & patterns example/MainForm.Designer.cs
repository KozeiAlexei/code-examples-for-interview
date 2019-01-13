namespace VoiceRecognizeMark
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.RecognizingProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.LearningModeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(110, 23);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Начать запись";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(128, 12);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(110, 23);
            this.StopButton.TabIndex = 1;
            this.StopButton.Text = "Стоп";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // RecognizingProgressBar
            // 
            this.RecognizingProgressBar.Location = new System.Drawing.Point(12, 41);
            this.RecognizingProgressBar.Name = "RecognizingProgressBar";
            this.RecognizingProgressBar.Size = new System.Drawing.Size(400, 15);
            this.RecognizingProgressBar.TabIndex = 2;
            // 
            // LogTextBox
            // 
            this.LogTextBox.Location = new System.Drawing.Point(12, 62);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.Size = new System.Drawing.Size(400, 132);
            this.LogTextBox.TabIndex = 4;
            // 
            // LearningModeButton
            // 
            this.LearningModeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.LearningModeButton.Location = new System.Drawing.Point(269, 12);
            this.LearningModeButton.Name = "LearningModeButton";
            this.LearningModeButton.Size = new System.Drawing.Size(143, 23);
            this.LearningModeButton.TabIndex = 5;
            this.LearningModeButton.Text = "Режим обучения";
            this.LearningModeButton.UseVisualStyleBackColor = false;
            this.LearningModeButton.Click += new System.EventHandler(this.LearningModeButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 209);
            this.Controls.Add(this.LearningModeButton);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.RecognizingProgressBar);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Оценка произношения";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.ProgressBar RecognizingProgressBar;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Button LearningModeButton;
    }
}

