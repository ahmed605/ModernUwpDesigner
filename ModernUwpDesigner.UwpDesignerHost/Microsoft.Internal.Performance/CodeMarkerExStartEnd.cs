using System;

namespace Microsoft.Internal.Performance;

internal struct CodeMarkerExStartEnd : IDisposable
{
	private int _end;

	private byte[] _aBuff;

	internal CodeMarkerExStartEnd(int begin, int end, byte[] aBuff, bool correlated = false)
	{
		_aBuff = (correlated ? Microsoft.Internal.Performance.CodeMarkers.AttachCorrelationId(aBuff, Guid.NewGuid()) : aBuff);
		_end = end;
		Microsoft.Internal.Performance.CodeMarkers.Instance.CodeMarkerEx(begin, _aBuff);
	}

	internal CodeMarkerExStartEnd(int begin, int end, Guid guidData, bool correlated = false)
		: this(begin, end, guidData.ToByteArray(), correlated)
	{
	}

	internal CodeMarkerExStartEnd(int begin, int end, string stringData, bool correlated = false)
		: this(begin, end, Microsoft.Internal.Performance.CodeMarkers.StringToBytesZeroTerminated(stringData), correlated)
	{
	}

	internal CodeMarkerExStartEnd(int begin, int end, uint uintData, bool correlated = false)
		: this(begin, end, BitConverter.GetBytes(uintData), correlated)
	{
	}

	internal CodeMarkerExStartEnd(int begin, int end, ulong ulongData, bool correlated = false)
		: this(begin, end, BitConverter.GetBytes(ulongData), correlated)
	{
	}

	public void Dispose()
	{
		if (_end != 0)
		{
			Microsoft.Internal.Performance.CodeMarkers.Instance.CodeMarkerEx(_end, _aBuff);
			_end = 0;
			_aBuff = null;
		}
	}
}
