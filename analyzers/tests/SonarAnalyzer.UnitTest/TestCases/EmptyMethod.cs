﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        void F2()
        {
            // Do nothing because of X and Y.
        }

        void F3()
        {
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        void F4()    // Noncompliant {{Add a nested comment explaining why this method is empty, throw a 'NotSupportedException' or complete the implementation.}}
        {
        }

        public void ConditionalCompilation()
        {
#if SomeThing
            Console.WriteLine();
#endif
        }

        public void ConditionalCompilationEmpty() // Noncompliant
        {
#if SomeThing
#endif
        }

        public void EmptyRegionTrivia() // Noncompliant
        {
            #region
            #endregion
        }

        protected virtual void F5()
        {
        }

        extern void F6();

        [DllImport("avifil32.dll")]
        private static extern void F7();
    }

    public abstract class MyClass
    {
        public void F1() { } // Noncompliant
//                  ^^

        public abstract void F2();
    }

    public class MyClass5 : MyClass
    {
        public override void F2()
        {
        }
    }

    public interface IInterface
    {
        public void F1() { } // Noncompliant
//                  ^^

        public virtual void F2() { }

        public abstract void F3();
    }

    public class WithProp
    {
        public string Prop
        {
            set { } // FN https://github.com/SonarSource/sonar-dotnet/issues/3753
        }
    }
}
