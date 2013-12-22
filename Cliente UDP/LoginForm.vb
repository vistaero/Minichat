Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Forms


Public Class LoginForm

    Inherits Form
    Public clientSocket As Socket
    Public epServer As EndPoint
    Public strName As String

    Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Main()

        CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click


        strName = txtName.Text

        Try
            'Using UDP sockets
            clientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

            'IP address of the server machine
            Dim ipAddress__1 As IPAddress = IPAddress.Parse(txtServerIP.Text)
            'Server is listening on port 1000
            Dim ipEndPoint As New IPEndPoint(ipAddress__1, 1000)

            epServer = DirectCast(ipEndPoint, EndPoint)

            Dim msgToSend As New Data()
            msgToSend.cmdCommand = Command.Login
            msgToSend.strMessage = Nothing
            msgToSend.strName = strName

            Dim byteData As Byte() = msgToSend.ToByte()

            'Login to the server
            clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, New AsyncCallback(AddressOf OnSend), _
                Nothing)

            Dim sgsClientForm As New SGSClient()
            sgsClientForm.clientSocket = Me.clientSocket
            sgsClientForm.strName = Me.strName
            sgsClientForm.epServer = Me.epServer

            sgsClientForm.ShowDialog()


        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try

        

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Close()

    End Sub

    Private Sub OnSend(ar As IAsyncResult)
        Try
            clientSocket.EndSend(ar)
            strName = txtName.Text
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub txtName_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
        If txtName.Text.Length > 0 AndAlso txtServerIP.Text.Length > 0 Then
            btnOK.Enabled = True
        Else
            btnOK.Enabled = False
        End If
    End Sub

    Private Sub txtServerIP_TextChanged(sender As Object, e As EventArgs) Handles txtServerIP.TextChanged
        If txtName.Text.Length > 0 AndAlso txtServerIP.Text.Length > 0 Then
            btnOK.Enabled = True
        Else
            btnOK.Enabled = False
        End If
    End Sub

    Private Shared Sub Main()
        
    End Sub


End Class

