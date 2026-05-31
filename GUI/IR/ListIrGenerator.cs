using GUI.Ast;
using System;
using System.Collections.Generic;

namespace GUI.IR
{
    public sealed class ListIrGenerator
    {
        private int _temporaryIndex;

        public IrProgram Generate(ProgramNode program)
        {
            _temporaryIndex = 0;

            IrProgram ir = new IrProgram();

            if (program == null)
                return ir;

            foreach (ListDeclarationNode declaration in program.Declarations)
                GenerateDeclaration(declaration, ir);

            return ir;
        }

        private void GenerateDeclaration(ListDeclarationNode declaration, IrProgram ir)
        {
            if (declaration == null || declaration.Initializer == null)
                return;

            List<string> elementTemporaries = new List<string>();

            foreach (AstNode element in declaration.Initializer.Elements)
            {
                string temp = NewTemporary();
                elementTemporaries.Add(temp);

                LiteralNode literal = element as LiteralNode;
                if (literal != null)
                {
                    ir.Instructions.Add(new IrInstruction(
                        temp,
                        "const_" + literal.ValueType.ToLowerInvariant(),
                        literal.RawValue));

                    continue;
                }

                IdentifierNode identifier = element as IdentifierNode;
                if (identifier != null)
                {
                    ir.Instructions.Add(new IrInstruction(
                        temp,
                        "load",
                        identifier.Name));

                    continue;
                }

                ir.Instructions.Add(new IrInstruction(
                    temp,
                    "unknown",
                    element.NodeType));
            }

            string listTemp = NewTemporary();

            ir.Instructions.Add(new IrInstruction(
                listTemp,
                "listof",
                elementTemporaries.ToArray()));

            ir.Instructions.Add(new IrInstruction(
                declaration.Name,
                "assign",
                listTemp));
        }

        private string NewTemporary()
        {
            _temporaryIndex++;
            return "t" + _temporaryIndex;
        }
    }
}