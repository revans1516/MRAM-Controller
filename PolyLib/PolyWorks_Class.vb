
Public Class PolyWorks_Class
    'This Class requires The IMInspectLib and IMWorksapceManagerLib to be referenced
    Public Poly_Process As New Process
    Public Inspect_Process As New Process
    Dim PolyMan As IMWorkspaceManagerLib.IMWorkspaceManager = Nothing
    Public PolyManCmd As IMWorkspaceManagerLib.IIMCommandCenter = Nothing
    Dim PolyIm As IMInspectLib.IMInspect = Nothing
    Dim PolyPro As IMInspectLib.IIMInspectProject = Nothing
    Public PolyIsCmd As IMInspectLib.IIMCommandCenter = Nothing



    Public Sub New()
        PolyMan = New IMWorkspaceManagerLib.IMWorkspaceManager
        PolyMan.CommandCenterCreate(PolyManCmd)
        PolyManCmd.CommandExecute("WINDOW APPLICATION MINIMIZE")
        PolyIm = New IMInspectLib.IMInspect
        PolyIm.ProjectGetCurrent(PolyPro)
        PolyPro.CommandCenterCreate(PolyIsCmd)
    End Sub

    Public Sub Close()
        PolyManCmd.CommandExecute("FILE CLOSE_WORKSPACE (  )")
        PolyManCmd.CommandExecute("FILE EXIT")
        PolyIsCmd = Nothing
        PolyPro = Nothing
        PolyIm = Nothing
        PolyManCmd = Nothing
        PolyMan = Nothing
    End Sub

    Public Sub Connect_To_Probe(Device_Name As String, IP_Address As String)
        With PolyIsCmd
            .CommandExecute("PROBE DEVICE LEICA AT9X0_LASER_TRACKER ADDRESS ( """ & IP_Address & """, )")
            .CommandExecute("PROBE DEVICE ( """ & Device_Name & """ )")
            .CommandExecute("PROBE DEVICE CONNECT ( ""On"", )")
        End With
    End Sub

    Public Sub Delete_Object(Del_Object As String)
        PolyIsCmd.CommandExecute("EDIT OBJECT DELETE (""" & Del_Object & """)")
    End Sub

    Public Sub Connect_To_Scanner()
        With PolyIsCmd
            .CommandExecute("DIGITIZE DEVICE (""Leica T-Scan/Tracker"")")
            .CommandExecute("DIGITIZE DEVICE CONNECT ( ""On"" )")
            .CommandExecute("DIGITIZE DEVICE SCAN SURFACE DATA_OBJECT NAME ( ""surface scan 1"" )")
            .CommandExecute("DIGITIZE LINE_SCAN SURFACE REAL_TIME_QUALITY_MESHING ( ""Off"" )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN EXPOSURE_TIME (1)")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN REFLECTION_FILTER_TYPE ( ""Low"" )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN LINE_WIDTH ( 90 )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN MAX_ANGLE_OF_INCIDENCE ( 65 )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN USE_MAX_ANGLE_OF_INCIDENCE ( ""On"" )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN POINT_TO_POINT_DISTANCE ( 0.075 )")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN MIN_LINE_TO_LINE_DISTANCE ( 0.0 )")
            .CommandExecute("DIGITIZE DEVICE SCAN TYPE ( ""Surface And Boundary"" )")
            .CommandExecute("DIGITIZE DEVICE SCAN SURFACE_AND_BOUNDARY INTERNAL_BOUNDARIES MIN_HOLE_WIDTH( 2 )")
        End With
    End Sub

    Public Sub Disconnect_Scanner()
        PolyIsCmd.CommandExecute("DIGITIZE DEVICE (""Leica T-Scan/Tracker"")")
        PolyIsCmd.CommandExecute("DIGITIZE DEVICE CONNECT ( ""Off"" )")
    End Sub

    Public Sub Start_Scan()
        With PolyIsCmd
            .CommandExecute("MACRO INTERACTIVE_MODE SCRIPTED_MODE BEGIN")
            .CommandExecute("DIGITIZE DEVICE SCAN START")
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN MEASUREMENT START")
        End With
    End Sub

    Public Sub Point_Scanner(Location As Double())
        Dim command As String = "DIGITIZE DEVICE LEICA TSCAN_TRACKER GO_XYZ ("
        command = command & Location(0).ToString & ", "
        command = command & Location(1).ToString & ", "
        command = command & Location(2).ToString & ")"
        PolyIsCmd.CommandExecute(command)
    End Sub


    Public Sub Create_Point(Location As Double())
        Dim command As String = "FEATURE PRIMITIVE POINT CREATE ( "
        command = command & Location(0).ToString & ", "
        command = command & Location(1).ToString & ", "
        command = command & Location(2).ToString & ")"
        PolyIsCmd.CommandExecute(command)
    End Sub


    Public Sub Stop_Scan()
        With PolyIsCmd
            .CommandExecute("DIGITIZE DEVICE LEICA TSCAN_TRACKER SCAN MEASUREMENT STOP")
            .CommandExecute("DIGITIZE DEVICE SCAN END")
            .CommandExecute("MACRO INTERACTIVE_MODE SCRIPTED_MODE END")
        End With
    End Sub

    'The location needs to be xyz and yaw pitch roll
    Public Sub Create_Frame(Frame_Name As String, Location As Double())
        PolyIsCmd.CommandExecute("ALIGN COORDINATE_SYSTEM CARTESIAN CREATE FROM_TRANSLATION_ROTATION ( " & Location(0) & "," & Location(1) & "," & Location(2) & "," & Location(3) & "," & Location(4) & "," & Location(5) & ",""ZYX"", """ & Frame_Name & """)")
        PolyIsCmd.CommandExecute("ALIGN COORDINATE_SYSTEM ACTIVE ( """ & Frame_Name & """ )")
    End Sub

    'Public Function Get_Frame() As Double()
    '    Dim Output() As Double
    '    PolyIsCmd.

    '    Return Output
    'End Function

    Public Sub Start_AAC()
        PolyIsCmd.CommandExecute("AAC START_APPLICATION")
    End Sub

    Public Sub Stop_AAC()
        PolyIsCmd.CommandExecute("AAC END_APPLICATION")
    End Sub

    Public Sub AAC_Connect_Tracker(ByVal IPAddress As String)
        PolyIsCmd.CommandExecute("AAC SETTRACKERIP (""" & IPAddress & """)""")
        PolyIsCmd.CommandExecute("AAC CONNECT_TRACKER")
    End Sub

    Public Sub AAC_Disconnect_Tracker()
        PolyIsCmd.CommandExecute("AAC DISCONNECT_TRACKER")
    End Sub

End Class
