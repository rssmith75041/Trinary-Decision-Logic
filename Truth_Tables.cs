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
    public partial class Truth_Tables : Form
    {
        private MCpu cpu = new MCpu(4);

        public Truth_Tables()
        {
            InitializeComponent();

            // Initialize registers R0 and R1 to MQWord size
            cpu.Registers[0].Value = new MQWord();
            cpu.Registers[1].Value = new MQWord();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Set bit[0] in R0 and R1
            cpu.Registers[0].Value.Bits![0].Flags = rbReg0False.Checked ? MFlags.False : rbReg0True.Checked ? MFlags.True : MFlags.Maybe;
            cpu.Registers[1].Value.Bits![0].Flags = rbReg1False.Checked ? MFlags.False : rbReg1True.Checked ? MFlags.True : MFlags.Maybe;

            cpu.MXor(2, 0, 1); // R2 = R0 + R1 (bitwise mimic add)
            if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.False) { rbReg2False.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.True) { rbReg2True.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.Maybe) { rbReg2Maybe.Checked = true; }
        }

        private void btnMultiply_Click(object sender, EventArgs e)
        {
            // Set bit[0] in R0 and R1
            cpu.Registers[0].Value.Bits![0].Flags = rbReg0False.Checked ? MFlags.False : rbReg0True.Checked ? MFlags.True : MFlags.Maybe;
            cpu.Registers[1].Value.Bits![0].Flags = rbReg1False.Checked ? MFlags.False : rbReg1True.Checked ? MFlags.True : MFlags.Maybe;

            cpu.MAnd(2, 0, 1); // R2 = R0 & R1 (bitwise mimic multiply)
            if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.False) { rbReg2False.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.True) { rbReg2True.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.Maybe) { rbReg2Maybe.Checked = true; }
        }

        private void btnNot_Click(object sender, EventArgs e)
        {
            // Set bit[0] in R0 and R1
            cpu.Registers[0].Value.Bits![0].Flags = rbReg0False.Checked ? MFlags.False : rbReg0True.Checked ? MFlags.True : MFlags.Maybe;
            cpu.Registers[1].Value.Bits![0].Flags = rbReg1False.Checked ? MFlags.False : rbReg1True.Checked ? MFlags.True : MFlags.Maybe;

            cpu.MNot(2, 0); // R2 = NOT R0 (bitwise mimic not)
            if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.False) { rbReg2False.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.True) { rbReg2True.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.Maybe) { rbReg2Maybe.Checked = true; }
        }

        private void btnOr_Click(object sender, EventArgs e)
        {
            // Set bit[0] in R0 and R1
            cpu.Registers[0].Value.Bits![0].Flags = rbReg0False.Checked ? MFlags.False : rbReg0True.Checked ? MFlags.True : MFlags.Maybe;
            cpu.Registers[1].Value.Bits![0].Flags = rbReg1False.Checked ? MFlags.False : rbReg1True.Checked ? MFlags.True : MFlags.Maybe;

            cpu.MOr(2, 0, 1); // R2 = R0 | R1 (bitwise mimic or)
            if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.False) { rbReg2False.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.True) { rbReg2True.Checked = true; }
            else if (cpu.Registers[2].Value.Bits![0].Flags is MFlags.Maybe) { rbReg2Maybe.Checked = true; }
        }
    }
}
