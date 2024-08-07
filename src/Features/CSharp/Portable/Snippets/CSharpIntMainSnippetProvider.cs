﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets;

[ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class CSharpIntMainSnippetProvider() : AbstractCSharpMainMethodSnippetProvider
{
    public override string Identifier => CSharpSnippetIdentifiers.StaticIntMain;

    public override string Description => CSharpFeaturesResources.static_int_Main;

    protected override TypeSyntax GenerateReturnType(SyntaxGenerator generator)
        => (TypeSyntax)generator.TypeExpression(SpecialType.System_Int32);

    protected override IEnumerable<StatementSyntax> GenerateInnerStatements(SyntaxGenerator generator)
    {
        var returnStatement = (StatementSyntax)generator.ReturnStatement(generator.LiteralExpression(0));
        return [returnStatement];
    }

    protected override int GetTargetCaretPosition(MethodDeclarationSyntax methodDeclaration, SourceText sourceText)
    {
        var body = methodDeclaration.Body!;
        var returnStatement = body.Statements.First();

        var triviaSpan = returnStatement.GetLeadingTrivia().Span;
        var line = sourceText.Lines.GetLineFromPosition(triviaSpan.Start);
        // Getting the location at the end of the line before the newline.
        return line.Span.End;
    }

    protected override async Task<Document> AddIndentationToDocumentAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var body = methodDeclaration.Body!;
        var returnStatement = body.Statements.First();

        var syntaxFormattingOptions = await document.GetSyntaxFormattingOptionsAsync(cancellationToken).ConfigureAwait(false);
        var indentationString = CSharpSnippetHelpers.GetBlockLikeIndentationString(document, body.OpenBraceToken.SpanStart, syntaxFormattingOptions, cancellationToken);

        var updatedReturnStatement = returnStatement.WithPrependedLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, indentationString));
        var updatedRoot = root.ReplaceNode(returnStatement, updatedReturnStatement);

        return document.WithSyntaxRoot(updatedRoot);
    }
}
