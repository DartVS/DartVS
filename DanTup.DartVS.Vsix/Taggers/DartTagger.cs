using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ITaggerProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[TagType(typeof(DartTokenTag))]
	internal sealed class DartTokenTagProvider : ITaggerProvider
	{
		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			return new DartTokenTagger(buffer) as ITagger<T>;
		}
	}

	public enum DartTokenType
	{
		Comment,
		String,
		Identifier,
		Keyword,
		Number,
		Operator
	}

	public class DartTokenTag : ITag
	{
		public DartTokenType Type { get; private set; }

		public DartTokenTag(DartTokenType type)
		{
			this.Type = type;
		}
	}

	class DartTokenTagger : ITagger<DartTokenTag>
	{
		#region Regular Expressions

		// https://www.dartlang.org/docs/dart-up-and-running/contents/ch02.html#keyword_table
		private static Regex rxKeywords1 = new Regex(@"\b(abstract|continue|extends|implements|part|throw|as|default|factory|import|rethrow|true|assert|do|false|in|return|try|break|dynamic|final|is|set|typedef|case|else|finally|library|static|var|catch|enum|for|new|super|void|class|export|get|null|switch|while|const|external|if|operator|this|with)\b", RegexOptions.Compiled);
		// https://github.com/dart-lang/dartlang.org/blob/54d221410e04ddf7424400aec473f0a31d08b194/src/site/js/lang-dart.js
		private static Regex rxKeywords2 = new Regex(@"\b(interface|native|part of|part|show|hide)\b", RegexOptions.Compiled);

		private static Regex rxIdentifier = new Regex(@"\b(bool|double|Dynamic|int|num|Object|String|void)\b|@\w+\b", RegexOptions.Compiled);
		private static Regex rxOperator = new Regex(@"[~!%\^&\*\+=\|\?:<>/-]+", RegexOptions.Compiled);
		private static Regex rxNumber = new Regex(@"\b\d+\b", RegexOptions.Compiled);
		private static Regex rxEverything = new Regex(".*", RegexOptions.Compiled);

		// TODO: Escaped quotes in strings
		private static Regex rxString = new Regex(@"'.*?'|"".*?""", RegexOptions.Compiled);
		private static Regex rxComment = new Regex("//.*|/\\*.*?\\*/", RegexOptions.Compiled);

		// Regexes for detecting multiline comments, and the in-line highlighting
		private static Regex rxMultilineComment = new Regex("/\\*(.*?)\\*/", RegexOptions.Compiled | RegexOptions.Singleline);
		private static Regex rxMultilineCommentStart = new Regex("/\\*.*", RegexOptions.Compiled);
		private static Regex rxMultilineCommentEnd = new Regex(".*\\*/", RegexOptions.Compiled);

		#endregion

		// Different regexes that can be applied to lines, depending on whether they're inside multiline constructs or not
		private readonly Dictionary<Regex, DartTokenType> everythingIsCommentRegexes;
		private readonly Dictionary<Regex, DartTokenType> standardRegexes;
		private readonly Dictionary<Regex, DartTokenType> standardRegexesWithCommentStart;
		private readonly Dictionary<Regex, DartTokenType> standardRegexesWithCommentEnd;
		private readonly Dictionary<Regex, DartTokenType> standardRegexesWithCommentStartAndEnd;

		ITextBuffer buffer;

		internal DartTokenTagger(ITextBuffer buffer)
		{
			this.buffer = buffer;

			// Used to we can handle lines covered entirely by multiline comment/string the same as other lines
			everythingIsCommentRegexes = new Dictionary<Regex, DartTokenType>
			{
				{ rxEverything, DartTokenType.Comment },
			};
			// Standard regexs to run on lines
			standardRegexes = new Dictionary<Regex, DartTokenType>
			{
				{ rxComment, DartTokenType.Comment },
				{ rxString, DartTokenType.String },
				{ rxIdentifier, DartTokenType.Identifier },
				{ rxKeywords1, DartTokenType.Keyword },
				{ rxKeywords2, DartTokenType.Keyword },
				{ rxNumber, DartTokenType.Number },
				{ rxOperator, DartTokenType.Operator },				
			};
			// Regexes to run on lines that have been detected to start a multiline comment
			standardRegexesWithCommentStart = new Dictionary<Regex, DartTokenType>(standardRegexes)
			{
				{ rxMultilineCommentStart, DartTokenType.Comment },
			};
			// Regexes to run on lines that have been detected to end a multiline comment
			standardRegexesWithCommentEnd = new Dictionary<Regex, DartTokenType>(standardRegexes)
			{
				{ rxMultilineCommentEnd, DartTokenType.Comment },
			};
			// Regexes to run on lines that have been detected to end a multiline comment
			standardRegexesWithCommentStartAndEnd = new Dictionary<Regex, DartTokenType>(standardRegexes)
			{
				{ rxMultilineCommentStart, DartTokenType.Comment },
				{ rxMultilineCommentEnd, DartTokenType.Comment },
			};
			// TODO: Handle lines with multiple mutiline comments!
		}

		public IEnumerable<ITagSpan<DartTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			var multilineInfo = GetMultilineConstructs(buffer.CurrentSnapshot);

			foreach (var span in spans)
			{
				List<Tuple<SnapshotSpan, DartTokenType>> classifiedSpans = new List<Tuple<SnapshotSpan, DartTokenType>>();

				// We'll need to vary the regex used when we're starting/end a multiline comment; so stash them here
				Dictionary<Regex, DartTokenType> validRegexesForCurrentLine;

				// The current span only ever covers a single line; so we need to detect whether this line is inside a multiline comment/string and
				// behave accordingly. Because this method is called unpredicatbly (editing a single line will just call it for that one line), we
				// need to examine the entire document. This might get slow for big documents, but we'll probably need to switch to a proper
				// parser at some point anyway (eg. for Intellisense).
				MultilineType multilineType;
				if (multilineInfo.TryGetValue(span.Start.GetContainingLine().LineNumber, out multilineType))
				{
					// TODO: Split the "Comment" part out of this, so we can support multi-line strings
					// TODO: Try to ignore multiline start/end constructs within strings; only real ones

					switch (multilineType)
					{
						// The entire line is within the multiline construct, so flag the whole line as this type and skip any further regex
						case MultilineType.WithinComment:
							validRegexesForCurrentLine = everythingIsCommentRegexes;
							break;

						// The line starts a multiline construct, so add in the regex for flagging just the start of the construct
						case MultilineType.StartsComment:
							validRegexesForCurrentLine = standardRegexesWithCommentStart;
							break;

						// The line ends a multiline construct, so add in the regex for flagging just the end of the construct
						case MultilineType.EndsComment:
							validRegexesForCurrentLine = standardRegexesWithCommentEnd;
							break;

						// The line both starts and ends multiline comments; so we'll need to evaluate both regexes
						case MultilineType.ContainsComment:
							validRegexesForCurrentLine = standardRegexesWithCommentStartAndEnd;
							break;

						default:
							if (Debugger.IsAttached)
								Debugger.Break();
							validRegexesForCurrentLine = new Dictionary<Regex, DartTokenType>();
							break;
					}
				}
				else
					validRegexesForCurrentLine = standardRegexes;

				// Now apply these regexes to the current span to classify the text
				foreach (Regex regex in validRegexesForCurrentLine.Keys)
				{
					foreach (Match match in regex.Matches(span.GetText()))
					{
						var str = new SnapshotSpan(span.Snapshot, span.Start.Position + match.Index, match.Length);

						// Keep all spans, then we'll remove intersecting ones later; otherwise the first match will win, even if it
						// it's not the correct one.
						// eg.
						//     var name = "Danny // embedded comment";
						//
						// If the comment regex runs first, we'd fail to classify the string.
						// We can't simply order the regex differently, because we might have the opposite:
						// eg.
						//     var name; // This is a "name"

						classifiedSpans.Add(Tuple.Create(str, validRegexesForCurrentLine[regex]));
					}
				}

				// Now we need to remove overlapping spans, but we *must* do them in order, since the real code is parsed left-to-right
				// eg.
				//     var name = "string // not a comment with 'not a string'"
				// 
				// In this case, both the comment and the single-quoted stringneed to be discarded.

				classifiedSpans = classifiedSpans
					.OrderBy(s => s.Item1.Start.Position)
					.ThenByDescending(s => s.Item2 == DartTokenType.Comment) // Prioritise comments so they don't clash with operators (note: Descending, because bools sort False, True!)
					.ToList();
				var requiresRescan = true;
				while (requiresRescan)
				{
					requiresRescan = false; // Assume we won't need to rescan
					foreach (var classificationSpan in classifiedSpans)
					{
						var intersectingSpans = classifiedSpans
							.SkipWhile(s => s != classificationSpan) // Skip anything before this span in the list, since we're processing in order
							.Skip(1) // Skip self
							.Where(s => s.Item1.IntersectsWith(classificationSpan.Item1)).ToArray();

						if (intersectingSpans.Any())
						{
							foreach (var intersectingSpan in intersectingSpans)
								classifiedSpans.Remove(intersectingSpan);
							requiresRescan = true;
							break; // We need to break out and start over, since we've modified the list
						}
					}

				}

				// Yield this lineline (spans) matches so far
				foreach (var tokenSpan in classifiedSpans)
					yield return new TagSpan<DartTokenTag>(tokenSpan.Item1, new DartTokenTag(tokenSpan.Item2));
			}
		}

		/// <summary>
		/// Returns data about construnts that span multiple lines so that they can be processed individually.
		/// </summary>
		private Dictionary<int, MultilineType> GetMultilineConstructs(ITextSnapshot snapshot)
		{
			var results = new Dictionary<int, MultilineType>();
			var multiLineComments = rxMultilineComment.Matches(snapshot.GetText());
			foreach (Match match in multiLineComments)
			{
				var commentStartsOnLine = snapshot.GetLineNumberFromPosition(match.Index);
				var commentEndsOnLine = snapshot.GetLineNumberFromPosition(match.Index + match.Length);

				if (commentStartsOnLine == commentEndsOnLine)
					results.Add(commentStartsOnLine, MultilineType.ContainsComment);
				else
				{
					results.Add(commentStartsOnLine, MultilineType.StartsComment);
					results.Add(commentEndsOnLine, MultilineType.EndsComment);
					for (int line = commentStartsOnLine + 1; line < commentEndsOnLine; line++)
						results.Add(line, MultilineType.WithinComment);
				}
			}
			return results;
		}

		private enum MultilineType
		{
			None,
			StartsComment,
			WithinComment,
			EndsComment,
			ContainsComment
			// TODO: Strings
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged
		{
			add { }
			remove { }
		}
	}
}
