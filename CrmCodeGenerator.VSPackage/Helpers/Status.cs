using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace CrmCodeGenerator.VSPackage.Helpers
{
    public static class Status
    {
        public static void Update(string message)
        {
            //Configuration.Instance.DTE.ExecuteCommand("View.Output");
            var dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
            var win = dte.Windows.Item("{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}");//todo mg: change to const
            win.Visible = true;

            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "Crm Code Generator", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);
            pane.Activate();
            pane.OutputString(message);
            pane.OutputString("\n");
            pane.FlushToTaskList();
            System.Windows.Forms.Application.DoEvents();
        }

        public static void Clear()
        {
            IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            IVsOutputWindowPane pane;
            int hr = outputWindow.CreatePane(guidGeneral, "Crm Code Generator", 1, 0);
            hr = outputWindow.GetPane(guidGeneral, out pane);
            pane.Clear();
            pane.FlushToTaskList();
            System.Windows.Forms.Application.DoEvents();
        }
    }
}