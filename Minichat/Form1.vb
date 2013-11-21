Imports System.IO

Public Class Form1

    Private DirTexto As String
    Dim sw As StreamWriter

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim fileDialog As New OpenFileDialog
        fileDialog.Filter = "Archivos de texto plano|*.txt"
        If fileDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            DirTexto = fileDialog.FileName
        End If
        Try
            TextBox1.Text = System.IO.File.ReadAllText(DirTexto)
        Catch ex As Exception
            NotifyIcon1.Visible = False
            Environment.Exit(0)
        End Try
        Timer1.Enabled = True
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Try
            TextBox1.Text = System.IO.File.ReadAllText(DirTexto)
        Catch ex As Exception
            If Not System.IO.File.Exists(DirTexto) Then
                Environment.Exit(0)
            End If
        End Try
        AjustarTexto()
    End Sub

    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox2.KeyDown
        Dim enviado As Boolean
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            Do Until enviado = True
                Try
                    sw = File.AppendText(DirTexto)
                    sw.WriteLine(TextBox2.Text)
                    enviado = True
                Catch ex As Exception
                    enviado = False
                Finally
                    sw.Flush()
                    sw.Close()
                End Try
            Loop
            TextBox2.Clear()
        End If
        If e.KeyCode = Keys.Escape Then
            System.IO.File.Delete(DirTexto)
            Environment.Exit(0)
        End If
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Escape Then
            System.IO.File.Delete(DirTexto)
            Environment.Exit(0)
        End If
    End Sub

    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        TextBox1.Clear()
        System.IO.File.WriteAllText(DirTexto, "")
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Private Sub AjustarTexto()
        TextBox1.SelectionStart = TextBox1.Text.Length
        TextBox1.ScrollToCaret()
    End Sub
End Class
