using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriggerStandaloneConsole;

namespace TriggerStandaloneConsoleTest
{
	[TestClass]
	public class StandaloneHttpBuilderTest
	{
		public TestContext TestContext
		{
			get;
			set;
		}

		static string postURL = "http://fakeserviceurl";
		static string monitorURL = postURL + "/monitor";
		static string email = "some@email.com";
		static string password = "password";

		[TestInitialize]
		public void Init()
		{
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_Source_POST_Fail_StatusCode() 
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up first "POST" request it will make
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.InternalServerError);

			//setup request factory
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_Source_POST_Returns_Fail_In_Body() 
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"badstatus\", \"id\": \"" + Guid.NewGuid() + "\" }"));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up first "POST" request it will make
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(responseStream);

			//setup request factory
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_Monitoring_URL_Returns_Fail_StatusCode()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			MemoryStream responseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(responseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.BadRequest);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);
			
			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_Monitoring_URL_Returns_Fail_In_Body()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			MemoryStream postResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));
			MemoryStream monitorResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"state\": \"failure\" }"));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(postResponseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockMonitorResponse.Setup(g => g.GetResponseStream()).Returns(monitorResponseStream);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}

			mockFactory.VerifyAll();
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_Android_Download_URL_Returns_Fail_StatusCode()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockAndroidDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockAndroidDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			string androidurl = "androidurl", iosurl = "iosurl";
			MemoryStream postResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));
			MemoryStream monitorResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"state\": \"success\", \"info\": { \"files\": { \"android\": \"" + androidurl + "\", \"ios\": \"" + iosurl + "\" } } }"));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(postResponseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockMonitorResponse.Setup(g => g.GetResponseStream()).Returns(monitorResponseStream);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//mock up Android download request/response
			mockAndroidDownloadResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.InternalServerError);
			mockAndroidDownloadRequest.Setup(g => g.GetResponse()).Returns(mockAndroidDownloadResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == androidurl))).Returns(mockAndroidDownloadRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}

			mockFactory.VerifyAll();
		}

		[TestMethod]
		public void Builder_Throws_BuildFailureException_When_iOS_Download_URL_Returns_Fail_StatusCode()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockAndroidDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockAndroidDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockiosDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockiosDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			string androidurl = "androidurl", iosurl = "iosurl";
			MemoryStream postResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));
			MemoryStream monitorResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"state\": \"success\", \"info\": { \"files\": { \"android\": \"" + androidurl + "\", \"ios\": \"" + iosurl + "\" } } }"));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(postResponseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockMonitorResponse.Setup(g => g.GetResponseStream()).Returns(monitorResponseStream);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//mock up Android download request/response
			mockAndroidDownloadResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockAndroidDownloadResponse.Setup(g => g.GetResponseStream()).Returns(new MemoryStream());
			mockAndroidDownloadRequest.Setup(g => g.GetResponse()).Returns(mockAndroidDownloadResponse.Object);

			//mock up iOS download request/response
			mockiosDownloadResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.InternalServerError);
			mockiosDownloadResponse.Setup(g => g.GetResponseStream()).Returns(new MemoryStream());
			mockiosDownloadRequest.Setup(g => g.GetResponse()).Returns(mockiosDownloadResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == androidurl))).Returns(mockAndroidDownloadRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == iosurl))).Returns(mockiosDownloadRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			try
			{
				b.BuildForPlatforms(BuildPlatform.iOS | BuildPlatform.Android, new MemoryStream(), mockResource.Object);
				Assert.Fail();
			}
			catch (BuildFailureException e)
			{
			}

			mockFactory.VerifyAll();
		}

		[TestMethod]
		public void Builder_Downloads_Returns_Android_Correctly()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockAndroidDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockAndroidDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockiosDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockiosDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			string androidurl = "androidurl", androidpackagecontents = "ANDROIDPACKAGE";
			MemoryStream postResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));
			MemoryStream monitorResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"state\": \"success\", \"info\": { \"files\": { \"android\": \"" + androidurl + "\" } } }"));
			MemoryStream downloadStream = new MemoryStream(Encoding.UTF8.GetBytes(androidpackagecontents));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(postResponseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockMonitorResponse.Setup(g => g.GetResponseStream()).Returns(monitorResponseStream);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//mock up Android download request/response
			mockAndroidDownloadResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockAndroidDownloadResponse.Setup(g => g.GetResponseStream()).Returns(downloadStream);
			mockAndroidDownloadRequest.Setup(g => g.GetResponse()).Returns(mockAndroidDownloadResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == androidurl))).Returns(mockAndroidDownloadRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			Dictionary<BuildPlatform, MemoryStream> dict = b.BuildForPlatforms(BuildPlatform.Android, new MemoryStream(), mockResource.Object);

			Assert.IsNotNull(dict);
			Assert.IsTrue(dict.ContainsKey(BuildPlatform.Android));
			Assert.IsNotNull(dict[BuildPlatform.Android]);
			string downloaded = string.Empty;

			byte[] buff = new byte[dict[BuildPlatform.Android].Length];
			dict[BuildPlatform.Android].Read(buff, 0, buff.Length);
			downloaded = Encoding.UTF8.GetString(buff);
			Assert.AreEqual(downloaded, androidpackagecontents);

			mockFactory.VerifyAll();
		}

		[TestMethod]
		public void Builder_Downloads_Returns_iOS_Correctly()
		{
			MockRepository repo = new MockRepository(MockBehavior.Loose);
			Mock<IHttpWebRequest> mockPostRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockPostResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockMonitorRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockMonitorResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockAndroidDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockAndroidDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<IHttpWebRequest> mockiosDownloadRequest = repo.Create<IHttpWebRequest>();
			Mock<HttpWebResponse> mockiosDownloadResponse = repo.Create<HttpWebResponse>();
			Mock<HttpWebRequestFactory> mockFactory = repo.Create<HttpWebRequestFactory>();
			Mock<PlatformResources> mockResource = repo.Create<PlatformResources>();
			Guid monitorId = Guid.NewGuid();
			string iosurl = "iosurl", iospackagecontents = "IOSPACKAGECONTENTS";
			MemoryStream postResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"result\": \"ok\", \"id\": \"" + monitorId + "\" }"));
			MemoryStream monitorResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("{ \"state\": \"success\", \"info\": { \"files\": { \"ios\": \"" + iosurl + "\" } } }"));
			MemoryStream downloadStream = new MemoryStream(Encoding.UTF8.GetBytes(iospackagecontents));

			//mock up resources
			mockResource.Setup(p => p.AndroidKeystore).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSCertificate).Returns(new MemoryStream());
			mockResource.Setup(p => p.iOSProfile).Returns(new MemoryStream());

			//mock up content "POST" request/response
			mockPostRequest.Setup(p => p.GetRequestStream()).Returns(new MemoryStream());
			mockPostRequest.Setup(p => p.GetResponse()).Returns(mockPostResponse.Object);
			mockPostResponse.Setup(p => p.StatusCode).Returns(HttpStatusCode.OK);
			mockPostResponse.Setup(p => p.GetResponseStream()).Returns(postResponseStream);

			//mock up monitoring "GET" request/response
			mockMonitorResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockMonitorResponse.Setup(g => g.GetResponseStream()).Returns(monitorResponseStream);
			mockMonitorRequest.Setup(g => g.GetResponse()).Returns(mockMonitorResponse.Object);

			//mock up iOS download request/response
			mockiosDownloadResponse.Setup(g => g.StatusCode).Returns(HttpStatusCode.OK);
			mockiosDownloadResponse.Setup(g => g.GetResponseStream()).Returns(downloadStream);
			mockiosDownloadRequest.Setup(g => g.GetResponse()).Returns(mockiosDownloadResponse.Object);

			//setup request factory
			string instanceMonitorURL = String.Format("{0}/{1}?email={2}&password={3}", monitorURL, monitorId, email, password);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "POST"), It.Is<string>(s => s == postURL))).Returns(mockPostRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == instanceMonitorURL))).Returns(mockMonitorRequest.Object);
			mockFactory.Setup(f => f.BuildRequest(It.Is<string>(s => s == "GET"), It.Is<string>(s => s == iosurl))).Returns(mockiosDownloadRequest.Object);

			StandaloneHttpBuilder b = new StandaloneHttpBuilder(mockFactory.Object, postURL, monitorURL, email, password);
			Dictionary<BuildPlatform, MemoryStream> dict = b.BuildForPlatforms(BuildPlatform.iOS, new MemoryStream(), mockResource.Object);

			Assert.IsNotNull(dict);
			Assert.IsTrue(dict.ContainsKey(BuildPlatform.iOS));
			Assert.IsNotNull(dict[BuildPlatform.iOS]);
			string downloaded = string.Empty;

			byte[] buff = new byte[dict[BuildPlatform.iOS].Length];
			dict[BuildPlatform.iOS].Read(buff, 0, buff.Length);
			downloaded = Encoding.UTF8.GetString(buff);
			Assert.AreEqual(downloaded, iospackagecontents);

			mockFactory.VerifyAll();
		}
	}

}
		
