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
			//parse and validate our command line args
			var options = new Options();
			if (!CommandLine.Parser.Default.ParseArguments(args, options))
				return;

			if (!options.Android && !options.iOS)
			{
				Console.WriteLine("At least one platform (Android or iOS) needs to be selected");
				return;
			}//end if

			if (!Directory.Exists(options.SrcPath))
			{
				Console.WriteLine("Src path does not exist");
				return;
			}//end if

			if (options.Android && (String.IsNullOrEmpty(options.AndroidKeystorePass) || 
									String.IsNullOrEmpty(options.AndroidKeystorePath) ||
									!File.Exists(options.AndroidKeystorePath) ||
									String.IsNullOrEmpty(options.AndroidKeyAlias) ||
									String.IsNullOrEmpty(options.AndroidKeyPass)))
			{
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
			ProgressBeginBuild();
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
					File.WriteAllBytes(String.Format("{0}\\android.apk", options.DownloadPath), buff);
				}

				if (successfulBuilds.ContainsKey(BuildPlatform.iOS))
				{
					byte[] buff = new byte[successfulBuilds[BuildPlatform.iOS].Length];
					successfulBuilds[BuildPlatform.iOS].Read(buff, 0, buff.Length);
					File.WriteAllBytes(String.Format("{0}\\ios.ipa", options.DownloadPath), buff);
				}
			}
			catch (Exception e)
			{
				ProgressEndBuild(false, e.Message);
			}//end try

			ProgressEndBuild(true, "Build complete");

			/*
			const string boundary = "---------------------------AaB03x";
			

			string email = options.Email;
			string password = options.Password;
			string DLDIR = options.DownloadPath;

			Dictionary<String, String> nameValues = new Dictionary<string, string>();
			Dictionary<String, String> namePaths = new Dictionary<string, string>();

			nameValues.Add("email", email);
			nameValues.Add("password", password);

			//Android
			if (options.Android)
			{
				namePaths.Add("and_keystore", options.AndroidKeystorePath);
				nameValues.Add("and_keypass", options.AndroidKeyPass);
				nameValues.Add("and_storepass", options.AndroidKeystorePass);
				nameValues.Add("and_keyalias", options.AndroidKeyAlias);
			}

			//iOS
			if (options.iOS)
			{
				namePaths.Add("ios_certificate", options.iOSCertificatePath);
				namePaths.Add("ios_profile", options.iOSProfilePath);
				nameValues.Add("ios_password", options.iOSCertificatePass);
			}

			//zip our src directory
			String srcZipPath = Path.Combine(options.DownloadPath, "src.zip");
			if (File.Exists(srcZipPath))
				File.Delete(srcZipPath);

			using (ZipFile zip = new ZipFile())
			{
				zip.AddDirectory(options.SrcPath);
				zip.Save(srcZipPath);
			}

			namePaths.Add("src_zip", srcZipPath);

			//create our web request
			HttpWebRequest req = (HttpWebRequest)WebRequest.CreateHttp(url);
			req.Timeout = 600000;//10 minutes, slow connection maybe
			req.Method = "POST";

			//headers
			req.Accept = "application/json";
			req.ContentType = String.Format("multipart/form-data; boundary={0}", boundary);

			//body
			StringBuilder body = new StringBuilder();
			using (System.IO.Stream s = req.GetRequestStream())
			{
				//add our form text values
				foreach (String name in nameValues.Keys)
				{
					String line = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", boundary, name, nameValues[name]);

					byte[] buff = Encoding.UTF8.GetBytes(line);
					s.Write(buff, 0, buff.Length);

					//append for debugging
					body.Append(line);
				}//end foreach

				//add our form files
				foreach (String name in namePaths.Keys)
				{
					String line1 = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\";filename=\"{2}\"\r\nContent-Type: application/base64\r\n\r\n", boundary, name, Path.GetFileName(namePaths[name]));

					//append for debugging
					body.Append(line1 + "[BinaryContentRemoved]\r\n");

					//stream the description
					byte[] buff = Encoding.UTF8.GetBytes(line1);
					s.Write(buff, 0, buff.Length);
					//stream the file
					buff = File.ReadAllBytes(namePaths[name]);
					s.Write(buff, 0, buff.Length);
					//stream end
					buff = Encoding.UTF8.GetBytes(Environment.NewLine);
					s.Write(buff, 0, buff.Length);
				}//end foreach

				//end our form data
				String endLine = String.Format("--{0}--", boundary);
				byte[] endBuff = Encoding.UTF8.GetBytes(endLine);
				s.Write(endBuff, 0, endBuff.Length);

				body.Append(endLine);
			}//end using

			Console.WriteLine("##teamcity[progressMessage 'Uploading package...']");
			Console.WriteLine(body.ToString());

			bool success = false;
			using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
			{
				if (resp.StatusCode == HttpStatusCode.OK)
				{
					//might be OK, might be missing some stuff, not sure yet so we need to inspect
					//GOOD response should be in the form of:
					//{"id": "SOME GUID HERE", "result": "ok"}
					//BAD response will be:
					//{"errors" { JSON ARRAY OF ERRORS } }

					StringBuilder sb = new StringBuilder();
					using (StreamReader r = new StreamReader(resp.GetResponseStream()))
					{
						while (!r.EndOfStream)
							sb.Append(r.ReadLine());
					}//end using
					String response = sb.ToString();

					JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
					var pushResult = jsSerializer.DeserializeObject(response) as Dictionary<string, object>;
					if (pushResult.ContainsKey("result") && String.Compare(pushResult["result"].ToString(), "ok", true) == 0)
					{
						String id = pushResult["id"].ToString();
						Console.WriteLine(String.Format("Response Success, ID is: {0}", id));
						Console.WriteLine("##teamcity[progressMessage 'Monitoring Build...']");

						//monitor the build by periodically asking if the build is complete, using id
						String askURL = String.Format("https://trigger.io/standalone/track/package/{0}?email={1}&password={2}", id, email, password);
						while (true)
						{
							HttpWebRequest askReq = WebRequest.CreateHttp(askURL);
							askReq.Method = "GET";
							using (HttpWebResponse askResponse = (HttpWebResponse)askReq.GetResponse())
							{
								if (askResponse.StatusCode != HttpStatusCode.OK)
								{
									break;
								}//end if

								sb = new StringBuilder();
								using (StreamReader r = new StreamReader(askResponse.GetResponseStream()))
								{
									while (!r.EndOfStream)
										sb.Append(r.ReadLine());
								}//end using

								String askResponseBody = sb.ToString();
								var monitorResult = jsSerializer.DeserializeObject(askResponseBody) as Dictionary<string, object>;
								String state = monitorResult.ContainsKey("state") ? monitorResult["state"].ToString() : null;
								if (String.Compare(state, "success", true) == 0)
								{
									Console.WriteLine("##teamcity[progressMessage 'Build Success. Downloading...']");
									Console.WriteLine("Response Is SUCCESS");
									Console.WriteLine(askResponseBody);
									bool androidSuccess = false, iosSuccess = false;

									if (options.Android)
									{
										//get our Android package url
										//should be in form {"info": {"files": {"android": "ADDRESS"} ...
										var info = monitorResult["info"] as Dictionary<string, object>;
										var files = info["files"] as Dictionary<string, object>;
										String dlURL = files["android"].ToString();
										Console.WriteLine(String.Format("Android Download URL: {0}", dlURL));
										Console.WriteLine("Downloading Android APK...");

										//download our Android APK
										HttpWebRequest dlReq = WebRequest.CreateHttp(dlURL);
										dlReq.Timeout = 600000;
										using (HttpWebResponse dlResp = (HttpWebResponse)dlReq.GetResponse())
										{
											//stream to dl dir
											using (Stream r = dlResp.GetResponseStream())
											using (FileStream fs = new FileStream(String.Format(@"{0}\android.apk", DLDIR), FileMode.Create))
											{
												byte[] buff = new byte[2048];
												int bytesRead = 0, totalBytesRead = 0;
												while ((bytesRead = r.Read(buff, 0, buff.Length)) > 0)
												{
													fs.Write(buff, 0, bytesRead);
													totalBytesRead += bytesRead;
												}//end while

												androidSuccess = true;
												Console.WriteLine(String.Format("##teamcity[progressMessage 'Android Download Complete. {0} bytes total']", totalBytesRead));
											}//end using
										}//end using
									}
									else
										androidSuccess = true;

									if (options.iOS)
									{
										//get our iOS package url
										var info = monitorResult["info"] as Dictionary<string, object>;
										var files = info["files"] as Dictionary<string, object>;
										String dlURL = files["ios"].ToString();
										Console.WriteLine(String.Format("iOS Download URL: {0}", dlURL));
										Console.WriteLine("Downloading iOS IPA...");

										//download our Android APK
										HttpWebRequest dlReq = WebRequest.CreateHttp(dlURL);
										dlReq.Timeout = 600000;
										using (HttpWebResponse dlResp = (HttpWebResponse)dlReq.GetResponse())
										{
											//stream to dl dir
											using (Stream r = dlResp.GetResponseStream())
											using (FileStream fs = new FileStream(String.Format(@"{0}\ios.ipa", DLDIR), FileMode.Create))
											{
												byte[] buff = new byte[2048];
												int bytesRead = 0, totalBytesRead = 0;
												while ((bytesRead = r.Read(buff, 0, buff.Length)) > 0)
												{
													fs.Write(buff, 0, bytesRead);
													totalBytesRead += bytesRead;
												}//end while

												iosSuccess = true;
												Console.WriteLine(String.Format("##teamcity[progressMessage 'iOS Download Complete. {0} bytes total']", totalBytesRead));
											}//end using
										}//end using
									}
									else
										iosSuccess = true;

									success = iosSuccess && androidSuccess;
									break;
								}
								else if (String.Compare(state, "failure", true) == 0)
								{
									Console.WriteLine("##teamcity[progressMessage 'Remote Build FAILURE']");
									Console.WriteLine(askResponseBody);
									break;
								}
								else if (String.Compare(state, "pending", true) != 0) //unknown status
								{
									Console.WriteLine("##teamcity[progressMessage 'Remote Build FAILURE - unknown status']");
									Console.WriteLine(String.Format("Response Is: {0}", askResponseBody));
									break;
								}//end if
							}//end using
						}//end while
					}
					else
						Console.WriteLine(String.Format("Response Error: {0}", response));
				}
				else
					Console.WriteLine(String.Format("BAD Response. Status Code: {0}", resp.StatusCode));
			}//end using

			if (!success)
				Console.WriteLine("##teamcity[buildStatus status='FAILURE' text='{build.status.text}']");
			Console.WriteLine("##teamcity[progressFinish 'End Build']"); */

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
