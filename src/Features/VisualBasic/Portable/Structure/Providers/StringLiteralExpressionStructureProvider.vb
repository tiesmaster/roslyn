﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Threading
Imports Microsoft.CodeAnalysis.PooledObjects
Imports Microsoft.CodeAnalysis.Structure
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.Structure
    Friend Class StringLiteralExpressionStructureProvider
        Inherits AbstractSyntaxNodeStructureProvider(Of LiteralExpressionSyntax)

        Protected Overrides Sub CollectBlockSpans(previousToken As SyntaxToken,
                                                  node As LiteralExpressionSyntax,
                                                  spans As ArrayBuilder(Of BlockSpan),
                                                  options As BlockStructureOptions,
                                                  cancellationToken As CancellationToken)
            If node.IsKind(SyntaxKind.StringLiteralExpression) AndAlso
                Not node.ContainsDiagnostics Then
                spans.Add(New BlockSpan(
                          type:=BlockTypes.Expression,
                          isCollapsible:=True,
                          textSpan:=node.Span,
                          autoCollapse:=True,
                          isDefaultCollapsed:=False))
            End If
        End Sub
    End Class
End Namespace
