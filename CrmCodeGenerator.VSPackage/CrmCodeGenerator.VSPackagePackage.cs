﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CrmCodeGenerator.VSPackage.Dialogs;
using CrmCodeGenerator.VSPackage.Helpers;
using CrmCodeGenerator.VSPackage.Model;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CrmCodeGenerator.VSPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    //this causes the class to load when VS starts [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCrmCodeGenerator_VSPackagePkgString)]
    [ProvideSolutionProps(_strSolutionPersistanceKey)]
    public sealed class CrmCodeGenerator_VSPackagePackage : Package, IVsPersistSolutionOpts, IVsSolutionEvents3
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require
        /// any Visual Studio service because at this point the package object is created but
        /// not sited yet inside Visual Studio environment. The place to do all the other
        /// initialization is the Initialize method.
        /// </summary>
        public CrmCodeGenerator_VSPackagePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            string assemblyFile = typeof(GuidList).Assembly.Location;
            string directory = Path.GetDirectoryName(assemblyFile);

            Status.Update("working directory:" + directory);
            new ManualAssemblyResolver(
                Assembly.LoadFile(directory + "\\Microsoft.Xrm.Sdk.dll"),
                Assembly.LoadFile(directory + "\\Microsoft.Xrm.Tooling.Connector.dll"),
                Assembly.LoadFile(directory + "\\Microsoft.IdentityModel.Clients.ActiveDirectory.dll"),
                Assembly.LoadFile(directory + "\\Microsoft.Crm.Sdk.Proxy.dll"),
                Assembly.LoadFile(directory + "\\Microsoft.Xrm.Sdk.Deployment.dll"));

            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID templateCmd = new CommandID(GuidList.guidCrmCodeGenerator_VSPackageCmdSet, (int)PkgCmdIDList.cmdidAddTemplate);
                MenuCommand tempalteItem = new MenuCommand(AddTemplateCallback, templateCmd);
                mcs.AddCommand(tempalteItem);
            }

            Configuration.Instance.DTE = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;

            AdviseSolutionEvents();

            Configuration.Instance.Settings = ConfigurationFile.ReadFromJsonFile(Configuration.Instance.DTE);
        }

        protected override void Dispose(bool disposing)
        {
            UnadviseSolutionEvents();

            base.Dispose(disposing);
        }

        private IVsSolution solution = null;
        private uint _handleCookie;

        private void AdviseSolutionEvents()
        {
            UnadviseSolutionEvents();

            solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out _handleCookie);
            }
        }

        private void UnadviseSolutionEvents()
        {
            if (solution != null)
            {
                if (_handleCookie != uint.MaxValue)
                {
                    solution.UnadviseSolutionEvents(_handleCookie);
                    _handleCookie = uint.MaxValue;
                }

                solution = null;
            }
        }

        #endregion Package Members

        #region IVsPersistSolutionProps Implementation Code

        private readonly Settings _settings = Configuration.Instance.Settings;

        private const string _strSolutionPersistanceKey = "CrmCodeGeneration";
        private const string _strUsername = "Username";
        private const string _strPassword = "Password";
        
        #region User Options

        public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
        {
            pPersistence.LoadPackageUserOpts(this, _strSolutionPersistanceKey + _strUsername);
            pPersistence.LoadPackageUserOpts(this, _strSolutionPersistanceKey + _strPassword);
            return VSConstants.S_OK;
        }

        public int ReadUserOptions(IStream pOptionsStream, string pszKey)
        {
            try
            {
                using (StreamEater wrapper = new StreamEater(pOptionsStream))
                {
                    string value;
                    using (var bReader = new System.IO.BinaryReader(wrapper))
                    {
                        value = bReader.ReadString();
                        using (var aes = new SimpleAES())
                        {
                            value = aes.Decrypt(value);
                        }
                    }

                    switch (pszKey)
                    {
                        case _strSolutionPersistanceKey + _strUsername:
                            _settings.Username = value;
                            break;

                        case _strSolutionPersistanceKey + _strPassword:
                            _settings.Password = value;
                            break;

                        default:
                            break;
                    }
                }
                return VSConstants.S_OK;
            }
            finally
            {
                Marshal.ReleaseComObject(pOptionsStream);
            }
        }

        public int SaveUserOptions(IVsSolutionPersistence pPersistence)
        {
            pPersistence.SavePackageUserOpts(this, _strSolutionPersistanceKey + _strUsername);
            pPersistence.SavePackageUserOpts(this, _strSolutionPersistanceKey + _strPassword);
            return VSConstants.S_OK;
        }

        public int WriteUserOptions(IStream pOptionsStream, string pszKey)
        {
            try
            {
                string value;
                switch (pszKey)
                {
                    case _strSolutionPersistanceKey + _strUsername:
                        value = _settings.Username;
                        break;

                    case _strSolutionPersistanceKey + _strPassword:
                        value = _settings.Password;
                        break;

                    default:
                        return VSConstants.S_OK;
                }

                using (var aes = new SimpleAES())
                {
                    value = aes.Encrypt(value);
                    using (StreamEater wrapper = new StreamEater(pOptionsStream))
                    {
                        using (var bw = new System.IO.BinaryWriter(wrapper))
                        {
                            bw.Write(value);
                        }
                    }
                }
                return VSConstants.S_OK;
            }
            finally
            {
                Marshal.ReleaseComObject(pOptionsStream);
            }
        }

        #endregion User Options

        public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
        {
            return VSConstants.S_OK;
        }

        #endregion IVsPersistSolutionProps Implementation Code

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void AddTemplateCallback(object sender, EventArgs args)
        {
            try
            {
                AddTemplate();

                _settings.IsActive = true;  // start saving the properties to the *.sln
            }
            catch (UserException e)
            {
                VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, e.Message, "Error", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            catch (Exception e)
            {
                var error = e.Message + "\n" + e.StackTrace;
                System.Windows.MessageBox.Show(error, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void AddTemplate()
        {
            var dte2 = this.GetService(typeof(SDTE)) as EnvDTE80.DTE2;

            var project = dte2.GetSelectedProject();
            if (string.IsNullOrWhiteSpace(project?.FullName))
            {
                throw new UserException("Please select a project first");
            }

            AddTemplate m = new AddTemplate(dte2, project);

            m.Closed += (sender, e) =>
            {
                // logic here Will be called after the child window is closed
                if (((AddTemplate)sender).Canceled == true)
                    return;

                var templatePath = Path.GetFullPath(Path.Combine(project.GetProjectDirectory(), m.Props.Template));  //GetFullpath removes un-needed relative paths  (ie if you are putting something in the solution directory)
                var configPath = Path.GetFullPath(Path.Combine(project.GetProjectDirectory(), "codegeneratorconfig.json"));

                AddFileToProject(project, m, configPath);

                AddFileToProject(project, m, templatePath, true);
            };
            m.ShowModal();
        }

        private void AddFileToProject(Project project, AddTemplate m, string templatePath, bool runCustomTool = false)
        {
            if (File.Exists(templatePath))
            {
                var results = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, "'" + templatePath + "' already exists, are you sure you want to overwrite?", "Overwrite", OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                if (results != 6)
                    return;

                //if the window is open we have to close it before we overwrite it.
                ProjectItem pi = project.GetProjectItem(m.Props.Template);
                pi?.Document?.Close(vsSaveChanges.vsSaveChangesNo);
            }

            var templateSamplesPath = Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates");
            var defaultTemplatePath = Path.Combine(templateSamplesPath, m.DefaultTemplate.SelectedValue.ToString());
            if (!File.Exists(defaultTemplatePath))
            {
                throw new UserException("T4Path: " + defaultTemplatePath + " is missing or you can access it.");
            }

            var dir = Path.GetDirectoryName(templatePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Status.Update("Adding " + templatePath + " to project");
            // When you add a TT file to visual studio, it will try to automatically compile it,
            // if there is error (and there will be error because we have custom generator)
            // the error will persit until you close Visual Studio. The solution is to add
            // a blank file, then overwrite it
            // http://stackoverflow.com/questions/17993874/add-template-file-without-custom-tool-to-project-programmatically
            var blankTemplatePath = Path.Combine(DteHelper.AssemblyDirectory(), @"Resources\Templates\Blank.tt");
            File.Copy(blankTemplatePath, templatePath, true);

            var p = project.ProjectItems.AddFromFile(templatePath);
            p.Properties.SetValue("CustomTool", "");

            File.Copy(defaultTemplatePath, templatePath, true);

            if (runCustomTool)
            {
                p.Properties.SetValue("CustomTool", typeof(CrmCodeGenerator2011).Name);
            }
        }

        #region SolutionEvents

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            _settings.IsActive = false;
            return VSConstants.S_OK;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion SolutionEvents
    }
}