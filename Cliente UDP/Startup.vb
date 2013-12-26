Imports System.Collections.Generic
Imports System.Windows.Forms

Namespace SGSclient
    NotInheritable Class Program
        Private Sub New()
        End Sub
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread> _
        Public Shared Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            Dim loginForm As New LoginForm()

            Application.Run(loginForm)
            If loginForm.DialogResult = DialogResult.OK Then
                Dim sgsClientForm As New SGSClient
                sgsClientForm.clientSocket = loginForm.clientSocket
                sgsClientForm.strName = loginForm.strName
                sgsClientForm.epServer = loginForm.epServer

                sgsClientForm.ShowDialog()
            End If

        End Sub
    End Class
End Namespace