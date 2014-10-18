namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using EnvDTE;
	using Microsoft.VisualStudio.Project;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;

	using __VSHPROPID = Microsoft.VisualStudio.Shell.Interop.__VSHPROPID;
	using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;
	using ComponentSelectorGuids80 = Microsoft.VisualStudio.Shell.Interop.ComponentSelectorGuids80;
	using CultureInfo = System.Globalization.CultureInfo;
	using MSBuild = Microsoft.Build.Evaluation;
	using OAVSProject = Microsoft.VisualStudio.Project.Automation.OAVSProject;
	using Path = System.IO.Path;
	using PrjKind = VSLangProj.PrjKind;
	using StackTrace = System.Diagnostics.StackTrace;
	using VSCOMPONENTSELECTORTABINIT = Microsoft.VisualStudio.Shell.Interop.VSCOMPONENTSELECTORTABINIT;
	using VSConstants = Microsoft.VisualStudio.VSConstants;

	[ComVisible(true)]
	public class DartProjectNode : ProjectNode
	{
		private static readonly char[] charsToEscape = new char[] { '%', '*', '?', '@', '$', '(', ')', ';', '\'' };

		private readonly DartBuildOptions _sharedBuildOptions;
		private VSLangProj.VSProject _vsProject;

		public DartProjectNode(ProjectPackage package)
			: base(package)
		{
			_sharedBuildOptions = new DartBuildOptions();
			CanProjectDeleteItems = true;
			OleServiceProvider.AddService(typeof(VSLangProj.VSProject), HandleCreateService, false);

			AddCatIdMapping(typeof(DartFileNodeProperties), typeof(FileNodeProperties).GUID);
		}

		public DartBuildOptions SharedBuildOptions
		{
			get
			{
				return _sharedBuildOptions;
			}
		}

		public override int ImageIndex
		{
			get
			{
				return HierarchyNode.NoImage;
			}
		}

		public override Guid ProjectGuid
		{
			get
			{
				StackTrace trace = new StackTrace();
				if (trace.GetFrames().Any(i => i.GetMethod().Name == "IsSupported" && i.GetMethod().DeclaringType.Name == "VsUtility" && i.GetMethod().DeclaringType.Namespace == "NuGet.VisualStudio"))
				{
					// NuGet only operates with a select group of project kinds. This hook tricks it into working with other projects.
					return new Guid(PrjKind.prjKindCSharpProject);
				}

				return typeof(DartProjectFactory).GUID;
			}
		}

		public override string ProjectType
		{
			get
			{
				return "Dart";
			}
		}

		protected override bool SupportsProjectDesigner
		{
			get
			{
				return true;
			}
		}

		protected internal VSLangProj.VSProject VSProject
		{
			get
			{
				if (_vsProject == null)
					_vsProject = new OAVSProject(this);

				return _vsProject;
			}
		}

		protected override ProjectElement AddFolderToMSBuild(string folder, string itemType)
		{
			if (itemType == ProjectFileConstants.Folder)
			{
				ProjectElement folderElement = new ProjectElement(this, null, true);
				folderElement.Rename(folder);
				folderElement.SetMetadata(ProjectFileConstants.Name, folder);
				folderElement.SetMetadata(ProjectFileConstants.BuildAction, itemType);
				return folderElement;
			}
			else
			{
				return base.AddFolderToMSBuild(folder, itemType);
			}
		}

		protected override string GetComponentSelectorBrowseFilters()
		{
			return "Java Archive Files (*.jar)\0*.jar\0";
		}

		protected override ReadOnlyCollection<VSCOMPONENTSELECTORTABINIT> GetComponentSelectorTabList()
		{
			// no .NET or COM assemblies
			return new List<VSCOMPONENTSELECTORTABINIT>()
				{
					//new VSCOMPONENTSELECTORTABINIT {
					//    guidTab = VSConstants.GUID_COMPlusPage,
					//    varTabInitInfo = GetComponentPickerDirectories(),
					//},
					//new VSCOMPONENTSELECTORTABINIT {
					//    guidTab = VSConstants.GUID_COMClassicPage,
					//},
					new VSCOMPONENTSELECTORTABINIT {
						// Tell the Add Reference dialog to call hierarchies GetProperty with the following
						// propID to enablefiltering out ourself from the Project to Project reference
						varTabInitInfo = (int)__VSHPROPID.VSHPROPID_ShowProjInSolutionPage,
						guidTab = VSConstants.GUID_SolutionPage,
					},
					// Add the Browse for file page            
					new VSCOMPONENTSELECTORTABINIT {
						varTabInitInfo = 0,
						guidTab = VSConstants.GUID_BrowseFilePage,
					},
					// Add the Maven packages page
					new VSCOMPONENTSELECTORTABINIT {
						varTabInitInfo = 0,
						guidTab = DartProjectConstants.MavenComponentSelectorGuid,
					},
					new VSCOMPONENTSELECTORTABINIT {
						guidTab = new Guid(ComponentSelectorGuids80.MRUPage),
					},
				}.AsReadOnly();
		}

		private static string Escape(string unescapedString)
		{
			if (unescapedString == null)
				throw new ArgumentNullException("unescapedString", "Null strings not allowed.");

			if (!ContainsReservedCharacters(unescapedString))
				return unescapedString;

			StringBuilder builder = new StringBuilder(unescapedString);
			foreach (char ch in charsToEscape)
			{
				int num = Convert.ToInt32(ch);
				string newValue = string.Format(CultureInfo.InvariantCulture, "%{0:x00}", new object[] { num });
				builder.Replace(ch.ToString(CultureInfo.InvariantCulture), newValue);
			}
			return builder.ToString();
		}

		private static bool ContainsReservedCharacters(string unescapedString)
		{
			return unescapedString.IndexOfAny(charsToEscape) != -1;
		}

#if false
        protected override void AddNonMemberFileItems(IList<string> fileList)
        {
            for (int i = fileList.Count - 1; i >= 0; i--)
            {
                if ((new FileInfo(Path.Combine(ProjectFolder, fileList[i])).Attributes & FileAttributes.Hidden) != 0)
                    fileList.RemoveAt(i);
            }

            base.AddNonMemberFileItems(fileList);
        }

        protected override void AddNonMemberFolderItems(IList<string> folderList)
        {
            for (int i = folderList.Count - 1; i >= 0; i--)
            {
                if ((new DirectoryInfo(Path.Combine(ProjectFolder, folderList[i])).Attributes & FileAttributes.Hidden) != 0)
                    folderList.RemoveAt(i);
            }

            base.AddNonMemberFolderItems(folderList);
        }
#endif

		protected override bool FilterItemTypeToBeAddedToHierarchy(string itemType)
		{
			return string.Equals(itemType, DartProjectFileConstants.JarReference, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(itemType, DartProjectFileConstants.MavenReference, StringComparison.OrdinalIgnoreCase)
				|| base.FilterItemTypeToBeAddedToHierarchy(itemType);
		}

		protected override ReferenceContainerNode CreateReferenceContainerNode()
		{
			// Hiding the References node for now.
			// See: https://github.com/DartVS/DartVS/issues/53
			return null;
		}

		public override FileNode CreateFileNode(ProjectElement item)
		{
			return new DartFileNode(this, item);
		}

		public override FolderNode CreateFolderNode(string path, ProjectElement element)
		{
			return new DartFolderNode(this, path, element);
		}

		protected override ConfigProvider CreateConfigProvider()
		{
			return new DartConfigProvider(this);
		}

		protected override bool PerformTargetFrameworkCheck()
		{
			return true;
		}

		protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref vsCommandStatus result)
		{
			if (cmdGroup == VsMenus.guidStandardCommandSet97)
			{
				switch ((VSConstants.VSStd97CmdID)cmd)
				{
				case VSConstants.VSStd97CmdID.BuildSln:
				case VSConstants.VSStd97CmdID.RebuildSln:
				case VSConstants.VSStd97CmdID.CleanSln:
					// "supported" includes these items in the Build menu, but they are not enabled
					result = vsCommandStatus.vsCommandStatusSupported;
					return VSConstants.S_OK;

				case VSConstants.VSStd97CmdID.BuildSel:
				case VSConstants.VSStd97CmdID.RebuildSel:
				case VSConstants.VSStd97CmdID.CleanSel:
					// "unsupported" removes these items from the Build menu
					result = vsCommandStatus.vsCommandStatusUnsupported;
					return VSConstants.S_OK;

				default:
					break;
				}
			}

			return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
		}

		public override bool IsCodeFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return false;

			string extension = Path.GetExtension(fileName);
			if (extension.Equals(".dart", StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		public void SetProjectProperty(string propertyName, _PersistStorageType storageType, string propertyValue, string condition)
		{
			SetProjectProperty(propertyName, storageType, propertyValue, condition, false);
		}

		public void SetProjectProperty(string propertyName, _PersistStorageType storageType, string propertyValue, string condition, bool treatPropertyValueAsLiteral)
		{
			if (propertyValue == null)
				propertyValue = string.Empty;

			// see if the value is the same as what's already in the project so we
			// know whether to actually mark the project file dirty or not
			string oldValue = GetProjectProperty(propertyName, storageType, true);

			if (!String.Equals(oldValue, propertyValue, StringComparison.Ordinal))
			{
				if (treatPropertyValueAsLiteral)
					propertyValue = Escape(propertyValue);

				SetPropertyUnderCondition(propertyName, storageType, condition, propertyValue);
			}
		}

		protected override Guid[] GetConfigurationIndependentPropertyPages()
		{
			return new Guid[]
				{
					typeof(PropertyPages.DartBuildEventsPropertyPage).GUID,
				};
		}

		protected override Guid[] GetConfigurationDependentPropertyPages()
		{
			return new Guid[]
				{
					typeof(PropertyPages.DartApplicationPropertyPage).GUID,
					typeof(PropertyPages.DartBuildPropertyPage).GUID,
					typeof(PropertyPages.DartDebugPropertyPage).GUID,
				};
		}

		protected override Guid[] GetPriorityProjectDesignerPages()
		{
			return new Guid[]
			{
				typeof(PropertyPages.DartApplicationPropertyPage).GUID,
				typeof(PropertyPages.DartBuildPropertyPage).GUID,
				typeof(PropertyPages.DartBuildEventsPropertyPage).GUID,
				typeof(PropertyPages.DartDebugPropertyPage).GUID,
			};
		}

		public override MSBuildResult CallMSBuild(string config, string platform, IVsOutputWindowPane output, string target)
		{
			if ("GetFrameworkPaths".Equals(target, StringComparison.OrdinalIgnoreCase))
				return MSBuildResult.Successful;

			return base.CallMSBuild(config, platform, output, target);
		}

		internal object HandleCreateService(Type serviceType)
		{
			object service = null;

			if (serviceType == typeof(VSLangProj.VSProject))
			{
				service = this.VSProject;
			}
			else if (serviceType == typeof(EnvDTE.Project))
			{
				service = this.GetAutomationObject();
			}

			return service;
		}
	}
}
