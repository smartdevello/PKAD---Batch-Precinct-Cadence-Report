using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PKAD___Batch_Precinct_Cadence_Report
{
    public class BatchPrecinctRenderer
    {
        private int width = 0, height = 0;
        private double totHeight = 1000;
        private Bitmap bmp = null;
        private Graphics gfx = null;
        private List<BatchPrecinctData> data = null;
        private Dictionary<string, int> printers = null;
        private Dictionary<string, int> left_info_dict = null;

        Image logoImg = Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "assets", "logo.png"));

        public BatchPrecinctRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public void setRenderSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public Dictionary<string, int> getPrinters()
        {
            return this.printers;
        }
        public int getPrintersCount()
        {
            if (this.printers == null) return 0;
            return this.printers.Count();
        }

        public List<BatchPrecinctData> getData()
        {
            return this.data;
        }

        public void setChatData(List<BatchPrecinctData> data, Dictionary<string, int> pp, Dictionary<string, int> qq)
        {
            this.data = data;
            this.printers = pp;
            this.left_info_dict = qq;
        }

        public Point convertCoord(Point a)
        {
            double px = height / totHeight;

            Point res = new Point();
            res.X = (int)((a.X + 20) * px);
            res.Y = (int)((1000 - a.Y) * px);
            return res;
        }
        public PointF convertCoord(PointF p)
        {
            double px = height / totHeight;
            PointF res = new PointF();
            res.X = (int)((p.X + 20) * px);
            res.Y = (int)((1000 - p.Y) * px);
            return res;
        }
        public Bitmap getBmp()
        {
            return this.bmp;
        }
        public void drawCenteredString(string content, Rectangle rect, Brush brush, Font font)
        {

            //using (Font font1 = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Point))

            // Create a StringFormat object with the each line of text, and the block
            // of text centered on the page.
            double px = height / totHeight;
            rect.Location = convertCoord(rect.Location);
            rect.Width = (int)(px * rect.Width);
            rect.Height = (int)(px * rect.Height);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            // Draw the text and the surrounding rectangle.
            gfx.DrawString(content, font, brush, rect, stringFormat);
            //gfx.DrawRectangle(Pens.Black, rect);

        }
        private void fillPolygon(Brush brush, PointF[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = convertCoord(points[i]);
            }
            gfx.FillPolygon(brush, points);
        }

        public void drawLine(Point p1, Point p2, Pen pen)
        {

            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(pen, p1, p2);
        }
        public void drawLine(Point p1, Point p2, Color color, int linethickness = 1)
        {
            if (color == null)
                color = Color.Gray;

            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(new Pen(color, linethickness), p1, p2);

        }
        public void drawString(Point o, string content, int font = 15)
        {

            double px = height / totHeight;
            o = convertCoord(o);

            // Create font and brush.
            Font drawFont = new Font("Arial", font);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            gfx.DrawString(content, drawFont, drawBrush, o.X, o.Y);

        }
        public void drawString(Color color, Point o, string content, int font = 15)
        {

            double px = height / totHeight;
            o = convertCoord(o);

            // Create font and brush.
            Font drawFont = new Font("Arial", font);
            SolidBrush drawBrush = new SolidBrush(color);

            gfx.DrawString(content, drawFont, drawBrush, o.X, o.Y);

            drawFont.Dispose();
            drawBrush.Dispose();

        }
        public void drawPie(Color color, Point o, Size size, float startAngle, float sweepAngle, string content = "")
        {
            // Create location and size of ellipse.
            double px = height / totHeight;
            size.Width = (int)(size.Width * px);
            size.Height = (int)(size.Height * px);

            Rectangle rect = new Rectangle(convertCoord(o), size);
            // Draw pie to screen.            
            Brush grayBrush = new SolidBrush(color);
            gfx.FillPie(grayBrush, rect, startAngle, sweepAngle);

            o.X += size.Width / 2;
            o.Y -= size.Height / 3;
            float radius = size.Width * 0.3f;
            o.X += (int)(radius * Math.Cos(Helper.DegreesToRadians(startAngle + sweepAngle / 2)));
            o.Y -= (int)(radius * Math.Sin(Helper.DegreesToRadians(startAngle + sweepAngle / 2)));
            if (!string.IsNullOrEmpty(content))
            {
                int percent = (int)(sweepAngle * 100.0f / 360.0f);
                content = percent.ToString() + "%";
                Font numberFont = new Font("Arial", 20, FontStyle.Bold, GraphicsUnit.Point);
                drawCenteredString(content, new Rectangle(o.X, o.Y, size.Width / 2 + 50, 50), Brushes.White, numberFont);
                numberFont.Dispose();
            }

        }
        public void drawCenteredString_withBorder(string content, Rectangle rect, Brush brush, Font font, Color borderColor)
        {

            //using (Font font1 = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Point))

            // Create a StringFormat object with the each line of text, and the block
            // of text centered on the page.
            double px = height / totHeight;
            rect.Location = convertCoord(rect.Location);
            rect.Width = (int)(px * rect.Width);
            rect.Height = (int)(px * rect.Height);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            // Draw the text and the surrounding rectangle.
            gfx.DrawString(content, font, brush, rect, stringFormat);

            Pen borderPen = new Pen(new SolidBrush(borderColor), 15);
            gfx.DrawRectangle(borderPen, rect);
            borderPen.Dispose();
        }



        public void fillRectangle(Color color, Rectangle rect)
        {
            rect.Location = convertCoord(rect.Location);
            double px = height / totHeight;
            rect.Width = (int)(rect.Width * px);
            rect.Height = (int)(rect.Height * px);

            Brush brush = new SolidBrush(color);
            gfx.FillRectangle(brush, rect);
            brush.Dispose();

        }
        public void drawRectangle(Pen pen, Rectangle rect)
        {
            rect.Location = convertCoord(rect.Location);
            double px = height / totHeight;
            rect.Width = (int)(rect.Width * px);
            rect.Height = (int)(rect.Height * px);
            gfx.DrawRectangle(pen, rect);
        }
        public void drawImg(Image img, Point o, Size size)
        {
            double px = height / totHeight;
            o = convertCoord(o);
            Rectangle rect = new Rectangle(o, new Size((int)(size.Width * px), (int)(size.Height * px)));
            gfx.DrawImage(img, rect);

        }
        public void drawNumberwithUnderLine_Content(string number, string content, Color color, Point o, int fontSize, int marginX = 0, int marginY = 0, int contentWidth = 200, int contentHeight = 70, int contentFontsize = 13)
        {


            // Create font and brush.
            //Font drawFont = new Font("Arial", fontSize, FontStyle.Bold | FontStyle.Underline);
            //SolidBrush drawBrush = new SolidBrush(color);
            //drawCenteredString(content, new Rectangle(o.X - marginX, o.Y - marginY, contentWidth, contentHeight), drawBrush, contentFontsize);

            //double px = height / totHeight;
            //o = convertCoord(o);            
            //gfx.DrawString(number, drawFont, drawBrush, o.X, o.Y);            

            //drawFont.Dispose();
            //drawBrush.Dispose();
        }
        private void drawPercentageLine(float percent, int X, int Y, Color color)
        {
            fillRectangle(color, new Rectangle(X, Y, 20, 20));
            fillRectangle(color, new Rectangle(X + 80, Y, 20, 20));
            drawLine(new Point(X, Y - 10), new Point(X + 80, Y - 10), color, 4);
            if (percent != 0.0F)
            {
                drawString(new Point(X + 30, Y + 10), string.Format("{0:F}%", percent), 8);
            }

        }

        public void draw(int currentChartIndex)
        {
            if (bmp == null)
                bmp = new Bitmap(width, height);
            else
            {
                if (bmp.Width != width || bmp.Height != height)
                {
                    bmp.Dispose();
                    bmp = new Bitmap(width, height);

                    gfx.Dispose();
                    gfx = Graphics.FromImage(bmp);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                }
            }
            if (gfx == null)
            {
                gfx = Graphics.FromImage(bmp);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            else
            {
                gfx.Clear(Color.Transparent);
            }

            drawImg(logoImg, new Point(20, 60), new Size(150, 50));

            if (data == null) return;
            float percentage = 0;

            Font numberFont = new Font("Arial", 35, FontStyle.Bold, GraphicsUnit.Point);
            Font textFont = new Font("Arial", 14, FontStyle.Bold, GraphicsUnit.Point);
            Font titleFont1 = new Font("Arial", 21, FontStyle.Bold, GraphicsUnit.Point);
            Font titleFont2 = new Font("Arial", 18, FontStyle.Bold, GraphicsUnit.Point);
            Font scaleFont = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point);
            Font percentFont = new Font("Arial", 21, FontStyle.Bold, GraphicsUnit.Point);

            drawCenteredString(data.Count().ToString(), new Rectangle(0, 920, 200, 80), Brushes.Red, numberFont);
            drawCenteredString("Ballots", new Rectangle(0, 860, 200, 50), Brushes.Black, titleFont1);
            drawCenteredString("Examined", new Rectangle(0, 820, 200, 50), Brushes.Black, titleFont2);

            int ev_count = 0, tot_count = 0;
            foreach(var item in data)
            {
                if (string.IsNullOrEmpty(item.left_info)) continue;
                string[] words = item.left_info.Split(' ');
                if (words.Length != 2) continue;
                if (!Regex.IsMatch(words[1], @"^[a-zA-Z]+$")) continue;
                tot_count++;

                if (words[1].ToUpper().StartsWith("EV")) ev_count++;
                else
                {
                    int x = 0;
                }
            }
            if (tot_count != 0)
                percentage = ev_count * 100 / (float)tot_count;
            else percentage = 0;

            drawCenteredString(string.Format("{0}%", Math.Round(percentage, 1)), new Rectangle(0, 750, 200, 80), Brushes.Red, numberFont);
            drawCenteredString("Early Vote", new Rectangle(0, 690, 200, 50), Brushes.Black, titleFont1);
            drawCenteredString("Ballots", new Rectangle(0, 650, 200, 50), Brushes.Black, titleFont2);

            drawCenteredString(tot_count.ToString(), new Rectangle(0, 580, 200, 50), Brushes.Red, numberFont);
            drawCenteredString("Precincts Reporting", new Rectangle(0, 530, 200, 100), Brushes.Black, titleFont1);


            Pen pen4= new Pen(Color.Black, 4);            
            drawLine(new Point(300, 650), new Point(1800, 650), pen4);

            Pen pen1 = new Pen(Color.Black, 1);
            pen1.DashCap = System.Drawing.Drawing2D.DashCap.Round;
            pen1.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

            for (int i = 1; i<=10; i++)
            {
                drawLine(new Point(300, 650 + 20 * i), new Point(1800, 650 + 20 * i), pen1);
                drawString(new Point(270, 655 + 20 * i), i.ToString(), 10);
                drawString(new Point(1810, 655 + 20 * i), i.ToString(), 10);

            }


            
            
            int height_gap = 40;
            int width_gap = 300;

            var sorted_printers = printers.OrderByDescending(o => o.Value);
            int len = sorted_printers.Count();

            int barcodeX = 0, barcodeY = 0;
            Random rnd = new Random();
            Color color = new Color();
            int prev_cnt = 0;
            for (int i = currentChartIndex * 70; i < Math.Min(sorted_printers.Count(), currentChartIndex * 70 + 70); i++)
            {
                if (sorted_printers.ElementAt(i).Value != prev_cnt)
                {
                    color = Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
                    prev_cnt = sorted_printers.ElementAt(i).Value;
                }

                int j = i % 70;
                drawString(new Point(300 + width_gap * (j / 14), 600 - height_gap * (j % 14)), sorted_printers.ElementAt(i).Key, 8);
                percentage = sorted_printers.ElementAt(i).Value * 100 / (float)data.Count;

                drawPercentageLine(percentage, 470 + width_gap * (j / 14), 600 - height_gap * (j % 14), color);
                barcodeX = 320 + j * 21;

                if (sorted_printers.ElementAt(i).Value <= 10)
                    barcodeY = 650 + sorted_printers.ElementAt(i).Value * 20;
                else
                {
                    barcodeY = 880;
                    drawString(new Point(barcodeX - 10, 900), sorted_printers.ElementAt(i).Value.ToString(), 10);
                }
                drawLine(new Point(barcodeX, 650), new Point(barcodeX, barcodeY), color, 4);
                
            }



            pen4.Dispose();
            pen1.Dispose();

            numberFont.Dispose();
            textFont.Dispose();
            titleFont1.Dispose();
            titleFont2.Dispose();
            scaleFont.Dispose();
            percentFont.Dispose();


        }

    }
}
