using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	class HttpWebRequestWrapper : IHttpWebRequest
	{
		private readonly HttpWebRequest _request;
		public HttpWebRequestWrapper(string url)
		{
			_request = WebRequest.CreateHttp(url);
		}

		public int Timeout
		{
			get
			{
				return _request.Timeout;
			}
			set
			{
				_request.Timeout = value;
			}
		}

		public string Accept
		{
			get
			{
				return _request.Accept;
			}
			set
			{
				_request.Accept = value;
			}
		}

		public string ContentType
		{
			get
			{
				return _request.ContentType;
			}
			set
			{
				_request.ContentType = value;
			}
		}

		public Stream GetRequestStream()
		{
			return _request.GetRequestStream();
		}


		public string Method
		{
			get
			{
				return _request.Method;
			}
			set
			{
				_request.Method = value;
			}
		}


		public HttpWebResponse GetResponse()
		{
			return _request.GetResponse() as HttpWebResponse;
		}
	}
}
