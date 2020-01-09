﻿using Infoearth.Solution.MultiPointTile;
using Infoearth.Solution.MultiPointTile.DBHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace BiqugeSpeeker
{
    public partial class FormMain : Form
    {
        private IDbProvider dbProvider = DbFactory.CreateDbProvider();
        BaiduApi baiduApi = new BaiduApi();
        IWMPPlaylist playList = null;
        Dictionary<int, int[]> keyValuePairs = new Dictionary<int, int[]>();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            DataSet dataSet= dbProvider.ExecuteDataSet($"select BookName,Id from BookBasic");
            if (dataSet != null)
            {
                List<BookInfo> bookInfos = new List<BookInfo>();
                foreach (DataRow item in dataSet.Tables[0].Rows)
                {
                    bookInfos.Add(new BookInfo()
                    {
                        Id = item[1].ToString(),
                        BookName = item[0].ToString()
                    });
                }

                listBox1.DataSource = bookInfos;
                listBox1.DisplayMember = "BookName";
            }         
            playList= axWindowsMediaPlayer1.playlistCollection.newPlaylist("MyPlayList");
            axWindowsMediaPlayer1.currentPlaylist = playList;
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;

           // richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Underline | FontStyle.Bold);
            richTextBox1.SelectionBackColor = SystemColors.Highlight;
        }

        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //高亮文本
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsTransitioning)
            {
                HighlightText();
            }

            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                played++;
                //最后一个
                if (played > 0 && played == playList.count)
                {
                    //清空播放列表
                    playList.clear();
                    keyValuePairs.Clear();
                    played = 0;
                    richTextBox1.Text = string.Empty;
                    //删除播放文件
                    BookInfo bookInfo = bindingSource2.DataSource as BookInfo;
                    string dir = Path.Combine(Path.GetTempPath(), bookInfo.BookName);
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {

                    }
                    //自动播放下一章
                    bookInfo = listBox2.SelectedItem as BookInfo;
                    List<BookInfo> bookInfos = listBox2.DataSource as List<BookInfo>;
                    int index = bookInfos.IndexOf(bookInfo);
                    if (index > 0 && index < bookInfos.Count - 1)
                    {
                        button3_Click(null, null);
                        bookInfo = bindingSource2.DataSource as BookInfo;
                        if (!backgroundWorker1.IsBusy)
                            backgroundWorker1.RunWorkerAsync(bookInfo);
                    }
                    else
                    {
                        if (!backgroundWorker1.IsBusy)
                            backgroundWorker1.RunWorkerAsync(new BookInfo() { BookName = "当前目录已播放完" });
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlBookInfo.Visible = true;
            pnlList.Visible = false;
            pnlDetail.Visible = false;
            BookInfo bookInfo = listBox1.SelectedItem as BookInfo;
            DataSet dataSet = dbProvider.ExecuteDataSet($"select * from BookBasic where Id='{bookInfo.Id}'");
            if (dataSet != null)
            {
                DataRow dataRow = dataSet.Tables[0].Rows[0];
                bookInfo.Author = dataRow["Author"].ToString();
                bookInfo.LatestChapter = dataRow["LatestChapter"].ToString();
                bookInfo.Desc1 = dataRow["Desc1"].ToString();
                try
                {
                    bookInfo.Image = (byte[])dataRow["Image"];
                    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
                    using (FileStream stream = new FileStream(tempPath, FileMode.Create))
                    {
                        stream.Write(bookInfo.Image, 0, bookInfo.Image.Length);
                    }
                    pictureBox1.Image = Image.FromFile(tempPath);
                }
                catch
                {

                }
            }
            bindingSource1.DataSource = bookInfo;
        }

        private void lblBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pnlBookInfo.Visible = true;
            pnlList.Visible = false;
            pnlDetail.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pnlBookInfo.Visible = false;
            pnlDetail.Visible = false;
            pnlList.Visible = true;
            BookInfo bookInfo = bindingSource1.DataSource as BookInfo;
            //初始化小说列表
            DataSet dataSet = dbProvider.ExecuteDataSet($"select Title,DId from BookContent where Id='{bookInfo.Id}' order by cast(Chapter as decimal(6,0))");
            List<BookInfo> bookInfos = new List<BookInfo>();
            foreach (DataRow item in dataSet.Tables[0].Rows)
            {
                bookInfos.Add(new BookInfo()
                {
                    Id = item[1].ToString(),
                    BookName = item[0].ToString()
                });
            }

            listBox2.DataSource = bookInfos;
            listBox2.DisplayMember = "BookName";
        }


        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pnlBookInfo.Visible = false;
            pnlDetail.Visible = false;
            pnlList.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<BookInfo> bookInfos = listBox2.DataSource as List<BookInfo>;
            BookInfo bookInfo = listBox2.SelectedItem as BookInfo;
            int index = bookInfos.IndexOf(bookInfo);
            if (index > 0)
            {
                listBox2.SelectedItem = bookInfos[index - 1];
                listBox2_DoubleClick(null, null);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<BookInfo> bookInfos = listBox2.DataSource as List<BookInfo>;
            BookInfo bookInfo = listBox2.SelectedItem as BookInfo;
            int index = bookInfos.IndexOf(bookInfo);
            if (index < bookInfos.Count - 1)
            {
                listBox2.SelectedItem = bookInfos[index + 1];
                listBox2_DoubleClick(null, null);
            }
        }

        private void SetDetail(string Id)
        {
            //初始化小说列表
            DataSet dataSet = dbProvider.ExecuteDataSet($"select * from BookContent where DId='{Id}'");
            DataRow dataRow = dataSet.Tables[0].Rows[0];
            BookInfo bookInfo = new BookInfo()
            {
                Desc1 = dataRow["Content"].ToString(),
                BookName = dataRow["Title"].ToString(),
            };
            bindingSource2.DataSource = bookInfo;
        }

        /// <summary>
        /// 高亮显示文本
        /// </summary>
        private void HighlightText()
        {
            if (keyValuePairs.ContainsKey(played))
            {
                int[] selected = keyValuePairs[played];
                if (richTextBox1.Text.Length >= selected[0] + selected[1])
                {
                    int index = richTextBox1.Find(richTextBox1.Text.Substring(selected[0], selected[1]));
                    if (index >= 0)
                    {
                        richTextBox1.SelectionStart = selected[0];
                        richTextBox1.SelectionLength = played < (keyValuePairs.Count - 1) ? (keyValuePairs[played + 1][0] - selected[0]) : selected[1];
                        //richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Regular);
                        richTextBox1.SelectionBackColor = SystemColors.Highlight;
                        richTextBox1.SelectionColor = Color.White;
                    }
                }
            }
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            pnlBookInfo.Visible = false;
            pnlDetail.Visible = true;
            pnlList.Visible = false;
            BookInfo bookInfo = listBox2.SelectedItem as BookInfo;
            SetDetail(bookInfo.Id);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //清空播放列表
            playList.clear();
            keyValuePairs.Clear();
            played = 0;
            //删除播放文件
            BookInfo bookInfo = bindingSource2.DataSource as BookInfo;
            string dir = Path.Combine(Path.GetTempPath(), bookInfo.BookName);
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {

            }
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
            backgroundWorker1.RunWorkerAsync(bookInfo);
         
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BookInfo bookInfo = e.Argument as BookInfo;
            string dir = Path.Combine(Path.GetTempPath(), bookInfo.BookName);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            int index = 0;
            int lastLen = 0;
            //播放章节标题
            string filePath = Path.Combine(dir, index + ".mp3");
            baiduApi.GetAudio(filePath, bookInfo.BookName);
            backgroundWorker1.ReportProgress(0, filePath);
            keyValuePairs.Add(index, new int[] { 0, 0 });
            index++;

            //请求资源
            if (!string.IsNullOrEmpty(bookInfo.Desc1))
            {
                foreach (var item in bookInfo.Desc1.Split('。', '，', '；', ',', '.'))
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    int len = item.Length;
                    for (int i = 0; i <= len / 50; i++)
                    {
                        string text = item.Substring(i * 50, Math.Min(item.Substring(i * 50).Length, 50));
                        filePath = Path.Combine(dir, index + ".mp3");
                        baiduApi.GetAudio(filePath, text);
                        backgroundWorker1.ReportProgress(0, filePath);
                        //播放文字长度
                        int start = bookInfo.Desc1.IndexOf(text, lastLen);
                        lastLen = start + Math.Min(item.Substring(i * 50).Length, 50);
                        keyValuePairs.Add(index, new int[] { start, Math.Min(item.Substring(i * 50).Length, 50) });
                        index++;
                    }
                }
            }
        }

        private int played = 0;

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string filePath = e.UserState.ToString();
            IWMPMedia media = axWindowsMediaPlayer1.newMedia(filePath); //参数为歌曲路径
            playList.appendItem(media);
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsReady)
            {
                //捕获异常 并忽略异常
                try
                {
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                catch (Exception)
                {

                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void 小说设置SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSetting formSetting = new FormSetting(this);
            formSetting.ShowDialog();
        }
    }

    public class BookInfo
    {
        public string BookName { get; set; }

        public string Id { get; set; }

        public byte[] Image { get; set; }

        public string Desc1 { get; set; }

        public string Author { get; set; }

        public string LatestChapter { get; set; }


    }

}