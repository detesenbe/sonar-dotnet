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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
            {
                SyntaxKind.MethodDeclaration,
                SyntaxKindEx.LocalFunctionStatement
            };

        protected override void CheckMethod(SonarSyntaxNodeReportingContext context)
        {
            if (LocalFunctionStatementSyntaxWrapper.IsInstance(context.Node))
            {
                var wrapper = (LocalFunctionStatementSyntaxWrapper)context.Node;
                if (wrapper.Body is { } body && body.IsEmpty())
                {
                    context.ReportIssue(Diagnostic.Create(Rule, wrapper.Identifier.GetLocation()));
                }
            }
            else
            {
                var methodDeclaration = (MethodDeclarationSyntax)context.Node;

                // No need to check for ExpressionBody as arrowed methods can't be empty
                if (methodDeclaration.Body is { } body
                    && body.IsEmpty()
                    && !ShouldMethodBeExcluded(context, methodDeclaration))
                {
                    context.ReportIssue(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation()));
                }
            }
        }

        private static bool ShouldMethodBeExcluded(SonarSyntaxNodeReportingContext context, BaseMethodDeclarationSyntax methodNode) =>
            methodNode.Modifiers.Any(SyntaxKind.VirtualKeyword)
            || context.SemanticModel.GetDeclaredSymbol(methodNode) is { IsOverride: true, OverriddenMethod.IsAbstract: true }
            || (methodNode.Modifiers.Any(SyntaxKind.OverrideKeyword) && context.IsTestProject());
    }
}
