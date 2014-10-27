using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion(AssemblyInfo.AssemblyVersion)]
[assembly: AssemblyFileVersion(AssemblyInfo.AssemblyFileVersion)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.AssemblyInformationalVersion)]
[assembly: AssemblyCompany("Danny Tuppeny")]
[assembly: AssemblyCopyright("Copyright Danny Tuppeny © 2014")]
[assembly: ComVisible(false)]

/// <seealso cref="DanTup.DartVS.DartConstants.InstalledProductVersion"/>
internal static class AssemblyInfo
{
	private const string MajorMinorVersion = "0.5";
	private const string InformationalSuffix = "-dev";

	public const string AssemblyVersion = MajorMinorVersion + ".0.0";
	public const string AssemblyFileVersion = MajorMinorVersion + ".0.0";
	public const string AssemblyInformationalVersion = AssemblyFileVersion + InformationalSuffix;

	/// <summary>
	/// The extension version as it appears in Help -> About Visual Studio. This should match the value defined in
	/// source.extension.vsixmanifest.
	/// </summary>
	public const string InstalledProductVersion = MajorMinorVersion;
}
