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

    Private Sub Form1_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus
        DetenerParpadeo()

    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Separamos el puerto 200145 para usarlo en nuestra aplicación
        ElSocket.Bind(New IPEndPoint(IPAddress.Any, 20145))
        'Habilitamos la opción Broadcast para el socket
        ElSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, True)
        HiloRecibir = New Thread(AddressOf RecibirDatos) 'Crea el hilo
        HiloRecibir.Start() 'Inicia el hilo
        
        Select Case My.Settings.IniciarConWindows
            Case True
                Me.ShowInTaskbar = False
                Me.Visible = False
            Case False
                If My.Settings.Usuario.Equals("") Then Form2.ShowDialog()
                Me.Show()
                txtMensaje.Focus()
                start_Up(True)
        End Select
        
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

    Private Sub txtMensaje_GotFocus(sender As Object, e As EventArgs) Handles txtMensaje.GotFocus
        DetenerParpadeo()
    End Sub
    Dim DatosBytes As Byte()
    Dim DirecciónDestino As New IPEndPoint(IPAddress.Broadcast, 20145)

    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMensaje.KeyDown
        If e.KeyCode = Keys.Enter Then
            Dim mensaje As String = txtMensaje.Text
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

            'Envía los datos
            ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DirecciónDestino)
            txtMensaje.Clear()

        End If
        If e.KeyCode = Keys.Escape Then
            Dim DirecciónDestino As New IPEndPoint(IPAddress.Broadcast, 20145)
            Dim DatosBytes As Byte() = Encoding.Default.GetBytes("/EMERGENCY")
            ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DirecciónDestino)
            Environment.Exit(0)
        End If
    End Sub

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
        ElSocket.SendTo(DatosBytes, DatosBytes.Length, SocketFlags.None, DirecciónDestino)
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
                                  Application.ExecutablePath.ToString)
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
        Me.WindowState = FormWindowState.Normal

    End Sub

End Class
