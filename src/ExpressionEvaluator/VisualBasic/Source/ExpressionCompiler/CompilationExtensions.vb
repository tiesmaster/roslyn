﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Collections.Immutable
Imports System.Collections.ObjectModel
Imports System.Reflection.Metadata
Imports System.Reflection.Metadata.Ecma335
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis.ExpressionEvaluator
Imports Microsoft.CodeAnalysis.PooledObjects
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols.Metadata.PE

Namespace Microsoft.CodeAnalysis.VisualBasic.ExpressionEvaluator

    Friend Module CompilationExtensions
        Private Function [GetType]([module] As PEModuleSymbol, typeHandle As TypeDefinitionHandle) As PENamedTypeSymbol
            Dim metadataDecoder = New MetadataDecoder([module])
            Return DirectCast(metadataDecoder.GetTypeOfToken(typeHandle), PENamedTypeSymbol)
        End Function

        <Extension>
        Friend Function [GetType](compilation As VisualBasicCompilation, moduleId As ModuleId, typeToken As Integer) As PENamedTypeSymbol
            Return [GetType](compilation.GetModule(moduleId), CType(MetadataTokens.Handle(typeToken), TypeDefinitionHandle))
        End Function

        <Extension>
        Friend Function GetSourceMethod(compilation As VisualBasicCompilation, moduleId As ModuleId, methodHandle As MethodDefinitionHandle) As PEMethodSymbol
            Dim method = GetMethod(compilation, moduleId, methodHandle)
            Dim metadataDecoder = New MetadataDecoder(DirectCast(method.ContainingModule, PEModuleSymbol))
            Dim containingType = method.ContainingType
            Dim sourceMethodName As String = Nothing
            If GeneratedNameParser.TryParseStateMachineTypeName(containingType.Name, sourceMethodName) Then
                For Each member In containingType.ContainingType.GetMembers(sourceMethodName)
                    Dim candidateMethod = TryCast(member, PEMethodSymbol)
                    If candidateMethod IsNot Nothing Then
                        Dim stateMachineTypeName As String = Nothing
                        If metadataDecoder.Module.HasStateMachineAttribute(candidateMethod.Handle, stateMachineTypeName) AndAlso
                            metadataDecoder.GetTypeSymbolForSerializedType(stateMachineTypeName).OriginalDefinition.Equals(containingType) Then
                            Return candidateMethod
                        End If
                    End If
                Next
            End If
            Return method
        End Function

        <Extension>
        Friend Function GetMethod(compilation As VisualBasicCompilation, moduleId As ModuleId, methodHandle As MethodDefinitionHandle) As PEMethodSymbol
            Dim [module] = compilation.GetModule(moduleId)
            Dim reader = [module].Module.MetadataReader
            Dim typeHandle = reader.GetMethodDefinition(methodHandle).GetDeclaringType()
            Dim type = [GetType]([module], typeHandle)
            Dim method = DirectCast(New MetadataDecoder([module], type).GetMethodSymbolForMethodDefOrMemberRef(methodHandle, type), PEMethodSymbol)
            Return method
        End Function

        <Extension>
        Friend Function GetModule(compilation As VisualBasicCompilation, moduleId As ModuleId) As PEModuleSymbol
            For Each pair In compilation.GetBoundReferenceManager().GetReferencedAssemblies()
                Dim assembly = DirectCast(pair.Value, AssemblySymbol)
                For Each [module] In assembly.Modules
                    Dim m = DirectCast([module], PEModuleSymbol)
                    Dim id = m.Module.GetModuleVersionIdOrThrow()
                    If id = moduleId.Id Then
                        Return m
                    End If
                Next
            Next

            Throw New BadMetadataModuleException(moduleId)
        End Function

        <Extension>
        Friend Function ToCompilation(metadataBlocks As ImmutableArray(Of MetadataBlock)) As VisualBasicCompilation
            Return ToCompilation(metadataBlocks, moduleId:=Nothing, MakeAssemblyReferencesKind.AllAssemblies)
        End Function

        <Extension>
        Friend Function ToCompilationReferencedModulesOnly(metadataBlocks As ImmutableArray(Of MetadataBlock), moduleId As ModuleId) As VisualBasicCompilation
            Return ToCompilation(metadataBlocks, moduleId, MakeAssemblyReferencesKind.DirectReferencesOnly)
        End Function

        <Extension>
        Friend Function ToCompilation(metadataBlocks As ImmutableArray(Of MetadataBlock), moduleId As ModuleId, kind As MakeAssemblyReferencesKind) As VisualBasicCompilation
            Dim referencesBySimpleName As IReadOnlyDictionary(Of String, ImmutableArray(Of (AssemblyIdentity, MetadataReference))) = Nothing
            Dim references = metadataBlocks.MakeAssemblyReferences(moduleId, IdentityComparer, kind, referencesBySimpleName)
            Dim options = s_compilationOptions
            If referencesBySimpleName IsNot Nothing Then
                Debug.Assert(kind = MakeAssemblyReferencesKind.AllReferences)
                Dim resolver = New EEMetadataReferenceResolver(IdentityComparer, referencesBySimpleName)
                options = options.WithMetadataReferenceResolver(resolver)
            End If
            Return VisualBasicCompilation.Create(
                assemblyName:=ExpressionCompilerUtilities.GenerateUniqueName(),
                references:=references,
                options:=options)
        End Function

        <Extension>
        Friend Function GetCustomTypeInfoPayload(compilation As VisualBasicCompilation, type As TypeSymbol) As ReadOnlyCollection(Of Byte)
            Dim builder = ArrayBuilder(Of String).GetInstance()
            Dim names = If(VisualBasicCompilation.TupleNamesEncoder.TryGetNames(type, builder) AndAlso compilation.HasTupleNamesAttributes,
                New ReadOnlyCollection(Of String)(builder.ToArray()),
                Nothing)
            builder.Free()
            Return CustomTypeInfo.Encode(Nothing, names)
        End Function

        Friend ReadOnly IdentityComparer As AssemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default

        ' XML file references, #r directives not supported:
        Private ReadOnly s_compilationOptions As VisualBasicCompilationOptions = New VisualBasicCompilationOptions(
            outputKind:=OutputKind.DynamicallyLinkedLibrary,
            platform:=Platform.AnyCpu, ' Platform should match PEModule.Machine, in this case I386.
            optimizationLevel:=OptimizationLevel.Release,
            assemblyIdentityComparer:=IdentityComparer).
            WithMetadataImportOptions(MetadataImportOptions.All).
            WithReferencesSupersedeLowerVersions(True).
            WithSuppressEmbeddedDeclarations(True).
            WithIgnoreCorLibraryDuplicatedTypes(True)

    End Module

End Namespace
