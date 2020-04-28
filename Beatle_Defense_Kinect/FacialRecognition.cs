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
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Diagnostics;
    
    public class FacialRecognition
    {
        public string pathToRootFolder        = "../../../../";
        public string pathToSystemDataFile    = "control_center/systemData.json";
        public string pathToPeopleFolder      = "public/people/";
        public string pathToActivationsFolder = "public/activations/";
        public int counter = 0;
        
        // helper by Groo
        public static class IEnumerableExt
        {
            // usage: IEnumerableExt.FromSingleItem(someObject);
            public static IEnumerable<T> FromSingleItem<T>(T item)
            {
                yield return item;
            }
        }

        public int recognized_threshold;
        public int add_new_training_threshold;

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
        
        // rendering helpers
        System.Drawing.Rectangle[] faces_detected;
        Image<Bgr, byte> frame;
        Image<Gray, byte> small_frame;

        public FacialRecognition()
        {
            classifier_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/opencv/data/haarcascades/haarcascade_frontalface_default.xml";
            database_path = "../../../../public/";
            people_path = database_path + "people/";
            activations_path = activations_path + "activations/";

            recognized_threshold = 105;
            add_new_training_threshold = 75;
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
            string system_data_string = File.ReadAllText(pathToRootFolder+pathToSystemDataFile);
            dynamic system_data = JsonConvert.DeserializeObject(system_data_string);
            Debug.WriteLine($"system_data is {system_data}");
            var people_names = system_data.faceRecognition.peopleNames;
            Debug.WriteLine($"people_names is {people_names}");

            foreach (string name in people_names)
            {
                label_to_int.Add(name, num_trained);
                Console.WriteLine($"{name} = {label_to_int[name]}");
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

            //update_single(face, int_label);
            train();

            // async call database_add_training_image
            // new Task(() => { database_add_training_image(name, face); }).Start();
        }

        public void database_add_training_image(string name, Image<Gray, byte> face)
        {
            string dir_path = people_path + name;
            int next_img_ix = 01;
            foreach (string file in Directory.EnumerateFiles(dir_path))
            {
                if (file.Contains("FacialRecTraining"))
                {
                    next_img_ix++;
                }
            }

            if (next_img_ix < 100)
            {
                face.Convert<Bgr, byte>().Save(dir_path + "/FacialRecTraining" + next_img_ix.ToString("D2") + ".png");
            }
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

            //update_single(face, label_to_int[name]);
            train();

            // async call database_add_new_person
            // new Task(() => { database_add_new_person(name, frame, face); }).Start();

            return label_to_int[name];
        }

        public void database_add_new_person(string name, Image<Bgr, byte> frame, Image<Gray, byte> face)
        {
            // add name to database file, create a new directory for this person, main image = frame, face = new facialrectraining image
            File.AppendAllLines(people_path + "DatabaseFile.txt", IEnumerableExt.FromSingleItem(name));
            string dir_path = people_path + name;
            Directory.CreateDirectory(dir_path);
            File.WriteAllText(dir_path + "/Info.txt", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-us")));
            frame.Save(dir_path + "/MainImage.png");
            face.Convert<Bgr, byte>().Save(dir_path + "/FacialRecTraining01.png");
        }

        public void update_last_seen(string name)
        {
            // async call databse_update_last_seen
            // new Task(() => { database_update_last_seen(name); }).Start();
        }

        public void database_update_last_seen(string name)
        {
            File.WriteAllText(people_path + name + "/Info.txt", DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-us")));
        }

        public void add_new_activation(Image<Bgr, byte> frame)
        {
            // new Task(() => { database_add_new_activation(frame); }).Start();
        }

        public void database_add_new_activation(Image<Bgr, byte> frame)
        {
            string[] lines = File.ReadAllLines(activations_path + "DatabaseFile.txt");
            int next_img_ix = lines.Length + 1;
            File.AppendAllLines(activations_path + "DatabaseFile.txt", IEnumerableExt.FromSingleItem(DateTime.Now.ToString(CultureInfo.GetCultureInfo("en-us"))));
            frame.Save(activations_path + "Activation" + next_img_ix.ToString("D3") +".png");
        }

        public void recognize_and_draw(DrawingContext dc, ref WriteableBitmap color_frame, int display_width, int display_height)
        {
            if (counter % 5 == 0) {
                // Get the current frame
                this.frame = writable_bitmap_to_image(color_frame);
                this.small_frame = this.frame.Convert<Gray, byte>().Resize(input_width, input_height, Inter.Cubic);

                this.faces_detected = face_finder.DetectMultiScale(small_frame, 1.2, 10, new System.Drawing.Size(10, 10));
                Debug.WriteLine($"faces_detected is {faces_detected}");
            }
            // for each face detected
            foreach (System.Drawing.Rectangle f in faces_detected)
            {
                Rect outline = conv_rectangle(f, display_width, display_height);
                dc.DrawRectangle(null, face_outline_pen, outline);

                Image<Gray, byte> face = small_frame.Copy(f).Resize(100, 100, Inter.Cubic);

                if (training_images.Count == 0)
                {
                    add_new_person(frame, face);
                }

                FaceRecognizer.PredictionResult pred = face_recognizer.Predict(face);

                string name;
                if (pred.Distance < recognized_threshold)
                {
                    name = training_labels[pred.Label];
                    if (pred.Distance > add_new_training_threshold)
                    {
                        add_training_image(face, pred.Label);
                    }
                }
                else
                {
                    int new_label = add_new_person(frame, face);
                    name = training_labels[new_label];
                }
                Console.WriteLine("{0} {1} {2}", training_labels[pred.Label], pred.Label, pred.Distance);

                // Draw the label for each face detected and recognized
                dc.DrawText(new FormattedText(name,
                                CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight,
                                face_label_font,
                                face_label_font_size,
                                face_label_brush),
                                conv_point(f.X, f.Y, display_width, display_height));
            }
            counter++;

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