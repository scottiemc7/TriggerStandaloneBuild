using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public interface IJSONValueSwapper
	{
		void Swap(string key, string newValue);
	}
}
