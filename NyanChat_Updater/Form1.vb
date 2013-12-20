Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RichTextBox1.Rtf = System.IO.File.ReadAllText("\\172.26.0.21\Volume_2\SOM\Programas vistaero\NyanChat\Changelog.rtf")
        Const nuevaversion As String = "\\172.26.0.21\Volume_2\SOM\Programas vistaero\NyanChat\NyanChat.exe"
        Dim versionactual As String = ""
        System.IO.File.Copy(nuevaversion, versionactual)
    End Sub
End Class
