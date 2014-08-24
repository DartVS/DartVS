using System;

namespace DanTup.DartAnalysis
{
	// TODO: Replace this with code-gen if/when Google update API to have a prseable version of this.
	[Flags]
	public enum AnalysisElementFlags
	{
		None,
		Abstract = 0x01,
		Constant = 0x02,
		Deprecated = 0x20,
		Final = 0x04,
		Private = 0x10,
		Static = 0x08,
	}
}
