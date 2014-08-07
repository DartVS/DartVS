namespace DanTup.DartAnalysis.Tests
{
	public abstract class Tests
	{
		protected const string SdkFolder = @"M:\Apps\Dart\sdk";
		protected const string ServerScript = @"M:\Coding\Applications\DanTup.DartVS\Dart\AnalysisServer.dart";
		protected const string SampleDartProject = @"M:\Coding\Applications\DanTup.DartVS\DanTup.DartAnalysis.Tests.SampleDartProject";
		protected const string HelloWorldFile = SampleDartProject + @"\hello_world.dart";
		protected const string SingleTypeErrorFile = SampleDartProject + @"\single_type_error.dart";
	}
}
