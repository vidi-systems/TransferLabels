using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViDi2.Training.UI;
using ViDi2.Training;

namespace ClassLibrary2
{
    public class Class1 : ViDi2.Training.UI.IPlugin
    {
        public void DeInitialize()
        {
            
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
            pluginMenuItem = new MenuItem()
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
            try {
                ITool currentTool = context.MainWindow.ToolChain.Tool;
                if (currentTool.Type == ViDi2.ToolType.Blue && ((IBlueTool)currentTool).Models.Count() > 0)
                {
                    IGreenTool greenTool = (IGreenTool) currentTool.Children.Add("Green", ViDi2.ToolType.Green);
                    IBlueRegionOfInterest blueROI = (IBlueRegionOfInterest) greenTool.RegionOfInterest;
                    blueROI.Model = ((IBlueTool) currentTool).Models.First();
                    double fsize = currentTool.Parameters.FeatureSize * 2;
                    blueROI.Size = new System.Windows.Size(fsize, fsize);
                    greenTool.Database.Process("", false);
                    var greenViews = greenTool.Database.List("");
                    var blueViews = currentTool.Database.List("");

                    foreach (var blueKeyPair in blueViews)
                    {
                        var blueMarking = currentTool.Database.GetMarking(blueKeyPair.Key.Filename);
                        var greenMarking = greenTool.Database.GetMarking(blueKeyPair.Key.Filename);
                    
                        foreach (ViDi2.IBlueLabeledView blueView in blueMarking.Views) {                        
                            int matchNumber = 0;
                            foreach (var match in blueView.Matches)
                            {
                                char featureId = match.Features.First().Id;

                                greenTool.Database.Tag("'" + blueKeyPair.Key.Filename + ":" + matchNumber + "'", featureId + "");
                                ++matchNumber;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
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
