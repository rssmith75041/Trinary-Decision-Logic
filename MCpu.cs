using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Trinary_Decision_Logic
{
    public class MCpu
    {
        public MRegister[] Registers;

        public MCpu(int registerCount)
        {
            Registers = new MRegister[registerCount];

            for (int i = 0; i < registerCount; i++)
            {
                Registers[i] = new MRegister($"R{i}");
            }
        }

        public MRegister GetRegister(int index) => Registers[index];

        public void MXor(int destIndex, int srcAIndex, int srcBIndex)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.Xor(Registers[srcAIndex].Value, Registers[srcBIndex].Value);
        }

        public void MAnd(int destIndex, int srcAIndex, int srcBIndex)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.And(Registers[srcAIndex].Value, Registers[srcBIndex].Value);
        }

        public void MOr(int destIndex, int srcAIndex, int srcBIndex)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.Or(Registers[srcAIndex].Value, Registers[srcBIndex].Value);
        }

        public void MNot(int destIndex, int srcIndex)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.Not(Registers[srcIndex].Value);
        }

        public void BAnd(int destIndex, int srcAIndex, int srcBIndex, int bit)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.BAnd(Registers[srcAIndex].Value, Registers[srcBIndex].Value, bit);
        }

        public void BNot(int destIndex, int srcIndex, int bit)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.BNot(Registers[srcIndex].Value, bit);
        }

        public void BOr(int destIndex, int srcAIndex, int srcBIndex, int bit)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.BOr(Registers[srcAIndex].Value, Registers[srcBIndex].Value, bit);
        }

        public void BXor(int destIndex, int srcAIndex, int srcBIndex, int bit)
        {
            if (Registers[destIndex].Locked) { return; }
            Registers[destIndex].Value = MAlu.BXor(Registers[srcAIndex].Value, Registers[srcBIndex].Value, bit);
        }
    }
}
