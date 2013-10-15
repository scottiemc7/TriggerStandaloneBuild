using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerStandaloneConsole
{
	class Options
	{
		[Option('e', "email", Required = true, HelpText = "Email Address.")]
		public string Email { get; set; }

		[Option('p', "password", Required = true, HelpText = "Password.")]
		public string Password { get; set; }

		[Option('s', "src", Required = true, HelpText = "Src path.")]
		public string SrcPath { get; set; }

		[Option('d', "download", Required = true, HelpText = "Download path. Compiled packages will be downloaded here.")]
		public string DownloadPath { get; set; }

		[Option('a', "android", Required = false, HelpText = "Build for Android.")]
		public bool Android { get; set; }

		[Option('i', "ios", Required = false, HelpText = "Build for iOS.")]
		public bool iOS { get; set; }

		[Option('k', "keystore", Required = false, HelpText = "Android Keystore Path.")]
		public string AndroidKeystorePath { get; set; }

		[Option('j', "keystorepass", Required = false, HelpText = "Android Keystore Password.")]
		public string AndroidKeystorePass { get; set; }

		[Option('f', "keyalias", Required = false, HelpText = "Android Key Alias.")]
		public string AndroidKeyAlias { get; set; }

		[Option('g', "keypass", Required = false, HelpText = "Android Key Password.")]
		public string AndroidKeyPass { get; set; }

		[Option('c', "cert", Required = false, HelpText = "iOS Certificate Path.")]
		public string iOSCertificatePath { get; set; }

		[Option('h', "certpass", Required = false, HelpText = "iOS Certificate Password.")]
		public string iOSCertificatePass { get; set; }

		[Option('n', "profile", Required = false, HelpText = "iOS Provisioning Profile Path.")]
		public string iOSProfilePath { get; set; }

		[Option('o', "androidignore", Required = false, HelpText = "List of directories to ignore when building for Android. Relative to src, seperated by ;")]
		public string AndroidIgnore { get; set; }

		[Option('q', "iosignore", Required = false, HelpText = "List of directories to ignore when building for iOS. Relative to src, seperated by ;")]
		public string iOSIgnore { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}

}
