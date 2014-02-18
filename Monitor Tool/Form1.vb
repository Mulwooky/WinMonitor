Public Class Form1
    Dim tagger As String
    Dim Response As Boolean
    Dim Alive As String
    Dim useronline As String
    Dim strUserName As String
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        csvExport.Visible = False
        DataGridView1.AllowUserToAddRows = False
        status1.Text = "0 machines in queue..."
        Me.DataGridView1.Columns.Item(0).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        Me.DataGridView1.Columns.Item(1).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        Me.DataGridView1.Columns.Item(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
        ToolTip1.SetToolTip(Admin, "Admin account is advised to allow enumeration of logged on users .")
    End Sub
    Private Sub TextBoxDrop_DragEnter(ByVal sender As Object, _
                ByVal e As System.Windows.Forms.DragEventArgs) _
                Handles TextBoxDrop.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.All
        End If
    End Sub
    Public Sub TextBoxDrop_DragDrop(ByVal sender As Object, _
                ByVal e As System.Windows.Forms.DragEventArgs) _
                Handles TextBoxDrop.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim MyFiles() As String
            MyFiles = e.Data.GetData(DataFormats.FileDrop)
            TextBoxDrop.Text = My.Computer.FileSystem.ReadAllText(MyFiles(0))
        End If
        status1.Text = TextBoxDrop.Lines.Length & " machines in queue"
        ProgressBar1.Maximum = TextBoxDrop.Lines.Length
        For X = 0 To Me.TextBoxDrop.Lines.Length
            status1.Text = X & "/" & TextBoxDrop.Lines.Length & " machines in queue " & "(" & TextBoxDrop.Lines(X) & ")"
            tagger = TextBoxDrop.Lines(X).ToString
            DataGridView1.Rows.Add(tagger)
            Try
                If My.Computer.Network.Ping(tagger, 1000) Then
                    Me.DataGridView1.Rows(X).Cells(1).Value = "Yes"
                End If
            Catch ex As Exception
                Me.DataGridView1.Rows(X).Cells(1).Value = "No"
            End Try
            For i = 0 To DataGridView1.Rows.Count - 1
                If DataGridView1.Rows(i).Cells(1).Value = "Yes" Then
                    DataGridView1.Rows(i).DefaultCellStyle.BackColor = Color.LightSkyBlue
                Else
                    If DataGridView1.Rows(i).Cells(1).Value = "No" Then
                        DataGridView1.Rows(i).DefaultCellStyle.BackColor = Color.Red
                    End If
                End If
            Next

            Try
                For Each row As DataGridViewRow In Me.DataGridView1.Rows
                    If Not row.IsNewRow Then
                        If Not row.Cells(1).Value Is DBNull.Value Then
                            If Me.DataGridView1.Rows(X).Cells(1).Value = "Yes" Then
                                Dim objWMIService = GetObject("winmgmts:" & "{impersonationLevel=impersonate}!\\" & tagger & "\root\cimv2")
                                Dim colLoggedOn = objWMIService.ExecQuery("Select UserName from Win32_ComputerSystem")
                                For Each objItem In colLoggedOn
                                    strUserName = objItem.UserName
                                Next
                                Me.DataGridView1.Rows(X).Cells(2).Value = strUserName
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
            End Try
            ProgressBar1.Value += 1
            If ProgressBar1.Value = TextBoxDrop.Lines.Length Then
                ProgressBar1.Value = 0
                ProgressBar1.Visible = False
                status1.Visible = False
                csvExport.Visible = True
                status1.Text = "Scan complete"
                DataGridView1.Sort(DataGridView1.Columns(1), System.ComponentModel.ListSortDirection.Descending)
                DataGridView1.Sort(DataGridView1.Columns(2), System.ComponentModel.ListSortDirection.Descending)
            End If
        Next

    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Application.Restart()
    End Sub
    Public Sub DataGridView1_CellClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.ColumnIndex = 0 Then
            Dim RDPStart As String = DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString
            Try
                If My.Computer.Network.Ping(RDPStart) And DataGridView1.Rows(e.RowIndex).Cells(2).Value = "" Then
                    System.Diagnostics.Process.Start("mstsc.exe", "/v:" & RDPStart)
                End If
                If DataGridView1.Rows(e.RowIndex).Cells(2).Value <> "" Then
                    MsgBox("Machine is in use, starting a remote session will end the current users session", vbCritical)
                End If
            Catch ex As Exception
                MsgBox("Computer is unavailable, connection cannot be made.")
            End Try
        End If
    End Sub
    Private Sub ProjectsDataGridView_CellMouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles DataGridView1.CellMouseClick
        If (e.Button = Windows.Forms.MouseButtons.Right) Then
            If DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = "" Then
                MsgBox("Please click on a machine name to start remote session")
            Else
                Try
                    DataGridView1.CurrentRow.Selected = False
                    DataGridView1.Rows(e.RowIndex).Selected = True
                    DataGridView1.CurrentCell = DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)
                    System.Diagnostics.Process.Start("compmgmt.msc", "/computer=" & DataGridView1.CurrentCell.Value.ToString)
                Catch ex As Exception
                    MsgBox(ex.Message)
                End Try
            End If
        End If
    End Sub
    Private Sub Admin_MouseHover(sender As Object, e As EventArgs) Handles Admin.MouseHover
        Me.ToolTip1.Active = True
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles csvExport.Click

        Dim StrExport As String = ""
        For Each C As DataGridViewColumn In DataGridView1.Columns
            StrExport &= """" & C.HeaderText & ""","
        Next
        StrExport = StrExport.Substring(0, StrExport.Length - 1)
        StrExport &= Environment.NewLine

        For Each R As DataGridViewRow In DataGridView1.Rows
            For Each C As DataGridViewCell In R.Cells
                If Not C.Value Is Nothing Then
                    StrExport &= """" & C.Value.ToString & ""","
                Else
                    StrExport &= """" & "" & ""","
                End If
            Next
            StrExport = StrExport.Substring(0, StrExport.Length - 1)
            StrExport &= Environment.NewLine
        Next

        SaveFileDialog1.Filter = "CSV Files (*.csv*)|*.csv"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
        Then
            SaveFileDialog1.OverwritePrompt = True
            My.Computer.FileSystem.WriteAllText(SaveFileDialog1.FileName, "Date of export " & DateTime.Now.ToString("dd/MM/yyyy HH:mm") & Environment.NewLine & Environment.NewLine & StrExport, False)
        End If
    End Sub
End Class
