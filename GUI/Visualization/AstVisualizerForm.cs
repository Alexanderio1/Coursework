using GUI.Ast;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GUI.Visualization
{
    public sealed class AstVisualizerForm : Form
    {
        public AstVisualizerForm(AstNode root)
        {
            Text = "Визуализация AST";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 800;

            AstCanvas canvas = new AstCanvas(root);
            canvas.Dock = DockStyle.Fill;

            Controls.Add(canvas);
        }
    }

    internal sealed class VisualAstNode
    {
        public string Label { get; private set; }
        public bool IsAttribute { get; private set; }
        public List<VisualAstNode> Children { get; private set; }

        public VisualAstNode(string label, bool isAttribute)
        {
            Label = label;
            IsAttribute = isAttribute;
            Children = new List<VisualAstNode>();
        }

        public void AddChild(VisualAstNode child)
        {
            if (child != null)
                Children.Add(child);
        }

        public void AddRolePrefix(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return;

            Label = role + ": " + Label;
        }
    }

    internal sealed class AstCanvas : Panel
    {
        private readonly VisualAstNode _root;

        private readonly Dictionary<VisualAstNode, RectangleF> _bounds;
        private readonly Dictionary<VisualAstNode, float> _subtreeWidths;

        private const float HorizontalGap = 45.0f;
        private const float VerticalGap = 90.0f;
        private const float Padding = 30.0f;

        public AstCanvas(AstNode root)
        {
            _root = BuildVisualTree(root);

            _bounds = new Dictionary<VisualAstNode, RectangleF>();
            _subtreeWidths = new Dictionary<VisualAstNode, float>();

            AutoScroll = true;
            BackColor = Color.White;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        private VisualAstNode BuildVisualTree(AstNode node)
        {
            if (node == null)
                return null;

            VisualAstNode visualNode = new VisualAstNode(node.NodeType, false);

            foreach (KeyValuePair<string, string> attribute in node.Attributes)
            {
                visualNode.AddChild(
                    new VisualAstNode(
                        attribute.Key + ": " + FormatAttributeValue(attribute.Key, attribute.Value),
                        true));
            }

            foreach (AstNode child in node.Children)
            {
                string role = node.GetChildRole(child);

                VisualAstNode visualChild = BuildVisualTree(child);

                if (visualChild == null)
                    continue;

                visualChild.AddRolePrefix(role);

                visualNode.AddChild(visualChild);
            }

            return visualNode;
        }

        private string FormatAttributeValue(string key, string value)
        {
            if (value == null)
                return "null";

            if (key == "name" || key == "keyword")
                return QuoteIfNeeded(value);

            return value;
        }

        private string QuoteIfNeeded(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value;

            if (value.StartsWith("'") && value.EndsWith("'"))
                return value;

            return "\"" + value + "\"";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_root == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            BuildLayout(e.Graphics);

            e.Graphics.TranslateTransform(
                AutoScrollPosition.X,
                AutoScrollPosition.Y);

            DrawEdges(e.Graphics, _root);
            DrawNodes(e.Graphics, _root);
        }

        private void BuildLayout(Graphics graphics)
        {
            _bounds.Clear();
            _subtreeWidths.Clear();

            ComputeSubtreeWidth(graphics, _root);
            PlaceNode(graphics, _root, Padding, Padding);

            float maxX = 0;
            float maxY = 0;

            foreach (RectangleF rect in _bounds.Values)
            {
                if (rect.Right > maxX)
                    maxX = rect.Right;

                if (rect.Bottom > maxY)
                    maxY = rect.Bottom;
            }

            AutoScrollMinSize = new Size(
                (int)(maxX + Padding),
                (int)(maxY + Padding));
        }

        private float ComputeSubtreeWidth(Graphics graphics, VisualAstNode node)
        {
            SizeF nodeSize = MeasureNode(graphics, node);

            if (node.Children.Count == 0)
            {
                _subtreeWidths[node] = nodeSize.Width;
                return nodeSize.Width;
            }

            float childrenWidth = 0;

            for (int i = 0; i < node.Children.Count; i++)
            {
                childrenWidth += ComputeSubtreeWidth(graphics, node.Children[i]);

                if (i < node.Children.Count - 1)
                    childrenWidth += HorizontalGap;
            }

            float subtreeWidth = Math.Max(nodeSize.Width, childrenWidth);
            _subtreeWidths[node] = subtreeWidth;

            return subtreeWidth;
        }

        private void PlaceNode(Graphics graphics, VisualAstNode node, float x, float y)
        {
            SizeF nodeSize = MeasureNode(graphics, node);
            float subtreeWidth = _subtreeWidths[node];

            float nodeX = x + (subtreeWidth - nodeSize.Width) / 2.0f;

            RectangleF nodeRect = new RectangleF(
                nodeX,
                y,
                nodeSize.Width,
                nodeSize.Height);

            _bounds[node] = nodeRect;

            if (node.Children.Count == 0)
                return;

            float childrenWidth = 0;

            for (int i = 0; i < node.Children.Count; i++)
            {
                childrenWidth += _subtreeWidths[node.Children[i]];

                if (i < node.Children.Count - 1)
                    childrenWidth += HorizontalGap;
            }

            float childX = x + (subtreeWidth - childrenWidth) / 2.0f;
            float childY = y + nodeRect.Height + VerticalGap;

            foreach (VisualAstNode child in node.Children)
            {
                PlaceNode(graphics, child, childX, childY);
                childX += _subtreeWidths[child] + HorizontalGap;
            }
        }

        private SizeF MeasureNode(Graphics graphics, VisualAstNode node)
        {
            SizeF textSize = graphics.MeasureString(
                node.Label,
                Font,
                new SizeF(260.0f, 1000.0f));

            float minWidth = node.IsAttribute ? 120.0f : 150.0f;

            float width = Math.Max(minWidth, textSize.Width + 24.0f);
            float height = Math.Max(45.0f, textSize.Height + 18.0f);

            return new SizeF(width, height);
        }

        private void DrawEdges(Graphics graphics, VisualAstNode node)
        {
            if (!_bounds.ContainsKey(node))
                return;

            RectangleF parentRect = _bounds[node];

            foreach (VisualAstNode child in node.Children)
            {
                if (!_bounds.ContainsKey(child))
                    continue;

                RectangleF childRect = _bounds[child];

                PointF start = new PointF(
                    parentRect.Left + parentRect.Width / 2.0f,
                    parentRect.Bottom);

                PointF end = new PointF(
                    childRect.Left + childRect.Width / 2.0f,
                    childRect.Top);

                using (Pen pen = new Pen(Color.DimGray, 1.6f))
                {
                    pen.CustomEndCap = new AdjustableArrowCap(4, 4);
                    graphics.DrawLine(pen, start, end);
                }

                DrawEdges(graphics, child);
            }
        }

        private void DrawNodes(Graphics graphics, VisualAstNode node)
        {
            if (!_bounds.ContainsKey(node))
                return;

            RectangleF rect = _bounds[node];

            Color fillColor = node.IsAttribute
                ? Color.FromArgb(250, 250, 250)
                : Color.FromArgb(235, 246, 255);

            Color borderColor = node.IsAttribute
                ? Color.Gray
                : Color.SteelBlue;

            using (SolidBrush brush = new SolidBrush(fillColor))
            using (Pen pen = new Pen(borderColor, 1.5f))
            {
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(
                    pen,
                    rect.X,
                    rect.Y,
                    rect.Width,
                    rect.Height);
            }

            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                graphics.DrawString(
                    node.Label,
                    Font,
                    textBrush,
                    rect,
                    format);
            }

            foreach (VisualAstNode child in node.Children)
            {
                DrawNodes(graphics, child);
            }
        }
    }
}