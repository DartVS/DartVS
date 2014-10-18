namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;

	using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

	[Guid(DartProjectConstants.ProjectFactoryGuidString)]
	public class DartProjectFactory : ProjectFactory
	{
		internal DartProjectFactory(DartProjectPackage package)
			: base(package)
		{
		}

		public new DartProjectPackage Package
		{
			get
			{
				return (DartProjectPackage)base.Package;
			}
		}

		protected override ProjectNode CreateProject()
		{
			DartProjectNode node = new DartProjectNode(Package);
			IOleServiceProvider serviceProvider = (IOleServiceProvider)((IServiceProvider)base.Package).GetService(typeof(IOleServiceProvider));
			node.SetSite(serviceProvider);
			return node;
		}
	}
}
