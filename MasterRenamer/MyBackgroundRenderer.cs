using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;

namespace MasterRenamer
{
    class MyBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        private TextEditor _editor;

        public int Line = 0;

        public MyBackgroundRenderer (TextEditor editor)
        {
            _editor = editor;
        }


        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document == null || Line < 1)
                return;

            textView.EnsureVisualLines();
            var highlight = Brushes.LightGray;
            var currentLine = _editor.Document.GetLineByNumber(Line);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                drawingContext.DrawRectangle(highlight, null, new Rect(rect.Location, new Size(rect.Width, rect.Height)));
            }
        }
    }
}
