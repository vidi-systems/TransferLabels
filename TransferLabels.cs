using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ViDi2.Training.UI;
using ViDi2.Training;

namespace TransferLabels
{
    public class Class1 : ViDi2.Training.UI.IPlugin
    {
        public void DeInitialize()
        {
            
        }

        public string Description
        {
            get { return "creates a green tool on top of an existing blue tool and transfers the labels"; }
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
            pluginMenuItem.Click += (o, a) =>
            {
                Task.Run(() =>
                {
                    Run();
                });
            };
            pluginContainerMenuItem.Items.Add(pluginMenuItem);   
        }

        void Run()
        {
            try {
                ITool currentTool = context.MainWindow.ToolChain.Tool;
                if (currentTool.Type == ViDi2.ToolType.Blue && ((IBlueTool)currentTool).Models.Count() > 0)
                {
                    IBlueTool blueTool = (IBlueTool)currentTool;
                    IGreenTool greenTool = (IGreenTool)currentTool.Children.Add("Green", ViDi2.ToolType.Green);
                    IBlueRegionOfInterest blueROI = (IBlueRegionOfInterest)greenTool.RegionOfInterest;
                    blueROI.Model = ((IBlueTool)currentTool).Models.First();
                    double fsize = currentTool.Parameters.FeatureSize * 2;
                    blueROI.Size = new System.Windows.Size(fsize, fsize);

                    greenTool.Database.Process("", false);

                    var greenViews = greenTool.Database.List("");
                    var blueViews = blueTool.Database.List("");

                    string previousFilename = "";

                    using (greenTool.Database.DeferChangedSignal())
                    {
                        foreach (var blueKeyPair in blueViews)
                        {
                            if (blueKeyPair.Key.Filename != previousFilename)
                            {
                                previousFilename = blueKeyPair.Key.Filename;

                                var blueMarking = blueTool.Database.GetMarking(blueKeyPair.Key.Filename);
                                var greenMarking = greenTool.Database.GetMarking(blueKeyPair.Key.Filename);

                                int matchNumber = 0;

                                foreach (var blueView in blueMarking.Views)
                                {
                                    if (blueView is ViDi2.IBlueLabeledView)
                                    {
                                        var labeledBlueView = (ViDi2.IBlueLabeledView)blueView;

                                        foreach (var match in labeledBlueView.Matches)
                                        {
                                            char closestFeatureId = '0';
                                            double closestDistance = Double.MaxValue;

                                            foreach (var labeledMatch in labeledBlueView.LabeledMatches)
                                            {
                                                double length = (match.Position - labeledMatch.Position).Length;
                                                if (length < closestDistance)
                                                {
                                                    closestFeatureId = labeledMatch.Features.First().Id;
                                                    closestDistance = length;
                                                }
                                            }

                                            var key = new ViewKey(blueKeyPair.Key.Filename, matchNumber);

                                            if (closestDistance < fsize / 4)
                                            {
                                                greenTool.Database.Tag(key.ToFilterString(), "" + closestFeatureId);
                                            }
                                            else
                                            {
                                                greenTool.Database.Tag(key.ToFilterString(), "False positive");
                                            }

                                            ++matchNumber;
                                        }
                                    }
                                    else
                                    {
                                        matchNumber += blueView.Matches.Count;
                                    }
                                }
                            }
                        }
                    }
                    System.Windows.MessageBox.Show("Done!");
                }
                else
                {
                    System.Windows.MessageBox.Show("The current tool is not a blue tool having at least one model.");
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        public string Name
        {
            get { return "Transfer labels"; }
        }

        public int Version
        {
            get { return 0; }
        }
    }
}
