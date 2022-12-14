Imports System.IO
Imports System.Runtime.CompilerServices
Imports AForge.Video.DirectShow
Imports ClosedXML.Excel
Imports DocumentFormat.OpenXml.Drawing.Wordprocessing
Imports Emgu.CV
Imports Emgu.CV.CvEnum
Imports GeometRi
Imports Size = System.Drawing.Size

''' <summary>
''' This class contains all the functions that been  attched to Controls.
''' </summary>
Public Module ControlsMethods
#Region "PictureBox Methods"

    ''' <summary>
    ''' Zoom image by zoom_factor.
    ''' </summary>
    ''' <paramname="zoom_factor">The factor for zooming in or zooming out.</param>
    ''' <paramname="ori">The list of original images.</param>
    ''' <paramname="cur">The list of images currently used in picturebox.</param>
    ''' <paramname="tab_index">The index of tab control.</param>
    Public Function ZoomImage(ByVal zoom_factor As Double, ByVal ori As List(Of Mat), ByVal cur As List(Of Mat), ByVal tab_index As Integer) As Mat
        Dim ori_img = ori.ElementAt(tab_index)
        Dim s As Size = New Size(Convert.ToInt32(ori_img.Width * zoom_factor), Convert.ToInt32(ori_img.Height * zoom_factor))
        Dim cur_img = cur.ElementAt(tab_index)
        Dim dst_img As Mat = New Mat()
        CvInvoke.Resize(ori_img, dst_img, s)
        Return dst_img
    End Function

    ''' <summary>
    ''' put image in the center of picturebox control.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control which you want to put in the center.</param>
    ''' <paramname="panel">The panel which includes a picturebox control.</param>
    <Extension()>
    Public Function CenteringImage(ByVal pictureBox As PictureBox, ByVal panel As Panel) As Point
        Dim left_top As Point = New Point((panel.Width - pictureBox.Width) / 2, (panel.Height - pictureBox.Height) / 2)
        Dim scroll_pos As Point = New Point With {
.X = (pictureBox.Width - panel.Width) / 2,
.Y = (pictureBox.Height - panel.Height) / 2
}

        pictureBox.Location = left_top

        panel.AutoScrollPosition = scroll_pos
        Return left_top
    End Function

    ''' <summary>
    ''' check if there is any item in the pos where mouse clicked and return the number of object
    ''' </summary>
    ''' <paramname="m_pt">The point of mouse cursor.</param>
    ''' <paramname="obj_list">The list of objects.</param>
    ''' <paramname="width">The width of picturebox.</param>
    ''' <paramname="height">The height of picturebox.</param>
    ''' <paramname="CF">The factor of measuring scale.</param>
    Public Function CheckItemInPos(ByVal m_pt As PointF, ByVal obj_list As List(Of MeasureObject), ByVal width As Integer, ByVal height As Integer, ByVal CF As Double) As Integer
        For Each item In obj_list
            Dim x_low = item.left_top.X
            Dim x_high = item.right_bottom.X
            Dim y_low = item.left_top.Y
            Dim y_high = item.right_bottom.Y

            If item.measure_type = MeasureType.line_horizontal Then
                y_low -= 0.01F
                y_high += 0.01F
            ElseIf item.measure_type = MeasureType.line_vertical Then
                x_low -= 0.01F
                x_high += 0.01F
            ElseIf item.measure_type = MeasureType.measure_scale Then
                x_low = item.start_point.X
                y_low = item.start_point.Y

                If Equals(item.scale_object.style, "horizontal") Then
                    x_high = x_low + CSng(item.length / CF)
                    y_high = y_low + 0.01F
                    y_low -= 0.01F
                Else
                    y_high = y_low + CSng(item.length / CF)
                    x_high = x_low + 0.01F
                    x_low -= 0.01F
                End If
            ElseIf item.measure_type = MeasureType.circle_fixed Then
                x_low = item.start_point.X - item.scale_object.length / (CF * width)
                x_high = item.start_point.X + item.scale_object.length / (CF * width)
                y_low = item.start_point.Y - item.scale_object.length / (CF * width)
                y_high = item.start_point.Y + item.scale_object.length / (CF * width)
            End If

            If m_pt.X > x_low AndAlso m_pt.X < x_high AndAlso m_pt.Y > y_low AndAlso m_pt.Y < y_high Then Return item.obj_num
        Next
        Return -1
    End Function

    ''' <summary>
    ''' check if there is any annotation in the pos where mouse clicked and return the number of object
    ''' </summary>
    ''' <paramname="m_pt">The point of mouse cursor.</param>
    ''' <paramname="obj_list">The list of objects.</param>
    ''' <paramname="width">The width of picturebox.</param>
    ''' <paramname="height">The height of picturebox.</param>
    Public Function CheckAnnotation(ByVal m_pt As PointF, ByVal obj_list As List(Of MeasureObject), ByVal width As Integer, ByVal height As Integer) As Integer

        For Each item In obj_list
            If item.measure_type = MeasureType.annotation Then
                Dim limit_x = item.left_top.X + item.anno_object.size.Width / CSng(width)
                Dim limit_y = item.left_top.Y + item.anno_object.size.Height / CSng(height)

                If m_pt.X > item.left_top.X AndAlso m_pt.X < limit_x AndAlso m_pt.Y > item.left_top.Y AndAlso m_pt.Y < limit_y Then Return item.obj_num
            End If

        Next
        Return -1
    End Function


    ''' <summary>
    ''' enable textbox when you clicked on annotation
    ''' </summary>
    ''' <paramname="textbox">The textbox you are goint to type on.</param>
    ''' <paramname="obj_selected">the object whose annotation is selected.</param>
    ''' <paramname="Width">the width of picturebox.</param>
    ''' <paramname="Height">the height of picturebox.</param>
    ''' <paramname="left_top">the left top cornor of picturebox.</param>
    ''' <paramname="scroll_pos">the position of scrollbar.</param>
    <Extension()>
    Public Sub EnableTextBox(ByVal textbox As TextBox, ByVal obj_selected As MeasureObject, ByVal width As Integer, ByVal height As Integer, ByVal left_top As Point, ByVal scroll_pos As Point)
        textbox.Text = obj_selected.annotation
        Dim pos_image As Point = New Point(obj_selected.draw_point.X * width, obj_selected.draw_point.Y * height)
        Dim pos_panel As Point = New Point(pos_image.X + left_top.X, pos_image.Y + left_top.Y)
        textbox.Location = pos_image
        Dim rt_size = obj_selected.anno_object.size
        textbox.Visible = True
        textbox.Size = rt_size
    End Sub

    ''' <summary>
    ''' Hightlight selected item
    ''' </summary>
    ''' <paramname="pictureBox">the picturebox control.</param>
    ''' <paramname="item">the object which is selected.</param>
    ''' <paramname="Width">the width of picturebox.</param>
    ''' <paramname="Height">the height of picturebox.</param>

    <Extension()>
    Public Sub HightLightItem(ByVal pictureBox As PictureBox, ByVal item As MeasureObject, ByVal width As Integer, ByVal height As Integer, ByVal CF As Double)
        Dim graph As Graphics = pictureBox.CreateGraphics()
        Dim graphPen As Pen = New Pen(Color.Black, 1)
        graphPen.DashStyle = Drawing2D.DashStyle.Dot

        Dim x_low = item.left_top.X
        Dim x_high = item.right_bottom.X
        Dim y_low = item.left_top.Y
        Dim y_high = item.right_bottom.Y

        If item.measure_type = MeasureType.line_horizontal Then
            y_low -= 0.02F
            y_high += 0.02F
        ElseIf item.measure_type = MeasureType.line_vertical Then
            x_low -= 0.02F
            x_high += 0.02F
        ElseIf item.measure_type = MeasureType.measure_scale Then
            x_low = item.start_point.X
            y_low = item.start_point.Y

            If Equals(item.scale_object.style, "horizontal") Then
                y_high = y_low + 0.02F
                y_low -= 0.02F
            Else
                x_high = x_low + 0.02F
                x_low -= 0.02F
            End If
        ElseIf item.measure_type = MeasureType.circle_fixed Then
            x_low = item.start_point.X - item.scale_object.length / (CF * width)
            x_high = item.start_point.X + item.scale_object.length / (CF * width)
            y_low = item.start_point.Y - item.scale_object.length / (CF * width)
            y_high = item.start_point.Y + item.scale_object.length / (CF * width)
        End If

        Dim x As Integer = x_low * width
        Dim y As Integer = y_low * height
        Dim rt_width = CInt(x_high * width) - x
        Dim rt_height = CInt(y_high * height) - y

        graph.DrawRectangle(graphPen, x, y, rt_width, rt_height)
        graphPen.Dispose()
        graph.Dispose()
    End Sub


    ''' <summary>
    ''' change the location of textbox 
    ''' </summary>
    ''' <paramname="textbox">The textbox you are goint to type on.</param>
    ''' <paramname="start_pt">the left top corner of annotaion in picturebox.</param>
    ''' <paramname="cur_left_top">current location of left top corner of picturebox.</param>
    ''' <paramname="cur_scroll">current location of scrollbar.</param>
    <Extension()>
    Public Sub UpdateLocation(ByVal textbox As TextBox, ByVal start_pt As Point, ByVal cur_left_top As Point, ByVal cur_scroll As Point)
        'Point cur_locate = new Point(start_pt.X + cur_left_top.X - cur_scroll.X, start_pt.Y + cur_left_top.Y - cur_scroll.Y);
        Dim cur_locate As Point = New Point(start_pt.X + cur_left_top.X, start_pt.Y + cur_left_top.Y)
        textbox.Location = start_pt
    End Sub

    ''' <summary>
    ''' disable textbox when foucs leave and update annotation with the text of textbox
    ''' </summary>
    ''' <paramname="textbox">The textbox to disable.</param>
    ''' <paramname="obj_list">the list of objects.</param>
    ''' <paramname="obj_num">the number of object which you are goint to update.</param>
    <Extension()>
    Public Sub DisableTextBox(ByVal textbox As TextBox, ByVal obj_list As List(Of MeasureObject), ByVal obj_num As Integer,
                                  ByVal width As Integer, ByVal height As Integer)
        textbox.Visible = False
        Dim i = 0
        Dim flag = False
        For Each item In obj_list
            If item.obj_num = obj_num Then
                flag = True
                Exit For
            End If
            i += 1
        Next
        If flag Then
            Dim temp_obj = obj_list(i)
            temp_obj.annotation = textbox.Text
            temp_obj.anno_object.size = textbox.Size
            temp_obj.right_bottom.X = temp_obj.left_top.X + textbox.Size.Width / CSng(width)
            temp_obj.right_bottom.Y = temp_obj.left_top.Y + textbox.Size.Height / CSng(height)
            obj_list.RemoveAt(i)
            obj_list.Insert(i, temp_obj)
        End If
    End Sub

    ''' <summary>
    ''' remove last item from list
    ''' </summary>
    ''' <paramname="obj_list">the list of objects.</param>
    Public Function RemoveObjFromList(ByVal obj_list As List(Of MeasureObject)) As Boolean
        Dim cnt As Integer = obj_list.Count()
        If cnt > 0 Then
            obj_list.RemoveAt(cnt - 1)
            Return True
        End If
        Return False
    End Function

    ''' <summary>
    ''' move selected object in distance of dx, dy.
    ''' </summary>
    ''' <paramname="obj_list">The list of objects.</param>
    ''' <paramname="sel_index">The number of selected object.</param>
    ''' <paramname="dx">The move distance in X axis.</param>
    ''' <paramname="dy">The move distance in Y axis.</param>
    Public Sub MoveObject(ByVal obj_list As List(Of MeasureObject), ByVal sel_index As Integer, ByVal dx As Single, ByVal dy As Single)
        Dim obj_selected = obj_list.ElementAt(sel_index)
        If obj_selected.measure_type <> MeasureType.annotation Then
            obj_selected.start_point.X += dx
            obj_selected.start_point.Y += dy
        End If

        obj_selected.middle_point.X += dx
        obj_selected.middle_point.Y += dy
        obj_selected.end_point.X += dx
        obj_selected.end_point.Y += dy
        obj_selected.draw_point.X += dx
        obj_selected.draw_point.Y += dy
        obj_selected.last_point.X += dx
        obj_selected.last_point.Y += dy
        obj_selected.common_point.X += dx
        obj_selected.common_point.Y += dy
        obj_selected.left_top.X += dx
        obj_selected.left_top.Y += dy
        obj_selected.right_bottom.X += dx
        obj_selected.right_bottom.Y += dy

        If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_horizontal OrElse obj_selected.measure_type = MeasureType.line_vertical OrElse obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line Then
            obj_selected.line_object.draw_pt.X += dx
            obj_selected.line_object.draw_pt.Y += dy
            obj_selected.line_object.side_drag.X += dx
            obj_selected.line_object.side_drag.Y += dy
            obj_selected.line_object.nor_pt1.X += dx
            obj_selected.line_object.nor_pt1.Y += dy
            obj_selected.line_object.nor_pt2.X += dx
            obj_selected.line_object.nor_pt2.Y += dy
            obj_selected.line_object.nor_pt3.X += dx
            obj_selected.line_object.nor_pt3.Y += dy
            obj_selected.line_object.nor_pt4.X += dx
            obj_selected.line_object.nor_pt4.Y += dy
            obj_selected.line_object.nor_pt5.X += dx
            obj_selected.line_object.nor_pt5.Y += dy
            obj_selected.line_object.nor_pt6.X += dx
            obj_selected.line_object.nor_pt6.Y += dy
            obj_selected.line_object.nor_pt7.X += dx
            obj_selected.line_object.nor_pt7.Y += dy
            obj_selected.line_object.nor_pt8.X += dx
            obj_selected.line_object.nor_pt8.Y += dy
        ElseIf obj_selected.measure_type = MeasureType.line_fixed Then
            obj_selected.line_object.nor_pt1.X += dx
            obj_selected.line_object.nor_pt1.Y += dy
            obj_selected.line_object.nor_pt3.X += dx
            obj_selected.line_object.nor_pt3.Y += dy
            obj_selected.line_object.nor_pt5.X += dx
            obj_selected.line_object.nor_pt5.Y += dy
            obj_selected.line_object.nor_pt6.X += dx
            obj_selected.line_object.nor_pt6.Y += dy
        ElseIf obj_selected.measure_type = MeasureType.angle OrElse obj_selected.measure_type = MeasureType.angle_far OrElse obj_selected.measure_type = MeasureType.angle_fixed Then
            obj_selected.angle_object.draw_pt.X += dx
            obj_selected.angle_object.draw_pt.Y += dy
            obj_selected.angle_object.side_drag.X += dx
            obj_selected.angle_object.side_drag.Y += dy
            obj_selected.angle_object.start_pt.X += dx
            obj_selected.angle_object.start_pt.Y += dy
            obj_selected.angle_object.end_pt.X += dx
            obj_selected.angle_object.end_pt.Y += dy
            obj_selected.angle_object.nor_pt1.X += dx
            obj_selected.angle_object.nor_pt1.Y += dy
            obj_selected.angle_object.nor_pt2.X += dx
            obj_selected.angle_object.nor_pt2.Y += dy
            obj_selected.angle_object.nor_pt3.X += dx
            obj_selected.angle_object.nor_pt3.Y += dy
            obj_selected.angle_object.nor_pt4.X += dx
            obj_selected.angle_object.nor_pt4.Y += dy
            obj_selected.angle_object.nor_pt5.X += dx
            obj_selected.angle_object.nor_pt5.Y += dy
            obj_selected.angle_object.nor_pt6.X += dx
            obj_selected.angle_object.nor_pt6.Y += dy
        ElseIf obj_selected.measure_type = MeasureType.radius Then
            obj_selected.radius_object.draw_pt.X += dx
            obj_selected.radius_object.draw_pt.Y += dy
            obj_selected.radius_object.side_drag.X += dx
            obj_selected.radius_object.side_drag.Y += dy
            obj_selected.radius_object.arr_pt1.X += dx
            obj_selected.radius_object.arr_pt1.Y += dy
            obj_selected.radius_object.arr_pt2.X += dx
            obj_selected.radius_object.arr_pt2.Y += dy
            obj_selected.radius_object.arr_pt3.X += dx
            obj_selected.radius_object.arr_pt3.Y += dy
            obj_selected.radius_object.arr_pt4.X += dx
            obj_selected.radius_object.arr_pt4.Y += dy
            obj_selected.radius_object.circle_pt.X += dx
            obj_selected.radius_object.circle_pt.Y += dy
            obj_selected.radius_object.center_pt.X += dx
            obj_selected.radius_object.center_pt.Y += dy
        ElseIf obj_selected.measure_type = MeasureType.circle_fixed Then
            obj_selected.radius_object.center_pt.X += dx
            obj_selected.radius_object.center_pt.Y += dy
        ElseIf obj_selected.measure_type = MeasureType.annotation Then
            obj_selected.anno_object.line_pt.X += dx
            obj_selected.anno_object.line_pt.Y += dy
        End If
        obj_list(sel_index) = obj_selected
    End Sub

    ''' <summary>
    ''' correct delta for scale when mouse is moving
    ''' </summary>
    ''' <paramname="dx">delta in X.</param>
    ''' <paramname="dy">delta in Y.</param>
    ''' <paramname="direction">specfiy direction.</param>

    Public Sub CorrectDeltaForScale(ByRef dx As Single, ByRef dy As Single, ByVal direction As String)
        If Equals(direction, "horizontal") Then
            dy = 0
        Else
            dx = 0
        End If
    End Sub

    ''' <summary>
    ''' move selected point in distance of dx, dy.
    ''' </summary>
    ''' <paramname="obj_list">The list of objects.</param>
    ''' <paramname="item_index">The number of selected object.</param>
    ''' <paramname="pt_index">The number of selected point.</param>
    ''' <paramname="dx">The move distance in X axis.</param>
    ''' <paramname="dy">The move distance in Y axis.</param>
    Public Sub MovePoint(ByVal obj_list As List(Of MeasureObject), ByVal item_index As Integer, ByVal pt_index As Integer, ByVal dx As Single, ByVal dy As Single)
        Dim obj_selected = obj_list.ElementAt(item_index)

        If obj_selected.measure_type = MeasureType.measure_scale Then CorrectDeltaForScale(dx, dy, obj_selected.scale_object.style)

        Select Case pt_index
            Case 1
                obj_selected.start_point.X += dx
                obj_selected.start_point.Y += dy
            Case 2
                obj_selected.middle_point.X += dx
                obj_selected.middle_point.Y += dy
            Case 3
                obj_selected.end_point.X += dx
                obj_selected.end_point.Y += dy
            Case 4
                obj_selected.last_point.X += dx
                obj_selected.last_point.Y += dy
        End Select
        obj_list(item_index) = obj_selected
    End Sub


    ''' <summary>
    ''' modify object selected when mouse down and return whether obj_selected is completed or not.
    ''' </summary>
    ''' <paramname="obj_selected">The object which is currently selected.</param>
    ''' <paramname="cur_measure_type">The current measurement type.</param>
    ''' <paramname="m_pt">The point mouse clicked.</param>
    ''' <paramname="width">The width of original image.</param>
    ''' <paramname="height">The height of original image.</param>
    ''' <paramname="line_infor">The information for drawing lines.</param>
    ''' <paramname="font_infor">The information for font and color.</param>
    ''' <paramname="CF">The factor for measuring scale.</param>

    Public Function ModifyObjSelected(ByRef obj_selected As MeasureObject, ByVal cur_measure_type As Integer, ByVal m_pt As PointF, ByVal width As Integer, ByVal height As Integer, ByVal line_infor As LineStyle, ByVal font_infor As FontInfor, ByVal CF As Double) As Boolean
        obj_selected.intialized = True

        Dim item_set_limit = 0
        If cur_measure_type = MeasureType.line_align OrElse cur_measure_type = MeasureType.line_horizontal OrElse cur_measure_type = MeasureType.line_vertical OrElse cur_measure_type = MeasureType.line_fixed OrElse cur_measure_type = MeasureType.angle_fixed Then
            item_set_limit = 3
        ElseIf cur_measure_type = MeasureType.angle OrElse cur_measure_type = MeasureType.radius OrElse cur_measure_type = MeasureType.line_para OrElse cur_measure_type = MeasureType.pt_line Then
            item_set_limit = 4
        ElseIf cur_measure_type = MeasureType.annotation OrElse cur_measure_type = MeasureType.draw_line OrElse cur_measure_type = MeasureType.circle_fixed Then
            item_set_limit = 2
        ElseIf cur_measure_type = MeasureType.angle_far Then
            item_set_limit = 5
        ElseIf cur_measure_type = MeasureType.measure_scale Then
            item_set_limit = 1
        End If

        If obj_selected.item_set < item_set_limit Then
            If cur_measure_type = MeasureType.line_align OrElse cur_measure_type = MeasureType.line_horizontal OrElse cur_measure_type = MeasureType.line_vertical OrElse cur_measure_type = MeasureType.measure_scale OrElse cur_measure_type = MeasureType.draw_line OrElse cur_measure_type = MeasureType.line_fixed Then
                If obj_selected.item_set = 0 Then
                    obj_selected.start_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 1 Then
                    obj_selected.end_point = m_pt
                    obj_selected.item_set += 1

                    obj_selected.left_top.X = Math.Min(obj_selected.start_point.X, obj_selected.end_point.X)
                    obj_selected.left_top.Y = Math.Min(obj_selected.start_point.Y, obj_selected.end_point.Y)
                    obj_selected.right_bottom.X = Math.Max(obj_selected.start_point.X, obj_selected.end_point.X)
                    obj_selected.right_bottom.Y = Math.Max(obj_selected.start_point.Y, obj_selected.end_point.Y)
                ElseIf obj_selected.item_set = 2 Then
                    obj_selected.draw_point = m_pt
                    obj_selected.item_set += 1
                End If
            ElseIf cur_measure_type = MeasureType.angle OrElse cur_measure_type = MeasureType.radius OrElse cur_measure_type = MeasureType.line_para OrElse cur_measure_type = MeasureType.pt_line OrElse cur_measure_type = MeasureType.angle_fixed Then
                If obj_selected.item_set = 0 Then
                    obj_selected.start_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 1 Then
                    obj_selected.middle_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 2 Then
                    If cur_measure_type = MeasureType.angle_fixed Then
                        obj_selected.draw_point = m_pt
                        obj_selected.item_set += 1
                    Else
                        obj_selected.end_point = m_pt
                        obj_selected.item_set += 1
                    End If

                    If cur_measure_type = MeasureType.radius OrElse cur_measure_type = MeasureType.pt_line OrElse cur_measure_type = MeasureType.line_para Then
                        Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X}
                        obj_selected.left_top.X = GetMinimumInSet(x_set)
                        obj_selected.right_bottom.X = GetMaximumInSet(x_set)
                        Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y}
                        obj_selected.left_top.Y = GetMinimumInSet(y_set)
                        obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
                    End If
                ElseIf obj_selected.item_set = 3 Then
                    obj_selected.draw_point = m_pt
                    obj_selected.item_set += 1
                End If
            ElseIf cur_measure_type = MeasureType.circle_fixed Then
                If obj_selected.item_set = 0 Then
                    obj_selected.start_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 1 Then
                    obj_selected.draw_point = m_pt
                    obj_selected.item_set += 1
                End If

            ElseIf cur_measure_type = MeasureType.annotation Then
                If obj_selected.item_set = 0 Then
                    obj_selected.start_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 1 Then
                    obj_selected.draw_point = m_pt
                    obj_selected.item_set += 1
                End If

            ElseIf cur_measure_type = MeasureType.angle_far Then
                If obj_selected.item_set = 0 Then
                    obj_selected.start_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 1 Then
                    obj_selected.middle_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 2 Then
                    obj_selected.end_point = m_pt
                    obj_selected.item_set += 1
                ElseIf obj_selected.item_set = 3 Then
                    obj_selected.last_point = m_pt
                    obj_selected.item_set += 1

                    Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X, obj_selected.last_point.X}
                    obj_selected.left_top.X = GetMinimumInSet(x_set)
                    obj_selected.right_bottom.X = GetMaximumInSet(x_set)
                    Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y, obj_selected.last_point.Y}
                    obj_selected.left_top.Y = GetMinimumInSet(y_set)
                    obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
                ElseIf obj_selected.item_set = 4 Then
                    obj_selected.draw_point = m_pt
                    obj_selected.item_set += 1
                End If
            End If

            Dim start_point As Point = New Point()
            Dim middle_point As Point = New Point()
            Dim end_point As Point = New Point()
            Dim draw_point As Point = New Point()

            start_point.X = CInt(obj_selected.start_point.X * width)
            start_point.Y = CInt(obj_selected.start_point.Y * height)
            middle_point.X = CInt(obj_selected.middle_point.X * width)
            middle_point.Y = CInt(obj_selected.middle_point.Y * height)
            end_point.X = CInt(obj_selected.end_point.X * width)
            end_point.Y = CInt(obj_selected.end_point.Y * height)
            draw_point.X = CInt(obj_selected.draw_point.X * width)
            draw_point.Y = CInt(obj_selected.draw_point.Y * height)
            Dim last_point As Point = New Point()
            last_point.X = CInt(obj_selected.last_point.X * width)
            last_point.Y = CInt(obj_selected.last_point.Y * height)

            If obj_selected.item_set = item_set_limit - 1 Then
                If cur_measure_type = MeasureType.line_align OrElse cur_measure_type = MeasureType.line_fixed Then
                    If cur_measure_type = MeasureType.line_align Then
                        obj_selected.length = Math.Sqrt(Math.Pow(end_point.X - start_point.X, 2) + Math.Pow(end_point.Y - start_point.Y, 2))
                    Else
                        Dim length = Math.Sqrt(Math.Pow(end_point.X - start_point.X, 2) + Math.Pow(end_point.Y - start_point.Y, 2)) * CF
                        obj_selected.end_point.X = obj_selected.start_point.X + (obj_selected.end_point.X - obj_selected.start_point.X) / length * obj_selected.length * width
                        obj_selected.end_point.Y = obj_selected.start_point.Y + (obj_selected.end_point.Y - obj_selected.start_point.Y) / length * obj_selected.length * height

                        obj_selected.left_top.X = Math.Min(obj_selected.start_point.X, obj_selected.end_point.X)
                        obj_selected.left_top.Y = Math.Min(obj_selected.start_point.Y, obj_selected.end_point.Y)
                        obj_selected.right_bottom.X = Math.Max(obj_selected.start_point.X, obj_selected.end_point.X)
                        obj_selected.right_bottom.Y = Math.Max(obj_selected.start_point.Y, obj_selected.end_point.Y)
                    End If

                    Dim angle As Double = 0

                    If start_point.Y >= end_point.Y Then
                        angle = CalcAngleBetweenTwoLines(end_point, start_point, New Point(start_point.X + 10, start_point.Y))
                    Else
                        angle = CalcAngleBetweenTwoLines(start_point, end_point, New Point(end_point.X + 10, end_point.Y))
                    End If

                    obj_selected.angle = angle * 360 / Math.PI / 2
                    obj_selected.name = "line aline"
                ElseIf cur_measure_type = MeasureType.line_horizontal Then
                    obj_selected.length = Math.Abs(CDbl(end_point.X - start_point.X))

                    obj_selected.angle = 0
                    obj_selected.name = "line horizonal"
                ElseIf cur_measure_type = MeasureType.line_vertical Then
                    obj_selected.length = Math.Abs(CDbl(end_point.Y - start_point.Y))

                    obj_selected.angle = 90
                    obj_selected.name = "line vertical"
                ElseIf cur_measure_type = MeasureType.line_para OrElse cur_measure_type = MeasureType.pt_line Then
                    obj_selected.length = CalcDistFromPointToLine(start_point, middle_point, end_point)

                    Dim angle As Double = 0

                    If start_point.Y >= middle_point.Y Then
                        angle = CalcAngleBetweenTwoLines(middle_point, start_point, New Point(start_point.X + 10, start_point.Y))
                    Else
                        angle = CalcAngleBetweenTwoLines(start_point, middle_point, New Point(middle_point.X + 10, middle_point.Y))
                    End If

                    obj_selected.angle = angle * 360 / Math.PI / 2 + 90

                    If cur_measure_type = MeasureType.line_para Then
                        obj_selected.name = "line para"
                    Else
                        obj_selected.name = "pt to line"
                    End If
                ElseIf cur_measure_type = MeasureType.angle Then
                    'correct code
                    Dim angle = CalcAngleBetweenTwoLines(start_point, middle_point, end_point)
                    obj_selected.angle = angle * 360 / Math.PI / 2
                    obj_selected.name = "angle"
                ElseIf cur_measure_type = MeasureType.radius Then
                    Dim A = start_point
                    Dim B = middle_point
                    Dim C = end_point
                    Dim d_AB = Math.Sqrt(Math.Pow(B.X - A.X, 2.0R) + Math.Pow(B.Y - A.Y, 2.0R))
                    Dim d_BC = Math.Sqrt(Math.Pow(B.X - C.X, 2.0R) + Math.Pow(B.Y - C.Y, 2.0R))
                    Dim d_AC = Math.Sqrt(Math.Pow(C.X - A.X, 2.0R) + Math.Pow(C.Y - A.Y, 2.0R))
                    If d_AB + d_BC < d_AC + 0.2R And d_AB + d_BC > d_AC - 0.2R Then
                        Return False
                    End If
                    Dim t As Triangle = New Triangle(New Point3d(start_point.X, start_point.Y, 0), New Point3d(middle_point.X, middle_point.Y, 0), New Point3d(end_point.X, end_point.Y, 0))
                    Dim angle_a = t.Angle_A * 360.0R / Math.PI
                    Dim angle_b = t.Angle_B * 360.0R / Math.PI
                    Dim angle_c = t.Angle_C * 360.0R / Math.PI
                    Dim circumcenterpt = t.Circumcenter
                    Dim centerpt = New Point(Convert.ToInt32(circumcenterpt.X), Convert.ToInt32(circumcenterpt.Y))

                    obj_selected.radius = t.Circumcircle.R

                    obj_selected.radius_object.radius = obj_selected.radius / width
                    obj_selected.radius_object.center_pt = New PointF(centerpt.X / CSng(width), centerpt.Y / CSng(height))
                    obj_selected.name = "radius"
                ElseIf cur_measure_type = MeasureType.annotation Then
                    obj_selected.annotation = "annotation"
                    obj_selected.name = "anno"
                ElseIf cur_measure_type = MeasureType.angle_far Then
                    Dim inter_pt = CalcInterSection(start_point, middle_point, end_point, last_point)
                    If inter_pt.X = 10000 AndAlso inter_pt.Y = 10000 Then
                        obj_selected.angle = 0
                    Else
                        Dim angle = CalcAngleBetweenTwoLines(start_point, inter_pt, end_point)
                        obj_selected.angle = angle * 360 / Math.PI / 2
                        obj_selected.common_point = New PointF(CSng(inter_pt.X) / width, CSng(inter_pt.Y) / height)
                    End If
                    obj_selected.name = "angle"
                ElseIf cur_measure_type = MeasureType.angle_fixed Then
                    obj_selected.name = "angle"
                ElseIf cur_measure_type = MeasureType.circle_fixed Then
                    obj_selected.name = "circle"
                End If
            End If

            obj_selected.line_infor.line_style = line_infor.line_style
            obj_selected.line_infor.line_width = line_infor.line_width
            obj_selected.line_infor.line_color = line_infor.line_color
            obj_selected.font_infor.font_color = font_infor.font_color
            obj_selected.font_infor.text_font = font_infor.text_font

            If cur_measure_type = MeasureType.measure_scale Then
                end_point = start_point

                Dim length_px As Integer
                If Equals(obj_selected.scale_object.style, "horizontal") Then
                    length_px = CInt(obj_selected.scale_object.length / CF)
                    end_point.X += length_px
                    obj_selected.scale_object.trans_angle = 0
                    obj_selected.length = obj_selected.scale_object.length / CF
                Else
                    length_px = CInt(obj_selected.scale_object.length / CF)
                    end_point.Y += length_px
                    obj_selected.scale_object.trans_angle = 90
                    obj_selected.length = obj_selected.scale_object.length / CF
                End If

                obj_selected.end_point = New PointF(end_point.X / CSng(width), end_point.Y / CSng(height))
                obj_selected.name = "scale"
            End If

            If obj_selected.item_set = item_set_limit Then
                If cur_measure_type = MeasureType.angle Then
                    obj_selected.start_point = obj_selected.angle_object.start_pt
                    obj_selected.end_point = obj_selected.angle_object.end_pt
                    Dim x_set = {obj_selected.middle_point.X, obj_selected.start_point.X, obj_selected.end_point.X}
                    Dim y_set = {obj_selected.middle_point.Y, obj_selected.start_point.Y, obj_selected.end_point.Y}
                    obj_selected.left_top.X = GetMinimumInSet(x_set)
                    obj_selected.left_top.Y = GetMinimumInSet(y_set)
                    obj_selected.right_bottom.X = GetMaximumInSet(x_set)
                    obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
                End If
                Return True
            Else
                Return False
            End If
        End If
        Return False
    End Function

    ''' <summary>
    ''' modify object selected when mouse move.
    ''' </summary>
    ''' <paramname="obj_list">The list of objects.</param>
    ''' <paramname="item_index">The index of selected item.</param>
    ''' <paramname="width">The width of origianl image.</param>
    ''' <paramname="height">The height of origianl image.</param>

    Public Sub ModifyObjSelected(ByVal obj_list As List(Of MeasureObject), ByVal item_index As Integer, ByVal width As Integer, ByVal height As Integer)
        Dim obj_selected = obj_list.ElementAt(item_index)

        Dim cur_measure_type = obj_selected.measure_type

        If cur_measure_type = MeasureType.line_align OrElse cur_measure_type = MeasureType.line_horizontal OrElse cur_measure_type = MeasureType.line_vertical OrElse cur_measure_type = MeasureType.measure_scale OrElse cur_measure_type = MeasureType.draw_line Then
            obj_selected.left_top.X = Math.Min(obj_selected.start_point.X, obj_selected.end_point.X)
            obj_selected.left_top.Y = Math.Min(obj_selected.start_point.Y, obj_selected.end_point.Y)
            obj_selected.right_bottom.X = Math.Max(obj_selected.start_point.X, obj_selected.end_point.X)
            obj_selected.right_bottom.Y = Math.Max(obj_selected.start_point.Y, obj_selected.end_point.Y)
        ElseIf cur_measure_type = MeasureType.radius OrElse cur_measure_type = MeasureType.pt_line OrElse cur_measure_type = MeasureType.line_para Then
            Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X}
            obj_selected.left_top.X = GetMinimumInSet(x_set)
            obj_selected.right_bottom.X = GetMaximumInSet(x_set)
            Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y}
            obj_selected.left_top.Y = GetMinimumInSet(y_set)
            obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
        ElseIf cur_measure_type = MeasureType.angle_far Then
            Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X, obj_selected.last_point.X}
            obj_selected.left_top.X = GetMinimumInSet(x_set)
            obj_selected.right_bottom.X = GetMaximumInSet(x_set)
            Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y, obj_selected.last_point.Y}
            obj_selected.left_top.Y = GetMinimumInSet(y_set)
            obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
        ElseIf cur_measure_type = MeasureType.angle Then
            Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X}
            obj_selected.left_top.X = GetMinimumInSet(x_set)
            obj_selected.right_bottom.X = GetMaximumInSet(x_set)
            Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y}
            obj_selected.left_top.Y = GetMinimumInSet(y_set)
            obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
        End If

        Dim start_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim draw_point As Point = New Point()

        start_point.X = CInt(obj_selected.start_point.X * width)
        start_point.Y = CInt(obj_selected.start_point.Y * height)
        end_point.X = CInt(obj_selected.end_point.X * width)
        end_point.Y = CInt(obj_selected.end_point.Y * height)

        middle_point.X = CInt(obj_selected.middle_point.X * width)
        middle_point.Y = CInt(obj_selected.middle_point.Y * height)
        draw_point.X = CInt(obj_selected.draw_point.X * width)
        draw_point.Y = CInt(obj_selected.draw_point.Y * height)
        Dim last_point As Point = New Point()
        last_point.X = CInt(obj_selected.last_point.X * width)
        last_point.Y = CInt(obj_selected.last_point.Y * height)
        If cur_measure_type = MeasureType.line_align Then
            obj_selected.length = Math.Sqrt(Math.Pow(end_point.X - start_point.X, 2) + Math.Pow(end_point.Y - start_point.Y, 2))

            Dim angle As Double = 0

            If start_point.Y >= end_point.Y Then
                angle = CalcAngleBetweenTwoLines(end_point, start_point, New Point(start_point.X + 10, start_point.Y))
            Else
                angle = CalcAngleBetweenTwoLines(start_point, end_point, New Point(end_point.X + 10, end_point.Y))
            End If

            obj_selected.angle = angle * 360 / Math.PI / 2
        ElseIf cur_measure_type = MeasureType.line_horizontal Then
            obj_selected.length = Math.Abs(CDbl(end_point.X - start_point.X))

            obj_selected.angle = 0
        ElseIf cur_measure_type = MeasureType.line_vertical Then
            obj_selected.length = Math.Abs(CDbl(end_point.Y - start_point.Y))

            obj_selected.angle = 90
        ElseIf cur_measure_type = MeasureType.measure_scale Then
            obj_selected.length = Math.Sqrt(Math.Pow(end_point.X - start_point.X, 2) + Math.Pow(end_point.Y - start_point.Y, 2))
        ElseIf cur_measure_type = MeasureType.line_para OrElse cur_measure_type = MeasureType.pt_line Then
            obj_selected.length = CalcDistFromPointToLine(start_point, middle_point, end_point)

            Dim angle As Double = 0

            If start_point.Y >= middle_point.Y Then
                angle = CalcAngleBetweenTwoLines(middle_point, start_point, New Point(start_point.X + 10, start_point.Y))
            Else
                angle = CalcAngleBetweenTwoLines(start_point, middle_point, New Point(middle_point.X + 10, middle_point.Y))
            End If
            obj_selected.angle = angle * 360 / Math.PI / 2 + 90
        ElseIf cur_measure_type = MeasureType.angle Then
            'correct code
            Dim angle = CalcAngleBetweenTwoLines(start_point, middle_point, end_point)
            obj_selected.angle = angle * 360 / Math.PI / 2
        ElseIf cur_measure_type = MeasureType.radius Then

            Dim A = start_point
            Dim B = middle_point
            Dim C = end_point
            Dim d_AB = Math.Sqrt(Math.Pow(B.X - A.X, 2.0R) + Math.Pow(B.Y - A.Y, 2.0R))
            Dim d_BC = Math.Sqrt(Math.Pow(B.X - C.X, 2.0R) + Math.Pow(B.Y - C.Y, 2.0R))
            Dim d_AC = Math.Sqrt(Math.Pow(C.X - A.X, 2.0R) + Math.Pow(C.Y - A.Y, 2.0R))
            If d_AB + d_BC < d_AC + 0.2R And d_AB + d_BC > d_AC - 0.2R Then
                Return
            End If
            Dim t As Triangle = New Triangle(New Point3d(start_point.X, start_point.Y, 0), New Point3d(middle_point.X, middle_point.Y, 0), New Point3d(end_point.X, end_point.Y, 0))
            Dim angle_a = t.Angle_A * 360.0R / Math.PI
            Dim angle_b = t.Angle_B * 360.0R / Math.PI
            Dim angle_c = t.Angle_C * 360.0R / Math.PI
            Dim circumcenterpt = t.Circumcenter
            Dim centerpt = New Point(Convert.ToInt32(circumcenterpt.X), Convert.ToInt32(circumcenterpt.Y))

            obj_selected.radius = t.Circumcircle.R

            obj_selected.radius_object.radius = obj_selected.radius / width
            obj_selected.radius_object.center_pt = New PointF(centerpt.X / CSng(width), centerpt.Y / CSng(height))
        ElseIf cur_measure_type = MeasureType.angle_far Then
            Dim inter_pt = CalcInterSection(start_point, middle_point, end_point, last_point)
            If inter_pt.X = 10000 AndAlso inter_pt.Y = 10000 Then
                obj_selected.angle = 0
            Else
                Dim angle = CalcAngleBetweenTwoLines(start_point, inter_pt, end_point)
                obj_selected.angle = angle * 360 / Math.PI / 2
                obj_selected.common_point = New PointF(CSng(inter_pt.X) / width, CSng(inter_pt.Y) / height)
            End If

        End If
        obj_list(item_index) = obj_selected
    End Sub

    ''' <summary>
    ''' Intialize CLine Object when you are movin C_Line.
    ''' </summary>
    ''' <paramname="Obj">The measureObject.</param>
    ''' <paramname="m_pt">The start point of Line Object.</param>
    Public Sub InitializeLineObj(ByRef Obj As MeasureObject, m_pt As PointF, ByVal line_infor As LineStyle, ByVal font_infor As FontInfor)
        Obj.start_point = m_pt
        Obj.end_point = m_pt
        Obj.line_object.nor_pt1 = m_pt
        Obj.line_object.nor_pt2 = m_pt
        Obj.line_object.nor_pt3 = m_pt
        Obj.line_object.nor_pt4 = m_pt
        Obj.line_object.nor_pt5 = m_pt
        Obj.line_object.nor_pt6 = m_pt
        Obj.line_object.nor_pt7 = m_pt
        Obj.line_object.nor_pt8 = m_pt
        Obj.measure_type = MeasureType.line_align
        Obj.line_infor.line_style = line_infor.line_style
        Obj.line_infor.line_width = line_infor.line_width
        Obj.line_infor.line_color = line_infor.line_color
        Obj.font_infor.font_color = font_infor.font_color
        Obj.font_infor.text_font = font_infor.text_font

    End Sub

    ''' <summary>
    ''' Intialize Line Object when you are moving C_Line.
    ''' </summary>
    ''' <paramname="Obj">The measureObject.</param>
    ''' <paramname="dx">the offset of X-axis.</param>
    ''' <paramname="dy">the offset of Y-axis.</param>
    ''' <paramname="width">the width of Original Image.</param>
    ''' <paramname="height">the height of Original Image.</param>
    Public Sub DrawLengthBetweenLines(pictureBox As PictureBox, ByRef Obj As MeasureObject, dx As Double, dy As Double, width As Integer, height As Integer, digit As Integer, CF As Double)
        Obj.end_point.X = Obj.start_point.X - dx
        Obj.end_point.Y = Obj.start_point.Y - dy
        Obj.line_object.nor_pt2.X = Obj.start_point.X - dx
        Obj.line_object.nor_pt2.Y = Obj.start_point.Y - dy
        Obj.line_object.nor_pt4.X = Obj.start_point.X - dx
        Obj.line_object.nor_pt4.Y = Obj.start_point.Y - dy

        Dim nor_point3 = New Point(Obj.line_object.nor_pt3.X * pictureBox.Width, Obj.line_object.nor_pt3.Y * pictureBox.Height)
        Dim nor_point4 = New Point(Obj.line_object.nor_pt4.X * pictureBox.Width, Obj.line_object.nor_pt4.Y * pictureBox.Height)
        Dim n_dist As Integer = CalcDistBetweenPoints(nor_point3, nor_point4)
        n_dist /= 10
        n_dist = Math.Max(n_dist, 5)
        Dim arr_points = New Point(1) {}
        Dim arr_points2 = New Point(1) {}
        arr_points = GetArrowPoints3(nor_point3, nor_point4, n_dist)
        arr_points2 = GetArrowPoints3(nor_point4, nor_point3, n_dist)
        Dim draw_pt = New Point((nor_point3.X + nor_point4.X) / 2, (nor_point3.Y + nor_point4.Y) / 2)
        If dx <> 0 Or dy <> 0 Then
            draw_pt.X += dy / Math.Sqrt(dx * dx + dy * dy) * 10
            draw_pt.X += dx / Math.Sqrt(dx * dx + dy * dy) * 10
        End If

        Dim angle As Double = 0
        If nor_point3.Y >= nor_point4.Y Then
            angle = CalcAngleBetweenTwoLines(nor_point4, nor_point3, New Point(nor_point3.X + 10, nor_point3.Y))
        Else
            angle = CalcAngleBetweenTwoLines(nor_point3, nor_point4, New Point(nor_point4.X + 10, nor_point4.Y))
        End If

        Obj.angle = angle * 360 / Math.PI / 2

        Dim ang_tran As Integer = Obj.angle
        Dim attri = -1
        If Obj.angle > 90 Then
            ang_tran = 180 - ang_tran
            attri = 1
        End If
        Obj.line_object.nor_pt5 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
        Obj.line_object.nor_pt6 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
        Obj.line_object.nor_pt7 = New PointF(CSng(arr_points2(0).X) / pictureBox.Width, CSng(arr_points2(0).Y) / pictureBox.Height)
        Obj.line_object.nor_pt8 = New PointF(CSng(arr_points2(1).X) / pictureBox.Width, CSng(arr_points2(1).Y) / pictureBox.Height)
        Obj.line_object.draw_pt = New PointF(CSng(draw_pt.X) / pictureBox.Width, CSng(draw_pt.Y) / pictureBox.Height)
        Obj.line_object.side_drag = Obj.line_object.nor_pt4
        Obj.line_object.trans_angle = Convert.ToInt32(attri * ang_tran)
        Obj.length = Math.Sqrt(Math.Pow(Obj.start_point.X * width - Obj.end_point.X * width, 2) + Math.Pow(Obj.start_point.Y * height - Obj.end_point.Y * height, 2))

        Dim graph As Graphics = pictureBox.CreateGraphics()
        Main_Form.show_legend = True
        DrawObjItem(graph, pictureBox, Obj, digit, CF)
        Main_Form.show_legend = False
        graph.Dispose()
    End Sub

    ''' <summary>
    ''' draw object list to picture box control.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="object_list">The list of objects which you are going to draw.</param>
    ''' <paramname="graphPen">The pen for drawing objects.</param>
    ''' <paramname="graphPen_line">The pen for drawing lines.</param>
    ''' <paramname="digit">The digit of decimal numbers.</param>
    ''' <paramname="CF">The factor of measuring scale.</param>
    ''' <paramname="flag">The flag determines refresh.</param>

    <Extension()>
    Public Sub DrawObjList(ByVal pictureBox As PictureBox, ByVal object_list As List(Of MeasureObject), ByVal graphPen As Pen, ByVal graphPen_line As Pen, ByVal digit As Integer, ByVal CF As Double, ByVal flag As Boolean)
        pictureBox.Refresh()

        Dim graph As Graphics = pictureBox.CreateGraphics()
        DrawObjList2(graph, pictureBox, object_list, graphPen, graphPen_line, digit, CF)

        graph.Dispose()
    End Sub

    ''' <summary>
    ''' draw object to picture box control.
    ''' </summary>
    ''' <paramname="graph">The graphics for drawing object list.</param>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="item">The object which you are going to draw.</param>
    ''' <paramname="digit">The digit of decimal numbers.</param>
    ''' <paramname="CF">The factor of measuring scale.</param>
    Public Sub DrawObjItem(ByVal graph As Graphics, ByVal pictureBox As PictureBox, ByVal item As MeasureObject, ByVal digit As Integer, ByVal CF As Double)
        Dim start_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim draw_point As Point = New Point()
        Dim inter_pt As Point = New Point()
        Dim last_point As Point = New Point()

        Dim graphPen2 As Pen = New Pen(item.line_infor.line_color, item.line_infor.line_width)
        Dim graphFont = item.font_infor.text_font
        Dim graphBrush As SolidBrush = New SolidBrush(item.font_infor.font_color)

        start_point.X = CInt(item.start_point.X * pictureBox.Width)
        start_point.Y = CInt(item.start_point.Y * pictureBox.Height)
        middle_point.X = CInt(item.middle_point.X * pictureBox.Width)
        middle_point.Y = CInt(item.middle_point.Y * pictureBox.Height)
        end_point.X = CInt(item.end_point.X * pictureBox.Width)
        end_point.Y = CInt(item.end_point.Y * pictureBox.Height)
        draw_point.X = CInt(item.draw_point.X * pictureBox.Width)
        draw_point.Y = CInt(item.draw_point.Y * pictureBox.Height)
        inter_pt.X = CInt(item.common_point.X * pictureBox.Width)
        inter_pt.Y = CInt(item.common_point.Y * pictureBox.Height)
        last_point.X = CInt(item.last_point.X * pictureBox.Width)
        last_point.Y = CInt(item.last_point.Y * pictureBox.Height)

        If item.measure_type = MeasureType.line_align OrElse item.measure_type = MeasureType.line_horizontal OrElse item.measure_type = MeasureType.line_vertical OrElse item.measure_type = MeasureType.line_para OrElse item.measure_type = MeasureType.pt_line OrElse item.measure_type = MeasureType.line_fixed Then
            'graph.DrawLine(graphPen, start_point, end_point);
            If item.measure_type = MeasureType.line_para Then end_point = last_point
            Dim nor_pt1, nor_pt2, nor_pt3, nor_pt4, nor_pt5, nor_pt6, nor_pt7, nor_pt8, side_pt, draw_pt As Point
            If item.measure_type = MeasureType.line_fixed Then
                Dim length = Math.Sqrt(Math.Pow(end_point.X - start_point.X, 2) + Math.Pow(end_point.Y - start_point.Y, 2)) * CF
                'end_point.X = start_point.X + (end_point.X - start_point.X) / length * item.length * pictureBox.Width
                'end_point.Y = start_point.Y + (end_point.Y - start_point.Y) / length * item.length * pictureBox.Width

                nor_pt1 = New Point(item.line_object.nor_pt1.X * pictureBox.Width, item.line_object.nor_pt1.Y * pictureBox.Height)
                nor_pt3 = New Point(item.line_object.nor_pt3.X * pictureBox.Width, item.line_object.nor_pt3.Y * pictureBox.Height)
                nor_pt5 = New Point(item.line_object.nor_pt5.X * pictureBox.Width, item.line_object.nor_pt5.Y * pictureBox.Height)
                nor_pt6 = New Point(item.line_object.nor_pt6.X * pictureBox.Width, item.line_object.nor_pt6.Y * pictureBox.Height)
                Dim deltaX = item.line_object.nor_pt1.X
                Dim deltaY = item.line_object.nor_pt1.Y
                nor_pt2 = New Point((item.line_object.nor_pt2.X / CF + deltaX) * pictureBox.Width, (item.line_object.nor_pt2.Y / CF + deltaY) * pictureBox.Height)
                deltaX = item.line_object.nor_pt3.X
                deltaY = item.line_object.nor_pt3.Y
                nor_pt4 = New Point((item.line_object.nor_pt4.X / CF + deltaX) * pictureBox.Width, (item.line_object.nor_pt4.Y / CF + deltaY) * pictureBox.Height)
                nor_pt7 = New Point(nor_pt4.X - (nor_pt5.X - nor_pt3.X), nor_pt4.Y - (nor_pt5.Y - nor_pt3.Y))
                nor_pt8 = New Point(nor_pt4.X - (nor_pt6.X - nor_pt3.X), nor_pt4.Y - (nor_pt6.Y - nor_pt3.Y))
                side_pt = New Point((item.line_object.side_drag.X / CF + deltaX) * pictureBox.Width, (item.line_object.side_drag.Y / CF + deltaY) * pictureBox.Height)
                draw_pt = New Point((item.line_object.draw_pt.X / CF + deltaX) * pictureBox.Width, (item.line_object.draw_pt.Y / CF + deltaY) * pictureBox.Height)
            Else
                nor_pt1 = New Point(item.line_object.nor_pt1.X * pictureBox.Width, item.line_object.nor_pt1.Y * pictureBox.Height)
                nor_pt2 = New Point(item.line_object.nor_pt2.X * pictureBox.Width, item.line_object.nor_pt2.Y * pictureBox.Height)
                nor_pt3 = New Point(item.line_object.nor_pt3.X * pictureBox.Width, item.line_object.nor_pt3.Y * pictureBox.Height)
                nor_pt4 = New Point(item.line_object.nor_pt4.X * pictureBox.Width, item.line_object.nor_pt4.Y * pictureBox.Height)
                nor_pt5 = New Point(item.line_object.nor_pt5.X * pictureBox.Width, item.line_object.nor_pt5.Y * pictureBox.Height)
                nor_pt6 = New Point(item.line_object.nor_pt6.X * pictureBox.Width, item.line_object.nor_pt6.Y * pictureBox.Height)
                nor_pt7 = New Point(item.line_object.nor_pt7.X * pictureBox.Width, item.line_object.nor_pt7.Y * pictureBox.Height)
                nor_pt8 = New Point(item.line_object.nor_pt8.X * pictureBox.Width, item.line_object.nor_pt8.Y * pictureBox.Height)
                side_pt = New Point(item.line_object.side_drag.X * pictureBox.Width, item.line_object.side_drag.Y * pictureBox.Height)
                draw_pt = New Point(item.line_object.draw_pt.X * pictureBox.Width, item.line_object.draw_pt.Y * pictureBox.Height)
            End If


            graph.DrawLine(graphPen2, start_point, nor_pt1)
            graph.DrawLine(graphPen2, end_point, nor_pt2)
            graph.DrawLine(graphPen2, nor_pt3, nor_pt4)
            graph.DrawLine(graphPen2, nor_pt3, nor_pt5)
            graph.DrawLine(graphPen2, nor_pt3, nor_pt6)
            graph.DrawLine(graphPen2, nor_pt4, nor_pt7)
            graph.DrawLine(graphPen2, nor_pt4, nor_pt8)
            graph.DrawLine(graphPen2, nor_pt4, side_pt)

            graph.RotateTransform(item.line_object.trans_angle)
            Dim trans_pt = GetRotationTransform(draw_pt, item.line_object.trans_angle)

            Dim length_decimal = GetDecimalNumber(item.length, digit, CF)
            If item.measure_type = MeasureType.line_fixed Then
                length_decimal = item.scale_object.length
            End If

            If Main_Form.show_legend = True Then
                Dim output = item.name + " " + length_decimal.ToString()
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            Else
                Dim output = item.name
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            End If

            graph.RotateTransform(-1 * item.line_object.trans_angle)
        ElseIf item.measure_type = MeasureType.angle OrElse item.measure_type = MeasureType.angle_far OrElse item.measure_type = MeasureType.angle_fixed Then

            Dim st_pt As Point = New Point(item.angle_object.start_pt.X * pictureBox.Width, item.angle_object.start_pt.Y * pictureBox.Height)
            Dim end_pt As Point = New Point(item.angle_object.end_pt.X * pictureBox.Width, item.angle_object.end_pt.Y * pictureBox.Height)
            Dim nor_pt1 As Point = New Point(item.angle_object.nor_pt1.X * pictureBox.Width, item.angle_object.nor_pt1.Y * pictureBox.Height)
            Dim nor_pt2 As Point = New Point(item.angle_object.nor_pt2.X * pictureBox.Width, item.angle_object.nor_pt2.Y * pictureBox.Height)
            Dim nor_pt3 As Point = New Point(item.angle_object.nor_pt3.X * pictureBox.Width, item.angle_object.nor_pt3.Y * pictureBox.Height)
            Dim nor_pt4 As Point = New Point(item.angle_object.nor_pt4.X * pictureBox.Width, item.angle_object.nor_pt4.Y * pictureBox.Height)
            Dim nor_pt5 As Point = New Point(item.angle_object.nor_pt5.X * pictureBox.Width, item.angle_object.nor_pt5.Y * pictureBox.Height)
            Dim nor_pt6 As Point = New Point(item.angle_object.nor_pt6.X * pictureBox.Width, item.angle_object.nor_pt6.Y * pictureBox.Height)
            Dim side_pt As Point = New Point(item.angle_object.side_drag.X * pictureBox.Width, item.angle_object.side_drag.Y * pictureBox.Height)
            Dim start_angle = item.angle_object.start_angle
            Dim sweep_angle = item.angle_object.sweep_angle
            If item.measure_type = MeasureType.angle_far Then middle_point = inter_pt
            Dim radius = Convert.ToInt32(item.angle_object.radius * pictureBox.Width)
            graph.DrawArc(graphPen2, New Rectangle(middle_point.X - radius, middle_point.Y - radius, radius * 2, radius * 2), CSng(start_angle), CSng(sweep_angle))

            If item.measure_type = MeasureType.angle OrElse item.measure_type = MeasureType.angle_fixed Then
                graph.DrawLine(graphPen2, middle_point, st_pt)
                graph.DrawLine(graphPen2, middle_point, end_pt)
            End If
            graph.DrawLine(graphPen2, nor_pt1, nor_pt2)
            graph.DrawLine(graphPen2, nor_pt1, nor_pt3)
            graph.DrawLine(graphPen2, nor_pt4, nor_pt5)
            graph.DrawLine(graphPen2, nor_pt4, nor_pt6)
            If item.angle_object.side_index = 1 Then
                graph.DrawLine(graphPen2, nor_pt1, side_pt)
            Else
                graph.DrawLine(graphPen2, nor_pt4, side_pt)
            End If

            graph.RotateTransform(item.angle_object.trans_angle)
            Dim draw_pt As Point = New Point(item.angle_object.draw_pt.X * pictureBox.Width, item.angle_object.draw_pt.Y * pictureBox.Height)
            Dim trans_pt = GetRotationTransform(draw_pt, item.angle_object.trans_angle)
            Dim length_decimal = GetDecimalNumber(Math.Abs(item.angle_object.sweep_angle), digit, 1)

            If Main_Form.show_legend = True Then
                Dim output = item.name + " " + length_decimal.ToString()
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            Else
                Dim output = item.name
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            End If
            graph.RotateTransform(-1 * item.angle_object.trans_angle)
        ElseIf item.measure_type = MeasureType.radius Then
            Dim center_pt As Point = New Point(item.radius_object.center_pt.X * pictureBox.Width, item.radius_object.center_pt.Y * pictureBox.Height)
            Dim circle_pt As Point = New Point(item.radius_object.circle_pt.X * pictureBox.Width, item.radius_object.circle_pt.Y * pictureBox.Height)
            'int radius = Convert.ToInt32(Utils.CalcDistBetweenPoints(center_pt, circle_pt));
            Dim draw_pt As Point = New Point(item.radius_object.draw_pt.X * pictureBox.Width, item.radius_object.draw_pt.Y * pictureBox.Height)
            Dim arr_pt1 As Point = New Point(item.radius_object.arr_pt1.X * pictureBox.Width, item.radius_object.arr_pt1.Y * pictureBox.Height)
            Dim arr_pt2 As Point = New Point(item.radius_object.arr_pt2.X * pictureBox.Width, item.radius_object.arr_pt2.Y * pictureBox.Height)
            Dim arr_pt3 As Point = New Point(item.radius_object.arr_pt3.X * pictureBox.Width, item.radius_object.arr_pt3.Y * pictureBox.Height)
            Dim arr_pt4 As Point = New Point(item.radius_object.arr_pt4.X * pictureBox.Width, item.radius_object.arr_pt4.Y * pictureBox.Height)
            Dim trans_angle As Single = item.radius_object.trans_angle

            'graph.DrawArc(graphPen2, new Rectangle(center_pt.X - radius, center_pt.Y - radius, radius * 2, radius * 2), 0, 360);
            Dim A = start_point
            Dim B = middle_point
            Dim C = end_point
            Dim d_AB = Math.Sqrt(Math.Pow(B.X - A.X, 2.0R) + Math.Pow(B.Y - A.Y, 2.0R))
            Dim d_BC = Math.Sqrt(Math.Pow(B.X - C.X, 2.0R) + Math.Pow(B.Y - C.Y, 2.0R))
            Dim d_AC = Math.Sqrt(Math.Pow(C.X - A.X, 2.0R) + Math.Pow(C.Y - A.Y, 2.0R))
            If d_AB + d_BC < d_AC + 0.2R And d_AB + d_BC > d_AC - 0.2R Then
                graph.DrawLine(graphPen2, A, B)
                graph.DrawLine(graphPen2, B, C)
            Else
                Dim t = New Triangle(New Point3d(start_point.X, start_point.Y, 0R), New Point3d(middle_point.X, middle_point.Y, 0R), New Point3d(end_point.X, end_point.Y, 0R))
                Dim angle_a = t.Angle_A * 360.0R / Math.PI
                Dim angle_b = t.Angle_B * 360.0R / Math.PI
                Dim angle_c = t.Angle_C * 360.0R / Math.PI
                Dim circumcenterpt = t.Circumcenter
                Dim centerpt = New Point(Convert.ToInt32(circumcenterpt.X), Convert.ToInt32(circumcenterpt.Y))
                Dim radius = Convert.ToInt32(t.Circumcircle.R)
                Dim right_cenerpt = New Point(centerpt.X + radius, centerpt.Y)
                Dim M = New PointF(right_cenerpt.X / CSng(pictureBox.Width), right_cenerpt.Y / CSng(pictureBox.Height))
                item = sorting_Points(item, M)
                A = New Point(item.start_point.X * pictureBox.Width, item.start_point.Y * pictureBox.Height)
                B = New Point(item.middle_point.X * pictureBox.Width, item.middle_point.Y * pictureBox.Height)
                C = New Point(item.end_point.X * pictureBox.Width, item.end_point.Y * pictureBox.Height)


                If centerpt.Y = C.Y Then
                    Return
                End If
                Dim p_t = New Triangle(New Point3d(centerpt.X, centerpt.Y, 0R), New Point3d(C.X, C.Y, 0R), New Point3d(centerpt.X + radius, centerpt.Y, 0R))
                Dim rotate_angle = Convert.ToInt32(angle_a + angle_c)
                Dim add_a = 0
                If A.Y < right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    If B.X > A.X And B.X > C.X Or B.X < A.X And B.X < C.X Or B.X < A.X And B.X > C.X Then
                        add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                    End If
                ElseIf A.Y < right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y < right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y < right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                End If
                graph.DrawArc(graphPen2, New Rectangle(centerpt.X - radius, centerpt.Y - radius, radius * 2, radius * 2), add_a, rotate_angle)

                graph.DrawLine(graphPen2, center_pt, circle_pt)
                graph.DrawLine(graphPen2, center_pt, arr_pt1)
                graph.DrawLine(graphPen2, center_pt, arr_pt2)
                graph.DrawLine(graphPen2, circle_pt, arr_pt3)
                graph.DrawLine(graphPen2, circle_pt, arr_pt4)

                graph.RotateTransform(trans_angle)
                Dim trans_pt = GetRotationTransform(draw_pt, trans_angle)

                Dim length_decimal = GetDecimalNumber(item.radius, digit, CF)

                If Main_Form.show_legend = True Then
                    Dim output = item.name + " " + length_decimal.ToString()
                    Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                    graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
                Else
                    Dim output = item.name
                    Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                    graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
                End If
                graph.RotateTransform(-1 * trans_angle)
            End If
        ElseIf item.measure_type = MeasureType.annotation Then
            Dim textsize As RectangleF = New RectangleF()
            textsize.X = draw_point.X
            textsize.Y = draw_point.Y
            textsize.Width = item.anno_object.size.Width
            textsize.Height = item.anno_object.size.Height
            graph.DrawString(item.annotation, graphFont, graphBrush, textsize)
            Dim left_top As Point = New Point(item.left_top.X * pictureBox.Width, item.left_top.Y * pictureBox.Height)
            Dim line_pt As Point = New Point(item.anno_object.line_pt.X * pictureBox.Width, item.anno_object.line_pt.Y * pictureBox.Height)
            graph.DrawLine(graphPen2, start_point, line_pt)
        ElseIf item.measure_type = MeasureType.draw_line Then
            If Equals(item.line_infor.line_style, "dotted") Then
                graphPen2.DashStyle = Drawing2D.DashStyle.Dot
            ElseIf Equals(item.line_infor.line_style, "dashed") Then
                Dim dashValues As Single() = {5, 2}
                graphPen2.DashStyle = Drawing2D.DashStyle.Dash
                graphPen2.DashPattern = dashValues
            End If
            graph.DrawLine(graphPen2, start_point, end_point)
        ElseIf item.measure_type = MeasureType.measure_scale Then
            Dim nor_pt1 As Point = New Point()
            Dim nor_pt2 As Point = New Point()
            Dim nor_pt3 As Point = New Point()
            Dim nor_pt4 As Point = New Point()
            If Equals(item.scale_object.style, "horizontal") Then
                end_point.Y = start_point.Y
                nor_pt1.X = start_point.X
                nor_pt1.Y = start_point.Y - 10
                nor_pt2.X = start_point.X
                nor_pt2.Y = start_point.Y + 10
                nor_pt3.X = end_point.X
                nor_pt3.Y = end_point.Y - 10
                nor_pt4.X = end_point.X
                nor_pt4.Y = end_point.Y + 10
            Else
                end_point.X = start_point.X
                nor_pt1.X = start_point.X - 10
                nor_pt1.Y = start_point.Y
                nor_pt2.X = start_point.X + 10
                nor_pt2.Y = start_point.Y
                nor_pt3.X = end_point.X - 10
                nor_pt3.Y = end_point.Y
                nor_pt4.X = end_point.X + 10
                nor_pt4.Y = end_point.Y
            End If

            Dim trans_angle As Single = item.scale_object.trans_angle
            Dim draw_pt As Point = New Point((start_point.X + end_point.X) / 2, (start_point.Y + end_point.Y) / 2)

            If Equals(item.scale_object.style, "horizontal") Then
                draw_pt.Y += 20
            Else
                draw_pt.X += 20
            End If
            graph.DrawLine(graphPen2, start_point, end_point)
            graph.DrawLine(graphPen2, nor_pt1, nor_pt2)
            graph.DrawLine(graphPen2, nor_pt3, nor_pt4)

            graph.RotateTransform(trans_angle)
            Dim trans_pt = GetRotationTransform(draw_pt, trans_angle)

            Dim length_decimal = GetDecimalNumber(item.length, digit, CF)
            If Main_Form.show_legend = True Then
                Dim output = item.name + " " + length_decimal.ToString()
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            Else
                Dim output = item.name
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            End If

            graph.RotateTransform(-1 * trans_angle)
        ElseIf item.measure_type = MeasureType.circle_fixed Then
            Dim center_pt As Point = New Point(item.radius_object.center_pt.X * pictureBox.Width, item.radius_object.center_pt.Y * pictureBox.Height)
            Dim deltaX = item.radius_object.center_pt.X
            Dim deltaY = item.radius_object.center_pt.Y
            Dim circle_pt As Point = New Point((item.radius_object.circle_pt.X / CF + deltaX) * pictureBox.Width, (item.radius_object.circle_pt.Y / CF + deltaY) * pictureBox.Height)
            Dim draw_pt As Point = New Point((item.radius_object.draw_pt.X / CF + deltaX) * pictureBox.Width, (item.radius_object.draw_pt.Y / CF + deltaY) * pictureBox.Height)
            Dim arr_pt1 As Point = New Point((item.radius_object.arr_pt1.X / CF + deltaX) * pictureBox.Width, (item.radius_object.arr_pt1.Y / CF + deltaY) * pictureBox.Height)
            Dim arr_pt2 As Point = New Point((item.radius_object.arr_pt2.X / CF + deltaX) * pictureBox.Width, (item.radius_object.arr_pt2.Y / CF + deltaY) * pictureBox.Height)
            Dim arr_pt3 As Point = New Point((item.radius_object.arr_pt3.X / CF + deltaX) * pictureBox.Width, (item.radius_object.arr_pt3.Y / CF + deltaY) * pictureBox.Height)
            Dim arr_pt4 As Point = New Point((item.radius_object.arr_pt4.X / CF + deltaX) * pictureBox.Width, (item.radius_object.arr_pt4.Y / CF + deltaY) * pictureBox.Height)
            Dim trans_angle As Single = item.radius_object.trans_angle

            Dim radius = CInt(item.radius / CF * pictureBox.Width)
            graph.DrawArc(graphPen2, New Rectangle(center_pt.X - radius, center_pt.Y - radius, radius * 2, radius * 2), 0, 360)

            graph.DrawLine(graphPen2, center_pt, circle_pt)
            graph.DrawLine(graphPen2, center_pt, arr_pt1)
            graph.DrawLine(graphPen2, center_pt, arr_pt2)
            graph.DrawLine(graphPen2, circle_pt, arr_pt3)
            graph.DrawLine(graphPen2, circle_pt, arr_pt4)

            graph.RotateTransform(trans_angle)
            Dim trans_pt = GetRotationTransform(draw_pt, trans_angle)

            'Dim length_decimal = item.scale_object.length
            Dim length_decimal = GetDecimalNumber(item.scale_object.length, digit, 1)

            If Main_Form.show_legend = True Then
                Dim output = item.name + " " + length_decimal.ToString()
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            Else
                Dim output = item.name
                Dim textSize As SizeF = graph.MeasureString(output, graphFont)
                graph.DrawString(output, graphFont, graphBrush, New RectangleF(trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2, textSize.Width, textSize.Height))
            End If
            graph.RotateTransform(-1 * trans_angle)
        Else
            DrawCurveObjItem(graph, pictureBox, item, digit, CF, False)
        End If

        graphPen2.Dispose()
        graphBrush.Dispose()
    End Sub


    ''' <summary>
    ''' draw object list to picture box control.
    ''' </summary>
    ''' <paramname="graph">The graphics for drawing object list.</param>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="object_list">The list of objects which you are going to draw.</param>
    ''' <paramname="graphPen">The pen for drawing objects.</param>
    ''' <paramname="graphPen_line">The pen for drawing lines.</param>
    ''' <paramname="digit">The digit of decimal numbers.</param>
    ''' <paramname="CF">The factor of measuring scale.</param>
    Public Sub DrawObjList2(ByVal graph As Graphics, ByVal pictureBox As PictureBox, ByVal object_list As List(Of MeasureObject), ByVal graphPen As Pen, ByVal graphPen_line As Pen, ByVal digit As Integer, ByVal CF As Double)
        For Each item In object_list
            DrawObjItem(graph, pictureBox, item, digit, CF)
        Next

    End Sub

    ''' <summary>
    ''' draw object selected to picture box control.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="obj_selected">The object which you are going to draw.</param>
    ''' <paramname="flag">The flag specifies the object is completed or not.</param>
    <Extension()>
    Public Sub DrawObjSelected(ByVal pictureBox As PictureBox, ByVal obj_selected As MeasureObject, ByVal flag As Boolean)
        Dim graph As Graphics = pictureBox.CreateGraphics()
        Dim graphPen As Pen = New Pen(Color.Red, 1)

        Dim start_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim draw_point As Point = New Point()
        Dim last_point As Point = New Point()

        start_point.X = CInt(obj_selected.start_point.X * pictureBox.Width)
        start_point.Y = CInt(obj_selected.start_point.Y * pictureBox.Height)
        end_point.X = CInt(obj_selected.end_point.X * pictureBox.Width)
        end_point.Y = CInt(obj_selected.end_point.Y * pictureBox.Height)

        middle_point.X = CInt(obj_selected.middle_point.X * pictureBox.Width)
        middle_point.Y = CInt(obj_selected.middle_point.Y * pictureBox.Height)
        draw_point.X = CInt(obj_selected.draw_point.X * pictureBox.Width)
        draw_point.Y = CInt(obj_selected.draw_point.Y * pictureBox.Height)
        last_point.X = CInt(obj_selected.last_point.X * pictureBox.Width)
        last_point.Y = CInt(obj_selected.last_point.Y * pictureBox.Height)

        Dim radius = 2

        For i = 0 To obj_selected.item_set - 1
            If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_horizontal OrElse obj_selected.measure_type = MeasureType.line_vertical OrElse obj_selected.measure_type = MeasureType.draw_line OrElse obj_selected.measure_type = MeasureType.measure_scale OrElse obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.circle_fixed OrElse obj_selected.measure_type = MeasureType.line_fixed Then
                If i = 0 Then
                    graph.DrawArc(graphPen, New Rectangle(start_point.X - radius, start_point.Y - radius, radius * 2, radius * 2), 0, 360)
                    If obj_selected.measure_type = MeasureType.measure_scale Then
                        graph.DrawArc(graphPen, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2), 0, 360)
                    End If
                ElseIf i = 1 And obj_selected.measure_type <> MeasureType.circle_fixed Then
                    graph.DrawArc(graphPen, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2), 0, 360)
                End If
            ElseIf obj_selected.measure_type = MeasureType.angle OrElse obj_selected.measure_type = MeasureType.radius OrElse obj_selected.measure_type = MeasureType.angle_far OrElse obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line OrElse obj_selected.measure_type = MeasureType.angle_fixed Then
                If i = 0 Then
                    graph.DrawArc(graphPen, New Rectangle(start_point.X - radius, start_point.Y - radius, radius * 2, radius * 2), 0, 360)
                ElseIf i = 1 Then
                    graph.DrawArc(graphPen, New Rectangle(middle_point.X - radius, middle_point.Y - radius, radius * 2, radius * 2), 0, 360)
                ElseIf i = 2 Then
                    graph.DrawArc(graphPen, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2), 0, 360)
                ElseIf i = 3 Then
                    graph.DrawArc(graphPen, New Rectangle(last_point.X - radius, last_point.Y - radius, radius * 2, radius * 2), 0, 360)
                End If
            End If

        Next
        graph.Dispose()
        graphPen.Dispose()

    End Sub

    ''' <summary>
    ''' Highlight selected point.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="obj_selected">The object which you are going to draw.</param>
    ''' <paramname="pt_index">The index of point.</param>
    <Extension()>
    Public Sub HighlightTargetPt(ByVal pictureBox As PictureBox, ByVal obj_selected As MeasureObject, ByVal pt_index As Integer)
        Dim graph As Graphics = pictureBox.CreateGraphics()
        Dim graphPen As Pen = New Pen(Color.Yellow, 2)

        Dim start_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim draw_point As Point = New Point()
        Dim last_point As Point = New Point()

        start_point.X = CInt(obj_selected.start_point.X * pictureBox.Width)
        start_point.Y = CInt(obj_selected.start_point.Y * pictureBox.Height)
        end_point.X = CInt(obj_selected.end_point.X * pictureBox.Width)
        end_point.Y = CInt(obj_selected.end_point.Y * pictureBox.Height)

        middle_point.X = CInt(obj_selected.middle_point.X * pictureBox.Width)
        middle_point.Y = CInt(obj_selected.middle_point.Y * pictureBox.Height)
        draw_point.X = CInt(obj_selected.draw_point.X * pictureBox.Width)
        draw_point.Y = CInt(obj_selected.draw_point.Y * pictureBox.Height)
        last_point.X = CInt(obj_selected.last_point.X * pictureBox.Width)
        last_point.Y = CInt(obj_selected.last_point.Y * pictureBox.Height)

        Dim radius = 3

        Select Case pt_index
            Case 1
                graph.DrawArc(graphPen, New Rectangle(start_point.X - radius, start_point.Y - radius, radius * 2, radius * 2), 0, 360)
            Case 2
                graph.DrawArc(graphPen, New Rectangle(middle_point.X - radius, middle_point.Y - radius, radius * 2, radius * 2), 0, 360)
            Case 3
                graph.DrawArc(graphPen, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2), 0, 360)
            Case 4
                graph.DrawArc(graphPen, New Rectangle(last_point.X - radius, last_point.Y - radius, radius * 2, radius * 2), 0, 360)
        End Select
    End Sub

    ''' <summary>
    ''' check if there is point in pos mouse clicked.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="obj_selected">The object which you are going to draw.</param>
    ''' <paramname="m_pt">The position of mouse cursor.</param>
    <Extension()>
    Public Function CheckPointInPos(ByVal pictureBox As PictureBox, ByVal obj_selected As MeasureObject, ByVal m_pt As Point) As Integer
        Dim flag = False
        Dim start_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim draw_point As Point = New Point()
        Dim last_point As Point = New Point()

        start_point.X = CInt(obj_selected.start_point.X * pictureBox.Width)
        start_point.Y = CInt(obj_selected.start_point.Y * pictureBox.Height)
        end_point.X = CInt(obj_selected.end_point.X * pictureBox.Width)
        end_point.Y = CInt(obj_selected.end_point.Y * pictureBox.Height)
        middle_point.X = CInt(obj_selected.middle_point.X * pictureBox.Width)
        middle_point.Y = CInt(obj_selected.middle_point.Y * pictureBox.Height)
        draw_point.X = CInt(obj_selected.draw_point.X * pictureBox.Width)
        draw_point.Y = CInt(obj_selected.draw_point.Y * pictureBox.Height)
        last_point.X = CInt(obj_selected.last_point.X * pictureBox.Width)
        last_point.Y = CInt(obj_selected.last_point.Y * pictureBox.Height)

        Dim radius = 5

        For i = 0 To obj_selected.item_set - 1
            If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_horizontal OrElse obj_selected.measure_type = MeasureType.line_vertical OrElse obj_selected.measure_type = MeasureType.draw_line OrElse obj_selected.measure_type = MeasureType.measure_scale Then
                If i = 0 Then
                    flag = PointInRect(m_pt, New Rectangle(start_point.X - radius, start_point.Y - radius, radius * 2, radius * 2))
                    If flag Then
                        Return 1
                    Else
                        If obj_selected.measure_type = MeasureType.measure_scale Then
                            flag = PointInRect(m_pt, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2))
                            If flag Then Return 3
                        End If
                    End If
                ElseIf i = 1 Then
                    flag = PointInRect(m_pt, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2))
                    If flag Then Return 3
                End If
            ElseIf obj_selected.measure_type = MeasureType.angle OrElse obj_selected.measure_type = MeasureType.radius OrElse obj_selected.measure_type = MeasureType.angle_far OrElse obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line Then
                If i = 0 Then
                    flag = PointInRect(m_pt, New Rectangle(start_point.X - radius, start_point.Y - radius, radius * 2, radius * 2))
                    If flag Then Return 1
                ElseIf i = 1 Then
                    flag = PointInRect(m_pt, New Rectangle(middle_point.X - radius, middle_point.Y - radius, radius * 2, radius * 2))
                    If flag Then Return 2
                ElseIf i = 2 Then
                    flag = PointInRect(m_pt, New Rectangle(end_point.X - radius, end_point.Y - radius, radius * 2, radius * 2))
                    If flag Then Return 3
                ElseIf i = 3 Then
                    flag = PointInRect(m_pt, New Rectangle(last_point.X - radius, last_point.Y - radius, radius * 2, radius * 2))
                    If flag Then Return 4
                End If
            End If

        Next

        Return -1
    End Function

    ''' <summary>
    ''' calculate start angle sweep angle between two lines
    ''' </summary>
    ''' <paramname="obj_selected">the object currently used.</param>
    ''' <paramname="start_point">The start point of the line.</param>
    ''' <paramname="middle_point">The middle point of the line.</param>
    ''' <paramname="end_point">The end point of the line.</param>
    ''' <paramname="target_point">The point of mouse cursor.</param>
    Public Function CalcStartAndSweepAngle(ByVal obj_selected As MeasureObject, ByVal start_point As Point, ByVal middle_point As Point, ByVal end_point As Point, ByVal target_point As Point) As Double()
        Dim angle = CalcAngleBetweenTwoLines(start_point, middle_point, target_point)
        angle = angle * 360 / Math.PI / 2
        'to calcuate the angle between x-axis and start_point-middle_point line
        Dim basis_pt As Point = New Point(middle_point.X + 10, middle_point.Y)

        Dim angle2 = CalcAngleBetweenTwoLines(basis_pt, middle_point, start_point)
        angle2 = angle2 * 360 / Math.PI / 2

        Dim centerpt = middle_point
        Dim radius = Convert.ToInt32(Math.Sqrt(Math.Pow(target_point.X - middle_point.X, 2) + Math.Pow(target_point.Y - middle_point.Y, 2)))
        Dim radius2 = If(radius - 10 > 1, radius - 10, 1)
        Dim clockwise = CheckAngleDirection(start_point, middle_point, end_point)
        Dim downbasis = CheckAngleDirection(basis_pt, middle_point, start_point)
        Dim start_angle, sweep_angle As Double
        Dim angle_direction As Integer

        If clockwise Then
            angle_direction = 1
        Else
            angle_direction = -1
        End If

        If 0 <= angle AndAlso angle < obj_selected.angle Then
            If downbasis Then
                start_angle = angle2
                sweep_angle = angle_direction * obj_selected.angle
            Else
                start_angle = 360 - angle2
                sweep_angle = angle_direction * obj_selected.angle
            End If
        Else
            If downbasis Then
                If clockwise Then
                    start_angle = angle2 + obj_selected.angle
                    sweep_angle = angle_direction * (180 - obj_selected.angle)
                Else
                    start_angle = 360 + angle2 - obj_selected.angle
                    sweep_angle = angle_direction * (180 - obj_selected.angle)
                End If
            Else
                If clockwise Then
                    start_angle = obj_selected.angle - angle2
                    sweep_angle = angle_direction * (180 - obj_selected.angle)
                Else
                    start_angle = 360 - obj_selected.angle - angle2
                    sweep_angle = angle_direction * (180 - obj_selected.angle)
                End If
            End If
        End If
        Dim ang_dirc As Integer
        If clockwise Then
            ang_dirc = 1
        Else
            ang_dirc = 2
        End If
        Dim angles = {start_angle, sweep_angle, ang_dirc}
        Return angles
    End Function

    ''' <summary>
    ''' calculate start angle sweep angle between two lines
    ''' </summary>
    ''' <paramname="obj_selected">the object currently used.</param>
    ''' <paramname="start_point">The start point of the line.</param>
    ''' <paramname="middle_point">The middle point of the line.</param>
    ''' <paramname="end_point">The end point of the line.</param>
    ''' <paramname="target_point">The point of mouse cursor.</param>
    Public Function CalcStartAndSweepAngleFixed(ByVal obj_selected As MeasureObject, ByVal start_point As Point, ByVal middle_point As Point, ByVal target_point As Point) As Double()
        'to calcuate the angle between x-axis and start_point-middle_point line
        Dim basis_pt As Point = New Point(middle_point.X + 10, middle_point.Y)
        Dim flag = CheckPointOnLine(start_point, middle_point, target_point)
        Dim angle2 = CalcAngleBetweenTwoLines(basis_pt, middle_point, start_point)
        angle2 = angle2 * 360 / Math.PI / 2

        Dim centerpt = middle_point
        Dim radius = Convert.ToInt32(Math.Sqrt(Math.Pow(target_point.X - middle_point.X, 2) + Math.Pow(target_point.Y - middle_point.Y, 2)))
        Dim radius2 = If(radius - 10 > 1, radius - 10, 1)
        Dim clockwise = True
        Dim downbasis = CheckAngleDirection(basis_pt, middle_point, start_point)
        Dim start_angle, sweep_angle As Double
        Dim angle_direction As Integer

        If clockwise Then
            angle_direction = 1
        Else
            angle_direction = -1
        End If

        If flag = 0 Or flag = 1 Then
            If downbasis Then
                start_angle = angle2
                sweep_angle = obj_selected.angle
            Else
                start_angle = 360 - angle2
                sweep_angle = obj_selected.angle
            End If
        ElseIf flag = -1 Then

            If downbasis Then
                start_angle = angle2
                sweep_angle = -1 * (obj_selected.angle)
            Else
                start_angle = 360 - angle2
                sweep_angle = -1 * (obj_selected.angle)
            End If
        End If
        Dim ang_dirc As Integer
        If flag >= 0 Then
            ang_dirc = 1
        Else
            ang_dirc = 2
        End If
        Dim angles = {start_angle, sweep_angle, ang_dirc}
        Return angles
    End Function

    ''' <summary>
    ''' draw temporal line when you are finializing current object.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="obj_selected">The object which you are going to draw.</param>
    ''' <paramname="target_point">The point of mouse cursor.</param>
    ''' <paramname="side_drag">the flag which determines side dragging.</param>
    ''' <paramname="digit">the digit of decimal numbers.</param>
    ''' <paramname="CF">the factor of measuring scale.</param>
    ''' <paramname="draw_flag">the flag which determines drawing string.</param>
    <Extension()>
    Public Sub DrawTempFinal(ByVal pictureBox As PictureBox, ByRef obj_selected As MeasureObject, ByVal target_point As Point, ByVal side_drag As Boolean, ByVal digit As Integer, ByVal CF As Double, ByVal draw_flag As Boolean)
        Dim offset As Size = New Size()     'offset from draw point to line consists of start point and end point
        Dim offset2 As Size = New Size()      'offset from end point to final temp
        Dim graph As Graphics = pictureBox.CreateGraphics()
        Dim graphPen As Pen = New Pen(obj_selected.line_infor.line_color, obj_selected.line_infor.line_width)
        Dim graphFont = obj_selected.font_infor.text_font
        Dim graphBrush As SolidBrush = New SolidBrush(obj_selected.font_infor.font_color)

        Dim start_point As Point = New Point()
        Dim end_point As Point = New Point()
        Dim middle_point As Point = New Point()
        Dim draw_point As Point = New Point()

        Dim last_point As Point = New Point()
        Dim inter_pt As Point = New Point()

        start_point.X = CInt(obj_selected.start_point.X * pictureBox.Width)
        start_point.Y = CInt(obj_selected.start_point.Y * pictureBox.Height)
        middle_point.X = CInt(obj_selected.middle_point.X * pictureBox.Width)
        middle_point.Y = CInt(obj_selected.middle_point.Y * pictureBox.Height)
        end_point.X = CInt(obj_selected.end_point.X * pictureBox.Width)
        end_point.Y = CInt(obj_selected.end_point.Y * pictureBox.Height)
        draw_point.X = CInt(obj_selected.draw_point.X * pictureBox.Width)
        draw_point.Y = CInt(obj_selected.draw_point.Y * pictureBox.Height)

        last_point.X = CInt(obj_selected.last_point.X * pictureBox.Width)
        last_point.Y = CInt(obj_selected.last_point.Y * pictureBox.Height)
        inter_pt.X = CInt(obj_selected.common_point.X * pictureBox.Width)
        inter_pt.Y = CInt(obj_selected.common_point.Y * pictureBox.Height)

        If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_horizontal OrElse obj_selected.measure_type = MeasureType.line_vertical OrElse obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line OrElse obj_selected.measure_type = MeasureType.line_fixed Then
            If (obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line) AndAlso obj_selected.item_set < 3 Then Return
            If obj_selected.item_set < 2 Then Return
            'pictureBox.Refresh();

            Dim nor_point1 As Point = New Point()
            Dim nor_point2 As Point = New Point()
            If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_fixed Then
                offset = GetNormalFromPointToLine(start_point, end_point, target_point)
                nor_point1.X = start_point.X - offset.Width
                nor_point1.Y = start_point.Y - offset.Height
                nor_point2.X = end_point.X - offset.Width
                nor_point2.Y = end_point.Y - offset.Height
            ElseIf obj_selected.measure_type = MeasureType.line_horizontal Then
                nor_point1.X = start_point.X
                nor_point1.Y = If(target_point.Y + 10 < pictureBox.Height, target_point.Y + 10, target_point.Y)
                nor_point2.X = end_point.X
                nor_point2.Y = If(target_point.Y + 10 < pictureBox.Height, target_point.Y + 10, target_point.Y)
            ElseIf obj_selected.measure_type = MeasureType.line_vertical Then
                nor_point1.X = If(target_point.X + 10 < pictureBox.Width, target_point.X + 10, target_point.X)
                nor_point1.Y = start_point.Y
                nor_point2.X = If(target_point.X + 10 < pictureBox.Width, target_point.X + 10, target_point.X)
                nor_point2.Y = end_point.Y
            ElseIf obj_selected.measure_type = MeasureType.line_para Then
                nor_point1 = middle_point
                offset = GetNormalFromPointToLine(start_point, middle_point, end_point)
                last_point.X = start_point.X - offset.Width
                last_point.Y = start_point.Y - offset.Height
                obj_selected.last_point = New PointF(CSng(start_point.X - offset.Width) / pictureBox.Width, CSng(start_point.Y - offset.Height) / pictureBox.Height)
                nor_point2 = New Point(middle_point.X - offset.Width, middle_point.Y - offset.Height)
            ElseIf obj_selected.measure_type = MeasureType.pt_line Then
                nor_point1 = middle_point
                nor_point2 = end_point
            End If


            'graph.DrawLine(graphPen, start_point, nor_point1);
            'if (obj_selected.measure_type == (int)MeasureType.line_para)
            '    graph.DrawLine(graphPen, last_point, nor_point2);
            'else
            '    graph.DrawLine(graphPen, end_point, nor_point2);

            Dim nor_point3 As Point = New Point()
            Dim nor_point4 As Point = New Point()
            If obj_selected.measure_type = MeasureType.line_align OrElse obj_selected.measure_type = MeasureType.line_fixed Then
                offset2 = GetNormalToLineFixedLen(start_point, end_point, target_point, 10, True)

                nor_point3.X = nor_point1.X + offset2.Width
                nor_point3.Y = nor_point1.Y + offset2.Height
                nor_point4.X = nor_point2.X + offset2.Width
                nor_point4.Y = nor_point2.Y + offset2.Height
            ElseIf obj_selected.measure_type = MeasureType.line_horizontal OrElse obj_selected.measure_type = MeasureType.line_vertical Then
                nor_point3 = nor_point1
                nor_point4 = nor_point2
            ElseIf obj_selected.measure_type = MeasureType.line_para Then
                Dim offset3 = GetNormalFromPointToLine(start_point, nor_point1, target_point)
                nor_point3.X = target_point.X + offset3.Width
                nor_point3.Y = target_point.Y + offset3.Height
                Dim X_max = Math.Max(start_point.X, nor_point1.X)
                Dim X_min = Math.Min(start_point.X, nor_point1.X)
                nor_point3.X = Math.Min(Math.Max(nor_point3.X, X_min), X_max)
                Dim Y_max = Math.Max(start_point.Y, nor_point1.Y)
                Dim Y_min = Math.Min(start_point.Y, nor_point1.Y)
                nor_point3.Y = Math.Min(Math.Max(nor_point3.Y, Y_min), Y_max)
                offset3 = GetNormalFromPointToLine(last_point, nor_point2, target_point)
                nor_point4.X = target_point.X + offset3.Width
                nor_point4.Y = target_point.Y + offset3.Height
                X_max = Math.Max(last_point.X, nor_point2.X)
                X_min = Math.Min(last_point.X, nor_point2.X)
                nor_point4.X = Math.Min(Math.Max(nor_point4.X, X_min), X_max)
                Y_max = Math.Max(last_point.Y, nor_point2.Y)
                Y_min = Math.Min(last_point.Y, nor_point2.Y)
                nor_point4.Y = Math.Min(Math.Max(nor_point4.Y, Y_min), Y_max)
            ElseIf obj_selected.measure_type = MeasureType.pt_line Then
                nor_point3 = end_point
                offset = GetNormalFromPointToLine(start_point, middle_point, end_point)
                nor_point4.X = nor_point3.X + offset.Width
                nor_point4.Y = nor_point3.Y + offset.Height
            End If

            Dim Side_pt = nor_point4
            If side_drag Then
                Dim dist1 = CalcDistFromPointToLine(start_point, nor_point1, target_point)
                Dim dist2 As Double
                If obj_selected.measure_type = MeasureType.line_para Then
                    dist2 = CalcDistFromPointToLine(last_point, nor_point2, target_point)
                Else
                    dist2 = CalcDistFromPointToLine(end_point, nor_point2, target_point)
                End If

                Dim sum = CalcDistBetweenPoints(nor_point3, nor_point4)

                If obj_selected.measure_type = MeasureType.pt_line Then
                    Side_pt.X = CInt((nor_point4.X - nor_point3.X) / sum * dist1 + nor_point4.X)
                    'graph.DrawLine(graphPen, nor_point4, Side_pt);
                    Side_pt.Y = CInt((nor_point4.Y - nor_point3.Y) / sum * dist1 + nor_point4.Y)
                Else
                    If dist1 > dist2 Then
                        Side_pt.X = CInt((nor_point4.X - nor_point3.X) / sum * dist2 + nor_point4.X)
                        'graph.DrawLine(graphPen, nor_point4, Side_pt);
                        Side_pt.Y = CInt((nor_point4.Y - nor_point3.Y) / sum * dist2 + nor_point4.Y)
                    Else
                        Side_pt.X = CInt((nor_point3.X - nor_point4.X) / sum * dist1 + nor_point3.X)
                        Side_pt.Y = CInt((nor_point3.Y - nor_point4.Y) / sum * dist1 + nor_point3.Y)
                        'graph.DrawLine(graphPen, nor_point3, Side_pt);
                    End If
                End If
            End If

            'graph.DrawLine(graphPen, nor_point3, nor_point4);
            Dim n_dist As Integer = CalcDistBetweenPoints(nor_point3, nor_point4)
            n_dist /= 10
            n_dist = Math.Max(n_dist, 5)

            Dim arr_points = New Point(1) {}
            Dim arr_points2 = New Point(1) {}
            If obj_selected.measure_type = MeasureType.line_para OrElse obj_selected.measure_type = MeasureType.pt_line Then
                arr_points = GetArrowPoints3(nor_point3, nor_point4, n_dist)
                arr_points2 = GetArrowPoints3(nor_point4, nor_point3, n_dist)
            Else
                arr_points = GetArrowPoints(start_point, end_point, nor_point1, nor_point3, n_dist)
                arr_points2 = GetArrowPoints(end_point, start_point, nor_point2, nor_point4, n_dist)
            End If
            'graph.DrawLine(graphPen, nor_point3, arr_points[0]);
            'graph.DrawLine(graphPen, nor_point3, arr_points[1]);
            'graph.DrawLine(graphPen, nor_point4, arr_points2[0]);
            'graph.DrawLine(graphPen, nor_point4, arr_points2[1]);

            'draw text
            Dim ang_tran As Integer = obj_selected.angle
            Dim attri = -1
            If obj_selected.angle > 90 Then
                ang_tran = 180 - ang_tran
                attri = 1
            End If

            'Point mid_pt = new Point((int)((start_point.X + end_point.X) / 2), (int)((start_point.Y + end_point.Y) / 2));
            'Point draw_pt = new Point(mid_pt.X - offset.Width, mid_pt.Y - offset.Height);

            'set limit to target point
            Dim draw_pt = target_point
            If obj_selected.measure_type = MeasureType.line_para Then
                Dim dist = CalcDistBetweenPoints(start_point, nor_point1)
                draw_pt.X += CInt((nor_point1.X - start_point.X) / dist * 20)
                draw_pt.Y += CInt((nor_point1.Y - start_point.Y) / dist * 20)
                Dim X_min = Math.Min(Math.Min(Math.Min(start_point.X, last_point.X), nor_point1.X), nor_point2.X)
                Dim X_max = Math.Max(Math.Max(Math.Max(start_point.X, last_point.X), nor_point1.X), nor_point2.X)
                Dim Y_min = Math.Min(Math.Min(Math.Min(start_point.Y, last_point.Y), nor_point1.Y), nor_point2.Y)
                Dim Y_max = Math.Max(Math.Max(Math.Max(start_point.Y, last_point.Y), nor_point1.Y), nor_point2.Y)
                draw_pt.X = Math.Min(Math.Max(draw_pt.X, X_min), X_max)
                draw_pt.Y = Math.Min(Math.Max(draw_pt.Y, Y_min), Y_max)
            ElseIf obj_selected.measure_type = MeasureType.pt_line Then
                Dim offset3 = GetNormalFromPointToLine(nor_point3, nor_point4, target_point, 10)
                If nor_point3.X = nor_point4.X Then
                    If nor_point3.Y > nor_point4.Y Then
                        draw_pt.Y = Math.Min(Math.Max(target_point.Y, nor_point4.Y), nor_point3.Y)
                    Else
                        draw_pt.Y = Math.Min(Math.Max(target_point.Y, nor_point3.Y), nor_point4.Y)
                    End If
                    draw_pt.X = GetXCoordinate(nor_point3, nor_point4, draw_pt)
                    draw_pt.X -= CInt(offset3.Width)
                    draw_pt.Y -= CInt(offset3.Height)
                Else
                    If nor_point3.X > nor_point4.X Then
                        draw_pt.X = Math.Min(Math.Max(target_point.X, nor_point4.X), nor_point3.X)
                    Else
                        draw_pt.X = Math.Min(Math.Max(target_point.X, nor_point3.X), nor_point4.X)
                    End If
                    draw_pt.Y = GetYCoordinate(nor_point3, nor_point4, draw_pt)
                    draw_pt.X -= CInt(offset3.Width)
                    draw_pt.Y -= CInt(offset3.Height)
                End If
            Else
                If obj_selected.measure_type <> MeasureType.line_horizontal Then
                    If nor_point1.Y > nor_point2.Y Then
                        draw_pt.Y = Math.Min(Math.Max(target_point.Y, nor_point2.Y), nor_point1.Y)
                    Else
                        draw_pt.Y = Math.Min(Math.Max(target_point.Y, nor_point1.Y), nor_point2.Y)
                    End If
                End If
                If obj_selected.measure_type <> MeasureType.line_vertical Then
                    If nor_point1.X > nor_point2.X Then
                        draw_pt.X = Math.Min(Math.Max(target_point.X, nor_point2.X), nor_point1.X)
                    Else
                        draw_pt.X = Math.Min(Math.Max(target_point.X, nor_point1.X), nor_point2.X)
                    End If
                End If
            End If

            If side_drag Then
                offset2 = GetNormalToLineFixedLen(nor_point3, nor_point4, target_point, 10, True)
                draw_pt.X = Side_pt.X - offset2.Width
                draw_pt.Y = Side_pt.Y - offset2.Height

            End If

            If draw_flag Then
                If obj_selected.measure_type = MeasureType.line_para Then end_point = last_point
                graph.DrawLine(graphPen, start_point, nor_point1)
                graph.DrawLine(graphPen, end_point, nor_point2)
                graph.DrawLine(graphPen, nor_point3, nor_point4)
                graph.DrawLine(graphPen, nor_point3, arr_points(0))
                graph.DrawLine(graphPen, nor_point3, arr_points(1))
                graph.DrawLine(graphPen, nor_point4, arr_points2(0))
                graph.DrawLine(graphPen, nor_point4, arr_points2(1))
                graph.DrawLine(graphPen, nor_point4, Side_pt)

                graph.RotateTransform(attri * ang_tran)
                Dim trans_pt = GetRotationTransform(draw_pt, attri * ang_tran)
                Dim length_decimal = GetDecimalNumber(obj_selected.length, digit, CF)
                If obj_selected.measure_type = MeasureType.line_fixed Then
                    length_decimal = obj_selected.scale_object.length
                End If
                Dim textSize As SizeF = graph.MeasureString(length_decimal.ToString(), graphFont)
                graph.DrawString(length_decimal.ToString(), graphFont, graphBrush, trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2)
            End If

            'Initialize line objects

            If obj_selected.measure_type = MeasureType.line_fixed Then
                obj_selected.line_object.nor_pt1 = New PointF(CSng(nor_point1.X) / pictureBox.Width, CSng(nor_point1.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt3 = New PointF(CSng(nor_point3.X) / pictureBox.Width, CSng(nor_point3.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt5 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt6 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
                Dim deltaX = obj_selected.line_object.nor_pt1.X
                Dim deltaY = obj_selected.line_object.nor_pt1.Y
                obj_selected.line_object.nor_pt2 = New PointF((CSng(nor_point2.X) / pictureBox.Width - deltaX) * CF, (CSng(nor_point2.Y) / pictureBox.Height - deltaY) * CF)
                deltaX = obj_selected.line_object.nor_pt3.X
                deltaY = obj_selected.line_object.nor_pt3.Y
                obj_selected.line_object.nor_pt4 = New PointF((CSng(nor_point4.X) / pictureBox.Width - deltaX) * CF, (CSng(nor_point4.Y) / pictureBox.Height - deltaY) * CF)
                obj_selected.line_object.nor_pt7 = New PointF((CSng(arr_points2(0).X) / pictureBox.Width - deltaX), (CSng(arr_points2(0).Y) / pictureBox.Height - deltaY))
                obj_selected.line_object.nor_pt8 = New PointF((CSng(arr_points2(1).X) / pictureBox.Width - deltaX), (CSng(arr_points2(1).Y) / pictureBox.Height - deltaY))
                obj_selected.line_object.draw_pt = New PointF((CSng(draw_pt.X) / pictureBox.Width - deltaX) * CF, (CSng(draw_pt.Y) / pictureBox.Height - deltaY) * CF)
                obj_selected.line_object.trans_angle = Convert.ToInt32(attri * ang_tran)
                    obj_selected.line_object.side_drag = New PointF((CSng(Side_pt.X) / pictureBox.Width - deltaX) * CF, (CSng(Side_pt.Y) / pictureBox.Height - deltaY) * CF)
                Else
                    obj_selected.line_object.nor_pt1 = New PointF(CSng(nor_point1.X) / pictureBox.Width, CSng(nor_point1.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt2 = New PointF(CSng(nor_point2.X) / pictureBox.Width, CSng(nor_point2.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt3 = New PointF(CSng(nor_point3.X) / pictureBox.Width, CSng(nor_point3.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt4 = New PointF(CSng(nor_point4.X) / pictureBox.Width, CSng(nor_point4.Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt5 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt6 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt7 = New PointF(CSng(arr_points2(0).X) / pictureBox.Width, CSng(arr_points2(0).Y) / pictureBox.Height)
                obj_selected.line_object.nor_pt8 = New PointF(CSng(arr_points2(1).X) / pictureBox.Width, CSng(arr_points2(1).Y) / pictureBox.Height)
                obj_selected.line_object.draw_pt = New PointF(CSng(draw_pt.X) / pictureBox.Width, CSng(draw_pt.Y) / pictureBox.Height)
                obj_selected.line_object.trans_angle = Convert.ToInt32(attri * ang_tran)
                obj_selected.line_object.side_drag = New PointF(CSng(Side_pt.X) / pictureBox.Width, CSng(Side_pt.Y) / pictureBox.Height)
            End If

        ElseIf obj_selected.measure_type = MeasureType.angle OrElse obj_selected.measure_type = MeasureType.angle_fixed Then
            If obj_selected.measure_type = MeasureType.angle And obj_selected.item_set < 3 Then Return
            If obj_selected.measure_type = MeasureType.angle_fixed And obj_selected.item_set < 2 Then Return

            Dim angles As Double()
            If obj_selected.measure_type = MeasureType.angle Then
                angles = CalcStartAndSweepAngle(obj_selected, start_point, middle_point, end_point, target_point)
            Else
                angles = CalcStartAndSweepAngleFixed(obj_selected, start_point, middle_point, target_point)
            End If

            Dim start_angle, sweep_angle As Double
            start_angle = angles(0)
            sweep_angle = angles(1)
            Dim ang_dirc As Integer = angles(2)
            Dim clockwise As Boolean
            If ang_dirc = 1 Then
                clockwise = True
            Else
                clockwise = False
            End If

            Dim radius = Convert.ToInt32(Math.Sqrt(Math.Pow(target_point.X - middle_point.X, 2) + Math.Pow(target_point.Y - middle_point.Y, 2)))
            Dim radius2 = If(radius - 10 > 1, radius - 10, 1)
            Dim centerpt = middle_point

            Dim first_point = CalcPositionInCircle(centerpt, radius, start_angle)
            Dim second_point = CalcPositionInCircle(centerpt, radius, start_angle + sweep_angle)

            Dim nor_point1 = CalcPositionInCircle(centerpt, radius2, start_angle)
            Dim nor_point4 = CalcPositionInCircle(centerpt, radius2, start_angle + sweep_angle)

            Dim arr_points = New Point(1) {}
            Dim arr_points2 = New Point(1) {}

            Dim dist As Integer = radius2 / 6
            dist = Math.Max(dist, 5)

            If clockwise Then
                arr_points = GetArrowPoints2(first_point, centerpt, nor_point1, dist)
                arr_points2 = GetArrowPoints2(centerpt, second_point, nor_point4, dist)
            Else
                arr_points = GetArrowPoints2(centerpt, first_point, nor_point1, dist)
                arr_points2 = GetArrowPoints2(second_point, centerpt, nor_point4, dist)
            End If

            Dim Side_pt = nor_point1
            Dim side_index = 1
            Dim draw_pt = CorrectDisplayPosition(target_point, start_point, middle_point, clockwise)
            Dim angle3 As Double = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, target_point)
            If side_drag Then
                Dim offset3 As SizeF
                Dim offset4 As SizeF

                Dim angle = CalcAngleBetweenTwoLines(start_point, middle_point, target_point)
                angle = angle * 360 / Math.PI / 2

                If angle > 0 AndAlso angle < obj_selected.angle Then
                    offset3 = GetNormalFromPointToLine(second_point, middle_point, target_point, 50)
                    Side_pt.X = nor_point4.X + CInt(offset3.Width)
                    Side_pt.Y = nor_point4.Y + CInt(offset3.Height)
                    'graph.DrawLine(graphPen, nor_point4, Side_pt);
                    angle3 = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, nor_point4)
                    draw_pt = Side_pt
                    offset4 = GetUnitVector(middle_point, nor_point4)
                    draw_pt.X += CInt(offset4.Width * 15)
                    draw_pt.Y += CInt(offset4.Height * 15)
                    side_index = 4
                Else
                    offset3 = GetNormalFromPointToLine(first_point, middle_point, target_point, 50)
                    Side_pt.X = nor_point1.X + CInt(offset3.Width)
                    Side_pt.Y = nor_point1.Y + CInt(offset3.Height)
                    'graph.DrawLine(graphPen, nor_point1, Side_pt);
                    angle3 = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, nor_point1)
                    draw_pt = Side_pt
                    offset4 = GetUnitVector(middle_point, nor_point1)
                    draw_pt.X += CInt(offset4.Width * 15)
                    draw_pt.Y += CInt(offset4.Height * 15)
                End If

            End If

            'draw text

            Dim attri = -1
            If CheckAngleDirection(New Point(middle_point.X + 10, middle_point.Y), middle_point, target_point) Then
                If angle3 > Math.PI / 2 Then
                    angle3 -= Math.PI / 2
                    attri = 1
                Else
                    angle3 = Math.PI / 2 - angle3
                    attri = -1
                End If
            Else
                If angle3 > Math.PI / 2 Then
                    angle3 -= Math.PI / 2
                    attri = -1
                Else
                    angle3 = Math.PI / 2 - angle3
                    attri = 1
                End If
            End If

            If draw_flag Then
                graph.DrawArc(graphPen, New Rectangle(middle_point.X - radius2, middle_point.Y - radius2, radius2 * 2, radius2 * 2), CSng(start_angle), CSng(sweep_angle))
                graph.DrawLine(graphPen, middle_point, first_point)
                graph.DrawLine(graphPen, middle_point, second_point)
                graph.DrawLine(graphPen, nor_point1, arr_points(0))
                graph.DrawLine(graphPen, nor_point1, arr_points(1))
                graph.DrawLine(graphPen, nor_point4, arr_points2(0))
                graph.DrawLine(graphPen, nor_point4, arr_points2(1))
                If side_index = 1 Then
                    graph.DrawLine(graphPen, nor_point1, Side_pt)
                Else
                    graph.DrawLine(graphPen, nor_point4, Side_pt)
                End If

                angle3 = angle3 * 360 / (2 * Math.PI)
                graph.RotateTransform(attri * angle3)
                Dim trans_pt = GetRotationTransform(draw_pt, attri * angle3)

                Dim length_decimal = GetDecimalNumber(Math.Abs(sweep_angle), digit, 1)
                Dim textSize As SizeF = graph.MeasureString(length_decimal.ToString(), graphFont)
                graph.DrawString(length_decimal.ToString(), graphFont, graphBrush, trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2)
            End If


            'initialize the angle object
            obj_selected.angle_object.radius = CSng(radius2) / pictureBox.Width
            'obj_selected.angle = sweep_angle;
            obj_selected.angle_object.start_pt = New PointF(CSng(first_point.X) / pictureBox.Width, CSng(first_point.Y) / pictureBox.Height)
            obj_selected.angle_object.end_pt = New PointF(CSng(second_point.X) / pictureBox.Width, CSng(second_point.Y) / pictureBox.Height)
            If obj_selected.measure_type = MeasureType.angle_fixed Then
                obj_selected.end_point = obj_selected.angle_object.end_pt
            End If
            obj_selected.angle_object.nor_pt1 = New PointF(CSng(nor_point1.X) / pictureBox.Width, CSng(nor_point1.Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt4 = New PointF(CSng(nor_point4.X) / pictureBox.Width, CSng(nor_point4.Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt2 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt3 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt5 = New PointF(CSng(arr_points2(0).X) / pictureBox.Width, CSng(arr_points2(0).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt6 = New PointF(CSng(arr_points2(1).X) / pictureBox.Width, CSng(arr_points2(1).Y) / pictureBox.Height)
            obj_selected.angle_object.start_angle = start_angle
            obj_selected.angle_object.sweep_angle = sweep_angle
            obj_selected.angle_object.draw_pt = New PointF(CSng(draw_pt.X) / pictureBox.Width, CSng(draw_pt.Y) / pictureBox.Height)
            obj_selected.angle_object.trans_angle = Convert.ToInt32(attri * angle3)
            obj_selected.angle_object.side_drag = New PointF(CSng(Side_pt.X) / pictureBox.Width, CSng(Side_pt.Y) / pictureBox.Height)
            obj_selected.angle_object.side_index = side_index

            'float[] x_set = { obj_selected.middle_point.X, obj_selected.angle_object.start_pt.X, obj_selected.angle_object.end_pt.X };
            'float[] y_set = { obj_selected.middle_point.Y, obj_selected.angle_object.start_pt.Y, obj_selected.angle_object.end_pt.Y };
            Dim x_set = {obj_selected.middle_point.X, obj_selected.start_point.X, obj_selected.end_point.X}
            Dim y_set = {obj_selected.middle_point.Y, obj_selected.start_point.Y, obj_selected.end_point.Y}
            obj_selected.left_top.X = GetMinimumInSet(x_set)
            obj_selected.left_top.Y = GetMinimumInSet(y_set)
            obj_selected.right_bottom.X = GetMaximumInSet(x_set)
            obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
        ElseIf obj_selected.measure_type = MeasureType.radius Then
            If obj_selected.item_set < 3 Then Return

            Dim A = start_point
            Dim B = middle_point
            Dim C = end_point
            Dim d_AB = Math.Sqrt(Math.Pow(B.X - A.X, 2.0R) + Math.Pow(B.Y - A.Y, 2.0R))
            Dim d_BC = Math.Sqrt(Math.Pow(B.X - C.X, 2.0R) + Math.Pow(B.Y - C.Y, 2.0R))
            Dim d_AC = Math.Sqrt(Math.Pow(C.X - A.X, 2.0R) + Math.Pow(C.Y - A.Y, 2.0R))
            If d_AB + d_BC < d_AC + 0.2R And d_AB + d_BC > d_AC - 0.2R Then
                graph.DrawLine(graphPen, A, B)
                graph.DrawLine(graphPen, B, C)
            Else
                Dim t = New Triangle(New Point3d(start_point.X, start_point.Y, 0R), New Point3d(middle_point.X, middle_point.Y, 0R), New Point3d(end_point.X, end_point.Y, 0R))
                Dim angle_a = t.Angle_A * 360.0R / Math.PI
                Dim angle_b = t.Angle_B * 360.0R / Math.PI
                Dim angle_c = t.Angle_C * 360.0R / Math.PI
                Dim circumcenterpt = t.Circumcenter
                Dim centerpt = New Point(Convert.ToInt32(circumcenterpt.X), Convert.ToInt32(circumcenterpt.Y))
                Dim radius = Convert.ToInt32(t.Circumcircle.R)
                Dim right_cenerpt = New Point(centerpt.X + radius, centerpt.Y)
                Dim M = New PointF(right_cenerpt.X / CSng(pictureBox.Width), right_cenerpt.Y / CSng(pictureBox.Height))
                Dim item = sorting_Points(obj_selected, M)
                A = New Point(item.start_point.X * pictureBox.Width, item.start_point.Y * pictureBox.Height)
                B = New Point(item.middle_point.X * pictureBox.Width, item.middle_point.Y * pictureBox.Height)
                C = New Point(item.end_point.X * pictureBox.Width, item.end_point.Y * pictureBox.Height)


                If centerpt.Y = C.Y Then
                    Return
                End If
                Dim p_t = New Triangle(New Point3d(centerpt.X, centerpt.Y, 0R), New Point3d(C.X, C.Y, 0R), New Point3d(centerpt.X + radius, centerpt.Y, 0R))
                Dim rotate_angle = Convert.ToInt32(angle_a + angle_c)
                Dim add_a = 0
                If A.Y < right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    If B.X > A.X And B.X > C.X Or B.X < A.X And B.X < C.X Or B.X < A.X And B.X > C.X Then
                        add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                    End If
                ElseIf A.Y < right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y < right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y < right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y < right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y > right_cenerpt.Y Then
                    add_a = Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                ElseIf A.Y > right_cenerpt.Y And B.Y > right_cenerpt.Y And C.Y < right_cenerpt.Y Then
                    add_a = -Convert.ToInt32(p_t.Angle_A * 180.0R / Math.PI)
                End If
                'graph.DrawArc(graphPen, new Rectangle(centerpt.X - radius, centerpt.Y - radius, radius * 2, radius * 2), add_a, rotate_angle);

                Dim basis_pt As Point = New Point(centerpt.X + 10, centerpt.Y)
                'radius between x-axis and mouse cursor
                Dim angle_mpt = CalcAngleBetweenTwoLines(basis_pt, centerpt, target_point)
                Dim angle_mpt_deg = Convert.ToInt32(angle_mpt * 360 / Math.PI / 2)

                Dim clockwise = CheckAngleDirection(basis_pt, centerpt, target_point)
                If Not clockwise Then angle_mpt_deg = -1 * angle_mpt_deg
                Dim pt_circle = CalcPositionInCircle(centerpt, radius, angle_mpt_deg)
                Dim draw_pt = CalcPositionInCircle(centerpt, radius + 15, angle_mpt_deg)

                'graph.DrawLine(graphPen, centerpt, pt_circle);

                'draw arrows
                Dim dist As Integer = radius / 5
                dist = Math.Min(Math.Max(dist, 5), 20)
                Dim arr_points = GetArrowPoints3(centerpt, pt_circle, dist)
                'graph.DrawLine(graphPen, centerpt, arr_points[0]);
                'graph.DrawLine(graphPen, centerpt, arr_points[1]);
                Dim arr_points2 = GetArrowPoints3(pt_circle, centerpt, dist)
                'graph.DrawLine(graphPen, pt_circle, arr_points2[0]);
                'graph.DrawLine(graphPen, pt_circle, arr_points2[1]);

                'draw string
                Dim attri = -1
                If clockwise Then
                    If angle_mpt > Math.PI / 2 Then
                        angle_mpt -= Math.PI / 2
                        attri = 1
                    Else
                        angle_mpt = Math.PI / 2 - angle_mpt
                        attri = -1
                    End If
                Else
                    If angle_mpt > Math.PI / 2 Then
                        angle_mpt -= Math.PI / 2
                        attri = -1
                    Else
                        angle_mpt = Math.PI / 2 - angle_mpt
                        attri = 1
                    End If
                End If


                If draw_flag Then
                    graph.DrawArc(graphPen, New Rectangle(centerpt.X - radius, centerpt.Y - radius, radius * 2, radius * 2), add_a, rotate_angle)

                    graph.DrawLine(graphPen, centerpt, pt_circle)
                    graph.DrawLine(graphPen, centerpt, arr_points(0))
                    graph.DrawLine(graphPen, centerpt, arr_points(1))
                    graph.DrawLine(graphPen, pt_circle, arr_points2(0))
                    graph.DrawLine(graphPen, pt_circle, arr_points2(1))

                    angle_mpt_deg = Convert.ToInt32(angle_mpt * 360 / (2 * Math.PI))
                    graph.RotateTransform(attri * angle_mpt_deg)
                    Dim trans_pt = GetRotationTransform(draw_pt, attri * angle_mpt_deg)

                    Dim length_decimal = GetDecimalNumber(radius, digit, CF)
                    Dim textSize As SizeF = graph.MeasureString(length_decimal.ToString(), graphFont)
                    graph.DrawString(length_decimal.ToString(), graphFont, graphBrush, trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2)
                End If


                'initialize the radius object
                obj_selected.radius_object.center_pt = New PointF(CSng(centerpt.X) / pictureBox.Width, CSng(centerpt.Y) / pictureBox.Height)
                obj_selected.radius_object.circle_pt = New PointF(CSng(pt_circle.X) / pictureBox.Width, CSng(pt_circle.Y) / pictureBox.Height)
                obj_selected.radius_object.arr_pt1 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
                obj_selected.radius_object.arr_pt2 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
                obj_selected.radius_object.arr_pt3 = New PointF(CSng(arr_points2(0).X) / pictureBox.Width, CSng(arr_points2(0).Y) / pictureBox.Height)
                obj_selected.radius_object.arr_pt4 = New PointF(CSng(arr_points2(1).X) / pictureBox.Width, CSng(arr_points2(1).Y) / pictureBox.Height)
                obj_selected.radius_object.draw_pt = New PointF(CSng(draw_pt.X) / pictureBox.Width, CSng(draw_pt.Y) / pictureBox.Height)
                obj_selected.radius_object.trans_angle = attri * angle_mpt_deg
            End If
        ElseIf obj_selected.measure_type = MeasureType.circle_fixed Then
            If obj_selected.item_set < 1 Then Return

            Dim centerpt = start_point
            Dim basis_pt As Point = New Point(centerpt.X + 10, centerpt.Y)
            Dim angle_mpt = CalcAngleBetweenTwoLines(basis_pt, centerpt, target_point)
            Dim angle_mpt_deg = Convert.ToInt32(angle_mpt * 360 / Math.PI / 2)

            Dim clockwise = CheckAngleDirection(basis_pt, centerpt, target_point)
            If Not clockwise Then angle_mpt_deg = -1 * angle_mpt_deg
            Dim radius = CInt(obj_selected.radius / CF * pictureBox.Width)
            Dim pt_circle = CalcPositionInCircle(centerpt, radius, angle_mpt_deg)
            Dim draw_pt = CalcPositionInCircle(centerpt, radius + 15, angle_mpt_deg)

            Dim dist As Integer = radius / 5
            dist = Math.Min(Math.Max(dist, 5), 20)
            Dim arr_points = GetArrowPoints3(centerpt, pt_circle, dist)
            Dim arr_points2 = GetArrowPoints3(pt_circle, centerpt, dist)

            Dim attri = -1
            If clockwise Then
                If angle_mpt > Math.PI / 2 Then
                    angle_mpt -= Math.PI / 2
                    attri = 1
                Else
                    angle_mpt = Math.PI / 2 - angle_mpt
                    attri = -1
                End If
            Else
                If angle_mpt > Math.PI / 2 Then
                    angle_mpt -= Math.PI / 2
                    attri = -1
                Else
                    angle_mpt = Math.PI / 2 - angle_mpt
                    attri = 1
                End If
            End If


            If draw_flag Then
                graph.DrawArc(graphPen, New Rectangle(centerpt.X - radius, centerpt.Y - radius, radius * 2, radius * 2), 0, 360)

                graph.DrawLine(graphPen, centerpt, pt_circle)
                graph.DrawLine(graphPen, centerpt, arr_points(0))
                graph.DrawLine(graphPen, centerpt, arr_points(1))
                graph.DrawLine(graphPen, pt_circle, arr_points2(0))
                graph.DrawLine(graphPen, pt_circle, arr_points2(1))

                angle_mpt_deg = Convert.ToInt32(angle_mpt * 360 / (2 * Math.PI))
                graph.RotateTransform(attri * angle_mpt_deg)
                Dim trans_pt = GetRotationTransform(draw_pt, attri * angle_mpt_deg)

                Dim length_decimal = GetDecimalNumber(radius, digit, CF)
                Dim textSize As SizeF = graph.MeasureString(length_decimal.ToString(), graphFont)
                graph.DrawString(length_decimal.ToString(), graphFont, graphBrush, trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2)
            End If


            'initialize the radius object
            obj_selected.radius_object.center_pt = New PointF(CSng(centerpt.X) / pictureBox.Width, CSng(centerpt.Y) / pictureBox.Height)
            Dim deltaX = obj_selected.radius_object.center_pt.X
            Dim deltaY = obj_selected.radius_object.center_pt.Y
            obj_selected.radius_object.circle_pt = New PointF((CSng(pt_circle.X) / pictureBox.Width - deltaX) * CF, (CSng(pt_circle.Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.arr_pt1 = New PointF((CSng(arr_points(0).X) / pictureBox.Width - deltaX) * CF, (CSng(arr_points(0).Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.arr_pt2 = New PointF((CSng(arr_points(1).X) / pictureBox.Width - deltaX) * CF, (CSng(arr_points(1).Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.arr_pt3 = New PointF((CSng(arr_points2(0).X) / pictureBox.Width - deltaX) * CF, (CSng(arr_points2(0).Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.arr_pt4 = New PointF((CSng(arr_points2(1).X) / pictureBox.Width - deltaX) * CF, (CSng(arr_points2(1).Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.draw_pt = New PointF((CSng(draw_pt.X) / pictureBox.Width - deltaX) * CF, (CSng(draw_pt.Y) / pictureBox.Height - deltaY) * CF)
            obj_selected.radius_object.trans_angle = attri * angle_mpt_deg

        ElseIf obj_selected.measure_type = MeasureType.annotation Then
            If obj_selected.item_set < 1 Then Return
            graph.DrawString(obj_selected.annotation, graphFont, graphBrush, target_point.X, target_point.Y)
            Dim textSize = graph.MeasureString(obj_selected.annotation, graphFont)

            Dim left_top = target_point
            Dim right_top As Point = New Point(target_point.X + CInt(textSize.Width), target_point.Y)
            Dim left_bottom As Point = New Point(target_point.X, target_point.Y + CInt(textSize.Height))
            Dim right_bottom As Point = New Point(target_point.X + CInt(textSize.Width), target_point.Y + CInt(textSize.Height))
            graph.DrawLine(graphPen, left_top, right_top)
            graph.DrawLine(graphPen, right_top, right_bottom)
            graph.DrawLine(graphPen, right_bottom, left_bottom)
            graph.DrawLine(graphPen, left_bottom, left_top)
            Dim pt_array = {left_top, right_top, left_bottom, right_bottom}
            Dim near_pt = GetShortestPath(start_point, pt_array)
            graph.DrawLine(graphPen, start_point, near_pt)

            'initialize the anno object
            obj_selected.anno_object.size.Width = CInt(textSize.Width)
            obj_selected.anno_object.size.Height = CInt(textSize.Height)
            obj_selected.left_top = New PointF(CSng(left_top.X) / pictureBox.Width, CSng(left_top.Y) / pictureBox.Height)
            obj_selected.right_bottom = New PointF(CSng(right_bottom.X) / pictureBox.Width, CSng(right_bottom.Y) / pictureBox.Height)

            obj_selected.anno_object.line_pt = New PointF(CSng(near_pt.X) / pictureBox.Width, CSng(near_pt.Y) / pictureBox.Height)
        ElseIf obj_selected.measure_type = MeasureType.angle_far Then
            If obj_selected.item_set < 4 Then Return

            middle_point = inter_pt

            Dim angles = CalcStartAndSweepAngle(obj_selected, start_point, middle_point, end_point, target_point)
            Dim start_angle, sweep_angle As Double
            start_angle = angles(0)
            sweep_angle = angles(1)
            Dim ang_dirc As Integer = angles(2)
            Dim clockwise As Boolean
            If ang_dirc = 1 Then
                clockwise = True
            Else
                clockwise = False
            End If

            Dim radius = Convert.ToInt32(Math.Sqrt(Math.Pow(target_point.X - middle_point.X, 2) + Math.Pow(target_point.Y - middle_point.Y, 2)))
            Dim radius2 = If(radius - 10 > 1, radius - 10, 1)
            Dim centerpt = middle_point

            'graph.DrawArc(graphPen, new Rectangle(centerpt.X - radius2, centerpt.Y - radius2, radius2 * 2, radius2 * 2),
            '                            (float)start_angle, (float)sweep_angle);
            'draw arrows
            Dim first_point = CalcPositionInCircle(centerpt, radius, start_angle)
            Dim second_point = CalcPositionInCircle(centerpt, radius, start_angle + sweep_angle)
            'graph.DrawLine(graphPen, centerpt, first_point);
            'graph.DrawLine(graphPen, centerpt, second_point);

            Dim nor_point1 = CalcPositionInCircle(centerpt, radius2, start_angle)
            Dim nor_point4 = CalcPositionInCircle(centerpt, radius2, start_angle + sweep_angle)

            Dim arr_points = New Point(1) {}
            Dim arr_points2 = New Point(1) {}
            If clockwise Then
                arr_points = GetArrowPoints2(first_point, centerpt, nor_point1, 10)
                arr_points2 = GetArrowPoints2(centerpt, second_point, nor_point4, 10)
            Else
                arr_points = GetArrowPoints2(centerpt, first_point, nor_point1, 10)
                arr_points2 = GetArrowPoints2(second_point, centerpt, nor_point4, 10)
            End If

            graph.DrawLine(graphPen, nor_point1, arr_points(0))
            graph.DrawLine(graphPen, nor_point1, arr_points(1))

            graph.DrawLine(graphPen, nor_point4, arr_points2(0))
            graph.DrawLine(graphPen, nor_point4, arr_points2(1))

            'side dragging
            Dim Side_pt = nor_point1
            Dim side_index = 1
            Dim draw_pt = CorrectDisplayPosition(target_point, start_point, middle_point, clockwise)
            Dim angle3 As Double = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, target_point)
            If side_drag Then
                Dim offset3 As SizeF
                Dim offset4 As SizeF

                Dim angle = CalcAngleBetweenTwoLines(start_point, middle_point, target_point)
                angle = angle * 360 / Math.PI / 2

                If angle > 0 AndAlso angle < obj_selected.angle Then
                    offset3 = GetNormalFromPointToLine(second_point, middle_point, target_point, 50)
                    Side_pt.X = nor_point4.X + CInt(offset3.Width)
                    Side_pt.Y = nor_point4.Y + CInt(offset3.Height)
                    graph.DrawLine(graphPen, nor_point4, Side_pt)
                    angle3 = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, nor_point4)
                    draw_pt = Side_pt
                    offset4 = GetUnitVector(middle_point, nor_point4)
                    draw_pt.X += CInt(offset4.Width * 10)
                    draw_pt.Y += CInt(offset4.Height * 10)
                    side_index = 4
                Else
                    offset3 = GetNormalFromPointToLine(first_point, middle_point, target_point, 50)
                    Side_pt.X = nor_point1.X + CInt(offset3.Width)
                    Side_pt.Y = nor_point1.Y + CInt(offset3.Height)
                    graph.DrawLine(graphPen, nor_point1, Side_pt)
                    angle3 = CalcAngleBetweenTwoLines(New Point(middle_point.X + 10, middle_point.Y), middle_point, nor_point1)
                    draw_pt = Side_pt
                    offset4 = GetUnitVector(middle_point, nor_point1)
                    draw_pt.X += CInt(offset4.Width * 10)
                    draw_pt.Y += CInt(offset4.Height * 10)
                End If

            End If

            'draw text

            Dim attri = -1
            If CheckAngleDirection(New Point(middle_point.X + 10, middle_point.Y), middle_point, target_point) Then
                If angle3 > Math.PI / 2 Then
                    angle3 -= Math.PI / 2
                    attri = 1
                Else
                    angle3 = Math.PI / 2 - angle3
                    attri = -1
                End If
            Else
                If angle3 > Math.PI / 2 Then
                    angle3 -= Math.PI / 2
                    attri = -1
                Else
                    angle3 = Math.PI / 2 - angle3
                    attri = 1
                End If
            End If

            If draw_flag Then
                graph.DrawArc(graphPen, New Rectangle(middle_point.X - radius2, middle_point.Y - radius2, radius2 * 2, radius2 * 2), CSng(start_angle), CSng(sweep_angle))

                graph.DrawLine(graphPen, nor_point1, arr_points(0))
                graph.DrawLine(graphPen, nor_point1, arr_points(1))
                graph.DrawLine(graphPen, nor_point4, arr_points2(0))
                graph.DrawLine(graphPen, nor_point4, arr_points2(1))
                If side_index = 1 Then
                    graph.DrawLine(graphPen, nor_point1, Side_pt)
                Else
                    graph.DrawLine(graphPen, nor_point4, Side_pt)
                End If

                angle3 = angle3 * 360 / (2 * Math.PI)
                graph.RotateTransform(attri * angle3)
                Dim trans_pt = GetRotationTransform(draw_pt, attri * angle3)

                Dim length_decimal = GetDecimalNumber(Math.Abs(sweep_angle), digit, 1)
                Dim textSize As SizeF = graph.MeasureString(length_decimal.ToString(), graphFont)
                graph.DrawString(length_decimal.ToString(), graphFont, graphBrush, trans_pt.X - textSize.Width / 2, trans_pt.Y - textSize.Height / 2)
            End If

            'initialize the angle object
            obj_selected.angle_object.radius = CSng(radius2) / pictureBox.Width
            'obj_selected.angle = sweep_angle;
            obj_selected.angle_object.start_pt = New PointF(CSng(first_point.X) / pictureBox.Width, CSng(first_point.Y) / pictureBox.Height)
            obj_selected.angle_object.end_pt = New PointF(CSng(second_point.X) / pictureBox.Width, CSng(second_point.Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt1 = New PointF(CSng(nor_point1.X) / pictureBox.Width, CSng(nor_point1.Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt4 = New PointF(CSng(nor_point4.X) / pictureBox.Width, CSng(nor_point4.Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt2 = New PointF(CSng(arr_points(0).X) / pictureBox.Width, CSng(arr_points(0).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt3 = New PointF(CSng(arr_points(1).X) / pictureBox.Width, CSng(arr_points(1).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt5 = New PointF(CSng(arr_points2(0).X) / pictureBox.Width, CSng(arr_points2(0).Y) / pictureBox.Height)
            obj_selected.angle_object.nor_pt6 = New PointF(CSng(arr_points2(1).X) / pictureBox.Width, CSng(arr_points2(1).Y) / pictureBox.Height)
            obj_selected.angle_object.start_angle = start_angle
            obj_selected.angle_object.sweep_angle = sweep_angle
            obj_selected.angle_object.draw_pt = New PointF(CSng(draw_pt.X) / pictureBox.Width, CSng(draw_pt.Y) / pictureBox.Height)
            obj_selected.angle_object.trans_angle = Convert.ToInt32(attri * angle3)
            obj_selected.angle_object.side_drag = New PointF(CSng(Side_pt.X) / pictureBox.Width, CSng(Side_pt.Y) / pictureBox.Height)
            obj_selected.angle_object.side_index = side_index

            Dim x_set = {obj_selected.start_point.X, obj_selected.middle_point.X, obj_selected.end_point.X, obj_selected.last_point.X}
            Dim y_set = {obj_selected.start_point.Y, obj_selected.middle_point.Y, obj_selected.end_point.Y, obj_selected.last_point.Y}
            obj_selected.left_top.X = GetMinimumInSet(x_set)
            obj_selected.left_top.Y = GetMinimumInSet(y_set)
            obj_selected.right_bottom.X = GetMaximumInSet(x_set)
            obj_selected.right_bottom.Y = GetMaximumInSet(y_set)
        End If

        graph.Dispose()
        graphPen.Dispose()
    End Sub

    ''' <summary>
    ''' draw dotted rectangle.
    ''' </summary>
    ''' <paramname="pictureBox">The pictureBox control in which you want to draw object list.</param>
    ''' <paramname="FirstPtOfEdge">The left top corner of selected region.</param>
    ''' <paramname="SecondPtOfEdge">The right bottom corner of selected region.</param>
    <Extension()>
    Public Sub DrawRectangle(ByVal pictureBox As PictureBox, ByVal FirstPtOfEdge As Point, ByVal SecondPtOfEdge As Point)
        Dim graph As Graphics = pictureBox.CreateGraphics()
        Dim graphPen As Pen = New Pen(Color.Black, 1)

        graph.DrawRectangle(graphPen, New Rectangle(FirstPtOfEdge.X, FirstPtOfEdge.Y, SecondPtOfEdge.X - FirstPtOfEdge.X, SecondPtOfEdge.Y - FirstPtOfEdge.Y))

        graphPen.Dispose()
        graph.Dispose()
    End Sub
#End Region

#Region "DataGrid Methods"
    ''' <summary>
    ''' set the items of combobox column of datagridview
    ''' </summary>
    ''' <paramname="name">The string which is display.</param>
    ''' <paramname="name_list">The list of strings which are included in combobox item.</param>
    Public Function SetComboItemContent(ByVal name As String, ByVal name_list As List(Of String)) As DataGridViewComboBoxCell
        Dim cell As DataGridViewComboBoxCell = New DataGridViewComboBoxCell()
        Dim str_array As String() = New String(name_list.Count) {}
        For i = 0 To str_array.Length() - 1
            If i = 0 Then
                str_array(i) = name
                Continue For
            End If
            str_array(i) = name_list(i - 1)
        Next
        cell.Items.AddRange(str_array)
        cell.Value = cell.Items(0)
        cell.ReadOnly = False
        Return cell
    End Function


    ''' <summary>
    ''' Loads all the data of objects.
    ''' </summary>
    ''' <paramname="listView">The list view to load objects on.</param>
    ''' <paramname="circles">The list of objects which you are going to load.</param>
    ''' <paramname="CF">The factor of measurig scale.</param>
    ''' <paramname="digit">The digit of decimal numbers.</param>
    ''' <paramname="unit">The unit in length.</param>
    ''' <paramname="name_list">The list of names which will included in combobox item.</param>
    <Extension()>
    Public Sub LoadObjectList(ByVal listView As DataGridView, ByVal object_list As List(Of MeasureObject), ByVal CF As Double, ByVal digit As Integer, ByVal unit As String, ByVal name_list As List(Of String))
        listView.Rows.Clear()
        If object_list.Count > 0 Then
            Dim i = 0
            Dim length As Double
            For Each item In object_list
                Dim str_item = New String(5) {}
                str_item(0) = item.name
                str_item(1) = ""
                str_item(2) = ""
                str_item(3) = ""
                str_item(4) = ""
                str_item(5) = item.remarks

                Select Case item.measure_type
                    Case MeasureType.line_align
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_horizontal
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_vertical
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_para
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.pt_line
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.angle
                        length = GetDecimalNumber(item.angle_object.sweep_angle, digit, 1)
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                    Case MeasureType.angle_far
                        length = GetDecimalNumber(item.angle_object.sweep_angle, digit, 1)
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                    Case MeasureType.radius
                        length = GetDecimalNumber(item.radius, digit, CF)
                        str_item(3) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.annotation
                    Case MeasureType.draw_line
                        str_item(0) = "line"
                    Case MeasureType.measure_scale
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.C_MinMax
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_fixed
                        length = item.scale_object.length
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.circle_fixed
                        length = GetDecimalNumber(item.scale_object.length, digit, 1)
                        str_item(3) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.angle_fixed
                        length = item.scale_object.length
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                End Select

                listView.Rows.Add(str_item)
                listView.Rows(i).Cells(0) = SetComboItemContent(str_item(0), name_list)
                i += 1
            Next
        End If

    End Sub
#End Region

#Region "ListView Methods"

    ''' <summary>
    ''' Loads all the data of objects.
    ''' </summary>
    ''' <paramname="listView">The list view to load objects on.</param>
    ''' <paramname="circles">The list of objects which you are going to load.</param>
    ''' <paramname="CF">The factor of measurig scale.</param>
    ''' <paramname="digit">The digit of decimal numbers.</param>
    ''' <paramname="unit">The unit in length.</param>
    <Extension()>
    Public Sub LoadObjectList(ByVal listView As ListView, ByVal object_list As List(Of MeasureObject), ByVal CF As Double, ByVal digit As Integer, ByVal unit As String)
        listView.Items.Clear()
        If object_list.Count > 0 Then
            Dim i = 1
            Dim length As Double
            For Each item In object_list
                Dim str_item = New String(5) {}
                str_item(0) = item.name
                str_item(1) = ""
                str_item(2) = ""
                str_item(3) = ""
                str_item(4) = ""
                str_item(5) = item.remarks

                Select Case item.measure_type
                    Case MeasureType.line_align
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_horizontal
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_vertical
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_para
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.pt_line
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.angle
                        length = GetDecimalNumber(item.angle_object.sweep_angle, digit, 1)
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                    Case MeasureType.angle_far
                        length = GetDecimalNumber(item.angle_object.sweep_angle, digit, 1)
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                    Case MeasureType.radius
                        length = GetDecimalNumber(item.radius, digit, CF)
                        str_item(3) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.annotation
                    Case MeasureType.draw_line
                        str_item(0) = "line"
                    Case MeasureType.measure_scale
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.C_MinMax
                        length = GetDecimalNumber(item.length, digit, CF)
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.line_fixed
                        length = item.scale_object.length
                        str_item(1) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.circle_fixed
                        length = GetDecimalNumber(item.scale_object.length, digit, 1)
                        str_item(3) = length.ToString()
                        str_item(4) = unit
                    Case MeasureType.angle_fixed
                        length = item.scale_object.length
                        str_item(2) = length.ToString()
                        str_item(4) = "degree"
                End Select

                Dim listViewItem = New ListViewItem(str_item)
                listView.Items.Add(listViewItem)
                i += 1
            Next
        End If

    End Sub


#End Region

#Region "HScrollBar Methods"
    ''' <summary>
    ''' Display the HScrollBar value to the Label.
    ''' </summary>
    ''' <paramname="label">The label you want to display the value of HScrollBar.</param>
    ''' <paramname="hScrollBar">The hScrollBar whose value is changed by user.</param>

    <Extension()>
    Public Sub DisplayDataToLabel(ByVal label As Label, ByVal hScrollBar As HScrollBar)
        label.Text = "" & hScrollBar.Value
    End Sub
#End Region
End Module

