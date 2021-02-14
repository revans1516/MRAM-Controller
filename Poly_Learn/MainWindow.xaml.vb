Imports System.Math
Imports ABB.Robotics.Controllers


Class MainWindow
#Region "Controllers"
    Dim Ping_Pong As Controller = Nothing
    Dim Tick_Tock As Controller = Nothing
    Dim Razzle_Dazzle As Controller = Nothing
    Dim Ping_Trig As IOSystemDomain.DigitalSignal
    Dim Ping_Prev As Robot_Status = Robot_Status.Disconnected
    Dim Tick_Prev As Robot_Status = Robot_Status.Disconnected
    Dim Razz_Prev As Robot_Status = Robot_Status.Disconnected
#End Region

#Region "UI Variables"
    Dim Robot_Thrd As New Threading.Thread(AddressOf Pick_Demo)
    Dim Check_Timer As New Timers.Timer
    Dim Graph_Thread As New Threading.Thread(AddressOf Graph_Speed)
    Dim counter As Integer = 0
    Dim PolyWorks As PolyLib.PolyWorks_Class
#End Region

#Region "Speech Recongition"
    Dim wordlist As String() = New String() {"Alis what is M-RAM's status", "Alis why can't M-RAM run"}
    Dim cult As New Globalization.CultureInfo("en-US")
    Dim sre As New Speech.Recognition.SpeechRecognitionEngine(cult)
    Dim synth As New Speech.Synthesis.SpeechSynthesizer
#End Region


    Dim InstructWind As InstructionsWindow
    Private Enum Robot_Status
        Ready = 1
        Disconnected = -1
        Manual_Mode = -2
        Motors_Off = -3
        Running = -4
    End Enum

#Region "Graphing Variables"
    Dim Tick_Line As New LiveCharts.Wpf.LineSeries
    Dim Tick_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Ping_Line As New LiveCharts.Wpf.LineSeries
    Dim Ping_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Razz_Line As New LiveCharts.Wpf.LineSeries
    Dim Razz_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Tock_Line As New LiveCharts.Wpf.LineSeries
    Dim Tock_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Pong_Line As New LiveCharts.Wpf.LineSeries
    Dim Pong_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Dazz_Line As New LiveCharts.Wpf.LineSeries
    Dim Dazz_Values As New LiveCharts.ChartValues(Of LiveCharts.Defaults.ObservablePoint)
    Dim Ping_pos As RapidDomain.Pos = Nothing
    Dim Pong_pos As RapidDomain.Pos = Nothing
    Dim Tick_pos As RapidDomain.Pos = Nothing
    Dim Tock_pos As RapidDomain.Pos = Nothing
    Dim Razz_pos As RapidDomain.Pos = Nothing
    Dim Dazz_pos As RapidDomain.Pos = Nothing
    Dim GraphRun As Boolean = True

#End Region


#Region "Window Functions"

    Private Sub loading() Handles Me.Loaded
        Connect_Thrd()
        rdbReady.IsChecked = StartUp.Default.Ready
        rdbNotReady.IsChecked = Not rdbReady.IsChecked
        cmbProgram.SelectedIndex = StartUp.Default.Robot_Program
        cmbTheme.SelectedIndex = StartUp.Default.Style
        Me.WindowState = WindowState.Maximized
        Initialize_Graph()
        Graph_Thread.Start()
        Me.Activate()
        Dim Tempthread As New Threading.Thread(AddressOf VoiceSetup)
        Tempthread.Start()
    End Sub




    Private Sub Shutdown() Handles Me.Closing
        Me.WindowState = WindowState.Minimized
        If InstructWind IsNot Nothing Then InstructWind.Close()
        CloseVoices()
        GraphRun = False
        chtRobot.IsEnabled = False
        Check_Timer.Stop()
        Robot_Thrd.Abort()
        StartUp.Default.Robot_Program = cmbProgram.SelectedIndex
        StartUp.Default.Style = cmbTheme.SelectedIndex
        StartUp.Default.Save()
        If Ping_Pong IsNot Nothing Then
            Ping_Pong.Logoff()
        End If
        If Tick_Tock IsNot Nothing Then
            Tick_Tock.Logoff()
        End If
        If Razzle_Dazzle IsNot Nothing Then
            Razzle_Dazzle.Logoff()
        End If
    End Sub

    Private Sub Themes() Handles cmbTheme.SelectionChanged

        Select Case cmbTheme.SelectedIndex
            Case 0
                My.Application.Resources.MergedDictionaries(0).Source = New Uri($"/Themes/Default.xaml", UriKind.Relative)
            Case 1
                My.Application.Resources.MergedDictionaries(0).Source = New Uri($"/Themes/ExpressionDark.xaml", UriKind.Relative)
            Case 2
                My.Application.Resources.MergedDictionaries(0).Source = New Uri($"/Themes/ExpressionLight.xaml", UriKind.Relative)
            Case 3
                My.Application.Resources.MergedDictionaries(0).Source = New Uri($"/Themes/ShinyBlue.xaml", UriKind.Relative)
            Case 4
                My.Application.Resources.MergedDictionaries(0).Source = New Uri($"/Themes/ShinyRed.xaml", UriKind.Relative)
        End Select
    End Sub

    Private Sub Dragging() Handles grdControls.MouseDown
        If Application.Current.MainWindow.WindowState = WindowState.Maximized Then Application.Current.MainWindow.WindowState = WindowState.Normal
        Application.Current.MainWindow.DragMove()
    End Sub

    Private Sub WinClose() Handles btnClose.Click

        Close()

    End Sub

    Private Sub WinMinimize() Handles btnMinimize.Click
        Me.WindowState = WindowState.Minimized
    End Sub

    Private Sub WinMaximize() Handles btnRestore.Click
        If Me.WindowState = WindowState.Maximized Then
            Me.WindowState = WindowState.Normal
        Else
            Me.WindowState = WindowState.Maximized
        End If
    End Sub

    Private Sub OpenInstructions() Handles btnSetup.Click
        If InstructWind Is Nothing OrElse Not InstructWind.IsLoaded Then
            InstructWind = New InstructionsWindow
            InstructWind.Show()
        End If
        InstructWind.WindowState = WindowState.Normal
        InstructWind.Activate()
    End Sub

    Private Sub rdbReadyChanged() Handles rdbReady.Checked
        StartUp.Default.Ready = rdbReady.IsChecked
    End Sub

    Private Sub rdbNotReadyChanged() Handles rdbNotReady.Checked
        StartUp.Default.Ready = rdbReady.IsChecked
    End Sub

    Private Sub testbtn() Handles btnTest.Click
        MessageBox.Show("STOP!!!! WHO TOLD YOU YOU COULD TOUCH THIS, IT MIGHT BREAK THINGS!")
    End Sub

#End Region



#Region "Connecting"

    Private Sub Check_Connections()


        Try
            Log_Ping()
            Log_Tick()
            Log_Razz()

            If Ping_Pong Is Nothing OrElse Not Ping_Pong.Connected Then ConnectToRobot(Ping_Pong, controllerIP:="172.19.209.18")

            Select Case Robot_Check(Ping_Pong)
                Case 1
                    Dispatcher.Invoke(
            Sub()
                txtPingStat.Text = Robot_Check(Ping_Pong).ToString
                txtPingStat.Background = Brushes.Green
            End Sub)
                Case -1
                    Dispatcher.Invoke(
                    Sub()
                        txtPingStat.Text = Replace(Robot_Check(Ping_Pong).ToString, "_", " ")
                        txtPingStat.Background = Brushes.Red
                    End Sub)
                Case -2
                    Dispatcher.Invoke(
                    Sub()
                        txtPingStat.Text = Replace(Robot_Check(Ping_Pong).ToString, "_", " ")
                        txtPingStat.Background = Brushes.Goldenrod
                    End Sub)
                Case -3
                    Dispatcher.Invoke(
                    Sub()
                        txtPingStat.Text = Replace(Robot_Check(Ping_Pong).ToString, "_", " ")
                        txtPingStat.Background = Brushes.Goldenrod
                    End Sub)
                Case -4
                    Dispatcher.Invoke(
                    Sub()
                        txtPingStat.Text = Replace(Robot_Check(Ping_Pong).ToString, "_", " ")
                        txtPingStat.Background = Brushes.Goldenrod
                    End Sub)
            End Select


            If Tick_Tock Is Nothing OrElse Not Tick_Tock.Connected Then ConnectToRobot(Tick_Tock, controllerIP:="172.19.209.22")

            Select Case Robot_Check(Tick_Tock)
                Case 1
                    Dispatcher.Invoke(
                Sub()
                    txtTickStat.Text = Robot_Check(Tick_Tock).ToString
                    txtTickStat.Background = Brushes.Green
                End Sub)
                Case -1
                    Dispatcher.Invoke(
                Sub()
                    txtTickStat.Text = Replace(Robot_Check(Tick_Tock).ToString, "_", " ")
                    txtTickStat.Background = Brushes.Red
                End Sub)
                Case -2
                    Dispatcher.Invoke(
                Sub()
                    txtTickStat.Text = Replace(Robot_Check(Tick_Tock).ToString, "_", " ")
                    txtTickStat.Background = Brushes.Goldenrod
                End Sub)
                Case -3
                    Dispatcher.Invoke(
                Sub()
                    txtTickStat.Text = Replace(Robot_Check(Tick_Tock).ToString, "_", " ")
                    txtTickStat.Background = Brushes.Goldenrod
                End Sub)
                Case -4
                    Dispatcher.Invoke(
                Sub()
                    txtTickStat.Text = Replace(Robot_Check(Tick_Tock).ToString, "_", " ")
                    txtTickStat.Background = Brushes.Goldenrod
                End Sub)
            End Select


            If Razzle_Dazzle Is Nothing OrElse Not Razzle_Dazzle.Connected Then ConnectToRobot(Razzle_Dazzle, controllerIP:="172.19.209.19")

            Select Case Robot_Check(Razzle_Dazzle)
                Case 1
                    Dispatcher.Invoke(
            Sub()
                txtRazzStat.Text = Robot_Check(Razzle_Dazzle).ToString
                txtRazzStat.Background = Brushes.Green
            End Sub)
                Case -1
                    Dispatcher.Invoke(
                    Sub()
                        txtRazzStat.Text = Replace(Robot_Check(Razzle_Dazzle).ToString, "_", " ")
                        txtRazzStat.Background = Brushes.Red
                    End Sub)
                Case -2
                    Dispatcher.Invoke(
                    Sub()
                        txtRazzStat.Text = Replace(Robot_Check(Razzle_Dazzle).ToString, "_", " ")
                        txtRazzStat.Background = Brushes.Goldenrod
                    End Sub)
                Case -3
                    Dispatcher.Invoke(
                    Sub()
                        txtRazzStat.Text = Replace(Robot_Check(Razzle_Dazzle).ToString, "_", " ")
                        txtRazzStat.Background = Brushes.Goldenrod
                    End Sub)
                Case -4
                    Dispatcher.Invoke(
                    Sub()
                        txtRazzStat.Text = Replace(Robot_Check(Razzle_Dazzle).ToString, "_", " ")
                        txtRazzStat.Background = Brushes.Goldenrod
                    End Sub)
            End Select



            If Robot_Check(Tick_Tock) = 1 AndAlso Robot_Check(Ping_Pong) = 1 AndAlso Robot_Check(Razzle_Dazzle) = 1 AndAlso Robot_Thrd.IsAlive = False AndAlso StartUp.Default.Ready Then
                Dispatcher.Invoke(Sub()
                                      btnStart.IsEnabled = True
                                      txtoverall.Text = "Ready"
                                      txtoverall.Background = Brushes.Green
                                      elsStatus.Fill = Brushes.Green
                                  End Sub)
            ElseIf Robot_Check(Tick_Tock) <> Robot_Status.Disconnected AndAlso Robot_Check(Ping_Pong) <> Robot_Status.Disconnected AndAlso Robot_Check(Razzle_Dazzle) <> Robot_Status.Disconnected Then
                Dispatcher.Invoke(Sub()
                                      btnStart.IsEnabled = False
                                      txtoverall.Text = "Not Ready"
                                      txtoverall.Background = Brushes.Goldenrod
                                      elsStatus.Fill = Brushes.Yellow
                                  End Sub)
            Else

                Dispatcher.Invoke(Sub()
                                      btnStart.IsEnabled = False
                                      txtoverall.Text = "Not Ready"
                                      txtoverall.Background = Brushes.Red
                                      elsStatus.Fill = Brushes.Red
                                  End Sub)
            End If
        Catch e As Exception
        End Try
    End Sub

    Private Function Robot_Check(Robot As Controller) As Robot_Status
        If Robot IsNot Nothing AndAlso Robot.Connected Then
            If Robot.OperatingMode = ControllerOperatingMode.Auto Then
                If Robot.State = ControllerState.MotorsOn Then
                    If Robot.Rapid.GetTask("T_ROB1").ExecutionStatus = RapidDomain.TaskExecutionStatus.Running Then
                        Return Robot_Status.Running
                    Else
                        Return Robot_Status.Ready
                    End If
                Else
                    Return Robot_Status.Motors_Off
                End If
            Else
                Return Robot_Status.Manual_Mode
            End If
        Else
            Return Robot_Status.Disconnected
        End If
    End Function

    Private Function ConnectToRobot(ByRef activeController As Controller, ByVal Optional controllerIP As String = "192.168.125.1") As Boolean
        'checks if the user is currently connected to a robot
        If activeController <> Nothing Then
            activeController.Logoff()
            activeController.Dispose()
        End If
        'checks if the user is trying to connect to a virtual robot

        'scans the network for controllers
        Dim scanner As New Discovery.NetworkScanner
        scanner.Scan()
        Discovery.NetworkScanner.AddRemoteController(controllerIP)
        Dim controllers As ControllerInfoCollection = scanner.Controllers
        'checks to see if the user is trying to connect to a real robot

        For Each control As ControllerInfo In controllers
            'gets the controller on the network with the matching IP
            If control.IPAddress.ToString = controllerIP Then
                'connects to the controller

                activeController = Controller.Connect(control, ConnectionType.Standalone)
                activeController.Logon(UserInfo.DefaultUser)

                Return True
            End If
        Next
        Return False
    End Function

    Private Sub Connect_Thrd()
        Dim connect_thread As New Threading.Thread(
            Sub()
                Check_Connections()
                AddHandler Check_Timer.Elapsed, AddressOf Check_Connections
                Check_Timer.Interval = 1000
                Check_Timer.Start()
            End Sub)
        connect_thread.Start()
    End Sub
#End Region

#Region "Demos"
    Private Sub Apodius_Demo()
        If Not My.Computer.Network.Ping(txtTrackerIP.Text) OrElse Not My.Computer.Network.Ping("10.168.2.200") Then
            MessageBox.Show("Either the tracker or scanner is not properly plugged in. Fix this issue and attempt to rerun.")
            Exit Sub
        End If
        Dim Ping_Master As Mastership = Mastership.Request(Ping_Pong)
        Dim Tick_Master As Mastership = Mastership.Request(Tick_Tock)
        Dim Razzle_Master As Mastership = Mastership.Request(Razzle_Dazzle)
        PolyWorks = New PolyLib.PolyWorks_Class()
        Razzle_Dazzle.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Tick_Tock.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.GetTask("T_ROB1").SetProgramPointer("Apodius", "Apodius_Main")
        Ping_Pong.Rapid.GetTask("T_ROB2").SetProgramPointer("Apodius", "Apodius_Main")
        Tick_Tock.Rapid.GetTask("T_ROB1").SetProgramPointer("Apodius", "Apodius_Main")
        Tick_Tock.Rapid.GetTask("T_ROB2").SetProgramPointer("Apodius", "Apodius_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB1").SetProgramPointer("Apodius", "Apodius_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB2").SetProgramPointer("Apodius", "Apodius_Main")
        Ping_Trig.Reset()
        Try
            Ping_Pong.Rapid.Start(True)
            Tick_Tock.Rapid.Start(True)
            Razzle_Dazzle.Rapid.Start(True)
        Catch exp As Exception
            Ping_Pong.Rapid.Stop()
            Tick_Tock.Rapid.Stop()
            Razzle_Dazzle.Rapid.Stop()
            MessageBox.Show("Unable to start all robots")
            MessageBox.Show(exp.Message)
            Exit Sub
        End Try
        PolyWorks.Connect_To_Scanner()
        'Wait for robot to get to position
        Do Until Ping_Trig.Value = 1
            Threading.Thread.Sleep(1000)
        Loop
        PolyWorks.Point_Scanner({-4442, -1132, -150})
        Threading.Thread.Sleep(3000)
        PolyWorks.Start_Scan()
        Ping_Trig.Reset()
        Threading.Thread.Sleep(1000)
        'Wait for robot to be done scanning
        Do Until Ping_Trig.Value = 1
            Threading.Thread.Sleep(1000)
        Loop
        PolyWorks.Stop_Scan()
        Ping_Trig.Reset()
        PolyWorks.Disconnect_Scanner()
        Ping_Master.Release()
        Tick_Master.Release()
        Razzle_Master.Release()
        Do Until Ping_Pong.Rapid.GetTask("T_ROB2").ExecutionStatus <> RapidDomain.TaskExecutionStatus.Running
            Threading.Thread.Sleep(500)
        Loop
        Do Until Tick_Tock.Rapid.GetTask("T_ROB2").ExecutionStatus <> RapidDomain.TaskExecutionStatus.Running
            Threading.Thread.Sleep(500)
        Loop
        Do Until Tick_Tock.Rapid.GetTask("T_ROB1").ExecutionStatus <> RapidDomain.TaskExecutionStatus.Running
            Threading.Thread.Sleep(500)
        Loop
    End Sub

    Private Sub Pick_Demo()
        Dim Ping_Master As Mastership = Mastership.Request(Ping_Pong)
        Dim Tick_Master As Mastership = Mastership.Request(Tick_Tock)
        Dim Razzle_Master As Mastership = Mastership.Request(Razzle_Dazzle)
        Razzle_Dazzle.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Tick_Tock.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Pick_Demo")
        Ping_Pong.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Pick_Demo")
        Tick_Tock.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Pick_Demo")
        Tick_Tock.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Pick_Demo")
        Razzle_Dazzle.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Pick_Demo")
        Razzle_Dazzle.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Pick_Demo")
        Try
            Ping_Pong.Rapid.Start(True)
            Tick_Tock.Rapid.Start(True)
            Razzle_Dazzle.Rapid.Start(True)
        Catch exp As Exception
            Ping_Pong.Rapid.Stop()
            Tick_Tock.Rapid.Stop()
            Razzle_Dazzle.Rapid.Stop()
            MessageBox.Show("Unable to start all robots")
            MessageBox.Show(exp.Message)
            Exit Sub
        End Try
        Ping_Master.Release()
        Tick_Master.Release()
        Razzle_Master.Release()
    End Sub

    Private Sub Move_Demo()
        Dim Ping_Master As Mastership = Mastership.Request(Ping_Pong)
        Dim Tick_Master As Mastership = Mastership.Request(Tick_Tock)
        Dim Razzle_Master As Mastership = Mastership.Request(Razzle_Dazzle)
        Razzle_Dazzle.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Tick_Tock.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Move_Demo")
        Ping_Pong.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Move_Demo")
        Tick_Tock.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Move_Demo")
        Tick_Tock.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Move_Demo")
        Razzle_Dazzle.Rapid.GetTask("T_ROB1").SetProgramPointer("Base_Demos", "Move_Demo")
        Razzle_Dazzle.Rapid.GetTask("T_ROB2").SetProgramPointer("Base_Demos", "Move_Demo")
        Try
            Ping_Pong.Rapid.Start(True)
            Tick_Tock.Rapid.Start(True)
            Razzle_Dazzle.Rapid.Start(True)
        Catch exp As Exception
            Ping_Pong.Rapid.Stop()
            Tick_Tock.Rapid.Stop()
            Razzle_Dazzle.Rapid.Stop()
            MessageBox.Show("Unable to start all robots")
            MessageBox.Show(exp.Message)
            Exit Sub
        End Try
    End Sub

    Private Sub Compressor_Demo()
        Dim PassFail As RapidDomain.RapidData = Ping_Pong.Rapid.GetRapidData({"T_ROB2", "Compressor", "Pass"})
        Dim Ping_Master As Mastership = Mastership.Request(Ping_Pong)
        Dim Tick_Master As Mastership = Mastership.Request(Tick_Tock)
        Dim Razzle_Master As Mastership = Mastership.Request(Razzle_Dazzle)
        Razzle_Dazzle.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Tick_Tock.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.GetTask("T_ROB1").SetProgramPointer("Compressor", "Compressor_Main")
        Ping_Pong.Rapid.GetTask("T_ROB2").SetProgramPointer("Compressor", "Compressor_Main")
        Tick_Tock.Rapid.GetTask("T_ROB1").SetProgramPointer("Compressor", "Compressor_Main")
        Tick_Tock.Rapid.GetTask("T_ROB2").SetProgramPointer("Compressor", "Compressor_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB1").SetProgramPointer("Compressor", "Compressor_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB2").SetProgramPointer("Compressor", "Compressor_Main")
        Try
            Ping_Pong.Rapid.Start(True)
            Tick_Tock.Rapid.Start(True)
            Razzle_Dazzle.Rapid.Start(True)
        Catch exp As Exception
            Ping_Pong.Rapid.Stop()
            Tick_Tock.Rapid.Stop()
            Razzle_Dazzle.Rapid.Stop()
            MessageBox.Show("Unable to start all robots")
            MessageBox.Show(exp.Message)
            Exit Sub
        End Try

        Do Until Ping_Pong.State = ControllerState.MotorsOff
            Threading.Thread.Sleep(500)
        Loop
        MessageBox.Show("Hit ok once the part has been assembled")
        Do Until Ping_Pong.State = ControllerState.MotorsOn
            MessageBox.Show("Make sure Motors are on")
        Loop
        Ping_Pong.Rapid.Start(True)
        Threading.Thread.Sleep(2000)
        Do Until Ping_Pong.Rapid.ExecutionStatus = RapidDomain.ExecutionStatus.Stopped
            Threading.Thread.Sleep(1000)
        Loop

        MessageBox.Show("The Part is correct: " & PassFail.StringValue)
        Ping_Master.Release()
        Tick_Master.Release()
        Razzle_Master.Release()
    End Sub

    Private Sub Yoke_Demo()
        Dim Ping_Master As Mastership = Mastership.Request(Ping_Pong)
        Dim Tick_Master As Mastership = Mastership.Request(Tick_Tock)
        Dim Razzle_Master As Mastership = Mastership.Request(Razzle_Dazzle)
        Razzle_Dazzle.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Tick_Tock.Rapid.Cycle = RapidDomain.ExecutionCycle.Once
        Ping_Pong.Rapid.GetTask("T_ROB1").SetProgramPointer("Deloitte", "Deloitte_Main")
        Ping_Pong.Rapid.GetTask("T_ROB2").SetProgramPointer("Deloitte", "Deloitte_Main")
        Tick_Tock.Rapid.GetTask("T_ROB1").SetProgramPointer("Deloitte", "Deloitte_Main")
        Tick_Tock.Rapid.GetTask("T_ROB2").SetProgramPointer("Deloitte", "Deloitte_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB1").SetProgramPointer("Deloitte", "Deloitte_Main")
        Razzle_Dazzle.Rapid.GetTask("T_ROB2").SetProgramPointer("Deloitte", "Deloitte_Main")
        Try
            Ping_Pong.Rapid.Start(True)
            Tick_Tock.Rapid.Start(True)
            Razzle_Dazzle.Rapid.Start(True)
        Catch exp As Exception
            Ping_Pong.Rapid.Stop()
            Tick_Tock.Rapid.Stop()
            Razzle_Dazzle.Rapid.Stop()
            MessageBox.Show("Unable to start all robots")
            MessageBox.Show(exp.Message)
            Exit Sub
        End Try
        Ping_Master.Release()
        Tick_Master.Release()
        Razzle_Master.Release()
    End Sub

    Private Sub Program() Handles btnStart.Click
        Select Case cmbProgram.SelectedIndex
            Case 0
                Robot_Thrd = New System.Threading.Thread(AddressOf Pick_Demo)
                Robot_Thrd.Start()
            Case 1
                'Robot_Thrd = New System.Threading.Thread(AddressOf Apodius_Demo)
                'Robot_Thrd.Start()
            Case 2
                Robot_Thrd = New System.Threading.Thread(AddressOf Move_Demo)
                Robot_Thrd.Start()
            Case 3
                Robot_Thrd = New System.Threading.Thread(AddressOf Compressor_Demo)
                Robot_Thrd.Start()
            Case 4
                Robot_Thrd = New System.Threading.Thread(AddressOf Yoke_Demo)
                Robot_Thrd.Start()
        End Select

    End Sub
#End Region

#Region "Voice Control"
    Private Sub VoiceSetup()
        sre.SetInputToDefaultAudioDevice()
        AddHandler sre.SpeechRecognized, AddressOf sre_SpeechRecognized
        'Dim Diction As System.Speech.Recognition.DictationGrammar = New System.Speech.Recognition.DictationGrammar
        Dim Directions As Speech.Recognition.Choices = New Speech.Recognition.Choices()
        Directions.Add(wordlist)
        Dim MyGram As New Speech.Recognition.Grammar(New Speech.Recognition.GrammarBuilder(Directions))
        sre.LoadGrammar(MyGram)
        sre.RecognizeAsync(Speech.Recognition.RecognizeMode.Multiple)

        synth.SetOutputToDefaultAudioDevice()
        synth.SelectVoiceByHints(Speech.Synthesis.VoiceGender.Female, Speech.Synthesis.VoiceAge.Teen)
        synth.Speak("Hello, my name is Alis. How may I help you today?")
    End Sub

    Private Sub CloseVoices()
        sre.RecognizeAsyncCancel()
    End Sub

    Private Sub sre_SpeechRecognized(sender As Object, e As Speech.Recognition.SpeechRecognizedEventArgs)
        Dim txt As String = e.Result.Text
        Dim conf As Single = e.Result.Confidence
        If conf < 0.65 Then
            synth.Speak("I am sorry, I could not understand that")
            Return
        End If

        Select Case txt
            Case "Alis what is M-RAM's status"
                Status_Speaker()
            Case "Alis why can't M-RAM run"
                ExcuseMaker()
        End Select



    End Sub

    Private Sub ExcuseMaker()


        If Ping_Prev <> Robot_Status.Ready OrElse Tick_Prev <> Robot_Status.Ready OrElse Razz_Prev <> Robot_Status.Ready Then
            Dim excuse As String
            excuse = "Because"

            If Ping_Prev <> Robot_Status.Ready Then excuse += (" Ping and Pong are in " & Ping_Prev.ToString().Replace("_", " "))
            If Tick_Prev <> Robot_Status.Ready Then excuse += (", Tick and Tock are in " & Tick_Prev.ToString().Replace("_", " "))
            If Razz_Prev <> Robot_Status.Ready Then excuse += (", Razzle and Dazzle are in " & Razz_Prev.ToString().Replace("_", " "))
            If Not StartUp.Default.Ready Then excuse += (", and M-RAM has not been cleared to run by the correct user")

            synth.Speak(excuse)
        Else
            If Ping_Prev = Robot_Status.Ready AndAlso Tick_Prev = Robot_Status.Ready AndAlso Razz_Prev = Robot_Status.Ready And StartUp.Default.Ready Then synth.Speak("M-RAM is ready to run according to me")

        End If

    End Sub

    Private Sub Status_Speaker()
        synth.Speak("Ping and Pong are in " & Ping_Prev.ToString().Replace("_", " "))
        synth.Speak("Tick and Tock are in " & Tick_Prev.ToString().Replace("_", " "))
        synth.Speak("Razzle and Dazzle are in " & Razz_Prev.ToString().Replace("_", " "))
        If Ping_Prev = Robot_Status.Ready AndAlso Tick_Prev = Robot_Status.Ready AndAlso Razz_Prev = Robot_Status.Ready And StartUp.Default.Ready Then
            synth.Speak("All Robots are ready to run")
        ElseIf Ping_Prev = Robot_Status.Ready AndAlso Tick_Prev = Robot_Status.Ready AndAlso Razz_Prev = Robot_Status.Ready And Not StartUp.Default.Ready Then
            synth.Speak("M-RAM has not been cleared to run by the correct user")
        Else
            synth.Speak("Please refer to the help menu to prepare the robots to run")
        End If

    End Sub
#End Region

#Region "Graphing"
    Private Sub Graph_Speed()
        While GraphRun


            Dim Rob_Target As RapidDomain.Pos
            Dim Tab_Active As Boolean = False
            Dim Zero_pos As RapidDomain.Pos = Nothing
            Dim Tick_Point As New LiveCharts.Defaults.ObservablePoint
            Dim Tock_Point As New LiveCharts.Defaults.ObservablePoint
            Dim Ping_Point As New LiveCharts.Defaults.ObservablePoint
            Dim Pong_Point As New LiveCharts.Defaults.ObservablePoint
            Dim Razz_Point As New LiveCharts.Defaults.ObservablePoint
            Dim Dazz_Point As New LiveCharts.Defaults.ObservablePoint
            Tick_Point.X = counter
            Tock_Point.X = counter
            Ping_Point.X = counter
            Pong_Point.X = counter
            Razz_Point.X = counter
            Dazz_Point.X = counter
            Tick_Point.Y = 0
            Tock_Point.Y = 0
            Ping_Point.Y = 0
            Pong_Point.Y = 0
            Razz_Point.Y = 0
            Dazz_Point.Y = 0



            Try
                counter += 1
                If Tick_Tock IsNot Nothing AndAlso Tick_Tock.Connected Then
                    Rob_Target = Tick_Tock.MotionSystem.MechanicalUnits.Item(0).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Tick_pos.Equals(Zero_pos) Then Tick_pos = Rob_Target
                    Tick_Point.Y = Round(Math.Sqrt((Rob_Target.X - Tick_pos.X) ^ 2 + (Rob_Target.Y - Tick_pos.Y) ^ 2 + (Rob_Target.Z - Tick_pos.Z) ^ 2), 2)
                    Tick_pos = Rob_Target
                    Rob_Target = Tick_Tock.MotionSystem.MechanicalUnits.Item(1).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Tock_pos.Equals(Zero_pos) Then Tock_pos = Rob_Target
                    Tock_Point.Y = Round(Math.Sqrt((Rob_Target.X - Tock_pos.X) ^ 2 + (Rob_Target.Y - Tock_pos.Y) ^ 2 + (Rob_Target.Z - Tock_pos.Z) ^ 2), 2)
                    Tock_pos = Rob_Target


                End If
                If Ping_Pong IsNot Nothing AndAlso Ping_Pong.Connected Then
                    Rob_Target = Ping_Pong.MotionSystem.MechanicalUnits.Item(0).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Ping_pos.Equals(Zero_pos) Then Ping_pos = Rob_Target
                    Ping_Point.Y = Round(Math.Sqrt((Rob_Target.X - Ping_pos.X) ^ 2 + (Rob_Target.Y - Ping_pos.Y) ^ 2 + (Rob_Target.Z - Ping_pos.Z) ^ 2), 2)
                    Ping_pos = Rob_Target
                    Rob_Target = Ping_Pong.MotionSystem.MechanicalUnits.Item(1).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Pong_pos.Equals(Zero_pos) Then Pong_pos = Rob_Target
                    Pong_Point.Y = Round(Math.Sqrt((Rob_Target.X - Pong_pos.X) ^ 2 + (Rob_Target.Y - Pong_pos.Y) ^ 2 + (Rob_Target.Z - Pong_pos.Z) ^ 2), 2)
                    Pong_pos = Rob_Target


                End If
                If Razzle_Dazzle IsNot Nothing AndAlso Razzle_Dazzle.Connected Then
                    Rob_Target = Razzle_Dazzle.MotionSystem.MechanicalUnits.Item(0).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Razz_pos.Equals(Zero_pos) Then Razz_pos = Rob_Target
                    Razz_Point.Y = Round(Math.Sqrt((Rob_Target.X - Razz_pos.X) ^ 2 + (Rob_Target.Y - Razz_pos.Y) ^ 2 + (Rob_Target.Z - Razz_pos.Z) ^ 2), 2)
                    Razz_pos = Rob_Target
                    Rob_Target = Razzle_Dazzle.MotionSystem.MechanicalUnits.Item(1).GetPosition(MotionDomain.CoordinateSystemType.World).Trans
                    If Dazz_pos.Equals(Zero_pos) Then Dazz_pos = Rob_Target
                    Dazz_Point.Y = Round(Math.Sqrt((Rob_Target.X - Dazz_pos.X) ^ 2 + (Rob_Target.Y - Dazz_pos.Y) ^ 2 + (Rob_Target.Z - Dazz_pos.Z) ^ 2), 2)
                    Dazz_pos = Rob_Target
                End If
                Razz_Values.Add(Razz_Point)
                Dazz_Values.Add(Dazz_Point)
                Ping_Values.Add(Ping_Point)
                Pong_Values.Add(Pong_Point)
                Tick_Values.Add(Tick_Point)
                Tock_Values.Add(Tock_Point)
                If Tick_Values.Count > 10 Then Tick_Values.RemoveAt(0)
                If Tock_Values.Count > 10 Then Tock_Values.RemoveAt(0)
                If Razz_Values.Count > 10 Then Razz_Values.RemoveAt(0)
                If Dazz_Values.Count > 10 Then Dazz_Values.RemoveAt(0)
                If Ping_Values.Count > 10 Then Ping_Values.RemoveAt(0)
                If Pong_Values.Count > 10 Then Pong_Values.RemoveAt(0)
                Dispatcher.Invoke(
                    Sub()
                        Tab_Active = tabHome.IsSelected
                    End Sub)
                If Tab_Active Then
                    If counter >= 10 Then
                        Dispatcher.Invoke(
                            Sub()
                                Razz_Line.Values = Razz_Values
                                Dazz_Line.Values = Dazz_Values
                                Tick_Line.Values = Tick_Values
                                Tock_Line.Values = Tock_Values
                                Ping_Line.Values = Ping_Values
                                Pong_Line.Values = Pong_Values
                                axsx.MinValue = counter - 9
                            End Sub)
                    Else
                        Dispatcher.Invoke(
                                Sub()
                                    Razz_Line.Values = Razz_Values
                                    Dazz_Line.Values = Dazz_Values
                                    Tick_Line.Values = Tick_Values
                                    Tock_Line.Values = Tock_Values
                                    Ping_Line.Values = Ping_Values
                                    Pong_Line.Values = Pong_Values
                                    axsx.MinValue = 0
                                End Sub)
                    End If
                End If
            Catch e As Exception
                'MessageBox.Show(e.ToString)
            End Try
            Threading.Thread.Sleep(1000)
        End While
        Threading.Thread.Sleep(1000)
    End Sub


    Private Sub Initialize_Graph()
        chtRobot.Series = New LiveCharts.SeriesCollection
        Tick_Line.Title = "Tick"
        Ping_Line.Title = "Ping"
        Razz_Line.Title = "Razzle"
        Tock_Line.Title = "Tock"
        Pong_Line.Title = "Pong"
        Dazz_Line.Title = "Dazzle"
        Tick_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Tock_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Ping_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Pong_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Razz_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Dazz_Values.Add(New LiveCharts.Defaults.ObservablePoint(0, 0))
        Tick_Line.Values = Tick_Values
        Tock_Line.Values = Tock_Values
        Ping_Line.Values = Ping_Values
        Pong_Line.Values = Pong_Values
        Razz_Line.Values = Razz_Values
        Dazz_Line.Values = Dazz_Values
        chtRobot.Series.Add(Tick_Line)
        chtRobot.Series.Add(Razz_Line)
        chtRobot.Series.Add(Ping_Line)
        chtRobot.Series.Add(Tock_Line)
        chtRobot.Series.Add(Pong_Line)
        chtRobot.Series.Add(Dazz_Line)
        axsx.MinValue = 0
    End Sub
#End Region

#Region "Logging"
    Private Sub Log_Ping()
        If Ping_Prev = Robot_Check(Ping_Pong) Then Exit Sub
        Ping_Prev = Robot_Check(Ping_Pong)
        Dim Write_String As String

        Write_String = "Ping Pong " & Ping_Prev.ToString.Replace("_", " ") & " " & System.DateTime.Now.ToString
        Dispatcher.Invoke(
            Sub()
                lstbLog.Items.Add(Write_String)
            End Sub)
    End Sub

    Private Sub Log_Tick()
        If Tick_Prev = Robot_Check(Tick_Tock) Then Exit Sub
        Tick_Prev = Robot_Check(Tick_Tock)
        Dim Write_String As String

        Write_String = "Tick Tock " & Tick_Prev.ToString.Replace("_", " ") & " " & System.DateTime.Now.ToString
        Dispatcher.Invoke(
            Sub()
                lstbLog.Items.Add(Write_String)
            End Sub)
    End Sub

    Private Sub Log_Razz()
        If Razz_Prev = Robot_Check(Razzle_Dazzle) Then Exit Sub
        Razz_Prev = Robot_Check(Razzle_Dazzle)
        Dim Write_String As String

        Write_String = "Razzle Dazzle " & Razz_Prev.ToString.Replace("_", " ") & " " & System.DateTime.Now.ToString
        Dispatcher.Invoke(
            Sub()
                lstbLog.Items.Add(Write_String)
            End Sub)
    End Sub


#End Region

    Private Sub BtnStop_Click(sender As Object, e As RoutedEventArgs) Handles btnStop.Click
        If Ping_Pong IsNot Nothing Then Ping_Pong.Rapid.Stop(RapidDomain.StopMode.Immediate)
        If Tick_Tock IsNot Nothing Then Tick_Tock.Rapid.Stop(RapidDomain.StopMode.Immediate)
        If Razzle_Dazzle IsNot Nothing Then Razzle_Dazzle.Rapid.Stop(RapidDomain.StopMode.Immediate)
    End Sub

    Private Sub btn3d_Click() Handles btn3d.Click

    End Sub
End Class
