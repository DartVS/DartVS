namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    public partial class DartApplicationPropertyPagePanel : DartPropertyPagePanel
    {
        private static readonly string DisplayJavaArchiveOutputType = "Java Archive (jar)";
        private static readonly string DisplayNotSetStartupObject = "(Not Set)";

        private static readonly ReadOnlyCollection<string> _emptyList = new ReadOnlyCollection<string>(new string[0]);

        private ReadOnlyCollection<string> _availableTargetVirtualMachines = _emptyList;
        private ReadOnlyCollection<string> _availableOutputTypes = _emptyList;
        private ReadOnlyCollection<string> _availableStartupObjects = _emptyList;

        public DartApplicationPropertyPagePanel()
            : this(null)
        {
        }

        public DartApplicationPropertyPagePanel(DartApplicationPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            InitializeComponent();
        }

        internal new DartApplicationPropertyPage ParentPropertyPage
        {
            get
            {
                return (DartApplicationPropertyPage)base.ParentPropertyPage;
            }
        }

        public ReadOnlyCollection<string> AvailableTargetVirtualMachines
        {
            get
            {
                return _availableTargetVirtualMachines;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Any(string.IsNullOrEmpty))
                    throw new ArgumentException("value cannot contain any null or empty strings", "value");

                if (_availableTargetVirtualMachines.SequenceEqual(value, StringComparer.CurrentCulture))
                    return;

                _availableTargetVirtualMachines = value;
                cmbTargetVirtualMachine.Items.Clear();
                cmbTargetVirtualMachine.Items.AddRange(value.ToArray());
            }
        }

        public ReadOnlyCollection<string> AvailableOutputTypes
        {
            get
            {
                return _availableOutputTypes;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Any(string.IsNullOrEmpty))
                    throw new ArgumentException("value cannot contain any null or empty strings", "value");

                if (_availableOutputTypes.SequenceEqual(value, StringComparer.CurrentCulture))
                    return;

                _availableOutputTypes = value;
                cmbOutputType.Items.Clear();
                cmbOutputType.Items.AddRange(value.Select(i => (i == DartProjectFileConstants.JavaArchiveOutputType) ? DisplayJavaArchiveOutputType : i).ToArray());
            }
        }

        public ReadOnlyCollection<string> AvailableStartupObjects
        {
            get
            {
                return _availableStartupObjects;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Contains(null))
                    throw new ArgumentException("value cannot contain any null", "value");

                if (_availableStartupObjects.SequenceEqual(value, StringComparer.CurrentCulture))
                    return;

                _availableStartupObjects = value;
                cmbStartupObject.Items.Clear();
                cmbStartupObject.Items.AddRange(value.Select(i => string.IsNullOrEmpty(i) ? DisplayNotSetStartupObject : i).ToArray());
            }
        }

        public string PackageName
        {
            get
            {
                return txtPackageName.Text;
            }

            set
            {
                txtPackageName.Text = value ?? string.Empty;
            }
        }

        public string TargetVirtualMachine
        {
            get
            {
                if (cmbTargetVirtualMachine.SelectedItem == null)
                    return string.Empty;

                return cmbTargetVirtualMachine.SelectedItem.ToString();
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                cmbTargetVirtualMachine.SelectedItem = value;
            }
        }

        public string OutputType
        {
            get
            {
                if (cmbOutputType.SelectedItem == null)
                    return string.Empty;

                string outputType = cmbOutputType.SelectedItem.ToString();
                if (outputType == DisplayJavaArchiveOutputType)
                    outputType = DartProjectFileConstants.JavaArchiveOutputType;

                return outputType;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                string outputType = value;
                if (outputType == DartProjectFileConstants.JavaArchiveOutputType)
                    outputType = DisplayJavaArchiveOutputType;

                cmbOutputType.SelectedItem = outputType;
            }
        }

        public string StartupObject
        {
            get
            {
                if (cmbStartupObject.SelectedItem == null)
                    return string.Empty;

                string startupObject = cmbStartupObject.SelectedItem.ToString();
                if (startupObject == DisplayNotSetStartupObject)
                    startupObject = string.Empty;

                return startupObject;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                string startupObject = value;
                if (string.IsNullOrEmpty(startupObject))
                    startupObject = DisplayNotSetStartupObject;

                cmbStartupObject.SelectedItem = startupObject;
            }
        }

        private void HandleBuildSettingChanged(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }
    }
}
