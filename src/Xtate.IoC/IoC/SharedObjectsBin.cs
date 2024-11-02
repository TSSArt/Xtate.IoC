namespace Xtate.IoC;

public class SharedObjectsBin : ObjectsBin
{
	private int _referenceCount;

	internal void AddReference() => Interlocked.Increment(ref _referenceCount);

	internal bool RemoveReference() => Interlocked.Decrement(ref _referenceCount) == -1;
}