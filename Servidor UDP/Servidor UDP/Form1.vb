Imports System.Collections.Generic
Imports System.Collections
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports System.Net
Imports System.Net.Sockets

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

Public Class Form1

    Inherits Form
    'The ClientInfo structure holds the required information about every
    'client connected to the server
    Private Structure ClientInfo
        Public endpoint As EndPoint
        'Socket of the client
        Public strName As String
        'Name by which the user logged into the chat room
    End Structure

    'The collection of all clients logged into the room (an array of type ClientInfo)
    Private clientList As ArrayList

    'The main socket on which the server listens to the clients
    Private serverSocket As Socket

    Private byteData As Byte() = New Byte(1023) {}

    Public Sub New()
        clientList = New ArrayList()
        InitializeComponent()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            CheckForIllegalCrossThreadCalls = False

            'We are using UDP sockets
            serverSocket = New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

            'Assign the any IP of the machine and listen on port number 1000
            Dim ipEndPoint As New IPEndPoint(IPAddress.Any, 1000)

            'Bind this address to the server
            serverSocket.Bind(ipEndPoint)

            Dim ipeSender As New IPEndPoint(IPAddress.Any, 0)
            'The epSender identifies the incoming clients
            Dim epSender As EndPoint = DirectCast(ipeSender, EndPoint)

            'Start receiving data
            serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, epSender, New AsyncCallback(AddressOf OnReceive), _
                epSender)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Private Sub OnReceive(ar As IAsyncResult)
        Try
            Dim ipeSender As New IPEndPoint(IPAddress.Any, 0)
            Dim epSender As EndPoint = DirectCast(ipeSender, EndPoint)

            serverSocket.EndReceiveFrom(ar, epSender)

            'Transform the array of bytes received from the user into an
            'intelligent form of object Data
            Dim msgReceived As New Data(byteData)

            'We will send this object in response the users request
            Dim msgToSend As New Data()

            Dim message As Byte()

            'If the message is to login, logout, or simple text message
            'then when send to others the type of the message remains the same
            msgToSend.cmdCommand = msgReceived.cmdCommand
            msgToSend.strName = msgReceived.strName

            Select Case msgReceived.cmdCommand
                Case Command.Login

                    'When a user logs in to the server then we add her to our
                    'list of clients

                    Dim clientInfo As New ClientInfo()
                    clientInfo.endpoint = epSender
                    clientInfo.strName = msgReceived.strName

                    clientList.Add(clientInfo)

                    'Set the text of the message that we will broadcast to all users
                    msgToSend.strMessage = "<<<" & msgReceived.strName & " has joined the room>>>"
                    Exit Select

                Case Command.Logout

                    'When a user wants to log out of the server then we search for her 
                    'in the list of clients and close the corresponding connection

                    Dim nIndex As Integer = 0
                    For Each client As ClientInfo In clientList
                        If client.endpoint Is epSender Then
                            clientList.RemoveAt(nIndex)
                            Exit For
                        End If
                        nIndex += 1
                    Next

                    msgToSend.strMessage = "<<<" & msgReceived.strName & " has left the room>>>"
                    Exit Select

                Case Command.Message

                    'Set the text of the message that we will broadcast to all users
                    msgToSend.strMessage = msgReceived.strName & ": " & msgReceived.strMessage
                    Exit Select

                Case Command.List

                    'Send the names of all users in the chat room to the new user
                    msgToSend.cmdCommand = Command.List
                    msgToSend.strName = Nothing
                    msgToSend.strMessage = Nothing

                    'Collect the names of the user in the chat room
                    For Each client As ClientInfo In clientList
                        'To keep things simple we use asterisk as the marker to separate the user names
                        msgToSend.strMessage += client.strName & "*"
                    Next

                    message = msgToSend.ToByte()

                    'Send the name of the users in the chat room
                    serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, epSender, New AsyncCallback(AddressOf OnSend), _
                        epSender)
                    Exit Select
            End Select

            If msgToSend.cmdCommand <> Command.List Then
                'List messages are not broadcasted
                message = msgToSend.ToByte()

                For Each clientInfo As ClientInfo In clientList
                    If clientInfo.endpoint IsNot epSender OrElse msgToSend.cmdCommand <> Command.Login Then
                        'Send the message to all users
                        serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, New AsyncCallback(AddressOf OnSend), _
                            clientInfo.endpoint)
                    End If
                Next

                txtLog.Text += msgToSend.strMessage & vbCr & vbLf
            End If

            'If the user is logging out then we need not listen from her
            If msgReceived.cmdCommand <> Command.Logout Then
                'Start listening to the message send by the user
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, epSender, New AsyncCallback(AddressOf OnReceive), _
                    epSender)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

    Public Sub OnSend(ar As IAsyncResult)
        Try
            serverSocket.EndSend(ar)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "SGSServerUDP", MessageBoxButtons.OK, MessageBoxIcon.[Error])
        End Try
    End Sub

End Class

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