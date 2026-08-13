using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinary_Decision_Logic
{
    public class MachineState
    {
        private List<string> ExecutionSteps = new List<string>();

        public string MachineName { get; set; }
        public MRegister[]? Registers;
        public bool Halted = false;
        public MFlags ContextFlag;
        public MBit[]? Memory;
        public bool WasMerged;
        public long Sequence;
        public MCpu? Cpu;
        public int PC;

        public MachineState(string machineName, int registerCount, int memorySize = 256)
        {
            Cpu = new MCpu(registerCount);
            Memory = new MBit[memorySize];
            MachineName = machineName;
            Registers = Cpu.Registers;
            ContextFlag = MFlags.True;
            Sequence = 0;
            PC = 0;
        }

        public MachineState Clone(string cloneName)
        {
            MachineState c = new MachineState($"{cloneName}", Registers!.Length);
            c.ContextFlag = ContextFlag;
            c.WasMerged = WasMerged;
            c.PC = PC;

            for (int i = 0; i < Registers.Length; i++)
            {
                if (Registers[i].Value != null)
                {
                    c.Registers![i].Value = Registers[i].Value.Copy();
                }
            }

            Array.Copy(Memory!, c.Memory!, Memory!.Length);
            return c;
        }
    }
}
