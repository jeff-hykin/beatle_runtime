//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Beatle_Defense_Kinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;
    using System.Windows.Input;
    using System.Timers;
    using System.Net;  
    using System.Text;
    using Newtonsoft.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public dynamic systemData = null;
        public string postData = "{ \"dummyData\": \"kinectIsRunning\" }";
        
        public dynamic font = new Typeface("Helvetica");
        public dynamic culture_info = CultureInfo.GetCultureInfo("en-us");
        
        /// <summary>
        /// Thickness of face bounding box and face points
        /// </summary>
        private const double DrawFaceShapeThickness = 8;

        /// <summary>
        /// Font size of face property text 
        /// </summary>
        private const double DrawTextFontSize = 10;

        /// <summary>
        /// Radius of face point circle
        /// </summary>
        private const double FacePointRadius = 1.0;

        /// <summary>
        /// Text layout offset in X axis
        /// </summary>
        private const float TextLayoutOffsetX = -0.1f;
        
        /// <summary>
        /// Text layout offset in Y axis
        /// </summary>
        private const float TextLayoutOffsetY = 0.25f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

                    /// <summary>
                    /// Reader for color frames
                    /// </summary>
                    private ColorFrameReader colorFrameReader = null;

                    /// <summary>
                    /// Bitmap to display
                    /// </summary>
                    private WriteableBitmap colorBitmap = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array to store bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Number of bodies tracked
        /// </summary>
        private int bodyCount;

        /// <summary>
        /// Width of display (color space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (color space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Display rectangle
        /// </summary>
        private Rect displayRect;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Array to store currently bodies
        /// </summary>
        private List<Body> bodies_active = new List<Body>();

        private int targetIndex = 0;

        //For Key Controls
        bool wait_for_left_key_up = false;
        bool wait_for_right_key_up = false;
        bool wait_for_space_key_up = false;

        //Servo Stuff
        private bool use_pan_tilt = false;
        public System.IO.Ports.SerialPort serialPort;

        //Targeting  Stuff
        private float target_degree_tolerance = 5.0f;
        private float movement_amount = 0.5f;

        private double current_x_degrees = 0;
        private double current_y_degrees = 0;

        //Room Searching Stuff
        private static System.Timers.Timer motion_cooldown_timer;
        private readonly float motion_detection_search_time = 15.0f;
        private float motion_cooldown_time_seconds;
        private bool motion_detected = false;

        private static System.Timers.Timer body_cooldown_timer;
        private readonly float body_detection_search_time = 5.0f;
        private float body_cooldown_time_seconds;
        private bool body_detected = false;
        
        //For Room Search Function - Will pan left of center first, before panning right of center
        private Boolean done_searching_left_side = false;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

                    // open the reader for the color frames
                    this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

                    // wire handler for frame arrival
                    this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

                    // create the colorFrameDescription from the ColorFrameSource using Bgra format
                    FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

                    // create the bitmap to display
                    this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);


            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get the color frame details
            //FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            this.displayRect = new Rect(0.0, 0.0, this.displayWidth, this.displayHeight);

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // wire handler for body frame arrival
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // set the maximum number of bodies that would be tracked by Kinect
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.bodyCount];

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();

            //Search Stuff
            motion_cooldown_time_seconds = motion_detection_search_time;
            body_cooldown_time_seconds = body_detection_search_time;
            
            // 
            // central_server stuff
            // 
            Task.Run(async () => {
                for(;;)
                {
                    await Task.Delay(1000); // once per second
                    SendPostRequest();
                }
            });
        }
        
        // this is automatically updated by the central server 
        public bool IsArmed
        {
            get
            {
                if (this.systemData != null)
                {
                    return this.systemData.status == "armed";
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // wire handler for body frame arrival
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }

            if(use_pan_tilt)
            {
                //Servo Stuff
                serialPort = new System.IO.Ports.SerialPort();

                // Close the serial port if it is already open
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                try
                {
                    // Configure our serial port *** You'll likely need to change these for your config! ***
                    serialPort.PortName = "COM6";
                    serialPort.BaudRate = 115200;
                    serialPort.Parity = System.IO.Ports.Parity.None;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = System.IO.Ports.StopBits.One;

                    //Now open the serial port
                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Couldn't open the Serial Port!");
                    Debug.WriteLine(ex.ToString());//Report the actual error
                    use_pan_tilt = false;
                }

                //Commands Pan-Tilt to Center itself when starting
                CommandServo(0, 0.0f, 100);
                CommandServo(1, 0.0f, 100);
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

                            if (this.colorFrameReader != null)
                            {
                                // ColorFrameReder is IDisposable
                                this.colorFrameReader.Dispose();
                                this.colorFrameReader = null;
                            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }

            if (use_pan_tilt)
            {
                //Servo Stuff
                //Closing serial port
                try
                {
                    for (int i = 0; i < 32; i++)
                    {
                        string command = "#" + i + "P0\r";
                        serialPort.Write(command);
                    }
                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Couldn't close the darned serial port. Here's what it said: " + Environment.NewLine);
                    Console.WriteLine(ex.ToString());
                }
            }

        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            // if the system is disarmed, basically do nothing
            if (!this.IsArmed)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawImage(this.colorBitmap, this.displayRect);
                }
            }
            else
            {
                using (var bodyFrame = e.FrameReference.AcquireFrame())
                {
                    if (bodyFrame != null)
                    {
                        // update body data
                        bodyFrame.GetAndRefreshBodyData(this.bodies);

                        using (DrawingContext dc = this.drawingGroup.Open())
                        {
                            dc.DrawImage(this.colorBitmap, this.displayRect);
                            // draw the dark background
                            // dc.DrawRectangle(Brushes.Black, null, this.displayRect);

                            //Used for counting bodies observed
                            bodies_active.Clear();

                            // iterate through each face source
                            for (int i = 0; i < this.bodyCount; i++)
                            {
                                if (bodies[i].IsTracked)
                                {
                                    Pen drawPen = new Pen(Brushes.White, 6);// = this.bodyColors[penIndex++];

                                    if (i < this.bodyCount)//Why do I have this???? -- Sebastion?     // idk -- Jeff
                                    {
                                        if (i == targetIndex)
                                        {
                                            drawPen = new Pen(Brushes.Red, 6);
                                        }
                                        else drawPen = new Pen(Brushes.White, 6);
                                    }

                                    //Populate bodies_active
                                    bodies_active.Add(bodies[i]);

                                    // draw face frame results
                                    this.DrawFaceFrameResults(i, dc);

                                    this.DrawClippedEdges(bodies[i], dc);

                                    IReadOnlyDictionary<JointType, Joint> joints = bodies[i].Joints;

                                    // convert the joint points to depth (display) space
                                    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                                    foreach (JointType jointType in joints.Keys)
                                    {
                                        // sometimes the depth(Z) of an inferred joint may show as negative
                                        // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                        CameraSpacePoint position = joints[jointType].Position;
                                        if (position.Z < 0)
                                        {
                                            position.Z = InferredZPositionClamp;
                                        }

                                        DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                        jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                                    }

                                    this.DrawBody(joints, jointPoints, dc, drawPen);
                                }

                                if (!this.bodies[targetIndex].IsTracked)//If curent body is target//Need to fix for selection code to work
                                {
                                    targetIndex = i;
                                }
                            } 
                            
                            var angleOfFace = Find_Angle_Of_Face(targetIndex);
                            var textToDisplay = $"Number of Bodies Detected = {bodies_active.Count}"+
                                                $"\nTargetIndex = {targetIndex}" + 
                                                $"\nTarget X-Angle from Camera : {angleOfFace.X}°" +
                                                $"\nTarget Y-Angle from Camera : {angleOfFace.Y}°";
                            
                            // Display the Number of bodies currently tracked
                            dc.DrawText(
                                new FormattedText(
                                    textToDisplay,
                                    this.culture_info,
                                    FlowDirection.LeftToRight,
                                    this.font,
                                    DrawTextFontSize,
                                    Brushes.White
                                ),
                                new Point(displayWidth/2 + 90, displayHeight - 50)
                            );

                            //Servo Tracking
                            if (use_pan_tilt)
                            {
                                //If not aiming close enough to target on y axis
                                if (Math.Abs(Find_Angle_Of_Face(targetIndex).Y) > 5)
                                {
                                    float amount = movement_amount;
                                    if (Find_Angle_Of_Face(targetIndex).Y > 0) amount *= -1;

                                    CommandServo(0, (float)(current_y_degrees + amount), 1000.0f);
                                }

                                //If not aiming close enough to target on x axis
                                if (Math.Abs(Find_Angle_Of_Face(targetIndex).X) > 1)
                                {
                                    float amount = movement_amount;
                                    if (Find_Angle_Of_Face(targetIndex).X < 0) amount *= -1;

                                    CommandServo(1, (float)(current_x_degrees + 0.1f*amount), 100.0f);
                                }

                                //If no bodies tracked for a LONG time then should search room.
                                //If no bodies tracked for a SHORT time then should stay where it is to hopefully catch the lost person.
                                if (bodies_active.Count < 1 && motion_detected && !body_detected) SearchRoom();
                                if (bodies_active.Count > 0) BodyDetected();

                                //Test Motion Simulation
                                if (Keyboard.IsKeyDown(Key.Space)) wait_for_space_key_up = true;

                                if (Keyboard.IsKeyUp(Key.Space) && wait_for_space_key_up)
                                {
                                    wait_for_space_key_up = false;
                                    MotionDetected();
                                }
                            }

                            //Shifting Tracking Target
                            if (bodies_active.Count > 1)
                            {
                                //LEFT SHIFT
                                if (Keyboard.IsKeyDown(Key.Left)) wait_for_left_key_up = true;

                                if (Keyboard.IsKeyUp(Key.Left) && wait_for_left_key_up)
                                {
                                    wait_for_left_key_up = false;
                                    Debug.Print("Shift Left");
                                    int target_active_index = bodies_active.IndexOf(bodies[targetIndex]);

                                    if (target_active_index <= 0) target_active_index = bodies_active.Count - 1;
                                    else target_active_index--;

                                    targetIndex = Array.IndexOf(bodies, bodies_active[target_active_index]);
                                }

                                //RIGHT SHIFT
                                if (Keyboard.IsKeyDown(Key.Right)) wait_for_right_key_up = true;

                                if (Keyboard.IsKeyUp(Key.Right) && wait_for_right_key_up)
                                {
                                    wait_for_right_key_up = false;
                                    Debug.Print("Shift Right");
                                    int target_active_index = bodies_active.IndexOf(bodies[targetIndex]);

                                    if (target_active_index >= (bodies_active.Count - 1)) target_active_index = 0;
                                    else target_active_index++;

                                    targetIndex = Array.IndexOf(bodies, bodies_active[target_active_index]);
                                }
                            }

                            this.drawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        /// <summary>
        /// Draws face frame results
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceResult">container of all face frame results</param>
        /// <param name="drawingContext">drawing context to render to</param>
        private void DrawFaceFrameResults(int faceIndex, DrawingContext drawingContext)
        {
            // red brush if currently targeted
            Brush drawingBrush = Brushes.White;
            if (faceIndex < this.bodyCount)
            {
                if (faceIndex == targetIndex)
                {
                    drawingBrush = Brushes.Red;
                }
                else drawingBrush = Brushes.White;
            }

            //Possibly draw bounding box in future around body

            string faceText = string.Empty;

            var head = bodies[faceIndex].Joints[JointType.Head];

            faceText += "Pedestrian Index " + bodies_active.IndexOf(bodies[faceIndex]) + "\n" +
                        "X-Angle from Camera : " + Find_Angle_Of_Face(faceIndex).X + "°\n" +
                        "Y-Angle from Camera : " + Find_Angle_Of_Face(faceIndex).Y + "°\n" +
                        "Distance From Camera: " + Find_Distance_To_Face(faceIndex) + "m\n";

            // render the face property and face rotation information
            Point faceTextLayout;
            if (this.GetFaceTextPositionInColorSpace(faceIndex, out faceTextLayout))
            {
                drawingContext.DrawText(
                        new FormattedText(
                            faceText,
                            CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight,
                            new Typeface("Georgia"),
                            DrawTextFontSize,
                            drawingBrush),
                        faceTextLayout);
            }
        }

        /// <summary>
        /// Computes the face result text position by adding an offset to the corresponding 
        /// body's head joint in camera space and then by projecting it to screen space
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceTextLayout">the text layout position in screen space</param>
        /// <returns>success or failure</returns>
        private bool GetFaceTextPositionInColorSpace(int faceIndex, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                    X = headJoint.X + TextLayoutOffsetX,
                    Y = headJoint.Y + TextLayoutOffsetY,
                    Z = headJoint.Z
                };

                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(textPoint);
                faceTextLayout.X = depthSpacePoint.X;
                faceTextLayout.Y = depthSpacePoint.Y;

                isLayoutValid = true;                
            }
            return isLayoutValid;
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);


                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }
                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            if (this.kinectSensor != null)
            {
                // on failure, set the status text
                this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                                : Properties.Resources.SensorNotAvailableStatusText;
            }
        }

        private Vector Find_Angle_Of_Face(int faceIndex)
        {
            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                //var headJoint = body.Joints[JointType.Head].Position;
                var midpoint = body.Joints[JointType.SpineMid].Position;

                double distance = Math.Sqrt(Math.Pow(midpoint.X, 2) + Math.Pow(midpoint.Y, 2) + Math.Pow(midpoint.Z, 2));
                double xAngle = (180.0 / Math.PI) * Math.Asin(midpoint.Y / distance);//Opposite over adjacent (y/hypotneuse)
                double yAngle = (180.0 / Math.PI) * Math.Asin(midpoint.X / distance);

                return new Vector(Math.Round(xAngle, 2), Math.Round(yAngle, 2));
            }
            else return new Vector(0, 0);
        }

        private double Find_Distance_To_Face(int faceIndex)
        {
            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                double distance = Math.Sqrt(Math.Pow(headJoint.X, 2) + Math.Pow(headJoint.Y, 2) + Math.Pow(headJoint.Z, 2));
                return Math.Round(distance, 2);
            }
            else return 0.0;
        }

        //SERVO FUNCTIONS
        public int DegreeToPulseWidth(float input_degree)
        {
            //Input Range -90 to +90
            //Output Range 500 to 2500
            float angle = (100 / 9) * input_degree + 1500;
            return (int)angle;//Should be between 500-2500
        }

        public void CommandServo(int servo_num, float desired_degrees, float speed)
        {
            if (desired_degrees >= -90.0f && desired_degrees <= 90.0f)
            {
                if (servo_num == 0) current_y_degrees = desired_degrees;
                else if (servo_num == 1) current_x_degrees = desired_degrees;

                int pulse_width = DegreeToPulseWidth(desired_degrees);
                string command = "#" + servo_num + "P" + pulse_width + "S" + speed + "\r";
                serialPort.Write(command);
            }
        }

        public void SearchRoom()
        {
            if (use_pan_tilt)
            {
                if(bodies_active.Count <= 0)
                {
                    float amount = movement_amount;
                    if (!done_searching_left_side && current_y_degrees < 90.0f)
                    {
                        CommandServo(0, (float)(current_y_degrees + amount), 250.0f);//Needs to be slowed down considerably
                    }
                    else if (done_searching_left_side && current_y_degrees > -90.0f)
                    {
                        CommandServo(0, (float)(current_y_degrees - amount), 250.0f);//Needs to be slowed down considerably
                    }

                    if (current_y_degrees >= 90.0f) done_searching_left_side = true;
                    if (current_y_degrees <= -90.0f && done_searching_left_side)//At this point it should be 90 degrees to the RIGHT and returning to center having finished doing a sweep of the room.
                    {
                        CommandServo(0, 0, 250.0f);
                        done_searching_left_side = false;
                    }
                }                
            }
        }

        public void MotionDetected()
        {
            //Reset memory
            if(motion_cooldown_timer != null && motion_cooldown_timer.Enabled == true)
            {
                motion_cooldown_timer.Stop();
                motion_cooldown_timer.Dispose();
                Debug.Print("Additional Motion Detected. Cooldown Reset at " + motion_cooldown_time_seconds + " seconds remaining");
                motion_cooldown_time_seconds = motion_detection_search_time;
            }
            else
            {
                Debug.Print("\nMotion Detected");
            }

            //Sets Cool down timer
            motion_detected = true;
            SearchRoom();
            motion_cooldown_timer = new System.Timers.Timer(1000);

            motion_cooldown_timer.Elapsed += Motion_Cooldown_Countdown;
            motion_cooldown_timer.AutoReset = true;
            motion_cooldown_timer.Start();
        }

        private void Motion_Cooldown_Countdown(Object source, ElapsedEventArgs e)//Will be called every second
        {
            if(!body_detected)
            {
                if (motion_cooldown_time_seconds > 0)
                {
                    Debug.Print("Continuing Search for " + motion_cooldown_time_seconds + " seconds.");
                    motion_cooldown_time_seconds--;
                }
                else//Countdown finished 
                {
                    motion_detected = false;
                    Debug.Print("Motion Detected Cooldown Has Expired. Entering Standby.\n");
                    motion_cooldown_time_seconds = motion_detection_search_time;//Should point to a static value
                    motion_cooldown_timer.Stop();
                    motion_cooldown_timer.Dispose();

                    //Center Kinect
                    CommandServo(0, 0.0f, 250.0f);//Returns to Center
                    CommandServo(1, 0.0f, 250.0f);
                }
            }
            else//A body was detected - Kill the search
            {
                Debug.Print("A body was detected - Discontinuing search");
                //motion_detected = false;
                //motion_cooldown_time_seconds = motion_detection_search_time;//Should point to a static value
                //motion_cooldown_timer.Stop();
                //motion_cooldown_timer.Dispose();
            }
        }

        private void BodyDetected()
        {
            //Reset memory
            if (body_cooldown_timer != null && body_cooldown_timer.Enabled == true)
            {
                body_cooldown_timer.Stop();
                body_cooldown_timer.Dispose();
                body_cooldown_time_seconds = body_detection_search_time;
            }

            //Sets Cool down timer
            body_detected = true;
            body_cooldown_timer = new System.Timers.Timer(1000);

            body_cooldown_timer.Elapsed += Body_Cooldown_Countdown;
            body_cooldown_timer.AutoReset = true;
            body_cooldown_timer.Start();
        }

        private void Body_Cooldown_Countdown(Object source, ElapsedEventArgs e)//Will be called every second
        {
            if (body_cooldown_time_seconds > 0)
            {
                body_cooldown_time_seconds--;
            }
            else//Countdown finished
            {
                body_detected = false;
                body_cooldown_time_seconds = body_detection_search_time;//Should point to a static value
                body_cooldown_timer.Stop();
                body_cooldown_timer.Dispose();
                CommandServo(1, 0.0f, 250.0f);

                //Center Kinect
                if (!motion_detected)
                {
                    CommandServo(0, 0.0f, 250.0f);//Returns to Center, unless there is motion detected
                }
            }
        }
        
        // partof: central_server_connection
        public void SendPostRequest()
        {
            var request = HttpWebRequest.Create(
                "http://localhost:3001/sync"
            ) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "text/json";
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }
        
        // partof: central_server_connection
        public void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                request.ContentType = "application/json";
                request.Method = "POST";
                Stream postStream = request.EndGetRequestStream(asynchronousResult);
                
                // 
                // Create the post data
                // 
                string postData = this.postData;
                
                // cleanup
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();
                // Start the web request
                request.BeginGetResponse(new AsyncCallback(GetResponceStreamCallback), request);
            }   
            catch (Exception ex)
            {
                Debug.WriteLine("Error when getting data from central server from: GetRequestStreamCallback()");
            }
        }

        // partof: central_server_connection
        public void GetResponceStreamCallback(IAsyncResult callbackResult)
        {
            try 
            {
                HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
                using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                {
                    this.systemData = JsonConvert.DeserializeObject(httpWebStreamReader.ReadToEnd());
                    Debug.WriteLine($"systemData.status is {systemData.status}");
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when getting data from central server from: GetResponceStreamCallback()");
            }
        }
        
        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }
    }
}