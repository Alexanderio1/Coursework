using GUI.Ast;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GUI.Semantics
{
    public sealed class SemanticError
    {
        public string InvalidFragment { get; set; }
        public string Message { get; set; }

        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int AbsoluteIndex { get; set; }

        public string LocationText
        {
            get
            {
                return string.Format("строка {0}, символ {1}", Line, StartColumn);
            }
        }
    }

    public sealed class SemanticResult
    {
        public List<SemanticError> Errors { get; private set; }


        public ProgramNode ValidAst { get; private set; }

        public SemanticResult()
        {
            Errors = new List<SemanticError>();
            ValidAst = new ProgramNode();
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }

        public int ErrorCount
        {
            get { return Errors.Count; }
        }

        public void AddError(AstNode node, string fragment, string message)
        {
            Errors.Add(new SemanticError
            {
                InvalidFragment = fragment,
                Message = message,
                Line = node.Line,
                StartColumn = node.Column,
                EndColumn = node.Column + Math.Max(fragment == null ? 1 : fragment.Length, 1) - 1,
                AbsoluteIndex = 0
            });
        }

        public void AddValidDeclaration(ListDeclarationNode declaration)
        {
            ValidAst.AddDeclaration(declaration);
        }
    }

    public sealed class SymbolEntry
    {
        public string Name { get; private set; }
        public string DeclaredType { get; private set; }
        public string ElementType { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public SymbolEntry(
            string name,
            string declaredType,
            string elementType,
            int line,
            int column)
        {
            Name = name;
            DeclaredType = declaredType;
            ElementType = elementType;
            Line = line;
            Column = column;
        }
    }

    public sealed class SymbolTable
    {
        private readonly Dictionary<string, SymbolEntry> _symbols;

        public SymbolTable()
        {
            _symbols = new Dictionary<string, SymbolEntry>();
        }

        public bool Declare(SymbolEntry entry, out SymbolEntry existing)
        {
            existing = null;

            if (_symbols.ContainsKey(entry.Name))
            {
                existing = _symbols[entry.Name];
                return false;
            }

            _symbols.Add(entry.Name, entry);
            return true;
        }

        public SymbolEntry Lookup(string name)
        {
            if (_symbols.ContainsKey(name))
                return _symbols[name];

            return null;
        }

        public bool CheckDuplicate(string name)
        {
            return _symbols.ContainsKey(name);
        }
    }

    public sealed class SemanticAnalyzer
    {
        private SymbolTable _symbols;

        public SemanticResult Analyze(ProgramNode program)
        {
            _symbols = new SymbolTable();

            SemanticResult result = new SemanticResult();

            if (program == null)
                return result;

            foreach (ListDeclarationNode declaration in program.Declarations)
            {
                AnalyzeDeclaration(declaration, result);
            }

            return result;
        }

        private void AnalyzeDeclaration(
            ListDeclarationNode declaration,
            SemanticResult result)
        {

            SymbolEntry existing = _symbols.Lookup(declaration.Name);

            if (existing != null)
            {
                result.AddError(
                    declaration,
                    declaration.Name,
                    "Повторное объявление идентификатора \"" +
                    declaration.Name +
                    "\". Первое объявление находится в строке " +
                    existing.Line +
                    ".");

                return;
            }

            string elementType = AnalyzeInitializer(declaration, result);
            string listType = "List<" + elementType + ">";

            declaration.AddAttribute("type", listType);

            if (declaration.Initializer != null)
                declaration.Initializer.AddAttribute("elementType", elementType);

            SymbolEntry entry = new SymbolEntry(
                declaration.Name,
                listType,
                elementType,
                declaration.Line,
                declaration.Column);

            SymbolEntry duplicate;
            _symbols.Declare(entry, out duplicate);


            result.AddValidDeclaration(declaration);
        }

        private string AnalyzeInitializer(
            ListDeclarationNode declaration,
            SemanticResult result)
        {


            if (declaration.Initializer == null)
                return "Unknown";


            if (declaration.Initializer.Elements.Count == 0)
                return "Any";

            List<string> elementTypes = new List<string>();

            foreach (AstNode element in declaration.Initializer.Elements)
            {
                string currentType = ResolveElementType(element, result);

                if (!string.IsNullOrEmpty(currentType))
                    elementTypes.Add(currentType);
            }

            if (elementTypes.Count == 0)
                return "Unknown";

            string firstType = elementTypes[0];

            for (int i = 1; i < elementTypes.Count; i++)
            {
                if (elementTypes[i] != firstType)
                    return "Any";
            }

            return firstType;
        }

        private string ResolveElementType(AstNode element, SemanticResult result)
        {
            LiteralNode literal = element as LiteralNode;
            if (literal != null)
            {
                ValidateLiteral(literal, result);
                return literal.ValueType;
            }

            IdentifierNode identifier = element as IdentifierNode;
            if (identifier != null)
            {
                SymbolEntry entry = _symbols.Lookup(identifier.Name);

                if (entry == null)
                {
                    result.AddError(
                        identifier,
                        identifier.Name,
                        "Использование необъявленного идентификатора \"" +
                        identifier.Name +
                        "\".");

                    return null;
                }

                identifier.AddAttribute("resolvedType", entry.DeclaredType);
                return entry.DeclaredType;
            }

            return null;
        }

        private void ValidateLiteral(LiteralNode literal, SemanticResult result)
        {
            if (literal.ValueType == "Int")
            {
                int value;
                if (!int.TryParse(
                    literal.RawValue,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value))
                {
                    result.AddError(
                        literal,
                        literal.RawValue,
                        "Значение целочисленного литерала выходит за пределы типа Int32.");
                }

                return;
            }

            if (literal.ValueType == "Double")
            {
                double value;
                if (!double.TryParse(
                    literal.RawValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value))
                {
                    result.AddError(
                        literal,
                        literal.RawValue,
                        "Некорректное значение вещественного литерала.");
                }

                return;
            }

            if (literal.ValueType == "Char")
            {
                string raw = literal.RawValue;

                if (raw == null || raw.Length < 3)
                {
                    result.AddError(
                        literal,
                        raw,
                        "Символьный литерал должен содержать ровно один символ.");

                    return;
                }

                string inner = raw.Substring(1, raw.Length - 2);

                if (inner.Length != 1)
                {
                    result.AddError(
                        literal,
                        raw,
                        "Символьный литерал должен содержать ровно один символ.");
                }
            }
        }
    }
}