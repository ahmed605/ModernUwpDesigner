namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

internal struct FileCopyToken
{
	public string SourcePath { get; }

	public string DestinationPath { get; }

	public FileCopyToken(string sourcePath, string destinationPath)
	{
		SourcePath = sourcePath;
		DestinationPath = destinationPath;
	}

	public override string ToString()
	{
		return "(" + SourcePath + ", " + DestinationPath + ")";
	}
}
