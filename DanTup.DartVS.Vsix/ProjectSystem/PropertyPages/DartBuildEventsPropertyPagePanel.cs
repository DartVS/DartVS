namespace DanTup.DartVS.ProjectSystem.PropertyPages
{
    using System;

    public partial class DartBuildEventsPropertyPagePanel : DartPropertyPagePanel
    {
        public DartBuildEventsPropertyPagePanel()
            : this(null)
        {
        }

        public DartBuildEventsPropertyPagePanel(DartPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            InitializeComponent();
        }

        public new DartBuildEventsPropertyPage ParentPropertyPage
        {
            get
            {
                return (DartBuildEventsPropertyPage)base.ParentPropertyPage;
            }
        }

        public string PreBuildEvent
        {
            get
            {
                return txtPreBuildCommandLine.Text;
            }

            set
            {
                txtPreBuildCommandLine.Text = value;
            }
        }

        public string PostBuildEvent
        {
            get
            {
                return txtPostBuildCommandLine.Text;
            }

            set
            {
                txtPostBuildCommandLine.Text = value;
            }
        }

        public string RunPostBuildEvent
        {
            get
            {
                switch (cmbRunPostBuildWhen.SelectedIndex)
                {
                case 0:
                    return "Always";

                case 2:
                    return "OnOutputUpdated";

                case 1:
                default:
                    return "OnBuildSuccess";
                }
            }
            set
            {
                switch (value)
                {
                case "Always":
                    cmbRunPostBuildWhen.SelectedIndex = 0;
                    break;

                case "OnOutputUpdated":
                    cmbRunPostBuildWhen.SelectedIndex = 2;
                    break;

                case "OnBuildSuccess":
                default:
                    cmbRunPostBuildWhen.SelectedIndex = 1;
                    break;
                }
            }
        }

        // when to run
        void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }

        // post-build
        void textBox2_TextChanged(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }

        // pre-build
        void textBox1_TextChanged(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }
    }
}
