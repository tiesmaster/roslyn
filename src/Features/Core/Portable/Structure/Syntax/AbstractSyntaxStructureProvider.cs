﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Structure;

internal abstract class AbstractSyntaxStructureProvider
{
    public abstract void CollectBlockSpans(
        SyntaxToken previousToken,
        SyntaxNode node,
        ArrayBuilder<BlockSpan> spans,
        BlockStructureOptions options,
        CancellationToken cancellationToken);

    public abstract void CollectBlockSpans(
        SyntaxTrivia trivia,
        ArrayBuilder<BlockSpan> spans,
        BlockStructureOptions options,
        CancellationToken cancellationToken);
}
