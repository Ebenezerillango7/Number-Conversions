using System;
using System.Collections.Generic;
using System.Linq;
using NumberConversions.Core;
using Paratext.PluginInterfaces;

namespace NumberConversions.Plugin
{
    public class ParatextScriptureService
    {
        private readonly IPluginHost _host;
        private readonly TamilNumberConversionEngine _engine;

        public ParatextScriptureService(IPluginHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _engine = new TamilNumberConversionEngine();
        }

        public List<ConversionProposal> ScanBook(IProject project, int bookNum, Action<int, int>? progressCallback = null)
        {
            var proposals = new List<ConversionProposal>();
            if (project == null)
                return proposals;

            int lastChapter = project.Versification.GetLastChapter(bookNum);

            for (int chapter = 1; chapter <= lastChapter; chapter++)
            {
                progressCallback?.Invoke(chapter, lastChapter);

                IEnumerable<IUSFMToken> tokens;
                try
                {
                    tokens = project.GetUSFMTokens(bookNum, chapter);
                }
                catch
                {
                    continue;
                }

                if (tokens == null)
                    continue;

                foreach (var token in tokens)
                {
                    // Strict structural filter: ignore non-scripture, footnotes, cross-references, figures, metadata
                    if (!token.IsScripture || token.IsFootnoteOrCrossReference || token.IsFigure || token.IsMetadata)
                        continue;

                    // Exclude specific structural markers (\c, \v, \r, \x, \f, \fig, etc.)
                    if (token is IUSFMMarkerToken markerToken)
                    {
                        if (UsfmTokenFilter.ShouldExcludeMarker(markerToken.Marker))
                            continue;
                    }

                    // Process text tokens
                    if (token is IUSFMTextToken textToken)
                    {
                        string text = textToken.Text;
                        if (string.IsNullOrWhiteSpace(text))
                            continue;

                        string verseRefStr = token.VerseRef != null 
                            ? project.GetFormattedReference(token.VerseRef, BookNameType.ShortName) 
                            : $"BK{bookNum} {chapter}:{token.VerseOffset}";

                        int verseNum = token.VerseRef != null ? token.VerseRef.VerseNum : 0;

                        var tokenProposals = _engine.ScanText(text, verseRefStr, bookNum, chapter, verseNum);
                        proposals.AddRange(tokenProposals);
                    }
                }
            }

            return proposals;
        }

        public bool ApplyApprovedConversions(
            IProject project,
            IPluginObject pluginObj,
            int bookNum,
            List<ConversionProposal> approvedProposals,
            out string statusMessage)
        {
            statusMessage = string.Empty;

            if (project == null)
            {
                statusMessage = "No project selected.";
                return false;
            }

            if (approvedProposals == null || approvedProposals.Count == 0)
            {
                statusMessage = "No approved conversions to apply.";
                return true;
            }

            // 1. Safety check: Verify edit permissions
            if (!project.CanEdit(pluginObj, bookNum, 1))
            {
                statusMessage = $"User does not have write permissions for book {bookNum}.";
                return false;
            }

            // 2. Paratext History Point for instant 1-click rollback
            try
            {
                project.MakeHistoryPoint(
                    $"Tamil Number Conversions: {approvedProposals.Count} replacements applied (0-199 words, 200+ digits)",
                    force: true);
            }
            catch (Exception ex)
            {
                _host.Log(pluginObj, "Failed to create history point: {0}", new object[] { ex.Message });
            }

            // Group proposals by chapter
            var byChapter = approvedProposals.GroupBy(p => p.ChapterNum);

            foreach (var chapterGroup in byChapter)
            {
                int chapterNum = chapterGroup.Key;

                // Request write lock for chapter
                IWriteLock? writeLock = null;
                try
                {
                    writeLock = project.RequestWriteLock(pluginObj, onRelease => { }, bookNum, chapterNum);
                    if (writeLock == null)
                        continue;

                    // Read current USFM tokens
                    var tokens = project.GetUSFMTokens(bookNum, chapterNum).ToList();
                    bool modified = false;

                    // Replace matching text tokens
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (tokens[i] is IUSFMTextToken textToken)
                        {
                            string original = textToken.Text;
                            var chapterProposals = chapterGroup
                                .Where(p => original.Contains(p.MatchedToken))
                                .ToList();

                            if (chapterProposals.Count > 0)
                            {
                                string updated = _engine.ApplyProposals(original, chapterProposals);
                                if (updated != original)
                                {
                                    // Paratext uses ConvertToUSFMTokens to generate replacement tokens
                                    var newTokens = project.ConvertToUSFMTokens(updated, bookNum, chapterNum, 1).ToList();
                                    tokens.RemoveAt(i);
                                    tokens.InsertRange(i, newTokens);
                                    i += newTokens.Count - 1;
                                    modified = true;
                                }
                            }
                        }
                    }

                    if (modified)
                    {
                        project.PutUSFMTokens(writeLock, tokens, bookNum);
                    }
                }
                catch (Exception ex)
                {
                    _host.Log(pluginObj, "Error writing chapter {0}: {1}", new object[] { chapterNum, ex.Message });
                }
                finally
                {
                    writeLock?.Dispose();
                }
            }

            statusMessage = $"Successfully applied {approvedProposals.Count} approved number conversions.";
            return true;
        }
    }
}
