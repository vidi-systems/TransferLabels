using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViDi2.Training.UI;

namespace ClassLibrary2
{
    public class TransferLabels : ViDi2.Training.UI.IPlugin
    {
        public void DeInitialize()
        {
            throw new NotImplementedException();
        }

        public string Description
        {
            get { return "create a green tool with blue tool label"; }
        }

        IPluginContext context;
        MenuItem pluginMenuItem;
        
        public void Initialize(ViDi2.Training.UI.IPluginContext context)
        {
            this.context = context;

            var pluginContainerMenuItem =
               context.MainWindow.MainMenu.Items.OfType<System.Windows.Controls.MenuItem>().
               First(i => (string)i.Header == "Plugins");
            pluginMenuItem = new MenuItem)(
            {
                Header = ((IPlugin)this).Name,
                IsEnabled = true,
                ToolTip = ((IPlugin)this).Description
            };
            pluginMenuItem.Click += (o, a) => { Run(); };
            pluginContainerMenuItem.Items.Add(pluginMenuItem);   
        }

        void Run()
        {

        }

        public string Name
        {
            get { return "my test plugin"; }
        }

        public int Version
        {
            get { return 0; }
        }
    }
}
