Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.Forms

Namespace SGSclient
    Partial Public Class LoginForm
        Inherits Form
        Public clientSocket As Socket
        Public epServer As EndPoint
        Public strName As String

        Public Sub New()
            InitializeComponent()
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

        Private Sub btnCancel_Click(sender As Object, e As EventArgs)
            Close()
        End Sub

        Private WithEvents label2 As System.Windows.Forms.Label
        Private WithEvents label1 As System.Windows.Forms.Label
        Private WithEvents txtServerIP As System.Windows.Forms.TextBox
        Private WithEvents txtName As System.Windows.Forms.TextBox
        Private WithEvents btnOK As System.Windows.Forms.Button
        Private WithEvents btnCancel As System.Windows.Forms.Button

        Private Sub InitializeComponent()
            Me.label2 = New System.Windows.Forms.Label()
            Me.label1 = New System.Windows.Forms.Label()
            Me.txtServerIP = New System.Windows.Forms.TextBox()
            Me.txtName = New System.Windows.Forms.TextBox()
            Me.btnOK = New System.Windows.Forms.Button()
            Me.btnCancel = New System.Windows.Forms.Button()
            Me.SuspendLayout()
            '
            'label2
            '
            Me.label2.AutoSize = True
            Me.label2.Location = New System.Drawing.Point(26, 120)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(54, 13)
            Me.label2.TabIndex = 7
            Me.label2.Text = "&Server IP:"
            '
            'label1
            '
            Me.label1.AutoSize = True
            Me.label1.Location = New System.Drawing.Point(26, 94)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(38, 13)
            Me.label1.TabIndex = 6
            Me.label1.Text = "&Name:"
            '
            'txtServerIP
            '
            Me.txtServerIP.Location = New System.Drawing.Point(87, 117)
            Me.txtServerIP.Name = "txtServerIP"
            Me.txtServerIP.Size = New System.Drawing.Size(171, 20)
            Me.txtServerIP.TabIndex = 9
            '
            'txtName
            '
            Me.txtName.Location = New System.Drawing.Point(87, 91)
            Me.txtName.Name = "txtName"
            Me.txtName.Size = New System.Drawing.Size(171, 20)
            Me.txtName.TabIndex = 8
            '
            'btnOK
            '
            Me.btnOK.Enabled = False
            Me.btnOK.Location = New System.Drawing.Point(102, 146)
            Me.btnOK.Name = "btnOK"
            Me.btnOK.Size = New System.Drawing.Size(75, 23)
            Me.btnOK.TabIndex = 10
            Me.btnOK.Text = "&OK"
            Me.btnOK.UseVisualStyleBackColor = True
            '
            'btnCancel
            '
            Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Me.btnCancel.Location = New System.Drawing.Point(183, 146)
            Me.btnCancel.Name = "btnCancel"
            Me.btnCancel.Size = New System.Drawing.Size(75, 23)
            Me.btnCancel.TabIndex = 11
            Me.btnCancel.Text = "&Cancel"
            Me.btnCancel.UseVisualStyleBackColor = True
            '
            'LoginForm
            '
            Me.ClientSize = New System.Drawing.Size(284, 261)
            Me.Controls.Add(Me.label2)
            Me.Controls.Add(Me.label1)
            Me.Controls.Add(Me.txtServerIP)
            Me.Controls.Add(Me.txtName)
            Me.Controls.Add(Me.btnOK)
            Me.Controls.Add(Me.btnCancel)
            Me.Name = "LoginForm"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private Sub LoginForm_Load_1(sender As Object, e As EventArgs) Handles MyBase.Load
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
            Catch ex As Exception
                MessageBox.Show(ex.Message, "SGSclient", MessageBoxButtons.OK, MessageBoxIcon.[Error])
            End Try
        End Sub

        Private Sub txtServerIP_TextChanged(sender As Object, e As EventArgs) Handles txtServerIP.TextChanged
            If txtName.Text.Length > 0 AndAlso txtServerIP.Text.Length > 0 Then
                btnOK.Enabled = True
            Else
                btnOK.Enabled = False
            End If
        End Sub

        Private Sub txtName_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
            If txtName.Text.Length > 0 AndAlso txtServerIP.Text.Length > 0 Then
                btnOK.Enabled = True
            Else
                btnOK.Enabled = False
            End If
        End Sub
    End Class
End Namespace