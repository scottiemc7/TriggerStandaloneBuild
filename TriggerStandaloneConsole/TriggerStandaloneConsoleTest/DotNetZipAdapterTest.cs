using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TriggerStandaloneConsole;
using Ionic.Zip;

namespace TriggerStandaloneConsoleTest
{
	[TestClass]
	public class DotNetZipAdapterTest
	{
		public TestContext TestContext
		{
			get;
			set;
		}

		[TestMethod]
		public void TestZipDirectory()
		{
			//create three directories in our zip dir
			string zipDir = Path.Combine(TestContext.TestDeploymentDir, "zipDir");
			string zipInner1Dir = Path.Combine(zipDir, "testDir1");
			string zipInner2Dir = Path.Combine(zipDir, "testDir2");
			string zipInner3Dir = Path.Combine(zipDir, "testDir3");
			string zipInnerInner3Dir = Path.Combine(zipInner2Dir, "testDir3");
			string zipInnerInner4Dir = Path.Combine(zipInner2Dir, "testDir4");

			Directory.CreateDirectory(zipDir);
			Directory.CreateDirectory(zipInner1Dir);
			Directory.CreateDirectory(zipInner2Dir);
			Directory.CreateDirectory(zipInner3Dir);
			Directory.CreateDirectory(zipInnerInner3Dir);
			Directory.CreateDirectory(zipInnerInner4Dir);

			//zip but exclude some of the dirs
			IZip zip = new DotNetZipAdapter();
			using (MemoryStream ms = zip.ZipDirectory(zipDir, String.Format("{0};{1};{2}", "\\testDir1", "testDir3", "testDir2\\testDir4")))
			{
				string zipPath = Path.Combine(TestContext.TestDir, "test.zip");
				File.WriteAllBytes(zipPath, ms.GetBuffer());

				//make sure testDir2 and testDir2//testDir3 are only dir in zip file
				using (ZipFile z = new ZipFile(zipPath))
				{
					Assert.IsTrue(z.ContainsEntry("testDir2/"));
					Assert.IsTrue(z.ContainsEntry("testDir2/testDir3/"));
					Assert.IsTrue(!z.ContainsEntry("testDir1/"));
					Assert.IsTrue(!z.ContainsEntry("testDir3/"));
					Assert.IsTrue(!z.ContainsEntry("testDir2/testDir4/"));
				}//end using
			}//end using
		}
	}
}
