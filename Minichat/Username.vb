Public Class Username

    Private Sub Form2_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed

    End Sub

    Private Sub Form2_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If My.Settings.Usuario.Equals("") Then
            If TextBox1.Text.Equals("") Then Environment.Exit(0)
        End If

    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        TextBox1.Text = My.Settings.Usuario

    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If Not TextBox1.TextLength = 0 And TextBox1.TextLength < 15 Then
                My.Settings.Usuario = TextBox1.Text
                My.Settings.Save()
                Me.Close()
            Else
                Label1.ForeColor = Color.Red
                Label1.Text = "Mínimo 1 carácter, máximo 15 carácteres."
            End If


        End If

        If e.KeyCode = Keys.Escape Then Environment.Exit(0)
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        With Label1
            .ForeColor = Color.Black
            .Text = "Escriba un nombre de usuario"
        End With


    End Sub

    Private Sub MenuItem1_Click(sender As Object, e As EventArgs)
        Environment.Exit(0)
    End Sub
End Class