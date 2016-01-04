﻿Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Scripting.InputHandler

Imports Variable = System.Collections.Generic.KeyValuePair(Of String, Object)

Namespace Runtime.MMU

    ''' <summary>
    ''' 字符串服务
    ''' </summary>
    Public Class Strings : Inherits Runtime.SCOM.RuntimeComponent

        Sub New(ScriptEngine As Runtime.ScriptEngine)
            Call MyBase.New(ScriptEngine)
        End Sub

        Const Variable As String = "(^|\\)?\$[^$^\\]+"
        Const Constant As String = "(^|\\)?&[^&^\\]+"

        ''' <summary>
        ''' 常量是区分大小写的
        ''' </summary>
        ''' <param name="Expr"></param>
        ''' <returns></returns>
        Public Function Format(Expr As String) As String
            Dim Matches = (From m As Match
                               In Regex.Matches(Expr, Variable, RegexOptions.Multiline)
                           Select m.Value).ToArray
            Dim ExprBuilder As StringBuilder = New StringBuilder(Expr)

            For Each var As KeyValuePair(Of String, Object) In (From varRef In MyBase.ScriptEngine.MMUDevice.ImportedConstants
                                                                Select varRef
                                                                Order By VisualBasic.Strings.Len(varRef.Key) Descending).ToArray
                Call ExprBuilder.Replace(var.Key, InputHandler.ToString(var.Value))
            Next

            If Matches.IsNullOrEmpty Then  '没有任何匹配，说明仅仅是一个字符串常量
                Return Expr
            End If

            Dim OriginalTokens = (From i As Integer In Matches.Sequence
                                  Let strValue As String = Matches(i)
                                  Where strValue.First = "\"c
                                  Select i, strValue).ToArray
            Dim EscapeTokens As List(Of KeyValuePair(Of String, String)) = New Generic.List(Of KeyValuePair(Of String, String))

            For Each OriginalToken In OriginalTokens '先处理需要被转义的部分
                Dim ESC As String = OriginalToken.i & "___//-$"
                Call EscapeTokens.Add(New KeyValuePair(Of String, String)(ESC, OriginalToken.strValue))
                Call ExprBuilder.Replace(OriginalToken.strValue, ESC)
            Next

            Dim ReplacedTokens = (From m As String In Matches Where m.First <> "\"c Select m).ToArray
            Dim vars = ScriptEngine.MMUDevice.Variables

            For Each Token As String In ReplacedTokens
                Call __replaceString(Token, ExprBuilder, vars)
            Next

            For Each ESC In EscapeTokens   '替换回转义字符
                Call ExprBuilder.Replace(ESC.Key, Mid(ESC.Value, 2))
            Next

            Return ExprBuilder.ToString
        End Function

        Private Sub __replaceString(Token As String, ByRef ExprBuilder As StringBuilder, varList As KeyValuePair(Of String, Object)())
            Dim Variable = (From varEntry In varList
                            Where InStr(Token, varEntry.Key, CompareMethod.Text) = 1
                            Select varEntry
                            Order By Len(varEntry.Key) Descending).ToArray

            If Variable.IsNullOrEmpty Then Return

            Dim var = Variable(Scan0)
            Token = Mid(Token, 1, Len(var.Key)) '主要是因为大小写的问题，所以需要原始的等长字符串来进行替换
            Call ExprBuilder.Replace(Token, InputHandler.ToString(var.Value))
        End Sub
    End Class
End Namespace