using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

		SolutionEvents solutionEvents;

		ConcurrentDictionary<string, Project> dartProjects = new ConcurrentDictionary<string, Project>();
		ConcurrentDictionary<string, FileSystemWatcher> openProjectWatchers = new ConcurrentDictionary<string, FileSystemWatcher>();

		ReplaySubject<DartProjectInfo[]> projectsChanged = new ReplaySubject<DartProjectInfo[]>(1); // Keep a buffer of one, so new subscribers get the projects immediately.
		public IObservable<DartProjectInfo[]> ProjectsChanged { get { return projectsChanged.AsObservable(); } }

		[ImportingConstructor]
		public DartProjectTracker([Import]SVsServiceProvider serviceProvider)
		{
			var dte = (EnvDTE.DTE)serviceProvider.GetService(typeof(EnvDTE.DTE));

			// Subscribe to project changes.
			solutionEvents = dte.Events.SolutionEvents;
			solutionEvents.ProjectAdded += TrackProject;
			solutionEvents.ProjectRemoved += UntrackProject;
			solutionEvents.BeforeClosing += UntrackAllProjects;

			// Subscribe for existing projects already open when we were triggered.
			foreach (Project project in dte.Solution.Projects)
				TrackProject(project);
		}

		void TrackProject(Project project)
		{
			var projectFolder = GetProjectLocation(project);

			openProjectWatchers.TryAdd(projectFolder, CreateWatcher(project));

			if (IsDartProject(project))
			{
				if (dartProjects.TryAdd(projectFolder, project))
					RaiseProjectsChanged();
			}
		}

		void UntrackProject(Project project)
		{
			var projectFolder = GetProjectLocation(project);

			// Remove the FSW
			FileSystemWatcher watcher;
			openProjectWatchers.TryRemove(projectFolder, out watcher);
			if (watcher != null)
				watcher.Dispose();

			// Untrack the project
			Project _;
			if (dartProjects.TryRemove(projectFolder, out _))
				RaiseProjectsChanged();
		}

		void UntrackAllProjects()
		{
			// Remove all FSWs
			foreach (var watcher in openProjectWatchers.Values)
				watcher.Dispose();
			openProjectWatchers.Clear();

			// Untrack all projects
			dartProjects.Clear();

			RaiseProjectsChanged();
		}

		string GetProjectLocation(Project project)
		{
			// This way seems to be reliable for both projects and "web sites" (whereas project.FullName gives URL for web sites).
			return (string)project.Properties.Item("FullPath").Value;
		}

		FileSystemWatcher CreateWatcher(Project project)
		{
			var watcher = new FileSystemWatcher(GetProjectLocation(project), "*.dart");

			watcher.IncludeSubdirectories = true;
			watcher.NotifyFilter = NotifyFilters.FileName;
			watcher.Created += (o, e) => UpdateProject(project, e.FullPath);
			watcher.Deleted += (o, e) => UpdateProject(project, e.FullPath);
			watcher.Renamed += (o, e) => UpdateProject(project, e.FullPath);

			watcher.EnableRaisingEvents = true;

			return watcher;
		}

		void RaiseProjectsChanged()
		{
			projectsChanged.OnNext(dartProjects.Values.Select(p => new DartProjectInfo(GetProjectLocation(p), p)).ToArray());
		}

		void UpdateProject(Project project, string fullPath)
		{
			// A file was modified that had/has a .dart extension, so we need to track/untrack accordingly.
			var isDartProject = IsDartProject(project);
			var wasDartProject = dartProjects.ContainsKey(GetProjectLocation(project));

			if (isDartProject && !wasDartProject)
				TrackProject(project);
			else if (!isDartProject && wasDartProject)
				UntrackProject(project);
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

		internal static bool IsDartFile(string filename)
		{
			return string.Equals(Path.GetExtension(filename), ".dart", StringComparison.OrdinalIgnoreCase);
		}
	}
}
