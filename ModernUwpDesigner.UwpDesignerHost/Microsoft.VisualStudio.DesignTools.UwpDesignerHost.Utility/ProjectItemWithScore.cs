using Microsoft.VisualStudio.DesignTools.DesignerContract;

namespace Microsoft.VisualStudio.DesignTools.UwpDesignerHost.Utility;

public class ProjectItemWithScore
{
	public int Score { get; }

	public IHostSourceItem Item { get; }

	public ProjectItemWithScore(int score, IHostSourceItem item)
	{
		Score = score;
		Item = item;
	}
}
