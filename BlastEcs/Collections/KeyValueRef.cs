namespace BlastEcs.Collections;

public ref struct KeyValueRef<TKey, TValue>
{
    public TKey Index;
    public ref TValue Value;

    public KeyValueRef(TKey index, ref TValue value)
    {
        Index = index;
        Value = ref value;
    }
}
