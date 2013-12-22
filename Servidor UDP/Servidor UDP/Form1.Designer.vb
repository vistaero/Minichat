<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.txtLog = New System.Windows.Forms.RichTextBox()
        Me.SuspendLayout
        '
        'txtLog
        '
        Me.txtLog.Location = New System.Drawing.Point(12, 12)
        Me.txtLog.Name = "txtLog"
        Me.txtLog.Size = New System.Drawing.Size(260, 237)
        Me.txtLog.TabIndex = 0
        Me.txtLog.Text = ""
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 261)
        Me.Controls.Add(Me.txtLog)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(false)

End Sub
    Friend WithEvents txtLog As System.Windows.Forms.RichTextBox

End Class
