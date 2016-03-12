﻿' Copyright (c) Microsoft Corporation
' All rights reserved

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.Composition
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Interpreter.LDM.Expressions
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Tagging
Imports Microsoft.VisualStudio.Utilities

Namespace OokLanguage

    <Export(GetType(ITaggerProvider)),
    ContentType("ook!"),
    TagType(GetType(ClassificationTag))>
    Friend NotInheritable Class OokClassifierProvider
        Implements ITaggerProvider

        <Export(), Name("ook!"), BaseDefinition("code")>
        Friend Shared OokContentType As ContentTypeDefinition = Nothing

        <Export(), FileExtension(".shl"), ContentType("ook!")>
        Friend Shared OokFileType As FileExtensionToContentTypeDefinition = Nothing

        <Import()>
        Friend ClassificationTypeRegistry As IClassificationTypeRegistryService = Nothing

        <Import()>
        Friend aggregatorFactory As IBufferTagAggregatorFactoryService = Nothing

        Public Function CreateTagger(Of T As ITag)(ByVal buffer As ITextBuffer) As ITagger(Of T) Implements ITaggerProvider.CreateTagger
            Dim ookTagAggregator As ITagAggregator(Of OokTokenTag) = aggregatorFactory.CreateTagAggregator(Of OokTokenTag)(buffer)
            Dim cls As Object = New OokClassifier(buffer, ookTagAggregator, ClassificationTypeRegistry)
            Dim tagger As ITagger(Of T) = TryCast(cls, ITagger(Of T))
            Return tagger
        End Function

    End Class

    Friend NotInheritable Class OokClassifier : Implements ITagger(Of ClassificationTag)

        Private _buffer As ITextBuffer
        Private _aggregator As ITagAggregator(Of OokTokenTag)
        Private _ookTypes As IDictionary(Of ExpressionTypes, IClassificationType)

        Friend Sub New(ByVal buffer As ITextBuffer, ByVal ookTagAggregator As ITagAggregator(Of OokTokenTag), ByVal typeService As IClassificationTypeRegistryService)
            _buffer = buffer
            _aggregator = ookTagAggregator
            _ookTypes = New Dictionary(Of ExpressionTypes, IClassificationType)
            _ookTypes(ExpressionTypes.Die) = typeService.GetClassificationType("die")
            _ookTypes(ExpressionTypes.CD) = typeService.GetClassificationType("cd")
            _ookTypes(ExpressionTypes.If) = typeService.GetClassificationType("if")
            _ookTypes(ExpressionTypes.GoTo) = typeService.GetClassificationType("goto")
        End Sub

        Public Function GetTags(ByVal spans As NormalizedSnapshotSpanCollection) As IEnumerable(Of ITagSpan(Of ClassificationTag)) Implements ITagger(Of ClassificationTag).GetTags
            Dim tags As New List(Of TagSpan(Of ClassificationTag))

            For Each tagSpan In Me._aggregator.GetTags(spans)
                Dim tagSpans = tagSpan.Span.GetSpans(spans(0).Snapshot)
                tags.Add(New TagSpan(Of ClassificationTag)(tagSpans(0), New ClassificationTag(_ookTypes(tagSpan.Tag.type))))
            Next tagSpan

            Return tags
        End Function

        Public Event TagsChanged(ByVal sender As Object, ByVal e As Microsoft.VisualStudio.Text.SnapshotSpanEventArgs) Implements Microsoft.VisualStudio.Text.Tagging.ITagger(Of Microsoft.VisualStudio.Text.Tagging.ClassificationTag).TagsChanged

    End Class

End Namespace
