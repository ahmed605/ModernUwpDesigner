using System;

namespace Microsoft.Internal.Performance;

internal struct CodeMarkerStartEnd : IDisposable
{
	private int _end;

	private byte[] _buffer;

	internal CodeMarkerStartEnd(int begin, int end, bool correlated = false)
	{
		_buffer = (correlated ? Microsoft.Internal.Performance.CodeMarkers.AttachCorrelationId(null, Guid.NewGuid()) : null);
		_end = end;
		CodeMarker(begin);
	}

	public void Dispose()
	{
		if (_end != 0)
		{
			CodeMarker(_end);
			_end = 0;
			_buffer = null;
		}
	}

	private void CodeMarker(int id)
	{
		if (_buffer == null)
		{
			Microsoft.Internal.Performance.CodeMarkers.Instance.CodeMarker(id);
		}
		else
		{
			Microsoft.Internal.Performance.CodeMarkers.Instance.CodeMarkerEx(id, _buffer);
		}
	}
}
