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

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class OptionalParameterWithDefaultValueCodeFix : SonarCodeFix
    {
        private const string Title = "Change to '[DefaultParameterValue]'";
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(OptionalParameterWithDefaultValue.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var attribute = root.FindNode(diagnosticSpan) as AttributeSyntax;

            if (attribute?.ArgumentList == null ||
                attribute.ArgumentList.Arguments.Count != 1)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

            var defaultParameterValueAttributeType = semanticModel?.Compilation.GetTypeByMetadataName(KnownType.System_Runtime_InteropServices_DefaultParameterValueAttribute);
            if (defaultParameterValueAttributeType == null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    _ =>
                    {
                        var attributeName = defaultParameterValueAttributeType.ToMinimalDisplayString(semanticModel, attribute.SpanStart);
                        attributeName = attributeName.Remove(attributeName.IndexOf("Attribute", System.StringComparison.Ordinal));

                        var newAttribute = attribute.WithName(SyntaxFactory.ParseName(attributeName));
                        var newRoot = root.ReplaceNode(attribute, newAttribute);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }
    }
}

