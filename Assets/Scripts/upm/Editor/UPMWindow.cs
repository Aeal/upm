using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
using UnityEditor;
//unity package manager
//manages access to code modules for unity

public class UPMWindow : EditorWindow
{


			
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
