using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
//using JsonFx.Json;
using LitJson;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml.Serialization;
//unity package manager
//manages access to code modules for unity

public class UPMWindow : EditorWindow
{
	public static string UPM_REPOS_LOCAL_FILEPATH
	{
		get { return Application.dataPath + "/upm.repos"; }
	}

	public static string UPM_REPOS_REMOTE
	{
		get { return "http://github.com/Aeal/upm/upm.repos"; }
	}

	[MenuItem("Unity Package Manager/Get New Packages")]
	private static void OpenPackageManager()
	{
		
	}
	//updates the UPM_REPO
	[MenuItem("Unity Package Manager/Update sources")]
	private static void CheckForUpdates()
	{
		//TODO get the master repository list 
	}

	public void SaveRepositoriesList()
	{
		var repo = new Repository
		{
			PackageName = @"Test Package",
			PackageURL = @"http://github.com/Test_Package",
			PackageMaintainer = @"Tyler Steele",
			PackageVersion = @"1.0"
		};
		var list = new List<Repository> { repo };
		string repoMap = JsonMapper.ToJson(list);

		using(TextWriter writer = new StreamWriter(UPM_REPOS_LOCAL_FILEPATH))
		{
			writer.Write(repoMap);
		}
	}

	public List<Repository>	LoadRepositories()
	{
		using(StreamReader reader = new StreamReader(UPM_REPOS_LOCAL_FILEPATH))
		{
			string repoList = reader.ReadToEnd();
			Debug.Log(repoList);
			return JsonMapper.ToObject<List<Repository>>(repoList);

		}

	}
	[MenuItem("Unity Package Manager/Update Repos")]
	 private static void GetRepos()
	 {
		
	 }

			
}

public class Repository
{
	public string  PackageName,
	               PackageURL,
	               PackageVersion,
				   PackageMaintainer;
}

public class UPM
{

	//updates the repository listings
	public void UpdateRepositoryListing()
	{
	}
	//installs a package with the specified package name
	public bool InstallPackage(string packageName)
	{
		return true;
	}

	//updates all packages to the latest version
	public bool UpdatePackages()
	{
		return true;

	}

	//updates a specific package to the latest version
	public bool UpdatePackage(string packageName)
	{
		return true;

	}
}
