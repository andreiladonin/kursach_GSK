namespace kursach_GSK
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            drawingArea = new PictureBox();
            menuStrip1 = new MenuStrip();
            statusStrip1 = new StatusStrip();
            label1 = new Label();
            numericUpDown1 = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)drawingArea).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // drawingArea
            // 
            drawingArea.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            drawingArea.BackColor = SystemColors.ControlLightLight;
            drawingArea.Location = new Point(0, 27);
            drawingArea.Name = "drawingArea";
            drawingArea.Size = new Size(1000, 617);
            drawingArea.TabIndex = 0;
            drawingArea.TabStop = false;
            drawingArea.Paint += DrawingArea_Paint;
            drawingArea.MouseDown += DrawingArea_MouseDown;
            drawingArea.MouseMove += DrawingArea_MouseMove;
            drawingArea.MouseUp += DrawingArea_MouseUp;
            // 
            // menuStrip1
            // 
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1186, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 647);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1186, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "Грапфический редактор";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(1005, 35);
            label1.Name = "label1";
            label1.Size = new Size(155, 15);
            label1.TabIndex = 3;
            label1.Text = "N-сторон многоугольника";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(1004, 59);
            numericUpDown1.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 23);
            numericUpDown1.TabIndex = 4;
            numericUpDown1.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1186, 669);
            Controls.Add(numericUpDown1);
            Controls.Add(label1);
            Controls.Add(statusStrip1);
            Controls.Add(drawingArea);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)drawingArea).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox drawingArea;
        private MenuStrip menuStrip1;
        private StatusStrip statusStrip1;
        private Label label1;
        private NumericUpDown numericUpDown1;
    }
}
