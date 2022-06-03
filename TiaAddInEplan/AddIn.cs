using System.Collections.Generic;
using Siemens.Engineering;
using Siemens.Engineering.Cax;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Compare;
using Siemens.Engineering.Download;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.RuntimeScripting;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Extensions;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW.Utilities;
using Siemens.Engineering.Library;
using Siemens.Engineering.Library.MasterCopies;
using Siemens.Engineering.Library.Types;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.TechnologicalObjects;
using Siemens.Engineering.SW.TechnologicalObjects.Motion;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Upload;
using Siemens.Engineering.AddIn.Menu;
using Siemens.Engineering.Settings;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TiaAddInEplan
{
    public class AddIn : ContextMenuAddIn
    {

        TiaPortal _tiaportal;

        private const string s_DisplayNameOfAddIn = "Tia Add In Eplan";
        public AddIn(TiaPortal tiaportal) : base(s_DisplayNameOfAddIn)
        {

            _tiaportal = tiaportal;
        }

        private readonly FileLogger logger = new FileLogger();

        private readonly string path = @"C:\Users\blaroche\source\repos\TiaAddInEplan\TiaAddInEPLAN\TIA_Portal_Export_Test.csv";
        protected override void BuildContextMenuItems(ContextMenuAddInRoot addInRootSubmenu)
        {

            addInRootSubmenu.Items.AddActionItem<Project>("Sync Project", OnSyncProject, OnStatusUpdateProject);
        }

        private void OnSyncProject(MenuSelectionProvider<Project> menuSelectionProvider)
        {

            try
            {
                logger.Log("Sync started");

                new Sync(menuSelectionProvider.GetSelection<Project>().First(), path).Synchronize();
            }
            catch (Exception ex)
            {
                logger.Log("Exception: " + ex.Message);
                throw;
            }

        }

        private MenuStatus OnStatusUpdateProject(MenuSelectionProvider<Project> menuSelectionProvider)
        {
            return MenuStatus.Enabled;
        }


    }

}