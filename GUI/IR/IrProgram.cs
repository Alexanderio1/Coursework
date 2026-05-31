using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GUI.IR
{
    public sealed class IrProgram
    {
        public List<IrInstruction> Instructions { get; private set; }

        public IrProgram()
        {
            Instructions = new List<IrInstruction>();
        }

        public IrProgram(IEnumerable<IrInstruction> instructions)
        {
            Instructions = instructions == null
                ? new List<IrInstruction>()
                : instructions.ToList();
        }

        public IrProgram Clone()
        {
            return new IrProgram(Instructions.Select(x => x.Clone()));
        }

        public string ToDisplayText()
        {
            StringBuilder builder = new StringBuilder();

            foreach (IrInstruction instruction in Instructions)
                builder.AppendLine(instruction.ToString());

            return builder.ToString().TrimEnd();
        }
    }
}