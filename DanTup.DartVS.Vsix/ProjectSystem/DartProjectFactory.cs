namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;
	using global::DartVS.Pub;

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

		protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled)
		{
			base.CreateProject(fileName, location, name, flags, ref projectGuid, out project, out canceled);

			// TODO: Wire this up.
			Action<string> writeToOutputPane = delegate(string message) { };

			// TOOD: Should this come via MEF? Is it possible here?
			var pub = new PubService();

			// TODO: Is it safe to block here? 
			pub.GetAsync(location, writeToOutputPane).Wait();
		}
	}
}
