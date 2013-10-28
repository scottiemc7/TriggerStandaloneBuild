using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public class DotNetZipAdapter : IZip
	{
		public MemoryStream ZipDirectory(string path, string excludeDirs)
		{
			//make a copy of the directory to zip if we need to exclude directories
			string usePath = path;
			bool cleanPath = false;
			if (!String.IsNullOrEmpty(excludeDirs))
			{
				usePath = String.Format("{0}{1}", System.IO.Path.GetTempPath(), Guid.NewGuid());
				new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(path, usePath);
				cleanPath = true;

				//remove excluded dirs
				foreach (string dir in excludeDirs.Split(';'))
				{
					string fullPath = Path.Combine(usePath, dir.Replace("/", "\\").TrimStart('\\'));
					Directory.Delete(fullPath, true);
				}
			}//end if

			MemoryStream ms = null;
			string zipPath = String.Format("{0}_{1}.zip", path.TrimEnd('\\'), Guid.NewGuid());
			using (ZipFile zip = new ZipFile())
			{
				zip.AddDirectory(usePath);
				
				if (File.Exists(zipPath))
					File.Delete(zipPath);

				zip.Save(zipPath);
			}//end using

			if (cleanPath)
				Directory.Delete(usePath, true);

			ms  = new MemoryStream(File.ReadAllBytes(zipPath));
			File.Delete(zipPath);		

			return ms;
		}
	}
}
