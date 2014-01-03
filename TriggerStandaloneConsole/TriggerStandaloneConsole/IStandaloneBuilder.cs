using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public enum BuildPlatform { Android = 1, iOS = 2 };

	interface IStandaloneBuilder
	{
		event ProgressEventHandler ProgressEvent;
		Dictionary<BuildPlatform, System.IO.MemoryStream> BuildForPlatforms(BuildPlatform platforms, MemoryStream source, PlatformResources resources);
	}

	public class BuildFailureException : Exception {
		public BuildFailureException() : base() { }
		public BuildFailureException(string message) : base(message) { }
	}

	public delegate void ProgressEventHandler(string message);
}
