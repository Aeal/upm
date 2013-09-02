using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using LitJson;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

//unity package manager
//manages access to code modules for unity
//TODO: Allow for addition of private repositories
//
public static class GitCommand
{
	public static bool IsGitInstalled
	{
		get { var syspath = System.Environment.GetEnvironmentVariable("PATH");
			var searchDirs = syspath.Split(';');
			foreach (string dir in searchDirs)
			{
				var gitPath = dir + @"\git.exe";
				if(File.Exists(gitPath))
				{
					return true;
				}

			}
			return false;
		}
	}
	public static void Execute(string command, EventHandler endedHandler = null)
	{
		if(!IsGitInstalled)
		{
			Debug.LogError("GIT is not installed on this system, Please install git");
			return;
		}
		Process process = new System.Diagnostics.Process();
		ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.CreateNoWindow = true; 
		startInfo.FileName = "git";
		startInfo.Arguments = command;
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		startInfo.UseShellExecute = false;
		process.OutputDataReceived += (sender, args) => { UPMConsole.Log(args.Data); };
		process.ErrorDataReceived += (sender, args) => { UPMConsole.Log(args.Data); };
		if(endedHandler != null) process.Exited += endedHandler;		
		process.StartInfo = startInfo;
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
	}
}



//Database of all sources
//keeps track available sources
//has collections and a list of individual repositories
public class UPMSource
{
	private List<Repository> repos = new List<Repository>();
	public List<Repository> Repositories = new List<Repository>();
	private List<RepositoryCollection> collections = new List<RepositoryCollection>();

	
//	public List<Repository> Repositories
//	{
//		get { return combinedRepos; }
//	}
	public void UpdateSources()
	{
		Repositories.Clear();

		foreach (var collection in collections)
		{
			collection.Update();
			foreach (var repo in collection.repositories)
			{
				Repositories.Add(repo);
			}
		}
		foreach (Repository repository in repos)
		{
			repository.UpdateSource();
			Repositories.Add(repository);
		}
	}


	internal void AddRepo(Repository repo)
	{
		repos.Add(repo);
		//throw new NotImplementedException();
	}

	internal void AddCollection(RepositoryCollection collection)
	{
		collections.Add(collection);

		//throw new NotImplementedException();
	}
}
//defines a class that handles colections of repositories
public  class RepositoryCollection
{
	public string GET_URL;
	public virtual void Update()
	{
	}
	public List<Repository> repositories = new List<Repository>();
	
}

public class GitHubRepositoryCollection : RepositoryCollection
{
	public override void Update()
	{

		ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
		WebRequest request = WebRequest.Create(new Uri(GET_URL));
		request.UseDefaultCredentials = true;
		WebResponse res = request.GetResponse();
		repositories.Clear();
		using (Stream stream = res.GetResponseStream())
		{
			StreamReader reader = new StreamReader(stream, Encoding.UTF8);
			var collectionData = reader.ReadToEnd();
			JsonData collection = JsonMapper.ToObject(collectionData);
			for (int i = 0; i < collection.Count; i++)
			{
				Debug.Log(collection[i]["name"]);
				var repository = new Repository()
										{
											PackageName = collection[i]["name"].ToString(),
											PackageURL = collection[i]["clone_url"].ToString()	,
											PackageMaintainer = collection[i]["owner"]["login"].ToString(),
											useCredentials = bool.Parse(collection[i]["private"].ToString())
										};
				repositories.Add(repository);
			}
		}

		//repositories.Add(UPM.LoadRepository(Application.dataPath + @"\tmp.col"));
		base.Update();
	}
}
public static class UPMConsole
{
	public static string ConsoleBuffer { get { return consoleBuffer; } }
	private static string consoleBuffer = "";

	public static void Log(object message)
	{
		consoleBuffer += "\n" + message.ToString();
	}
}
public class UPMWindow : EditorWindow	
{

	public const string UPM_DEFAULT_MENU = "Unity Package Manager/";

	private static UPMWindow window;

	[MenuItem(UPM_DEFAULT_MENU+"Get New Packages")]
	private static void OpenPackageManager()
	{
		if(!window)
		{
			window = Editor.CreateInstance<UPMWindow>();
		}
		UPM.LoadRemoteRepos();
		if(!GitCommand.IsGitInstalled)
		{
			EditorUtility.DisplayDialog("Error",
			                            "Git not installed UPM will not work until you have installed Git on your computer.",
			                            "Ok");
		}
		else
		{
			window.Show();
			
		}
	}

	private Vector3 scrollPosition;
	private void OnGUI()
	{
		if(UPM.RemoteSources.Repositories.Count == 0)
		{
			UPM.LoadRemoteRepos();
		}
		DrawMainMenu();
		DrawRepositorySelector();
		DrawOutputConsole();
	}

	private void DrawOutputConsole()
	{
		GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true),GUILayout.MinHeight(400f));
		GUILayout.TextArea(UPMConsole.ConsoleBuffer,GUILayout.ExpandHeight(true));
		GUILayout.EndHorizontal();

		//throw new NotImplementedException();
	}

	private void DrawMainMenu()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Button("Repository List");
		GUILayout.Button("Settings");
		GUILayout.EndHorizontal();
	}
	private void DrawRepositorySelector()
	{
		GUILayout.BeginHorizontal();
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		foreach(var repo in UPM.RemoteSources.Repositories)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(repo.PackageName, GUILayout.Width(200));
			GUILayout.Button("Package Description");
			if(GUILayout.Button("Install Package"))
			{
				UPM.PullPackage(repo);
			}

			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();
	}

	[MenuItem("Unity Package Manager/Update Repos")]
	private static void UpdateRemoteRepos()
	{
		UPM.CheckForUpdates();
	}
	[MenuItem(UPM_DEFAULT_MENU + "Create dummy repos")]
	private  static void createDummyRepos()
	{
		UPM.SaveRepositoriesList(UPM.UPM_MASTER_REPOS_LOCAL);
	}

	

}

public class Repository
{
	public string  PackageName,
	               PackageURL,
				   PackageMaintainer,
				   PackageDescription,
				   InfoUrl;

	public int PackageVersion;
	public bool useCredentials = false;
	public string Username = "", 
				  Password = "";
	public void UpdateSource()
	{
		
	}

	public void UpdateInfo()
	{
		
	}
}

public class UPM
{
	public static UPMSource remoteSources = new UPMSource();
	public static UPMSource localSources = new UPMSource();
	public const string UPM_DEFAULT_MENU = "Unity Package Manager/";

	public static void LoadRemoteRepos()
	{
		remoteSources = LoadSources(UPM_MASTER_REPOS_LOCAL);
		Debug.Log("Loaded sources: " + remoteSources.Repositories[0].PackageName);
	}
	public static string UPM_MASTER_REPOS_LOCAL
	{
		get { return Application.dataPath + "/upm.repos"; }
	}

	public static string MODULES_PATH
	{
		get { return Application.dataPath + "/upm_modules/"; }
	}
	//path to file containing the local modules
	public static string UPM_LOCAL_MODULES_PATH
	{
		get { return Application.dataPath + "/upm.modules"; }
	}

	public static string UPM_MASTER_REPOS_REMOTE
	{
		get { return "http://github.com/Aeal/upm/raw/master/upm.repos"; }
	}

//	private static List<Repository> remote_repositories = new List<Repository>();
//	private static List<Repository> local_repositories = new List<Repository>();

	public static Repository LoadRepository(string path)
	{
		using(StreamReader reader = new StreamReader(path))
		{
			string repoList = reader.ReadToEnd();
			return JsonMapper.ToObject<Repository>(repoList);
		}
	}

	
	public static UPMSource LoadSources(string path)
	{
		using(StreamReader reader = new StreamReader(path))
		{
			string repoList = reader.ReadToEnd();
			Debug.Log(repoList);
			var source = JsonMapper.ToObject<UPMSource>(repoList);
 			Debug.Log(source);
 			Debug.Log(source.Repositories.Count);
			return source;

		}

	}

	//updates the master repository list
	public static void CheckForUpdates()
	{
		
		localSources.UpdateSources();
		remoteSources.UpdateSources();
//		ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
//		WebClient client = new WebClient();
//		client.DownloadFileCompleted += ClientOnDownloadFileCompleted;
//		client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
//		client.DownloadFileAsync(new Uri(UPM_MASTER_REPOS_REMOTE), UPM_MASTER_REPOS_LOCAL);
//		remote_repositories = LoadRemoteRepos();
		
	}

	public static void PullPackage(Repository repoToPull)
	{
		GitCommand.Execute("clone " + repoToPull.PackageURL + " " + MODULES_PATH + repoToPull.PackageName,(e,args)=>
			                                                                                                  {
				                                                                                                  UPMConsole.Log(
					                                                                                                  "Package downloaded");
				                                                                                                  AssetDatabase.Refresh();
			                                                                                                  });
	}

//	private static List<Repository> LoadRemoteRepos()
//	{
//		using(StreamReader reader = new StreamReader(UPM_MASTER_REPOS_LOCAL))
//		{
//			string repoList = reader.ReadToEnd();
//			Debug.Log(repoList);
//			return JsonMapper.ToObject<List<Repository>>(repoList);
//
//		}
//	}

	#region Download handlers

	private static void ClientOnDownloadProgressChanged(object sender,DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
	{
		Debug.Log("File download progress: " +(downloadProgressChangedEventArgs.BytesReceived / downloadProgressChangedEventArgs.TotalBytesToReceive));
	}

	private static void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
	{
		Debug.Log("Updated List successfully");
	}

	#endregion
	public static void SaveRepositoriesList(string toPath)
	{
		var repo = new Repository
		{
			PackageName = "SSE",
			PackageURL = @"https://github.com/Aeal/SSE.git",
			PackageMaintainer = @"Aeal",
			PackageVersion = 1,
			InfoUrl = @"https://raw.github.com/Aeal/SSE/info.ups"
		};
		var source = new UPMSource();
		source.AddRepo(repo);
		var collections = new GitHubRepositoryCollection();
		collections.GET_URL = "https://api.github.com/orgs/UPMCollection/repos";
		source.AddCollection(collections);
		source.UpdateSources();
		var builder = new StringBuilder();
		var jsonSettings = new JsonWriter(builder);
		jsonSettings.PrettyPrint = true;
		JsonMapper.ToJson(source,jsonSettings);
										
		using(TextWriter writer = new StreamWriter(toPath))
		{
			writer.Write(builder);
			 
		}

		var rbuilder = new StringBuilder();
		var settings = new JsonWriter(rbuilder);
		settings.PrettyPrint = true;
		JsonMapper.ToJson(repo, settings);
		using(TextWriter twriter = new StreamWriter(toPath+".r"))
		{
			twriter.Write(rbuilder);
			 
		}
	}


	public static UPMSource RemoteSources { get { return remoteSources; } }

	
}
