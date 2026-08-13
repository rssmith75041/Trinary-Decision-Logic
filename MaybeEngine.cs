using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Trinary_Decision_Logic
{
    public class MaybeEngine
    {
        private List<string> ExecutionStepsFalse = new List<string>();
        private List<string> ExecutionStepsTrue = new List<string>();
        private List<string> ExecutionSteps = new List<string>();

        public Func<MachineState, MachineState, MachineState>? MergeStates;
        public Func<MachineState, int, MachineState>? RunPath;
        private Task<MachineState> RunPathAsync(MachineState s, int startPC)
        {
            return Task.Run(() => RunPath!(s, startPC));
        }

        public int BranchMaybe(MachineState s, int condReg, int truePC, int falsePC)
        {
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->Start");
            ExecutionSteps.Add($"\tBranchMaybe->Started");
            MBaseWord cond = s.Registers![condReg].Value;

            // Multi-bit Maybe detection
            bool anyFalse = cond.Bits!.Any(b => b.Flags == MFlags.False);
            bool anyTrue = cond.Bits!.Any(b => b.Flags == MFlags.True);
            bool isMaybe = anyTrue && anyFalse;

            if (!isMaybe)
            {
                if (anyTrue) return truePC;
                if (anyFalse) return falsePC;
            }

            // Maybe branch: clone both paths
            MachineState falseState = s.Clone("FalseState");
            ExecutionStepsFalse.Add($"\t{falseState.MachineName}");

            MachineState trueState = s.Clone("TrueState");
            ExecutionStepsTrue.Add($"\t{trueState.MachineName}");

            falseState.ContextFlag = MFlags.Maybe;
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->FalseState->InitialContextFlag->{falseState.ContextFlag}");
            ExecutionStepsFalse.Add($"\tInitial ContextFlag->{falseState.ContextFlag}");

            trueState.ContextFlag = MFlags.Maybe;
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->TrueState->InitialContextFlag->{trueState.ContextFlag}");
            ExecutionStepsTrue.Add($"\tInitial ContextFlag->{trueState.ContextFlag}");

            falseState.PC = falsePC;
            trueState.PC = truePC;

            Global.Add_ExecutionStepsInOrder($"BranchMaybe->FalseState->Start->RunPath->PC({falseState.PC})");
            ExecutionStepsFalse.Add($"\tStart RunPath PC->{falseState.PC}");
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->TrueState->Start->RunPath->PC({trueState.PC})");
            ExecutionStepsTrue.Add($"\tStart RunPath PC->{trueState.PC}");

            Task<MachineState> tFalse = RunPathAsync(falseState, falsePC);
            Task<MachineState> tTrue = RunPathAsync(trueState, truePC);
            Task.WaitAll(tTrue, tFalse);

            MachineState mergedFalse = tFalse.Result;
            MachineState mergedTrue = tTrue.Result;

            Global.Add_ExecutionStepsInOrder($"BranchMaybe->FalseState->RunPath->Ended->PC({falseState.PC})");
            ExecutionStepsFalse.Add($"\tRunPath Ended PC->{mergedFalse.PC}");
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->TrueState->RunPath->Ended->PC({trueState.PC})");
            ExecutionStepsTrue.Add($"\tRunPath Ended PC->{mergedTrue.PC}");

            Global.ExecutionSteps.Add("False", ExecutionStepsFalse);
            Global.ExecutionSteps.Add("True", ExecutionStepsTrue);

            MachineState merged = MergeStates!(mergedTrue, mergedFalse);
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->Merged{merged.MachineName}->MergedStates->PC({merged.PC})");
            ExecutionSteps.Add($"\tMerged->{merged.MachineName}->MergedStates->PC({merged.PC})");

            // Copy merged back into s
            ExecutionSteps.Add($"\t{merged.MachineName}->CopyMerge->BackTo->{s.MachineName}");
            s.ContextFlag = merged.ContextFlag;
            s.Registers = merged.Registers;
            s.WasMerged = merged.WasMerged;
            s.Memory = merged.Memory;

            s.PC = Math.Max(mergedTrue.PC, mergedFalse.PC);
            Global.Add_ExecutionStepsInOrder($"BranchMaybe->Ended->PC({s.PC})");
            ExecutionSteps.Add($"\tFinalPC->{s.PC}");
            Global.ExecutionSteps.Add("BranchMaybe", ExecutionSteps);
            return s.PC;
        }

        public int BBranchMaybe(MachineState s, int condReg, int condBit, int truePC, int falsePC)
        {
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->Start");
            ExecutionSteps.Add($"\tBBranchMaybe->Started");
            MBit bit = s.Registers![condReg].Value!.Bits![condBit];

            // Deterministic branch
            if (bit.Flags == MFlags.True) { return truePC; }
            if (bit.Flags == MFlags.False) { return falsePC; }

            // Maybe branch → fork futures
            MachineState falseState = s.Clone("FalseState");
            ExecutionStepsFalse.Add($"\t{falseState.MachineName}");

            MachineState trueState = s.Clone("TrueState");
            ExecutionStepsTrue.Add($"\t{trueState.MachineName}");

            falseState.ContextFlag = MFlags.Maybe;
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->FalseState->InitialContextFlag->{falseState.ContextFlag}");
            ExecutionStepsFalse.Add($"\tInitial ContextFlag->{falseState.ContextFlag}");

            trueState.ContextFlag = MFlags.Maybe;
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->TrueState->InitialContextFlag->{trueState.ContextFlag}");
            ExecutionStepsTrue.Add($"\tInitial ContextFlag->{trueState.ContextFlag}");

            falseState.PC = falsePC;
            trueState.PC = truePC;

            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->FalseState->Start->RunPath->PC({falseState.PC})");
            ExecutionStepsFalse.Add($"\tStart RunPath PC->{falseState.PC}");
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->TrueState->Start->RunPath->PC({trueState.PC})");
            ExecutionStepsTrue.Add($"\tStart RunPath PC->{trueState.PC}");

            Task<MachineState> tFalse = RunPathAsync(falseState, falsePC);
            Task<MachineState> tTrue = RunPathAsync(trueState, truePC);
            Task.WaitAll(tTrue, tFalse);

            MachineState mergedFalse = tFalse.Result;
            MachineState mergedTrue = tTrue.Result;

            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->FalseState->RunPath->Ended->PC({falseState.PC})");
            ExecutionStepsFalse.Add($"\tRunPath Ended PC->{mergedFalse.PC}");
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->TrueState->RunPath->Ended->PC({trueState.PC})");
            ExecutionStepsTrue.Add($"\tRunPath Ended PC->{mergedTrue.PC}");

            Global.ExecutionSteps.Add("False", ExecutionStepsFalse);
            Global.ExecutionSteps.Add("True", ExecutionStepsTrue);

            MachineState merged = MergeStates!(tTrue.Result, tFalse.Result);
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->Merged{merged.MachineName}->MergedStates->PC({merged.PC})");
            ExecutionSteps.Add($"\tMerged->{merged.MachineName}->MergedStates->PC({merged.PC})");

            // Copy merged back
            ExecutionSteps.Add($"\t{merged.MachineName}->CopyMerge->BackTo->{s.MachineName}");
            s.ContextFlag = merged.ContextFlag;
            s.Registers = merged.Registers;
            s.WasMerged = merged.WasMerged;
            s.Memory = merged.Memory;

            s.PC = Math.Max(tTrue.Result.PC, tFalse.Result.PC);
            Global.Add_ExecutionStepsInOrder($"BBranchMaybe->Ended->PC({s.PC})");
            ExecutionSteps.Add($"\tFinalPC->{s.PC}");
            Global.ExecutionSteps.Add("BBranchMaybe", ExecutionSteps);
            return s.PC;
        }
    }
}
