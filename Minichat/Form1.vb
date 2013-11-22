Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Public Class Form1

#Region "Variables"
    'Variable de objeto que contiene el socket
    Dim ElSocket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

    'Variable que contiene al hilo encargado de recibir los datos
    Dim HiloRecibir As Thread

    'Variable que indica si el programa se está cerrando
    Dim Saliendo As Boolean = False

    'Variables temporales para almacenar los datos recibidos
    Dim DireccIP As String, ContenidoMensaje As String

    'Nombre de usuario
    Public Usuario As String

#End Region

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Saliendo = True 'Indica que se está saliendo del programa
        ElSocket.Close() 'Cierra el socket
        HiloRecibir.Abort() 'Termina el proceso del hilo
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Separamos el puerto 200145 para usarlo en nuestra aplicación
        ElSocket.Bind(New IPEndPoint(IPAddress.Any, 20145))
        'Habilitamos la opción Broadcast para el socket
        ElSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, True)
        HiloRecibir = New Thread(AddressOf RecibirDatos) 'Crea el hilo
        HiloRecibir.Start() 'Inicia el hilo
        Form2.ShowDialog()

    End Sub

    Private Sub RecibirDatos()
        'Mientras el inidicador de salida no sea verdadero
        Do Until Saliendo

            'Variable para obtener la IP de la máquína remitente
            Dim LaIPRemota As New IPEndPoint(IPAddress.Any, 0)
            'Variable para almacenar la IP temporalmente
            Dim IPRecibida As EndPoint = CType(LaIPRemota, EndPoint)
            Dim RecibirBytes(255) As Byte 'Buffer
            Dim Datos As String = "" 'Texto a mostrar

            Try
                'Recibe los datos
                ElSocket.ReceiveFrom(RecibirBytes, RecibirBytes.Length, SocketFlags.None, IPRecibida)
                'Los convierte y lo guarda en la variable Datos
                Datos = Encoding.Default.GetString(RecibirBytes)
            Catch ex As SocketException
                If ex.ErrorCode = 10040 Then 'Datos muy largos
                    Datos &= "[truncado]" 'Añade la cadena "[truncado]" al texto recibido
                Else
                    'Muestra el mensaje de error
                    MsgBox("Error '" & ex.ErrorCode.ToString & "' " & ex.Message, MsgBoxStyle.Critical, "Error al recibir datos")
                End If
            End Try



            'Convierte el tipo EndPoint a IPEndPoint con sus respectivas variables
            LaIPRemota = CType(IPRecibida, IPEndPoint)
            'Guarda los datos en variables temporales
            DireccIP = LaIPRemota.Address.ToString
            ContenidoMensaje = Datos.ToString

            'Invoca al evento que mostrará los datos en txtDatosRecibidos
            txtDatosRecibidos.Invoke(New EventHandler(AddressOf ActualizarTextoMensaje))

        Loop
    End Sub

    Private Sub txtDatosRecibidos_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDatosRecibidos.TextChanged
        'Mostrar siempre la última línea del TextBox.
        txtDatosRecibidos.SelectionStart = txtDatosRecibidos.TextLength
        txtDatosRecibidos.ScrollToCaret()
    End Sub

    Protected Sub ActualizarTextoMensaje(ByVal sender As Object, ByVal e As System.EventArgs)
        'Si txtDatosRecibidos está vacío:
        If txtDatosRecibidos.TextLength = 0 Then
            txtDatosRecibidos.Text = ">" & ContenidoMensaje
        Else
            'de lo contrario insertar primero un salto de línea y luego los datos.
            txtDatosRecibidos.Text &= vbCrLf & ">" & ContenidoMensaje
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Timer1.Enabled = True
    End Sub

    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMensaje.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            'Contiene la dirección de Broadcast y el puerto utilizado
            Dim DirecciónDestino As New IPEndPoint(IPAddress.Broadcast, 20145)
            'Buffer que guardará los datos hasta que se envíen
            Dim DatosBytes As Byte() = Encoding.Default.GetBytes(Usuario & ": " & txtMensaje.Text)

            'Envía los datos
            ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DirecciónDestino)

            txtMensaje.Clear()
        End If
        If e.KeyCode = Keys.Escape Then
            Dim DirecciónDestino As New IPEndPoint(IPAddress.Broadcast, 20145)
            Dim DatosBytes As Byte() = Encoding.Default.GetBytes("EMERGENCY")
            Environment.Exit(0)
        End If


    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles txtDatosRecibidos.KeyDown
        If e.KeyCode = Keys.Escape Then
            Environment.Exit(0)
        End If
    End Sub

    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        txtDatosRecibidos.Clear()
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles txtMensaje.TextChanged

    End Sub

    Private Sub AjustarTexto()
        txtDatosRecibidos.SelectionStart = txtDatosRecibidos.Text.Length
        txtDatosRecibidos.ScrollToCaret()
    End Sub

End Class
