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

namespace SonarAnalyzer.Rules;

public abstract class MultipleVariableDeclarationCodeFixBase : SonarCodeFix
{
    internal const string Title = "Separate declarations";

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MultipleVariableDeclarationConstants.DiagnosticId);

    protected sealed override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                c =>
                {
                    var newRoot = CalculateNewRoot(root, node);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                Title),
            context.Diagnostics);

        return Task.CompletedTask;
    }

    protected abstract SyntaxNode CalculateNewRoot(SyntaxNode root, SyntaxNode node);
}
