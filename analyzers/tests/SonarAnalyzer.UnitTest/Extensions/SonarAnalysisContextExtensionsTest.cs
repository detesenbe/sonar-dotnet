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

extern alias csharp;
extern alias vbnet;

using Moq;
using SonarAnalyzer.AnalysisContext;
using CS = csharp::SonarAnalyzer.Extensions.SonarAnalysisContextExtensions;
using VB = vbnet::SonarAnalyzer.Extensions.SonarAnalysisContextExtensions;

namespace SonarAnalyzer.UnitTest.Extensions;

[TestClass]
public class SonarAnalysisContextExtensions
{
    private static readonly DiagnosticDescriptor DummyMainDescriptor = AnalysisScaffolding.CreateDescriptorMain();

    [DataTestMethod]
    [DataRow("// <auto-generated/>", false)]
    [DataRow("// any random comment", true)]
    public void ReportIssue_SonarSymbolAnalysisContext_CS(string comment, bool expected)
    {
        var (tree, model) = TestHelper.CompileCS($$"""
                {{comment}}
                public class Sample {}
                """);
        var wasReported = false;
        var symbolContext = new SymbolAnalysisContext(Mock.Of<ISymbol>(), model.Compilation, AnalysisScaffolding.CreateOptions(), _ => wasReported = true, _ => true, default);
        var context = new SonarSymbolReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), symbolContext);
        CS.ReportIssue(context, Diagnostic.Create(DummyMainDescriptor, tree.GetRoot().GetLocation()));

        wasReported.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("' <auto-generated/>", false)]
    [DataRow("' any random comment", true)]
    public void ReportIssue_SonarSymbolAnalysisContext_VB(string comment, bool expected)
    {
        var (tree, model) = TestHelper.CompileVB($"""
                {comment}
                Public Class Sample
                End Class
                """);
        var wasReported = false;
        var symbolContext = new SymbolAnalysisContext(Mock.Of<ISymbol>(), model.Compilation, AnalysisScaffolding.CreateOptions(), _ => wasReported = true, _ => true, default);
        var context = new SonarSymbolReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), symbolContext);
        VB.ReportIssue(context, Diagnostic.Create(DummyMainDescriptor, tree.GetRoot().GetLocation()));

        wasReported.Should().Be(expected);
    }
}
