using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	[Flags]
	public enum BuildPlatform { Android = 1, iOS = 2 };

	interface IStandaloneBuilder
	{
		//void AddProgressListener();
		Dictionary<BuildPlatform, System.IO.MemoryStream> BuildForPlatforms(BuildPlatform platforms, MemoryStream source, PlatformResources resources);
	}

	public class BuildFailureException : Exception
	{
	}
}
