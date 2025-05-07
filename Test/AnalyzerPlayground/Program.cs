using DocumentFormat.OpenXml.Wordprocessing;

namespace AnalyzerPlayground;

internal static class Program
{
    public static void Main(string[] args)
    {
        Paragraph p = null;
        _ = (Paragraph)p.Clone();
        _ = (Paragraph)p.CloneNode(true);
        _ = (Paragraph)p.CloneNode(false);
    }
}
