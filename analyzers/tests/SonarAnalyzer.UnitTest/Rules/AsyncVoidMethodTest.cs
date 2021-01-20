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
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class AsyncVoidMethodTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void AsyncVoidMethod() =>
            Verifier.VerifyAnalyzer(@"TestCases\AsyncVoidMethod.cs", new CS.AsyncVoidMethod());

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void AsyncVoidMethod_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\AsyncVoidMethod.CSharp9.cs", new CS.AsyncVoidMethod());
#endif

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void AsyncVoidMethod_MsTestV2(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\AsyncVoidMethod_MsTestV2.cs",
                new CS.AsyncVoidMethod(),
                NuGetMetadataReference.MSTestTestFramework(testFwkVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void AsyncVoidMethod_MsTestV1() =>
            Verifier.VerifyAnalyzer(@"TestCases\AsyncVoidMethod_MsTestV1.cs",
                new CS.AsyncVoidMethod(),
                NuGetMetadataReference.MicrosoftVisualStudioQualityToolsUnitTestFramework);
    }
}
