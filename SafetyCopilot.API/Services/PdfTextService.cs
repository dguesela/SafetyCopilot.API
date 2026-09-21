using SafetyCopilot.API.Services.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace SafetyCopilot.API.Services;

public class PdfTextService : IPdfTextService
{
    public Task<string> ExtractTextAsync(
        byte[] pdfContent,
        CancellationToken cancellationToken = default)
    {
        if (pdfContent == null ||
            pdfContent.Length == 0)
        {
            throw new ArgumentException(
                "The PDF content is empty.",
                nameof(pdfContent));
        }

        var builder =
            new StringBuilder();

        using var stream =
            new MemoryStream(
                pdfContent);

        using var document =
            PdfDocument.Open(
                stream);

        foreach (var page
                 in document.GetPages())
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            /*
             * IMPORTANT:
             *
             * Do NOT use:
             *
             *     page.Text
             *
             * For this PDF, page.Text can produce:
             *
             *     Thesystemshallprovide...
             *
             * PdfPig's GetWords() gives us individual
             * words, allowing us to rebuild text with
             * normal spaces.
             */
            var words =
                page
                    .GetWords()
                    .ToList();

            if (words.Count == 0)
            {
                continue;
            }

            foreach (var word
                     in words)
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(
                        word.Text))
                {
                    continue;
                }

                var cleanedWord =
                    CleanWord(
                        word.Text);

                if (string.IsNullOrWhiteSpace(
                        cleanedWord))
                {
                    continue;
                }

                builder.Append(
                    cleanedWord);

                /*
                 * Explicitly insert one space after
                 * every extracted PDF word.
                 */
                builder.Append(' ');
            }

            /*
             * Separate pages so that text from the
             * end of one page does not run into the
             * beginning of the next.
             */
            builder.AppendLine();
            builder.AppendLine();
        }

        var result =
            NormalizeExtractedText(
                builder.ToString());

        return Task.FromResult(
            result);
    }

    /*
     * ============================================================
     * CLEAN INDIVIDUAL PDF WORD
     * ============================================================
     */
    private static string
        CleanWord(
            string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return string.Empty;
        }

        var cleaned =
            value.Trim();

        /*
         * Remove soft hyphen.
         */
        cleaned =
            cleaned.Replace(
                "\u00AD",
                "");

        /*
         * Remove Unicode replacement and
         * invalid/control artifacts occasionally
         * produced by PDF extraction.
         */
        cleaned =
            cleaned
                .Replace(
                    "\uFFFD",
                    "")
                .Replace(
                    "\uFFFE",
                    "")
                .Replace(
                    "\uFFFF",
                    "");

        /*
         * Normalize non-breaking spaces.
         */
        cleaned =
            cleaned.Replace(
                '\u00A0',
                ' ');

        return cleaned.Trim();
    }

    /*
     * ============================================================
     * NORMALIZE COMPLETE EXTRACTED TEXT
     * ============================================================
     */
    private static string
        NormalizeExtractedText(
            string text)
    {
        if (string.IsNullOrWhiteSpace(
                text))
        {
            return string.Empty;
        }

        var normalized =
            text
                .Replace(
                    "\r\n",
                    "\n")
                .Replace(
                    '\r',
                    '\n');

        /*
         * Collapse repeated spaces and tabs,
         * but preserve page/newline separation.
         */
        var lines =
            normalized
                .Split(
                    '\n',
                    StringSplitOptions.None);

        var output =
            new StringBuilder();

        foreach (var line in lines)
        {
            var cleanedLine =
                NormalizeLine(
                    line);

            if (string.IsNullOrWhiteSpace(
                    cleanedLine))
            {
                output.AppendLine();
                continue;
            }

            output.AppendLine(
                cleanedLine);
        }

        return output
            .ToString()
            .Trim();
    }

    /*
     * ============================================================
     * NORMALIZE ONE TEXT LINE
     * ============================================================
     */
    private static string
        NormalizeLine(
            string line)
    {
        if (string.IsNullOrWhiteSpace(
                line))
        {
            return string.Empty;
        }

        var cleaned =
            line.Trim();

        /*
         * Collapse multiple spaces/tabs into
         * exactly one normal space.
         */
        while (cleaned.Contains(
                   "  ",
                   StringComparison.Ordinal))
        {
            cleaned =
                cleaned.Replace(
                    "  ",
                    " ");
        }

        cleaned =
            cleaned.Replace(
                "\t",
                " ");

        /*
         * Remove unnecessary spaces before common
         * punctuation.
         *
         * Example:
         *
         * "locations ."
         *
         * becomes:
         *
         * "locations."
         */
        cleaned =
            cleaned
                .Replace(
                    " .",
                    ".")
                .Replace(
                    " ,",
                    ",")
                .Replace(
                    " ;",
                    ";")
                .Replace(
                    " :",
                    ":")
                .Replace(
                    " !",
                    "!")
                .Replace(
                    " ?",
                    "?");

        /*
         * Normalize spaces around parentheses.
         */
        cleaned =
            cleaned
                .Replace(
                    "( ",
                    "(")
                .Replace(
                    " )",
                    ")");

        /*
         * Make sure there is only one space
         * between words after all replacements.
         */
        while (cleaned.Contains(
                   "  ",
                   StringComparison.Ordinal))
        {
            cleaned =
                cleaned.Replace(
                    "  ",
                    " ");
        }

        return cleaned.Trim();
    }
}