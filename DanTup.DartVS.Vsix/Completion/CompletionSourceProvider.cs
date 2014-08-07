using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	[Export(typeof(ICompletionSourceProvider))]
	[ContentType(DartContentTypeDefinition.DartContentType)]
	[Name("Dart Completion")]
	class CompletionSourceProvider : ICompletionSourceProvider
	{
		[Import]
		ITextDocumentFactoryService textDocumentFactory = null;

		[Import]
		DartAnalysisService analysisService = null;

		public ICompletionSource TryCreateCompletionSource(ITextBuffer buffer)
		{
			return new CompletionSource(this, buffer, textDocumentFactory, analysisService);
		}
	}

	class CompletionSource : ICompletionSource
	{
		CompletionSourceProvider provider;
		ITextBuffer buffer;
		ITextDocumentFactoryService textDocumentFactory;
		DartAnalysisService analysisService;

		public CompletionSource(CompletionSourceProvider provider, ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, DartAnalysisService analysisService)
		{
			this.provider = provider;
			this.buffer = buffer;
			this.textDocumentFactory = textDocumentFactory;
			this.analysisService = analysisService;
		}

		public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
		{
			var triggerPoint = session.GetTriggerPoint(buffer.CurrentSnapshot);
			if (triggerPoint == null)
				return;

			var applicableTo = buffer.CurrentSnapshot.CreateTrackingSpan(new SnapshotSpan(triggerPoint.Value, 1), SpanTrackingMode.EdgeInclusive);

			var completions = new ObservableCollection<Completion>();
			completions.Add(new Completion("Something1"));
			completions.Add(new Completion("Something2"));
			completions.Add(new Completion("Something3"));

			completionSets.Add(new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Completion>()));
		}

		public void Dispose()
		{
			GC.SuppressFinalize(true);
		}
	}
}
