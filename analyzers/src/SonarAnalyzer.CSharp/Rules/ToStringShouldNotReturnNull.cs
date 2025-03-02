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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToStringShouldNotReturnNull : ToStringShouldNotReturnNullBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SyntaxKind MethodKind => SyntaxKind.MethodDeclaration;

    protected override void Initialize(SonarAnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterNodeAction(
            c => ToStringReturnsNull(c, ((MethodDeclarationSyntax)c.Node).ExpressionBody),
            SyntaxKind.MethodDeclaration);
    }

    protected override IEnumerable<SyntaxNode> Conditionals(SyntaxNode expression) =>
        expression is ConditionalExpressionSyntax conditional
        ? new SyntaxNode[] { conditional.WhenTrue, conditional.WhenFalse }
        : Array.Empty<SyntaxNode>();

    protected override bool IsLocalOrLambda(SyntaxNode node) =>
        node.IsAnyKind(SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKindEx.LocalFunctionStatement);
}
