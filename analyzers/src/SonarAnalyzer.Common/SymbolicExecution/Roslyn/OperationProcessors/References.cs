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

internal sealed class InstanceReference : ISimpleProcessor
{
    public ProgramState Process(SymbolicContext context) =>
        context.SetOperationValue(SymbolicValue.NotNull);     // Implicit and Explicit
}

internal sealed class LocalReference : SimpleProcessor<ILocalReferenceOperationWrapper>
{
    protected override ILocalReferenceOperationWrapper Convert(IOperation operation) =>
        ILocalReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, ILocalReferenceOperationWrapper localReference) =>
        context.State[localReference.Local] is { } value
            ? context.SetOperationValue(value)
            : context.State;
}

internal sealed class ParameterReference : SimpleProcessor<IParameterReferenceOperationWrapper>
{
    protected override IParameterReferenceOperationWrapper Convert(IOperation operation) =>
        IParameterReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IParameterReferenceOperationWrapper parameterReference) =>
        context.State[parameterReference.Parameter] is { } value
            ? context.SetOperationValue(value)
            : context.State;
}

internal sealed class FieldReference : SimpleProcessor<IFieldReferenceOperationWrapper>
{
    protected override IFieldReferenceOperationWrapper Convert(IOperation operation) =>
        IFieldReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IFieldReferenceOperationWrapper fieldReference)
    {
        var state = fieldReference.WrappedOperation.TrackedSymbol() is { } fieldSymbol && context.State[fieldSymbol] is { } value
            ? context.SetOperationValue(value)
            : context.State;
        return fieldReference.Instance.TrackedSymbol() is { } instanceSymbol
            ? state.SetSymbolConstraint(instanceSymbol, ObjectConstraint.NotNull)
            : state;
    }
}

internal sealed class PropertyReference : BranchingProcessor<IPropertyReferenceOperationWrapper>
{
    protected override IPropertyReferenceOperationWrapper Convert(IOperation operation) =>
        IPropertyReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState PreProcess(ProgramState state, IPropertyReferenceOperationWrapper operation)
    {
        if (operation.Instance.TrackedSymbol() is { } symbol)
        {
            if (!IsNullableProperty(operation, "HasValue"))
            {
                state = state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull);
            }
            if (IsNullableProperty(operation, "Value") && state[symbol] is { } value)
            {
                state = state.SetOperationValue(operation, value);
            }
        }
        return state;
    }

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IPropertyReferenceOperationWrapper operation, bool isLoopCondition, int visitCount) =>
        IsNullableProperty(operation, "HasValue") && state[operation.Instance]?.Constraint<ObjectConstraint>() is { } objectConstraint
            ? BoolConstraint.From(objectConstraint == ObjectConstraint.NotNull)
            : null;

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IPropertyReferenceOperationWrapper operation, bool isLoopCondition, int visitCount, bool falseBranch) =>
        IsNullableProperty(operation, "HasValue") && operation.Instance.TrackedSymbol() is { } testedSymbol
            // Can't use ObjectConstraint.ApplyOpposite() because here, we are sure that it is either Null or NotNull
            ? state.SetSymbolConstraint(testedSymbol, falseBranch ? ObjectConstraint.Null : ObjectConstraint.NotNull)
            : state;

    private static bool IsNullableProperty(IPropertyReferenceOperationWrapper operation, string name) =>
        operation.Instance is not null && operation.Instance.Type.IsNullableValueType() && operation.Property.Name == name;
}

internal sealed class ArrayElementReference : SimpleProcessor<IArrayElementReferenceOperationWrapper>
{
    protected override IArrayElementReferenceOperationWrapper Convert(IOperation operation) =>
        IArrayElementReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IArrayElementReferenceOperationWrapper arrayElementReference) =>
        arrayElementReference.ArrayReference.TrackedSymbol() is { } symbol
            ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
            : context.State;
}

internal sealed class EventReference : SimpleProcessor<IEventReferenceOperationWrapper>
{
    protected override IEventReferenceOperationWrapper Convert(IOperation operation) =>
        IEventReferenceOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IEventReferenceOperationWrapper eventReference) =>
        eventReference.Instance.TrackedSymbol() is { } symbol
            ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
            : context.State;
}
