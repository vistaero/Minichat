Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Globalization.CultureInfo
Imports Microsoft
Imports Microsoft.Win32.Registry


Public Class Form1

    Private IPDestino As String = "255.255.255.255"
    Private PuertoDestino As Integer = "20145"

    <DllImport("user32.dll")> _
    Public Shared Function FlashWindowEx(ByRef pfwi As FLASHWINFO) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)> _
    Structure FLASHWINFO
        Dim cbSize As Integer
        Dim hwnd As System.IntPtr
        Dim dwFlags As Integer
        Dim uCount As Integer
        Dim dwTimeout As Integer
    End Structure

    Private Const FLASHW_STOP As Integer = &H0
    Private Const FLASHW_CAPTION As Integer = &H1
    Private Const FLASHW_TRAY As Integer = &H2
    Private Const FLASHW_ALL As Integer = (FLASHW_CAPTION Or FLASHW_TRAY)
    Private Const FLASHW_TIMER As Integer = &H4
    Private Const FLASHW_TIMERNOFG As Integer = &HC

    Private Sub ComenzarParpadeo()
        If Not txtMensaje.Focused Then
            Dim FlashInfo As FLASHWINFO
            With FlashInfo
                .cbSize = Convert.ToUInt32(Marshal.SizeOf(GetType(FLASHWINFO)))
                .dwFlags = CType(FLASHW_ALL Or FLASHW_TIMER, Int32)
                .hwnd = Me.Handle
                .dwTimeout = 0
                .uCount = 0
            End With
            FlashWindowEx(FlashInfo)
        End If

    End Sub

    Private Sub DetenerParpadeo()
        Dim FlashInfo As FLASHWINFO
        With FlashInfo
            .cbSize = Convert.ToUInt32(Marshal.SizeOf(GetType(FLASHWINFO)))
            .dwFlags = CType(FLASHW_STOP, Int32)
            .hwnd = Me.Handle
            .dwTimeout = 0
            .uCount = 0
        End With
        FlashWindowEx(FlashInfo)
    End Sub

    Private Sub ShakeMe(ByVal info As ShakeInfo)
        Dim startTime As Date = Now
        Dim offset As Integer = -1
        Dim workingLeft As Integer = info.InitialLeft

        While Now.Subtract(startTime).TotalSeconds < info.ShakeTimeInSeconds
            workingLeft += offset
            If offset < 0 Then
                If Left <= info.InitialLeft - info.MaxPixelDrift Then
                    offset = 1
                End If
            ElseIf Left >= info.InitialLeft + info.MaxPixelDrift Then
                offset = -1
            End If
            MoveMe(workingLeft)
        End While

        MoveMe(info.InitialLeft)
    End Sub

    Private Sub MoveMe(ByVal toLeft)
        If Me.InvokeRequired Then
            Dim move As New MoveMeDelegate(AddressOf DoMoveMe)
            Me.Invoke(move, toLeft)
        Else
            DoMoveMe(toLeft)
        End If
    End Sub

    Protected Delegate Sub MoveMeDelegate(ByVal toLeft As Integer)
    Protected Sub DoMoveMe(ByVal toLeft As Integer)
        Left = toLeft
    End Sub

    Public Structure ShakeInfo
        Public ShakeTimeInSeconds As Double
        Public MaxPixelDrift As Integer
        Public InitialLeft As Integer
    End Structure

    Private Sub Zumbido()
        Dim info As New ShakeInfo
        info.ShakeTimeInSeconds = 1.0
        info.MaxPixelDrift = 10
        info.InitialLeft = Me.Left
        System.Threading.ThreadPool.QueueUserWorkItem(AddressOf ShakeMe, info)
    End Sub

#Region "Variables"
    'Variable de objeto que contiene el socket
    Dim ElSocket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

    'Variable que contiene al hilo encargado de recibir los datos
    Dim HiloRecibir As Thread

    'Variable que indica si el programa se está cerrando
    Dim Saliendo As Boolean = False

    'Variables temporales para almacenar los datos recibidos
    Dim DireccIP As String, ContenidoMensaje As String


#End Region

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Me.WindowState = FormWindowState.Minimized
        Me.Visible = False
        e.Cancel = True
    End Sub

    Private Sub IniciarSocket()
        'Separamos el puerto 20145 para usarlo en nuestra aplicación
        ElSocket.Bind(New IPEndPoint(IPAddress.Any, PuertoDestino))
        'Habilitamos la opción Broadcast para el socket
        ElSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, True)
        HiloRecibir = New Thread(AddressOf RecibirDatos) 'Crea el hilo
        HiloRecibir.Start() 'Inicia el hilo
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        IniciarSocket()

        If My.Settings.IniciarConWindows = False Then
            If My.Settings.Usuario.Equals("") Then Form2.ShowDialog()
            Me.Show()
            txtMensaje.Focus()
        End If

        If My.Application.CommandLineArgs.Count > 0 Then
            If My.Application.CommandLineArgs.First = "/h" Then
                Me.ShowInTaskbar = False
                Me.Visible = False
            End If
        End If

        IPDestinoTexto.Text = IPDestino
        PuertoTexto.Value = PuertoDestino

        If Not My.Settings.Usuario.Equals("Afrinfor") Then start_Up(True)




        ' Comprobar actualizaciones
        Dim VersionActual As String = My.Application.Info.Version.ToString
        Dim versiontxtpath As String = Environment.CurrentDirectory & "\version.txt"
        If System.IO.File.Exists(versiontxtpath) Then System.IO.File.Delete(versiontxtpath)
        My.Computer.Network.DownloadFile("http://vistaero.es/NyanChat/version.txt", Environment.CurrentDirectory & "\version.txt")
        Dim Version As Integer = System.IO.File.ReadAllText(Environment.CurrentDirectory & "\version.txt")
        Const Actualizador As String = "C:\Users\Jesús Garcés\Documents\GitHub\Minichat\NyanChat_Updater\bin\Release\NyanChat_Updater.exe"
        If Version < VersionActual Then
            ' Lanzar actualizador
            Process.Start(Actualizador)
            Environment.Exit(0)
        End If



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

    Protected Sub ActualizarTextoMensaje(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim Addline As Boolean = True
        If ContenidoMensaje.StartsWith("/SHUTDOWN") Then
            If Not My.Settings.Usuario.Equals("vistaero") Then
                Process.Start("shutdown.exe", "-s -t 0")
                Addline = False
            Else
                Addline = False
                txtDatosRecibidos.AppendText("Has apagado todos los ordenadores.")
            End If
        End If

        If ContenidoMensaje.StartsWith("/ZUMBIDO") Then
            Addline = False
            Zumbido()
            ComenzarParpadeo()
        End If

        If ContenidoMensaje.StartsWith("/EMERGENCY") Then
            Environment.Exit(0)
        End If

        If Addline = True Then
            'Si txtDatosRecibidos está vacío:
            If txtDatosRecibidos.TextLength = 0 Then
                txtDatosRecibidos.Text = ContenidoMensaje
            Else
                'de lo contrario insertar primero un salto de línea y luego los datos.
                txtDatosRecibidos.Text &= vbCrLf & ContenidoMensaje
            End If
            ComenzarParpadeo()
        End If
    End Sub

    Private Sub txtMensaje_GotFocus(sender As Object, e As EventArgs)
        DetenerParpadeo()
    End Sub
    Dim DatosBytes As Byte()
    Dim DireccionDestino As New IPEndPoint(IPAddress.Parse(IPDestino), PuertoDestino)

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Escape Then
            Environment.Exit(0)
        End If
    End Sub

    Private Sub Form1_LostFocus(sender As Object, e As EventArgs) Handles Me.LostFocus
        txtMensaje.Focus()
    End Sub

    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        DatosBytes = Encoding.Default.GetBytes("/ZUMBIDO")
        ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DireccionDestino)
        ToolStripButton1.Enabled = False
        ZumbidoHabilitado.Enabled = True
    End Sub

    Private Function start_Up(ByVal bCreate As Boolean) As String
        Const key As String = "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
        Dim subClave As String = Application.ProductName.ToString
        Dim msg As String = ""
        Try
            Dim Registro As RegistryKey = CurrentUser.CreateSubKey(key, RegistryKeyPermissionCheck.ReadWriteSubTree)
            With Registro
                .OpenSubKey(key, True)
                Select Case bCreate
                    Case True
                        .SetValue(subClave, _
                                  Application.ExecutablePath.ToString & " /h")
                        My.Settings.IniciarConWindows = True
                        My.Settings.Save()
                    Case False
                        If .GetValue(subClave, "").ToString <> "" Then
                            .DeleteValue(subClave)
                            My.Settings.IniciarConWindows = False
                            My.Settings.Save()
                        End If
                End Select
            End With
        Catch ex As Exception
            msg = ex.Message.ToString
        End Try
        Return Nothing
    End Function

    Private Sub ToolStripButton2_Click(sender As Object, e As EventArgs) Handles ToolStripButton2.Click
        Form2.ShowDialog()

    End Sub

    Private Sub Salir()
        Saliendo = True 'Indica que se está saliendo del programa
        ElSocket.Close() 'Cierra el socket
        HiloRecibir.Abort() 'Termina el proceso del hilo
        NotifyIcon1.Visible = False
        Environment.Exit(0)

    End Sub

    Private Sub SalirToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SalirToolStripMenuItem.Click
        Salir()
    End Sub

    Private Sub MostrarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MostrarToolStripMenuItem.Click
        Me.Show()
        Me.Visible = True
        Me.ShowInTaskbar = True
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private DireccionDestinoValida As Boolean = True

    Private Sub IPDestinoTexto_LostFocus(sender As Object, e As EventArgs) Handles IPDestinoTexto.LostFocus
        If IPDestinoTexto.Text.Equals("") Then IPDestinoTexto.Text = "255.255.255.255"
    End Sub

    Private Sub ActualizarDestino()
        IPDestino = IPDestinoTexto.Text.ToString
        PuertoDestino = PuertoTexto.Value
        Try
            DireccionDestino.Address = IPAddress.Parse(IPDestino)
            DireccionDestino.Port = PuertoDestino
            DireccionDestinoValida = True
        Catch ex As Exception
            DireccionDestinoValida = False
        End Try
    End Sub
    Private Sub IPDestinoTexto_TextChanged(sender As Object, e As EventArgs) Handles IPDestinoTexto.TextChanged
        ActualizarDestino()


    End Sub

    Private Sub PuertoTexto_TextChanged(sender As Object, e As EventArgs)
        PuertoDestino = PuertoTexto.Value

    End Sub

    Private Sub ZumbidoHabilitado_Disposed(sender As Object, e As EventArgs) Handles ZumbidoHabilitado.Disposed
        ToolStripButton1.Enabled = True

    End Sub

    Private Sub ZumbidoHabilitado_Tick(sender As Object, e As EventArgs) Handles ZumbidoHabilitado.Tick
        ZumbidoHabilitado.Dispose()

    End Sub

    Private Sub txtDatosRecibidos_TextChanged(sender As Object, e As EventArgs) Handles txtDatosRecibidos.TextChanged
        'Mostrar siempre la última línea del TextBox.
        txtDatosRecibidos.SelectionStart = txtDatosRecibidos.TextLength
        txtDatosRecibidos.ScrollToCaret()

    End Sub

    Private Sub txtMensaje_GotFocus1(sender As Object, e As EventArgs) Handles txtMensaje.GotFocus
        DetenerParpadeo()
    End Sub

    Private Sub txtMensaje_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMensaje.KeyDown
        Dim mensaje As String = txtMensaje.Text
        If e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.Escape Then
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                Select Case txtMensaje.Text
                    Case Is = "/ZUMBIDO"
                        mensaje = "/ZUMBIDO"
                        DatosBytes = Encoding.Default.GetBytes(mensaje)
                    Case Is = "/SHUTDOWN"
                        mensaje = "/SHUTDOWN"
                        DatosBytes = Encoding.Default.GetBytes(mensaje)
                    Case Is = "/EMERGENCY"
                        mensaje = "/EMERGENCY"
                        DatosBytes = Encoding.Default.GetBytes(mensaje)
                    Case Else
                        DatosBytes = Encoding.Default.GetBytes(My.Settings.Usuario & ": " & txtMensaje.Text)
                End Select
            End If
            If e.KeyCode = Keys.Escape Then
                mensaje = "/EMERGENCY"
                DatosBytes = Encoding.Default.GetBytes(mensaje)
            End If

            'Envía los datos
            If Not txtMensaje.Text = "" OrElse txtMensaje.Text = vbNewLine Then
                If DireccionDestinoValida = True Then
                    ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DireccionDestino)
                    txtMensaje.Clear()
                Else
                    MsgBox("La dirección de destino no es válida")
                End If
            End If
        End If
    End Sub

    Private Sub PuertoTexto_ValueChanged(sender As Object, e As EventArgs) Handles PuertoTexto.ValueChanged
        PuertoDestino = PuertoTexto.Value
        ActualizarDestino()

    End Sub

    Private Sub IPDestinoTexto_Click(sender As Object, e As EventArgs) Handles IPDestinoTexto.Click

    End Sub

    Private Sub txtMensaje_TextChanged(sender As Object, e As EventArgs) Handles txtMensaje.TextChanged

    End Sub
End Class
