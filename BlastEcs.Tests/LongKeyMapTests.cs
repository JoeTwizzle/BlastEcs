using BlastEcs.Collections;

namespace BlastEcs.Tests;

public class LongKeyMapTests
{
    LongKeyMap<int> map;

    [SetUp]
    public void Setup()
    {
        map = new();
    }

    [Test]
    public void LongKeyMapAddTest()
    {
        Assert.That(map.Count, Is.EqualTo(0));
        map.Add(1, 2);
        Assert.That(map.Count, Is.EqualTo(1));
    }

    [Test]
    public void LongKeyMapContainsTest()
    {
        Assert.That(map.Contains(1), Is.False);
        map.Add(1, 2);
        Assert.That(map.Contains(1), Is.True);
    }

    [Test]
    public void LongKeyMapRemoveTest()
    {
        map.Add(1, 2);
        Assert.That(map.Contains(1), Is.True);
        map.Remove(1);
        Assert.That(map.Contains(1), Is.False);
    }
}