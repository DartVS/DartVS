// Code generated by UpdateJson.fsx. Do not hand-edit!

using DanTup.DartAnalysis.Json;

namespace DanTup.DartAnalysis
{
	public class ServerGetVersion : Request<Response<ServerGetVersionResponse>>
	{

	}

	public class ServerShutdown : Request<Response>
	{

	}

	public class ServerSetSubscriptions : Request<ServerSetSubscriptionsRequest, Response>
	{
		public ServerSetSubscriptions(ServerSetSubscriptionsRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisGetErrors : Request<AnalysisGetErrorsRequest, Response<AnalysisGetErrorsResponse>>
	{
		public AnalysisGetErrors(AnalysisGetErrorsRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisGetHover : Request<AnalysisGetHoverRequest, Response<AnalysisGetHoverResponse>>
	{
		public AnalysisGetHover(AnalysisGetHoverRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisReanalyze : Request<Response>
	{

	}

	public class AnalysisSetAnalysisRoots : Request<AnalysisSetAnalysisRootsRequest, Response>
	{
		public AnalysisSetAnalysisRoots(AnalysisSetAnalysisRootsRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisSetPriorityFiles : Request<AnalysisSetPriorityFilesRequest, Response>
	{
		public AnalysisSetPriorityFiles(AnalysisSetPriorityFilesRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisSetSubscriptions : Request<AnalysisSetSubscriptionsRequest, Response>
	{
		public AnalysisSetSubscriptions(AnalysisSetSubscriptionsRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisUpdateContent : Request<AnalysisUpdateContentRequest, Response>
	{
		public AnalysisUpdateContent(AnalysisUpdateContentRequest @params)
			: base(@params)
		{
		}
	}

	public class AnalysisUpdateOptions : Request<AnalysisUpdateOptionsRequest, Response>
	{
		public AnalysisUpdateOptions(AnalysisUpdateOptionsRequest @params)
			: base(@params)
		{
		}
	}

	public class CompletionGetSuggestions : Request<CompletionGetSuggestionsRequest, Response<CompletionGetSuggestionsResponse>>
	{
		public CompletionGetSuggestions(CompletionGetSuggestionsRequest @params)
			: base(@params)
		{
		}
	}

	public class SearchFindElementReferences : Request<SearchFindElementReferencesRequest, Response<SearchFindElementReferencesResponse>>
	{
		public SearchFindElementReferences(SearchFindElementReferencesRequest @params)
			: base(@params)
		{
		}
	}

	public class SearchFindMemberDeclarations : Request<SearchFindMemberDeclarationsRequest, Response<SearchFindMemberDeclarationsResponse>>
	{
		public SearchFindMemberDeclarations(SearchFindMemberDeclarationsRequest @params)
			: base(@params)
		{
		}
	}

	public class SearchFindMemberReferences : Request<SearchFindMemberReferencesRequest, Response<SearchFindMemberReferencesResponse>>
	{
		public SearchFindMemberReferences(SearchFindMemberReferencesRequest @params)
			: base(@params)
		{
		}
	}

	public class SearchFindTopLevelDeclarations : Request<SearchFindTopLevelDeclarationsRequest, Response<SearchFindTopLevelDeclarationsResponse>>
	{
		public SearchFindTopLevelDeclarations(SearchFindTopLevelDeclarationsRequest @params)
			: base(@params)
		{
		}
	}

	public class SearchGetTypeHierarchy : Request<SearchGetTypeHierarchyRequest, Response<SearchGetTypeHierarchyResponse>>
	{
		public SearchGetTypeHierarchy(SearchGetTypeHierarchyRequest @params)
			: base(@params)
		{
		}
	}

	public class EditGetAssists : Request<EditGetAssistsRequest, Response<EditGetAssistsResponse>>
	{
		public EditGetAssists(EditGetAssistsRequest @params)
			: base(@params)
		{
		}
	}

	public class EditGetAvailableRefactorings : Request<EditGetAvailableRefactoringsRequest, Response<EditGetAvailableRefactoringsResponse>>
	{
		public EditGetAvailableRefactorings(EditGetAvailableRefactoringsRequest @params)
			: base(@params)
		{
		}
	}

	public class EditGetFixes : Request<EditGetFixesRequest, Response<EditGetFixesResponse>>
	{
		public EditGetFixes(EditGetFixesRequest @params)
			: base(@params)
		{
		}
	}

	public class EditGetRefactoring : Request<EditGetRefactoringRequest, Response<EditGetRefactoringResponse>>
	{
		public EditGetRefactoring(EditGetRefactoringRequest @params)
			: base(@params)
		{
		}
	}

	public class DebugCreateContext : Request<DebugCreateContextRequest, Response<DebugCreateContextResponse>>
	{
		public DebugCreateContext(DebugCreateContextRequest @params)
			: base(@params)
		{
		}
	}

	public class DebugDeleteContext : Request<DebugDeleteContextRequest, Response>
	{
		public DebugDeleteContext(DebugDeleteContextRequest @params)
			: base(@params)
		{
		}
	}

	public class DebugMapUri : Request<DebugMapUriRequest, Response<DebugMapUriResponse>>
	{
		public DebugMapUri(DebugMapUriRequest @params)
			: base(@params)
		{
		}
	}

	public class DebugSetSubscriptions : Request<DebugSetSubscriptionsRequest, Response>
	{
		public DebugSetSubscriptions(DebugSetSubscriptionsRequest @params)
			: base(@params)
		{
		}
	}


}