using System;

namespace DanTup.DartAnalysis
{
	class AnalysisMethodAttribute : Attribute
	{
		public string Name { get; set; }

		public AnalysisMethodAttribute(string name)
		{
			this.Name = name;
		}
	}
}
