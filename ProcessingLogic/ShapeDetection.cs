using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingLogic
{
    public class ShapeDetection
    {
        public Bitmap GetShapeDetectionImage(Bitmap bitmap)
        {
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            // apply the filter

            ColorFiltering colorFilter = new ColorFiltering();

            colorFilter.Red = new IntRange(0, 64);
            colorFilter.Green = new IntRange(0, 64);
            colorFilter.Blue = new IntRange(0, 64);
            colorFilter.FillOutsideRange = false;

            colorFilter.ApplyInPlace(bitmap);

            Bitmap grayImage = filter.Apply(bitmap);

            //DifferenceEdgeDetector filter2 = new DifferenceEdgeDetector();
            //filter2.ApplyInPlace(grayImage);
            //FillHoles filter2 = new FillHoles();
            //filter2.MaxHoleHeight = 20;
            //filter2.MaxHoleWidth = 20;
            //filter2.CoupledSizeFiltering = false;
            //filter2.ApplyInPlace(grayImage);
            var filter2 = new Mean();
            filter2.ApplyInPlace(grayImage);
            filter2.ApplyInPlace(grayImage);
            filter2.ApplyInPlace(grayImage);

            //var filter4 = new Median();
            //filter4.ApplyInPlace(grayImage);
            // step 1 - turn background to black
            // create and configure the filter
           

            var filter3 = new SobelEdgeDetector();
            // apply the filter
            filter3.ApplyInPlace(grayImage);

            Threshold threshold = new Threshold(50);
            // apply the filter
            Bitmap binary =  threshold.Apply(grayImage);

            FillHoles fill = new FillHoles();
            fill.MaxHoleHeight = 50;
            fill.MaxHoleWidth = 50;
            fill.CoupledSizeFiltering = true;
            // apply the filter
            Bitmap filled = fill.Apply(binary);
            //filled = fill.Apply(filled);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.CoupledSizeFiltering = false;
            blobCounter.MinHeight = 50;
            blobCounter.MinWidth = 20;
            blobCounter.FilterBlobs = true;
            blobCounter.ProcessImage(filled);

            var blobs = blobCounter.GetObjectsInformation();
            var shapeChecker = new SimpleShapeChecker();
            
            Graphics g = Graphics.FromImage(bitmap);
            Pen redPen = new Pen(Color.Red, 2);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                List<IntPoint> corners;
                //List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

                shapeChecker.LengthError = 0.8f;
                if (edgePoints.Count > 4)//&& PointsCloud.FindQuadrilateralCorners(edgePoints))
                {
                    corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
                    // get length of 2 adjacent sides
                    //float side1Length = (float)corners[0].DistanceTo(corners[1]);
                    //float side2Length = (float)corners[0].DistanceTo(corners[3]);
                    //if (side1Length > 10 && side2Length > 10)
                    {
                        g.DrawPolygon(redPen, ToPointsArray(corners));
                    }
                }
            }

            redPen.Dispose();
            g.Dispose();
            return bitmap;
        }

        private PointF[] ToPointsArray(List<IntPoint> corners)
        {
            PointF[] points = new PointF[corners.Count];
            for (int i= 0; i < corners.Count; i++)
            {
                points[i] = new PointF(corners[i].X, corners[i].Y);
            }

            return points;
        }
    }
}
