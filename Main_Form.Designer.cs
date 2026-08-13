namespace Trinary_Decision_Logic
{
    partial class Main_Form
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
            menuStrip1 = new MenuStrip();
            tsmiFile = new ToolStripMenuItem();
            tsmiExit = new ToolStripMenuItem();
            tsmiForms = new ToolStripMenuItem();
            tsmiTruthTables = new ToolStripMenuItem();
            tsmiScenarioBuilderTester = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { tsmiFile, tsmiForms });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1172, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // tsmiFile
            // 
            tsmiFile.DropDownItems.AddRange(new ToolStripItem[] { tsmiExit });
            tsmiFile.Name = "tsmiFile";
            tsmiFile.Size = new Size(37, 20);
            tsmiFile.Text = "File";
            // 
            // tsmiExit
            // 
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(92, 22);
            tsmiExit.Text = "Exit";
            // 
            // tsmiForms
            // 
            tsmiForms.DropDownItems.AddRange(new ToolStripItem[] { tsmiTruthTables, tsmiScenarioBuilderTester });
            tsmiForms.Name = "tsmiForms";
            tsmiForms.Size = new Size(52, 20);
            tsmiForms.Text = "Forms";
            // 
            // tsmiTruthTables
            // 
            tsmiTruthTables.Name = "tsmiTruthTables";
            tsmiTruthTables.Size = new Size(195, 22);
            tsmiTruthTables.Text = "Truth Tables";
            tsmiTruthTables.Click += tsmiTruthTables_Click;
            // 
            // tsmiScenarioBuilderTester
            // 
            tsmiScenarioBuilderTester.Name = "tsmiScenarioBuilderTester";
            tsmiScenarioBuilderTester.Size = new Size(195, 22);
            tsmiScenarioBuilderTester.Text = "Scenario Builder/Tester";
            tsmiScenarioBuilderTester.Click += tsmiScenarioBuilderTester_Click;
            // 
            // Main_Form
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1172, 801);
            Controls.Add(menuStrip1);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip1;
            Name = "Main_Form";
            Text = "Trinary Main Form";
            WindowState = FormWindowState.Maximized;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem tsmiFile;
        private ToolStripMenuItem tsmiExit;
        private ToolStripMenuItem tsmiForms;
        private ToolStripMenuItem tsmiTruthTables;
        private ToolStripMenuItem tsmiScenarioBuilderTester;
    }
}