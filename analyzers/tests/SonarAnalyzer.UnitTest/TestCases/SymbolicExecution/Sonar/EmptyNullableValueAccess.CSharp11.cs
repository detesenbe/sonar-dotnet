﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.TestCases.SymbolicExecution.Sonar
{
    public class EmptyNullableValueAccess
    {
        [Obsolete(nameof(argument.Value))]  // Compliant
        public void ExtendedScopeNameOfInAttribute(int? argument)
        {
            int? nullArgument = null;
            var shouldThrow = nullArgument.Value; // Noncompliant
        }
    }
}

