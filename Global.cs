using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinary_Decision_Logic
{
    //Op for machine name
    //Make a list for trace statements

    public enum OpCode
    {
        And,
        BranchMaybe,        //Arg1 = Conditional Register, Arg2 = True PC, Arg3 = False PC (PC = Program Counter)
        Merge,
        MInit,              //Initialize memory size of Arg1 of MBit
        MStore,             //Memory Store, store value in Arg2 at memory address Arg1
        Not,                //R[Arg1] = NOT R[Arg2]
        Or,
        RMStore,            //Store register Arg1 value into memory starting at Arg2
        RRSet,              //Set value of register Arg1 into register Arg2
        RSet,               //Set R[Arg1] = Arg2 (True, False, Maybe)
        Xor,

        BAnd,
        BBranchMaybe,
        BMerge,
        BMStore,            //Store bit value Arg2 into memory at Arg1
        BNot,
        BOr,
        BRStore,            //Store bit from memory at Arg1 into R[Arg2]->Bit->[Arg3]
        BSet,               //Set individual bits in a register
        BXor,

        MRSet
    }

    public struct Instruction
    {
        public OpCode Op;
        public int Arg1;
        public int Arg2;
        public int Arg3;
        public int Arg4;
    }

    public static class Global
    {
        public static Dictionary<string, List<string>> ExecutionSteps = new Dictionary<string, List<string>>();
        public static List<string> ExecutionStepsInOrder = new List<string>();
        //public static long GlobalSequence = 0;

        private static object lockObject = new object();
        public static void Add_ExecutionStepsInOrder(string msg)
        {
            lock(lockObject)
            {
                ExecutionStepsInOrder.Add(msg);
            }
        }
    }
}
