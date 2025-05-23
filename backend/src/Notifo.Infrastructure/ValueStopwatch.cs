﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Notifo.Infrastructure;

public readonly struct ValueStopwatch(long startTime)
{
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;

    private static readonly double TickFrequency;

    static ValueStopwatch()
    {
        if (Stopwatch.IsHighResolution)
        {
            TickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
        }
    }

    public static ValueStopwatch StartNew()
    {
        return new ValueStopwatch(Stopwatch.GetTimestamp());
    }

    public long Stop()
    {
        var elapsed = Stopwatch.GetTimestamp() - startTime;

        if (elapsed < 0)
        {
            return elapsed;
        }

        if (Stopwatch.IsHighResolution)
        {
            elapsed = unchecked((long)(elapsed * TickFrequency));
        }

        return elapsed / TicksPerMillisecond;
    }
}
