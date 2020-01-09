using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiqugeSpeeker
{
    public partial class FormSetting : Form
    {
        private RedisConfigInfo redisConfigInfo = RedisConfigInfo.GetRedisConfigInfo();
        private FormMain _mainForm = null;
        public FormSetting(FormMain formMain)
        {
            _mainForm = formMain;
            InitializeComponent();
        }

        private void FormSetting_Load(object sender, EventArgs e)
        {
            //初始化语音库
            List<PerInfo> perInfos = new List<PerInfo>();
            perInfos.Add(new PerInfo() { Val = 0, Display = "度小美" });
            perInfos.Add(new PerInfo() { Val = 1, Display = "度小宇" });
            perInfos.Add(new PerInfo() { Val = 3, Display = "度逍遥" });
            perInfos.Add(new PerInfo() { Val = 4, Display = "度丫丫" });
            perInfos.Add(new PerInfo() { Val = 106, Display = "度博文" });
            perInfos.Add(new PerInfo() { Val = 110, Display = "度小童" });
            perInfos.Add(new PerInfo() { Val = 111, Display = "度小萌" });
            perInfos.Add(new PerInfo() { Val = 103, Display = "度米朵" });
            perInfos.Add(new PerInfo() { Val = 5, Display = "度小娇" });

            per.DataSource = perInfos;
            per.DisplayMember = "Display";
            per.ValueMember = "Val";


            //初始化设置Redis
            foreach (Control ctl in groupBox1.Controls)
            {
                string key = ctl.Name;
                int val = redisConfigInfo._GetKey<int>(key);
                if (ctl.GetType()==typeof(TrackBar))
                {
                    TrackBar trackBar = ctl as TrackBar;                    
                    if (val == default(int))
                    {
                        val = 5;
                    }
                    trackBar.Value = val;
                }
                else if(ctl.GetType()==typeof(ComboBox))
                {
                    ComboBox comboBox = ctl as ComboBox;
                    if (val == default(int))
                    {
                        val = 0;
                    }
                    comboBox.SelectedValue = val;
                }
            }
        }

        private void spd_Scroll(object sender, EventArgs e)
        {
            TrackBar trackBar = sender as TrackBar;
            int val = trackBar.Value;
            redisConfigInfo._AddKey<int>(trackBar.Name, val);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                int val = (comboBox.SelectedItem as PerInfo).Val;
                if (val > 0)
                    redisConfigInfo._AddKey<int>(comboBox.Name, val);
            }
        }
    }

    public class PerInfo
    {
        public int Val { get; set; }

        public string Display { get; set; }
    }
}
