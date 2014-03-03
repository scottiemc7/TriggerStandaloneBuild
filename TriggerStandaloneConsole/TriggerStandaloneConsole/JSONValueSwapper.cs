using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TriggerStandaloneConsole
{
	public class JSONValueSwapper : IJSONValueSwapper
	{
		private JavaScriptSerializer _serializer = new JavaScriptSerializer();
		private readonly Dictionary<string, object> _jsonObject;
		public JSONValueSwapper(string json)
		{
			_jsonObject = _serializer.DeserializeObject(json) as Dictionary<string, object>;
		}

		public void Swap(string key, string newValue)
		{
			string[] keys = key.Split('\\');
			Dictionary<string, object> dictEntry = _jsonObject;
			try
			{
				for (int i = 0; i < (keys.Length - 1); i++)
					dictEntry = dictEntry[keys[i]] as Dictionary<string, object>;
			}
			catch (KeyNotFoundException)
			{
				throw new ArgumentException("Unknown key");	
			}

			if (!dictEntry.ContainsKey(keys[keys.Length - 1]))
				throw new ArgumentException("Unknown key");			

			dictEntry[keys[keys.Length-1]] = newValue;
		}

		public override string ToString()
		{
			return _serializer.Serialize(_jsonObject);
		}
	}
}
