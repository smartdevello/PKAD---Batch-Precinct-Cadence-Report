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
        Image magnify_glassImg = Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "assets", "magnifying-glass.png"));


        private Dictionary<int, Color> colorDic  = null;
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

            colorDic = new Dictionary<int, Color>();
            var sorted_printers = printers.OrderByDescending(o => o.Value);
            int prev_cnt = 0;
            Random rnd = new Random();


            for (int i = 0; i < sorted_printers.Count(); i++)
            {
                if (sorted_printers.ElementAt(i).Value != prev_cnt)
                {
                    prev_cnt = sorted_printers.ElementAt(i).Value;
                    if (!colorDic.ContainsKey(prev_cnt))
                    {
                        colorDic[prev_cnt] = System.Drawing.Color.FromArgb(rnd.Next(0, 200), rnd.Next(0, 200), rnd.Next(0, 200));
                    }
                }
            }
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
        protected void RenderDropshadowText(string text, Font font, Color foreground, Color shadow,  int shadowAlpha, PointF location)
        {
            const int DISTANCE = 2;
            for (int offset = 1; 0 <= offset; offset--)
            {
                Color color = ((offset < 1) ?
                    foreground : Color.FromArgb(shadowAlpha, shadow));
                using (var brush = new SolidBrush(color))
                {
                    var point = new PointF()
                    {
                        X = location.X + (offset * DISTANCE),
                        Y = location.Y + (offset * DISTANCE)
                    };
                    point = convertCoord(point);
                    gfx.DrawString(text, font, brush, point);
                }
            }
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
            Font numberFont2 = new Font("Arial", 25, FontStyle.Bold, GraphicsUnit.Point);
            Font textFont = new Font("Arial", 14, FontStyle.Bold, GraphicsUnit.Point);
            Font titleFont1 = new Font("Arial", 21, FontStyle.Bold, GraphicsUnit.Point);
            Font titleFont2 = new Font("Arial", 18, FontStyle.Bold, GraphicsUnit.Point);
            Font scaleFont = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point);
            Font copyrightFont = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Point);
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

            //drawCenteredString(tot_count.ToString(), new Rectangle(0, 580, 200, 50), Brushes.Red, numberFont);
            //drawCenteredString("Precincts Reporting", new Rectangle(0, 530, 200, 100), Brushes.Black, titleFont1);


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
           

            drawCenteredString(sorted_printers.Count().ToString(), new Rectangle(0, 580, 200, 50), Brushes.Red, numberFont);
            drawCenteredString("Precincts Reporting", new Rectangle(0, 530, 200, 100), Brushes.Black, titleFont1);






            int barcodeX = 0, barcodeY = 0;
            int prev_cnt = 0;


            int group_count_more5 = 0, count_more5 = 0;

            Dictionary<int, int> dic_more5 = new Dictionary<int, int>();
            for (int i = 0; i <sorted_printers.Count(); i++)
            {
                int key = sorted_printers.ElementAt(i).Value;

                if (dic_more5.ContainsKey(key))
                {
                    dic_more5[key]++;
                }
                else dic_more5[key] = 1;
            }
            foreach(KeyValuePair<int, int> entry in dic_more5)
            {
                if (entry.Value >= 5)
                {
                    count_more5 += entry.Value;
                    group_count_more5++;
                }
             
            }

            prev_cnt = 0;
            int len = 0;
            for (int i = currentChartIndex * 70; i < Math.Min(sorted_printers.Count(), currentChartIndex * 70 + 70); i++)
            {
                len++;
                if (sorted_printers.ElementAt(i).Value != prev_cnt)
                {
                    prev_cnt = sorted_printers.ElementAt(i).Value;
                }
                int j = i % 70;
                drawString(new Point(300 + width_gap * (j / 14), 600 - height_gap * (j % 14)), sorted_printers.ElementAt(i).Key, 8);
                percentage = sorted_printers.ElementAt(i).Value * 100 / (float)data.Count;

                drawPercentageLine(percentage, 470 + width_gap * (j / 14), 600 - height_gap * (j % 14), colorDic[prev_cnt]);
                barcodeX = 320 + j * 21;

                if (sorted_printers.ElementAt(i).Value <= 10)
                    barcodeY = 650 + sorted_printers.ElementAt(i).Value * 20;
                else
                {
                    barcodeY = 880;
                    drawString(new Point(barcodeX - 10, 900), sorted_printers.ElementAt(i).Value.ToString(), 10);
                }
                drawLine(new Point(barcodeX, 650), new Point(barcodeX, barcodeY), colorDic[prev_cnt], 4);
                
            }



            drawCenteredString(group_count_more5.ToString(), new Rectangle(0, 400, 200, 50), Brushes.Red, numberFont);
            drawImg(magnify_glassImg, new Point(40, 450), new Size(200, 180));
            
            int percent = (int)(count_more5 * 100 / sorted_printers.Count());

            drawCenteredString(percent.ToString() + "%", new Rectangle(120, 400, 200, 50), Brushes.Red, numberFont2);
            drawCenteredString("Measured\nRythms\nDETECTED", new Rectangle(0, 320, 200, 100), Brushes.Red, titleFont1);


            //drawCenteredString(string.Format("ppp{0:0.####}", sorted_printers.Count() * 100 / (float)743), new Rectangle(0, 120, 180, 100), Brushes.Orange, textFont);

            RenderDropshadowText(string.Format("ppp{0:0.####}", sorted_printers.Count() * 100 / (float)743), textFont, Color.Orange, Color.Black, 50, new PointF(30, 90));


            int chartCount = 1;
            chartCount = chartCount + sorted_printers.Count() / 70;
            if (sorted_printers.Count() % 70 == 0) chartCount--;


            drawCenteredString(string.Format("{0} of {1}", currentChartIndex + 1, chartCount), new Rectangle(1000, 70, 400, 100), Brushes.Red, numberFont2);
            drawCenteredString("© 2021 Tesla Laboratories, llc & JHP", new Rectangle(1500, 70, 400, 100), Brushes.Black, copyrightFont);

            pen4.Dispose();
            pen1.Dispose();

            numberFont.Dispose();
            numberFont2.Dispose();
            textFont.Dispose();
            titleFont1.Dispose();
            titleFont2.Dispose();
            scaleFont.Dispose();
            percentFont.Dispose();
            copyrightFont.Dispose();

        }

    }
}
