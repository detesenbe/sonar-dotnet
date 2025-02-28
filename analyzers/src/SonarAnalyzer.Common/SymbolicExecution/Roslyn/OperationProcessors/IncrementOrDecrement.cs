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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class IncrementOrDecrement : SimpleProcessor<IIncrementOrDecrementOperationWrapper>
{
    protected override IIncrementOrDecrementOperationWrapper Convert(IOperation operation) =>
        IIncrementOrDecrementOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IIncrementOrDecrementOperationWrapper incrementOrDecrement)
    {
        if (context.State[incrementOrDecrement.Target]?.Constraint<NumberConstraint>() is { } oldNumber)
        {
            var newNumber = incrementOrDecrement.WrappedOperation.Kind == OperationKindEx.Increment
                ? NumberConstraint.From(oldNumber.Min + 1, oldNumber.Max + 1)
                : NumberConstraint.From(oldNumber.Min - 1, oldNumber.Max - 1);
            var state = incrementOrDecrement.Target.TrackedSymbol() is { } symbol
                ? context.SetSymbolConstraint(symbol, newNumber)
                : context.State;
            return incrementOrDecrement.IsPostfix
                ? state.SetOperationConstraint(context.Operation, oldNumber)
                : state.SetOperationConstraint(context.Operation, newNumber);
        }
        else
        {
            return context.State;
        }
    }
}
