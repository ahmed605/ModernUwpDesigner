using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal interface IPriFileGenerator
{
	string ConfigFilePath { get; }

	string DestinationPriFilePath { get; }

	string PriInfoDumpFilePath { get; }

	Task<string> DumpPriAsync(string indexFile, string outputFile, CancellationToken cancellationToken);

	Task<string> MakePriAsync(string sourceDirectory, string packageName, CancellationToken cancellationToken);

	void WriteMakePriConfig(string priPath, string indexerType);
}
