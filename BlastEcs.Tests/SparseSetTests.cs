using BlastEcs.Collections;

namespace BlastEcs.Tests;

public class SparseSetTests
{
    SparseMap<int> set;

    [SetUp]
    public void Setup()
    {
        set = new();
    }

    [Test]
    public void SparseSetAddTest()
    {
        Assert.That(set.Count, Is.EqualTo(0));
        set.Add(1, 2);
        Assert.That(set.Count, Is.EqualTo(1));
    }

    [Test]
    public void SparseSetContainsTest()
    {
        Assert.That(set.Contains(1), Is.False);
        set.Add(1, 2);
        Assert.That(set.Contains(1), Is.True);
    }

    [Test]
    public void SparseSetRemoveTest()
    {
        set.Add(1, 2);
        Assert.That(set.Contains(1), Is.True);
        set.Remove(1);
        Assert.That(set.Contains(1), Is.False);
    }
}
