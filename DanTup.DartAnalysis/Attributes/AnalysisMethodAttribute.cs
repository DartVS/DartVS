using System;

namespace DanTup.DartAnalysis
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class AnalysisMethodAttribute : Attribute
	{
		public string Name { get; private set; }

		public AnalysisMethodAttribute(string name)
		{
			this.Name = name;
		}
	}
}
