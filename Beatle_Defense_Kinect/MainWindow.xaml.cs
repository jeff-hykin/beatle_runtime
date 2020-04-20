// ------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Beatle_Defense_Kinect
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Timers;
    using System.Linq;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;
    using Emgu.CV;
    using Emgu.CV.Structure;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Face;
    using Emgu.CV.WPF;
    using Phidget22;
    using System.Net;  
    using System.Text;
    using Newtonsoft.Json;
    using System.Threading;
    using System.Threading.Tasks;
    
    
    public class Helper {
        public dynamic mainWindow;
        public Helper(dynamic mainWindow=null) {
            this.mainWindow = mainWindow;
        }
        public void afterConstructor() {}
        public void afterNewFrame() {}
        public void afterDesctructor() {}
    }
    
    // 
    // Communication
    //
    public class OutgoingData
    {
        public int numberOfPeople = 0;
    }
    public class CommunicationHelper : Helper {
        // copy-paste constructor from Helpers
        public CommunicationHelper(dynamic mainWindow) { this.mainWindow = mainWindow; }
        
        // data
        public dynamic systemData = null;
        public dynamic outgoingData = new OutgoingData();
        
        // 
        // events (construct, newFrame, destruct)
        // 
        
        new public void afterConstructor() {
            Task.Run(async () => {
                for(;;)
                {
                    await Task.Delay(1000); // once per second
                    this.SendPostRequest();
                }
            });
        }
        
        // 
        // methods
        // 
        
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
        
        public void SendPostRequest()
        {
            var request = HttpWebRequest.Create(
                "http://localhost:3001/sync"
            ) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "text/json";
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }
        
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
                string postData = JsonConvert.SerializeObject(this.outgoingData);
                
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

        public void GetResponceStreamCallback(IAsyncResult callbackResult)
        {
            try 
            {
                HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
                using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                {
                    this.systemData = JsonConvert.DeserializeObject(httpWebStreamReader.ReadToEnd());
                    Debug.WriteLine($"systemData.status is {this.systemData.status}");
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when getting data from central server from: GetResponceStreamCallback()");
            }
        }
        
    }
    
    // 
    // Strobe
    // 
    public class StrobeHelper : Helper {
        // copy-paste constructor from Helpers
        public StrobeHelper(dynamic mainWindow) { this.mainWindow = mainWindow; }
        
        // data
        // DigitalOutput digitalOutput = new DigitalOutput();
        public const int phidgetSerialNumber = 12312;
        
        // 
        // events (construct, newFrame, destruct)
        // 
        
        new public void afterConstructor() {
            var timeoutDuration = 5000; // 5 seconds
            // this.digitalOutput.Open(timeoutDuration);
            // this.digitalOutput.DeviceSerialNumber = phidgetSerialNumber;
        }
        
        new public void afterNewFrame() {
            
        }
        
        new public void afterDesctructor() {
            // digitalOutput.Close();
        }
        
        // 
        // helpers
        // 
        
        private void TurnOn()
        {
            // digitalOutput.State = true;
        }

        private void TurnOff()
        {
            // digitalOutput.State = false;
        }
    }
    
    // 
    // Servo & Search
    // 
    public class ServoHelper : Helper {
        // copy-paste constructor from Helpers
        public ServoHelper(dynamic mainWindow) { this.mainWindow = mainWindow; }
        
        // Servo
        private bool use_pan_tilt = false;
        public System.IO.Ports.SerialPort serialPort;
        
        // Room Searching Stuff
        private static System.Timers.Timer motion_cooldown_timer;
        private readonly float motion_detection_search_time = 15.0f;
        private float motion_cooldown_time_seconds;
        private bool motion_detected = false;
        private static System.Timers.Timer body_cooldown_timer;
        private readonly float body_detection_search_time = 5.0f;
        private float body_cooldown_time_seconds;
        private bool body_detected = false;
        private Boolean done_searching_left_side = false;
        
        // Targeting  Stuff
        private float target_degree_tolerance = 5.0f;
        private float movement_amount = 0.5f;
        private double current_x_degrees = 0;
        private double current_y_degrees = 0;
        
        // 
        // events (construct, newFrame, destruct)
        // 
        
        new public void afterConstructor() {
            
            motion_cooldown_time_seconds = motion_detection_search_time;
            body_cooldown_time_seconds = body_detection_search_time;
            
            if (use_pan_tilt)
            {
                // Servo Stuff
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

                    // Now open the serial port
                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Couldn't open the Serial Port!");
                    Debug.WriteLine(ex.ToString()); // Report the actual error
                    use_pan_tilt = false;
                }

                // Commands Pan-Tilt to Center itself when starting
                CommandServo(0, 0.0f, 100);
                CommandServo(1, 0.0f, 100);
            }
        }
        
        new public void afterNewFrame() {
            // Servo Tracking
            if (use_pan_tilt)
            {
                var faceAngle = mainWindow.Find_Angle_Of_Face(mainWindow.targetBody);
                // If not aiming close enough to target on y axis
                if (Math.Abs(faceAngle.Y) > 5)
                {
                    float amount = movement_amount;
                    if (faceAngle.Y > 0)
                    {
                        amount *= -1;
                    }

                    CommandServo(0, (float)(current_y_degrees + amount), 1000.0f);
                }

                // If not aiming close enough to target on x axis
                if (Math.Abs(faceAngle.X) > 1)
                {
                    float amount = movement_amount;
                    if (faceAngle.X < 0)
                    {
                        amount *= -1;
                    }

                    CommandServo(1, (float)(current_x_degrees + 0.1f*amount), 100.0f);
                }

                // If no bodies tracked for a LONG time then should search room.
                // If no bodies tracked for a SHORT time then should stay where it is to hopefully catch the lost person.
                if (mainWindow.activeBodies.Count < 1 && motion_detected && !body_detected) SearchRoom();
                if (mainWindow.activeBodies.Count > 0) BodyDetected();
            }
        }
        
        new public void afterDesctructor() {
            if (use_pan_tilt)
            {
                // Closing serial port
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
        
        // 
        // helper methods
        // 
        
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
        
        public int DegreeToPulseWidth(float input_degree)
        {
            // Input Range -90 to +90
            // Output Range 500 to 2500
            float angle = (100 / 9) * input_degree + 1500;
            return (int)angle; // Should be between 500-2500
        }
        
        public void SearchRoom()
        {
            if (use_pan_tilt)
            {
                if (mainWindow.activeBodies.Count <= 0)
                {
                    float amount = movement_amount;
                    if (!done_searching_left_side && current_y_degrees < 90.0f)
                    {
                        CommandServo(0, (float)(current_y_degrees + amount), 250.0f); // Needs to be slowed down considerably
                    }
                    else if (done_searching_left_side && current_y_degrees > -90.0f)
                    {
                        CommandServo(0, (float)(current_y_degrees - amount), 250.0f); // Needs to be slowed down considerably
                    }

                    if (current_y_degrees >= 90.0f) done_searching_left_side = true;
                    if (current_y_degrees <= -90.0f && done_searching_left_side) // At this point it should be 90 degrees to the RIGHT and returning to center having finished doing a sweep of the room.
                    {
                        CommandServo(0, 0, 250.0f);
                        done_searching_left_side = false;
                    }
                }                
            }
        }
        
        public void MotionDetected()
        {
            // Reset memory
            if (motion_cooldown_timer != null && motion_cooldown_timer.Enabled == true)
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

            // Sets Cool down timer
            motion_detected = true;
            SearchRoom();
            motion_cooldown_timer = new System.Timers.Timer(1000);

            motion_cooldown_timer.Elapsed += Motion_Cooldown_Countdown;
            motion_cooldown_timer.AutoReset = true;
            motion_cooldown_timer.Start();
        }

        private void Motion_Cooldown_Countdown(Object source, ElapsedEventArgs e) // Will be called every second
        {
            if (!body_detected)
            {
                if (motion_cooldown_time_seconds > 0)
                {
                    Debug.Print("Continuing Search for " + motion_cooldown_time_seconds + " seconds.");
                    motion_cooldown_time_seconds--;
                }
                else // Countdown finished 
                {
                    motion_detected = false;
                    Debug.Print("Motion Detected Cooldown Has Expired. Entering Standby.\n");
                    motion_cooldown_time_seconds = motion_detection_search_time; // Should point to a static value
                    motion_cooldown_timer.Stop();
                    motion_cooldown_timer.Dispose();

                    // Center Kinect
                    CommandServo(0, 0.0f, 250.0f); // Returns to Center
                    CommandServo(1, 0.0f, 250.0f);
                }
            }
            else // A body was detected - Kill the search
            {
                Debug.Print("A body was detected - Discontinuing search");
                // motion_detected = false;
                // motion_cooldown_time_seconds = motion_detection_search_time; // Should point to a static value
                // motion_cooldown_timer.Stop();
                // motion_cooldown_timer.Dispose();
            }
        }

        private void BodyDetected()
        {
            // Reset memory
            if (body_cooldown_timer != null && body_cooldown_timer.Enabled == true)
            {
                body_cooldown_timer.Stop();
                body_cooldown_timer.Dispose();
                body_cooldown_time_seconds = body_detection_search_time;
            }
            // Sets Cool down timer
            body_detected = true;
            body_cooldown_timer = new System.Timers.Timer(1000);

            body_cooldown_timer.Elapsed += Body_Cooldown_Countdown;
            body_cooldown_timer.AutoReset = true;
            body_cooldown_timer.Start();
        }

        private void Body_Cooldown_Countdown(Object source, ElapsedEventArgs e) // Will be called every second
        {
            if (body_cooldown_time_seconds > 0)
            {
                body_cooldown_time_seconds--;
            }
            else // Countdown finished
            {
                body_detected = false;
                body_cooldown_time_seconds = body_detection_search_time; // Should point to a static value
                body_cooldown_timer.Stop();
                body_cooldown_timer.Dispose();
                CommandServo(1, 0.0f, 250.0f);

                // Center Kinect
                if (!motion_detected)
                {
                    CommandServo(0, 0.0f, 250.0f); // Returns to Center, unless there is motion detected
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // 
        // helpers
        // 
        public dynamic communicationHelper;
        public dynamic strobeHelper;
        public dynamic servoHelper;
        
        // 
        // drawing
        // 
        public dynamic font = new Typeface("Helvetica");
        public dynamic cultureInfo = CultureInfo.GetCultureInfo("en-us");
        
        public double drawFaceShapeThickness = 8;
        public double drawTextFontSize = 10;
        public double facePointRadius = 1.0;
        public float textLayoutOffsetX = -0.1f;
        public float textLayoutOffsetY = 0.25f;

        public double jointThickness = 3;
        public double clipBoundsThickness = 10;
        public int bodyPenThickness = 10;

        private const double DrawTextFontSize = 10;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary> Drawing group for body rendering output </summary>
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        /// <summary> Constant for clamping Z values of camera space points from being negative </summary>
        private float inferredZPositionClamp = 0.1f;
        
        // 
        // kinect
        // 
        private KinectSensor kinectSensor = null;

            private ColorFrameReader colorFrameReader = null;
            private BodyFrameReader bodyFrameReader = null;
            private WriteableBitmap colorBitmap = null;

        
        // 
        // body
        // 
        public Body[] bodies = null;
        private int maxBodyCount;
        private List<Tuple<JointType, JointType>> bones;
        private List<JointType> usedJoints;
        /// <summary> Coordinate mapper to map one type of point to another </summary>
        private CoordinateMapper coordinateMapper = null;

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
        private List<Body> activeBodies = new List<Body>();

        public Body targetBody;

        // For Key Controls
        bool waitForLeftKeyUp = false;
        bool waitForRightKeyUp = false;
        bool waitForSpaceKeyUp = false;

        CascadeClassifier face;
        //FontFace font = FontFace.HersheyTriplex;
        Image<Gray, byte> result;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        Dictionary<string, int> label_to_int = new Dictionary<string, int>();
        List<string> labels = new List<string>();
        List<int> int_labels = new List<int>();
        List<string> NamePersons = new List<string>();
        int ContTrain, t;
        string name, startupPath, peopleDataPath, actDataPath;

        int strobe_state;
        DigitalOutput strobe_do;
        int strobe_phidget_sn;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.communicationHelper = new CommunicationHelper(this);
            this.strobeHelper        = new StrobeHelper(this);
            this.servoHelper         = new ServoHelper(this);
            
            SetupKinectStuff();
            FacialRecSetup();
            StrobeSetup();
            
            this.communicationHelper.afterConstructor();
            this.strobeHelper.afterConstructor();
            this.servoHelper.afterConstructor();
        }

        private void FacialRecSetup()
        {
            // absolute path to start of database
            peopleDataPath = "./people";
            actDataPath = "./activations";
            // cascadeclassifier is in this dir
            startupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            face = new CascadeClassifier(startupPath + "/opencv/data/lbpcascades/lbpcascade_frontalface.xml");

            // SOCKET.IO
            // get full list of names from database and all facialRecTraining images for each of them
            ContTrain = 0;
            string[] people_names = File.ReadAllText(peopleDataPath + "/DatabaseFile.txt").Split('%');
            if (people_names[0] == "")
            {
                people_names = people_names.Skip(1).ToArray();
            }
            foreach (string name in people_names)
            {

                label_to_int.Add(name, ContTrain);
                string dir = peopleDataPath + "/" + name;
                foreach (string file in Directory.EnumerateFiles(dir))
                {
                    if (file.Contains("FacialRecTraining"))
                    {
                        int_labels.Add(ContTrain);
                        labels.Add(name);
                        trainingImages.Add(new Image<Gray, byte>(file));
                    }
                }
                ContTrain++;
            }
        }

        private void StrobeSetup()
        {
            // STROBE
            strobe_state = 0;
            strobe_phidget_sn = 12312;
            strobe_do.DeviceSerialNumber = strobe_phidget_sn;
            strobe_do.Open(5000);
        }
        
        // System
        public void SetupKinectStuff() {
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

            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            this.displayRect = new Rect(0.0, 0.0, this.displayWidth, this.displayHeight);

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();
            this.usedJoints = new List<JointType>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.usedJoints.Add(JointType.Head);
            this.usedJoints.Add(JointType.Neck);
            this.usedJoints.Add(JointType.SpineShoulder);
            this.usedJoints.Add(JointType.SpineMid);
            this.usedJoints.Add(JointType.SpineBase);
            this.usedJoints.Add(JointType.ShoulderRight);
            this.usedJoints.Add(JointType.ShoulderLeft);

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.usedJoints.Add(JointType.ElbowRight);
            this.usedJoints.Add(JointType.WristRight);
            this.usedJoints.Add(JointType.HandRight);

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.usedJoints.Add(JointType.ElbowLeft);
            this.usedJoints.Add(JointType.WristLeft);
            this.usedJoints.Add(JointType.HandLeft);

            // wire handler for body frame arrival
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // set the maximum number of bodies that would be tracked by Kinect
            this.maxBodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.maxBodyCount];

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        private void turn_on_strobe()
        {
            strobe_state = 1;
            strobe_do.State = true;
        }

        private void turn_off_strobe()
        {
            strobe_state = 0;
            strobe_do.State = false;
        }

        private void add_new_face(Image <Bgr, byte> frame, Image<Gray, byte> face)
        {
            //Trained face counter
            ContTrain = ContTrain + 1;

            //resize face detected image for force to compare the same size with the 
            trainingImages.Add(face);
            labels.Add("Unknown" + ContTrain.ToString());
            label_to_int.Add(labels.Last(), labels.Count);
            int_labels.Add(label_to_int[labels.Last()]);

            string name = labels.Last();

            // SOCKET.IO
            // add name to database file, create a new directory for this person, main image = frame, face = new facialrectraining image
            File.AppendAllText(peopleDataPath + "/DatabaseFile.txt", "%" + name);
            Directory.CreateDirectory(peopleDataPath + "/" + name);
            File.WriteAllText(peopleDataPath + "/" + name + "/Info.txt", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-us")));
            frame.Save(peopleDataPath + "/" + name + "/MainImage.png");
            face.Convert<Bgr, byte>().Save(peopleDataPath + "/" + name + "/FacialRecTraining01.png");
        }

        private void add_training_face(Image<Gray, byte> face, int int_label)
        {
            //resize face detected image for force to compare the same size with the 
            trainingImages.Add(face);
            labels.Add(labels[int_label]);
            int_labels.Add(int_label);

            string name = labels.Last();

            // SOCKET.IO
            // face = new facialrectraining image
            int next_img_ix = 01;
            foreach (string file in Directory.EnumerateFiles(peopleDataPath + "/" + name))
            {
                if (file.Contains("FacialRecTraining"))
                {
                    next_img_ix++;
                }
            }
            if (next_img_ix < 100)
            {
                string fsp = peopleDataPath + "/" + name + "/FacialRecTraining" + next_img_ix.ToString("D2") + ".png";
                face.Convert<Bgr, byte>().Save(fsp);
            }
        }

        private Rect conv_rectangle(System.Drawing.Rectangle r, int width, int height)
        {
            double w_fac = (double)width / 426.0, h_fac = (double)height / 240.0;
            return new Rect(r.X * w_fac, r.Y * h_fac, r.Width * w_fac, r.Height * h_fac);
        }

        private Point conv_point(int x, int y, int width, int height)
        {
            double w_fac = (double)width / 426.0, h_fac = (double)height / 240.0;
            return new Point(x * w_fac, y * h_fac);
        }

        private Image<Bgr, byte> wbm_to_img(WriteableBitmap wbm)
        {
            return BitmapSourceConvert.ToMat(wbm).ToImage<Bgr, byte>().Resize(426, 240, Inter.Cubic);
        }

        private void face_recognition(DrawingContext dc)
        {
            System.Windows.Media.Pen drawPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.LightBlue, 3);

            //Get the current frame
            Image<Bgr, byte> frame = wbm_to_img(this.colorBitmap);

            //Face Detector
            System.Drawing.Rectangle[] facesDetected = face.DetectMultiScale(frame.Convert<Gray, byte>(), 1.2, 10, new System.Drawing.Size(10, 10));

            //Action for each element detected
            foreach (System.Drawing.Rectangle f in facesDetected)
            {
                Rect coili = conv_rectangle(f, this.displayWidth, this.displayHeight);
                dc.DrawRectangle(null, drawPen, coili);
                result = frame.Convert<Gray, byte>().Copy(f).Resize(100, 100, Inter.Cubic);

                if (trainingImages.Count() == 0)
                {
                    add_new_face(frame, result);
                }

                LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer();
                recognizer.Train<Gray, Byte>(trainingImages.ToArray(), int_labels.ToArray());

                FaceRecognizer.PredictionResult pred = recognizer.Predict(result);

                Console.WriteLine("{0} {1} ", pred.Distance, pred.Label);
                Console.WriteLine(labels.Count);
                if (pred.Distance < 100)
                {
                        name = labels[pred.Label];
                        if (pred.Distance > 70)
                        {
                            add_training_face(result, pred.Label);
                        }
                }
                else
                {
                    add_new_face(frame, result);
                    name = labels.Last();
                }
                Console.WriteLine(labels[pred.Label]);

                //Draw the label for each face detected and recognized
                dc.DrawText(new FormattedText(name,
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                new Typeface("Georgia"),
                                DrawTextFontSize, Brushes.LightGreen),
                            conv_point(f.X, f.Y, this.displayWidth, this.displayHeight));
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
        
        public int targetIndex
        {
            get
            {
                return Array.IndexOf(this.bodies, targetBody);
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
            
            this.communicationHelper.afterDesctructor();
            this.strobeHelper.afterDesctructor();
            this.servoHelper.afterDesctructor();
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            // if the system is disarmed, basically do nothing
            if (!this.communicationHelper.IsArmed)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    dc.DrawImage(this.colorBitmap, this.displayRect);
                }
                return;
            }
            
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    // 
                    // draw  visible/bodies/text
                    // 
                    
                    // update body data
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        // facial recognition
                        face_recognition(dc);

                        // 
                        // draw visible spectrum
                        // 
                        dc.DrawImage(this.colorBitmap, this.displayRect);
            
                        
                        // 
                        // iterate over each body
                        // 
                        
                        // Used for counting bodies observed
                        this.activeBodies.Clear();
                        foreach (var body in this.bodies)
                        {
                            if (body == null || !(body.IsTracked))
                            {
                                continue;
                            }
                            
                            // Populate activeBodies
                            this.activeBodies.Add(body);
                            // red brush if currently targeted, white otherwise
                            var brush = this.SelectBrush(body);
                            
                            // 
                            // draw
                            // 
                            this.DrawPersonInfo(body, dc, brush);
                            this.DrawClippedEdges(body, dc);
                            this.DrawBody(body, dc, brush);
                        }
                        
                        // 
                        // tell central server who was found
                        // 
                        this.communicationHelper.outgoingData.numberOfPeople = this.activeBodies.Count;
                        
                        // 
                        // HUD info
                        // 

                        var angleOfFace = Find_Angle_Of_Face(this.targetBody);
                        dc.DrawText(
                            new FormattedText(
                                $"Number of Bodies Detected = {this.activeBodies.Count}"+
                                $"\nTargetIndex = {this.targetIndex}" + 
                                $"\nTarget X-Angle from Camera : {angleOfFace.X}°" +
                                $"\nTarget Y-Angle from Camera : {angleOfFace.Y}°",
                                this.cultureInfo,
                                FlowDirection.LeftToRight,
                                this.font,
                                this.drawTextFontSize,
                                Brushes.White
                            ),
                            new Point(displayWidth / 2 + 90, displayHeight - 50)
                        );
                        
                        this.drawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);
                    }
                    
                    // 
                    // run helpers
                    // 
                    
                    this.communicationHelper.afterNewFrame();
                    this.strobeHelper.afterNewFrame();
                    this.servoHelper.afterNewFrame();
                    
                    // 
                    // Motion Input Simulation
                    // 
                    if (Keyboard.IsKeyDown(Key.Space))
                    {
                        this.waitForSpaceKeyUp = true;
                    }
                    if (Keyboard.IsKeyUp(Key.Space) && this.waitForSpaceKeyUp)
                    {
                        this.waitForSpaceKeyUp = false;
                        this.servoHelper.MotionDetected();
                    }
                    
                    // 
                    // Change Which Person is the Target
                    // 
                    if (activeBodies.Count > 0)
                    {
                        int target_active_index = this.activeBodies.IndexOf(this.targetBody);
                        // if target isn't in found in active bodies automatically, then switch to a new body
                        if (target_active_index < 0) {
                            target_active_index = 0;
                            this.targetBody = this.activeBodies[0];
                        }
                        
                        // 
                        // LEFT SHIFT (change target body)
                        // 
                        if (Keyboard.IsKeyDown(Key.Left)) 
                        {
                            this.waitForLeftKeyUp = true;
                        }
                        if (Keyboard.IsKeyUp(Key.Left) && this.waitForLeftKeyUp)
                        {
                            this.waitForLeftKeyUp = false;
                            Debug.Print("Shift Left");
                            target_active_index--;
                        }
                        
                        // 
                        // RIGHT SHIFT (change target body)
                        // 
                        if (Keyboard.IsKeyDown(Key.Right)) 
                        {
                            this.waitForRightKeyUp = true;
                        }
                        if (Keyboard.IsKeyUp(Key.Right) && this.waitForRightKeyUp)
                        {
                            this.waitForRightKeyUp = false;
                            Debug.Print("Shift Right");
                            target_active_index++;
                        }
                        
                        // 
                        // keep target in range
                        //
                        if (target_active_index < 0)
                        {
                            // if negative, wrap it around
                            target_active_index = this.activeBodies.Count + target_active_index;
                        }
                        else if (target_active_index >= this.activeBodies.Count)
                        {
                            // if positive, wrap it down using modulus
                            target_active_index = target_active_index % this.activeBodies.Count;
                        }
                        
                        // update target
                        this.targetBody = this.activeBodies[target_active_index];
                    }
                }
            }
        }
        
        public Brush SelectBrush(Body body)
        {
            if (body == this.targetBody)
            {
                return Brushes.Red;
            }
            else
            {
                return Brushes.White;
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(Body body, DrawingContext drawingContext, Brush drawingBrush)
        {
            // 
            // get the joints
            // 
            
            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
            // convert the joint points to depth (display) space
            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

            foreach (JointType jointType in joints.Keys)
            {
                // sometimes the depth(Z) of an inferred joint may show as negative
                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                CameraSpacePoint position = joints[jointType].Position;
                if (position.Z < 0)
                {
                    position.Z = this.inferredZPositionClamp;
                }

                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
            }

            
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, new Pen(drawingBrush, this.bodyPenThickness));
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                if (usedJoints.Contains(jointType))
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
                        drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], this.jointThickness, this.jointThickness);
                    }
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
                    new Rect(0, this.displayHeight - this.clipBoundsThickness, this.displayWidth, this.clipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, this.clipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.clipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - this.clipBoundsThickness, 0, this.clipBoundsThickness, this.displayHeight));
            }
        }

        /// <summary>
        /// Draws face frame results
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceResult">container of all face frame results</param>
        /// <param name="drawingContext">drawing context to render to</param>
        private void DrawPersonInfo(Body body, DrawingContext drawingContext, Brush drawingBrush)
        {
            // Possibly draw bounding box in future around body

            string faceText = string.Empty;

            var head = body.Joints[JointType.Head];

            faceText += "Pedestrian Index " + activeBodies.IndexOf(body) + "\n" +
                        "X-Angle from Camera : " + Find_Angle_Of_Face(body).X + "°\n" +
                        "Y-Angle from Camera : " + Find_Angle_Of_Face(body).Y + "°\n" +
                        "Distance From Camera: " + Find_Distance_To_Face(body) + "m\n";

            // render the face property and face rotation information
            Point faceTextLayout;
            if (this.GetFaceTextPositionInColorSpace(body, out faceTextLayout))
            {
                drawingContext.DrawText(
                    new FormattedText(
                        faceText,
                        this.cultureInfo,
                        FlowDirection.LeftToRight,
                        this.font,
                        this.drawTextFontSize,
                        drawingBrush
                    ),
                    faceTextLayout
                );
            }
        }

        /// <summary>
        /// Computes the face result text position by adding an offset to the corresponding 
        /// body's head joint in camera space and then by projecting it to screen space
        /// </summary>
        /// <param name="faceIndex">the index of the face frame corresponding to a specific body in the FOV</param>
        /// <param name="faceTextLayout">the text layout position in screen space</param>
        /// <returns>success or failure</returns>
        private bool GetFaceTextPositionInColorSpace(Body body, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                    X = headJoint.X + this.textLayoutOffsetX,
                    Y = headJoint.Y + this.textLayoutOffsetY,
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
                                ColorImageFormat.Bgra
                            );

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
                this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText : Properties.Resources.SensorNotAvailableStatusText;
            }
        }

        private Vector Find_Angle_Of_Face(Body body)
        {
            if (body != null && body.IsTracked)
            {
                // var headJoint = body.Joints[JointType.Head].Position;
                var midpoint = body.Joints[JointType.SpineMid].Position;

                double distance = Math.Sqrt(Math.Pow(midpoint.X, 2) + Math.Pow(midpoint.Y, 2) + Math.Pow(midpoint.Z, 2));
                double xAngle = (180.0 / Math.PI) * Math.Asin(midpoint.Y / distance); // Opposite over adjacent (y/hypotneuse)
                double yAngle = (180.0 / Math.PI) * Math.Asin(midpoint.X / distance);

                return new Vector(Math.Round(xAngle, 2), Math.Round(yAngle, 2));
            }
            return new Vector(0, 0);
        }

        private double Find_Distance_To_Face(Body body)
        {
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                double distance = Math.Sqrt(Math.Pow(headJoint.X, 2) + Math.Pow(headJoint.Y, 2) + Math.Pow(headJoint.Z, 2));
                return Math.Round(distance, 2);
            }
            else return 0.0;
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