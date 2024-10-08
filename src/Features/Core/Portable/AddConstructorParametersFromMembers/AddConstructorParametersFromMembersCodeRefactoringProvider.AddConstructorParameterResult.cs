﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.AddConstructorParametersFromMembers;

internal sealed partial class AddConstructorParametersFromMembersCodeRefactoringProvider
{
    private readonly struct AddConstructorParameterResult(
        ImmutableArray<AddConstructorParametersCodeAction> requiredParameterActions,
        ImmutableArray<AddConstructorParametersCodeAction> optionalParameterActions,
        bool useSubMenu)
    {
        internal readonly ImmutableArray<AddConstructorParametersCodeAction> RequiredParameterActions = requiredParameterActions;
        internal readonly ImmutableArray<AddConstructorParametersCodeAction> OptionalParameterActions = optionalParameterActions;
        internal readonly bool UseSubMenu = useSubMenu;
    }
}
