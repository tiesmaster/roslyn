﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles
Imports Microsoft.CodeAnalysis.NamingStyles

Namespace Microsoft.CodeAnalysis.Editor.Implementation.Diagnostics.UnitTests
    Partial Public Class NamingStyleTests
        Private Shared Function CreateNamingStyle(
            Optional prefix As String = "",
            Optional suffix As String = "",
            Optional wordSeparator As String = "",
            Optional capitalizationScheme As Capitalization = Capitalization.PascalCase) As NamingStyle

            Return New NamingStyle With
                {
                    .prefix = prefix,
                    .suffix = suffix,
                    .wordSeparator = wordSeparator,
                    .capitalizationScheme = capitalizationScheme
                }
        End Function

        Private Shared Sub TestNameCreation(namingStyle As NamingStyle, expectedName As String, ParamArray words As String())
            Assert.Equal(expectedName, namingStyle.CreateName(words.ToImmutableArray()))
        End Sub

        Private Shared Sub TestNameCompliance(namingStyle As NamingStyle, candidateName As String)
            Dim reason As String = Nothing
            Assert.True(namingStyle.IsNameCompliant(candidateName, reason))
        End Sub

        Private Shared Sub TestNameNoncomplianceAndFixedNames(namingStyle As NamingStyle, candidateName As String, ParamArray expectedFixedNames As String())
            Dim reason As String = Nothing
            Assert.False(namingStyle.IsNameCompliant(candidateName, reason))

            Dim actualFixedNames = namingStyle.MakeCompliant(candidateName).ToList()

            Assert.Equal(expectedFixedNames.Length, actualFixedNames.Count)
            For i = 0 To expectedFixedNames.Length - 1
                Assert.Equal(expectedFixedNames(i), actualFixedNames(i))
            Next
        End Sub
    End Class
End Namespace
