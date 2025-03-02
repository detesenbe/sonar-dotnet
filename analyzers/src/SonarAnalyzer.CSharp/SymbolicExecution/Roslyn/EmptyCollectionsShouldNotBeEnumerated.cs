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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public sealed class EmptyCollectionsShouldNotBeEnumerated : EmptyCollectionsShouldNotBeEnumeratedBase
{
    public static readonly DiagnosticDescriptor S4158 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S4158;

    public override bool ShouldExecute()
    {
        var walker = new Walker();
        walker.SafeVisit(Node);
        return walker.Result;
    }

    private sealed class Walker : SafeCSharpSyntaxWalker
    {
        public bool Result { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (!Result)
            {
                base.Visit(node);
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            Result = RaisingMethods.Contains(node.Name.GetName());
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node) =>
            Result = true;
    }
}
