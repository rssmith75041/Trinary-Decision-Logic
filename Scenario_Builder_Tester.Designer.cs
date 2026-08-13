namespace Trinary_Decision_Logic
{
    partial class Scenario_Builder_Tester
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
            txtProgram = new TextBox();
            txtOutput = new TextBox();
            btnExecute = new Button();
            txtErrors = new TextBox();
            cmbScenario = new ComboBox();
            lblScenarioName = new Label();
            txtScenarioName = new TextBox();
            btnSaveScenario = new Button();
            btnCheckScenario = new Button();
            SuspendLayout();
            // 
            // txtProgram
            // 
            txtProgram.AcceptsReturn = true;
            txtProgram.AcceptsTab = true;
            txtProgram.AllowDrop = true;
            txtProgram.Location = new Point(15, 23);
            txtProgram.Multiline = true;
            txtProgram.Name = "txtProgram";
            txtProgram.ScrollBars = ScrollBars.Both;
            txtProgram.Size = new Size(620, 465);
            txtProgram.TabIndex = 0;
            // 
            // txtOutput
            // 
            txtOutput.BackColor = Color.White;
            txtOutput.Location = new Point(650, 23);
            txtOutput.Multiline = true;
            txtOutput.Name = "txtOutput";
            txtOutput.ReadOnly = true;
            txtOutput.ScrollBars = ScrollBars.Both;
            txtOutput.Size = new Size(558, 465);
            txtOutput.TabIndex = 1;
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(16, 547);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(141, 23);
            btnExecute.TabIndex = 2;
            btnExecute.Text = "Execute Program";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // txtErrors
            // 
            txtErrors.BackColor = Color.White;
            txtErrors.Location = new Point(650, 495);
            txtErrors.Multiline = true;
            txtErrors.Name = "txtErrors";
            txtErrors.ReadOnly = true;
            txtErrors.ScrollBars = ScrollBars.Both;
            txtErrors.Size = new Size(558, 151);
            txtErrors.TabIndex = 3;
            // 
            // cmbScenario
            // 
            cmbScenario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbScenario.FormattingEnabled = true;
            cmbScenario.Items.AddRange(new object[] { "Select Scenario ...", "Scenario 1", "Scenario 2", "Scenario 3", "Scenario 4", "Scenario 5", "Scenario 6", "Scenario 7", "Scenario X" });
            cmbScenario.Location = new Point(164, 516);
            cmbScenario.Name = "cmbScenario";
            cmbScenario.Size = new Size(221, 23);
            cmbScenario.TabIndex = 4;
            cmbScenario.SelectedIndexChanged += cmbScenario_SelectedIndexChanged;
            // 
            // lblScenarioName
            // 
            lblScenarioName.AutoSize = true;
            lblScenarioName.Location = new Point(21, 495);
            lblScenarioName.Name = "lblScenarioName";
            lblScenarioName.Size = new Size(87, 15);
            lblScenarioName.TabIndex = 5;
            lblScenarioName.Text = "Scenario Name";
            // 
            // txtScenarioName
            // 
            txtScenarioName.Location = new Point(16, 516);
            txtScenarioName.Name = "txtScenarioName";
            txtScenarioName.Size = new Size(142, 23);
            txtScenarioName.TabIndex = 6;
            // 
            // btnSaveScenario
            // 
            btnSaveScenario.Location = new Point(17, 576);
            btnSaveScenario.Name = "btnSaveScenario";
            btnSaveScenario.Size = new Size(142, 23);
            btnSaveScenario.TabIndex = 7;
            btnSaveScenario.Text = "Save Scenario";
            btnSaveScenario.UseVisualStyleBackColor = true;
            btnSaveScenario.Click += btnSaveScenario_Click;
            // 
            // btnCheckScenario
            // 
            btnCheckScenario.Location = new Point(164, 547);
            btnCheckScenario.Name = "btnCheckScenario";
            btnCheckScenario.Size = new Size(141, 23);
            btnCheckScenario.TabIndex = 8;
            btnCheckScenario.Text = "Check Scenario";
            btnCheckScenario.UseVisualStyleBackColor = true;
            btnCheckScenario.Click += btnCheckScenario_Click;
            // 
            // Scenario_Builder_Tester
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1220, 664);
            Controls.Add(btnCheckScenario);
            Controls.Add(btnSaveScenario);
            Controls.Add(txtScenarioName);
            Controls.Add(lblScenarioName);
            Controls.Add(cmbScenario);
            Controls.Add(txtErrors);
            Controls.Add(btnExecute);
            Controls.Add(txtOutput);
            Controls.Add(txtProgram);
            Name = "Scenario_Builder_Tester";
            Text = "Scenario_Builder_Tester";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtProgram;
        private TextBox txtOutput;
        private Button btnExecute;
        private TextBox txtErrors;
        private ComboBox cmbScenario;
        private Label lblScenarioName;
        private TextBox txtScenarioName;
        private Button btnSaveScenario;
        private Button btnCheckScenario;
    }
}