using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trinary_Decision_Logic
{
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
        }

        private void tsmiTruthTables_Click(object sender, EventArgs e)
        {
            Truth_Tables tt = new Truth_Tables();
            tt.MdiParent = this;
            tt.Show();
        }

        private void tsmiScenarioBuilderTester_Click(object sender, EventArgs e)
        {
            Scenario_Builder_Tester sbt = new Scenario_Builder_Tester();
            sbt.MdiParent = this;
            sbt.Show();
        }
    }
}
