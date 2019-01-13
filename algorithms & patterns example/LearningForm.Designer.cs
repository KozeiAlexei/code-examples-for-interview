namespace VoiceRecognizeMark
{
    partial class LearningForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LearningProgressBar = new System.Windows.Forms.ProgressBar();
            this.StartLearningButton = new System.Windows.Forms.Button();
            this.SelectLearingPathButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LearningPathTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // LearningProgressBar
            // 
            this.LearningProgressBar.Location = new System.Drawing.Point(12, 80);
            this.LearningProgressBar.MarqueeAnimationSpeed = 3;
            this.LearningProgressBar.Maximum = 10;
            this.LearningProgressBar.Name = "LearningProgressBar";
            this.LearningProgressBar.Size = new System.Drawing.Size(378, 15);
            this.LearningProgressBar.TabIndex = 9;
            // 
            // StartLearningButton
            // 
            this.StartLearningButton.Location = new System.Drawing.Point(94, 51);
            this.StartLearningButton.Name = "StartLearningButton";
            this.StartLearningButton.Size = new System.Drawing.Size(296, 23);
            this.StartLearningButton.TabIndex = 8;
            this.StartLearningButton.Text = "Начать обучение";
            this.StartLearningButton.UseVisualStyleBackColor = true;
            this.StartLearningButton.Click += new System.EventHandler(this.StartLearningButton_Click);
            // 
            // SelectLearingPathButton
            // 
            this.SelectLearingPathButton.Location = new System.Drawing.Point(12, 51);
            this.SelectLearingPathButton.Name = "SelectLearingPathButton";
            this.SelectLearingPathButton.Size = new System.Drawing.Size(76, 23);
            this.SelectLearingPathButton.TabIndex = 7;
            this.SelectLearingPathButton.Text = "Выбрать";
            this.SelectLearingPathButton.UseVisualStyleBackColor = true;
            this.SelectLearingPathButton.Click += new System.EventHandler(this.SelectLearingPathButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Путь к обучающим данным: ";
            // 
            // LearningPathTextBox
            // 
            this.LearningPathTextBox.Location = new System.Drawing.Point(12, 25);
            this.LearningPathTextBox.Name = "LearningPathTextBox";
            this.LearningPathTextBox.ReadOnly = true;
            this.LearningPathTextBox.Size = new System.Drawing.Size(378, 20);
            this.LearningPathTextBox.TabIndex = 5;
            this.LearningPathTextBox.Text = "C:\\Users\\Алексей\\Downloads\\LearningDatabase";
            // 
            // LearningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 112);
            this.Controls.Add(this.LearningProgressBar);
            this.Controls.Add(this.StartLearningButton);
            this.Controls.Add(this.SelectLearingPathButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LearningPathTextBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LearningForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LearningForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar LearningProgressBar;
        private System.Windows.Forms.Button StartLearningButton;
        private System.Windows.Forms.Button SelectLearingPathButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LearningPathTextBox;
    }
}