﻿namespace Mafia.Extensions;

public static class IntExtensions
{
    public static bool Between(this int value, int a, int b) => a <= value && value <= b;
    public static bool Between(this int value, (int a, int b) v) => value.Between(v.a, v.b);
}
