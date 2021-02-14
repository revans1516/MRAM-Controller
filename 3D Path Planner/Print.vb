Public Module Print

    Dim strFileName As String = Nothing
    Dim FolderLoc As String = Nothing

    Private Function SelectFile() As String

        Using dialog As New System.Windows.Forms.OpenFileDialog()
            dialog.Filter = ".gcode"

            dialog.ShowDialog()
            strFileName = dialog.FileName
        End Using

        Return strFileName
    End Function

    Private Sub ConvertGCode()
        Dim filereader As System.IO.StreamReader
        Dim FileCounter As Integer = 0
        Dim CurrtentPrint As Boolean = False
        filereader = My.Computer.FileSystem.OpenTextFileReader(strFileName)

        While Not filereader.EndOfStream
            FileCounter += 1
            Dim FileWriter As New System.IO.StreamWriter(FolderLoc & "/" & "Print" & FileCounter & ".mod")
            FileWriter.WriteLine("MODULE Print" & FileCounter)
            FileWriter.WriteLine("PROC Printing" & FileCounter & "()")



            While FileCounter < 1000 And Not filereader.EndOfStream
                Dim CurrentLine As String = Nothing
                CurrentLine = filereader.ReadLine()
                If CurrentLine.Substring(0, 2).ToLower = "g1" Then

                End If
            End While

            FileWriter.WriteLine("ENDPROC")
            FileWriter.Close()


        End While

    End Sub


End Module
