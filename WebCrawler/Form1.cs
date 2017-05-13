using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebCrawler
{
    public partial class Form1 : Form
    {
        List<Thread> threadList = new List<Thread>();
        Thread thread = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime dtStart = DateTime.Now;
            button3.Enabled = true;
            button2.Enabled = true;
            button1.Enabled = false;
            int page = 0;
            int count = 0;
            int personCount = 0;
            label1.Text = "已完成页数：0";
            int index = 0;

            for (int i = 1; i <= 10; i++)
            {
                thread = new Thread(new ParameterizedThreadStart(delegate (object obj)
                {
                    for (int j = 1; j <= 10; j++)
                    {
                        try
                        {
                            index = (Convert.ToInt32(obj) - 1) * 10 + j;
                            string pageHtml = HttpRequestUtil.GetPageHtml("http://tt.mop.com/c44/0/1_" + index.ToString() + ".html");
                            Regex regA = new Regex("<a[\\s]+class=\"J-userPic([^<>]*?)[\\s]+href=\"([^\"]*?)\"");
                            Regex regImg = new Regex("<p class=\"tc mb10\"><img[\\s]+src=\"([^\"]*?)\"");
                            MatchCollection mc = regA.Matches(pageHtml);
                            text(pageHtml.ToString());
                            foreach (Match match in mc)
                            {
                                int start = match.ToString().IndexOf("href=\"");
                                string url = match.ToString().Substring(start + 6);
                                int end = url.IndexOf("\"");
                                url = url.Substring(0, end);
                                if (url.IndexOf("/") == 0)
                                {
                                    string imgPageHtml = HttpRequestUtil.GetPageHtml("http://tt.mop.com" + url);
                                    personCount++;
                                    label2.Invoke(new Action(delegate () { label2.Text = "已完成条数：" + personCount.ToString(); }));
                                    MatchCollection mcImgPage = regImg.Matches(imgPageHtml);
                                    foreach (Match matchImgPage in mcImgPage)
                                    {
                                        start = matchImgPage.ToString().IndexOf("src=\"");
                                        string imgUrl = matchImgPage.ToString().Substring(start + 5);
                                        end = imgUrl.IndexOf("\"");
                                        imgUrl = imgUrl.Substring(0, end);
                                        if (imgUrl.IndexOf("http://i1") == 0)
                                        {
                                            try
                                            {
                                                HttpRequestUtil.HttpDownloadFile(imgUrl);
                                                count++;
                                                label3.Invoke(new Action(delegate ()
                                                {
                                                    label3.Text = "已下载图片数" + count.ToString();
                                                    DateTime dt = DateTime.Now;
                                                    double time = dt.Subtract(dtStart).TotalSeconds;
                                                    if (time > 0)
                                                    {
                                                        label4.Text = "速度：" + (count / time).ToString("0.0") + "张/秒";
                                                    }
                                                }));
                                            }
                                            catch { }
                                            Thread.Sleep(1);
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        page++;
                        label1.Invoke(new Action(delegate () { label1.Text = "已完成页数：" + page.ToString(); }));

                        if (page == 100)
                        {
                            button1.Invoke(new Action(delegate () { button1.Enabled = true; }));
                            MessageBox.Show("完成！");
                        }
                    }
                }));
                thread.Start(i);
                threadList.Add(thread);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Invoke(new Action(delegate ()
            {
                foreach (Thread thread in threadList)
                {
                    if (thread.ThreadState == ThreadState.Suspended)
                    {
                        thread.Resume();
                    }
                    thread.Abort();
                }
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (Thread thread in threadList)
            {
                if (thread.ThreadState == ThreadState.Running)
                {
                    thread.Suspend();
                }
            }
            button3.Enabled = false;
            button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Thread thread in threadList)
            {
                if (thread.ThreadState == ThreadState.Suspended)
                {
                    thread.Resume();
                }
            }
            button3.Enabled = true;
            button4.Enabled = false;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Thread thread in threadList)
            {
                thread.Abort();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 写入文本文件
        /// </summary>
        /// <param name="value"></param>
        public void text(string value)
        {
            string path = Application.StartupPath + "\\download\\TextMessage.txt";
            FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(value);
            sw.Flush();
            sw.Close();
            f.Close();
        }
    }
}
