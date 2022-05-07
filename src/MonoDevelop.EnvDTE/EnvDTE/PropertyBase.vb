'
' PropertyBase.cs
'
' Author:
'       Matt Ward <matt.ward@microsoft.com>
'
' Copyright (c) 2022 Microsoft
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy of this
' software and associated documentation files (the "Software"), to deal in the Software
' without restriction, including without limitation the rights to use, copy, modify, merge,
' publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
' to whom the Software is furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all copies or
' substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
' INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
' PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
' FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
' OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.

Imports System

Namespace MonoDevelop.EnvDTE
	Public MustInherit Class PropertyBase
		Inherits MarshalByRefObject

		Property IndexedValue(
			Index1 As Object,
			Optional Index2 As Object = Nothing,
			Optional Index3 As Object = Nothing,
			Optional Index4 As Object = Nothing) As Object
			Get
				Return GetIndexedValue(Index1, Index2, Index3, Index4)
			End Get
			Set(Value As Object)
				SetIndexedValue(Value, Index1, Index2, Index3, Index4)
			End Set
		End Property

		Protected MustOverride Function GetIndexedValue(Index1 As Object, Index2 As Object, Index3 As Object, Index4 As Object) As Object

		Protected MustOverride Sub SetIndexedValue(Value As Object, Index1 As Object, Index2 As Object, Index3 As Object, Index4 As Object)

	End Class
End Namespace