﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Compliant
{
    class ClassWithProperty
    {
        int SomeProperty => 42;
    }
    class ClassWithField
    {
        int SomeField = 42;
    }
    class ClassWithMethod
    {
        void Method() { }
    }
    class ClassWithIndexer
    {
        int this[int index] => 42;
    }
    class ClassWithDelegate
    {
        delegate void MethodDelegate();
    }
    class ClassWithEvent
    {
        event EventHandler CustomEvent;
    }

    class GenericNotEmpty<T>
    {
        void Method(T arg) { }
    }

    class GenericNotEmptyWithConstraints<T>
        where T : class
    {
        void Method(T arg) { }
    }

    partial class PartialNotEmpty
    {
        int Prop => 42;
    }

    partial class PartialEmpty { }               // Compliant - Source Generators and some frameworks use empty partial classes as placeholders

    [ComVisible(true)]
    class ClassWithAttribute { }                 // Compliant - types with attributes are ignored

    [ComVisible(true), Obsolete]
    class ClassWithMultipleAttributes { }

    class OuterClass
    {
        class InnerEmpty1 { }                    // Noncompliant
        private class InnerEmpty2 { }            // Noncompliant
        protected class InnerEmpty3 { }          // Noncompliant
        internal class InnerEmpty4 { }           // Noncompliant
        protected internal class InnerEmpty5 { } // Noncompliant
        public class InnerEmpty6 { }             // Noncompliant

        public class InnerEmptyWithComments      // Noncompliant
        {
            // Some comment
        }

        class InnerNonEmpty
        {
            public int SomeProperty => 42;
        }
    }

    class IntegerList : List<int> { }             // Compliant - creates a more specific type without adding new members to it (similar to using the typedef keyword in C/C++)
    class StringLookup<T> : Dictionary<string, T> { }
    interface IIntegerSet<T> : ISet<int> { }

    class Conditional                            // Compliant - it's not empty when the given symbol is defined
    {
#if NOTDEFINED
    public override string ToString()
    {
        return "Debug Text";
    }
#endif
    }
}

namespace NonCompliant
{
    class Empty { }                              // Noncompliant {{Remove this empty class, write its code or make it an "interface".}}
    //    ^^^^^

    public class PublicEmpty { }                 // Noncompliant

    internal class InternalEmpty { }             // Noncompliant

    class EmptyWithComments                      // Noncompliant
    {
        // Some comment
    }

    static class StaticEmpty { }                 // Noncompliant

    abstract class AbstractEmpty { }             // Noncompliant

    class GenericEmpty<T> { }                    // Noncompliant
    //    ^^^^^^^^^^^^
    class GenericEmptyWithConstraints<T>         // Noncompliant
        where T : class { }
}

namespace Ignore
{
    class { }                                    // Error

    []                                           // Error
    class AttributeError { }

    interface IMarker { }                        // Compliant - this rule only deals with classes
    
    class ImplementsMarker: IMarker { }          // Compliant - implements a marker interface

    struct EmptyStruct { }                       // Compliant - this rule only deals with classes

    enum EmptyEnum { }                           // Compliant - this rule only deals with classes
}
