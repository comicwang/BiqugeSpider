using Infoearth.Solution.MultiPointTile;
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
        //Dictionary<int, int[]> keyValuePairs = new Dictionary<int, int[]>();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            DataSet dataSet = dbProvider.ExecuteDataSet($"select BookName,Id,Catelog,LatestTime from BookBasic order by LatestTime");
            if (dataSet != null)
            {
                List<BookInfo> bookInfos = new List<BookInfo>();
                foreach (DataRow item in dataSet.Tables[0].Rows)
                {
                    bookInfos.Add(new BookInfo()
                    {
                        Id = item[1].ToString(),
                        BookName = item[0].ToString(),
                        Catelog = item[2].ToString()
                    });
                }

                listBox1.DataSource = bookInfos;
                listBox1.DisplayMember = "BookName";

                comboBox1.Items.Add("所有类别");
                comboBox1.Items.AddRange(bookInfos.Select(t => t.Catelog).Distinct().ToArray());
                comboBox1.Text = "所有类别";
            }

            playList = axWindowsMediaPlayer1.playlistCollection.newPlaylist("MyPlayList");
            axWindowsMediaPlayer1.currentPlaylist = playList;
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;

            // richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Underline | FontStyle.Bold);
            richTextBox1.SelectionBackColor = SystemColors.Highlight;
        }

        private bool isplayed = false;

        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //高亮文本
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsTransitioning)
            {
                HighlightText();
            }

            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                isplayed = true;
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
                bookInfo.Catelog = dataRow["Catelog"].ToString();
                bookInfo.Desc1 = dataRow["Desc1"].ToString();
                if (dataRow["LatestTime"] != null)
                    bookInfo.LatestTime = DateTime.Parse(dataRow["LatestTime"].ToString());
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
            SetChapterLst();
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

        /// <summary>
        /// 获取章节详细信息
        /// </summary>
        /// <param name="Id"></param>
        private void SetDetail(string Id)
        {
            //初始化小说列表
            DataSet dataSet = dbProvider.ExecuteDataSet($"select * from BookContent where DId='{Id}'");
            DataRow dataRow = dataSet.Tables[0].Rows[0];
            BookInfo bookInfo = new BookInfo()
            {
                Desc1 = dataRow["Content"].ToString().Replace("笔趣阁手机端  http://m.biquwu.cc    ", ""),
                BookName = dataRow["Title"].ToString(),
            };
            bindingSource2.DataSource = bookInfo;
        }

        /// <summary>
        /// 获取章节信息
        /// </summary>
        /// <param name="order"></param>
        private void SetChapterLst(string order = "asc")
        {
            BookInfo bookInfo = bindingSource1.DataSource as BookInfo;
            //初始化小说列表
            DataSet dataSet = dbProvider.ExecuteDataSet($"select Title,DId from BookContent where Id='{bookInfo.Id}' order by cast(Chapter as decimal(6,0)) {order}");
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

        /// <summary>
        /// 高亮显示文本
        /// </summary>
        private void HighlightText()
        {
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectionLength = lastIndex;
            //richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Regular);
            richTextBox1.SelectionBackColor = SystemColors.Highlight;
            richTextBox1.SelectionColor = Color.White;
        }

        /// <summary>
        /// 获取书本类别信息
        /// </summary>
        /// <param name="order"></param>
        private void GetBookList(string order="desc")
        {
            string catelog = comboBox1.SelectedItem.ToString();
            string sql = $"select BookName,Id,Catelog from BookBasic where 1=1";
            if (catelog != "所有类别")
            {
                sql = $"select BookName,Id,Catelog,LatestTime from BookBasic where Catelog='{catelog}'";
            }
            string text = textBox2.Text;
            if (text != "请输入小说名")
            {
                sql += $" and BookName like '%{text}%'";
            }
            sql += $" order by LatestTime {order}";

            DataSet dataSet = dbProvider.ExecuteDataSet(sql);
            if (dataSet != null)
            {
                List<BookInfo> bookInfos = new List<BookInfo>();
                foreach (DataRow item in dataSet.Tables[0].Rows)
                {
                    bookInfos.Add(new BookInfo()
                    {
                        Id = item[1].ToString(),
                        BookName = item[0].ToString(),
                        Catelog = item[2].ToString()
                    });
                }

                listBox1.DataSource = bookInfos;
            }
        }

        /// <summary>
        /// 播放智能语音
        /// </summary>
        private void PlayBook()
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
            else
            {
                BookInfo bookInfo = bindingSource2.DataSource as BookInfo;
                backgroundWorker1.RunWorkerAsync(bookInfo);
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
            if (button5.Text=="开始朗读")
            {
                isplay = true;
                PlayBook();
                button5.Text = "停止朗读";
            }
            else
            {
                isplay = false;
                axWindowsMediaPlayer1.Ctlcontrols.pause();
                backgroundWorker1.CancelAsync();
                button5.Text = "开始朗读";
            }
        }

        private bool isplay = false;

        private int lastIndex = 0;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BookInfo bookInfo = e.Argument as BookInfo;
            string dir = Path.Combine(Path.GetTempPath(), bookInfo.BookName);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            int index = 0;
            lastIndex = 0;
            //播放章节标题
            string filePath = Path.Combine(dir, index + ".mp3");
            baiduApi.GetAudio(filePath, bookInfo.BookName);
            backgroundWorker1.ReportProgress(0, filePath);
            index++;

            //请求资源
            if (!string.IsNullOrEmpty(bookInfo.Desc1))
            {
                string[] content = bookInfo.Desc1.Split('。', '，', '；', ',', '.');
                int current = 1;
                foreach (var txt in content)
                {
                    string item = txt.Trim();
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    int len = item.Length;
                    for (int i = 0; i <= len / 50; i++)
                    {
                        try
                        {
                            isplayed = false;
                            string text = item.Substring(i * 50, Math.Min(item.Substring(i * 50).Length, 50));
                            if (Directory.Exists(dir) == false)
                            {
                                Directory.CreateDirectory(dir);
                            }
                            filePath = Path.Combine(dir, index + ".mp3");
                            baiduApi.GetAudio(filePath, text);
                            //播放文字长度                       
                            backgroundWorker1.ReportProgress((int)(current * 100 / content.Length), filePath);
                            while (isplayed == false)
                            {
                                if (backgroundWorker1.CancellationPending)
                                {
                                    e.Cancel = true;
                                    return;
                                }
                            }
                            int start = bookInfo.Desc1.IndexOf(text, lastIndex);
                            lastIndex = start + Math.Min(item.Substring(i * 50).Length, 50);
                            index++;
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                    current++;
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Value = e.ProgressPercentage;
            string filePath = e.UserState.ToString();
            lastFilePath = filePath;
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

        /// <summary>
        /// 最近的播放文件路径
        /// </summary>
        private string lastFilePath = string.Empty;

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            //清空播放列表
            playList.clear();

            //删除上次播放目录
            if (File.Exists(lastFilePath))
            {
                string dir = Path.GetDirectoryName(lastFilePath);
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {

                }
            }

            if (isplay)
            {
                BookInfo bookInfo = bindingSource2.DataSource as BookInfo;
                if (e.Cancelled)
                {
                    if (!backgroundWorker1.IsBusy)
                        backgroundWorker1.RunWorkerAsync(bookInfo);
                }
                else
                {
                    //自动播放下一章
                    List<BookInfo> bookInfos = listBox2.DataSource as List<BookInfo>;
                    int index = bookInfos.IndexOf(bookInfo);
                    if (index > 0 && index < bookInfos.Count - 1)
                    {
                        listBox2.SelectedItem = bookInfos[index + 1];
                        SetDetail(bookInfo.Id);
                        if (!backgroundWorker1.IsBusy)
                            backgroundWorker1.RunWorkerAsync(bookInfos[index + 1]);
                    }
                    else
                    {
                        if (!backgroundWorker1.IsBusy)
                        {
                            backgroundWorker1.RunWorkerAsync(new BookInfo() { BookName = "当前目录已播放完" });
                            isplay = false;
                        }
                    }
                }
            }
        }

        private void 小说设置SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSetting formSetting = new FormSetting(this);
            formSetting.ShowDialog();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            GetBookList();
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "请输入小说名")
            {
                textBox2.Text = string.Empty;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text = "请输入小说名";
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = sender as LinkLabel;
            SetChapterLst(linkLabel.Name.Contains("1") ? "asc" : "desc");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetBookList();
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel linkLabel = sender as LinkLabel;
            GetBookList(linkLabel.Name.Contains("5") ? "asc" : "desc");
        }

        private void bindingSource2_DataSourceChanged(object sender, EventArgs e)
        {
            if (isplay)
            {
                PlayBook();
            }
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

        public DateTime LatestTime { get; set; }

        public string Catelog { get; set; }


    }

}
