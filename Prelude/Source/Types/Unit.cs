#nullable enable
using System;

namespace Kehlet.SourceGenerator;

internal readonly struct Unit : IEquatable<Unit>
{
    public bool Equals(Unit other) => true;
    public override bool Equals(object? obj) => obj is Unit;
    public override int GetHashCode() => 0;
    public override string ToString() => "()";

    public static bool operator ==(Unit a, Unit b) => true;
    public static bool operator !=(Unit a, Unit b) => false;
}
