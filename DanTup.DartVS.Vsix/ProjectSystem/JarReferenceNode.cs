namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Project;
	using File = System.IO.File;
	using Path = System.IO.Path;

	[ComVisible(true)]
	public class JarReferenceNode : ReferenceNode
	{
		private readonly string _projectRelativeFilePath;

		private Automation.OAJarReference _jarReference;

		public JarReferenceNode(ProjectNode root, ProjectElement element)
			: base(root, element)
		{
			if (root == null)
				throw new ArgumentNullException("root");
			if (element == null)
				throw new ArgumentNullException("element");

			_projectRelativeFilePath = element.Item.EvaluatedInclude;
			ProjectManager.ItemIdMap.UpdateCanonicalName(this);
		}

		public JarReferenceNode(ProjectNode root, string fileName)
			: base(root)
		{
			if (root == null)
				throw new ArgumentNullException("root");
			if (fileName == null)
				throw new ArgumentNullException("fileName");
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentException("fileName cannot be empty", "fileName");

			_projectRelativeFilePath = fileName;
		}

		public string InstalledFilePath
		{
			get
			{
				return Path.Combine(ProjectManager.ProjectFolder, ProjectRelativeFilePath);
			}
		}

		public override string Url
		{
			get
			{
				return ProjectRelativeFilePath;
			}
		}

		public override bool CanCacheCanonicalName
		{
			get
			{
				return !string.IsNullOrEmpty(ProjectRelativeFilePath);
			}
		}

		public override string Caption
		{
			get
			{
				return Path.GetFileNameWithoutExtension(ProjectRelativeFilePath);
			}
		}

		public override object Object
		{
			get
			{
				if (_jarReference == null)
					_jarReference = new Automation.OAJarReference(this);

				return _jarReference;
			}
		}

		private string ProjectRelativeFilePath
		{
			get
			{
				return _projectRelativeFilePath;
			}
		}

		protected override NodeProperties CreatePropertiesObject()
		{
			return new JarReferenceProperties(this);
		}

		protected override void BindReferenceData()
		{
			if (ItemNode == null || ItemNode.Item == null)
			{
				ProjectElement element = new ProjectElement(ProjectManager, ProjectRelativeFilePath, DartProjectFileConstants.JarReference);

				// Set the basic information about this reference
				element.SetMetadata(DartProjectFileConstants.IncludeInBuild, true.ToString());
				element.SetMetadata(ProjectFileConstants.Private, false.ToString());

				ItemNode = element;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public override bool IsAlreadyAdded(out ReferenceNode existingEquivalentNode)
		{
			string fullPath = Path.GetFullPath(InstalledFilePath).Replace('\\', '/');

			ReferenceContainerNode referencesFolder = this.ProjectManager.FindChild(ReferenceContainerNode.ReferencesNodeVirtualName) as ReferenceContainerNode;
			for (HierarchyNode node = referencesFolder.FirstChild; node != null; node = node.NextSibling)
			{
				JarReferenceNode referenceNode = node as JarReferenceNode;
				if (referenceNode != null)
				{
					string otherFullPath = Path.GetFullPath(referenceNode.InstalledFilePath).Replace('\\', '/');
					if (string.Equals(fullPath, otherFullPath, StringComparison.OrdinalIgnoreCase))
					{
						existingEquivalentNode = referenceNode;
						return true;
					}
				}
			}

			existingEquivalentNode = null;
			return false;
		}

		protected override bool CanShowDefaultIcon()
		{
			return File.Exists(InstalledFilePath);
		}

		protected override void ResolveReference()
		{
		}
	}
}
