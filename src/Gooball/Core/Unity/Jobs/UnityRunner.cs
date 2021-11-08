﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gooball
{

	internal class UnityRunner
	{
		public readonly UnityArgs Args;

		public UnityRunner(UnityArgs args)
		{
			Args = args;
		}

		public int Open(Project project)
		{
			var helper = new UnityInstallationHelper();
			var args = new Queue<string>(Args);
			if (Args.ProjectPath is null)
			{
				args.Enqueue($"-projectPath");
				args.Enqueue($"{project.Path}");
			}

			var editorPath = helper.GetBestEditor(project.EditorVersion);

			return Execute(editorPath, args);
		}

		public int Build(Project project)
		{
			var helper = new UnityInstallationHelper();
			var args = GetDefaultArgsBatch(project);

			var editorPath = helper.GetBestEditor(project.EditorVersion);

			return Execute(editorPath, args);
		}

		public int Test(Project project)
		{
			var helper = new UnityInstallationHelper();
			var args = GetDefaultArgsBatch(project, quit: false);
			if (!Args.RunTests)
			{
				args.Enqueue($"-runTests");
			}

			var editorPath = helper.GetBestEditor(project.EditorVersion);

			return Execute(editorPath, args);
		}

		public int Run()
		{
			var helper = new UnityInstallationHelper();
			var args = Args;

			var editorPath = UnityInstallationHelper.GetExecutablePath(helper.GetInstalledEditors().First());

			return Execute(editorPath, args);
		}

		private Queue<string> GetDefaultArgsBatch(Project project, bool batchMode = true, bool quit = true)
		{
			var args = new Queue<string>();
			if (!Args.BatchMode && batchMode)
			{
				args.Enqueue($"-batchMode");
			}
			if (!Args.Quit && quit)
			{
				args.Enqueue($"-quit");
			}
			if (Args.ProjectPath is null)
			{
				args.Enqueue($"-projectPath");
				args.Enqueue($"{project.Path}");
			}
			foreach (var argument in Args)
			{
				args.Enqueue(argument);
			}

			return args;
		}

		private int Execute(string editorPath, IEnumerable<string> args)
		{
			var argsString = string.Join(" ", args.Select(WrapArgument));

			using (var process = new Process())
			{
				process.StartInfo.FileName = editorPath;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.Arguments = argsString;
				process.Start();

				process.WaitForExit();

				return process.ExitCode;
			}

			string WrapArgument(string arg)
			{
				return $"\"{arg}\"";
			}
		}
	}
}
