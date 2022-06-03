using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TiaAddInEplan
{
    public class Sync
    {
        private readonly Project project;
        private readonly string path;

        private readonly FileLogger logger = new FileLogger();

        public Sync(Project project, string path)
        {
            this.project = project ?? throw new ArgumentNullException("project cannot be null");
            this.path = path ?? throw new ArgumentNullException("path cannot be null");
        }

        public void Synchronize()
        {

            var eplanDevices = new EplanDevices(project, path);

        }
    }
}