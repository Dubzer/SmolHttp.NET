namespace SmolHttp;

class RotatableIndex
{
    private uint _index;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public uint GetIndex()
    {
        try
        {
            _semaphore.Wait();
            if(_index == uint.MaxValue)
                _index = 0;

            return _index++;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}