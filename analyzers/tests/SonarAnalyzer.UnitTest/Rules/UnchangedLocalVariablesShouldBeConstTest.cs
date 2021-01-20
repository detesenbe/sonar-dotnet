﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnchangedLocalVariablesShouldBeConstTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UnchangedLocalVariablesShouldBeConst() =>
            Verifier.VerifyAnalyzer(@"TestCases\UnchangedLocalVariablesShouldBeConst.cs", new CS.UnchangedLocalVariablesShouldBeConst());

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void UnchangedLocalVariablesShouldBeConst_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\UnchangedLocalVariablesShouldBeConst.CSharp9.cs", new CS.UnchangedLocalVariablesShouldBeConst());
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void UnchangedLocalVariablesShouldBeConst_InvalidCode() =>
            Verifier.VerifyCSharpAnalyzer(@"
// invalid code
public void Test_TypeThatCannotBeConst(int arg)
{
    System.Random random = 1;
}

// invalid code
public void (int arg)
{
    int intVar = 1; // Noncompliant
}", new CS.UnchangedLocalVariablesShouldBeConst(), CompilationErrorBehavior.Ignore);
    }
}
