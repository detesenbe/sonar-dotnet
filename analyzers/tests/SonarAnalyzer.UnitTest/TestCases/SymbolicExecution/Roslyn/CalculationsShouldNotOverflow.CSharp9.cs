﻿using System;
using System.Collections.Generic;
using System.Linq;

int i = 2147483600;
_ = i + 100; // Noncompliant

public class NativeInt
{
    public void RelationalPattern(int i)
    {
        switch (i)
        {
            case <= 2147483547:
                _ = i + 100;    // Compliant
                break;
            default:
                _ = i + 100;    // FN
                break;
        }
        _ = i switch
        {
            <= 2147483547 => i + 100,   // Compliant
            _ => i + 100,               // FN
        };
    }

    public void PositiveOverflowNativeInt()
    {
        nint i = 2147483600;
        _ = i + 100; // Compliant, we can't tell what's the native MaxValue on the run machine

        nuint ui = (nuint)18446744073709551615;
        _ = ui + 100; // Compliant, we can't tell what's the native MaxValue on the run machine
    }

    public void NegativeOverflowNativeInt()
    {
        nint i = -2147483600;
        _ = i - 100;  // Compliant, we can't tell what's the native MinValue for the run machine
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            int i = -2147483600;
            _ = i - 100; // Noncompliant
        };
    }

    public void LambdaDiscard(IEnumerable<int> items)
    {
        var result = items.Select((_, _) =>
        {
            int i = -2147483600;
            _ = i - 100; // Noncompliant
            return i;
        });
    }
}

public class Properties
{
    public int GetInit
    {
        get
        {
            int i = 2147483600;
            _ = i + 100; // // Noncompliant
            return i;
        }
        init
        {
            int i = 2147483600;
            _ = i + 100; // Noncompliant
        }
    }
}

public record SampleRecord(int I)
{
    public void Inc()
    {
        int i = 2147483600;
        _ = i + 100; // Noncompliant
    }

    public void With()
    {
        var r = new SampleRecord(0);
        r = r with { I = 2147483600 };
        _ = r.I + 100;  // FN
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        int i = 2147483600;
        _ = i + 100; // Noncompliant
    }
}
