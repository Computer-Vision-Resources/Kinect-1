﻿using System;
using System.Collections.Generic;
using System.IO;
using xn;

namespace Kinect.Core.Gestures
{
    public class ClickGesture : GestureBase
    {
        private List<Point3D> list;
        private static int PointCount = 50;
        private static float MarginX = 200;
        private static float MarginY = 200;
        private static float MaxDepth = 175;
        private static float MinDepth = -175;
        private static int SingleClickWaitCount = 100;
        private int clickWait = 0;

        public const string LogFile = @"c:\temp\LogFile.txt";

        public delegate void MouseClickHandler();

        public event MouseClickHandler LeftClick;

        public event MouseClickHandler RightClick;

        public int LastCheck{ get; private set; }

        public ClickGesture()
            : base()
        {
            list = new List<Point3D>();
        }

        public void AddPoint(int coordinateX, int coordinateY, int coordinateZ)
        {
            AddPoint(new Point3D(coordinateX, coordinateY, coordinateZ));
        }

        public void AddPoint(Point3D point)
        {
            list.Add(point);
            if (list.Count > PointCount)
            {
                list.RemoveAt(0);
            }

            if (clickWait <= 0)
            {
                CheckLeftClick();
            }
            else
            {
                clickWait--;
            }
        }

        private void CheckLeftClick()
        {
            if (LeftClick != null)
            {
                float down = 0;
                float up = 0;
                Point3D highest = list[0];
                Point3D lowest = list[0];

                bool clicked = false;
                foreach (var point in list)
                {
                    float isLowest = CheckPoint(point, highest);
                    float backHigh = CheckPoint(point, lowest);

                    if (isLowest == 0)
                    {
                        highest = point;
                        lowest = point;
                        up = 0;
                        down = 0;
                        continue;
                    }

                    if (down <= MinDepth && (isLowest == 0 && backHigh == 0))
                    {
                        highest = point;
                        lowest = point;
                        up = 0;
                        down = 0;
                        continue;
                    }
                    
                    if (isLowest < 0 && isLowest < down)
                    {
                        down = isLowest;
                        lowest = point;
                    }

                    if (down <= MinDepth && backHigh > 0 && backHigh > up)
                    {
                        up = backHigh;
                    }

                    if (up >= MaxDepth)
                    {
                        clicked = true;
                        break;
                    }
                }

                if (clicked)
                {
                    clickWait = SingleClickWaitCount;
                    LeftClick.Invoke();
                }
            }
        }

        private void CheckRightClick()
        {
            if (RightClick != null)
            {
                float down = 0;
                float up = 0;
                Point3D highest = list[0];
                Point3D lowest = list[0];

                bool clicked = false;
                foreach (var point in list)
                {
                    float isLowest = CheckPoint(point, highest);
                    float backHigh = CheckPoint(point, lowest);

                    if (isLowest == 0)
                    {
                        highest = point;
                        lowest = point;
                        up = 0;
                        down = 0;
                        continue;
                    }

                    if (down <= MinDepth && (isLowest == 0 && backHigh == 0))
                    {
                        highest = point;
                        lowest = point;
                        up = 0;
                        down = 0;
                        continue;
                    }

                    if (isLowest < 0 && isLowest < down)
                    {
                        down = isLowest;
                        lowest = point;
                    }

                    if (down <= MinDepth && backHigh > 0 && backHigh > up)
                    {
                        up = backHigh;
                    }

                    if (up >= MaxDepth)
                    {
                        clicked = true;
                        break;
                    }
                }

                if (clicked)
                {
                    clickWait = SingleClickWaitCount;
                    RightClick.Invoke();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="previous"></param>
        /// <returns>0 == No match, 1 == Up, -1 == Down</returns>
        private float CheckPoint(Point3D point, Point3D previous)
        {
            if (WithinMargin(point.X, previous.X, MarginX) &&
                WithinMargin(point.Y, previous.Y, MarginY))
            {
                return point.Z - previous.Z;
            }
            return 0;
        }

        private bool WithinMargin(float point, float previous, float margin)
        {
            return point - margin <= previous &&
                   point + margin >= previous;
        }

        public void WriteToLogFile()
        {
            // Create a writer and open the file:
            StreamWriter log;

            if (!File.Exists(LogFile))
            {
                log = new StreamWriter(LogFile); 
            }
            else
            {
                log = File.AppendText(LogFile);
            }

            // Write to the file:
            foreach (Point3D p in list)
            {
                log.WriteLine(p);
            }

            log.WriteLine();

            // Close the stream:

            // TODO: Use flush and use Try / finally. Can you use Using? This method smell expensive...
            log.Close();
        }

        protected override string GestureName
        {
            get { return "ClickGesture"; }
        }

        public override void Process(IUserChangedEvent evt)
        {
            throw new NotImplementedException();
        }
    }
}