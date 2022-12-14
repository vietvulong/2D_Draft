Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Interop
Imports AForge.Video
Imports AForge.Video.DirectShow
Imports Emgu.CV
Imports Emgu.CV.Ocl
Imports Color = System.Drawing.Color
Imports ComboBox = System.Windows.Forms.ComboBox
Imports Font = System.Drawing.Font
Imports TextBox = System.Windows.Forms.TextBox

'enum for specify the measuring types
Public Enum MeasureType
    init_state = -1
    line_align = 0
    line_horizontal = 1
    line_vertical = 2
    angle = 3
    radius = 4
    annotation = 5
    angle_far = 6
    line_para = 7
    draw_line = 8
    pt_line = 9
    measure_scale = 10
    circle_fixed = 11
    line_fixed = 12
    angle_fixed = 13
    C_Line = 14
    C_Poly = 15
    C_Point = 16
    C_Curve = 17
    C_CuPoly = 18
    C_Sel = 19
    C_MinMax = 20
    Curves = 21
End Enum

'structure for drawing line
Public Structure LineStyle
    Public line_style As String
    Public line_width As Integer
    Public line_color As Color
    Public Sub New(ByVal width As Integer)
        line_style = "dotted"
        line_width = width
        line_color = Color.Black
    End Sub
End Structure

'structure for text font
Public Structure FontInfor
    Public font_color As Color
    Public text_font As Font
    Public Sub New(ByVal height As Integer)
        font_color = Color.Black
        text_font = New Font("Arial", height, FontStyle.Regular)
    End Sub
End Structure

'structure for line_align, line_horizon, line_vertical, line_para, draw_line, pt_line
Public Structure LineObject
    Public nor_pt1 As PointF        'connect with start point
    Public nor_pt2 As PointF        'connect with end point    
    Public nor_pt3 As PointF        'connect with nor_pt4
    Public nor_pt4 As PointF
    Public nor_pt5 As PointF        'connect with nor_pt3
    Public nor_pt6 As PointF        'connect with nor_pt3
    Public nor_pt7 As PointF        'connect with nor_pt4
    Public nor_pt8 As PointF        'connect with nor_pt4
    Public draw_pt As PointF        'point for drawing line
    Public trans_angle As Integer   'angle between measuring line and X-axis
    Public side_drag As PointF      'flag for side drawing
End Structure

'structure for angle and angle_far
Public Structure AngleObject
    Public start_angle As Double        'angle between first line of arc and X-axis
    Public sweep_angle As Double        'angle between first line and second line of arc
    Public radius As Double             'radius of angle
    Public start_pt As PointF           'start point of angle
    Public end_pt As PointF             'end point of angle
    Public nor_pt1 As PointF            'included in first line of angle
    Public nor_pt2 As PointF            'connect to nor_pt1
    Public nor_pt3 As PointF            'connect to nor_pt1
    Public nor_pt4 As PointF            'included in second line of angle
    Public nor_pt5 As PointF            'connect to nor_pt4
    Public nor_pt6 As PointF            'connect to nor_pt4
    Public draw_pt As PointF            'point for drawing text
    Public trans_angle As Integer       'angle between text and X-axis
    Public side_drag As PointF          'flag for side drawing
    Public side_index As Integer        'index of nor_pt for side drawing
End Structure

'Structure for radius
Public Structure RadiusObject
    Public center_pt As PointF      'center point of arc
    Public circle_pt As PointF      'point in circle which is used for drawing radius
    Public draw_pt As PointF        'point for drawing text
    Public arr_pt1 As PointF        'used for drawing arrows
    Public arr_pt2 As PointF        'used for drawing arrows
    Public arr_pt3 As PointF        'used for drawing arrows
    Public arr_pt4 As PointF        'used for drawing arrows
    Public trans_angle As Integer   'angle between text and X-axis
    Public radius As Double         'radius of arc
    Public side_drag As PointF      'flag for side drawing

End Structure

'structure for annotation
Public Structure AnnoObject
    Public line_pt As PointF        'point used for drawing line
    Public size As Size             'size of anno object
End Structure

'structure for measure_scale
Public Structure ScaleObject
    Public style As String          'flag for horizontal or vertical
    Public length As Double         'the length of scale
    Public start_pt As PointF       'start point of scale
    Public end_pt As PointF         'end point of scale
    Public nor_pt1 As PointF        'used for drawing bounds
    Public nor_pt2 As PointF        'used for drawing bounds
    Public nor_pt3 As PointF        'used for drawing bounds
    Public nor_pt4 As PointF        'used for drawing bounds
    Public trans_angle As Integer   'angle between measure object and X-axis

End Structure

Public Structure C_LineObject
    Public FirstPointOfLine As PointF
    Public SecndPointOfLine As PointF
    Public LDrawPos As PointF

    Public Sub Refresh()
        FirstPointOfLine.X = 0
        FirstPointOfLine.Y = 0
        SecndPointOfLine.X = 0
        SecndPointOfLine.Y = 0
        LDrawPos.X = 0
        LDrawPos.Y = 0
    End Sub
End Structure

Public Structure C_PointObject
    Public PointPoint As PointF
    Public PDrawPos As PointF

    Public Sub Refresh()
        PointPoint.X = 0
        PointPoint.Y = 0
        PDrawPos.X = 0
        PDrawPos.Y = 0
    End Sub
End Structure

'structure for drawing objects
Public Structure MeasureObject
    Public start_point As PointF        'start point of object
    Public middle_point As PointF       'middle point of object
    Public end_point As PointF          'end point of object
    Public last_point As PointF         'optional, used for angle_far, the fourth point
    Public common_point As PointF       'optional, used for angle_far, the common point of first line and second line of angle

    Public draw_point As PointF         'point for drawing text
    Public measure_type As Integer      'measuring type of current object
    Public intialized As Boolean        'flag for specifying that object is initialized or not
    Public item_set As Integer          'the limit of points
    Public length As Double             'the length of object
    Public angle As Double              'optional, used for angle, angle_far
    Public radius As Double             'optional, used for radius
    Public annotation As String         'optional, used for annotation
    Public line_object As LineObject
    Public angle_object As AngleObject
    Public radius_object As RadiusObject
    Public anno_object As AnnoObject
    Public obj_num As Integer           'the order of current object
    Public line_infor As LineStyle      'information of drawing line
    Public scale_object As ScaleObject
    Public font_infor As FontInfor      'information of text font

    Public left_top As PointF           'the left top cornor of object
    Public right_bottom As PointF       'the right bottom cornor of object

    Public name As String               'the name of object
    Public remarks As String            'remarks of object

    Public curve_object As CurveObject
    Public dot_flag As Boolean
    Public Sub Refresh()
        start_point.X = 0
        start_point.Y = 0
        middle_point.X = 0
        middle_point.Y = 0
        end_point.X = 0
        end_point.Y = 0
        draw_point.X = 0
        draw_point.Y = 0
        last_point.X = 0
        last_point.Y = 0
        common_point.X = 0
        common_point.Y = 0

        length = 0
        angle = 0
        radius = 0
        annotation = ""
        intialized = False
        item_set = 0

        name = ""
        remarks = ""
        dot_flag = False

        measure_type = MeasureType.init_state
    End Sub
End Structure

Public Enum SegType
    circle = 0
    intersection = 1
    phaseSegmentation = 2
    BlobSegment = 3
End Enum


Public Structure SegObject
    Public measureType As Integer
    Public circleObj As CircleObj
    Public sectObj As InterSectionObj
    Public phaseSegObj As PhaseSeg
    Public BlobSegObj As BlobSeg

    Public Sub Refresh()
        circleObj.Refresh()
        sectObj.Refresh()
        phaseSegObj.Refresh()
        BlobSegObj.Refresh()
    End Sub
End Structure
Public Class Main_Form
    Public origin_image As List(Of Mat) = New List(Of Mat)()           'original image
    Public resized_image As List(Of Mat) = New List(Of Mat)()          'the image which is resized to fit the picturebox control
    Public current_image As List(Of Mat) = New List(Of Mat)()          'the image which is currently used
    Private initial_ratio As Single() = New Single(24) {}               'the ratio of resized_image and original image
    Private zoom_factor As Double() = New Double(24) {}                 'the zooming factor
    Private cur_measure_type As Integer                                 'current measurement type
    Private cur_measure_type_prev As Integer                            'backup of current measurement type
    Private cur_obj_num As Integer() = New Integer(24) {}               'the number of current object
    Private obj_selected As MeasureObject = New MeasureObject()         'current measurement object
    Private obj_selected2 As MeasureObject = New MeasureObject()         'current measurement object
    Private object_list As List(Of List(Of MeasureObject)) = New List(Of List(Of MeasureObject))()        'the list of measurement objects
    Private ID_MY_TEXTBOX As TextBox() = New TextBox(24) {}             'textbox for editing annotation
    Private left_top As Point = New Point()                             'the position left top cornor of picture control in panel
    Private scroll_pos As Point = New Point()                           'the position of scroll bar
    Private anno_num As Integer                                         'the number of annotation object in the list
    Private graphFont As Font                                           'the font for text
    Private undo_num As Integer = 0                                     'count number of undo clicked and reset
    Private graphPen As Pen = New Pen(Color.Black, 1)                   'pen for drawing objects
    Private graphPen_line As Pen = New Pen(Color.Black, 1)              'pen for drawing lines
    Private dashValues As Single() = {5, 2}                             'format dash style of line
    Private line_infor As LineStyle = New LineStyle(1)                  'include the information of style, width, color ...
    Private side_drag As Boolean = False                                'flag of side drawing
    Public show_legend As Boolean = False                              'flag of show legend
    Private scale_style As String = "horizontal"                        'the style of measuring scale horizontal or vertical
    Private scale_value As Integer = 0                                  'the value of measuring scale
    Private scale_unit As String = "cm"                                 'unit of measuring scale may be cm, mm, ...
    Private ID_TAG_PAGE As TabPage() = New TabPage(24) {}               'tab includes panel
    Private ID_PANEL As Panel() = New Panel(24) {}                      'panel includes picturebox
    Public ID_PICTURE_BOX As PictureBox() = New PictureBox(24) {}      'picturebox for drawing objects
    Public tab_index As Integer = 0                                    'selected index of tab control
    Private CF As Double = 1.0                                          'the ratio of per pixel by per unit
    Private digit As Integer                                            'The digit of decimal numbers
    Private font_infor As FontInfor = New FontInfor(10)                 'include the information font and color
    Private brightness As Integer() = New Integer(24) {}                'brightness of current image
    Private contrast As Integer() = New Integer(24) {}                  'contrast of current image
    Private gamma As Integer() = New Integer(24) {}                     'gamma of current image
    Private sel_index As Integer = -1                                   'selected index for object
    Public m_cur_drag As PointF = New PointF()                         'the position of mouse cursor
    Private redraw_flag As Boolean                                      'flag for redrawing objects
    Private sel_pt_index As Integer = -1                                'selected index of a point of object
    Private tag_page_flag As Boolean() = New Boolean(24) {}             'specify that target tag page is opened
    Private img_import_flag As Boolean() = New Boolean(24) {}           'specify that you can import image in target tag
    Private name_list As List(Of String) = New List(Of String)          'specify the list of item names
    Private CF_list As List(Of String) = New List(Of String)            'specify the names of CF
    Private CF_num As List(Of Double) = New List(Of Double)             'specify the values of CF
    Private menu_click As Boolean = False                               'specify whether the menu item is clicked

    'member variable for webcam
    Private videoDevices As FilterInfoCollection                        'usable video devices
    Private videoDevice As VideoCaptureDevice                           'video device currently used 
    Private snapshotCapabilities As VideoCapabilities()
    Private ReadOnly listCamera As ArrayList = New ArrayList()
    Private Shared needSnapshot As Boolean = False
    Private newImage As Bitmap = Nothing                                'used for capturing frame of webcam
    Private ReadOnly _devicename As String = "MultitekHDCam"            'device name
    'Private ReadOnly _devicename As String = "USB Camera"
    'Private ReadOnly _devicename As String = "Lenovo FHD Webcam"
    Private ReadOnly photoList As New System.Windows.Forms.ImageList    'list of captured images
    Private file_counter As Integer = 0                                 'the count of captured images
    Private camera_state As Boolean = False                             'the state of camera is opened or not
    Public imagepath As String = ""                                     'path of folder storing captured images
    Private flag As Boolean = False                                     'flag for live image

    'member variable for keygen
    Dim licState As licState                                            'the state of this program is licensed or not
    Public licModel As New licensInfoModel
    Dim licGen As New LicGen
    Dim licpath As String = "active.lic"                                'the path of license file
    Private path As String

    'member variable for setting.ini
    Private ini_path As String = "C:\Users\Public\Documents\setting.ini"    'the path of setting.ini
    Private ini As IniFile

    'member variable for Curves
    Private exe_path As String = "WindowsApp1.exe"
    Private ToCurveImg_path As String = "C:\Users\Public\Documents\ToCurve.bmp"    'the path of image for Curves
    Private ReturnedImg_path As String = "C:\Users\Public\Documents\To2D.bmp"       'the path of image returned from Curves
    Private ReturnedTxt_path As String = "C:\Users\Public\Documents\To2D.txt"    'the path of text file contains data-table

    Public PolyDrawEndFlag As Boolean          'flag specifies that end point of polygen is drawed
    Public CuPolyDrawEndFlag As Boolean        'flag specifies that end point of Curve&polygen is drawed
    Public dumyPoint As Point                  'temp point 

    Public CReadySelectFalg As Boolean                                 'flag specifies whether curve&poly object is ready to select or not. when mouse cursor is in range of object, this becomes true, otherwise this becomes false
    Public CReadySelectArrayIndx As Integer                            'the candidate index of curve object for selection
    Public CRealSelectArrayIndx As Integer                             'the real index of curve object which is selected
    Public CReadySelectArrayIndx_L As Integer                          'the candidate index of label of curve object for selection
    Public CRealSelectArrayIndx_L As Integer                           'the real index of label of curve object which is selected

    Public CuPolyReadySelectArrayIndx As Integer                       'the candidate index of curve&poly object for selection
    Public CuPolyRealSelectArrayIndx As Integer                        'the real index of curve&poly object which is selected
    Public CuPolyReadySelectArrayIndx_L As Integer                     'the candidate index of label of curve&poly object for selection
    Public CuPolyRealSelectArrayIndx_L As Integer                      'the real index of label of curve&poly object which is selected

    Public PolyReadySelectArrayIndx As Integer                         'the candidate index of polygen object for selection
    Public PolyRealSelectArrayIndx As Integer                          'the real index of polygen object which is selected
    Public PolyReadySelectArrayIndx_L As Integer                       'the candidate index of label of polygen object for selection
    Public PolyRealSelectArrayIndx_L As Integer                        'the real index of label of polygen object which is selected

    Public LReadySelectArrayIndx As Integer                            'the candidate index of line object for selection
    Public LRealSelectArrayIndx As Integer                             'the real index of line object which is selected
    Public LReadySelectArrayIndx_L As Integer                          'the candidate index of label of line object for selection
    Public LRealSelectArrayIndx_L As Integer                           'the real index of label of line object which is selected

    Public PReadySelectArrayIndx As Integer                            'the candidate index of point object for selection
    Public PRealSelectArrayIndx As Integer                             'the real index of point object which is selected
    Public PReadySelectArrayIndx_L As Integer                          'the candidate index of label of point object for selection
    Public PRealSelectArrayIndx_L As Integer                           'the real index of label of point object which is selected

    Public CurvePreviousPoint As System.Nullable(Of Point) = Nothing           'previous point of curve object
    Public LinePreviousPoint As System.Nullable(Of Point) = Nothing            'previous point of Line object
    Public PointPreviousPoint As System.Nullable(Of Point) = Nothing           'previous point of Point object
    Public PolyPreviousPoint As System.Nullable(Of Point) = Nothing            'previous point of polygen object
    Public CuPolyPreviousPoint As System.Nullable(Of Point) = Nothing          'previous point of curve&poly object
    Public MousePosPoint As System.Nullable(Of Point) = Nothing                'the position of mouse cursor

    Public XsLinePoint As Integer                                      'X-coordinate of foot of perpendicular
    Public YsLinePoint As Integer                                      'Y-coordinate of foot of perpendicular
    Public PXs, PYs As Integer                                         'points used for drawing max, min lines
    Public FinalPXs, FinalPYs As Integer

    Public DotX, DotY, CDotX, CDotY As Integer                         'points used for dotted lines

    Public OutPointFlag As Boolean                                     'flag specifies whether the foot of perpendicular is in range of object or not
    Public COutPointFlag As Boolean                                    'flag specifies whether the foot of perpendicular is in range of curve&poly object or not
    Public PDotX As Integer                                            'X-coordinate of point which is used for drawing dotted line in case of polygen object
    Public PDotY As Integer                                            'Y-coordinate of point which is used for drawing dotted line in case of polygen object
    Public POutFlag As Boolean                                         'flag specifies whether the foot of perpendicular is in range of polygen object or not

    Private C_PolyObj As C_PolyObject = New C_PolyObject()
    Private C_PointObj As C_PointObject = New C_PointObject()
    Private C_LineObj As C_LineObject = New C_LineObject()
    Private C_CuPolyObj As C_CuPolyObject = New C_CuPolyObject()
    Private C_CurveObj As C_CurveObject = New C_CurveObject()
    Private curve_sel_index As Integer
    Private move_line As Boolean
    Private StartPtOfMove As PointF = New PointF()
    Private EndPtOfMove As PointF = New PointF()

    'member variables for edge detection
    Public EdgeRegionDrawReady As Boolean
    Public EdgeRegionDrawed As Boolean
    Public FirstPtOfEdge As Point = New Point()
    Public SecondPtOfEdge As Point = New Point()
    Public MouseDownFlag As Boolean
    Public Col_list As List(Of String) = New List(Of String)        'the list of color names
    Public Obj_Seg As SegObject = New SegObject()



    Public Sub New()
        InitializeComponent()
        InitializeCustomeComeponent()

        anno_num = -1
        cur_measure_type = -1
        cur_measure_type_prev = -1
        graphPen_line.DashStyle = Drawing2D.DashStyle.Dot
        ID_BTN_CUR_COL.BackColor = Color.Black
        ID_BTN_TEXT_COL.BackColor = Color.Black
        ID_COMBO_LINE_SHAPE.SelectedIndex = 0
        Dim mat As Mat = Nothing

        For i = 0 To 24

            Dim list As List(Of MeasureObject) = New List(Of MeasureObject)()
            initial_ratio(i) = 1
            object_list.Add(list)
            cur_obj_num(i) = 0
            origin_image.Add(mat)
            resized_image.Add(mat)
            current_image.Add(mat)
            gamma(i) = 100
            zoom_factor(i) = 1.0
        Next

    End Sub

    Private Const EM_GETLINECOUNT As Integer = &HBA
    <DllImport("user32", EntryPoint:="SendMessageA", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
    Private Shared Function SendMessage(ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetCapture(ByVal hWnd As Integer) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ReleaseCapture() As Long
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetCapture() As IntPtr
    End Function

#Region "Main Form Methods"
    'Initialize custome controls
    Private Sub InitializeCustomeComeponent()
        graphFont = New Font("Arial", 10, FontStyle.Regular)
        For i = 0 To 24

            ID_TAG_PAGE(i) = New TabPage()
            ID_PANEL(i) = New Panel()
            ID_PICTURE_BOX(i) = New PictureBox()
            ID_MY_TEXTBOX(i) = New TextBox()

            ID_TAG_CTRL.Controls.Add(ID_TAG_PAGE(i))

            ID_TAG_PAGE(i).Location = New Point(4, 24)
            ID_TAG_PAGE(i).Name = "ID_TAG_PAGE" & i.ToString()
            ID_TAG_PAGE(i).Padding = New Padding(3)
            ID_TAG_PAGE(i).Size = New Size(800, 600)
            ID_TAG_PAGE(i).Text = "Image" & (i + 1).ToString()
            ID_TAG_PAGE(i).UseVisualStyleBackColor = True
            ID_TAG_PAGE(i).Controls.Add(ID_PANEL(i))

            ID_PANEL(i).Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
            ID_PANEL(i).AutoScroll = True
            ID_PANEL(i).AutoSizeMode = AutoSizeMode.GrowAndShrink
            ID_PANEL(i).BackColor = Color.Gray
            ID_PANEL(i).Location = New Point(0, 1)
            ID_PANEL(i).Name = "ID_PANEL" & i.ToString()
            ID_PANEL(i).Size = New Size(800, 600)
            AddHandler ID_PANEL(i).Scroll, New ScrollEventHandler(AddressOf ID_PANEL_Scroll)
            AddHandler ID_PANEL(i).SizeChanged, New EventHandler(AddressOf ID_PANEL_SizeChanged)
            AddHandler ID_PANEL(i).MouseWheel, New MouseEventHandler(AddressOf ID_PANEL_MouseWheel)
            ID_PANEL(i).Controls.Add(ID_PICTURE_BOX(i))

            ID_PICTURE_BOX(i).BackColor = Color.Gray
            ID_PICTURE_BOX(i).Location = New Point(0, -1)
            ID_PICTURE_BOX(i).Name = "ID_PICTURE_BOX" & i.ToString()
            ID_PICTURE_BOX(i).Size = New Size(800, 600)
            ID_PICTURE_BOX(i).SizeMode = PictureBoxSizeMode.AutoSize
            ID_PICTURE_BOX(i).TabIndex = 0
            ID_PICTURE_BOX(i).TabStop = False
            ID_PICTURE_BOX(i).Image = Nothing
            AddHandler ID_PICTURE_BOX(i).MouseDown, New MouseEventHandler(AddressOf ID_PICTURE_BOX_MouseDown)
            AddHandler ID_PICTURE_BOX(i).MouseMove, New MouseEventHandler(AddressOf ID_PICTURE_BOX_MouseMove)
            AddHandler ID_PICTURE_BOX(i).MouseDoubleClick, New MouseEventHandler(AddressOf ID_PICTURE_BOX_MouseDoubleClick)
            AddHandler ID_PICTURE_BOX(i).MouseUp, New MouseEventHandler(AddressOf ID_PICTURE_BOX_MouseUp)

            AddHandler ID_PICTURE_BOX(i).Paint, New PaintEventHandler(AddressOf ID_PICTURE_BOX_Paint)

            ID_PICTURE_BOX(i).Controls.Add(ID_MY_TEXTBOX(i))

            ID_MY_TEXTBOX(i).Name = "ID_MY_TEXTBOX"
            ID_MY_TEXTBOX(i).Multiline = True
            ID_MY_TEXTBOX(i).AutoSize = False
            ID_MY_TEXTBOX(i).Visible = False
            ID_MY_TEXTBOX(i).Font = graphFont
            AddHandler ID_MY_TEXTBOX(i).TextChanged, New EventHandler(AddressOf ID_MY_TEXTBOX_TextChanged)
        Next

        'remove unnessary tab pages
        For i = 1 To 24
            ID_TAG_CTRL.TabPages.Remove(ID_TAG_PAGE(i))
            tag_page_flag(i) = False
            img_import_flag(i) = True
        Next

        tag_page_flag(0) = True
        img_import_flag(0) = True


    End Sub

    'Initialize the color of measuring buttons
    Private Sub Initialize_Button_Colors()
        ID_BTN_ANGLE.BackColor = Color.LightBlue
        ID_BTN_ANNOTATION.BackColor = Color.LightBlue
        ID_BTN_ARC.BackColor = Color.LightBlue
        ID_BTN_LINE_ALIGN.BackColor = Color.LightBlue
        ID_BTN_LINE_HOR.BackColor = Color.LightBlue
        ID_BTN_LINE_PARA.BackColor = Color.LightBlue
        ID_BTN_LINE_VER.BackColor = Color.LightBlue
        ID_BTN_PENCIL.BackColor = Color.LightBlue
        ID_BTN_P_LINE.BackColor = Color.LightBlue
        ID_BTN_RADIUS.BackColor = Color.LightBlue
        ID_BTN_SCALE.BackColor = Color.LightBlue
        ID_BTN_C_LINE.BackColor = Color.LightBlue
        ID_BTN_C_POLY.BackColor = Color.LightBlue
        ID_BTN_C_POINT.BackColor = Color.LightBlue
        ID_BTN_C_CURVE.BackColor = Color.LightBlue
        ID_BTN_C_CUPOLY.BackColor = Color.LightBlue
        ID_BTN_C_SEL.BackColor = Color.LightBlue
    End Sub

    'get setting information from ini file
    Private Sub GetInforFromIni()

        If IO.File.Exists(ini_path) Then
            ini = New IniFile(ini_path)
            Dim Keys As ArrayList = ini.GetKeys("Config")
            Dim myEnumerator As System.Collections.IEnumerator = Keys.GetEnumerator()
            While myEnumerator.MoveNext()
                If myEnumerator.Current.Name = "unit" Then
                    scale_unit = myEnumerator.Current.value
                Else
                    digit = CInt(myEnumerator.Current.value)
                End If
            End While
            ID_NUM_DIGIT.Value = digit

            Keys.Clear()
            Keys = ini.GetKeys("CF")
            Dim cnt As Integer = 0
            Dim index As Integer = 0
            myEnumerator = Keys.GetEnumerator()
            While myEnumerator.MoveNext()
                If myEnumerator.Current.Name = "index" Then
                    index = CInt(myEnumerator.Current.value)
                Else
                    Dim line As String = myEnumerator.Current.value
                    Dim parse_num = line.IndexOf(":")
                    Dim CF_key = line.Substring(0, parse_num)
                    Dim CF_val = CDbl(line.Substring(parse_num + 1))

                    CF_list.Add(CF_key)
                    CF_num.Add(CF_val)
                    ID_COMBOBOX_CF.Items.Add(CF_key)
                End If

            End While

            Keys.Clear()
            Keys = ini.GetKeys("name")
            cnt = 0
            myEnumerator = Keys.GetEnumerator()
            While myEnumerator.MoveNext()
                Dim line As String = myEnumerator.Current.value
                name_list.Add(line)
            End While

            ID_COMBOBOX_CF.SelectedIndex = index
        Else
            'set default value when ini file does not exist in document folder
            scale_unit = "cm"
            digit = 0
            ID_NUM_DIGIT.Value = digit

            Dim CF_cnt = 9

            CF_list.Add("1.0X")
            CF_num.Add(1.0)
            CF_list.Add("1.25X")
            CF_num.Add(1.25)
            CF_list.Add("1.5X")
            CF_num.Add(1.5)
            CF_list.Add("2.0X")
            CF_num.Add(2.0)
            CF_list.Add("2.5X")
            CF_num.Add(2.5)
            CF_list.Add("3.5X")
            CF_num.Add(3.5)
            CF_list.Add("5.0X")
            CF_num.Add(5.0)
            CF_list.Add("7.5X")
            CF_num.Add(7.5)
            CF_list.Add("10.0X")
            CF_num.Add(10.0)

            For i = 0 To CF_list.Count - 1
                ID_COMBOBOX_CF.Items.Add(CF_list(i))
            Next

            Dim name_cnt = 4
            name_list.Add("Line")
            name_list.Add("Angle")
            name_list.Add("Arc")
            name_list.Add("Scale")

            ID_COMBOBOX_CF.SelectedIndex = 0
        End If

    End Sub

    'check license information when main dialog is loading
    Private Sub Main_Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            'Init()
            Initialize_Button_Colors()
            Timer1.Interval = 30
            Timer1.Start()
            GetInforFromIni()
            initVar()
        Catch ex As Exception

            'ID_GROUP_BOX_CONTROL.Enabled = False
            MessageBox.Show(ex.Message.ToString())

        End Try

        Try
            OpenCamera()
            SelectResolution(videoDevice, CameraResolutionsCB)
            If Not My.Settings.camresindex.Equals("") Then
                CameraResolutionsCB.SelectedIndex = My.Settings.camresindex + 1
            End If

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        If My.Settings.imagefilepath.Equals("") Then
            imagepath = "MyImages"
            My.Settings.imagefilepath = imagepath
            My.Settings.Save()
            txtbx_imagepath.Text = imagepath
        Else
            imagepath = My.Settings.imagefilepath
            txtbx_imagepath.Text = My.Settings.imagefilepath
        End If

        obj_seg.circleObj = New CircleObj()
        obj_seg.sectObj = New InterSectionObj()
        Obj_Seg.phaseSegObj = New PhaseSeg()
        Obj_Seg.BlobSegObj = New BlobSeg()
        Dim colType As Type = GetType(System.Drawing.Color)

        For Each prop As PropertyInfo In colType.GetProperties()
            If prop.PropertyType Is GetType(System.Drawing.Color) Then
                Col_list.Add(prop.Name)
            End If
        Next
        DeleteImages(imagepath)
        Createdirectory(imagepath)
    End Sub

    'change the color of button when it is clicked
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If cur_measure_type_prev <> cur_measure_type Then
            Initialize_Button_Colors()
            cur_measure_type_prev = cur_measure_type
            Select Case cur_measure_type
                Case MeasureType.line_align
                    If menu_click = False Then ID_BTN_LINE_ALIGN.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a line through two input points."
                Case MeasureType.line_horizontal
                    If menu_click = False Then ID_BTN_LINE_HOR.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a horizontal line through two input points."
                Case MeasureType.line_vertical
                    If menu_click = False Then ID_BTN_LINE_VER.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a vertical line through two input points."
                Case MeasureType.angle
                    If menu_click = False Then ID_BTN_ARC.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates angle through three points in degree."
                Case MeasureType.radius
                    If menu_click = False Then ID_BTN_RADIUS.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a arc through three input points."
                Case MeasureType.annotation
                    If menu_click = False Then ID_BTN_ANNOTATION.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Add a annotation."
                Case MeasureType.angle_far
                    If menu_click = False Then ID_BTN_ANGLE.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates angle through two lines in degree."
                Case MeasureType.line_para
                    If menu_click = False Then ID_BTN_LINE_PARA.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a line through two parallel lines."
                Case MeasureType.draw_line
                    If menu_click = False Then ID_BTN_PENCIL.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a line through two input points."
                Case MeasureType.pt_line
                    If menu_click = False Then ID_BTN_P_LINE.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Calculates a line between a point and a line."
                Case MeasureType.measure_scale
                    If menu_click = False Then ID_BTN_SCALE.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Insert a measuring scale."
                Case MeasureType.C_Line
                    If menu_click = False Then ID_BTN_C_LINE.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a line."
                Case MeasureType.C_Poly
                    If menu_click = False Then ID_BTN_C_POLY.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a polygen."
                Case MeasureType.C_Point
                    If menu_click = False Then ID_BTN_C_POINT.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a point."
                Case MeasureType.C_Curve
                    If menu_click = False Then ID_BTN_C_CURVE.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a curve."
                Case MeasureType.C_CuPoly
                    If menu_click = False Then ID_BTN_C_CUPOLY.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "Draw a curve&polygen."
                Case MeasureType.C_Sel
                    If menu_click = False Then ID_BTN_C_SEL.BackColor = Color.DodgerBlue
                    ID_STATUS_LABEL.Text = "select objects."
            End Select

        End If

    End Sub

    'open camera
    Private Sub ID_MENU_OPEN_CAM_Click(sender As Object, e As EventArgs) Handles ID_MENU_OPEN_CAM.Click
        Try
            OpenCamera()
            SelectResolution(videoDevice, CameraResolutionsCB)
            If Not My.Settings.camresindex.Equals("") Then
                CameraResolutionsCB.SelectedIndex = My.Settings.camresindex + 1
            End If
        Catch excpt As Exception
            MessageBox.Show(excpt.Message)
        End Try
    End Sub

    'close camera
    Private Sub ID_MENU_CLOSE_CAM_Click(sender As Object, e As EventArgs) Handles ID_MENU_CLOSE_CAM.Click
        Try
            CloseCamera()
            ID_PICTURE_BOX(0).Image = Nothing
            ID_PICTURE_BOX_CAM.Image = Nothing
        Catch excpt As Exception
            MessageBox.Show(excpt.Message)
        End Try
    End Sub

    'import image and draw it to picturebox
    'format variables
    Private Sub ID_MENU_OPEN_Click(sender As Object, e As EventArgs) Handles ID_MENU_OPEN.Click
        cur_measure_type = -1

        Dim filter = "JPEG Files|*.jpg|PNG Files|*.png|BMP Files|*.bmp|All Files|*.*"
        Dim title = "Open"

        Dim start As Integer = tab_index
        img_import_flag(tab_index) = True

        Dim img_cnt = ID_PICTURE_BOX(0).LoadImageFromFiles(filter, title, origin_image, resized_image, initial_ratio, start, img_import_flag)

        If img_cnt >= 1 Then
            ID_PICTURE_BOX(tab_index).Image = Nothing
            obj_selected.Refresh()
            cur_measure_type = -1
            sel_index = -1
            curve_sel_index = -1
            initVar()
        End If
        Dim added_tag = 0
        While added_tag < img_cnt
            If start > 24 Then Exit While

            If tag_page_flag(start) = True AndAlso ID_PICTURE_BOX(start).Image IsNot Nothing Then
                start = start + 1
                Continue While
            End If

            If tag_page_flag(start) = False Then
                ID_TAG_CTRL.TabPages.Add(ID_TAG_PAGE(start))
                tag_page_flag(start) = True
            End If

            Dim img = resized_image.ElementAt(start)
            ID_PICTURE_BOX(start).Invoke(New Action(Sub() ID_PICTURE_BOX(start).Image = img.ToBitmap()))

            left_top = ID_PICTURE_BOX(start).CenteringImage(ID_PANEL(start))
            current_image(start) = img
            cur_obj_num(start) = 0
            Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, start).Clear()
            brightness(start) = 0
            contrast(start) = 0
            gamma(start) = 100
            img_import_flag(start) = False

            start = start + 1
            added_tag = added_tag + 1
        End While

        start = start - 1
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(start), CF, digit, scale_unit, name_list)
        ID_TAG_CTRL.SelectedTab = ID_TAG_PAGE(start)
    End Sub

    'export image to jpg
    Private Sub ID_MENU_SAVE_Click(sender As Object, e As EventArgs) Handles ID_MENU_SAVE.Click
        Dim filter = "JPEG Files|*.jpg"
        Dim title = "Save"
        ID_PICTURE_BOX(tab_index).SaveImageInFile(filter, title, object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF)
    End Sub

    'save object information as excel file
    Private Sub ID_MENU_SAVE_XLSX_Click(sender As Object, e As EventArgs) Handles ID_MENU_SAVE_XLSX.Click
        Dim filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        Dim title = "Save"
        ID_PICTURE_BOX.SaveListToExcel(object_list, filter, title, CF, digit, scale_unit)
    End Sub

    'save object list and image as excel file
    Private Sub ID_MENU_EXPORT_REPORT_Click(sender As Object, e As EventArgs) Handles ID_MENU_EXPORT_REPORT.Click
        Dim filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        Dim title = "Save"
        ID_PICTURE_BOX.SaveReportToExcel(filter, title, object_list, graphPen, graphPen_line, digit, CF, scale_unit)
    End Sub

    'exit the program
    Private Sub ID_MENU_EXIT_Click(sender As Object, e As EventArgs) Handles ID_MENU_EXIT.Click
        Call Application.Exit()
    End Sub

    'set current measurement type as line_align
    'reset the current object
    Private Sub ID_BTN_LINE_ALIGN_Click(sender As Object, e As EventArgs) Handles ID_BTN_LINE_ALIGN.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_align
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub LINEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LINEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_align
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as line_horizontal
    'reset the current object
    Private Sub ID_BTN_LINE_HOR_Click(sender As Object, e As EventArgs) Handles ID_BTN_LINE_HOR.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_horizontal
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub HORIZONTALLINEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HORIZONTALLINEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_horizontal
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as line_vertical
    'reset the current object
    Private Sub ID_BTN_LINE_VER_Click(sender As Object, e As EventArgs) Handles ID_BTN_LINE_VER.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_vertical
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub VERTICALLINEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VERTICALLINEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_vertical
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as line parallel
    'reset the current object
    Private Sub ID_BTN_LINE_PARA_Click(sender As Object, e As EventArgs) Handles ID_BTN_LINE_PARA.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_para
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub PARALLELLINEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PARALLELLINEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_para
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as angle
    'reset the current object
    Private Sub ID_BTN_ARC_Click(sender As Object, e As EventArgs) Handles ID_BTN_ARC.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.angle
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub ANGLETHROUGHTHREEPOINTSToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ANGLETHROUGHTHREEPOINTSToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.angle
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as angle far
    'reset the current object
    Private Sub ID_BTN_ANGLE_Click(sender As Object, e As EventArgs) Handles ID_BTN_ANGLE.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.angle_far
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub ANGLETHROUGHTWOLINESToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ANGLETHROUGHTWOLINESToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.angle_far
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as radius
    'reset the current object
    Private Sub ID_BTN_RADIUS_Click(sender As Object, e As EventArgs) Handles ID_BTN_RADIUS.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.radius
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub ARCToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ARCToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.radius
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as annotation
    'reset the current object
    Private Sub ID_BTN_ANNOTATION_Click(sender As Object, e As EventArgs) Handles ID_BTN_ANNOTATION.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.annotation
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub ANNOTATIONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ANNOTATIONToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.annotation
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as draw line
    'reset the current object
    Private Sub ID_BTN_PENCIL_Click(sender As Object, e As EventArgs) Handles ID_BTN_PENCIL.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.draw_line
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set current measurement type as point to line
    'reset the current object
    Private Sub ID_BTN_P_LINE_Click(sender As Object, e As EventArgs) Handles ID_BTN_P_LINE.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.pt_line
        obj_selected.measure_type = cur_measure_type
    End Sub

    Private Sub DISTANCEFROMPOINTTOLINEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DISTANCEFROMPOINTTOLINEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.pt_line
        obj_selected.measure_type = cur_measure_type
    End Sub

    'set measureing scale 
    'set current measurement type as measuring scale
    'reset the current object
    Private Sub ID_BTN_SCALE_Click(sender As Object, e As EventArgs) Handles ID_BTN_SCALE.Click
        menu_click = False
        Dim form As ID_FORM_SCALE = New ID_FORM_SCALE(scale_unit)
        If form.ShowDialog() = DialogResult.OK Then
            scale_style = form.scale_style
            scale_value = form.scale_value
            scale_unit = form.scale_unit

            obj_selected.Refresh()
            cur_measure_type = MeasureType.measure_scale
            obj_selected.measure_type = cur_measure_type

            obj_selected.scale_object.style = scale_style
            obj_selected.scale_object.length = scale_value
        End If

    End Sub

    'set current measurement type as circle_fixed
    Private Sub ANGLEOFFIXEDDIAMETERToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ANGLEOFFIXEDDIAMETERToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.circle_fixed
        obj_selected.measure_type = cur_measure_type

        ID_STATUS_LABEL.Text = "Drawing a circle which has fixed radius"
        Dim form = New Form3()
        If form.ShowDialog() = DialogResult.OK Then
            obj_selected.scale_object.length = CSng(form.ID_TEXT_FIXED.Text)
            obj_selected.radius = obj_selected.scale_object.length / ID_PICTURE_BOX(tab_index).Width
        End If
    End Sub

    'set current measurement type as line_fixed
    Private Sub LINEOFFIXEDLENGTHToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LINEOFFIXEDLENGTHToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.line_fixed
        obj_selected.measure_type = cur_measure_type

        ID_STATUS_LABEL.Text = "Drawing a line which has fixed length"
        Dim form = New Form3()
        If form.ShowDialog() = DialogResult.OK Then
            obj_selected.scale_object.length = CSng(form.ID_TEXT_FIXED.Text)
            obj_selected.length = obj_selected.scale_object.length / ID_PICTURE_BOX(tab_index).Width
        End If
    End Sub

    'set current measurement type as angle_fixed
    Private Sub FIXEDANGLEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FIXEDANGLEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.angle_fixed
        obj_selected.measure_type = cur_measure_type

        ID_STATUS_LABEL.Text = "Drawing a angle which has fixed angle"
        Dim form = New Form3()
        If form.ShowDialog() = DialogResult.OK Then
            obj_selected.angle = CSng(form.ID_TEXT_FIXED.Text)
        End If
    End Sub

    'set move_line to ture so that you can move curves line object
    Private Sub MOVELINEOBJECTToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MOVELINEOBJECTToolStripMenuItem.Click
        move_line = True
        ID_STATUS_LABEL.Text = "Duplicating Curve Line Object"
    End Sub

    'zoom image
    Private Sub Zoom_Image()
        Try
            Dim ratio = zoom_factor(tab_index)
            Dim zoomed = ZoomImage(ratio, current_image, current_image, tab_index)
            'Dim Image = Enumerable.ElementAt(current_image, tab_index).ToBitmap()
            Dim Image = zoomed.ToBitmap()
            Dim Adjusted = AdjustBrightnessAndContrast(Image, brightness(tab_index), contrast(tab_index), gamma(tab_index))

            'ID_PICTURE_BOX(tab_index).Invoke(New Action(Sub() ID_PICTURE_BOX(tab_index).Image = Enumerable.ElementAt(current_image, tab_index).ToBitmap()))
            ID_PICTURE_BOX(tab_index).Image = Adjusted
            left_top = ID_PICTURE_BOX(tab_index).CenteringImage(ID_PANEL(tab_index))
            scroll_pos.X = ID_PANEL(tab_index).HorizontalScroll.Value
            scroll_pos.Y = ID_PANEL(tab_index).VerticalScroll.Value
            ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
            Dim flag = False
            If sel_index >= 0 Then flag = True
            ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, flag)
            If ID_MY_TEXTBOX(tab_index).Visible = True Then
                Dim obj_anno = object_list.ElementAt(tab_index).ElementAt(anno_num)
                Dim st_pt As Point = New Point(obj_anno.draw_point.X * ID_PICTURE_BOX(tab_index).Width, obj_anno.draw_point.Y * ID_PICTURE_BOX(tab_index).Height)
                ID_MY_TEXTBOX(tab_index).UpdateLocation(st_pt, left_top, scroll_pos)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
        End Try
    End Sub

    'zoom in image and draw it to picturebox
    Private Sub ID_BTN_ZOON_IN_Click(sender As Object, e As EventArgs) Handles ID_BTN_ZOON_IN.Click
        menu_click = False
        zoom_factor(tab_index) *= 1.1
        Zoom_Image()
        ID_STATUS_LABEL.Text = "Zoom In"
    End Sub

    'zoom out image and draw it to picturebox
    Private Sub ID_BTN_ZOOM_OUT_Click(sender As Object, e As EventArgs) Handles ID_BTN_ZOOM_OUT.Click
        menu_click = False
        zoom_factor(tab_index) /= 1.1
        Zoom_Image()
        ID_STATUS_LABEL.Text = "Zoom Out"
    End Sub

    'zoom in
    Private Sub ZOOMINToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZOOMINToolStripMenuItem.Click
        menu_click = True
        zoom_factor(tab_index) *= 1.1
        Zoom_Image()
        ID_STATUS_LABEL.Text = "Zoom In"
    End Sub

    'zoom out
    Private Sub ZOOMOUTToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ZOOMOUTToolStripMenuItem.Click
        menu_click = True
        zoom_factor(tab_index) /= 1.1
        Zoom_Image()
        ID_STATUS_LABEL.Text = "Zoom Out"
    End Sub

    'undo last object and last row of listview
    Private Sub Undo()
        If undo_num > 0 Then
            obj_selected.Refresh()
            sel_index = -1
            sel_pt_index = -1
            curve_sel_index = -1
            Dim flag = RemoveObjFromList(object_list.ElementAt(tab_index))
            If flag = True Then
                ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
                undo_num -= 1
                cur_obj_num(tab_index) -= 1
            End If
        End If
    End Sub
    Private Sub ID_BTN_UNDO_Click(sender As Object, e As EventArgs) Handles ID_BTN_UNDO.Click
        menu_click = False
        Undo()
        ID_STATUS_LABEL.Text = "Undo"
    End Sub


    Private Sub UNDOToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UNDOToolStripMenuItem.Click
        menu_click = True
        Undo()
        ID_STATUS_LABEL.Text = "Undo"
    End Sub

    'reset current object
    Private Sub ID_BTN_RESEL_Click(sender As Object, e As EventArgs) Handles ID_BTN_RESEL.Click
        menu_click = False
        obj_selected.Refresh()
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_STATUS_LABEL.Text = "Reselect"
    End Sub

    Private Sub RESELECTToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RESELECTToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_STATUS_LABEL.Text = "Reselect"
    End Sub

    'reset digit
    'reload image and obj_list
    Private Sub ID_NUM_DIGIT_ValueChanged(sender As Object, e As EventArgs) Handles ID_NUM_DIGIT.ValueChanged
        menu_click = False
        digit = CInt(ID_NUM_DIGIT.Value)
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
        ID_STATUS_LABEL.Text = "Changing the digit of decimal numbers."
    End Sub

    'set the width of line
    Private Sub ID_NUM_LINE_WIDTH_ValueChanged(sender As Object, e As EventArgs) Handles ID_NUM_LINE_WIDTH.ValueChanged
        menu_click = False
        line_infor.line_width = CInt(ID_NUM_LINE_WIDTH.Value)
        ID_STATUS_LABEL.Text = "Changing the width of drawing line."
    End Sub

    'change the color of LineStyle object
    Private Sub ID_BTN_COL_PICKER_Click(sender As Object, e As EventArgs) Handles ID_BTN_COL_PICKER.Click
        menu_click = False
        Dim clrDialog As ColorDialog = New ColorDialog()

        'show the colour dialog and check that user clicked ok
        If clrDialog.ShowDialog() = DialogResult.OK Then
            'save the colour that the user chose
            line_infor.line_color = clrDialog.Color
            ID_BTN_CUR_COL.BackColor = clrDialog.Color
        End If
        ID_STATUS_LABEL.Text = "Changing the color of drawing line."
    End Sub

    'set the line style
    Private Sub ID_COMBO_LINE_SHAPE_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ID_COMBO_LINE_SHAPE.SelectedIndexChanged
        menu_click = False
        Dim comboIndex = ID_COMBO_LINE_SHAPE.SelectedIndex
        If comboIndex = 0 Then
            graphPen_line.DashStyle = Drawing2D.DashStyle.Dot
            'obj_selected.line_shape = "dotted";
            line_infor.line_style = "dotted"
        ElseIf comboIndex = 1 Then
            graphPen_line.DashPattern = dashValues
            'obj_selected.line_shape = "dashed";
            line_infor.line_style = "dashed"
        End If
        ID_STATUS_LABEL.Text = "Changing the shape of drawing line."
    End Sub

    'set text fore color
    Private Sub ID_BTN_TEXT_COL_PICKER_Click(sender As Object, e As EventArgs) Handles ID_BTN_TEXT_COL_PICKER.Click
        Dim clrDialog As ColorDialog = New ColorDialog()

        'show the colour dialog and check that user clicked ok
        If clrDialog.ShowDialog() = DialogResult.OK Then
            'save the colour that the user chose
            font_infor.font_color = clrDialog.Color
            ID_BTN_TEXT_COL.BackColor = clrDialog.Color
        End If
        ID_STATUS_LABEL.Text = "Changing the color of text."
    End Sub

    'set text font
    Private Sub ID_BTN_TEXT_FONT_Click(sender As Object, e As EventArgs) Handles ID_BTN_TEXT_FONT.Click
        Dim fontDialog As FontDialog = New FontDialog()

        If fontDialog.ShowDialog() = DialogResult.OK Then
            font_infor.text_font = fontDialog.Font
        End If
        ID_STATUS_LABEL.Text = "Changing the font of text."
    End Sub

    'redraw objects
    Private Sub ID_PANEL_Scroll(sender As Object, e As ScrollEventArgs)
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        Dim flag = False
        If sel_index >= 0 Then flag = True
        ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, flag)
    End Sub

    'keep the image in the center when the panel size in changed
    'redraw objects
    Private Sub ID_PANEL_SizeChanged(sender As Object, e As EventArgs)
        left_top = ID_PICTURE_BOX(tab_index).CenteringImage(ID_PANEL(tab_index))
        scroll_pos.X = ID_PANEL(tab_index).HorizontalScroll.Value
        scroll_pos.Y = ID_PANEL(tab_index).VerticalScroll.Value
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        Dim flag = False
        If sel_index >= 0 Then flag = True
        ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, flag)
        If ID_MY_TEXTBOX(tab_index).Visible = True Then
            Dim obj_anno = object_list.ElementAt(tab_index).ElementAt(anno_num)
            Dim st_pt As Point = New Point(obj_anno.draw_point.X * ID_PICTURE_BOX(tab_index).Width, obj_anno.draw_point.Y * ID_PICTURE_BOX(tab_index).Height)
            ID_MY_TEXTBOX(tab_index).UpdateLocation(st_pt, left_top, scroll_pos)
        End If
    End Sub

    'redraw objects
    Private Sub ID_PANEL_MouseWheel(sender As Object, e As MouseEventArgs)
        Dim flag = False
        If sel_index >= 0 Then flag = True
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, flag)
    End Sub

    'detect edge of selected region
    Private Sub EDGEDETECTToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EDGEDETECTToolStripMenuItem.Click
        EdgeRegionDrawReady = True
        obj_selected.Refresh()
        obj_selected.measure_type = MeasureType.C_Curve
        ID_STATUS_LABEL.Text = "Detect edge."
    End Sub

    'update object selected
    'when mouse is clicked on annotation insert textbox there to you can edit it
    'draw objects and load list of objects to listview
    Private Sub ID_PICTURE_BOX_MouseDown(sender As Object, e As MouseEventArgs)
        If ID_PICTURE_BOX(tab_index).Image Is Nothing OrElse current_image(tab_index) Is Nothing Then
            Return
        End If
        If e.Button = MouseButtons.Left Then
            SetCapture(CInt(ID_PICTURE_BOX(tab_index).Handle))
            Dim m_pt As PointF = New PointF()
            m_pt.X = CSng(e.X) / ID_PICTURE_BOX(tab_index).Width
            m_pt.Y = CSng(e.Y) / ID_PICTURE_BOX(tab_index).Height
            m_pt.X = Math.Min(Math.Max(m_pt.X, 0), 1)
            m_pt.Y = Math.Min(Math.Max(m_pt.Y, 0), 1)
            m_cur_drag = m_pt

            Dim m_pt2 As Point = New Point(e.X, e.Y)

            If cur_measure_type >= 0 Then
                If cur_measure_type < MeasureType.C_Line Then
                    Dim completed = ModifyObjSelected(obj_selected, cur_measure_type, m_pt, Enumerable.ElementAt(origin_image, tab_index).Width, Enumerable.ElementAt(origin_image, tab_index).Height, line_infor, font_infor, CF)

                    If completed Then
                        obj_selected.obj_num = cur_obj_num(tab_index)
                        object_list(tab_index).Add(obj_selected)
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
                        obj_selected.Refresh()
                        cur_measure_type = -1
                        cur_obj_num(tab_index) += 1
                        If undo_num < 2 Then undo_num += 1
                    Else
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, False)
                    End If
                Else    'Curve objects
                    If cur_measure_type = MeasureType.C_Poly Then
                        If PolyPreviousPoint IsNot Nothing Then
                            C_PolyObj.PolyPoint(C_PolyObj.PolyPointIndx) = m_pt
                            C_PolyObj.PolyPointIndx += 1
                            PolyPreviousPoint = Nothing
                        End If
                    ElseIf cur_measure_type = MeasureType.C_CuPoly Then
                        CuPolyDrawEndFlag = False
                        C_CuPolyObj.CuPolyPointIndx_j += 1
                        C_CuPolyObj.CuPolyPoint(C_CuPolyObj.CuPolyPointIndx_j, 0) = m_pt
                    ElseIf cur_measure_type = MeasureType.C_Point Then
                        C_PointObj.PointPoint = m_pt
                    ElseIf cur_measure_type = MeasureType.C_Line Then
                        If LinePreviousPoint Is Nothing Then
                            LinePreviousPoint = e.Location
                            C_LineObj.FirstPointOfLine = m_pt
                        End If
                    ElseIf cur_measure_type = MeasureType.C_Sel Then
                        If curve_sel_index >= 0 Then
                            Dim obj = object_list.ElementAt(tab_index).ElementAt(curve_sel_index)
                            If obj.measure_type = MeasureType.C_CuPoly Then
                                CuPolyRealSelectArrayIndx = curve_sel_index
                            ElseIf obj.measure_type = MeasureType.C_Curve Then
                                CRealSelectArrayIndx = curve_sel_index
                            ElseIf obj.measure_type = MeasureType.C_Line Then
                                LRealSelectArrayIndx = curve_sel_index
                            ElseIf obj.measure_type = MeasureType.C_Point Then
                                PRealSelectArrayIndx = curve_sel_index
                            ElseIf obj.measure_type = MeasureType.C_Poly Then
                                PolyRealSelectArrayIndx = curve_sel_index
                            End If
                            ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                            DrawCurveObjSelected(ID_PICTURE_BOX(tab_index), obj, digit, CF)
                        End If
                    End If

                End If

            Else
                'select point of selected object
                If sel_index >= 0 Then
                    sel_pt_index = ID_PICTURE_BOX(tab_index).CheckPointInPos(object_list.ElementAt(tab_index).ElementAt(sel_index), m_pt2)
                    If sel_pt_index >= 0 Then
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        ID_PICTURE_BOX(tab_index).HightLightItem(object_list.ElementAt(tab_index).ElementAt(sel_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, CF)
                        ID_PICTURE_BOX(tab_index).DrawObjSelected(object_list.ElementAt(tab_index).ElementAt(sel_index), True)
                        ID_PICTURE_BOX(tab_index).HighlightTargetPt(object_list.ElementAt(tab_index).ElementAt(sel_index), sel_pt_index)
                        Return
                    End If
                End If

                sel_index = CheckItemInPos(m_pt, object_list.ElementAt(tab_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, CF)
                If sel_index >= 0 Then
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    ID_PICTURE_BOX(tab_index).HightLightItem(object_list.ElementAt(tab_index).ElementAt(sel_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, CF)
                    ID_PICTURE_BOX(tab_index).DrawObjSelected(object_list.ElementAt(tab_index).ElementAt(sel_index), True)
                Else
                    If anno_num >= 0 Then
                        ID_MY_TEXTBOX(tab_index).DisableTextBox(object_list.ElementAt(tab_index), anno_num, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
                        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
                        anno_num = -1
                    End If
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                End If
            End If

            If EdgeRegionDrawReady = True Then
                FirstPtOfEdge = m_pt2
                EdgeRegionDrawed = False
            End If
            MouseDownFlag = True

            If move_line = True And curve_sel_index >= 0 Then
                Dim obj = object_list.ElementAt(tab_index).ElementAt(curve_sel_index)
                If obj.measure_type = MeasureType.C_Line Then
                    StartPtOfMove = m_pt
                    C_LineObj.Refresh()
                    C_LineObj = CloneLineObj(obj.curve_object.LineItem(0))
                    obj_selected2.Refresh()
                    InitializeLineObj(obj_selected2, C_LineObj.LDrawPos, line_infor, font_infor)
                End If
            End If
        Else    'right click
            If cur_measure_type = MeasureType.C_Poly Then
                PolyPreviousPoint = Nothing
                C_PolyObj.PolyDrawPos = PolyGetPos(C_PolyObj)
                Dim tempObj = ClonePolyObj(C_PolyObj)
                obj_selected.curve_object = New CurveObject()
                obj_selected.curve_object.PolyItem.Add(tempObj)
                obj_selected.name = "PL" & cur_obj_num(tab_index)
                AddCurveToList()
                C_PolyObj.Refresh()
                PolyDrawEndFlag = True
            ElseIf cur_measure_type = MeasureType.C_CuPoly Then
                CuPolyPreviousPoint = Nothing
                C_CuPolyObj.CuPolyDrawPos = CuPolyGetPos(C_CuPolyObj)
                Dim tempObj = CloneCuPolyObj(C_CuPolyObj)
                obj_selected.curve_object = New CurveObject()
                obj_selected.curve_object.CuPolyItem.Add(tempObj)
                obj_selected.name = "CP" & cur_obj_num(tab_index)
                AddCurveToList()
                C_CuPolyObj.Refresh()
                CuPolyDrawEndFlag = True
            End If
        End If

    End Sub

    'release capture
    Private Sub ID_PICTURE_BOX_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        Call ReleaseCapture()

        If cur_measure_type = MeasureType.C_Point Then
            C_PointObj.PDrawPos = PGetPos(C_PointObj.PointPoint)
            Dim tempObj = ClonePointObj(C_PointObj)
            obj_selected.curve_object = New CurveObject()
            obj_selected.curve_object.PointItem.Add(tempObj)
            obj_selected.name = "P" & cur_obj_num(tab_index)
            AddCurveToList()
            C_PointObj.Refresh()
        ElseIf cur_measure_type = MeasureType.C_Line Then
            If C_LineObj.SecndPointOfLine.X <> 0 And C_LineObj.SecndPointOfLine.Y <> 0 Then
                LinePreviousPoint = Nothing
                C_LineObj.LDrawPos = LGetPos(C_LineObj)
                Dim tempObj = CloneLineObj(C_LineObj)
                obj_selected.curve_object = New CurveObject()
                obj_selected.curve_object.LineItem.Add(tempObj)
                obj_selected.name = "L" & cur_obj_num(tab_index)
                AddCurveToList()
                C_LineObj.Refresh()
            End If
        ElseIf cur_measure_type = MeasureType.C_Curve Then
            CurvePreviousPoint = Nothing
            C_CurveObj.CDrawPos = CGetPos(C_CurveObj)
            Dim tempObj = CloneCurveObj(C_CurveObj)
            obj_selected.curve_object = New CurveObject()
            obj_selected.curve_object.CurveItem.Add(tempObj)
            obj_selected.name = "C" & cur_obj_num(tab_index)
            AddCurveToList()
            C_CurveObj.Refresh()
        ElseIf cur_measure_type = MeasureType.C_CuPoly Then
            CuPolyPreviousPoint = Nothing
        End If

        If EdgeRegionDrawReady = True And SecondPtOfEdge.X <> 0 And SecondPtOfEdge.Y <> 0 Then
            'run code for detect edge
            If obj_selected.measure_type = MeasureType.C_Curve Then
                Dim input As Image = resized_image(tab_index).ToBitmap()
                Dim Adjusted = AdjustBrightnessAndContrast(input, brightness(tab_index), contrast(tab_index), gamma(tab_index))
                C_CurveObj = Canny(Adjusted, FirstPtOfEdge, SecondPtOfEdge)

                CurvePreviousPoint = Nothing
                C_CurveObj.CDrawPos = CGetPos(C_CurveObj)
                Dim tempObj = CloneCurveObj(C_CurveObj)
                obj_selected.curve_object = New CurveObject()
                obj_selected.curve_object.CurveItem.Add(tempObj)
                obj_selected.name = "C" & cur_obj_num(tab_index)
                AddCurveToList()
                C_CurveObj.Refresh()
                EdgeRegionDrawReady = False
                FirstPtOfEdge.X = 0
                FirstPtOfEdge.Y = 0
                SecondPtOfEdge.X = 0
                SecondPtOfEdge.Y = 0
                Dim form = New Form4()
                Dim result = form.ShowDialog()
                If result = DialogResult.Cancel Then
                    Undo()
                    undo_num += 1

                ElseIf result = DialogResult.Retry Then
                    Undo()
                    undo_num += 1
                    EdgeRegionDrawReady = True
                    obj_selected.measure_type = MeasureType.C_Curve
                End If
            Else
                EdgeRegionDrawed = True
            End If
        End If

        If move_line = True And EndPtOfMove.X <> 0 And EndPtOfMove.Y <> 0 Then
            obj_selected2.obj_num = cur_obj_num(tab_index)
            object_list(tab_index).Add(obj_selected2)
            obj_selected2.Refresh()
            cur_measure_type = -1
            cur_obj_num(tab_index) += 1
            If undo_num < 2 Then undo_num += 1

            C_LineObj.LDrawPos = LGetPos(C_LineObj)
            Dim tempObj = CloneLineObj(C_LineObj)
            obj_selected.curve_object = New CurveObject()
            obj_selected.curve_object.LineItem.Add(tempObj)
            obj_selected.name = "L" & cur_obj_num(tab_index)
            AddCurveToList()
            C_LineObj.Refresh()
            StartPtOfMove.X = 0
            StartPtOfMove.Y = 0
            EndPtOfMove.X = 0
            EndPtOfMove.Y = 0
            move_line = False
        End If
    End Sub


    'draw temporal objects according to mouse cursor
    Private Sub ID_PICTURE_BOX_MouseMove(sender As Object, e As MouseEventArgs)
        Dim m_pt As PointF = New PointF()
        m_pt.X = CSng(e.X) / ID_PICTURE_BOX(tab_index).Width
        m_pt.Y = CSng(e.Y) / ID_PICTURE_BOX(tab_index).Height
        m_pt.X = Math.Min(Math.Max(m_pt.X, 0), 1)
        m_pt.Y = Math.Min(Math.Max(m_pt.Y, 0), 1)
        Dim dx = m_pt.X - m_cur_drag.X
        Dim dy = m_pt.Y - m_cur_drag.Y

        Dim m_pt2 = New Point(e.X, e.Y)

        If GetCapture() = ID_PICTURE_BOX(tab_index).Handle Then
            If cur_measure_type < 0 Then
                If sel_index >= 0 Then
                    m_cur_drag = m_pt
                    If sel_pt_index >= 0 Then
                        ID_PICTURE_BOX(tab_index).Refresh()
                        MovePoint(object_list.ElementAt(tab_index), sel_index, sel_pt_index, dx, dy)
                        ModifyObjSelected(object_list.ElementAt(tab_index), sel_index, Enumerable.ElementAt(origin_image, tab_index).Width, Enumerable.ElementAt(origin_image, tab_index).Height)
                        Dim obj = object_list.ElementAt(tab_index).ElementAt(sel_index)
                        Dim target_pt As Point = New Point()
                        If obj.measure_type = MeasureType.angle Then

                            Dim start_point As Point = New Point()
                            Dim end_point As Point = New Point()
                            Dim middle_point As Point = New Point()

                            start_point.X = CInt(obj.start_point.X * ID_PICTURE_BOX(tab_index).Width)
                            start_point.Y = CInt(obj.start_point.Y * ID_PICTURE_BOX(tab_index).Height)
                            middle_point.X = CInt(obj.middle_point.X * ID_PICTURE_BOX(tab_index).Width)
                            middle_point.Y = CInt(obj.middle_point.Y * ID_PICTURE_BOX(tab_index).Height)
                            end_point.X = CInt(obj.end_point.X * ID_PICTURE_BOX(tab_index).Width)
                            end_point.Y = CInt(obj.end_point.Y * ID_PICTURE_BOX(tab_index).Height)

                            target_pt.X = (start_point.X + end_point.X) / 2
                            target_pt.Y = (start_point.Y + end_point.Y) / 2
                            Dim angles = CalcStartAndSweepAngle(obj, start_point, middle_point, end_point, target_pt)
                            Dim start_angle, sweep_angle As Double
                            start_angle = angles(0)
                            sweep_angle = angles(1)
                            Dim angle As Integer = CInt(2 * start_angle + sweep_angle) / 2
                            Dim radius = CInt(obj.angle_object.radius * ID_PICTURE_BOX(tab_index).Width) + 10
                            target_pt = CalcPositionInCircle(middle_point, radius, angle)
                        Else
                            target_pt = New Point(obj.draw_point.X * ID_PICTURE_BOX(tab_index).Width, obj.draw_point.Y * ID_PICTURE_BOX(tab_index).Height)
                        End If
                        ID_PICTURE_BOX(tab_index).DrawTempFinal(obj, target_pt, side_drag, digit, CF, False)
                        object_list(tab_index)(sel_index) = obj
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)

                    Else
                        MoveObject(object_list.ElementAt(tab_index), sel_index, dx, dy)
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    End If
                    ID_PICTURE_BOX(tab_index).HightLightItem(object_list.ElementAt(tab_index).ElementAt(sel_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, CF)
                    ID_PICTURE_BOX(tab_index).DrawObjSelected(object_list.ElementAt(tab_index).ElementAt(sel_index), True)
                End If
            End If

            If cur_measure_type = MeasureType.C_Curve Then
                If CurvePreviousPoint Is Nothing Then
                    CurvePreviousPoint = e.Location
                    C_CurveObj.CurvePoint(0) = m_pt
                Else
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    DrawCurveObj(ID_PICTURE_BOX(tab_index), line_infor, C_CurveObj)
                    DrawLineBetweenTwoPoints(ID_PICTURE_BOX(tab_index), line_infor, CurvePreviousPoint.Value, e.Location)
                    C_CurveObj.CPointIndx += 1
                    CurvePreviousPoint = e.Location
                    C_CurveObj.CurvePoint(C_CurveObj.CPointIndx) = m_pt
                End If
            ElseIf cur_measure_type = MeasureType.C_Line Then
                If LinePreviousPoint IsNot Nothing Then
                    C_LineObj.SecndPointOfLine = m_pt
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    DrawLineBetweenTwoPoints(ID_PICTURE_BOX(tab_index), line_infor, LinePreviousPoint.Value, e.Location)
                End If
            ElseIf cur_measure_type = MeasureType.C_CuPoly Then
                If CuPolyDrawEndFlag = False Then
                    If CuPolyPreviousPoint Is Nothing Then
                        CuPolyPreviousPoint = e.Location
                        C_CuPolyObj.CuPolyPoint(C_CuPolyObj.CuPolyPointIndx_j, 0) = m_pt
                    Else
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        DrawCuPolyObj(ID_PICTURE_BOX(tab_index), line_infor, C_CuPolyObj)
                        DrawLineBetweenTwoPoints(ID_PICTURE_BOX(tab_index), line_infor, CuPolyPreviousPoint.Value, e.Location)
                        C_CuPolyObj.CuPolyPointIndx_k(C_CuPolyObj.CuPolyPointIndx_j) += 1
                        CuPolyPreviousPoint = e.Location
                        C_CuPolyObj.CuPolyPoint(C_CuPolyObj.CuPolyPointIndx_j, C_CuPolyObj.CuPolyPointIndx_k(C_CuPolyObj.CuPolyPointIndx_j)) = m_pt
                    End If
                End If
            End If

            If EdgeRegionDrawReady = True Then
                SecondPtOfEdge = m_pt2
                ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                ID_PICTURE_BOX(tab_index).DrawRectangle(FirstPtOfEdge, SecondPtOfEdge)
            End If

            If move_line = True And curve_sel_index >= 0 And StartPtOfMove.X <> 0 And StartPtOfMove.Y <> 0 Then
                EndPtOfMove = m_pt
                Dim Obj = object_list.ElementAt(tab_index).ElementAt(curve_sel_index).curve_object.LineItem(0)
                C_LineObj.FirstPointOfLine.X = (EndPtOfMove.X - StartPtOfMove.X) + Obj.FirstPointOfLine.X
                C_LineObj.FirstPointOfLine.Y = (EndPtOfMove.Y - StartPtOfMove.Y) + Obj.FirstPointOfLine.Y
                C_LineObj.SecndPointOfLine.X = (EndPtOfMove.X - StartPtOfMove.X) + Obj.SecndPointOfLine.X
                C_LineObj.SecndPointOfLine.Y = (EndPtOfMove.Y - StartPtOfMove.Y) + Obj.SecndPointOfLine.Y
                ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                DrawLineObject(ID_PICTURE_BOX(tab_index), C_LineObj)
                Dim Delta = GetNormalFromPointToLine(New Point(Obj.FirstPointOfLine.X * ID_PICTURE_BOX(tab_index).Width, Obj.FirstPointOfLine.Y * ID_PICTURE_BOX(tab_index).Height),
                                                     New Point(Obj.SecndPointOfLine.X * ID_PICTURE_BOX(tab_index).Width, Obj.SecndPointOfLine.Y * ID_PICTURE_BOX(tab_index).Height), m_pt2)
                DrawLengthBetweenLines(ID_PICTURE_BOX(tab_index), obj_selected2, CDbl(Delta.Width / ID_PICTURE_BOX(tab_index).Width), CDbl(Delta.Height / ID_PICTURE_BOX(tab_index).Height), origin_image(tab_index).Width, origin_image(tab_index).Height, digit, CF)
            End If
        Else    'mouse is not clicked

            If sel_index >= 0 Then
                ID_PICTURE_BOX(tab_index).HightLightItem(object_list.ElementAt(tab_index).ElementAt(sel_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, CF)
                ID_PICTURE_BOX(tab_index).DrawObjSelected(object_list.ElementAt(tab_index).ElementAt(sel_index), True)
            End If

            If cur_measure_type >= 0 Then
                If cur_measure_type < MeasureType.C_Line Then
                    Dim temp As Point = New Point(e.X, e.Y)
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    ID_PICTURE_BOX(tab_index).DrawObjSelected(obj_selected, False)
                    ID_PICTURE_BOX(tab_index).DrawTempFinal(obj_selected, temp, side_drag, digit, CF, True)
                ElseIf cur_measure_type = MeasureType.C_Poly Then
                    'If PolyDrawEndFlag = False Then
                    If PolyPreviousPoint Is Nothing Then
                        PolyPreviousPoint = e.Location
                        Dim ptF = New PointF(e.X / CSng(ID_PICTURE_BOX(tab_index).Width), e.Y / CSng(ID_PICTURE_BOX(tab_index).Height))
                        C_PolyObj.PolyPoint(C_PolyObj.PolyPointIndx) = ptF
                    Else
                        If C_PolyObj.PolyPointIndx >= 1 Then
                            ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                            DrawPolyObj(ID_PICTURE_BOX(tab_index), line_infor, C_PolyObj)
                            DrawLineBetweenTwoPoints(ID_PICTURE_BOX(tab_index), line_infor, PolyPreviousPoint.Value, e.Location)
                        End If
                    End If
                    'End If
                ElseIf cur_measure_type = MeasureType.C_CuPoly Then
                    If CuPolyDrawEndFlag = False Then
                        Dim temp As Point
                        If C_CuPolyObj.CuPolyPointIndx_j > 0 Then
                            Dim tempF = C_CuPolyObj.CuPolyPoint(C_CuPolyObj.CuPolyPointIndx_j, C_CuPolyObj.CuPolyPointIndx_k(C_CuPolyObj.CuPolyPointIndx_j))
                            temp = New Point(tempF.X * ID_PICTURE_BOX(tab_index).Width, tempF.Y * ID_PICTURE_BOX(tab_index).Height)
                        Else
                            temp = dumyPoint
                        End If
                        If temp <> dumyPoint Then
                            ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                            DrawCuPolyObj(ID_PICTURE_BOX(tab_index), line_infor, C_CuPolyObj)
                            DrawLineBetweenTwoPoints(ID_PICTURE_BOX(tab_index), line_infor, temp, e.Location)
                        End If
                    End If
                ElseIf cur_measure_type = MeasureType.C_Sel Then
                    curve_sel_index = CheckCurveItemInPos(ID_PICTURE_BOX(tab_index), m_pt, object_list.ElementAt(tab_index))
                    If curve_sel_index >= 0 Then
                        Dim obj = object_list.ElementAt(tab_index).ElementAt(curve_sel_index)
                        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                        DrawCurveObjSelected(ID_PICTURE_BOX(tab_index), obj, digit, CF)
                    End If
                End If
            End If

            If move_line Then
                curve_sel_index = CheckCurveItemInPos(ID_PICTURE_BOX(tab_index), m_pt, object_list.ElementAt(tab_index))
                If curve_sel_index >= 0 Then
                    Dim obj = object_list.ElementAt(tab_index).ElementAt(curve_sel_index)
                    ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
                    DrawCurveObjSelected(ID_PICTURE_BOX(tab_index), obj, digit, CF)
                End If
            End If
        End If
    End Sub

    'select annotation
    Private Sub ID_PICTURE_BOX_MouseDoubleClick(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim m_pt As PointF = New Point()
        m_pt.X = CSng(e.X) / ID_PICTURE_BOX(tab_index).Width
        m_pt.Y = CSng(e.Y) / ID_PICTURE_BOX(tab_index).Height
        m_pt.X = Math.Min(Math.Max(m_pt.X, 0), 1)
        m_pt.Y = Math.Min(Math.Max(m_pt.Y, 0), 1)

        Dim an_num = CheckAnnotation(m_pt, object_list.ElementAt(tab_index), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
        If an_num >= 0 AndAlso Enumerable.ElementAt(Of MeasureObject)(Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, tab_index), an_num).measure_type = MeasureType.annotation Then
            ID_MY_TEXTBOX(tab_index).Font = font_infor.text_font
            ID_MY_TEXTBOX(tab_index).EnableTextBox(object_list.ElementAt(tab_index).ElementAt(an_num), ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height, left_top, scroll_pos)
            anno_num = an_num
        End If
    End Sub

    'draw objects to picturebox when ID_FORM_BRIGHTNESS is actived
    Private Sub ID_PICTURE_BOX_Paint(ByVal sender As Object, ByVal e As PaintEventArgs)
        If redraw_flag Then ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, True)
    End Sub

    'change the size of textbox when the text is changed
    Private Sub ID_MY_TEXTBOX_TextChanged(sender As Object, e As EventArgs)
        Dim textBox = CType(sender, TextBox)
        Dim numberOfLines = SendMessage(textBox.Handle.ToInt32(), EM_GETLINECOUNT, 0, 0)
        textBox.Height = (textBox.Font.Height + 2) * numberOfLines
    End Sub

    'set tab_index
    'reload image and object list
    Private Sub ID_TAG_CTRL_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ID_TAG_CTRL.SelectedIndexChanged
        Dim tab_name As String = ID_TAG_CTRL.SelectedTab.Name
        tab_name = tab_name.Substring(11)
        tab_index = CInt(tab_name)
        ID_MY_TEXTBOX(tab_index).Visible = False
        left_top = ID_PICTURE_BOX(tab_index).CenteringImage(ID_PANEL(tab_index))
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
        obj_selected.Refresh()
        cur_measure_type = -1
        sel_index = -1
        curve_sel_index = -1
        initVar()
    End Sub

    'set brightness, contrast and gamma to current image
    Private Sub ID_BTN_BRIGHTNESS_Click(sender As Object, e As EventArgs) Handles ID_BTN_BRIGHTNESS.Click
        redraw_flag = True
        Dim ratio = zoom_factor(tab_index)
        Dim zoomed = ZoomImage(ratio, resized_image, current_image, tab_index)
        Dim Image = zoomed.ToBitmap()
        Dim form As ID_FORM_BRIGHTNESS = New ID_FORM_BRIGHTNESS(ID_PICTURE_BOX(tab_index), Image, brightness(tab_index), contrast(tab_index), gamma(tab_index))
        'Dim image = origin_image(tab_index).Clone().ToBitmap()

        Dim InitialImage = AdjustBrightnessAndContrast(Image, brightness(tab_index), contrast(tab_index), gamma(tab_index))
        If form.ShowDialog() = DialogResult.OK Then
            brightness(tab_index) = form.brightness
            contrast(tab_index) = form.contrast
            gamma(tab_index) = form.gamma
            'current_image(tab_index) = GetMatFromSDImage(ID_PICTURE_BOX(tab_index).Image)

            'Dim UpdatedImage = AdjustBrightnessAndContrast(image, brightness(tab_index), contrast(tab_index), gamma(tab_index))
            'origin_image(tab_index) = GetMatFromSDImage(UpdatedImage)
        Else
            ID_PICTURE_BOX(tab_index).Image = form.InitialImage
            'origin_image(tab_index) = GetMatFromSDImage(InitialImage)
        End If
        redraw_flag = False
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
    End Sub

    'add new tab
    Private Sub Add_Tab()
        If tab_index >= 24 Then
            Return
        End If

        tab_index += 1
        If tag_page_flag(tab_index) = False Then
            tag_page_flag(tab_index) = True

            ID_PICTURE_BOX(tab_index).Image = Nothing
            current_image(tab_index) = Nothing
            resized_image(tab_index) = Nothing
            origin_image(tab_index) = Nothing
            cur_obj_num(tab_index) = 0
            Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, tab_index).Clear()
            brightness(tab_index) = 0
            contrast(tab_index) = 0
            gamma(tab_index) = 100
            img_import_flag(tab_index) = True

            ID_TAG_CTRL.TabPages.Add(ID_TAG_PAGE(tab_index))
            ID_TAG_CTRL.SelectedTab = ID_TAG_PAGE(tab_index)
        End If
    End Sub

    'add tag page at the end
    Private Sub ID_BTN_TAB_ADD_Click(sender As Object, e As EventArgs) Handles ID_BTN_TAB_ADD.Click
        Add_Tab()
        ID_STATUS_LABEL.Text = "Add tab."
    End Sub

    'remove tab
    Private Sub Remove_Tab()
        If tab_index < 0 Then
            Return
        End If

        If tag_page_flag(tab_index) = True Then

            current_image(tab_index) = Nothing
            resized_image(tab_index) = Nothing
            origin_image(tab_index) = Nothing
            cur_obj_num(tab_index) = 0
            Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, tab_index).Clear()
            brightness(tab_index) = 0
            contrast(tab_index) = 0
            gamma(tab_index) = 100
            img_import_flag(tab_index) = True
            ID_PICTURE_BOX(tab_index).Image = Nothing

            If tab_index = 0 Then
                ID_TAG_CTRL.SelectedIndex = 0
            Else
                tag_page_flag(tab_index) = False
                Dim cur_index = ID_TAG_CTRL.SelectedIndex
                ID_TAG_CTRL.TabPages.Remove(ID_TAG_PAGE(tab_index))
                ID_TAG_CTRL.SelectedIndex = cur_index - 1
            End If

        End If
    End Sub
    'remove last tag page
    Private Sub ID_BTN_TAB_REMOVE_Click(sender As Object, e As EventArgs) Handles ID_BTN_TAB_REMOVE.Click
        Remove_Tab()
        ID_STATUS_LABEL.Text = "Remove tab."
    End Sub

    Private Sub ADDTAGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ADDTAGToolStripMenuItem.Click
        Add_Tab()
        ID_STATUS_LABEL.Text = "Add tab."
    End Sub

    Private Sub REMOVETAGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles REMOVETAGToolStripMenuItem.Click
        Remove_Tab()
        ID_STATUS_LABEL.Text = "Remove tab."
    End Sub

    'save setting information to setting.ini
    'close camera
    Private Sub Main_Form_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        On Error Resume Next

        If IO.File.Exists(ini_path) Then
            If ini IsNot Nothing Then
                ini.ChangeValue("unit", "Config", scale_unit)
                ini.ChangeValue("digit", "Config", digit.ToString())
                ini.ChangeValue("index", "CF", ID_COMBOBOX_CF.SelectedIndex)
                ini.Save(ini_path)
            End If
        Else
            'set default value when ini file does Not exist in document folder
            ini = New IniFile(ini_path)
            ini.AddSection("Config")
            ini.AddKey("unit", scale_unit, "Config")
            ini.AddKey("digit", digit.ToString(), "Config")

            ini.AddSection("CF")
            ini.AddKey("index", ID_COMBOBOX_CF.SelectedIndex, "CF")
            For i = 0 To CF_list.Count - 1
                Dim key = "No" & (i + 1)
                Dim value = CF_list(i) & ":" & CF_num(i)
                ini.AddKey(key, value, "CF")
            Next
            ini.AddSection("name")
            ini.AddKey("No1", "Line", "name")
            ini.AddKey("No2", "Angle", "name")
            ini.AddKey("No3", "Arc", "name")
            ini.AddKey("No4", "Scale", "name")
            ini.Sort()
            ini.Save(ini_path)
        End If

        If videoDevice Is Nothing Then
        ElseIf videoDevice.IsRunning Then
            videoDevice.SignalToStop()
            RemoveHandler videoDevice.NewFrame, New NewFrameEventHandler(AddressOf Device_NewFrame)
            videoDevice = Nothing
        End If
        camera_state = False
    End Sub

    'Data grid events make combobox column editable
    Private Sub ID_LISTVIEW_EditingControlShowing(sender As Object, e As DataGridViewEditingControlShowingEventArgs) Handles ID_LISTVIEW.EditingControlShowing

        If ID_LISTVIEW.CurrentCell Is ID_LISTVIEW(0, 0) Then
            Dim cb As ComboBox = TryCast(e.Control, ComboBox)

            If cb IsNot Nothing Then
                RemoveHandler cb.SelectedIndexChanged, AddressOf CB_SelectedIndexChanged

                ' Following line needed for initial setup.
                cb.DropDownStyle = If(cb.SelectedIndex = 0, ComboBoxStyle.DropDown, ComboBoxStyle.DropDownList)

                AddHandler cb.SelectedIndexChanged, AddressOf CB_SelectedIndexChanged
            End If
        End If

    End Sub

    'make only first item of combobox item of datagridview editable
    Private Sub CB_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim cb As ComboBox = TryCast(sender, ComboBox)
        cb.DropDownStyle = If(cb.SelectedIndex = 0, ComboBoxStyle.DropDown, ComboBoxStyle.DropDownList)
    End Sub

    'update first and fifth item of datagridview and update the object 
    Private Sub ID_LISTVIEW_CellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs) Handles ID_LISTVIEW.CellValidating
        If e.ColumnIndex = 0 Then

            Dim cell = TryCast(ID_LISTVIEW.Rows(e.RowIndex).Cells(e.ColumnIndex), DataGridViewComboBoxCell)

            If cell IsNot Nothing AndAlso Not Equals(e.FormattedValue.ToString(), String.Empty) AndAlso Not cell.Items.Contains(e.FormattedValue) Then
                cell.Items(0) = e.FormattedValue

                If ID_LISTVIEW.IsCurrentCellDirty Then
                    ID_LISTVIEW.CommitEdit(DataGridViewDataErrorContexts.Commit)
                End If

                cell.Value = e.FormattedValue
                Dim obj_list = object_list.ElementAt(tab_index)
                Dim obj = obj_list.ElementAt(e.RowIndex)
                obj.name = cell.Value
                obj_list(e.RowIndex) = obj
                object_list(tab_index) = obj_list

            End If
        ElseIf e.ColumnIndex = 5 Then
            Dim cell = TryCast(ID_LISTVIEW.Rows(e.RowIndex).Cells(e.ColumnIndex), DataGridViewTextBoxCell)

            If cell IsNot Nothing AndAlso Not Equals(e.FormattedValue.ToString(), String.Empty) Then

                If ID_LISTVIEW.IsCurrentCellDirty Then
                    ID_LISTVIEW.CommitEdit(DataGridViewDataErrorContexts.Commit)
                End If

                cell.Value = e.FormattedValue
                Dim obj_list = object_list.ElementAt(tab_index)
                Dim obj = obj_list.ElementAt(e.RowIndex)
                obj.remarks = cell.Value
                obj_list(e.RowIndex) = obj
                object_list(tab_index) = obj_list

            End If
        End If
    End Sub

    'handles exception for datagridview
    Private Sub ID_LISTVIEW_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles ID_LISTVIEW.DataError
        If e.ColumnIndex = 0 AndAlso e.RowIndex = 0 Then
            e.Cancel = True
        End If
    End Sub

    'update CF value
    'redraw objects
    'reload object list to datagridView
    Private Sub ID_COMBOBOX_CF_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ID_COMBOBOX_CF.SelectedIndexChanged
        Dim index = ID_COMBOBOX_CF.SelectedIndex()
        CF = CF_num(index)
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
        ID_STATUS_LABEL.Text = "Changing CF."
    End Sub

    'show About dialog
    Private Sub ID_MENU_ABOUT_Click(sender As Object, e As EventArgs) Handles ID_MENU_ABOUT.Click
        Dim ad As New About
        If ad.ShowDialog() = DialogResult.OK Then

        End If
    End Sub

    'side dragging for small thickness when ID_CHECK_SIDE is checked
    Private Sub ID_CHECK_SIDE_CheckedChanged(sender As Object, e As EventArgs) Handles ID_CHECK_SIDE.CheckedChanged
        If ID_CHECK_SIDE.Checked = True Then
            side_drag = True
        Else
            side_drag = False
        End If
    End Sub

    'show legend when ID_CHECK_SHOW_LEGEND is checked
    Private Sub ID_CHECK_SHOW_LEGEND_CheckedChanged(sender As Object, e As EventArgs) Handles ID_CHECK_SHOW_LEGEND.CheckedChanged
        If ID_CHECK_SHOW_LEGEND.Checked = True Then
            show_legend = True
        Else
            show_legend = False
        End If
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
    End Sub

    'display setting.ini
    Private Sub ID_MENU_SETTING_INFO_Click(sender As Object, e As EventArgs) Handles ID_MENU_SETTING_INFO.Click

        Try
            Dim alive As System.Diagnostics.Process
            If System.IO.File.Exists(ini_path) = True Then
                alive = Process.Start(ini_path)
            Else
                'set default value when ini file does Not exist in document folder
                ini = New IniFile(ini_path)
                ini.AddSection("Config")
                ini.AddKey("unit", scale_unit, "Config")
                ini.AddKey("digit", digit.ToString(), "Config")

                ini.AddSection("CF")
                ini.AddKey("index", ID_COMBOBOX_CF.SelectedIndex, "CF")
                For i = 0 To CF_list.Count - 1
                    Dim key = "No" & (i + 1)
                    Dim value = CF_list(i) & ":" & CF_num(i)
                    ini.AddKey(key, value, "CF")
                Next
                ini.AddSection("name")
                ini.AddKey("No1", "Line", "name")
                ini.AddKey("No2", "Angle", "name")
                ini.AddKey("No3", "Arc", "name")
                ini.AddKey("No4", "Scale", "name")
                ini.Sort()
                ini.Save(ini_path)
                alive = Process.Start(ini_path)
            End If

            alive.WaitForExit()
            name_list.Clear()
            ID_COMBOBOX_CF.Items.Clear()
            CF_list.Clear()
            CF_num.Clear()
            GetInforFromIni()
        Catch ex As Exception
            MessageBox.Show(ex.Message.ToString())
        End Try
    End Sub
#End Region

#Region "Webcam Methods"

    'pop one frame from webcam and display it to pictureboxs
    Public Sub Device_NewFrame(sender As Object, eventArgs As AForge.Video.NewFrameEventArgs)
        On Error Resume Next

        Me.Invoke(Sub()
                      newImage = DirectCast(eventArgs.Frame.Clone(), Bitmap)

                      If flag = False Then
                          ID_PICTURE_BOX(0).Image = newImage.Clone()
                      End If
                      ID_PICTURE_BOX_CAM.Image = newImage.Clone()
                      newImage?.Dispose()
                  End Sub)

    End Sub

    'open camera
    Private Sub OpenCamera()
        Dim cameraInt As Int32 = CheckPerticularCamera(videoDevices, _devicename)
        If (cameraInt < 0) Then
            MessageBox.Show("Compatible Camera not found..")
            Exit Sub
        End If

        videoDevices = New FilterInfoCollection(FilterCategory.VideoInputDevice)
        videoDevice = New VideoCaptureDevice(videoDevices(Convert.ToInt32(cameraInt)).MonikerString)
        If Not My.Settings.camresindex.Equals("") Then
            videoDevice.VideoResolution = videoDevice.VideoCapabilities(Convert.ToInt32(My.Settings.camresindex))
        End If
        AddHandler videoDevice.NewFrame, New NewFrameEventHandler(AddressOf Device_NewFrame)
        videoDevice.Start()
        camera_state = True
    End Sub

    'close camera
    Private Sub CloseCamera()

        If videoDevice Is Nothing Then
        ElseIf videoDevice.IsRunning Then
            videoDevice.SignalToStop()
            RemoveHandler videoDevice.NewFrame, New NewFrameEventHandler(AddressOf Device_NewFrame)
            videoDevice.Source = Nothing
        End If
        camera_state = False
    End Sub

    'capture image and add it to ID_LISTVIEW_IMAGE
    Private Sub ID_BTN_CAPTURE_Click(sender As Object, e As EventArgs) Handles ID_BTN_CAPTURE.Click

        Try

            If ID_PICTURE_BOX_CAM.Image Is Nothing Then
                Return
            End If
            Dim img1 As Image = ID_PICTURE_BOX_CAM.Image.Clone()

            Createdirectory(imagepath)
            If photoList.Images.Count <= 0 Then
                file_counter = photoList.Images.Count + 1
            Else
                file_counter = Convert.ToInt32(IO.Path.GetFileNameWithoutExtension(photoList.Images.Keys.Item(photoList.Images.Count - 1).ToString()).Split("_")(1)) + 1
            End If

            img1.Save(imagepath & "\\test_" & (file_counter) & ".jpeg", Imaging.ImageFormat.Jpeg)
            photoList.ImageSize = New Size(160, 120)
            photoList.Images.Add("\\test_" & (file_counter) & ".jpeg", img1)
            ID_LISTVIEW_IMAGE.LargeImageList = photoList
            'img1.Dispose()
            ID_LISTVIEW_IMAGE.Items.Clear()
            For index = 0 To photoList.Images.Count - 1
                Dim item As New ListViewItem With {
                    .ImageIndex = index,
                        .Tag = imagepath & photoList.Images.Keys.Item(index).ToString(),
                        .Text = IO.Path.GetFileNameWithoutExtension(photoList.Images.Keys.Item(index).ToString())
                }
                ID_LISTVIEW_IMAGE.Items.Add(item)
            Next

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

    End Sub

    'clear all image list and items in ID_LISTVIEW_CAM
    Private Sub ID_BTN_CLEAR_ALL_Click(sender As Object, e As EventArgs) Handles ID_BTN_CLEAR_ALL.Click
        file_counter = 0
        ID_LISTVIEW_IMAGE.Clear()
        ID_LISTVIEW_IMAGE.Items.Clear()
        photoList.Images.Clear()
        ID_PICTURE_BOX_CAM.Image = Nothing
        ID_PICTURE_BOX(tab_index).Image = Nothing
        DeleteImages(imagepath)
    End Sub

    'stop camera and display the selected image to ID_PICTURE_BOX
    Private Sub ID_LISTVIEW_IMAGE_DoubleClick(sender As Object, e As EventArgs) Handles ID_LISTVIEW_IMAGE.DoubleClick
        Try
            flag = True

            Dim itemSelected As Integer = GetListViewSelectedItemIndex(ID_LISTVIEW_IMAGE)
            SetListViewSelectedItem(ID_LISTVIEW_IMAGE, itemSelected)
            Dim Image As Image = Image.FromFile(ID_LISTVIEW_IMAGE.SelectedItems(0).Tag)
            ID_PICTURE_BOX_CAM.Image = Image

            Dim page_num = tab_index

            If tab_index = 0 Or img_import_flag(tab_index) = False Then
                For i = 1 To 24
                    If img_import_flag(i) = True Then
                        page_num = i
                        tag_page_flag(i) = True

                        ID_PICTURE_BOX(i).Image = Nothing
                        current_image(i) = Nothing
                        resized_image(i) = Nothing
                        origin_image(i) = Nothing
                        cur_obj_num(i) = 0
                        Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, i).Clear()
                        brightness(i) = 0
                        contrast(i) = 0
                        gamma(i) = 100

                        ID_TAG_CTRL.TabPages.Add(ID_TAG_PAGE(i))
                        Exit For
                    End If
                Next
            Else
                page_num = tab_index
            End If

            ID_PICTURE_BOX(page_num).LoadImageFromFile(ID_LISTVIEW_IMAGE.SelectedItems(0).Tag, origin_image, resized_image,
                                                         initial_ratio, page_num)

            Dim img = resized_image.ElementAt(page_num)

            ID_PICTURE_BOX(page_num).Image = img.ToBitmap()

            left_top = ID_PICTURE_BOX(page_num).CenteringImage(ID_PANEL(page_num))

            current_image(page_num) = img
            cur_obj_num(page_num) = 0
            Enumerable.ElementAt(Of List(Of MeasureObject))(object_list, page_num).Clear()
            brightness(page_num) = 0
            contrast(page_num) = 0
            gamma(page_num) = 100
            img_import_flag(page_num) = False
            ID_LISTVIEW.LoadObjectList(object_list.ElementAt(page_num), CF, digit, scale_unit, name_list)

            ID_TAG_CTRL.SelectedTab = ID_TAG_PAGE(page_num)
        Catch ex As Exception
            MessageBox.Show(ex.ToString())
        End Try
    End Sub

    'display property window for the video capture
    Private Sub Btn_CameraProperties_Click(sender As Object, e As EventArgs) Handles Btn_CameraProperties.Click

        If videoDevice Is Nothing Then
            MsgBox("Please start Camera First")

        ElseIf videoDevice.IsRunning Then
            videoDevice.DisplayPropertyPage(Me.Handle)
        End If
    End Sub

    'set flag for live image so that live images can be displayed to tab
    Private Sub btn_live_Click(sender As Object, e As EventArgs) Handles btn_live.Click
        flag = False

    End Sub

    'change the resolution of webcam
    Private Sub CameraResolutionsCB_SelectedIndexChanged(sender As Object, e As EventArgs) Handles CameraResolutionsCB.SelectedIndexChanged

        If CameraResolutionsCB.SelectedIndex > 0 Then
            My.Settings.camresindex = CameraResolutionsCB.SelectedIndex - 1
            My.Settings.Save()
            CloseCamera()
            Threading.Thread.Sleep(500)
            OpenCamera()
        End If

    End Sub

    'set the path of directory for captured images
    Private Sub btn_setpath_Click(sender As Object, e As EventArgs) Handles btn_setpath.Click
        Dim dialog = New FolderBrowserDialog With {
            .SelectedPath = Application.StartupPath
        }
        If DialogResult.OK = dialog.ShowDialog() Then
            txtbx_imagepath.Text = dialog.SelectedPath & "\MyImages"
            imagepath = txtbx_imagepath.Text
            My.Settings.imagefilepath = imagepath
            My.Settings.Save()
            Createdirectory(imagepath)
        End If
    End Sub

    'delete all captured images
    Private Sub btn_delete_Click(sender As Object, e As EventArgs) Handles btn_delete.Click

        For Each v As ListViewItem In ID_LISTVIEW_IMAGE.SelectedItems
            ID_LISTVIEW_IMAGE.Items.Remove(v)
            photoList.Images.RemoveAt(v.ImageIndex)
            Dim FileDelete As String = v.Tag
            If File.Exists(FileDelete) = True Then
                File.Delete(FileDelete)
            End If
        Next

    End Sub

    'open browser and load captured images
    Private Sub btn_browse_Click(sender As Object, e As EventArgs) Handles btn_browse.Click
        Dim ofd As New OpenFileDialog With {
            .Filter = "Image File (*.ico;*.jpg;*.jpeg;*.bmp;*.gif;*.png)|*.jpg;*.jpeg;*.bmp;*.gif;*.png;*.ico",
            .Multiselect = True,
            .FilterIndex = 1
        }

        If ofd.ShowDialog() = DialogResult.OK Then
            Try
                Dim files As String() = ofd.FileNames
                For Each file In files
                    Dim img1 As New Bitmap(file)
                    Createdirectory(imagepath)
                    If photoList.Images.Count <= 0 Then
                        file_counter = photoList.Images.Count + 1
                    Else
                        file_counter = Convert.ToInt32(IO.Path.GetFileNameWithoutExtension(photoList.Images.Keys.Item(photoList.Images.Count - 1).ToString()).Split("_")(1)) + 1
                    End If

                    img1.Save(imagepath & "\\test_" & (file_counter) & ".jpeg", Imaging.ImageFormat.Jpeg)
                    photoList.ImageSize = New Size(200, 150)
                    photoList.Images.Add("\\test_" & (file_counter) & ".jpeg", img1)
                    ID_LISTVIEW_IMAGE.LargeImageList = photoList
                    img1.Dispose()
                    ID_LISTVIEW_IMAGE.Items.Clear()
                    For index = 0 To photoList.Images.Count - 1
                        Dim item As New ListViewItem With {
                        .ImageIndex = index,
                            .Tag = imagepath & photoList.Images.Keys.Item(index).ToString(),
                            .Text = IO.Path.GetFileNameWithoutExtension(photoList.Images.Keys.Item(index).ToString())
                    }
                        ID_LISTVIEW_IMAGE.Items.Add(item)
                    Next

                Next

            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If


    End Sub
#End Region

#Region "Keygen Methods"
    'check for license
    Private Sub Init()
        licState = licGen.getLicState(licModel)
        MessageBox.Show("License State : " + licState.ToString)
        If licState = licState.Success Then
            ID_GROUP_BOX_CONTROL.Enabled = True
        Else
            ID_GROUP_BOX_CONTROL.Enabled = False
        End If
        path = Application.StartupPath + "img\image1.jpg"
    End Sub

    'show activate dialog
    Private Sub ID_MENU_ACTIVATE_Click(sender As Object, e As EventArgs) Handles ID_MENU_ACTIVATE.Click
        Dim ad As New ActiveInfo
        If ad.ShowDialog() = DialogResult.OK Then
            Dim OfDLicense As New OpenFileDialog()
            OfDLicense.Filter = "License (*.lic)|*.lic"
            OfDLicense.FilterIndex = 1
            OfDLicense.RestoreDirectory = True
            Dim dest As String = System.IO.Path.Combine(Application.StartupPath, licpath)
            If OfDLicense.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Dim file = New FileInfo(OfDLicense.FileName)
                file = file.CopyTo(dest, True)

            End If
        End If


        Init()
    End Sub

    'show license info dialog
    Private Sub ID_MENU_LICENSE_INFO_Click(sender As Object, e As EventArgs) Handles ID_MENU_LICENSE_INFO.Click

        Dim ld As New LicInfo()
        ld.mParent = Me
        If licState = licState.NoFile Or licState = licState.Incorrect Then
            ld.bInfo = False
            ld.serial = licGen.getSn
            ld.machine = licGen.getMn

        Else
            ld.bInfo = True
            ld.serial = licModel.sn
            ld.machine = licModel.mn
            ld.customer = licModel.cname
            ld.email = licModel.cmail

        End If

        ld.ShowDialog()
    End Sub
#End Region

#Region "Curves Methods"
    Private Sub ID_MENU_TO_CURVES_Click(sender As Object, e As EventArgs) Handles ID_MENU_TO_CURVES.Click

        If System.IO.File.Exists(exe_path) = True Then
            Try
                ID_PICTURE_BOX(tab_index).SaveImageForCurves(ToCurveImg_path, object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF)
                Dim alive = Process.Start(exe_path)

                alive.WaitForExit()
                ID_PICTURE_BOX(tab_index).LoadImageFromFile(ReturnedImg_path, origin_image, resized_image,
                                                         initial_ratio, tab_index)
                Dim img = resized_image.ElementAt(tab_index)
                ID_PICTURE_BOX(tab_index).Image = img.ToBitmap()
                left_top = ID_PICTURE_BOX(tab_index).CenteringImage(ID_PANEL(tab_index))
                current_image(tab_index) = img
                AppendDataToObjList(ReturnedTxt_path, object_list(tab_index), cur_obj_num(tab_index))
                ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)

            Catch ex As Exception

                MessageBox.Show(ex.Message.ToString())
            End Try
        End If

    End Sub


    ''' <summary>
    ''' set current measurement type as C_Line
    ''' </summary>
    Private Sub ID_BTN_C_LINE_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_LINE.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Line
        obj_selected.measure_type = cur_measure_type

    End Sub
    Private Sub LINEToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles LINEToolStripMenuItem1.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Line
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' set current measurement type as C_Poly
    ''' </summary>
    Private Sub ID_BTN_C_POLY_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_POLY.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Poly
        obj_selected.measure_type = cur_measure_type

    End Sub

    Private Sub POLYGENToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles POLYGENToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Poly
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' set current measurement type as C_Point
    ''' </summary>
    Private Sub ID_BTN_C_POINT_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_POINT.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Point
        obj_selected.measure_type = cur_measure_type

    End Sub

    Private Sub POINTToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles POINTToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Point
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' set current measurement type as C_Curve
    ''' </summary>
    Private Sub ID_BTN_C_CURVE_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_CURVE.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Curve
        obj_selected.measure_type = cur_measure_type

    End Sub

    Private Sub CURVEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CURVEToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Curve
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' set current measurement type as C_Cupoly
    ''' </summary>
    Private Sub ID_BTN_C_CUPOLY_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_CUPOLY.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_CuPoly
        obj_selected.measure_type = cur_measure_type

    End Sub

    Private Sub CURVEPOLYGENToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CURVEPOLYGENToolStripMenuItem.Click
        menu_click = True
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_CuPoly
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' set current measurement type as C_Sel
    ''' </summary>
    Private Sub ID_BTN_C_SEL_Click(sender As Object, e As EventArgs) Handles ID_BTN_C_SEL.Click
        menu_click = False
        obj_selected.Refresh()
        cur_measure_type = MeasureType.C_Sel
        obj_selected.measure_type = cur_measure_type

    End Sub

    ''' <summary>
    ''' Add Curve object to obj list
    ''' </summary>
    Private Sub AddCurveToList()
        AddMaxMinToList()

        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
    End Sub

    Private Sub AddMaxMinToList()
        obj_selected.obj_num = cur_obj_num(tab_index)
        SetLineAndFont(obj_selected, line_infor, font_infor)
        object_list(tab_index).Add(obj_selected)
        obj_selected.Refresh()
        cur_measure_type = -1
        cur_obj_num(tab_index) += 1
        If undo_num < 2 Then undo_num += 1
    End Sub

    ''' <summary>
    ''' calculate minimum distance between two selected objects
    ''' </summary>
    Private Sub MinCalcBtn_Click(sender As Object, e As EventArgs) Handles MinCalcBtn.Click
        ID_STATUS_LABEL.Text = "Calculate minimum distance between selected objects."

        If CuPolyRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcMinBetweenCuPolyAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CuPolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMinBetweenCuPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcMinBetweenCurveAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMinBetweenCurveAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcMinBetweenPointAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcMinBetweenPointAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcMinBetweenLineAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcMinBetweenCurveAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If

        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
    End Sub

    ''' <summary>
    ''' calculate maximum distance between two selected objects
    ''' </summary>
    Private Sub MaxCalcBtn_Click(sender As Object, e As EventArgs) Handles MaxCalcBtn.Click
        ID_STATUS_LABEL.Text = "Calculate maximum distance between selected objects."

        If CuPolyRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenCuPolyAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CuPolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenCuPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenCurveAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenCurveAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenCurveAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenLineAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenLineAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcMaxBetweenPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
    End Sub

    ''' <summary>
    ''' calculate minimum perpendicular distance between two selected objects
    ''' </summary>
    Private Sub PerMin_Click(sender As Object, e As EventArgs) Handles PerMin.Click
        ID_STATUS_LABEL.Text = "Calculate perpendicular minimum distance between selected objects."

        If CuPolyRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenCuPolyAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CuPolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenCuPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenCurveAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenCurveAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenPointAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenPointAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenLineAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcPMinBetweenCurveAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If

        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
    End Sub

    ''' <summary>
    ''' calculate maximum perpendicular distance between two selected objects
    ''' </summary>
    Private Sub PerMax_Click(sender As Object, e As EventArgs) Handles PerMax.Click
        ID_STATUS_LABEL.Text = "Calculate perpendicular maximum distance between selected objects."

        If CuPolyRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenCuPolyAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CuPolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CuPolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenCuPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And LRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenCurveAndLine(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenCurveAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If CRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(CRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenCurveAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenLineAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If LRealSelectArrayIndx >= 0 And PolyRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(LRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenLineAndPoly(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        If PolyRealSelectArrayIndx >= 0 And PRealSelectArrayIndx >= 0 Then
            Dim obj1 = object_list.ElementAt(tab_index).ElementAt(PolyRealSelectArrayIndx)
            Dim obj2 = object_list.ElementAt(tab_index).ElementAt(PRealSelectArrayIndx)
            obj_selected = CalcPMaxBetweenPolyAndPoint(obj1, obj2, ID_PICTURE_BOX(tab_index).Width, ID_PICTURE_BOX(tab_index).Height)
            AddMaxMinToList()
        End If
        ID_PICTURE_BOX(tab_index).DrawObjList(object_list.ElementAt(tab_index), graphPen, graphPen_line, digit, CF, False)
        ID_LISTVIEW.LoadObjectList(object_list.ElementAt(tab_index), CF, digit, scale_unit, name_list)
    End Sub
#End Region

#Region "Segmentation Tool"
    Private Sub CIRCLEDETECTIONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CIRCLEDETECTIONToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.circle
        Dim form = New Circle()
        form.Show()
    End Sub

    Private Sub INTERSECTIONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles INTERSECTIONToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.intersection
        Dim form = New Intersection()
        form.Show()
    End Sub

    Private Sub PHASESEGMENTATIONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PHASESEGMENTATIONToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.phaseSegmentation
        Dim form = New Phase_Segmentation()
        form.Show()
    End Sub

    Private Sub COUNTCLASSIFICATIONToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles COUNTCLASSIFICATIONToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.BlobSegment
        Dim form = New Count_Classification()
        form.Show()
    End Sub

    Private Sub PARTICIPLESIZEToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PARTICIPLESIZEToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.BlobSegment
        Dim form = New ParticipleSize()
        form.Show()
    End Sub

    Private Sub NODULARITYToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NODULARITYToolStripMenuItem.Click
        Obj_Seg.Refresh()
        Obj_Seg.measureType = SegType.BlobSegment
        Dim form = New Nodularity()
        form.Show()
    End Sub
#End Region
End Class
