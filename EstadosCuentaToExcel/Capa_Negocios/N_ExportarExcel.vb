﻿Imports Microsoft.Office.Interop
Imports Capa_Identidad

Public Class N_ExportarExcel
#Region "Variables"
    Private Archivo As Excel.Application
    Private Libro As Excel.Workbook
    Private Hoja As Excel.Worksheet
    Private Rango As Excel.Range

    Private _info As I_Archivo
    Private _campos As List(Of I_Formato_campos)
    Private _tabla As DataTable
    Private _ruta As String

#End Region
#Region "PROPIEDADES"
    Public Property Tabla As DataTable
        Get
            Return _tabla
        End Get
        Set(value As DataTable)
            _tabla = value
        End Set
    End Property

    Public Property Ruta As String
        Get
            Return _ruta
        End Get
        Set(value As String)
            _ruta = value
        End Set
    End Property

    Public Property Campos As List(Of I_Formato_campos)
        Get
            Return _campos
        End Get
        Set(value As List(Of I_Formato_campos))
            _campos = value
        End Set
    End Property

    Public Property Info As I_Archivo
        Get
            Return _info
        End Get
        Set(value As I_Archivo)
            _info = value
        End Set
    End Property

#End Region
#Region "Constructor"
    Public Sub New()
    End Sub
    Public Sub New(ByVal datos As DataTable, ByVal campos_ As List(Of I_Formato_campos), ByVal ruta_ As String, ByVal info_ As I_Archivo)
        Tabla = datos
        Ruta = ruta_
        Campos = campos_
        Exportar(Tabla, Campos, _ruta, info_)
    End Sub
#End Region
#Region "Funciones"
    '----- EXPORTACION MAIN ----------------------------------------
    Public Function Exportar(ByVal datos As DataTable, ByVal campos_ As List(Of I_Formato_campos), ByVal ruta_ As String, ByVal info_ As I_Archivo) As Boolean
        Tabla = datos
        Ruta = ruta_
        Campos = campos_
        Info = info_

        ExcelHeader()
        ExcelBody()
        ExcelFormato()
        ExcelGuardar()

        Return True
    End Function

    '----- CABECERA ------------------------------------------------
    Private Sub ExcelHeader()
        Dim i As Integer = 0
        Dim tipo As String

        Try
            ' Creamos todo lo necesario para un excel
            Archivo = CreateObject("Excel.Application")
            Archivo.Visible = False 'Para que no se muestre mientras se crea
            Libro = Archivo.Workbooks.Add
            Hoja = Libro.ActiveSheet

            'NOMBRE DE COLUMNAS
            Rango = Hoja.Range("a1")
            Rango.EntireRow.Font.Bold = True
            Hoja.Columns.NumberFormat = "@"

            For Each columna As DataColumn In Tabla.Columns
                i += 1
                Hoja.Cells(1, i).Value = columna.ColumnName
                tipo = Campos(i - 1).Tipo
                If tipo = "Decimal" Then
                    Hoja.Columns(i).NumberFormat = "###,###,###,##0.00"
                End If
            Next

        Catch ex As Exception

        End Try
    End Sub

    '----- CUERPO -------------------------------------------------
    Private Sub ExcelBody()
        Dim tipo As String
        Dim X, Y As Integer

        Try

            Y = 2
            For Each Linea As DataRow In Tabla.Rows
                X = 0
                ' Cargamos la información en el excel

                For Each campo As Object In Linea.ItemArray
                    tipo = Campos(X).Tipo
                    X += 1
                    If tipo = "Decimal" Then
                        Try
                            Hoja.Cells(Y, X).Value = Convert.ToDecimal(campo)
                        Catch ex As Exception
                            Hoja.Cells(Y, X).Value = ""
                        End Try
                    Else
                        Hoja.Cells(Y, X).Value = Convert.ToString(campo)
                    End If
                Next
                Y += 1
            Next
        Catch ex As Exception

        End Try
    End Sub

    '----- FORMATO FINAL -----------------------------------------
    Private Sub ExcelFormato()
        Dim col, lin As Integer
        col = Tabla.Columns.Count
        lin = Tabla.Rows.Count + 1

        Try
            'ANCHO DE COLUMNAS
            Hoja.Range("A1:" & GetLetraAbc(col) & lin).Columns.AutoFit()
            Hoja.Range("B1:B" & lin).ColumnWidth = 105

            Hoja.ListObjects.AddEx(Excel.XlListObjectSourceType.xlSrcRange, Hoja.Range("A1:" & GetLetraAbc(col) & lin),, Excel.XlYesNoGuess.xlYes)
            Hoja.ListObjects("Tabla1").TableStyle = "TableStyleMedium7"

            Hoja.Range("A1:" & GetLetraAbc(col) & lin).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft
            Hoja.Range("A1:" & GetLetraAbc(col) & lin).VerticalAlignment = Excel.XlHAlign.xlHAlignCenter
        Catch ex As Exception

        End Try

    End Sub

    'GUARDAR EXCEL
    Private Sub ExcelGuardar()
        Dim nom As String
        Try
            nom = GetNombreFichero()
            ' Guardamos el excel en la ruta que ha especificado el usuario
            Libro.SaveAs(nom)
            ' Cerramos el workbook
            Archivo.Workbooks.Close()
            ' Eliminamos el objeto excel
            Archivo.Quit()
        Catch ex As Exception
            MsgBox("Error al exportar los datos a excel: " & ex.ToString, vbCritical, "Bancos")
        End Try

    End Sub

    Private Function GetNombreFichero() As String
        Dim nom As String
        Dim res As String

        nom = Info.Rfc + " - " + Format(Info.Fecha, "yyyy-MM")

        res = Ruta + "\" + nom

        Return res
    End Function

    Private Function GetLetraAbc(ByVal num As Integer) As String
        Dim Letra As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Try
            Return Letra(num - 1)
        Catch ex As Exception
            Return ""
        End Try

    End Function
#End Region

End Class
