namespace DanTup.DartVS.ProjectSystem
{
	using DanTup.DartVS.ProjectSystem.PropertyPages;

	public sealed class DartBuildOptions
	{
		private DartGeneralPropertyPagePanel _general;
		private DartBuildPropertyPagePanel _build;
		private DartDebugPropertyPagePanel _debug;

		public DartGeneralPropertyPagePanel General
		{
			get
			{
				return _general;
			}

			set
			{
				_general = value;
				if (_general != null)
					_general.Disposed += (sender, e) => _general = null;
			}
		}

		public DartBuildPropertyPagePanel Build
		{
			get
			{
				return _build;
			}

			set
			{
				_build = value;
				if (_build != null)
					_build.Disposed += (sender, e) => _build = null;
			}
		}

		public DartDebugPropertyPagePanel Debug
		{
			get
			{
				return _debug;
			}

			set
			{
				_debug = value;
				if (_debug != null)
					_debug.Disposed += (sender, e) => _debug = null;
			}
		}
	}
}
