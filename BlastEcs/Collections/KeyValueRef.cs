namespace BlastEcs.Collections;

public ref struct KeyValueRef<TKey, TValue>
{
    public TKey Key;
    public ref TValue Value;

    public KeyValueRef(TKey index, ref TValue value)
    {
        Key = index;
        Value = ref value;
    }
}
