Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports System.Net
Imports System.Net.Sockets

'The commands for interaction between the server and the client
Enum Command
    Login
    'Log into the server
    Logout
    'Logout of the server
    Message
    'Send a text message to all the chat clients
    List
    'Get a list of users in the chat room from the server
    Null
    'No command
End Enum

Partial Public Class SGSClient
    Inherits Form
    Public clientSocket As Socket
    'The main client socket
    Public strName As String
    'Name by which the user logs into the room
    Public epServer As EndPoint
    'The EndPoint of the server
    Private byteData As Byte() = New Byte(1023) {}

    'Broadcast the message typed by the user to everyone
    Private Sub btnSend_Click(sender As Object, e As EventArgs)
        Try
            'Fill the info for the message to be send
            Dim msgToSend As New Data()

            msgToSend.strName = strName
            msgToSend.strMessage = txtMessage.Text
            msgToSend.cmdCommand = Command.Message

            Dim byteData As Byte() = msgToSend.ToByte()

            'Send it to the server
            clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, New AsyncCallback(AddressOf OnSend), _
                Nothing)

            txtMessage.Text = Nothing
        Catch generatedExceptionName As Exception
            MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " & strName, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub OnSend(ar As IAsyncResult)
        Try
            clientSocket.EndSend(ar)
        Catch generatedExceptionName As ObjectDisposedException
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSclient: " & strName, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub OnReceive(ar As IAsyncResult)
        Try
            clientSocket.EndReceive(ar)

            'Convert the bytes received into an object of type Data
            Dim msgReceived As New Data(byteData)

            'Accordingly process the message received
            Select Case msgReceived.cmdCommand
                Case Command.Login
                    lstChatters.Items.Add(msgReceived.strName)
                    Exit Select

                Case Command.Logout
                    lstChatters.Items.Remove(msgReceived.strName)
                    Exit Select

                Case Command.Message
                    Exit Select

                Case Command.List
                    lstChatters.Items.AddRange(msgReceived.strMessage.Split("*"c))
                    lstChatters.Items.RemoveAt(lstChatters.Items.Count - 1)
                    txtChatBox.Text += "<<<" & strName & " has joined the room>>>" & vbCr & vbLf
                    Exit Select
            End Select

            If msgReceived.strMessage IsNot Nothing AndAlso msgReceived.cmdCommand <> Command.List Then
                txtChatBox.Text += msgReceived.strMessage & vbCr & vbLf
            End If

            byteData = New Byte(1023) {}

            'Start listening to receive more data from the user
            clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, epServer, New AsyncCallback(AddressOf OnReceive), _
                Nothing)
        Catch generatedExceptionName As ObjectDisposedException
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSclient: " & strName, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub txtMessage_TextChanged(sender As Object, e As EventArgs)
        If txtMessage.Text.Length = 0 Then
            btnSend.Enabled = False
        Else
            btnSend.Enabled = True
        End If
    End Sub

    Private Sub SGSClient_FormClosing(sender As Object, e As FormClosingEventArgs)
        If MessageBox.Show("Are you sure you want to leave the chat room?", "SGSclient: " & strName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) = DialogResult.No Then
            e.Cancel = True
            Return
        End If

        Try
            'Send a message to logout of the server
            Dim msgToSend As New Data()
            msgToSend.cmdCommand = Command.Logout
            msgToSend.strName = strName
            msgToSend.strMessage = Nothing

            Dim b As Byte() = msgToSend.ToByte()
            clientSocket.SendTo(b, 0, b.Length, SocketFlags.None, epServer)
            clientSocket.Close()
        Catch generatedExceptionName As ObjectDisposedException
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSclient: " & strName, MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub txtMessage_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            btnSend_Click(sender, Nothing)
        End If
    End Sub

    Private Sub SGSClient_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        CheckForIllegalCrossThreadCalls = False

        Me.Text = "SGSclient: " & strName

        'The user has logged into the system so we now request the server to send
        'the names of all users who are in the chat room
        Dim msgToSend As New Data()
        msgToSend.cmdCommand = Command.List
        msgToSend.strName = strName
        msgToSend.strMessage = Nothing

        byteData = msgToSend.ToByte()

        clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, New AsyncCallback(AddressOf OnSend), _
            Nothing)

        byteData = New Byte(1023) {}
        'Start listening to the data asynchronously
        clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, epServer, New AsyncCallback(AddressOf OnReceive), _
            Nothing)
    End Sub
End Class

'The data structure by which the server and the client interact with 
'each other
Class Data
    'Default constructor
    Public Sub New()
        Me.cmdCommand = Command.Null
        Me.strMessage = Nothing
        Me.strName = Nothing
    End Sub

    'Converts the bytes into an object of type Data
    Public Sub New(data__1 As Byte())
        'The first four bytes are for the Command
        Me.cmdCommand = CType(BitConverter.ToInt32(data__1, 0), Command)

        'The next four store the length of the name
        Dim nameLen As Integer = BitConverter.ToInt32(data__1, 4)

        'The next four store the length of the message
        Dim msgLen As Integer = BitConverter.ToInt32(data__1, 8)

        'This check makes sure that strName has been passed in the array of bytes
        If nameLen > 0 Then
            Me.strName = Encoding.UTF8.GetString(data__1, 12, nameLen)
        Else
            Me.strName = Nothing
        End If

        'This checks for a null message field
        If msgLen > 0 Then
            Me.strMessage = Encoding.UTF8.GetString(data__1, 12 + nameLen, msgLen)
        Else
            Me.strMessage = Nothing
        End If
    End Sub

    'Converts the Data structure into an array of bytes
    Public Function ToByte() As Byte()
        Dim result As New List(Of Byte)()

        'First four are for the Command
        result.AddRange(BitConverter.GetBytes(CInt(cmdCommand)))

        'Add the length of the name
        If strName IsNot Nothing Then
            result.AddRange(BitConverter.GetBytes(strName.Length))
        Else
            result.AddRange(BitConverter.GetBytes(0))
        End If

        'Length of the message
        If strMessage IsNot Nothing Then
            result.AddRange(BitConverter.GetBytes(strMessage.Length))
        Else
            result.AddRange(BitConverter.GetBytes(0))
        End If

        'Add the name
        If strName IsNot Nothing Then
            result.AddRange(Encoding.UTF8.GetBytes(strName))
        End If

        'And, lastly we add the message text to our array of bytes
        If strMessage IsNot Nothing Then
            result.AddRange(Encoding.UTF8.GetBytes(strMessage))
        End If

        Return result.ToArray()
    End Function

    Public strName As String
    'Name by which the client logs into the room
    Public strMessage As String
    'Message text
    Public cmdCommand As Command
    'Command type (login, logout, send message, etcetera)
End Class
