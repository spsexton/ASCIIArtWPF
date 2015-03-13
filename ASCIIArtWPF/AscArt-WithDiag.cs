using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ASCIIArtWPF
{
    public static class AscArtWithDiag
    {
        // Typical width/height for ASCII characters
        private const double FontAspectRatio = 0.6;

        // Available character set, ordered by decreasing intensity (brightness)
        private const string OutputCharSet = "@%#*+=-:. ";

        // Alternate char set uses more chars, but looks less realistic
        private const string OutputCharSetAlternate = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";

        public static void GenerateAsciiArt(Bitmap bmInput, string outputFile, int outputWidth, bool diagLog)
        {
            FileInfo fi = new FileInfo(outputFile);
            StreamWriter diagWriter = null;
            if (diagLog)
            {
                diagWriter = new StreamWriter(Path.Combine(fi.DirectoryName, Path.GetFileNameWithoutExtension(outputFile) + ".log"));
                diagWriter.AutoFlush = true;
            }

            // pixelChunkWidth/pixelChunkHeight - size of a chunk of pixels that will 
            // map to 1 character.  These are doubles to avoid progressive rounding
            // error.
            double pixelChunkWidth = (double)bmInput.Width / (double)outputWidth;
            double pixelChunkHeight = pixelChunkWidth / FontAspectRatio;

            // Calculate output height to capture entire image
            int outputHeight = (int)Math.Round((double)bmInput.Height / pixelChunkHeight);

            if (diagLog)
            {
                diagWriter.WriteLine(string.Format("outputWidth={0}, calc'd outputHeight={1}", outputWidth, outputHeight));
                diagWriter.WriteLine(string.Format("bmWidth={0}, bmHeight={1}", bmInput.Width, bmInput.Height));
                diagWriter.WriteLine(string.Format("pixelChunkWidth={0}, pixelChunkHeight={1}", pixelChunkWidth, pixelChunkHeight));
            }

            // Generate output image, row by row
            double pixelOffSetTop = 0.0;
            StringBuilder sbOutput = new StringBuilder();

            for (int row = 1; row <= outputHeight; row++)
            {
                double pixelOffSetLeft = 0.0;

                for (int col = 1; col <= outputWidth; col++)
                {
                    if (diagLog)
                        diagWriter.WriteLine(string.Format("row={0}, col={1}, pixelOffsetLeft={2}, pixelOffsetTop={3}", row, col, pixelOffSetLeft, pixelOffSetTop));

                    // Calculate brightness for this set of pixels by averaging
                    // brightness across all pixels in this pixel chunk
                    double brightSum = 0.0;
                    int pixelCount = 0;
                    for (int pixelLeftInd = 0; pixelLeftInd < (int)pixelChunkWidth; pixelLeftInd++)
                        for (int pixelTopInd = 0; pixelTopInd < (int)pixelChunkHeight; pixelTopInd++)
                        {
                            // Each call to GetBrightness returns value between 0.0 and 1.0
                            int x = (int)pixelOffSetLeft + pixelLeftInd;
                            int y = (int)pixelOffSetTop + pixelTopInd;
                            if ((x < bmInput.Width) && (y < bmInput.Height))
                            {
                                brightSum += bmInput.GetPixel(x, y).GetBrightness();
                                pixelCount++;

                                if (diagLog)
                                    diagWriter.WriteLine(string.Format("  x={0}, y={1}", x, y));
                            }
                        }

                    // Average brightness for this entire pixel chunk, between 0.0 and 1.0
                    double pixelChunkBrightness = brightSum / pixelCount;
                    if (diagLog)
                        diagWriter.WriteLine(string.Format("  brightness={0}", pixelChunkBrightness));

                    // Target character is just relative position in ordered set of output characters
                    int outputIndex = (int)Math.Floor(pixelChunkBrightness * OutputCharSet.Length);
                    if (outputIndex == OutputCharSet.Length)
                        outputIndex--;
                    if (diagLog)
                        diagWriter.WriteLine(string.Format("  outputindex={0} (len={1})", outputIndex, OutputCharSet.Length));

                    char targetChar = OutputCharSet[outputIndex];

                    sbOutput.Append(targetChar);

                    pixelOffSetLeft += pixelChunkWidth;
                }
                sbOutput.AppendLine();
                pixelOffSetTop += pixelChunkHeight;
            }

            // Dump output string to file
            File.WriteAllText(outputFile, sbOutput.ToString());
        }
    }
}
