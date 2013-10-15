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
			MemoryStream ms = new MemoryStream();
			using (ZipFile zip = new ZipFile())
			{
				zip.AddDirectory(path);
				if (!String.IsNullOrEmpty(excludeDirs))
				{
					foreach(string dir in excludeDirs.Split(';'))
						zip.RemoveSelectedEntries("name = " + dir.TrimStart('\\', '/') + "/");
				}//end if				
				zip.Save(ms);
			}//end using

			return ms;
		}
	}
}
