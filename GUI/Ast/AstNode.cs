using GUI.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace GUI.Ast
{
    public abstract class AstNode
    {
        public string NodeType { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Dictionary<string, string> Attributes { get; private set; }
        public List<AstNode> Children { get; private set; }

        protected AstNode(string nodeType, int line, int column)
        {
            NodeType = nodeType;
            Line = line;
            Column = column;
            Attributes = new Dictionary<string, string>();
            Children = new List<AstNode>();
        }

        public void AddAttribute(string name, string value)
        {
            if (Attributes.ContainsKey(name))
                Attributes[name] = value;
            else
                Attributes.Add(name, value);
        }

        public void AddChild(AstNode child)
        {
            if (child != null)
                Children.Add(child);
        }

        public string GetOneLineLabel()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(NodeType);

            if (Attributes.Count > 0)
            {
                builder.Append(" [");

                bool first = true;
                foreach (KeyValuePair<string, string> pair in Attributes)
                {
                    if (!first)
                        builder.Append(", ");

                    builder.Append(pair.Key);
                    builder.Append("=");
                    builder.Append(pair.Value);
                    first = false;
                }

                builder.Append("]");
            }

            return builder.ToString();
        }

        public string GetMultiLineLabel()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(NodeType);

            foreach (KeyValuePair<string, string> pair in Attributes)
            {
                builder.Append(pair.Key);
                builder.Append(": ");
                builder.AppendLine(pair.Value);
            }

            return builder.ToString().TrimEnd();
        }

        public string ToTreeString()
        {
            StringBuilder builder = new StringBuilder();
            AppendTree(builder, "", true, true);
            return builder.ToString();
        }

        private void AppendTree(
            StringBuilder builder,
            string prefix,
            bool isLast,
            bool isRoot)
        {
            if (isRoot)
            {
                builder.AppendLine(GetOneLineLabel());
            }
            else
            {
                builder.Append(prefix);
                builder.Append(isLast ? "└── " : "├── ");
                builder.AppendLine(GetOneLineLabel());
            }

            string childPrefix;

            if (isRoot)
            {
                childPrefix = "";
            }
            else
            {
                childPrefix = prefix + (isLast ? "    " : "│   ");
            }

            for (int i = 0; i < Children.Count; i++)
            {
                bool childIsLast = i == Children.Count - 1;
                Children[i].AppendTree(builder, childPrefix, childIsLast, false);
            }
        }
    }

    public sealed class ProgramNode : AstNode
    {
        public List<ListDeclarationNode> Declarations { get; private set; }

        public ProgramNode()
            : base("ProgramNode", 1, 1)
        {
            Declarations = new List<ListDeclarationNode>();
        }

        public void AddDeclaration(ListDeclarationNode declaration)
        {
            if (declaration == null)
                return;

            Declarations.Add(declaration);
            AddChild(declaration);
        }
    }

    public sealed class ListDeclarationNode : AstNode
    {
        public string Name { get; private set; }
        public ListOfNode Initializer { get; private set; }

        public ListDeclarationNode(string name, int line, int column)
            : base("ListDeclarationNode", line, column)
        {
            Name = name;
            AddAttribute("name", name);
            AddAttribute("keyword", "val");
        }

        public void SetInitializer(ListOfNode initializer)
        {
            Initializer = initializer;
            AddChild(initializer);
        }
    }

    public sealed class ListOfNode : AstNode
    {
        public List<AstNode> Elements { get; private set; }

        public ListOfNode(int line, int column)
            : base("ListOfNode", line, column)
        {
            Elements = new List<AstNode>();
            AddAttribute("call", "listOf");
        }

        public void AddElement(AstNode element)
        {
            if (element == null)
                return;

            Elements.Add(element);
            AddChild(element);
        }
    }

    public sealed class LiteralNode : AstNode
    {
        public string ValueType { get; private set; }
        public string RawValue { get; private set; }

        public LiteralNode(string valueType, string rawValue, int line, int column)
            : base("LiteralNode", line, column)
        {
            ValueType = valueType;
            RawValue = rawValue;

            AddAttribute("type", valueType);
            AddAttribute("value", rawValue);
        }
    }

    public sealed class IdentifierNode : AstNode
    {
        public string Name { get; private set; }

        public IdentifierNode(string name, int line, int column)
            : base("IdentifierNode", line, column)
        {
            Name = name;
            AddAttribute("name", name);
        }
    }

    public sealed class AstBuilder
    {
        private IReadOnlyList<LexerItem> _tokens;
        private int _position;

        public ProgramNode Build(IReadOnlyList<LexerItem> tokens)
        {
            _tokens = tokens;
            _position = 0;

            ProgramNode program = new ProgramNode();

            while (!IsAtEnd)
            {
                program.AddDeclaration(ParseDeclaration());
            }

            return program;
        }

        private ListDeclarationNode ParseDeclaration()
        {
            Expect(LexerTokenCode.Val);

            LexerItem nameToken = Expect(LexerTokenCode.Identifier);

            Expect(LexerTokenCode.Assign);

            LexerItem listOfToken = Expect(LexerTokenCode.ListOf);

            Expect(LexerTokenCode.LeftParen);

            ListOfNode listOfNode = new ListOfNode(
                listOfToken.Line,
                listOfToken.StartColumn);

            if (!Check(LexerTokenCode.RightParen))
            {
                ParseElements(listOfNode);
            }

            Expect(LexerTokenCode.RightParen);
            Expect(LexerTokenCode.Semicolon);

            ListDeclarationNode declaration = new ListDeclarationNode(
                nameToken.Lexeme,
                nameToken.Line,
                nameToken.StartColumn);

            declaration.SetInitializer(listOfNode);

            return declaration;
        }

        private void ParseElements(ListOfNode listOfNode)
        {
            listOfNode.AddElement(ParseElement());

            while (Match(LexerTokenCode.Comma))
            {
                listOfNode.AddElement(ParseElement());
            }
        }

        private AstNode ParseElement()
        {
            LexerItem token = Current;

            if (Match(LexerTokenCode.String))
                return new LiteralNode("String", token.Lexeme, token.Line, token.StartColumn);

            if (Match(LexerTokenCode.Char))
                return new LiteralNode("Char", token.Lexeme, token.Line, token.StartColumn);

            if (Match(LexerTokenCode.True))
                return new LiteralNode("Boolean", token.Lexeme, token.Line, token.StartColumn);

            if (Match(LexerTokenCode.False))
                return new LiteralNode("Boolean", token.Lexeme, token.Line, token.StartColumn);

            if (Match(LexerTokenCode.Identifier))
                return new IdentifierNode(token.Lexeme, token.Line, token.StartColumn);

            if (Check(LexerTokenCode.Plus) || Check(LexerTokenCode.Minus))
                return ParseSignedNumber();

            if (Match(LexerTokenCode.Int))
                return new LiteralNode("Int", token.Lexeme, token.Line, token.StartColumn);

            if (Match(LexerTokenCode.Double))
                return new LiteralNode("Double", token.Lexeme, token.Line, token.StartColumn);

            throw new InvalidOperationException(
                "Невозможно построить AST: неожиданный элемент списка.");
        }

        private AstNode ParseSignedNumber()
        {
            LexerItem signToken = Advance();

            LexerItem numberToken = Current;

            if (Match(LexerTokenCode.Int))
            {
                return new LiteralNode(
                    "Int",
                    signToken.Lexeme + numberToken.Lexeme,
                    signToken.Line,
                    signToken.StartColumn);
            }

            if (Match(LexerTokenCode.Double))
            {
                return new LiteralNode(
                    "Double",
                    signToken.Lexeme + numberToken.Lexeme,
                    signToken.Line,
                    signToken.StartColumn);
            }

            throw new InvalidOperationException(
                "Невозможно построить AST: после знака ожидалось число.");
        }

        private bool Match(LexerTokenCode code)
        {
            if (!Check(code))
                return false;

            Advance();
            return true;
        }

        private LexerItem Expect(LexerTokenCode code)
        {
            if (Check(code))
                return Advance();

            throw new InvalidOperationException(
                "Невозможно построить AST: ожидалась лексема " + code + ".");
        }

        private bool Check(LexerTokenCode code)
        {
            if (IsAtEnd)
                return false;

            return Current.Code.HasValue && Current.Code.Value == (int)code;
        }

        private LexerItem Advance()
        {
            LexerItem token = Current;
            _position++;
            return token;
        }

        private bool IsAtEnd
        {
            get
            {
                return _tokens == null || _position >= _tokens.Count;
            }
        }

        private LexerItem Current
        {
            get
            {
                if (IsAtEnd)
                    throw new InvalidOperationException("Достигнут конец списка токенов.");

                return _tokens[_position];
            }
        }
    }
}