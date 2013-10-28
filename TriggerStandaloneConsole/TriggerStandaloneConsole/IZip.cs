using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public interface IZip
	{
		MemoryStream ZipDirectory(string path, string excludeDirs);
	}
}
