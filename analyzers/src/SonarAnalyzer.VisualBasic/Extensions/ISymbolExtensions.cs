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

namespace SonarAnalyzer.Extensions
{
    internal static class ISymbolExtensions
    {
        public static IEnumerable<SyntaxNode> GetLocationNodes(this ISymbol symbol, SyntaxNode node) =>
            symbol.Locations.SelectMany(location => GetDescendantNodes(location, node));

        public static IEnumerable<SyntaxNode> GetDescendantNodes(Location location, SyntaxNode node) =>
            location.SourceTree?.GetRoot() is { } locationRootNode
            && locationRootNode == node.SyntaxTree.GetRoot()
            && locationRootNode.FindNode(location.SourceSpan)
                               .FirstAncestorOrSelf<VariableDeclaratorSyntax>() is { } declaration
                ? declaration.DescendantNodes()
                : Enumerable.Empty<SyntaxNode>();
    }
}
