using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DanTup.DartVS
{
	/// <summary>
	/// Tracks which open projects ar possible Dart projects so that they can be analysed.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export]
	public class DartProjectTracker
	{
		[Import]
		internal SVsServiceProvider ServiceProvider = null;

		ConcurrentDictionary<string, Project> trackedProjects = new ConcurrentDictionary<string, Project>();

		[ImportingConstructor]
		public DartProjectTracker([Import]SVsServiceProvider serviceProvider)
		{
			var dte = (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));

			// Subscribe to project changes.
			dte.Events.SolutionEvents.ProjectAdded += TrackProject;
			dte.Events.SolutionEvents.ProjectRemoved += UntrackProject;
			dte.Events.SolutionEvents.BeforeClosing += UntrackAllProjects;

			// Subscribe for existing projects already open when we were triggered.
			foreach (Project project in dte.Solution.Projects)
				TrackProject(project);
		}

		void TrackProject(Project project)
		{
			if (!IsDartProject(project))
				return;

			trackedProjects.TryAdd(GetProjectLocation(project), project);
		}

		void UntrackProject(Project project)
		{
			Project _;
			trackedProjects.TryRemove(GetProjectLocation(project), out _);
		}

		void UntrackAllProjects()
		{
			trackedProjects.Clear();
		}

		string GetProjectLocation(Project project)
		{
			// This way seems to be reliable for both projects and "web sites" (whereas project.FullName gives URL for web sites).
			return (string)project.Properties.Item("FullPath").Value;
		}

		bool IsDartProject(Project project)
		{
			if (project == null)
				return false;

			var allProjectItems = project.ProjectItems == null
				? null
				: project.ProjectItems.Cast<ProjectItem>().Flatten(pi => pi.ProjectItems == null ? null : pi.ProjectItems.Cast<ProjectItem>());

			return allProjectItems
				.Any(pi =>
					// Has a filename that's a Dart file.
					Enumerable.Range(0, pi.FileCount)
					.Select(fi => pi.FileNames[(short)fi])
					.Any(IsDartFile)
				);
		}

		bool IsDartFile(string filename)
		{
			return string.Equals(Path.GetExtension(filename), ".dart", StringComparison.OrdinalIgnoreCase);
		}
	}
}
