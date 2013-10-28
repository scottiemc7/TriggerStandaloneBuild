using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TriggerStandaloneConsole
{
	public class StandaloneHttpBuilder : IStandaloneBuilder
	{
		private const string BOUNDARY = "---------------------------AaB03x";
		private readonly HttpWebRequestFactory _requestFactory;
		private readonly string _serviceURL;
		private readonly string _monitorURL;
		private readonly string _email;
		private readonly string _password;

		public StandaloneHttpBuilder(HttpWebRequestFactory factory, string serviceURL, string monitorURL, string email, string password)
		{
			_serviceURL = serviceURL;
			_requestFactory = factory;
			_email = email;
			_password = password;
			_monitorURL = monitorURL;
		}

		public event ProgressEventHandler ProgressEvent;
		private void FireProgressEvent(string msg)
		{
			if (ProgressEvent != null)
				ProgressEvent(msg);
		}

		public Dictionary<BuildPlatform, System.IO.MemoryStream> BuildForPlatforms(BuildPlatform platforms, MemoryStream source, PlatformResources resources)
		{
			String platformString = "";
			if ((platforms & (BuildPlatform.Android | BuildPlatform.iOS)) == (BuildPlatform.Android | BuildPlatform.iOS))
				//Trace.TraceInformation("Building for Android and iOS");
				//FireProgressEvent("Begin build iOS & Android", ProgressStatus.BEGIN);
				platformString = "Android & iOS";
			else if ((platforms & BuildPlatform.Android) == BuildPlatform.Android)
				//Trace.TraceInformation("Building for Android");
				//FireProgressEvent("Begin build Android", ProgressStatus.BEGIN);
				platformString = "Android";
			else if ((platforms & BuildPlatform.iOS) == BuildPlatform.iOS)
				//Trace.TraceInformation("Building for iOS");
				//FireProgressEvent("Begin build iOS", ProgressStatus.BEGIN);
				platformString = "iOS";
			else
				throw new ArgumentException("unknown platform");

			//create our POST request
			IHttpWebRequest request = CreateRequest(platforms, source, resources);
			request.Timeout = 600000;
			JavaScriptSerializer jsSerializer = new JavaScriptSerializer();

			FireProgressEvent(String.Format("Uploading - {0}", platformString));

			//get the url to monitor for build progress
			string monitorURL = null;
			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					String msg = String.Format("Failed to upload package. Status: {0}", response.StatusCode);
					throw new BuildFailureException(msg);
				}

				FireProgressEvent("Upload success");

				StringBuilder sb = new StringBuilder();
				using (StreamReader r = new StreamReader(response.GetResponseStream()))
				{
					while (!r.EndOfStream)
						sb.Append(r.ReadLine());
				}//end using
				
				var postResult = jsSerializer.DeserializeObject(sb.ToString()) as Dictionary<string, object>;
				if (!postResult.ContainsKey("result") || String.Compare(postResult["result"].ToString(), "ok", true) != 0 || !postResult.ContainsKey("id"))
					throw new BuildFailureException();
				
				monitorURL = String.Format("{0}/{1}?email={2}&password={3}", _monitorURL, postResult["id"].ToString(), _email, _password);
			}//end using

			FireProgressEvent(String.Format("Monitoring - {0}", platformString));

			//monitor and get download url(s)
			string androidDownloadURL = null, iOSDownloadURL = null;
			while (true)
			{
				IHttpWebRequest monitorRequest = _requestFactory.BuildRequest("GET", monitorURL);
				using (HttpWebResponse monitorResponse = monitorRequest.GetResponse() as HttpWebResponse)
				{
					if (monitorResponse.StatusCode != HttpStatusCode.OK)
					{
						String msg = String.Format("Unable to get monitoring url. Response: {0}", monitorResponse.StatusCode);
						throw new BuildFailureException(msg);
					}					

					StringBuilder sb = new StringBuilder();
					using (StreamReader r = new StreamReader(monitorResponse.GetResponseStream()))
					{
						while (!r.EndOfStream)
							sb.Append(r.ReadLine());
					}//end using

					String askResponseBody = sb.ToString();
					var monitorResult = jsSerializer.DeserializeObject(askResponseBody) as Dictionary<string, object>;
					String state = monitorResult.ContainsKey("state") ? monitorResult["state"].ToString() : null;
					if (String.Compare(state, "success", true) == 0)
					{
						var info = monitorResult["info"] as Dictionary<string, object>;
						var files = info["files"] as Dictionary<string, object>;
						if ((platforms & BuildPlatform.Android) == BuildPlatform.Android)
							androidDownloadURL = files["android"].ToString();
						if ((platforms & BuildPlatform.iOS) == BuildPlatform.iOS)
							iOSDownloadURL = files["ios"].ToString();
						break;
					}
					else if (String.Compare(state, "failure", true) == 0)
					{						
						String msg = "Build failure";
						throw new BuildFailureException(msg);
					}
					else if (String.Compare(state, "pending", true) != 0) //not pending, unknown status
					{
						String msg = String.Format("Build failure. Unexpected state {0}", state);
						throw new BuildFailureException(msg);
					}//end if
				}//end using
			}//end while(true)

			FireProgressEvent("Downloading packages");

			//download the package(s)
			Dictionary<BuildPlatform, MemoryStream> ret = new Dictionary<BuildPlatform, MemoryStream>();
			if ((platforms & BuildPlatform.Android) == BuildPlatform.Android)
			{
				FireProgressEvent("Downloading - Android");

				IHttpWebRequest dlReq = _requestFactory.BuildRequest("GET", androidDownloadURL);
				dlReq.Timeout = 600000;
				using (HttpWebResponse dlResp = (HttpWebResponse)dlReq.GetResponse())
				{
					if (dlResp.StatusCode != HttpStatusCode.OK)
					{
						String msg = String.Format("Unable to get Android download url. Response: {0}", dlResp.StatusCode);
						throw new BuildFailureException(msg);
					}

					//save to memory stream
					MemoryStream ms = new MemoryStream();
					using (Stream r = dlResp.GetResponseStream())
					{
						byte[] buff = new byte[2048];
						int bytesRead = 0, totalBytesRead = 0;
						while ((bytesRead = r.Read(buff, 0, buff.Length)) > 0)
						{
							ms.Write(buff, 0, bytesRead);
							totalBytesRead += bytesRead;

							//FireProgressEvent(String.Format("Downloaded {0} bytes", totalBytesRead), ProgressStatus.INPROGRESS);
						}//end while
					}//end using

					ms.Position = 0;
					ret.Add(BuildPlatform.Android, ms);
				}//end using
			}//end if

			if ((platforms & BuildPlatform.iOS) == BuildPlatform.iOS)
			{
				FireProgressEvent("Downloading - iOS");

				IHttpWebRequest dlReq = _requestFactory.BuildRequest("GET", iOSDownloadURL);
				dlReq.Timeout = 600000;
				using (HttpWebResponse dlResp = (HttpWebResponse)dlReq.GetResponse())
				{
					if (dlResp.StatusCode != HttpStatusCode.OK)
					{
						String msg = String.Format("Unable to get iOS download url. Response: {0}", dlResp.StatusCode);
						throw new BuildFailureException(msg);
					}

					//save to memory stream
					MemoryStream ms = new MemoryStream();
					using (Stream r = dlResp.GetResponseStream())
					{
						byte[] buff = new byte[2048];
						int bytesRead = 0, totalBytesRead = 0;
						while ((bytesRead = r.Read(buff, 0, buff.Length)) > 0)
						{
							ms.Write(buff, 0, bytesRead);
							totalBytesRead += bytesRead;

							//FireProgressEvent(String.Format("Downloaded {0} bytes", totalBytesRead), ProgressStatus.INPROGRESS);
						}//end while
					}//end using

					ms.Position = 0;
					ret.Add(BuildPlatform.iOS, ms);
				}//end using
			}//end if

			return ret;
		}

		private IHttpWebRequest CreateRequest(BuildPlatform platforms, MemoryStream source, PlatformResources resources)
		{
			IHttpWebRequest request = _requestFactory.BuildRequest("POST", _serviceURL);
			//add our headers
			request.Accept = "application/json";
			request.ContentType = String.Format("multipart/form-data; boundary={0}", BOUNDARY);

			Dictionary<string, string> nameValues = new Dictionary<string, string>();
			Dictionary<string, Tuple<string, MemoryStream>> nameStreams = new Dictionary<string, Tuple<string, MemoryStream>>();
			
			nameValues.Add("email", _email);
			nameValues.Add("password", _password);
			source.Position = 0;
			nameStreams.Add("src_zip", new Tuple<string, MemoryStream>("src.zip", source));

			//Android
			if ((platforms & BuildPlatform.Android) == BuildPlatform.Android)
			{
				resources.AndroidKeystore.Position = 0;
				nameStreams.Add("and_keystore", new Tuple<string, MemoryStream>(resources.AndroidKeystoreFileName, resources.AndroidKeystore));
				nameValues.Add("and_keypass", resources.AndroidKeyPassword);
				nameValues.Add("and_storepass", resources.AndroidKeystorePassword);
				nameValues.Add("and_keyalias", resources.AndroidKeyAlias);
			}//end if

			//iOS
			if ((platforms & BuildPlatform.iOS) == BuildPlatform.iOS)
			{
				resources.iOSCertificate.Position = 0;
				resources.iOSProfile.Position = 0;
				nameStreams.Add("ios_certificate", new Tuple<string, MemoryStream>(resources.iOSCertificateFileName, resources.iOSCertificate));
				nameStreams.Add("ios_profile", new Tuple<string, MemoryStream>(resources.iOSProfileFileName, resources.iOSProfile));
				nameValues.Add("ios_password", resources.iOSCertificatePassword);
			}//end if

			//build the body
			using (Stream s = request.GetRequestStream())
			{
				//add our form text values
				foreach (String name in nameValues.Keys)
				{
					String line = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", BOUNDARY, name, nameValues[name]);
					byte[] buff = Encoding.UTF8.GetBytes(line);
					s.Write(buff, 0, buff.Length);
				}//end foreach

				//add our form files
				foreach (String name in nameStreams.Keys)
				{
					String line = String.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\";filename=\"{2}\"\r\nContent-Type: application/base64\r\n\r\n", BOUNDARY, name, nameStreams[name].Item1);

					//stream the description
					byte[] buff = Encoding.UTF8.GetBytes(line);
					s.Write(buff, 0, buff.Length);
					//stream the file
					buff = new byte[nameStreams[name].Item2.Length];
					nameStreams[name].Item2.Read(buff, 0, buff.Length);
					//buff = nameStreams[name].Item2.GetBuffer();//File.ReadAllBytes(namePaths[name]);
					s.Write(buff, 0, buff.Length);
					//stream endline
					buff = Encoding.UTF8.GetBytes(Environment.NewLine);
					s.Write(buff, 0, buff.Length);
				}//end foreach

				//end our form data
				String endLine = String.Format("--{0}--", BOUNDARY);
				byte[] endBuff = Encoding.UTF8.GetBytes(endLine);
				s.Write(endBuff, 0, endBuff.Length);
			}//end using

			return request;
		}

		
	}
}
