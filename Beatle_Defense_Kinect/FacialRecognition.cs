namespace Microsoft.Samples.Kinect.Beatle_Defense_Kinect
{
    using System;
    using System.IO;
    using System.Globalization;
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Emgu.CV;
    using Emgu.CV.Structure;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Face;
    using Emgu.CV.WPF;
    using Emgu.CV.Util;

    public class FacialRecognition
    {
        // helper by Groo
        public static class IEnumerableExt
        {
            // usage: IEnumerableExt.FromSingleItem(someObject);
            public static IEnumerable<T> FromSingleItem<T>(T item)
            {
                yield return item;
            }
        }

        public string database_path, classifier_path, people_path, activations_path;
        public int num_trained;
        public Dictionary<string, int> label_to_int;
        public List<Image<Gray, byte>> training_images;
        public List<string> training_labels;
        public List<int> training_int_labels;

        public static int input_height, input_width;

        CascadeClassifier face_finder;
        LBPHFaceRecognizer face_recognizer;

        public System.Windows.Media.Pen face_outline_pen;
        public System.Windows.Media.Brush face_label_brush;
        public Typeface face_label_font;
        public double face_label_font_size;

        public FacialRecognition()
        {
            classifier_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/opencv/data/lbpcascades/lbpcascade_frontalface.xml";
            database_path = "C:/Users/Walter/Documents/beatle_repo/public/";
            people_path = database_path + "people/";
            activations_path = activations_path + "activations/";

            num_trained = 0;
            input_height = 240;
            input_width = 426;

            face_outline_pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.LightBlue, 3);
            face_label_brush = Brushes.Black;
            face_label_font = new Typeface("Georgia");
            face_label_font_size = 10;

            label_to_int = new Dictionary<string, int>();
            training_images = new List<Image<Gray, byte>>();
            training_int_labels = new List<int>();
            training_labels = new List<string>();

            face_finder = new CascadeClassifier(classifier_path);
            face_recognizer = new LBPHFaceRecognizer();

            load_database();
        }

        public void load_database()
        {
            // get full list of names from database and all facialRecTraining images for each of them
            string[] people_names = File.ReadAllLines(people_path + "DatabaseFile.txt");
            Console.WriteLine("loaded {0} faces", people_names.Length);
            foreach (string name in people_names)
            {
                label_to_int.Add(name, num_trained);
                num_trained++;

                string person_dir = people_path + name;
                foreach (string file in Directory.EnumerateFiles(person_dir))
                {
                    if (file.Contains("FacialRecTraining"))
                    {
                        training_int_labels.Add(label_to_int[name]);
                        training_labels.Add(name);
                        training_images.Add(new Image<Gray, byte>(file));
                    }
                }
            }
        }

        public void train()
        {
            if (training_images.Count != 0)
            {
                face_recognizer.Train<Gray, byte>(training_images.ToArray(), training_int_labels.ToArray());
            }
        }

        public void update_single(Image<Gray, byte> face, int label)
        {
            face_recognizer.Update(IEnumerableExt.FromSingleItem(face).ToArray(), IEnumerableExt.FromSingleItem(label).ToArray());
        }

        public void add_training_image(Image<Gray, byte> face, int int_label)
        {
            //resize face detected image for force to compare the same size with the 
            string name = training_labels[int_label];
            training_int_labels.Add(int_label);
            training_labels.Add(name);
            training_images.Add(face);

            update_single(face, int_label);

            // async call database_add_training_image
        }

        public int add_new_person(Image<Bgr, byte> frame, Image<Gray, byte> face)
        {
            //resize face detected image for force to compare the same size with the
            string name = "Unknown" + num_trained.ToString();
            label_to_int.Add(name, num_trained);
            num_trained++;

            training_int_labels.Add(label_to_int[name]);
            training_labels.Add(name);
            training_images.Add(face);

            update_single(face, label_to_int[name]);

            // async call database_add_new_person

            return label_to_int[name];
        }

        public void update_last_seen()
        {
            // async call databse_update_last_seen
        }

        public void recognize_and_draw(DrawingContext dc, ref WriteableBitmap color_frame)
        {
            // Get the current frame
            Image<Bgr, byte> frame = writable_bitmap_to_image(color_frame);

            System.Drawing.Rectangle[] faces_detected = face_finder.DetectMultiScale(frame.Convert<Gray, byte>(), 1.2, 10, new System.Drawing.Size(10, 10));
            //System.Drawing.Rectangle[] faces_detected = new System.Drawing.Rectangle[0];

            Console.WriteLine("detected {0} faces", faces_detected.Length);

            // for each face detected
            foreach (System.Drawing.Rectangle f in faces_detected)
            {
                Rect outline = conv_rectangle(f, frame.Width, frame.Height);
                dc.DrawRectangle(null, face_outline_pen, outline);

                Image<Gray, byte> face = frame.Convert<Gray, byte>().Copy(f).Resize(100, 100, Inter.Cubic);

                if (training_images.Count == 0)
                {
                    add_new_person(frame, face);
                }

                FaceRecognizer.PredictionResult pred = face_recognizer.Predict(face);

                string name;
                if (pred.Distance < 100)
                {
                    name = training_labels[pred.Label];
                    if (pred.Distance > 70 && pred.Distance < 75)
                    {
                        add_training_image(face, pred.Label);
                    }
                }
                else
                {
                    int new_label = add_new_person(frame, face);
                    name = training_labels[new_label];
                }

                //Draw the label for each face detected and recognized
                dc.DrawText(new FormattedText(name,
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                face_label_font,
                                face_label_font_size,
                                face_label_brush),
                                conv_point(f.X, f.Y, frame.Width, frame.Height));
            }
        }

        public async void database_add_training_image()
        {
            //int next_img_ix = 01;
            //foreach (string file in Directory.EnumerateFiles(peopleDataPath + "/" + name))
            //{
            //    if (file.Contains("FacialRecTraining"))
            //    {
            //        next_img_ix++;
            //    }
            //}
            //if (next_img_ix < 100)
            //{
            //    string fsp = peopleDataPath + "/" + name + "/FacialRecTraining" + next_img_ix.ToString("D2") + ".png";
            //    face.Convert<Bgr, byte>().Save(fsp);
            //}
        }

        public async void database_add_new_person()
        {
            // add name to database file, create a new directory for this person, main image = frame, face = new facialrectraining image
            //File.AppendAllText(peopleDataPath + "/DatabaseFile.txt", "%" + name);
            //Directory.CreateDirectory(peopleDataPath + "/" + name);
            //File.WriteAllText(peopleDataPath + "/" + name + "/Info.txt", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-us")));
            //frame.Save(peopleDataPath + "/" + name + "/MainImage.png");
            //face.Convert<Bgr, byte>().Save(peopleDataPath + "/" + name + "/FacialRecTraining01.png");

            ////Trained face counter
            //ContTrain = ContTrain + 1;
        }

        public async void database_add_new_activation()
        {

        }

        public async void database_update_last_seen()
        {

        }

        public static Rect conv_rectangle(System.Drawing.Rectangle r, int width, int height)
        {
            double w_fac = (double)width / (double)input_width, h_fac = (double)height / (double)input_height;
            return new Rect(r.X * w_fac, r.Y * h_fac, r.Width * w_fac, r.Height * h_fac);
        }

        public static Point conv_point(int x, int y, int width, int height)
        {
            double w_fac = (double)width / (double)input_width, h_fac = (double)height / (double)input_height;
            return new Point(x * w_fac, y * h_fac);
        }

        public static Image<Bgr, byte> writable_bitmap_to_image(WriteableBitmap wbm)
        {
            return BitmapSourceConvert.ToMat(wbm).ToImage<Bgr, byte>();
            // .Resize(input_width, input_height, Inter.Cubic);
        }
    }
}