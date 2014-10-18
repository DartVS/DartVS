using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DartVS
{
	public static class DartSdk
	{
		const string RemoteSdkZipUrl = "https://storage.googleapis.com/dart-archive/channels/stable/release/latest/sdk/dartsdk-windows-ia32-release.zip";

		public static async Task<string> GetSdkPathAsync()
		{
			string result = Environment.GetEnvironmentVariable("DART_SDK", EnvironmentVariableTarget.Process);
			if (!Directory.Exists(result))
			{
				string extensionName = "DartVS";
				string extensionVersion = AssemblyInfo.AssemblyInformationalVersion;
				string tempDir = Path.Combine(Path.GetTempPath(), string.Format("{0}-{1}-sdk", extensionName, extensionVersion));
				result = Path.Combine(tempDir, "dart-sdk");
				if (!Directory.Exists(result))
				{
					// TODO: This code might have issues if two threads ask or the SDK at the same time and we need to download it...
					// Thread1 will start the download.
					// Thread2 will skip this code (DirectoryExists; unless we're very unlucky with timing), then crash on
					// the File.Exists check for dart.exe.

					Directory.CreateDirectory(tempDir);
					string sdkName = "dartsdk-windows-ia32-release.zip";
					string compressed = Path.Combine(tempDir, sdkName);

					using (var httpClient = new HttpClient())
					using (var stream = await httpClient.GetStreamAsync(RemoteSdkZipUrl).ConfigureAwait(false))
					using (var outputStream = File.OpenWrite(compressed))
						await stream.CopyToAsync(outputStream).ConfigureAwait(false);

					ZipFile.ExtractToDirectory(compressed, tempDir);
				}
			}

			if (!Directory.Exists(result) || !File.Exists(Path.Combine(result, "bin", "dart.exe")))
				throw new NotSupportedException("Could not locate or download the Dart SDK. All analysis is disabled.");

			return result;
		}
	}
}
