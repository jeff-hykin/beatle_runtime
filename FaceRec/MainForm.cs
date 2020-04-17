
//Multiple face detection and recognition in real time
//Using EmguCV cross platform .Net wrapper to the Intel OpenCV image processing library for C#.Net
//Writed by Sergio Andrés Guitérrez Rojas
//"Serg3ant" for the delveloper comunity
// Sergiogut1805@hotmail.com
//Regards from Bucaramanga-Colombia ;)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace MultiFaceRec
{
    public partial class FrmPrincipal : Form
    {
        //Declararation of all variables, vectors and haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        CascadeClassifier face;
        FontFace font = FontFace.HersheyTriplex;
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        Dictionary<string, int> label_to_int = new Dictionary<string, int>();
        List<string> labels= new List<string>();
        List<int> int_labels = new List<int>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;


        public FrmPrincipal()
        {
            InitializeComponent();
            //Load haarcascades for face detection
            face = new CascadeClassifier(Application.StartupPath + "/opencv/data/lbpcascades/lbpcascade_frontalface.xml");
            try
            {
                //Load of previus trainned faces and labels for each image
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                Console.WriteLine("\"" + Labelsinfo + "\"");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                    if (!label_to_int.ContainsKey(Labels[tf]))
                    {
                        label_to_int.Add(Labels[tf], tf);
                        int_labels.Add(tf);
                    }
                    else
                    {
                        int_labels.Add(label_to_int[Labels[tf]]);
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Nothing in binary database, please add at least a face(Simply train the prototype with the Add Face Button).", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            //Initialize the capture device
            grabber = new Capture();
            grabber.QueryFrame();
            //Initialize the FrameGraber event
            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false;
        }


        private void button2_Click(object sender, System.EventArgs e)
        {
            //Trained face counter
            ContTrain = ContTrain + 1;

            //Get the current frame form capture device
            gray = grabber.QueryFrame().ToImage<Bgr, byte>().Resize(320, 240, Inter.Cubic).Convert<Gray, Byte>();

            //Face Detector
            Rectangle[] facesDetected = face.DetectMultiScale(
                gray,
                1.2,
                10,
                new Size(10, 10));

            if (facesDetected.Length > 0)
            {
                TrainedFace = currentFrame.Copy(facesDetected[0]).Convert<Gray, byte>();
            }
            else
            {
                MessageBox.Show("No face detected!", "Training FAILED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

                //resize face detected image for force to compare the same size with the 
            //test image with cubic interpolation type method
            TrainedFace = result.Resize(100, 100, Inter.Cubic);
            trainingImages.Add(TrainedFace);
            labels.Add(textBox1.Text);
            if (!label_to_int.ContainsKey(labels.Last()))
            {
                label_to_int.Add(labels.Last(), labels.Count);
                int_labels.Add(labels.Count);
            }
            else
            {
                int_labels.Add(label_to_int[labels.Last()]);
            }

            //Show face added in gray scale
            imageBox1.Image = TrainedFace;

            //Write the number of triained faces in a file text for further load
            File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");

            //Write the labels of triained faces in a file text for further load
            for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
            {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
            }

            MessageBox.Show(textBox1.Text + "´s face detected and added :)", "Training OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        void FrameGrabber(object sender, EventArgs e)
        {
            label3.Text = "0";
            NamePersons.Add("");

            //Get the current frame form capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, byte>().Resize(320, 240, Inter.Cubic);

            //Convert it to Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //Face Detector
            Rectangle[] facesDetected = face.DetectMultiScale(gray, 1.2, 10, new Size(10, 10));

            //Action for each element detected
            foreach (Rectangle f in facesDetected)
            {
                t = t + 1;
                result = currentFrame.Copy(f).Convert<Gray, byte>().Resize(100, 100, Inter.Cubic);
                //draw the face detected in the 0th (gray) channel with blue color
                currentFrame.Draw(f, new Bgr(Color.Red), 2);


                if (trainingImages.ToArray().Length != 0)
                {
                    //Eigen face recognizer
                    //EigenFaceRecognizer recognizer = new EigenFaceRecognizer(0, 3000);
                    LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer();
                    recognizer.Train<Gray, Byte>(trainingImages.ToArray(), int_labels.ToArray());

                    FaceRecognizer.PredictionResult pred = recognizer.Predict(result);
                    Console.WriteLine(pred.Distance);
                    if (pred.Distance < 100)
                    {
                        name = labels[pred.Label - 1];
                    }
                    else
                    {
                        name = "Unknown";
                    }

                    //Draw the label for each face detected and recognized
                    //currentFrame.Draw(name, ref font, new Point(f.X - 2, f.Y - 2), new Bgr(Color.LightGreen));
                    currentFrame.Draw(name, new Point(f.X - 2, f.Y - 2), font, 1.0, new Bgr(Color.LightGreen));


                }

                    NamePersons[t-1] = name;
                    NamePersons.Add("");


                //Set the number of faces detected on the scene
                label3.Text = facesDetected.Length.ToString();

            }
            t = 0;

            //Names concatenation of persons recognized
            for (int nnn = 0; nnn < facesDetected.Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
            }
            //Show the faces procesed and recognized
            imageBoxFrameGrabber.Image = currentFrame;
            label4.Text = names;
            names = "";
            //Clear the list(vector) of names
            NamePersons.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start("Donate.html");
        }

    }
}