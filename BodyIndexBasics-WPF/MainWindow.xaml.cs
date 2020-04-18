//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyIndexBasics
{
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;  
    using System.Text;  
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    // using AForge.Video.FFMPEG;

    /// <summary>
    /// Interaction logic for the MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        /// <summary>
        /// An object used to create a video file
        /// </summary>
        // public VideoFileWriter VideoWriter = new VideoFileWriter();
        public int WhichFrame = 0;
        
        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Collection of colors to be used to display the BodyIndexFrame data.
        /// </summary>
        private static readonly uint[] BodyColor =
        {
            0x0000FF00,
            0x00FF0000,
            0xFFFF4000,
            0x40FFFF00,
            0xFF40FF00,
            0xFF808000,
        };

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for body index frames
        /// </summary>
        private BodyIndexFrameReader bodyIndexFrameReader = null;

        /// <summary>
        /// Description of the data contained in the body index frame
        /// </summary>
        private FrameDescription bodyIndexFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap bodyIndexBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private uint[] bodyIndexPixels = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.kinectSensor = KinectSensor.GetDefault();            
            
            // 
            // body index frame reader
            // 
            
            // open the reader for the depth frames
            this.bodyIndexFrameReader = this.kinectSensor.BodyIndexFrameSource.OpenReader();
            // attach the callback handler (via +=) that will be called on every frame arrival
            this.bodyIndexFrameReader.FrameArrived += this.bodyIndexReader_onFrameArrived;
            this.bodyIndexFrameDescription = this.kinectSensor.BodyIndexFrameSource.FrameDescription;

            // allocate space to put the pixels being converted
            this.bodyIndexPixels = new uint[this.bodyIndexFrameDescription.Width * this.bodyIndexFrameDescription.Height];
            // create the bitmap to display
            this.bodyIndexBitmap = new WriteableBitmap(this.bodyIndexFrameDescription.Width, this.bodyIndexFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            
            
            // 
            // setup saving data to video file
            // 
            
            // // create new video file with the same dimensions as the incoming frames
            // var frameRate = 25; // fps
            // this.VideoWriter.Open("test.avi", this.bodyIndexFrameDescription.Width, this.bodyIndexFrameDescription.Height, frameRate, VideoCodec.MPEG4);

            // open the sensor
            this.kinectSensor.Open();

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
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
                return this.bodyIndexBitmap;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.bodyIndexFrameReader != null)
            {
                // remove the event handler
                this.bodyIndexFrameReader.FrameArrived -= this.bodyIndexReader_onFrameArrived;

                // BodyIndexFrameReder is IDisposable
                this.bodyIndexFrameReader.Dispose();
                this.bodyIndexFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            
            // // clean up the video writer
            // this.VideoWriter.Close();
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.bodyIndexBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.bodyIndexBitmap));

                string screenshotTime = System.DateTime.UtcNow.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string whereToSaveScreenshot = Path.Combine(myPhotos, "KinectScreenshot-BodyIndex-" + screenshotTime + ".png");

                // save the file
                try { using (FileStream fs = new FileStream(whereToSaveScreenshot, FileMode.Create)) {
                    encoder.Save(fs);
                }} catch (IOException) { }
            }
        }

        /// <summary>
        /// Handles the body index frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void bodyIndexReader_onFrameArrived(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            bool bodyIndexFrameProcessed = false;
            
            // get the new frame
            using (BodyIndexFrame bodyIndexFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer bodyIndexBuffer = bodyIndexFrame.LockImageBuffer())
                    {
                        var widthMatches = this.bodyIndexFrameDescription.Width == this.bodyIndexBitmap.PixelWidth;
                        var heightMatches = this.bodyIndexFrameDescription.Height == this.bodyIndexBitmap.PixelHeight;
                        var sizeMatches = (this.bodyIndexFrameDescription.Width * this.bodyIndexFrameDescription.Height) == bodyIndexBuffer.Size;
                        // verify data and write the color data to the display bitmap
                        if (sizeMatches && heightMatches && widthMatches)
                        {
                            // this is inside its own function only because it has to be marked as "unsafe"
                            // it changes the `this.bodyIndexPixels` which is used later
                            this.ProcessBodyIndexFrameData(bodyIndexBuffer.UnderlyingBuffer, bodyIndexBuffer.Size);
                            
                            // set this to true because we want to finish using (dispose of) the bodyIndexBuffer 
                            // to free up memory before we convert the indexes into a bitmap
                            bodyIndexFrameProcessed = true;
                        }
                    }
                }
            }
            
            // if something changed, then save it to the bitmap variable
            if (bodyIndexFrameProcessed)
            {
                this.bodyIndexBitmap.WritePixels(
                    new Int32Rect(0, 0, this.bodyIndexBitmap.PixelWidth, this.bodyIndexBitmap.PixelHeight),
                    this.bodyIndexPixels,
                    this.bodyIndexBitmap.PixelWidth * (int)BytesPerPixel,
                    0
                );
                // // add a frame to the video
                // this.VideoWriter.WriteVideoFrame(this.BitmapFromWriteableBitmap(this.bodyIndexBitmap));
                this.WhichFrame += 1;
                if ((this.WhichFrame % 10) == 1) {
                    SendPostRequest();
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the BodyIndexFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the bodyIndexFrameData pointer.
        /// </summary>
        /// <param name="bodyIndexFrameData">Pointer to the BodyIndexFrame image data</param>
        /// <param name="bodyIndexFrameDataSize">Size of the BodyIndexFrame image data</param>
        private unsafe void ProcessBodyIndexFrameData(IntPtr bodyIndexFrameData, uint bodyIndexFrameDataSize)
        {
            byte* frameData = (byte*)bodyIndexFrameData;

            // convert body index to a visual representation
            for (int i = 0; i < (int)bodyIndexFrameDataSize; ++i)
            {
                // the BodyColor array has been sized to match
                // BodyFrameSource.BodyCount
                if (frameData[i] < BodyColor.Length)
                {
                    // this pixel is part of a player,
                    // display the appropriate color
                    this.bodyIndexPixels[i] = BodyColor[frameData[i]];
                }
                else
                {
                    // this pixel is not part of a player
                    // display black
                    this.bodyIndexPixels[i] = 0x00000000;
                }
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
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            // End the stream request operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Create the post data
            string postData = "{ \"from_c_sharp\": \"this works\" }";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();

            // Start the web request
            request.BeginGetResponse(new AsyncCallback(GetResponceStreamCallback), request);
        }

        public void GetResponceStreamCallback(IAsyncResult callbackResult)
        {
            HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {
                string result = httpWebStreamReader.ReadToEnd();
                Console.WriteLine($"The response from the server is:{result}");
            };
        }
    }
}
