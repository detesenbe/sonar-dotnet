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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StaticFieldInitializerOrderTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<StaticFieldInitializerOrder>();

        [TestMethod]
        public void StaticFieldInitializerOrder() =>
            builder.AddPaths("StaticFieldInitializerOrder.cs", "StaticFieldInitializerOrder_PartialClass.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .Verify();

#if NET

        [TestMethod]
        public void StaticFieldInitializerOrder_CSharp9() =>
            builder.AddPaths("StaticFieldInitializerOrder.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void StaticFieldInitializerOrder_CSharp10() =>
            builder.AddPaths("StaticFieldInitializerOrder.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}
