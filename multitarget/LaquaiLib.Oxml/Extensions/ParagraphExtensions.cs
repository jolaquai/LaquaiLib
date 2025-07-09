using DocumentFormat.OpenXml.Wordprocessing;

namespace LaquaiLib.Oxml.Extensions;

public static class ParagraphExtensions
{
    extension(Paragraph paragraph)
    {
        /// <summary>
        /// Gets or sets the style of the <see cref="Paragraph"/>.
        /// </summary>
        public string Style
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => paragraph.ParagraphProperties?.ParagraphStyleId?.Val ?? "";
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var pPr = paragraph.ParagraphProperties ??= new ParagraphProperties();
                var styleId = pPr.ParagraphStyleId ??= new ParagraphStyleId();
                styleId.Val = value;
            }
        }
    }
}
