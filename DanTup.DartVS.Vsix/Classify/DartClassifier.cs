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
		private static Regex _rxString = new Regex(@"'([^']+)'", RegexOptions.Compiled);
		private static Regex _rxComment = new Regex("//.*", RegexOptions.Compiled);
		private Dictionary<Regex, IClassificationType> _map;

		public DartClassifier(IClassificationTypeRegistryService registry)
		{
			_map = new Dictionary<Regex, IClassificationType>
			{
				{_rxComment, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment)},
				{_rxString, registry.GetClassificationType(PredefinedClassificationTypeNames.String)},
				{_rxIdentifier, registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition)},
				{_rxKeywords1, registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword)},
				{_rxKeywords2, registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword)},
			};
		}

		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			IList<ClassificationSpan> list = new List<ClassificationSpan>();
			string text = span.GetText(); // the span is always a single line

			foreach (Regex regex in _map.Keys)
				foreach (Match match in regex.Matches(text))
				{
					var str = new SnapshotSpan(span.Snapshot, span.Start.Position + match.Index, match.Length);

					// Make sure we don't double classify
					if (list.Any(s => s.Span.IntersectsWith(str)))
						continue;

					list.Add(new ClassificationSpan(str, _map[regex]));
				}

			return list;
		}

		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
		{
			add { }
			remove { }
		}
	}
}