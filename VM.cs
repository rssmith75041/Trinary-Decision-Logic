using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Trinary_Decision_Logic
{
    [Flags]
    public enum RFlags : byte       //Register flags
    {
        False,
        True,
        Maybe,
        Mixed,
        Empty
    }

    public class VM
    {
        private List<(int, MFlags, int)> registersUsed = new List<(int, MFlags, int)>();
        private List<string> ExecutionSteps = new List<string>();
        private string vmName = string.Empty;
        private MachineState State => state;
        private Instruction[] program;
        private MachineState state;
        private MaybeEngine maybe;

        public bool SuperPosition(List<int> regs)
        {
            bool isSuperposed = true;

            foreach (int reg in regs)
            {
                if (reg < 0 || reg >= State.Registers!.Length)
                {
                    throw new Exception($"Register Index {reg} out of bounds, Number of Registers is {State.Registers!.Length}");
                }
                
                if (State.Registers![reg].Value.Bits![0].Flags != MFlags.Superposed)
                {
                    isSuperposed = false;
                    break;
                }
            }

            return isSuperposed;
        }

        public MFlags State_Context_Flag => state.ContextFlag;
        public bool Merged => State.WasMerged;
        public int PC => state.PC;

        public MBit Memory(int mIndex) => state.Memory![mIndex];
        
        public MRegister Register(int rIndex)
        {
            if (rIndex < 0 || rIndex >= state.Registers!.Length)
            {
                throw new Exception($"Invalid Regiser Index: {rIndex}, R0 - R{state.Registers!.Length - 1}");
            }

            return state.Registers![rIndex];
        }

        public RFlags RegisterFirstBit(int rIndex)
        {
            MRegister reg = Register(rIndex);
            if (reg.Value == null || reg.Value.Bits![0] == null) return RFlags.Empty;
            return reg.Value.Bits![0].Flags == MFlags.False ? RFlags.False :
                reg.Value.Bits[0].Flags == MFlags.True ? RFlags.True :
                reg.Value.Bits[0].Flags == MFlags.Maybe ? RFlags.Maybe :
                RFlags.Empty;
        }

        public RFlags RegsiterMap(int rIndex)
        {
            MRegister reg = Register(rIndex);
            MBaseWord mbw = reg.Value;
            MBit[] bits = mbw.Bits!;
            int len = bits.Length;

            if (len == 0) return RFlags.Empty;
            MFlags firstFlag = bits[0].Flags;

            for (int i = 1; i < len; i++)
            {
                if (bits[i].Flags != firstFlag)
                {
                    return RFlags.Mixed;
                }
            }

            return firstFlag == MFlags.True ? RFlags.True : firstFlag == MFlags.False ? RFlags.False : firstFlag == MFlags.Maybe ? RFlags.Maybe : RFlags.Mixed;
        }

        public VM(string vmname, Instruction[] program)
        {
            Global.ExecutionSteps = new Dictionary<string, List<string>>();
            Global.ExecutionStepsInOrder = new List<string>();
            Global.Add_ExecutionStepsInOrder($"{vmname}");
            Global.Add_ExecutionStepsInOrder($"Initializing");
            ExecutionSteps.Add($"\tInitializing");
            vmName = vmname;

            state = new MachineState($"VM:{vmName}:Machine", 8);
            this.program = program;

            maybe = new MaybeEngine();
            maybe.RunPath = RunPath;
            maybe.MergeStates = MergeStates;

            Global.Add_ExecutionStepsInOrder($"Initialized");
            ExecutionSteps.Add($"\tInitialized");
        }

        public void Run()
        {
            Global.Add_ExecutionStepsInOrder($"Running");
            ExecutionSteps.Add($"\tRunning");
            while (!State.Halted && State.PC < program.Length)
            {
                Step();
            }

            Output_Register_Values();
            Global.Add_ExecutionStepsInOrder($"Ended");
            ExecutionSteps.Add($"\tEnded");
            Global.ExecutionSteps.Add(vmName, ExecutionSteps);
        }

        public void Step()
        {
            var instr = program[state.PC];

            ExecutionSteps.Add($"\tStep->{instr.Op}");
            switch (instr.Op)
            {
                case OpCode.And:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->And(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                        state.Cpu!.MAnd(instr.Arg1, instr.Arg2, instr.Arg3);
                        state.PC++;
                    }
                    break;
                case OpCode.Or:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->Or(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                        state.Cpu!.MOr(instr.Arg1, instr.Arg2, instr.Arg3);
                        state.PC++;
                    }
                    break;
                case OpCode.Not:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->Not(R[{instr.Arg1}], R[{instr.Arg2}])");
                        state.Cpu!.MNot(instr.Arg1, instr.Arg2);
                        state.PC++;
                    }
                    break;
                case OpCode.BranchMaybe:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BranchMaybe(R[{instr.Arg1}], TrueState->PC({instr.Arg2}), FalseState->PC({instr.Arg3}))");
                        state.PC = maybe.BranchMaybe(state, instr.Arg1, instr.Arg2, instr.Arg3);
                    }
                    break;
                case OpCode.Merge:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->Merge");
                        state.PC++;
                    }
                    break;
                case OpCode.RSet:
                    {
                        int reg = instr.Arg1;    // destination register
                        int val = instr.Arg2;    // 0=False, 1=True, 2=Maybe
                        int type = instr.Arg3;   // 0=MWord, 1=MByte, 2=MDWord, 3=MQWord
                        MFlags vFlags = (val == 0 ? MFlags.False : val == 1 ? MFlags.True : MFlags.Maybe);

                        // Create a new M-Type set to True or False
                        MBaseWord? w = null;
                        switch (type)
                        {
                            case 0: w = new MByte(); break;
                            case 1: w = new MWord(); break;
                            case 2: w = new MDWord(); break;
                            case 3: w = new MQWord(); break;
                            default: w = new MWord(); break;
                        }

                        Global.Add_ExecutionStepsInOrder($"Step->{state.MachineName}->PC({state.PC})->RSet(R[{reg}])<-{vFlags}");
                        for (int i = 0; i < w.Bits!.Length; i++)
                        {
                            w!.Bits![i].Flags = vFlags;
                        }

                        registersUsed.Add((reg, vFlags, state.PC));
                        state.Registers![reg].Value = w;
                        state.PC++;
                    }
                    break;
                case OpCode.Xor:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->Xor(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                        state.Cpu!.MXor(instr.Arg1, instr.Arg2, instr.Arg3);
                        state.PC++;
                    }
                    break;

                case OpCode.BAnd:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BAnd(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                        state.Cpu!.BAnd(instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                        state.PC++;
                    }
                    break;
                case OpCode.BBranchMaybe:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BBranchMaybe(R[{instr.Arg1}][{instr.Arg2}], TrueState->PC({instr.Arg3}), FalseState->PC({instr.Arg4}))");
                        state.PC = maybe.BBranchMaybe(state, instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                    }
                    break;
                case OpCode.BMerge:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BMerge");
                        state.PC++;
                    }
                    break;
                case OpCode.BNot:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BNot(R[{instr.Arg1}][{instr.Arg3}], R[{instr.Arg2}][{instr.Arg3}])");
                        state.Cpu!.BNot(instr.Arg1, instr.Arg2, instr.Arg3);
                        state.PC++;
                    }
                    break;
                case OpCode.BOr:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BOr(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                        state.Cpu!.BOr(instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                        state.PC++;
                    }
                    break;
                case OpCode.BSet:
                    {
                        int reg = instr.Arg1;    // destination register
                        int bit = instr.Arg2;    // destination bit in register
                        int val = instr.Arg3;    // 0=False, 1=True, 2=Maybe
                        int type = instr.Arg4;   // 0=MWord, 1=MByte, 2=MDWord, 3=MQWord

                        if (state.Registers![reg].Value is null)
                        {
                            MBaseWord? w = null;

                            switch (type)
                            {
                                case 0: w = new MByte(); break;
                                case 1: w = new MWord(); break;
                                case 2: w = new MDWord(); break;
                                case 3: w = new MQWord(); break;
                                default: w = new MWord(); break;
                            }
                            
                            state.Registers![reg].Value = w;
                        }

                        MFlags vFlags = (val == 0 ? MFlags.False : val == 1 ? MFlags.True : MFlags.Maybe);
                        state.Registers![reg].Value.Bits![bit].Flags = vFlags;
                        Global.Add_ExecutionStepsInOrder($"Step->{state.MachineName}->PC({state.PC})->BSet(R[{reg}][{bit}])<-{vFlags}");
                        state.PC++;
                    }
                    break;
                case OpCode.BXor:
                    {
                        Global.Add_ExecutionStepsInOrder($"Step->PC({state.PC})->BXor(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                        state.Cpu!.BXor(instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                        state.PC++;
                    }
                    break;

                case OpCode.MRSet:
                    {
                        int addr = instr.Arg1;      // R[Arg1]
                        int bit = instr.Arg2;       // R[Arg1][Arg2]
                        int mAddr = instr.Arg3;     // M[Arg3]

                        Global.Add_ExecutionStepsInOrder($"RunPath->{state.MachineName}->PC({state.PC})->MRSet(R[{addr}][{bit}]<-M[{mAddr}])");
                        state.Registers![addr].Value.Bits![bit] = state.Memory![mAddr];
                    }
                    break;
            }

            if (state.PC >= program.Length)
            {
                Global.Add_ExecutionStepsInOrder($"Step->Halted");
                ExecutionSteps.Add($"\tStep->Halted");
                state.Halted = true;
            }
        }

        private void Output_Register_Values()
        {
            if (registersUsed.Count > 0)
            {
                string msg = string.Empty;
                foreach ((int, MFlags, int) reg in registersUsed)
                {
                    if (!string.IsNullOrEmpty(msg)) { msg += "; "; }
                    msg += $"Initialized: R[{reg.Item1}] -> {reg.Item2}";
                }
                Global.Add_ExecutionStepsInOrder($"{msg}");
                msg = string.Empty;
                foreach ((int, MFlags, int) reg in registersUsed)
                {
                    if (!string.IsNullOrEmpty(msg)) { msg += "; "; }
                    msg += $"Result: R[{reg.Item1}] -> {state.Registers![reg.Item1].Value.Bits![0]}";
                }
                Global.Add_ExecutionStepsInOrder($"{msg}");
            }
        }

        private MachineState RunPath(MachineState s, int startPC)
        {
            ExecutionSteps.Add($"\t\tRunPath->PC({startPC})");
            s.PC = startPC;

            while (!s.Halted && s.PC >= 0 && s.PC < program.Length)
            {
                var instr = program[s.PC];
                if (instr.Op == OpCode.Merge)
                {
                    Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->Ended->PC({s.PC})");
                    ExecutionSteps.Add($"\t\tRunPath->Ended->PC({s.PC})");
                    s.Halted = true;
                    return s;
                }

                ExecutionSteps.Add($"\t\tRunPath->PC({s.PC})->OpCode({instr.Op})");
                switch (instr.Op)
                {
                    case OpCode.And:
                        {
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MAnd(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                            s.Cpu!.MAnd(instr.Arg1, instr.Arg2, instr.Arg3);
                            s.PC++;
                        }
                        break;
                    case OpCode.MInit:
                        {
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MInit(M[0-{instr.Arg1}])");
                            for (int ndx = 0; ndx < instr.Arg1; ndx++)
                            {
                                s.Memory![ndx] = new MBit(MFlags.Maybe);
                            }

                            s.PC++;
                        }
                        break;
                    case OpCode.Not:
                        {
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MNot(R[{instr.Arg1}], R[{instr.Arg2}])");
                            s.Cpu!.MNot(instr.Arg1, instr.Arg2);
                            s.PC++;
                        }
                        break;
                    case OpCode.Or:
                        {
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MOr(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                            s.Cpu!.MOr(instr.Arg1, instr.Arg2, instr.Arg3);
                            s.PC++;
                        }
                        break;
                    case OpCode.MStore:
                        {
                            byte val = (byte)(instr.Arg2 & 0xFF);
                            int addr = instr.Arg1;

                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MStore(M[{addr}-{addr+8}])<-{val}");
                            for (int i = 0; i < 8; i++)
                            {
                                bool bit = ((val >> i) & 1) != 0;
                                s.Memory![addr + i].Flags = bit ? MFlags.True : MFlags.False;
                            }

                            s.PC++;
                        }
                        break;
                    case OpCode.RMStore:
                        {
                            int addr = instr.Arg1;   // memory start address
                            int src = instr.Arg2;    // register index

                            MBaseWord w = s.Registers![src].Value;
                            // Store each trinary bit into memory
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->RMStore(R[{src}]->M[{addr}-{addr + w.Bits!.Length}])");
                            for (int i = 0; i < w.Bits!.Length; i++)
                            {
                                s.Memory![addr + i].Flags = w.Bits[i].Flags;
                            }

                            s.PC++;
                        }
                        break;
                    case OpCode.RRSet:
                        {
                            int dst = instr.Arg1;
                            int src = instr.Arg2;

                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->RRSet(R[{dst}], R[{src}])");
                            s.Registers![dst].Value = s.Registers![src].Value.Copy();
                            s.PC++;
                        }
                        break;
                    case OpCode.RSet:
                        {
                            int reg = instr.Arg1;    // destination register
                            int val = instr.Arg2;    // 0=False, 1=True, 2=Maybe
                            int type = instr.Arg3;   // 0=MWord, 1=MByte, 2=MDWord, 3=MQWord
                            MFlags vFlags = (val == 0 ? MFlags.False : val == 1 ? MFlags.True : MFlags.Maybe);

                            // Create a new M-Type set to True or False
                            MBaseWord? w = null;
                            switch (type)
                            {
                                case 0: w = new MByte(); break;
                                case 1: w = new MWord(); break;
                                case 2: w = new MDWord(); break;
                                case 3: w = new MQWord(); break;
                                default: w = new MWord(); break;
                            }

                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->RSet(R[{reg}])<-{vFlags}");
                            for (int i = 0; i < w.Bits!.Length; i++)
                            {
                                w!.Bits![i].Flags = vFlags;
                            }

                            s.Registers![reg].Value = w;
                            s.PC++;
                        }
                        break;
                    case OpCode.Xor:
                        {
                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->Xor(R[{instr.Arg1}], R[{instr.Arg2}], R[{instr.Arg3}])");
                            s.Cpu!.MXor(instr.Arg1, instr.Arg2, instr.Arg3);
                            s.PC++;
                        }
                        break;

                    case OpCode.MRSet:
                        {
                            int addr = instr.Arg1;  // R[Arg1]
                            int bit = instr.Arg2;   // R[Arg1][Arg2]
                            int mAddr = instr.Arg3; // M[Arg3]

                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->MRSet(R[{addr}][{bit}]<-M[{mAddr}])");
                            s.Registers![addr].Value.Bits![bit] = s.Memory![mAddr];
                        }
                        break;

                    case OpCode.BAnd:
                        {
                            Global.Add_ExecutionStepsInOrder($"Step->PC({s.PC})->BAnd(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                            s.Cpu!.BAnd(instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                            s.PC++;
                        }
                        break;
                    case OpCode.BNot:
                        {
                            Global.Add_ExecutionStepsInOrder($"Step->PC({s.PC})->BNot(R[{instr.Arg1}][{instr.Arg3}], R[{instr.Arg2}][{instr.Arg3}])");
                            s.Cpu!.BNot(instr.Arg1, instr.Arg2, instr.Arg3);
                            s.PC++;
                        }
                        break;
                    case OpCode.BOr:
                        {
                            Global.Add_ExecutionStepsInOrder($"Step->PC({s.PC})->BOr(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                            s.Cpu!.MOr(instr.Arg1, instr.Arg2, instr.Arg3);
                            s.PC++;
                        }
                        break;
                    case OpCode.BMStore:
                        {
                            int addr = instr.Arg1;   // memory bit address
                            int val = instr.Arg2;    // value
                            MFlags vFlags = (val == 0 ? MFlags.False : val == 1 ? MFlags.True : MFlags.Maybe);

                            Global.Add_ExecutionStepsInOrder($"RunPath->{s.MachineName}->PC({s.PC})->BMStore(M[{addr}]<-{vFlags})");
                            s.Memory![addr].Flags = vFlags;
                            s.PC++;
                        }
                        break;
                    case OpCode.BRStore:
                        {
                            int addr = instr.Arg1;   // memory bit address
                            int reg = instr.Arg2;    // register
                            int bit = instr.Arg3;
                            s.Registers![reg].Value.Bits![bit] = s.Memory![addr];
                        }
                        break;
                    case OpCode.BSet:
                        {
                            int reg = instr.Arg1;    // destination register
                            int bit = instr.Arg2;    // destination bit in register
                            int val = instr.Arg3;    // 0=False, 1=True, 2=Maybe
                            int type = instr.Arg4;   // 0=MWord, 1=MByte, 2=MDWord, 3=MQWord

                            if (s.Registers![reg].Value is null)
                            {
                                MBaseWord? w = null;

                                switch (type)
                                {
                                    case 0: w = new MByte(); break;
                                    case 1: w = new MWord(); break;
                                    case 2: w = new MDWord(); break;
                                    case 3: w = new MQWord(); break;
                                    default: w = new MWord(); break;
                                }

                                s.Registers![reg].Value = w;
                            }

                            MFlags vFlags = (val == 0 ? MFlags.False : val == 1 ? MFlags.True : MFlags.Maybe);
                            s.Registers![reg].Value.Bits![bit].Flags = vFlags;
                            Global.Add_ExecutionStepsInOrder($"Step->{s.MachineName}->PC({s.PC})->BSet(R[{reg}][{bit}])<-{vFlags}");
                            s.PC++;
                        }
                        break;
                    case OpCode.BXor:
                        {
                            Global.Add_ExecutionStepsInOrder($"Step->PC({s.PC})->BXor(R[{instr.Arg1}][{instr.Arg4}], R[{instr.Arg2}][{instr.Arg4}], R[{instr.Arg3}][{instr.Arg4}])");
                            s.Cpu!.BXor(instr.Arg1, instr.Arg2, instr.Arg3, instr.Arg4);
                            s.PC++;
                        }
                        break;
                }
            }

            //Global.Add_ExecutionStepsInOrder($"{s.MachineName}->RunPath->Ended->PC({s.PC})");
            ExecutionSteps.Add($"\t\tRunPath->Ended->PC({s.PC})");
            return s;
        }

        private List<string> mergeStatesList = new List<string>();
        private MachineState MergeStates(MachineState a, MachineState b)
        {
            Global.Add_ExecutionStepsInOrder($"MergeStates->Start");
            mergeStatesList.Add($"\tMergStates->Start");
            MachineState r = a.Clone("MergeStates");

            mergeStatesList.Add($"\t\tMerge {a.MachineName} and {b.MachineName}");
            if (a.ContextFlag == MFlags.Maybe || b.ContextFlag == MFlags.Maybe)
            {
                r.ContextFlag = MFlags.Maybe;
                Global.Add_ExecutionStepsInOrder($"MergeStates->ContextFlag->{r.ContextFlag}");
                mergeStatesList.Add($"\t\tContextFlag({r.ContextFlag})");
            }

            for (int i = 0; i < r.Registers!.Length; i++)
            {
                MBaseWord wa = a.Registers![i].Value;
                MBaseWord wb = b.Registers![i].Value;

                // if both sides are null, nothing to merge
                if (wa == null && wb == null)
                {
                    Global.Add_ExecutionStepsInOrder($"MergeStates->R[{i}]->NothingToMerge");
                    mergeStatesList.Add($"\t\tR[{i}]->NothingToMerge");
                    continue;
                }

                // ensure result word exists
                if (r.Registers[i].Value == null)
                {
                    // pick a type from wa or wb; here assume same type as wa if not null
                    MBaseWord src = wa ?? wb;
                    r.Registers[i].Value = src.Copy(); // or new MWord(src.Bits.Length) etc.
                }

                MBaseWord wr = r.Registers[i].Value;
                for (int bit = 0; bit < wr.Bits!.Length; bit++)
                {
                    MFlags fa = wa?.Bits?[bit].Flags ?? MFlags.Maybe;
                    MFlags fb = wb?.Bits?[bit].Flags ?? MFlags.Maybe;

                    if (fa != fb) { wr.Bits[bit].Flags = MFlags.Maybe; }
                    else
                    {
                        // They agree — check if this agreement came from a Maybe-branch superposition
                        if (a.ContextFlag == MFlags.Maybe && b.ContextFlag == MFlags.Maybe)
                        {
                            wr.Bits[bit].Flags = MFlags.Superposed;
                        }
                        else
                        {
                            wr.Bits[bit].Flags = fa;
                        }
                        //wr.Bits[bit].Flags = fa;
                    }
                }

                Global.Add_ExecutionStepsInOrder($"MergeStates->R[{i}]->Merged");
                mergeStatesList.Add($"\t\tR[{i}]->Merged");
            }

            for (int i = 0; i < r.Memory!.Length; i++)
            {
                if (a.Memory![i] != b.Memory![i])
                {
                    r.ContextFlag = MFlags.Maybe;
                }
            }

            Global.Add_ExecutionStepsInOrder($"MergeStates->Registers->Merged");
            mergeStatesList.Add($"\tRegisters->Merged");
            r.WasMerged = true;
            r.PC++;

            Global.Add_ExecutionStepsInOrder($"MergeStates->Unified->PC({r.PC})");
            mergeStatesList.Add($"\tMergeStates->Unified PC->{r.PC}");
            Global.ExecutionSteps.Add($"Merge", mergeStatesList);
            return r;
        }
    }
}
