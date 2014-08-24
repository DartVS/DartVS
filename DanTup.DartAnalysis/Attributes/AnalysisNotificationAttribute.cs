using System;

namespace DanTup.DartAnalysis
{
	class AnalysisNotificationAttribute : Attribute
	{
		public string Name { get; set; }

		public AnalysisNotificationAttribute(string name)
		{
			this.Name = name;
		}
	}
}
