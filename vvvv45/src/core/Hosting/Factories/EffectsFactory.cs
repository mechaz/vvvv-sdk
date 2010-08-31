﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.CodeDom.Compiler;
using System.IO;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	/// <summary>
	/// Effects factory, parses and watches the effect directory
	/// </summary>
	[Export(typeof(IAddonFactory))]
	public class EffectsFactory : AbstractFileFactory<IEffectHost>
	{
		[Import]
		protected ISolution FSolution;
		
		[Import]
		protected ILogger Logger { get; set; }
		
		private Dictionary<string, FXProject> FProjects;
		private Dictionary<FXProject, INodeInfo> FProjectNodeInfo;
		
		public EffectsFactory()
			: base(Shell.CallerPath.ConcatPath(@"..\..\effects"), ".fx")
		{
			FProjects = new Dictionary<string, FXProject>();
			FProjectNodeInfo = new Dictionary<FXProject, INodeInfo>();
		}

		//create a node info from a filename
		protected override IEnumerable<INodeInfo> GetNodeInfos(string filename)
		{
			FXProject project;
			
			if (!FProjects.TryGetValue(filename, out project))
			{
				project = new FXProject(new Uri(filename));
				if (FSolution.Projects.CanAdd(project))
					FSolution.Projects.Add(project);
				project.Load();
				//project.CompileCompleted += new CompileCompletedHandler(project_CompileCompleted);
				project.DoCompile += new EventHandler(project_DoCompile);
				FProjects[filename] = project;
			}
			
			project.Compile();
			
			yield return FProjectNodeInfo[project];
		}
		
		void project_DoCompile(object sender, EventArgs e)
		{
			//parse nodeinfo of project
			var nodeInfo = new NodeInfo();
			var project = (sender as FXProject);
			string filename = project.Location.LocalPath;
			nodeInfo.Name = Path.GetFileNameWithoutExtension(filename);
			nodeInfo.Category = "EX9.Effect";
			nodeInfo.Filename = filename;
			nodeInfo.Type = NodeType.Effect;
//			nodeInfo.Executable = project.Exectuable;
			
			var includes = new List<string>();
			
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(filename))
				{
					string line;
					string author = @"//@author:";
					string desc = @"//@help:";
					string tags = @"//@tags:";
					string credits = @"//@credits:";
					string include = @"#include ";
					
					// Parse lines from the file until the end of
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						if (line.StartsWith(author))
							nodeInfo.Author = line.Replace(author, "").Trim();
						
						else if (line.StartsWith(desc))
							nodeInfo.Help = line.Replace(desc, "").Trim();
						
						else if (line.StartsWith(tags))
							nodeInfo.Tags = line.Replace(tags, "").Trim();
						
						else if (line.StartsWith(credits))
							nodeInfo.Credits = line.Replace(credits, "").Trim();
						
						else if (line.StartsWith(include))
						{
							var inc = line.Replace(include, "").Trim();
							inc = Path.Combine(Path.GetDirectoryName(filename), inc.Trim(new char[1]{'"'}));
							includes.Add(inc);
						}
					}
				}
				
				//remove all references that are not in the includes
				for (int i = project.Documents.Count - 1; i >= 0; i--)
				{
					var r = includes.Find(delegate(string includePath) {return includePath == project.Documents[i].Location.LocalPath;});
					if ((r == null) && (!(project.Documents[i] is FXDocument)))
						project.Documents.Remove(project.Documents[i]);
				}
				
				//add all includes that are not yet in the references
				foreach(var include in includes)
				{
					var name = Path.GetFileName(include);
					if (!project.Documents.Contains(name))
					{
						if (File.Exists(include))
						{
							var doc = new FXHDocument(new Uri(include));
							doc.Load();
							project.Documents.Add(doc);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log(LogType.Error, "Could not extract effect info");
				Logger.Log(ex);
			}

			//for being available in the ExtractNodeInfo call
			if (!FProjectNodeInfo.ContainsKey(project))
				FProjectNodeInfo.Add(project, nodeInfo);
			
			//re-register nodeinfo with vvvv
			OnNodeInfoAdded(nodeInfo);
		}
		
		protected override bool CreateNode(INodeInfo nodeInfo, IEffectHost effectHost)
		{
			if (nodeInfo.Type != NodeType.Effect)
				return false;
			
			var project = FProjects[nodeInfo.Filename];
			
			//get the code of the FXProject associated with the nodeinfos filename
			effectHost.SetEffect(nodeInfo.Filename, project.Code);

			//now the effect is compiled in vvvv and we can access the errors
			string e = effectHost.GetErrors();
			if (string.IsNullOrEmpty(e))
				e = "";
			project.Errors = e;
			
			//and the input pins
			string f = effectHost.GetParameterDescription();
			if (string.IsNullOrEmpty(f))
				f = "";
			project.ParameterDescription = f;

			return true;
		}
		
		protected override bool DeleteNode(IEffectHost host)
		{
			return true;
		}
		
		protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			if (nodeInfo.Type == NodeType.Effect)
			{
				var project = FProjects[nodeInfo.Filename];
				var newProject = project.Clone() as FXProject;
				
				var projectDir = project.Location.GetLocalDir();
				var newProjectName = name + ".fx";
				var newLocation = new Uri(projectDir.ConcatPath(newProjectName));
				newProject.Location = newLocation;
				
				var newLocationDir = newLocation.GetLocalDir();
				foreach (var doc in newProject.Documents)
				{
					// The documents are cloned but their location still refers to the old one.
					// Only clone FX files, not FXH.
					// FXProjects are saved as one FXDocument.
					if (doc is FXDocument)
					{
						doc.Location = newLocation;
						doc.Save();
						break;
					}
				}
				
				// Save the project.
				newProject.Save();
				
				return true;
			}
			
			return base.CloneNode(nodeInfo, path, name, category, version);
		}
	}
}
