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
    public sealed class InsecureEncryptionAlgorithm : InsecureEncryptionAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ArgumentListSyntax, ArgumentSyntax>
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override ArgumentListSyntax ArgumentList(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.ArgumentList;

        protected override SeparatedSyntaxList<ArgumentSyntax> Arguments(ArgumentListSyntax argumentList) =>
            argumentList.Arguments;

        protected override bool IsStringLiteralArgument(ArgumentSyntax argument) =>
            argument.Expression.IsKind(SyntaxKind.StringLiteralExpression);

        protected override string StringLiteralValue(ArgumentSyntax argument) =>
            ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;

        protected override Location Location(SyntaxNode objectCreation) =>
            objectCreation is ObjectCreationExpressionSyntax objectCreationExpression
                ? objectCreationExpression.Type.GetLocation()
                : objectCreation.GetLocation();
    }
}
