Public Class InstructionsWindow

#Region "Windows Functions"
    Private Sub Switch() Handles tvwhelp.SelectedItemChanged
        If Me.IsLoaded Then
            Grd1.Visibility = Visibility.Collapsed
            Grd2.Visibility = Visibility.Collapsed
            Grd3.Visibility = Visibility.Collapsed
            Grd4.Visibility = Visibility.Collapsed
            Grd5.Visibility = Visibility.Collapsed
            Grd6.Visibility = Visibility.Collapsed
            Grd7.Visibility = Visibility.Collapsed
            Grd8.Visibility = Visibility.Collapsed
            Grd9.Visibility = Visibility.Collapsed
            Grd10.Visibility = Visibility.Collapsed
            Select Case Loopertree(tvwhelp)
                Case 0
                    Grd1.Visibility = Visibility.Visible
                Case 1
                    Grd2.Visibility = Visibility.Visible
                Case 2
                    Grd3.Visibility = Visibility.Visible
                Case 3
                    Grd4.Visibility = Visibility.Visible
                Case 4
                    Grd5.Visibility = Visibility.Visible
                Case 5
                    Grd6.Visibility = Visibility.Visible
                Case 6
                    Grd7.Visibility = Visibility.Visible
                Case 7
                    Grd8.Visibility = Visibility.Visible
                Case 8
                    Grd9.Visibility = Visibility.Visible
                Case 9
                    Grd10.Visibility = Visibility.Visible
            End Select
        End If
    End Sub

    Private Function Looper(Item As TreeViewItem, ByRef Loopint As Integer) As Boolean
        If Item.IsSelected Then Return True
        For Each tvitem As TreeViewItem In Item.Items
            Loopint += 1
            If tvitem.IsSelected Then
                Return True
            End If
            If tvitem.HasItems Then
                Dim tempint As Integer
                tempint = Looper(tvitem, Loopint)
                If tempint <> -1 Then Return False
            End If
        Next
        Return False
    End Function

    Private Function Loopertree(Item As TreeView) As Integer
        Dim ItemCounter As Integer = 0
        For Each tvitem As TreeViewItem In Item.Items
            If Looper(tvitem, ItemCounter) Then Return ItemCounter
            ItemCounter += 1
        Next
        Return ItemCounter
    End Function

    Private Sub Dragging() Handles grdControls.MouseDown
        If Me.WindowState = WindowState.Maximized Then Me.WindowState = WindowState.Normal
        Me.DragMove()
    End Sub

    Private Sub WinClose() Handles btnClose.Click
        Me.Close()
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
#End Region

End Class
