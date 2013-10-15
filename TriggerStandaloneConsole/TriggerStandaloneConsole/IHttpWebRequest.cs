using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public interface IHttpWebRequest
	{
		int Timeout { get; set; }
		string Accept { get; set; }
		string ContentType { get; set; }
		string Method { get; set; }

		Stream GetRequestStream();
		HttpWebResponse GetResponse();
	}
}
