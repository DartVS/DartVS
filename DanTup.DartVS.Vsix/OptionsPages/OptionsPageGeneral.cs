using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS.OptionsPages
{
	public class OptionsPageGeneral : DialogPage
	{
		public OptionsPageGeneral()
		{
		}

		[Category("Dartium")]
		[Description("Dartium location")]
		[DisplayName("Dartium location")]
		public string DartiumLocation
		{
			get;
			set;
		}

		[Category("Dartium")]
		[Description("Dartium profile location")]
		[DisplayName("Dartium profile location")]
		public string DartiumProfileLocation
		{
			get;
			set;
		}
	}
}
