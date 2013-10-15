using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	public class PlatformResources
	{
		public virtual MemoryStream AndroidKeystore { get; set; }
		public virtual string AndroidKeystoreFileName { get; set; }
		public virtual string AndroidKeyAlias { get; set; }
		public virtual string AndroidKeyPassword { get; set; }
		public virtual string AndroidKeystorePassword { get; set; }

		public virtual MemoryStream iOSCertificate { get; set; }
		public virtual string iOSCertificateFileName { get; set; }
		public virtual MemoryStream iOSProfile { get; set; }
		public virtual string iOSProfileFileName { get; set; }
		public virtual string iOSCertificatePassword { get; set; }
	}
}
