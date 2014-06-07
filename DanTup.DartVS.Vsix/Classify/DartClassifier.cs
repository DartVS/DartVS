using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace DanTup.DartVS
{
	class DartClassifier : IClassifier
	{
		// https://www.dartlang.org/docs/dart-up-and-running/contents/ch02.html#keyword_table
		private static Regex _rxKeywords1 = new Regex(@"\b(abstract|continue|extends|implements|part|throw|as|default|factory|import|rethrow|true|assert|do|false|in|return|try|break|dynamic|final|is|set|typedef|case|else|finally|library|static|var|catch|enum|for|new|super|void|class|export|get|null|switch|while|const|external|if|operator|this|with)\b", RegexOptions.Compiled);
		// https://github.com/dart-lang/dartlang.org/blob/54d221410e04ddf7424400aec473f0a31d08b194/src/site/js/lang-dart.js
		private static Regex _rxKeywords2 = new Regex(@"\b(interface|native|part of|part|show|hide)\b", RegexOptions.Compiled);
		private static Regex _rxIdentifier = new Regex(@"\b(bool|double|Dynamic|int|num|Object|String|void)\b|@\w+\b", RegexOptions.Compiled);
		// TODO: Escaped quotes in strings
		private static Regex _rxString = new Regex(@"'.*?'|"".*?""", RegexOptions.Compiled);
		private static Regex _rxComment = new Regex("//.*|/\\*.*?\\*/", RegexOptions.Compiled);

		// Regexes for detecting multiline comments, and the in-line highlighting
		private static Regex _rxMultilineComment = new Regex("/\\*(.*?)\\*/", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex _rxMultilineCommentStart = new Regex("/\\*.*", RegexOptions.Compiled);
		private static Regex _rxMultilineCommentEnd = new Regex(".*\\*/", RegexOptions.Compiled);
		private IClassificationType commentType;

		// Different regexes that can be applied to lines, depending on whether they're inside multiline constructs or not
		private static Dictionary<Regex, IClassificationType> noRegexes = new Dictionary<Regex, IClassificationType>();
		private Dictionary<Regex, IClassificationType> standardRegexes;
		private Dictionary<Regex, IClassificationType> standardRegexesWithCommentStart;
		private Dictionary<Regex, IClassificationType> standardRegexesWithCommentEnd;

		public DartClassifier(IClassificationTypeRegistryService registry)
		{
			standardRegexes = new Dictionary<Regex, IClassificationType>
			{
				{_rxComment, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment)},
				{_rxString, registry.GetClassificationType(PredefinedClassificationTypeNames.String)},
				{_rxIdentifier, registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition)},
				{_rxKeywords1, registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword)},
				{_rxKeywords2, registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword)},
			};
			standardRegexesWithCommentStart = new Dictionary<Regex, IClassificationType>(standardRegexes)
			{
				{_rxMultilineCommentStart, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment)},
			};
			standardRegexesWithCommentEnd = new Dictionary<Regex, IClassificationType>(standardRegexes)
			{
				{_rxMultilineCommentEnd, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment)},
			};

			commentType = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
		}

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			IList<ClassificationSpan> allClassificationSpans = new List<ClassificationSpan>();
			string text = span.GetText(); // the span is always a single line

			// We'll need to vary the regex used when we're starting/end a multiline comment; so stash them here
			Dictionary<Regex, IClassificationType> validRegexesForCurrentLine;

			// The current span only ever covers a single line; so we need to detect whether this line is inside a multiline comment/string and
			// behave accordingly. Because this method is called unpredicatbly (editing a single line will just call it for that one line), we
			// need to examine the entire document. This might get slow for big documents, but we'll probably need to switch to a proper
			// parser at some point anyway (eg. for Intellisense).
			var multilineType = LookForMultilineConstructs(span);

			// TODO: Split the "Comment" part out of this, so we can support multi-line strings
			// TODO: Try to ignore multiline start/end constructs within strings; only real ones

			switch (multilineType)
			{
				// The entire line is within the multiline construct, so flag the whole line as this type and skip any further regex
				case MultilineType.WithinComment:
					var comment = new SnapshotSpan(span.Snapshot, span.Start.Position, span.Length);
					allClassificationSpans.Add(new ClassificationSpan(comment, commentType));
					validRegexesForCurrentLine = noRegexes;
					break;

				// The line starts a multiline construct, so add in the regex for flagging just the start of the construct
				case MultilineType.StartsComment:
					validRegexesForCurrentLine = standardRegexesWithCommentStart;
					break;

				// The line ends a multiline construct, so add in the regex for flagging just the end of the construct
				case MultilineType.EndsComment:
					validRegexesForCurrentLine = standardRegexesWithCommentEnd;
					break;

				// This line is not part of a multiline construct, so use the standard regex
				default:
					validRegexesForCurrentLine = standardRegexes;
					break;
			}

			foreach (Regex regex in validRegexesForCurrentLine.Keys)
			{
				foreach (Match match in regex.Matches(text))
				{
					var str = new SnapshotSpan(span.Snapshot, span.Start.Position + match.Index, match.Length);

					// If there's an intersecting span; we need to keep the *earliest* one
					//if (list.Any(s => s.Span.IntersectsWith(str)))
					//	continue;

					// Keep all spans, then we'll remove intersecting ones later; otherwise the first match will win, even if it
					// it's not the correct one.
					// eg.
					//     var name = "Danny // embedded comment";
					//
					// If the comment regex runs first, we'd fail to classify the string.
					// We can't simply order the regex differently, because we might have the opposite:
					// eg.
					//     var name; // This is a "name"

					allClassificationSpans.Add(new ClassificationSpan(str, validRegexesForCurrentLine[regex]));
				}
			}

			// Now we need to remove overlapping spans, but we *must* do them in order, since the real code is parsed left-to-right
			// eg.
			//     var name = "string // not a comment with 'not a string'"
			// 
			// In this case, both the comment and the single-quoted stringneed to be discarded.

			allClassificationSpans = allClassificationSpans.OrderBy(s => s.Span.Start.Position).ToList();
			var requiresRescan = true;
			while (requiresRescan)
			{
				requiresRescan = false; // Assume we won't need to rescan
				foreach (var classificationSpan in allClassificationSpans)
				{
					var intersectingSpans = allClassificationSpans
						.SkipWhile(s => s != classificationSpan) // Skip anything before this span in the list, since we're processing in order
						.Skip(1) // Skip self
						.Where(s => s.Span.IntersectsWith(classificationSpan.Span)).ToArray();

					if (intersectingSpans.Any())
					{
						foreach (var intersectingSpan in intersectingSpans)
							allClassificationSpans.Remove(intersectingSpan);
						requiresRescan = true;
						break; // We need to break out and start over, since we've modified the list
					}
				}

			}

			return allClassificationSpans;
		}

		/// <summary>
		/// Checks whether the current line is part of a multiline construct (comment, string) by expaining the entire
		/// document snapshot. This might turn out to be slow (especially on large documents) and unreliable (if you can embed
		/// a multiline start/end in an non-real form; eg. in a string).
		/// </summary>
		private MultilineType LookForMultilineConstructs(SnapshotSpan span)
		{
			var multiLineComments = _rxMultilineComment.Matches(span.Snapshot.GetText());
			foreach (Match match in multiLineComments)
			{
				bool commentStartedBeforeLine = match.Index < span.Start.Position;
				bool commentStartedOnLine = match.Index >= span.Start.Position && match.Index <= span.End.Position;
				bool commentEndedAfterLine = match.Index + match.Length > span.End.Position;
				bool commentEndOnLine = match.Index + match.Length >= span.Start.Position && match.Index + match.Length <= span.End.Position;

				if (commentStartedBeforeLine && commentEndedAfterLine)
					return MultilineType.WithinComment;
				else if (commentStartedBeforeLine && commentEndOnLine)
					return MultilineType.EndsComment;
				else if (commentStartedOnLine && !commentEndOnLine)
					return MultilineType.StartsComment;
				// Note: If it starts and ends on this line, we'll just ignore it; since it'll handled as part of the normal comment regex
			}

			return MultilineType.None;
		}

		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
		{
			add { }
			remove { }
		}

		private enum MultilineType
		{
			None,
			StartsComment,
			WithinComment,
			EndsComment
			// TODO: Strings
		}
	}
}