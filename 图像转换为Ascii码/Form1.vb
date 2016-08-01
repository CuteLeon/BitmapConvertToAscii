Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class Form1
    Dim CellSize As Size = New Size(5, 10)
    Dim AsciiCount As Int16 = 32
    Dim Ascii() As Char = {" ", "`", ".", "^", ",", ":", "~", """", "<", "!", "c", "t", "+", "{", "i", "7", "?", "u", "3", "0", "p", "w", "4", "A", "8", "D", "X", "%", "#", "H", "W", "M"}
    Dim AscGray() As Byte = {0, 14, 19, 25, 36, 42, 48, 53, 59, 65, 70, 76, 82, 87, 93, 99, 104, 110, 116, 121, 127, 133, 138, 144, 150, 155, 167, 172, 178, 187, 192, 198}
    Dim TestBitmap As Bitmap
    Dim mFrameCount As Integer
    Dim mTimeDimension As FrameDimension
    Dim FrameBitmap As Bitmap
    Dim FrameGraphics As Graphics
    Dim mFrameIndex As Integer

    '@Leon：算法原理：
    '   1>.对图像进行去色处理转换为灰阶图
    '   2>.对灰阶图分块取灰度
    '   3>.匹配灰度最接近的Ascii码

    '# 图像转换为Ascii码 #
    'Function BitmapToAscii(ByVal GrayScaleBitmap As Bitmap, ByVal CellSize As Size) As String
    '# 使用二分法匹配灰度最相近的Ascii码 #
    'Function AsciiFromGrayScale(ByVal GrayScale As Byte) As Char
    '# 对图像进行去色处理 #
    'Function GrayScaleBitmap(ByVal InitialBitmap As Bitmap) As Bitmap

    Private Function GrayScaleBitmap(ByVal InitialBitmap As Bitmap) As Bitmap
        Dim GrayBitmapData As Imaging.BitmapData = New Imaging.BitmapData
        GrayBitmapData = InitialBitmap.LockBits(New Rectangle(0, 0, InitialBitmap.Width, InitialBitmap.Height),
                Imaging.ImageLockMode.WriteOnly, InitialBitmap.PixelFormat)
        Dim DataStride As Integer = GrayBitmapData.Stride
        Dim DataHeight As Integer = GrayBitmapData.Height
        Dim GrayDataArray(DataStride * DataHeight - 1) As Byte
        Marshal.Copy(GrayBitmapData.Scan0, GrayDataArray, 0, GrayDataArray.Length)

        Dim Index As Integer, Gray As Byte
        For Index = 0 To GrayDataArray.Length - 1 Step 4
            Gray = GrayDataArray(Index + 2) * 0.229 + GrayDataArray(Index + 1) * 0.587 + GrayDataArray(Index + 0) * 0.114
            GrayDataArray(Index + 0) = Gray
            GrayDataArray(Index + 1) = Gray
            GrayDataArray(Index + 2) = Gray
        Next

        Marshal.Copy(GrayDataArray, 0, GrayBitmapData.Scan0, GrayDataArray.Length)
        InitialBitmap.UnlockBits(GrayBitmapData)
        Return InitialBitmap
    End Function

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        If Not CustomImageDialog.ShowDialog = DialogResult.OK Then Exit Sub
        TestBitmap = New Bitmap(CustomImageDialog.FileName)

        Try
            mFrameCount = TestBitmap.GetFrameCount(Imaging.FrameDimension.Time)
        Catch ex As Exception
            mFrameCount = 1
        End Try
        If mFrameCount = 1 Then
            GIFTimer.Stop()
            TestBitmap = GrayScaleBitmap(TestBitmap) '先转换为灰阶图像
            TextBox1.Text = BitmapToAscii(TestBitmap, CellSize) '对图像分块进行Ascii灰度比对
            TextBox1.SelectionStart = 0
            TextBox1.SelectionLength = 0
        Else
            mFrameIndex = 0
            mTimeDimension = New FrameDimension(TestBitmap.FrameDimensionsList.First)
            GIFTimer.Start()
        End If
    End Sub
    Private Sub ToolStripTextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ToolStripTextBox1.KeyPress
        If e.KeyChar <> Chr(Keys.Enter) Then Exit Sub
        If IsNumeric(ToolStripTextBox1.Text) Then
            CellSize.Width = Int(ToolStripTextBox1.Text.Trim)
            CellSize.Height = CellSize.Width * 2
            TextBox1.Text = BitmapToAscii(TestBitmap, CellSize) '对图像分块进行Ascii灰度比对
        End If
    End Sub

    Private Sub 复制到剪切板ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 复制到剪切板ToolStripMenuItem.Click
        My.Computer.Clipboard.SetText(TextBox1.Text)
        Me.Text = "文本已经复制到剪切板！ - " & My.Computer.Clock.LocalTime
    End Sub

    Private Function BitmapToAscii(ByVal GrayScaleBitmap As Bitmap, ByVal CellSize As Size) As String
        Dim AsciiString As String = vbNullString
        Dim IndexX, IndexY As Integer
        Dim CellBitmap As Bitmap
        Dim CellGrayScale As Byte
        For IndexY = 0 To GrayScaleBitmap.Height - 1 - CellSize.Height Step CellSize.Height
            For IndexX = 0 To GrayScaleBitmap.Width - 1 - CellSize.Width Step CellSize.Width
                CellBitmap = GrayScaleBitmap.Clone(New Rectangle(IndexX, IndexY, CellSize.Width, CellSize.Height), Imaging.PixelFormat.Format32bppArgb)
                CellBitmap = New Bitmap(CellBitmap, 1, 1)
                CellGrayScale = 255 - CellBitmap.GetPixel(0, 0).R
                AsciiString &= AsciiFromGrayScale(CellGrayScale)
            Next
            AsciiString &= vbCrLf
        Next

        Return Strings.Left(AsciiString, AsciiString.Length - 1)
    End Function

    Private Function AsciiFromGrayScale(ByVal GrayScale As Byte) As Char
        Dim lower As Byte = 0
        Dim higher As Byte = AsciiCount
        Dim Mid As Byte
        While ((higher - lower) > 1)
            Mid = (lower + higher) / 2
            If (GrayScale > AscGray(Mid)) Then
                lower = Mid
            Else
                higher = Mid
            End If
        End While
        Return Ascii(lower)
    End Function

    Private Sub TextBox1_DoubleClick(sender As Object, e As EventArgs) Handles TextBox1.DoubleClick
        MenuStrip1.Visible = Not MenuStrip1.Visible
    End Sub

    Private Sub GIFTimer_Tick(sender As Object, e As EventArgs) Handles GIFTimer.Tick
        TestBitmap.SelectActiveFrame(mTimeDimension, mFrameIndex)

        FrameBitmap = Nothing
        FrameBitmap = New Bitmap(TestBitmap.Width, TestBitmap.Height)
        FrameGraphics = Graphics.FromImage(FrameBitmap)
        FrameGraphics.DrawImageUnscaled(TestBitmap, 0, 0)
        FrameGraphics.Dispose()
        FrameBitmap = GrayScaleBitmap(FrameBitmap) '先转换为灰阶图像
        TextBox1.Text = BitmapToAscii(FrameBitmap, CellSize) '对图像分块进行Ascii灰度比对
        TextBox1.SelectionStart = 0
        TextBox1.SelectionLength = 0
        Me.Text = mFrameIndex & " / " & mFrameCount
        mFrameIndex = IIf(mFrameIndex = mFrameCount - 1, 0, mFrameIndex + 1)
    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        Dim SaveBitmap As Bitmap = New Bitmap(1, 1)
        Dim SaveGraphics As Graphics = Graphics.FromImage(SaveBitmap)
        Dim FontHeight As Integer = TextBox1.Font.GetHeight(SaveGraphics)
        Try
            SaveBitmap = New Bitmap(TextBox1.Lines.First.Length * 6, FontHeight * (TextBox1.Lines.Count - 1))
            SaveGraphics = Graphics.FromImage(SaveBitmap)
            SaveGraphics.FillRectangle(Brushes.White, 0, 0, SaveBitmap.Width, SaveBitmap.Height)
            SaveGraphics.DrawString(TextBox1.Text, TextBox1.Font, Brushes.Black, 0, 0)
            SaveBitmap.Save(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) & "\" & My.Computer.Clock.TickCount & ".png", Imaging.ImageFormat.Png)
            Me.Text = "文件已保存到桌面！ - " & My.Computer.Clock.LocalTime
        Catch ex As Exception
            Me.Text = ex.Message & " -  & My.Computer.Clock.LocalTime" & My.Computer.Clock.LocalTime
        Finally
            SaveGraphics.Dispose()
            SaveBitmap = Nothing
        End Try
    End Sub

End Class
