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
            Width = 1100;
            Height = 750;

            AstCanvas canvas = new AstCanvas(root);
            canvas.Dock = DockStyle.Fill;

            Controls.Add(canvas);
        }
    }

    internal sealed class AstCanvas : Panel
    {
        private readonly AstNode _root;

        private readonly Dictionary<AstNode, RectangleF> _bounds;
        private readonly Dictionary<AstNode, float> _subtreeWidths;

        private const float HorizontalGap = 45.0f;
        private const float VerticalGap = 95.0f;
        private const float Padding = 30.0f;

        public AstCanvas(AstNode root)
        {
            _root = root;
            _bounds = new Dictionary<AstNode, RectangleF>();
            _subtreeWidths = new Dictionary<AstNode, float>();

            AutoScroll = true;
            BackColor = Color.White;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
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

        private float ComputeSubtreeWidth(Graphics graphics, AstNode node)
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

        private void PlaceNode(Graphics graphics, AstNode node, float x, float y)
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

            foreach (AstNode child in node.Children)
            {
                PlaceNode(graphics, child, childX, childY);
                childX += _subtreeWidths[child] + HorizontalGap;
            }
        }

        private SizeF MeasureNode(Graphics graphics, AstNode node)
        {
            string text = node.GetMultiLineLabel();

            SizeF textSize = graphics.MeasureString(
                text,
                Font,
                new SizeF(280.0f, 1000.0f));

            float width = Math.Max(150.0f, textSize.Width + 28.0f);
            float height = Math.Max(55.0f, textSize.Height + 20.0f);

            return new SizeF(width, height);
        }

        private void DrawEdges(Graphics graphics, AstNode node)
        {
            if (!_bounds.ContainsKey(node))
                return;

            RectangleF parentRect = _bounds[node];

            foreach (AstNode child in node.Children)
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

                string role = node.GetChildRole(child);

                if (!string.IsNullOrWhiteSpace(role))
                    DrawEdgeLabel(graphics, role, start, end);

                DrawEdges(graphics, child);
            }
        }

        private void DrawEdgeLabel(
            Graphics graphics,
            string text,
            PointF start,
            PointF end)
        {
            PointF center = new PointF(
                (start.X + end.X) / 2.0f,
                (start.Y + end.Y) / 2.0f);

            SizeF textSize = graphics.MeasureString(text, Font);

            RectangleF background = new RectangleF(
                center.X - textSize.Width / 2.0f - 4.0f,
                center.Y - textSize.Height / 2.0f - 2.0f,
                textSize.Width + 8.0f,
                textSize.Height + 4.0f);

            using (SolidBrush backgroundBrush = new SolidBrush(Color.White))
            using (SolidBrush textBrush = new SolidBrush(Color.DimGray))
            {
                graphics.FillRectangle(backgroundBrush, background);

                graphics.DrawString(
                    text,
                    Font,
                    textBrush,
                    background.Left + 4.0f,
                    background.Top + 2.0f);
            }
        }

        private void DrawNodes(Graphics graphics, AstNode node)
        {
            if (!_bounds.ContainsKey(node))
                return;

            RectangleF rect = _bounds[node];

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(235, 246, 255)))
            using (Pen pen = new Pen(Color.SteelBlue, 1.5f))
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
                    node.GetMultiLineLabel(),
                    Font,
                    textBrush,
                    rect,
                    format);
            }

            foreach (AstNode child in node.Children)
            {
                DrawNodes(graphics, child);
            }
        }
    }
}