<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SGSClient
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstChatters = New System.Windows.Forms.ListBox()
        Me.txtMessage = New System.Windows.Forms.TextBox()
        Me.txtChatBox = New System.Windows.Forms.TextBox()
        Me.btnSend = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lstChatters
        '
        Me.lstChatters.FormattingEnabled = True
        Me.lstChatters.Location = New System.Drawing.Point(382, 14)
        Me.lstChatters.Name = "lstChatters"
        Me.lstChatters.Size = New System.Drawing.Size(103, 277)
        Me.lstChatters.TabIndex = 8
        '
        'txtMessage
        '
        Me.txtMessage.Location = New System.Drawing.Point(12, 270)
        Me.txtMessage.Name = "txtMessage"
        Me.txtMessage.Size = New System.Drawing.Size(282, 20)
        Me.txtMessage.TabIndex = 7
        '
        'txtChatBox
        '
        Me.txtChatBox.BackColor = System.Drawing.SystemColors.Window
        Me.txtChatBox.Location = New System.Drawing.Point(12, 12)
        Me.txtChatBox.Multiline = True
        Me.txtChatBox.Name = "txtChatBox"
        Me.txtChatBox.ReadOnly = True
        Me.txtChatBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtChatBox.Size = New System.Drawing.Size(363, 252)
        Me.txtChatBox.TabIndex = 6
        '
        'btnSend
        '
        Me.btnSend.Enabled = False
        Me.btnSend.Location = New System.Drawing.Point(300, 270)
        Me.btnSend.Name = "btnSend"
        Me.btnSend.Size = New System.Drawing.Size(75, 21)
        Me.btnSend.TabIndex = 5
        Me.btnSend.Text = "&Send"
        Me.btnSend.UseVisualStyleBackColor = True
        '
        'SGSClient
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(521, 354)
        Me.Controls.Add(Me.lstChatters)
        Me.Controls.Add(Me.txtMessage)
        Me.Controls.Add(Me.txtChatBox)
        Me.Controls.Add(Me.btnSend)
        Me.Name = "SGSClient"
        Me.Text = "SGSClientForm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lstChatters As System.Windows.Forms.ListBox
    Private WithEvents txtMessage As System.Windows.Forms.TextBox
    Private WithEvents txtChatBox As System.Windows.Forms.TextBox
    Private WithEvents btnSend As System.Windows.Forms.Button
End Class
