using System;
using System.Collections.Generic;
using System.IO;
using MediaToolkit;
using System.Drawing;
using System.Threading;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace video_to_ascii_text
{
    class Program
    {
        static string CharacterList = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "characters.txt"));
        static void Main(string[] args)
        {
            string OutputDir = Directory.GetCurrentDirectory() + @"\output";

            ClearDir(OutputDir);
            double TimePerFrame = VideoToImages();
            Console.WriteLine(TimePerFrame);

            List<string> AsciiList = new List<string>();
            AsciiList = AllImagesToAscii(OutputDir);

            DisplayText(OutputDir, TimePerFrame, AsciiList);

            Console.ReadLine();
        }


        static double VideoToImages()
        {
            using (var engine = new Engine())
            {
                Console.WriteLine("Converting video to series of images...");
                var mp4 = new MediaFile { Filename = "video.mp4" };

                engine.GetMetadata(mp4);
                var i = 0;
                while (i < mp4.Metadata.Duration.Seconds * mp4.Metadata.VideoData.Fps)
                {
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds((mp4.Metadata.Duration.Seconds / (mp4.Metadata.VideoData.Fps * mp4.Metadata.Duration.Seconds)) * i) };
                    var outputFile = new MediaFile { Filename = string.Format("{0}\\image-{1}.jpeg", "output", i) };
                    engine.GetThumbnail(mp4, outputFile, options);
                    i++;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Complete!");
                Console.ForegroundColor = ConsoleColor.White;
                Thread.Sleep(300);
                Console.Clear();

                return (mp4.Metadata.Duration.Seconds / (mp4.Metadata.VideoData.Fps * mp4.Metadata.Duration.Seconds));
            }
        }

        static void ClearDir(string dir)
        {
            foreach (var i in Directory.GetFiles(dir))
            {
                File.Delete(i);
            }
        }
        static Bitmap GetImg(string dir)
        {

            Bitmap Img = null;
            try
            {
                Img = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), dir));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid file path.");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            Img = new Bitmap(Img, new Size(180, 60));
            Img.RotateFlip(RotateFlipType.Rotate270FlipY);

            return Img;
        }

        static List<string> AllImagesToAscii(string OutputDir)
        {
            List<string> list = new List<string>();

            int DirLength = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\output").Length;

            for (int i = 0; i < DirLength; i++) //This Fucked Up!
            {

                string CurImgDir = string.Format("{0}\\image-{1}.jpeg", "output", i);
                Bitmap Img = GetImg(CurImgDir);

                List<float> PixelAvgs = GetPixelBrightness(Img);
                List<char> AsciiConverted = AvgToAscii(PixelAvgs);


                string Output = "";

                for (int a = 0; a < AsciiConverted.Count; a++)
                {
                    if (a % Img.Height == 0)
                    {
                        Output += "\n";
                    }
                    Output += AsciiConverted[a].ToString();
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Processing image {i + 1}/{DirLength}");
                list.Add(Output);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Images Processed, press enter to display...");
            Console.ReadLine();
            Console.Clear();
            return list;
        }

        static void DisplayText(string dir, double TimePerFrame, List<string> text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var i in text)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(i);
                Thread.Sleep((int)(TimePerFrame * 1000));
            }

        }


        static List<float> GetPixelBrightness(Bitmap img)
        {
            List<float> avg = new List<float>();
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    avg.Add((pixel.R + pixel.G + pixel.B) / 3.0f);
                }
            }
            return avg;

        }


        static List<char> AvgToAscii(List<float> avg)
        {
            List<char> list = new List<char>();

            foreach (var i in avg)
            {
                for (int a = 0; a < CharacterList.Length; a++)
                {
                    float amount = (255.0f / CharacterList.Length) * (a + 1.0f);
                    if (i <= amount)
                    {
                        list.Add(CharacterList[a]);
                        break;
                    }
                }
            }
            return list;
        }

    }
}
