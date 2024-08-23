using System;

using Jelly.Utilities;

namespace Jelly;

public struct Tag(uint bitmask) : IEquatable<Tag>
{
    public uint Bitmask { get; set; } = bitmask;

    public Tag() : this(0)
    {
    }

    public static bool Matches(Tag tag1, Tag tag2, TagFilter filter) => filter switch
    {
        TagFilter.AtLeastOne => (tag1.Bitmask & tag2.Bitmask) != 0,
        TagFilter.All => (tag1.Bitmask & tag2.Bitmask) == tag2.Bitmask,
        TagFilter.None => (tag1.Bitmask & tag2.Bitmask) == 0,
        _ => false,
    };

    public static explicit operator Tag(uint value) => new(value);

    public static explicit operator uint(Tag value) => value.Bitmask;

    public static Tag operator ~(Tag value) => new(~value.Bitmask);

    public static Tag operator &(Tag left, Tag right) => new(left.Bitmask & right.Bitmask);

    public static Tag operator |(Tag left, Tag right) => new(left.Bitmask | right.Bitmask);

    public static Tag operator ^(Tag left, Tag right) => new(left.Bitmask ^ right.Bitmask);

    public readonly bool Matches(Tag other, TagFilter filter) => Matches(this, other, filter);

    public void Add(Tag tag) => Bitmask |= tag.Bitmask;

    public void Add(uint tag) => Bitmask |= tag;

    public void Remove(Tag tag) => Bitmask &= ~tag.Bitmask;

    public void Remove(uint tag) => Bitmask &= ~tag;

    public readonly bool Equals(Tag other)
    {
        return other.Bitmask == Bitmask;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Tag tag && Equals(tag);
    }

    public static bool operator ==(Tag left, Tag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Tag left, Tag right)
    {
        return !(left == right);
    }
}
