using SafetyCopilot.API.Services.Interfaces;
using System.Text.RegularExpressions;

namespace SafetyCopilot.API.Services;

public class RequirementParserService
    : IRequirementParserService
{
    /*
     * ============================================================
     * REQUIREMENT PATTERN
     * ============================================================
     *
     * Matches requirements such as:
     *
     * REQ-SYS-001:
     * The system shall provide a unified material identifier...
     * [Non-Hazard]
     *
     * REQ-SAF-041:
     * The system shall continuously monitor fissile material...
     * [Hazard]
     *
     * The pattern deliberately works across line breaks because
     * PDF extraction may wrap a requirement across several lines.
     *
     * Groups:
     *
     * id:
     *     REQ-SYS-001
     *
     * text:
     *     The system shall ...
     *
     * label:
     *     Hazard
     *     or
     *     Non-Hazard
     *
     * IMPORTANT:
     * The label is recognized so that we know where the
     * requirement ends, but it is NOT included in the
     * RequirementText returned to the application.
     */
    private static readonly Regex RequirementRegex =
        new(
            @"(?<id>REQ-[A-Z]+-\d{3})\s*:\s*" +
            @"(?<text>.*?)" +
            @"\s*\[(?<label>Hazard|Non-Hazard)\]",
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline |
            RegexOptions.Compiled);

    /*
     * ============================================================
     * REQUIREMENT ID PATTERN
     * ============================================================
     *
     * Used for diagnostic/fallback extraction.
     *
     * Examples:
     *
     * REQ-SYS-001
     * REQ-OPR-021
     * REQ-SAF-041
     * REQ-WST-061
     * REQ-CYB-081
     * REQ-HMI-101
     * REQ-DAT-121
     * REQ-MNT-141
     * REQ-EMR-161
     * REQ-VV-181
     */
    private static readonly Regex RequirementIdRegex =
        new(
            @"REQ-[A-Z]+-\d{3}",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

    /*
     * ============================================================
     * PARSE REQUIREMENTS
     * ============================================================
     */
    public IReadOnlyList<ParsedRequirement>
        ParseRequirements(
            string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<ParsedRequirement>();
        }

        /*
         * Normalize the text before attempting
         * requirement extraction.
         */
        var normalizedText =
            NormalizePdfText(text);

        /*
         * First use the normal labeled requirement
         * extraction strategy.
         */
        var requirements =
            ParseLabeledRequirements(
                normalizedText);

        /*
         * If labeled extraction did not find anything,
         * use a fallback strategy based only on REQ IDs.
         *
         * This makes the parser more reusable if we later
         * upload a requirements document that does not
         * contain [Hazard] / [Non-Hazard].
         */
        if (requirements.Count == 0)
        {
            requirements =
                ParseRequirementsById(
                    normalizedText);
        }

        /*
         * Ensure deterministic ordering.
         */
        return requirements
            .Select(
                (
                    requirement,
                    index
                ) =>
                    requirement with
                    {
                        SequenceNumber =
                            index + 1
                    })
            .ToList();
    }

    /*
     * ============================================================
     * LABELED REQUIREMENT EXTRACTION
     * ============================================================
     *
     * This is the preferred extraction strategy for the
     * current SafetyCopilot research requirements PDF.
     */
    private static List<ParsedRequirement>
        ParseLabeledRequirements(
            string text)
    {
        var requirements =
            new List<ParsedRequirement>();

        var seenRequirementNumbers =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var matches =
            RequirementRegex
                .Matches(text);

        foreach (Match match in matches)
        {
            if (!match.Success)
            {
                continue;
            }

            var requirementNumber =
                NormalizeRequirementNumber(
                    match
                        .Groups["id"]
                        .Value);

            var requirementText =
                CleanRequirementText(
                    match
                        .Groups["text"]
                        .Value);

            if (string.IsNullOrWhiteSpace(
                    requirementNumber))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    requirementText))
            {
                continue;
            }

            /*
             * Prevent duplicate REQ IDs.
             */
            if (!seenRequirementNumbers.Add(
                    requirementNumber))
            {
                continue;
            }

            /*
             * IMPORTANT:
             *
             * We intentionally ignore:
             *
             * match.Groups["label"]
             *
             * Therefore:
             *
             * [Hazard]
             * [Non-Hazard]
             *
             * are NOT included in RequirementText.
             *
             * This preserves experimental blindness
             * during the human classification phase.
             */
            requirements.Add(
                new ParsedRequirement(
                    requirementNumber,
                    requirementText,
                    requirements.Count + 1));
        }

        return requirements;
    }

    /*
     * ============================================================
     * FALLBACK EXTRACTION BY REQUIREMENT ID
     * ============================================================
     *
     * This handles future PDFs that may contain:
     *
     * REQ-SYS-001: The system shall ...
     * REQ-SYS-002: The system shall ...
     *
     * but do not contain:
     *
     * [Hazard]
     * [Non-Hazard]
     *
     * A requirement starts at one REQ ID and ends immediately
     * before the next REQ ID.
     */
    private static List<ParsedRequirement>
        ParseRequirementsById(
            string text)
    {
        var requirements =
            new List<ParsedRequirement>();

        var matches =
            RequirementIdRegex
                .Matches(text);

        if (matches.Count == 0)
        {
            return requirements;
        }

        var seenRequirementNumbers =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        for (
            var index = 0;
            index < matches.Count;
            index++)
        {
            var currentMatch =
                matches[index];

            var requirementNumber =
                NormalizeRequirementNumber(
                    currentMatch.Value);

            if (string.IsNullOrWhiteSpace(
                    requirementNumber))
            {
                continue;
            }

            /*
             * Determine where requirement text starts.
             */
            var textStart =
                currentMatch.Index +
                currentMatch.Length;

            /*
             * Determine where it ends.
             *
             * Normally this is immediately before
             * the next REQ identifier.
             */
            var textEnd =
                index < matches.Count - 1
                    ? matches[index + 1].Index
                    : text.Length;

            if (textEnd <= textStart)
            {
                continue;
            }

            var length =
                textEnd -
                textStart;

            var requirementText =
                text.Substring(
                    textStart,
                    length);

            /*
             * Remove punctuation between the
             * REQ ID and actual sentence.
             *
             * Example:
             *
             * REQ-SYS-001:
             *
             * becomes:
             *
             * The system shall...
             */
            requirementText =
                requirementText
                    .TrimStart(
                        ' ',
                        '\t',
                        '\r',
                        '\n',
                        ':',
                        '-',
                        '.');

            /*
             * If this fallback encounters a
             * ground-truth label, remove it.
             */
            requirementText =
                RemoveGroundTruthLabel(
                    requirementText);

            requirementText =
                CleanRequirementText(
                    requirementText);

            /*
             * Remove likely section/header material
             * that can occur between the end of a
             * requirement and the next requirement.
             */
            requirementText =
                RemoveTrailingSectionText(
                    requirementText);

            if (string.IsNullOrWhiteSpace(
                    requirementText))
            {
                continue;
            }

            if (!seenRequirementNumbers.Add(
                    requirementNumber))
            {
                continue;
            }

            requirements.Add(
                new ParsedRequirement(
                    requirementNumber,
                    requirementText,
                    requirements.Count + 1));
        }

        return requirements;
    }

    /*
     * ============================================================
     * NORMALIZE PDF TEXT
     * ============================================================
     */
    private static string
        NormalizePdfText(
            string text)
    {
        var normalized =
            text
                .Replace(
                    "\r\n",
                    "\n")
                .Replace(
                    '\r',
                    '\n')
                .Replace(
                    '\u00A0',
                    ' ');

        /*
         * Remove soft hyphens.
         */
        normalized =
            normalized.Replace(
                "\u00AD",
                "");

        /*
         * Remove common Unicode artifacts that can
         * appear during PDF text extraction.
         */
        normalized =
            normalized
                .Replace(
                    "\uFFFE",
                    "")
                .Replace(
                    "\uFFFF",
                    "");

        /*
         * Some PDF extraction libraries produce
         * the replacement/control character below.
         *
         * Remove it while leaving ordinary hyphens
         * intact because REQ-SYS-001 depends on them.
         */
        normalized =
            normalized.Replace(
                "\uFFFD",
                "");

        return normalized;
    }

    /*
     * ============================================================
     * CLEAN REQUIREMENT TEXT
     * ============================================================
     */
    private static string
        CleanRequirementText(
            string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var cleaned =
            text.Trim();

        /*
         * Replace all PDF whitespace:
         *
         * spaces
         * tabs
         * newlines
         * repeated spaces
         *
         * with one normal space.
         *
         * Since PdfTextService now uses GetWords(),
         * the words should already be separated.
         */
        cleaned =
            Regex.Replace(
                cleaned,
                @"\s+",
                " ");

        /*
         * Remove spaces before punctuation.
         *
         * Example:
         *
         * "storage locations ."
         *
         * becomes:
         *
         * "storage locations."
         */
        cleaned =
            Regex.Replace(
                cleaned,
                @"\s+([.,;:!?])",
                "$1");

        /*
         * Normalize opening parentheses.
         *
         * "( e.g."
         *
         * becomes:
         *
         * "(e.g."
         */
        cleaned =
            Regex.Replace(
                cleaned,
                @"\(\s+",
                "(");

        /*
         * Normalize closing parentheses.
         *
         * "waste )"
         *
         * becomes:
         *
         * "waste)"
         */
        cleaned =
            Regex.Replace(
                cleaned,
                @"\s+\)",
                ")");

        /*
         * Normalize spaces around apostrophes.
         */
        cleaned =
            Regex.Replace(
                cleaned,
                @"\s+'\s+",
                "'");

        /*
         * Remove any ground-truth label that may
         * have survived extraction.
         */
        cleaned =
            RemoveGroundTruthLabel(
                cleaned);

        return cleaned.Trim();
    }

    /*
     * ============================================================
     * REMOVE GROUND TRUTH LABEL
     * ============================================================
     *
     * The PDF contains:
     *
     * [Hazard]
     * [Non-Hazard]
     *
     * These MUST NOT be shown to the human participant.
     */
    private static string
        RemoveGroundTruthLabel(
            string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return Regex.Replace(
                text,
                @"\s*\[(?:Hazard|Non-Hazard)\]\s*",
                " ",
                RegexOptions.IgnoreCase)
            .Trim();
    }

    /*
     * ============================================================
     * REMOVE TRAILING SECTION TEXT
     * ============================================================
     *
     * Mainly used by fallback extraction.
     *
     * A PDF may produce:
     *
     * REQ-SYS-020: ...
     *
     * 3.2 Operational Workflow Requirements
     *
     * REQ-OPR-021: ...
     *
     * We do not want the section heading appended
     * to REQ-SYS-020.
     */
    private static string
        RemoveTrailingSectionText(
            string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        /*
         * Most requirements in this document are
         * complete sentences.
         *
         * If we find a period followed by something
         * that looks like a numbered section heading,
         * keep only the requirement sentence.
         */
        var sectionMatch =
            Regex.Match(
                text,
                @"^(?<requirement>.*?\.)\s+" +
                @"\d+\.\d+\s+" +
                @"[A-Z]",
                RegexOptions.Singleline);

        if (sectionMatch.Success)
        {
            return sectionMatch
                .Groups["requirement"]
                .Value
                .Trim();
        }

        return text.Trim();
    }

    /*
     * ============================================================
     * NORMALIZE REQUIREMENT NUMBER
     * ============================================================
     */
    private static string
        NormalizeRequirementNumber(
            string requirementNumber)
    {
        if (string.IsNullOrWhiteSpace(
                requirementNumber))
        {
            return string.Empty;
        }

        var normalized =
            requirementNumber
                .Trim()
                .ToUpperInvariant();

        /*
         * Remove accidental whitespace.
         *
         * Example:
         *
         * REQ-SYS- 001
         *
         * becomes:
         *
         * REQ-SYS-001
         */
        normalized =
            Regex.Replace(
                normalized,
                @"\s+",
                "");

        return normalized;
    }
}