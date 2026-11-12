using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    public static class UsfmTokenFilter
    {
        // Set of USFM markers whose contents and tokens must NEVER be altered
        public static readonly HashSet<string> ExcludedMarkers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "c",      // Chapter number
            "v",      // Verse number
            "r",      // Parallel reference
            "x",      // Cross-reference start
            "x*",     // Cross-reference end
            "xo",     // Cross-reference origin
            "xt",     // Cross-reference target
            "f",      // Footnote start
            "f*",     // Footnote end
            "fr",     // Footnote reference
            "ft",     // Footnote text
            "fe",     // Endnote
            "fe*",    // Endnote end
            "fig",    // Figure / illustration start
            "fig*",   // Figure end
            "id",     // Book ID header
            "h",      // Running header
            "toc1",   // Long table of contents
            "toc2",   // Short table of contents
            "toc3",   // Book abbreviation
            "mt1",    // Major title 1
            "mt2",    // Major title 2
            "mt3",    // Major title 3
            "rem"     // Remarks / Translator Comments
        };

        public static bool ShouldExcludeMarker(string marker)
        {
            if (string.IsNullOrWhiteSpace(marker))
                return false;

            string clean = marker.TrimStart('\\').Trim();
            return ExcludedMarkers.Contains(clean);
        }

        public static bool IsPublishableScriptureParagraph(string marker)
        {
            if (string.IsNullOrWhiteSpace(marker))
                return true;

            string clean = marker.TrimStart('\\').Trim();
            // Typical scripture body paragraph and poetry markers
            return clean == "p" || clean == "m" || clean == "po" ||
                   clean == "pr" || clean == "cls" || clean == "pmo" ||
                   clean == "pm" || clean == "pmc" || clean == "pmr" ||
                   clean == "pi" || clean == "mi" || clean == "nb" ||
                   clean == "pc" || clean == "b" ||
                   clean.StartsWith("q") || clean.StartsWith("s");
        }
    }
}
