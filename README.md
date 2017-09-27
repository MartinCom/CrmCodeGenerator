# **Dynamics CRM Code Generator for Visual Studio**
Visual Studio extension that will generated the early bound classes for for Microsoft Dynamics CRM.

Fork from https://github.com/xairrick/CrmCodeGenerator

## Latest Release

### Version 2.0

- Support for Visual Studio 2017
- Add Connection String 
- Fix Connect to CRM Online

## Benefits of using this tool over the standard tool

### Control which entities to generate classes for
This will keep the size of the generated code to a minimum. If you use the CrmSvcUtil.exe to generate, the code file will be 200,000 lines. Compared ~1000 lines for each entity you select.

### Built for Visual Studio
You never have to leave Visual Studio to regenerate the early bound classes.   All the configurations* are stored in the .SLN of the solution which allows you save them to Source Control.  This is very helpful if you are working with other developers.  (*username & password are stored in the .SUO file,  which typically isn’t checked into Source Control)

The Developer Toolkit does allow you to stay inside Visual Studio but it wont allow you change connection settings  while in Visual Studio.  You have to exit Visual Studio, delete your .SUO file then restart Visual Studio.

## How to use

1. Install the VS extension.
2. Add a template to your project.
3. Regenerate Code by right click the template and click "Run Custom Tool".



# **Documentation**

## Add a template to your project

1. Highlight the project where you want to store the template and generated code.   
2. Tools –> Add CRM Code Generator Template...  (if you don’t see this menu, then shutdwon visual studio and reinstall the extension)

## Start with one of the provided templates

- **CrmSvcUtil.tt** – Code generated from this will be exactly what is produced from CRmSvcUtil.exe
- **CrmSvcUtilExtended.tt** – Adds fieldname & Option Sets values  (used v2 instead)
- **CrmSvcUtilExtendedV2.tt** – Adds enum properties for all Two Option and Option Set fields (this is the one I use)
- **JavaScript.tt** – Example of how to generate a JavaScript file from the CRM Schema (from https://crm2011codegen.codeplex.com )
- **CSharp.tt** – POCO example of how to put C# data annotation on the fields.  (requires you to put a reference to  System.ComponentModel.DataAnnotations in your project)
- **Sample.tt** – Just another example of how to create a template for C#

## Connection & Entities

1. After a template is added to your project you will be prompted for CRM connection info. Connection info is saved to the .SLN file (username & password are saved to .SUO).
2. Pick the entities that you want to include. (**NOTE**: if you click refresh you must have all the connection information filled out)
3. Click the **“Generate Code”** button (this process takes about 45 seconds, but seems like 5 minutes)
When the dialog goes away, it’s done and the generated code is ready to use.

## Refreshing metadata from the server

If you make schema changes in CRM and you want to refresh the code, right click the template and select **“Run Custom Tool”**

## Changing the template

When you make changes to the template and save, Visual Studio will automatically attempt to re-generate the code. **HINT**: select ‘No’ for fresh entities if you have just made changes in the template and don’t need to refresh from the CRM server, it will be much faster.
