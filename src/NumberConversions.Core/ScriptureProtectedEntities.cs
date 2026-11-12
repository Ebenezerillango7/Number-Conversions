using System;
using System.Collections.Generic;

namespace NumberConversions.Core
{
    /// <summary>
    /// Maintains catalog of biblical book names and protected terms with leading digits
    /// to prevent unintentional replacement of book titles in scripture citations.
    /// </summary>
    public static class ScriptureProtectedEntities
    {
        public static readonly HashSet<string> NumberedBookTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Full names
            "1 சாமுவேல்", "2 சாமுவேல்",
            "1 இராஜாக்கள்", "2 இராஜாக்கள்",
            "1 நாளாகமம்", "2 நாளாகமம்",
            "1 கொரிந்தியர்", "2 கொரிந்தியர்",
            "1 தெசலோனிக்கேயர்", "2 தெசலோனிக்கேயர்",
            "1 தீமோத்தேயு", "2 தீமோத்தேயு",
            "1 பேதுரு", "2 பேதுரு",
            "1 யோவான்", "2 யோவான்", "3 யோவான்",

            // Abbreviations
            "1சாமு", "2சாமு",
            "1இரா", "2இரா",
            "1நாளா", "2நாளா",
            "1கொரி", "2கொரி",
            "1தெச", "2தெச",
            "1தீமோ", "2தீமோ",
            "1பேது", "2பேது",
            "1யோவா", "2யோவா", "3யோவா"
        };

        public static bool IsProtectedTitlePrefix(string text, int index)
        {
            if (string.IsNullOrWhiteSpace(text) || index < 0 || index >= text.Length)
                return false;

            string substring = text.Substring(index);
            foreach (var title in NumberedBookTitles)
            {
                if (substring.StartsWith(title, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
