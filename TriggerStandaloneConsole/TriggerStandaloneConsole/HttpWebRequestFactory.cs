using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public class HttpWebRequestFactory
	{
		public virtual IHttpWebRequest BuildRequest(string method, string url)
		{
			IHttpWebRequest request = new HttpWebRequestWrapper(url);
			request.Method = method;
			return request;
		}
	}
}
