using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace DanTup.DartVS
{
	// TODO: This doesn't seem to work; but leaving it here for later when hopefully we can figure it out...

	//[Export(typeof(IPeekableItemSourceProvider))]
	[ContentType(DartConstants.ContentType)]
	[SupportsStandaloneFiles(true)]
	class PeekableItemProvider : IPeekableItemSourceProvider
	{
		[Import]
		IPeekResultFactory peekResultFactory = null;

		public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
		{
			return textBuffer.Properties.GetOrCreateSingletonProperty(() => new PeekableItemSource(textBuffer, peekResultFactory));
		}
	}

	class PeekableItemSource : IPeekableItemSource
	{
		readonly ITextBuffer buffer;
		readonly IPeekResultFactory peekResultFactory;

		public PeekableItemSource(ITextBuffer buffer, IPeekResultFactory peekResultFactory)
		{
			this.buffer = buffer;
			this.peekResultFactory = peekResultFactory;
		}

		public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
		{
			var triggerPoint = session.GetTriggerPoint(buffer.CurrentSnapshot);
			if (triggerPoint == null)
				return;

			peekableItems.Add(new MyPeekItem(peekResultFactory));
		}

		public void Dispose()
		{
		}
	}

	class MyPeekItem : IPeekableItem
	{
		internal readonly IPeekResultFactory peekResultFactory;

		public MyPeekItem(IPeekResultFactory peekResultFactory)
		{
			this.peekResultFactory = peekResultFactory;
		}

		public string DisplayName
		{
			get { return "Danny's Name"; }
		}

		public IPeekResultSource GetOrCreateResultSource(string relationshipName)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPeekRelationship> Relationships
		{
			get { yield return PredefinedPeekRelationships.Definitions; }
		}
	}

	class MyPeekResultSource : IPeekResultSource
	{
		MyPeekItem peekableItem;

		public MyPeekResultSource(MyPeekItem peekableItem)
		{
			this.peekableItem = peekableItem;
		}

		public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
		{
			if (relationshipName != PredefinedPeekRelationships.Definitions.Name)
				return;

			var file = @"M:\Coding\Applications\DanTup.DartVS\DanTup.DartVS.Vsix\LICENCE.txt";
			using (var displayInfo = new PeekResultDisplayInfo("Danny Label", file, "My Title", file))
			{
				var result = peekableItem.peekResultFactory.Create(displayInfo, file, new Span(10, 10), 10, false);
				resultCollection.Add(result);
				callback.ReportProgress(1);
			}
		}
	}
}
