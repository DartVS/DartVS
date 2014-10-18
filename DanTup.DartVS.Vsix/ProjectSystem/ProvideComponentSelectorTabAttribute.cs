namespace DanTup.DartVS.ProjectSystem
{
	using System;
	using Microsoft.VisualStudio.Shell;

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ProvideComponentSelectorTabAttribute : RegistrationAttribute
	{
		private readonly Guid _componentSelectorTabGuid;
		private readonly string _name;

		private int _sortOrder = 0x35;

		public ProvideComponentSelectorTabAttribute(Type componentSelectorTabType, string name)
		{
			if (componentSelectorTabType == null)
				throw new ArgumentNullException("componentSelectorTabType");
			if (name == null)
				throw new ArgumentNullException("name");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name cannot be empty", "name");

			_componentSelectorTabGuid = componentSelectorTabType.GUID;
			_name = name;
		}

		public ProvideComponentSelectorTabAttribute(Guid componentSelectorTabGuid, string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name cannot be empty", "name");
			if (componentSelectorTabGuid == Guid.Empty)
				throw new ArgumentException("componentSelectorTabGuid cannot be empty", "componentSelectorTabGuid");

			_componentSelectorTabGuid = componentSelectorTabGuid;
			_name = name;
		}

		public Guid ComponentSelectorTabGuid
		{
			get
			{
				return _componentSelectorTabGuid;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public int SortOrder
		{
			get
			{
				return _sortOrder;
			}

			set
			{
				_sortOrder = value;
			}
		}

		private string BaseRegistryKey
		{
			get
			{
				return string.Format(@"ComponentPickerPages\{0}", _name);
			}
		}

		public override void Register(RegistrationContext context)
		{
			using (var key = context.CreateKey(BaseRegistryKey))
			{
				key.SetValue(string.Empty, string.Empty);
				key.SetValue("Package", context.ComponentType.GUID.ToString("B"));
				key.SetValue("Page", _componentSelectorTabGuid.ToString("B"));
				key.SetValue("Sort", _sortOrder);
			}
		}

		public override void Unregister(RegistrationContext context)
		{
			context.RemoveKey(BaseRegistryKey);
		}
	}
}
