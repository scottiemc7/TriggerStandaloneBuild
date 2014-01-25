using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TriggerStandaloneConsole
{
	class Program
	{
		static readonly string BUILDURL = "https://trigger.io/standalone/package";
		static readonly string MONITORURL = "https://trigger.io/standalone/track/package";

		static void Main(string[] args)
		{
            ProgressBeginBuild();

			//parse and validate our command line args
			var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ProgressEndBuild(false, "Argument failure");
                return;
            }//end if

			if (!options.Android && !options.iOS)
			{
                ProgressEndBuild(false, "Argument failure");
				Console.WriteLine("At least one platform (Android or iOS) needs to be selected");
				return;
			}//end if

			if (!Directory.Exists(options.SrcPath))
			{
                ProgressEndBuild(false, "Argument failure");
				Console.WriteLine("Src path does not exist");
				return;
			}//end if

			if (options.Android && (String.IsNullOrEmpty(options.AndroidKeystorePass) || 
									String.IsNullOrEmpty(options.AndroidKeystorePath) ||
									!File.Exists(options.AndroidKeystorePath) ||
									String.IsNullOrEmpty(options.AndroidKeyAlias) ||
									String.IsNullOrEmpty(options.AndroidKeyPass)))
			{
                ProgressEndBuild(false, "Argument failure");
				Console.WriteLine("At least one Android required argument is missing or file could not be found");
				return;
			}//end if

			if (options.iOS && (String.IsNullOrEmpty(options.iOSCertificatePass) ||
								String.IsNullOrEmpty(options.iOSCertificatePath) ||
								!File.Exists(options.iOSCertificatePath) ||
								String.IsNullOrEmpty(options.iOSProfilePath)))
			{
				Console.WriteLine("At least one iOS required argument is missing or file could not be found");
				return;
			}//end if
			
			BuildPlatform platforms = (options.Android ? BuildPlatform.Android : 0) | (options.iOS ? BuildPlatform.iOS : 0);
			IStandaloneBuilder builder = new StandaloneHttpBuilder(new HttpWebRequestFactory(), BUILDURL, MONITORURL, options.Email, options.Password);
			IZip zipper = new DotNetZipAdapter();

			builder.ProgressEvent += builder_ProgressEvent;

			//load our resources
			PlatformResources resources = new PlatformResources();
			if((platforms & BuildPlatform.Android) == BuildPlatform.Android) 
			{
				resources.AndroidKeyPassword = options.AndroidKeyPass;
				resources.AndroidKeystorePassword = options.AndroidKeystorePass;
				resources.AndroidKeystore = new MemoryStream(File.ReadAllBytes(options.AndroidKeystorePath));
				resources.AndroidKeystoreFileName = Path.GetFileName(options.AndroidKeystorePath);
				resources.AndroidKeyAlias = options.AndroidKeyAlias;
			}//end if

			if ((platforms & BuildPlatform.iOS) == BuildPlatform.iOS)
			{
				resources.iOSCertificate = new MemoryStream(File.ReadAllBytes(options.iOSCertificatePath));
				resources.iOSCertificatePassword = options.iOSCertificatePass;
				resources.iOSCertificateFileName = Path.GetFileName(options.iOSCertificatePath);
				resources.iOSProfile = new MemoryStream(File.ReadAllBytes(options.iOSProfilePath));
				resources.iOSProfileFileName = Path.GetFileName(options.iOSProfilePath);
			}//end if

			//use multiple sources if we are building for more than one platform
			//AND there are excluded dirs for either platform
			Dictionary<BuildPlatform, MemoryStream> successfulBuilds = null;
			if ((platforms & (BuildPlatform.Android | BuildPlatform.iOS)) == (BuildPlatform.Android | BuildPlatform.iOS) &&
				(!String.IsNullOrEmpty(options.iOSIgnore) || !String.IsNullOrEmpty(options.AndroidIgnore)))
			{
				//use multiple sources (multiple builds)
				using(MemoryStream andZip = zipper.ZipDirectory(options.SrcPath, options.AndroidIgnore))
				using(MemoryStream iosZip = zipper.ZipDirectory(options.SrcPath, options.iOSIgnore))
				{
					successfulBuilds = new Dictionary<BuildPlatform, MemoryStream>();
					try
					{						
						successfulBuilds.Add(BuildPlatform.Android, builder.BuildForPlatforms(BuildPlatform.Android, andZip, resources)[BuildPlatform.Android]);
						successfulBuilds.Add(BuildPlatform.iOS, builder.BuildForPlatforms(BuildPlatform.iOS, iosZip, resources)[BuildPlatform.iOS]);
					}
					catch (BuildFailureException e)
					{
						ProgressEndBuild(false, e.Message);
						return;
					}//end try
				}//end using
			}
			else
			{
				//use one source, single build
				using (MemoryStream zip = zipper.ZipDirectory(options.SrcPath, null))
				{
					try
					{
						successfulBuilds = builder.BuildForPlatforms(platforms, zip, resources);
					}
					catch (BuildFailureException e)
					{
						ProgressEndBuild(false, e.Message);
						return;
					}
				}
			}//end if

			try
			{
				if (successfulBuilds.ContainsKey(BuildPlatform.Android))
				{
					byte[] buff = new byte[successfulBuilds[BuildPlatform.Android].Length];
					successfulBuilds[BuildPlatform.Android].Read(buff, 0, buff.Length);
					File.WriteAllBytes(String.Format("{0}\\{1}.apk", options.DownloadPath, String.IsNullOrEmpty(options.AndroidPackageName) ? "android" : options.AndroidPackageName), buff);
				}

				if (successfulBuilds.ContainsKey(BuildPlatform.iOS))
				{
					byte[] buff = new byte[successfulBuilds[BuildPlatform.iOS].Length];
					successfulBuilds[BuildPlatform.iOS].Read(buff, 0, buff.Length);
                    File.WriteAllBytes(String.Format("{0}\\{1}.ipa", options.DownloadPath, String.IsNullOrEmpty(options.iOSPackageName) ? "ios" : options.iOSPackageName), buff);
				}
			}
			catch (Exception e)
			{
				ProgressEndBuild(false, e.Message);
			}//end try

			ProgressEndBuild(true, "Build complete");
		}

		private static void ProgressBeginBuild()
		{
			Console.WriteLine("##teamcity[progressStart '']");
		}

		private static void ProgressEndBuild(bool success, string msg)
		{
			Console.WriteLine(String.Format("##teamcity[progressFinish 'Build Finished With {0}']", success ? "SUCCESS" : "FAILURE"));
			Console.WriteLine(String.Format("##teamcity[buildStatus status='{1}' text='{0}']", msg, success ? "SUCCESS" : "FAILURE"));
		}

		private static void ProgressMessage(string msg)
		{
			Console.WriteLine(String.Format("##teamcity[progressMessage '{0}']", msg));
		}

		static void builder_ProgressEvent(string message)
		{
			ProgressMessage(message);
			/*
			switch (status)
			{
				case ProgressStatus.FAIL:
					
					Console.WriteLine("##teamcity[progressFinish 'Build Finished With Failure']");
					break;
				case ProgressStatus.SUCCESS:
					Console.WriteLine(String.Format("##teamcity[buildStatus status='SUCCESS' text='{0}']", message));
					Console.WriteLine("##teamcity[progressFinish 'Build Finished Successfully']");
					break;
				case ProgressStatus.BEGIN:
					Console.WriteLine(String.Format("##teamcity[progressStart '{0}']", message));
					break;
				case ProgressStatus.INPROGRESS:
				default:
					
					break;
			}//end switch*/
		}
	}
}
