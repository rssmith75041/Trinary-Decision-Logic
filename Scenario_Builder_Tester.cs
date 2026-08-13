using System.Configuration;
using System.Globalization;

namespace Trinary_Decision_Logic
{
    public partial class Scenario_Builder_Tester : Form
    {
        private Dictionary<string, List<string>> scenariosList = new Dictionary<string, List<string>>();
        private Dictionary<string, int> setRegisters = new Dictionary<string, int>();
        private List<Instruction> programList = new List<Instruction>();
        private string scenariosFolder = string.Empty;
        private bool scenarioCorrect = false;

        public Scenario_Builder_Tester()
        {
            InitializeComponent();
            scenariosFolder = ConfigurationManager.AppSettings["Scenarios_Folder"]!;
            txtProgram.SelectionStart = txtProgram.Text.Length;
            Load_Scenarios();
        }

        private int sizeOfMemory { get; set; } = 256; //Default

        private void btnCheckScenario_Click(object sender, EventArgs e)
        {
            setRegisters = new Dictionary<string, int>();
            programList = new List<Instruction>();
            scenarioCorrect = true;
            txtOutput.Text = "";

            //Check Instructions
            for (int ndx = 0; ndx < txtProgram.Lines.Length; ndx++)
            {
                string line = txtProgram.Lines[ndx];
                string instLine = line.Replace("{", string.Empty).Replace("}", string.Empty);
                if (instLine.Trim() is "") { continue; }
                string[] parts = instLine.Split(new char[] { ' ', ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length is 0) { continue; }

                if (parts[0].ToUpper() != "OP") { scenarioCorrect = false; Write_Program_Error($"Line[{ndx}]: First argument must be Op"); }
                if (!parts[1].ToUpper().StartsWith("OPCODE.")) { scenarioCorrect = false; Write_Program_Error($"Line[{ndx}]: OpCode used must start with 'OpCode.'"); }
                if (!Enum.IsDefined(typeof(OpCode), parts[1].Replace("OpCode.", string.Empty)))
                {
                    Write_Program_Error($"Line[{ndx}]: Invalid OpCode '{parts[1]}'");
                    scenarioCorrect = false;
                }

                Dictionary<string, int> argList = new Dictionary<string, int>();
                if (!Correct_Args_For_OpCode(parts, ndx, txtProgram.Lines.Length, ref argList)) { scenarioCorrect = false; break; } //Error in instruction opcodes
                Instruction inst = Construct_Instruction(parts, argList);
                programList.Add(inst);
            }

            txtErrors.Text = $"Scenario: {txtScenarioName.Text} is correct";
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (scenarioCorrect)
            {
                if (txtScenarioName.Text.Trim() == "") { txtScenarioName.Text = "Generic"; }
                //Execute
                VM vm = new VM(txtScenarioName.Text, programList.ToArray());
                vm.Run();

                Write_Logging_Ext();
            }
        }

        private Instruction Construct_Instruction(string[] parts, Dictionary<string, int> argList)
        {
            Instruction inst = new Instruction();

            inst.Op = (OpCode)Enum.Parse(typeof(OpCode), parts[1].Replace("OpCode.", string.Empty));
            if (argList.Keys.Count > 0)
            {
                if (argList.ContainsKey("Arg1")) { inst.Arg1 = argList["Arg1"]; }
                if (argList.ContainsKey("Arg2")) { inst.Arg2 = argList["Arg2"]; }
                if (argList.ContainsKey("Arg3")) { inst.Arg3 = argList["Arg3"]; }
                if (argList.ContainsKey("Arg4")) { inst.Arg4 = argList["Arg4"]; }
            }

            return inst;
        }

        private bool Correct_Args_For_OpCode(string[] parts, int lineNdx, int totalLines, ref Dictionary<string, int> argValues)
        {
            // The default number of registers in VM is 8

            //Args# start at [2]
            string opCode = parts[1].Replace("OpCode.", string.Empty);
            switch (opCode)
            {
                case "BAnd":                //R[Arg1][Arg4] = R[Arg2][Arg4] AND R[Arg3][Arg4]
                case "BOr":                 //R[Arg1][Arg4] = R[Arg2][Arg4] OR R[Arg3][Arg4]
                case "BXor":                //R[Arg1][Arg4] = R[Arg2][Arg4] XOR R[Arg3][Arg4]
                    {
                        //All registers must be setup using RSet so size can be established.
                        if (parts.Length != 10) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        int reg1Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        int reg2Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL must be 0 - 8 for register."); return false; }
                        int reg3Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg3", val);

                        //Make sure all registers are same size
                        if (reg1Size != reg2Size && reg1Size != reg3Size) { Write_Program_Error($"Line[{lineNdx}]: All registers must be same size."); return false; }

                        if (parts[8] != "Arg4") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[9], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg4 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > reg1Size) { Write_Program_Error($"Line[{lineNdx}]: Arg4 = VAL must be between 0 and {reg1Size}."); return false; }
                        //Need to make sure Arg4 (Bit addr) does not fall outside bounds of R[Arg1], R[Arg2] and R[Arg3]
                        argValues.Add("Arg4", val);
                    }
                    break;
                case "And":                //R[Arg1] = R[Arg2] & R[Arg3]
                case "Or":                 //R[Arg1] = R[Arg2] | R[Arg3]
                case "Xor":                //R[Arg1] = R[Arg2] XOR R[Arg3]
                    {
                        if (parts.Length != 8) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        int reg1Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        int reg2Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL must be 0 - 8 for register."); return false; }
                        int reg3Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg3", val);

                        //Make sure all registers are same size
                        if (reg1Size != reg2Size && reg1Size != reg3Size) { Write_Program_Error($"Line[{lineNdx}]: All registers must be same size."); return false; }
                    }
                    break;
                case "BBranchMaybe":
                    {
                        //{ Op = OpCode.BBranchMaybe, Arg1 = 2, Arg2 = 3, Arg3 = 4, Arg4 = 6 }
                        // R[Arg1][Arg2] = BBranchMaybe Arg3 = True PC(Bit), Arg4 = False PC(Bit)
                        if (parts.Length != 10) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        int sizeOfReg = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg1", val);

                        //Bit for R{Arg1][Arg2]
                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        if (val < 0 || val > sizeOfReg) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = Bit({val}) outside register bounds."); return false; }
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > totalLines - 1) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = PC({val}) outside instruction count bounds."); return false; }
                        argValues.Add("Arg3", val);

                        if (parts[8] != "Arg4") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ..., Arg4 = ... in order."); return false; }
                        if (!int.TryParse(parts[9], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg4 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > totalLines - 1) { Write_Program_Error($"Line[{lineNdx}]: Arg4 = PC({val}) outside instruction count bounds."); return false; }
                        argValues.Add("Arg4", val);
                    }
                    break;
                case "BranchMaybe":         //R[Arg1] = BranchMaybe Arg2 = True PC, Arg3 = False -> PC (PC = Program Counter -> 0 indexed)
                    {
                        if (parts.Length != 8) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > totalLines - 1) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = PC({val}) outside instruction count bounds."); return false; }
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > totalLines - 1) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = PC({val}) outside instruction count bounds."); return false; }
                        argValues.Add("Arg3", val);
                    }
                    break;
                case "BMerge":
                case "Merge": { if (parts.Length > 2) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}."); return false; } } break; //No Args
                case "MInit":               //M[Size] = Arg1 -> MBit (EX: 256) -> Max 1024
                    {
                        if (parts.Length != 4) { Write_Program_Error($"Line[{lineNdx}]: Not enough instruction settings."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 1024) { Write_Program_Error($"MInit Size({val}): must be between 256 and 1024"); return false; }
                        argValues.Add("Arg1", val);
                        sizeOfMemory = val;
                    }
                    break;
                case "MStore":              //M[Arg1] = Arg2
                    {
                        if (parts.Length != 6) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > sizeOfMemory) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL out of memory bounds[0-{sizeOfMemory}]."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (parts[5].StartsWith("0x"))
                        {
                            string intHex = parts[5].Replace("0x", string.Empty);
                            if (!int.TryParse(intHex, NumberStyles.HexNumber, null, out val))
                            {
                                Write_Program_Error($"Line[{lineNdx}]: Arg2 = 0xVAL is not an 'int' value.");
                                return false;
                            }
                        }
                        else if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        //If larger than int returns false
                        argValues.Add("Arg2", val);
                    }
                    break;
                case "BNot":                //R[Arg1][Arg3] = NOT R[Arg2][Arg3]
                    {
                        if (parts.Length != 8) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        int reg1Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        int reg2Size = setRegisters[$"R[{val}]"];
                        argValues.Add("Arg2", val);

                        //Make sure all registers are same size
                        if (reg1Size != reg2Size) { Write_Program_Error($"Line[{lineNdx}]: All registers must be same size."); return false; }

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > reg1Size) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = Bit({val}) outside register bounds."); return false; }
                        argValues.Add("Arg3", val);
                    }
                    break;
                case "Not":                 //R[Arg1] = NOT R[Arg2]
                    {
                        if (parts.Length != 6) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg2", val);
                    }
                    break;
                case "RRSet":               //R[Arg1] = R[Arg2], R[Arg1] and R[Arg2] size must be set using RSet (Or could cause R[Arg1] to grow to accomodate R[Arg2]
                    {
                        if (parts.Length != 6) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg2", val);
                    }
                    break;
                case "BMStore":             //M[Arg1] = MBit(Arg2)
                    {
                        if (parts.Length != 6) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > sizeOfMemory) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = {val} out of memory bounds[0-{sizeOfMemory}]."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[4], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        if (val < 0 || val > 2) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be [0, 1 or 2]."); return false; }
                        argValues.Add("Arg2", val);
                    }
                    break;
                case "RMStore":             //M[Arg1] = R[Arg2]
                    {
                        if (parts.Length != 6) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > sizeOfMemory) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL out of memory bounds[0-{sizeOfMemory}]."); return false; }
                        argValues.Add("Arg1", val);

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg2", val);
                    }
                    break;
                case "RSet":               //R[Arg1] = Arg2 (0=False, 1=True, 2=Maybe) -> Size of Arg3(0=MWord, 1=MByte, 2=MDWord, 3=MQWord)
                    {
                        if (parts.Length != 8) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg1", val);
                        int reg = val;

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 2) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL must be [0, 1 or 2]."); return false; }
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 3) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be [0, 1, 2 or 3]."); return false; }
                        argValues.Add("Arg3", val);

                        if (!setRegisters.ContainsKey($"R[{reg}]"))
                        {
                            setRegisters.Add($"R[{reg}]", val is 0 ? 8 : val is 1 ? 16 : val is 2 ? 32 : 64);
                        }
                        else { setRegisters[$"R[{reg}]"] = val is 0 ? 8 : val is 1 ? 16 : val is 2 ? 32 : 64; }
                    }
                    break;
                case "MRSet":           // R[Arg1][Arg2] = M[Arg3] -> R[Arg1] must be already set to size using RSet
                    {
                        if (parts.Length != 10) { Write_Program_Error($"Line[{lineNdx}]: Op = OpCode.{opCode}, Arg1 = ..., Arg2 = ..., Arg3 = ..."); return false; }
                        if (parts[2] != "Arg1") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[3], out int val)) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > 8) { Write_Program_Error($"Line[{lineNdx}]: Arg1 = VAL must be 0 - 8 for register."); return false; }
                        argValues.Add("Arg1", val);
                        int reg = val;

                        if (!setRegisters.ContainsKey($"R[{reg}]")) { return false; }
                        int sizeOfReg = setRegisters[$"R[{reg}]"];

                        if (parts[4] != "Arg2") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[5], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > sizeOfReg) { Write_Program_Error($"Line[{lineNdx}]: Arg2 = VAL out of register bounds[0-{sizeOfReg}]."); return false; }
                        argValues.Add("Arg2", val);

                        if (parts[6] != "Arg3") { Write_Program_Error($"Line[{lineNdx}]: Arg1 = ..., Arg2 = ..., Arg3 = ... in order."); return false; }
                        if (!int.TryParse(parts[7], out val)) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL is not an 'int' value."); return false; }
                        else if (val < 0 || val > sizeOfMemory) { Write_Program_Error($"Line[{lineNdx}]: Arg3 = VAL out of memory bounds[0-{sizeOfMemory}]."); return false; }
                        argValues.Add("Arg3", val);
                    }
                    break;
                default: { Write_Program_Error($"Line[{lineNdx}]: Invalid Op = OpCode.{opCode}"); return false; }
            }

            return true;
        }

        private void Write_Program_Error(string msg)
        {
            txtErrors.Text += $"{msg}\r\n";
        }

        private void Write_Logging_Ext()
        {
            for (int ndx = 0; ndx < Global.ExecutionStepsInOrder.Count; ndx++)
            {
                txtOutput.AppendText($"{Global.ExecutionStepsInOrder[ndx]}\r\n");
            }
        }

        private void cmbScenario_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbScenario.SelectedIndex > 0)
            {
                txtProgram.Text = "";
                txtErrors.Text = "";
                txtOutput.Text = "";

                txtProgram.Lines = scenariosList[cmbScenario.Text].ToArray();
                txtScenarioName.Text = cmbScenario.Text;
            }
        }

        private void btnSaveScenario_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = scenariosFolder;
            sfd.Filter = "Scenario Files (*.tdl)|*.tdl";
            sfd.DefaultExt = "tdl";
            sfd.AddExtension = true;

            if (sfd.ShowDialog() is DialogResult.OK)
            {
                string fileNameAndPath = sfd.FileName;
                File.WriteAllText(fileNameAndPath, txtProgram.Text);
            }
        }

        private void Load_Scenarios()
        {
            if (string.IsNullOrEmpty(scenariosFolder) || Directory.GetFiles(scenariosFolder).Length is 0)
            {
                scenariosList.Add("Select...", new List<string>());
                scenariosList.Add("Scenario 1", scenario1);
                scenariosList.Add("Scenario 2", scenario2);
                scenariosList.Add("Scenario 3", scenario3);
                scenariosList.Add("Scenario 4", scenario4);
                scenariosList.Add("Scenario 5", scenario5);
                scenariosList.Add("Scenario 6", scenario6);
                scenariosList.Add("Scenario 7", scenario7);
                scenariosList.Add("Scenario X", scenarioX);
                scenariosList.Add("Scenario Y", scenarioY);
            }
            else
            {
                scenariosList.Add("Select...", new List<string>());
                string[] scenarioFiles = Directory.GetFiles(scenariosFolder);
                foreach (string fileName in scenarioFiles)
                {
                    FileInfo fi = new FileInfo(fileName);

                    scenariosList.Add(fi.Name.Replace(".tdl", string.Empty), File.ReadLines(fileName).ToList());
                }
            }

            cmbScenario.DataSource = scenariosList.Keys.ToList();
        }

        //Default scenarios to load into list if none found in configured folder
        private List<string> scenario1 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }",  // R0[0] = Maybe
            // Branch on R0, PC(True) -> 2, PC(False) -> 3
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 2, Arg3 = 3 }",
            "{ Op = OpCode.Merge }",
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario2 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 1, Arg2 = 1, Arg3 = 0 }",  // R0[1] = True
            "{ Op = OpCode.RSet, Arg1 = 2, Arg2 = 0, Arg3 = 0 }",  // R0[2] = False
            "{ Op = OpCode.RSet, Arg1 = 3, Arg2 = 2, Arg3 = 0 }",  // R0[3] = Maybe
            //Add (XOR)
            "{ Op = OpCode.Add, Arg1 = 3, Arg2 = 1, Arg3 = 2 }",    // R[3] = R[3] + R[2] (XOR)
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario3 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }",  // R0[0] = Maybe
            "{ Op = OpCode.RSet, Arg1 = 1, Arg2 = 1, Arg3 = 0 }",  // R0[1] = True
            // Branch on R0, PC(True) -> 3, PC(False) -> 4
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 3, Arg3 = 4 }",
            "{ Op = OpCode.Merge }",
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario4 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }",  // R0[0] = Maybe
            "{ Op = OpCode.RSet, Arg1 = 5, Arg2 = 1, Arg3 = 0 }",  // R0[5] = True
            // Branch on R0, PC(True) -> 3, PC(False) -> 5
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 3, Arg3 = 5 }",
            // TRUE PATH (PC = 3)
            "{ Op = OpCode.RSet, Arg1 = 5, Arg2 = 1, Arg3 = 1 }",  // R5[0] = True -> MWord
            "{ Op = OpCode.Merge }",
            // FALSE PATH (PC = 5)
            "{ Op = OpCode.RSet, Arg1 = 5, Arg2 = 0, Arg3 = 1 }",  // R5[0] = False -> MWord
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario5 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }",  // R0[0] = Maybe
            // Branch on R0 -> PC(True) 2, PC(False) -> 5
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 2, Arg3 = 5 }",
            // TRUE PATH (PC = 2)
            "{ Op = OpCode.MInit,  Arg1 = 256 }",
            "{ Op = OpCode.MStore, Arg1 = 100, Arg2 = 0x55 }",      //M[100] = 0x55
            "{ Op = OpCode.Merge }",
            // FALSE PATH (PC = 5)
            "{ Op = OpCode.MInit,  Arg1 = 256 }",
            "{ Op = OpCode.MStore, Arg1 = 100, Arg2 = 0xAA }",      //M[100] = 0xAA
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario6 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }", // R0[0] = Maybe
            "{ Op = OpCode.RSet, Arg1 = 1, Arg2 = 1, Arg3 = 0 }", // R1[0] = True
            "{ Op = OpCode.RSet, Arg1 = 2, Arg2 = 1, Arg3 = 0 }", // R2[0] = True
            // Add (XOR)
            "{ Op = OpCode.Add, Arg1 = 1, Arg2 = 1, Arg3 = 2 }",   // R1 = R1 + R2 (XOR)
            // BranchMaybe (both paths jump to Merge)
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 5, Arg3 = 5 }",
            // Merge
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenario7 = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet, Arg1 = 0, Arg2 = 2, Arg3 = 0 }", // R0[0] = Maybe
            "{ Op = OpCode.RSet, Arg1 = 1, Arg2 = 1, Arg3 = 0 }", // R1[0] = True    
            // Degenerate branch (both paths jump to PC = 2)
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 3, Arg3 = 3 }",
            // Merge immediately (no divergent instructions)
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenarioX = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet,      Arg1 = 0, Arg2 = 2, Arg3 = 0 }",    // R0 = Maybe
            "{ Op = OpCode.RSet,      Arg1 = 1, Arg2 = 1, Arg3 = 0 }",    // R1 = True
            "{ Op = OpCode.RSet,      Arg1 = 3, Arg2 = 1, Arg3 = 0 }",    // R3 = True
            "{ Op = OpCode.RSet,      Arg1 = 5, Arg2 = 1, Arg3 = 0 }",    // R5 = False
            "{ Op = OpCode.RSet,      Arg1 = 7, Arg2 = 0, Arg3 = 0 }",    // R7 = False
            // Branch on R0, -> PC(True) -> 6, PC(False) -> 13
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 6, Arg3 = 13 }",
            // TRUE PATH (starts at PC = 6)
            "{ Op = OpCode.MInit,      Arg1 = 256 }",
            "{ Op = OpCode.RMStore,    Arg1 = 1,   Arg2 = 5 }",            // MEM[1] = R5
            "{ Op = OpCode.Add,        Arg1 = 2,   Arg2 = 1, Arg3 = 3 }",  // R2 = R1 + R3 (XOR)
            "{ Op = OpCode.MInit,      Arg1 = 256 }",
            "{ Op = OpCode.RMStore,    Arg1 = 100, Arg2 = 2 }",            // MEM[100] = R2
            "{ Op = OpCode.Add,        Arg1 = 4,   Arg2 = 2, Arg3 = 1 }",  // R4 = R2 + R1 (XOR)
            "{ Op = OpCode.Merge }",
            // FALSE PATH (starts at PC = 13)
            "{ Op = OpCode.MInit,      Arg1 = 256 }",
            "{ Op = OpCode.RMStore,    Arg1 = 1,   Arg2 = 7 }",            // MEM[1] = R7
            "{ Op = OpCode.Add,        Arg1 = 2,   Arg2 = 1, Arg3 = 3 }",  // R2 = R1 + R3 (XOR)
            "{ Op = OpCode.MInit,      Arg1 = 256 }",
            "{ Op = OpCode.RMStore,    Arg1 = 100, Arg2 = 2 }",            // MEM[100] = R2
            "{ Op = OpCode.Add,        Arg1 = 4,   Arg2 = 2, Arg3 = 1 }",  // R4 = R2 + R1 (XOR)
            "{ Op = OpCode.Merge }"
        };

        private List<string> scenarioY = new List<string>()
        {
            // Initialize registers (pure instructions)
            "{ Op = OpCode.RSet,      Arg1 = 0, Arg2 = 2, Arg3 = 0 }",    // R0 = Maybe
            "{ Op = OpCode.RSet,      Arg1 = 1, Arg2 = 2, Arg3 = 0 }",    // R1 = Maybe
            // Branch on R0, -> PC(True) -> 2, PC(False) -> 2
            "{ Op = OpCode.BranchMaybe, Arg1 = 0, Arg2 = 4, Arg3 = 4 }",
            // Branch on R1, -> PC(True) -> 2, PC(False) -> 2
            "{ Op = OpCode.BranchMaybe, Arg1 = 1, Arg2 = 6, Arg3 = 6 }",
            "{ Op = OpCode.Merge }"
        };
    }
}
