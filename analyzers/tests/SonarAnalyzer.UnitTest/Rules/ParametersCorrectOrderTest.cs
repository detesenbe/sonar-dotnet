﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ParametersCorrectOrderTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ParametersCorrectOrder>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ParametersCorrectOrder>();

        [TestMethod]
        public void ParametersCorrectOrder_CSharp8() =>
            builderCS.AddPaths("ParametersCorrectOrder.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void ParametersCorrectOrder_CSharp11() =>
            builderCS.AddPaths("ParametersCorrectOrder.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

#endif

        [TestMethod]
        public void ParametersCorrectOrder_InvalidCode_CS() =>
            builderCS.AddSnippet(@"
public class Foo
{
    public void Bar()
    {
        new Foo
        new ()
        new System. ()
    }
}").WithErrorBehavior(CompilationErrorBehavior.Ignore).Verify();

        [TestMethod]
        public void ParametersCorrectOrder_VB() =>
            builderVB.AddPaths("ParametersCorrectOrder.vb").Verify();

        [TestMethod]
        public void ParametersCorrectOrder_InvalidCode_VB() =>
            builderVB.AddSnippet(@"
Public Class Foo
    Public Sub Bar()
        Dim x = New ()
        Dim y = New System. ()
    End Sub
End Class").WithErrorBehavior(CompilationErrorBehavior.Ignore).Verify();
    }
}
