Class Application

    Public splash As New System.Windows.SplashScreen("/Icons\Auto.png")
    Private Sub Application_Startup(ByVal sender As Object, ByVal e As System.Windows.StartupEventArgs) Handles Me.Startup
        splash.Show(True, True)
    End Sub
End Class
