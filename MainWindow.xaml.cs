using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace V2rayNCustomizePAC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[][][] PACForbiddenHosts;
        public MainWindow()
        {
            InitializeComponent();
            // 删除PAConfig.json
            string jsonfile = "PAConfig.json";
            /*
            if (System.IO.File.Exists(jsonfile))
            {
                System.IO.File.Delete(jsonfile);
            }
            */

            // 查找是否有PAConfig.json,如果有,读取PACFilePath项的内容,放入PACFilePath文本框中
            // string jsonfile = "PAConfig.json";
            if (System.IO.File.Exists(jsonfile))
            {
                JObject jObject = Readjson(jsonfile);
                PACFilePath.Text = jObject["PACFilePath"].ToString();
                // PACLastProxyWebsite.Text = jObject["PACLastProxyWebsite"].ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 打开一个文件选择器，将选择的文件路径放入 PACFilePath 这个TextBox中
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            dlg.Filter = "PAC文件 (pac.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                PACFilePath.Text = dlg.FileName;
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // 读取PAConfig.json的内容,在不改变其他内容的前提下,修改PACFilePath项为PACFilePath文本框的内容
            string jsonfile = "PAConfig.json";
            JObject jObject = new JObject();
            if (System.IO.File.Exists(jsonfile))
            {
                jObject = Readjson(jsonfile);
            }
            jObject["PACFilePath"] = PACFilePath.Text;
            // jObject["PACLastProxyWebsite"] = PACLastProxyWebsite.Text;
            // 判断jObject["CustomHost"]是否存在
            if (jObject["CustomHost"] == null)
            {
                jObject["CustomHost"] = new JArray();
            }

            PACForbiddenHosts = GetForbiddenHosts(PACFilePath.Text);
            // Console.WriteLine(PACForbiddenHosts);
            PACContext.Text = "";
            string pacContextText = "";
            foreach (string host in jObject["CustomHost"])
            {
                pacContextText += host + "\r\n";
            }
            PACContext.Text = pacContextText;

            string[] allPACTXTForbiddenHost = PACForbiddenHosts[1][1];
            /*
            // 找到PACLastProxyWebsite文本框中的内容在PACForbiddenHosts数组中的位置
            int index = Array.IndexOf(allPACTXTForbiddenHost, PACLastProxyWebsite.Text);
            // 将这个位置之后的host与CustomHost对比,如果PACForbiddenHosts中没有,则输出到UnjoinedHost文本框中
            string[] pairPACTXTForbiddenHost= allPACTXTForbiddenHost.Skip(index).ToArray();
            */
            UnjoinedHost.Text = "";
            string unjoinedHostText = "";
            string[] CustomHostArray = jObject["CustomHost"].ToObject<string[]>();

            foreach (string host in CustomHostArray)
            {
                if (!allPACTXTForbiddenHost.Contains(host))
                {
                    // Console.WriteLine(host);
                    unjoinedHostText += host + "\r\n";
                }
            }
            UnjoinedHost.Text = unjoinedHostText;
            Writejson(jsonfile, jObject);


        }

        private JObject Readjson(string jsonfile)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(jsonfile))
            {
                // 控制台输出jsonfile 位置
                // Console.WriteLine(jsonfile);
                string json = file.ReadToEnd();
                JObject jObject = JObject.Parse(json);
                return jObject;
            }
        }

        public void Writejson(string jsonfile, JObject jObject)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(jsonfile))
            {
                file.Write(jObject.ToString());
            }
        }
        static string[][][] GetForbiddenHosts(string pacFilePath)
        {
            string pacContent = System.IO.File.ReadAllText(pacFilePath);

            string pattern = @"var rules = ([\s\S]*?);";
            Match match = Regex.Match(pacContent, pattern);

            if (match.Success)
            {

                string subArray = match.Groups[1].Value;
                string[][][] host = JsonConvert.DeserializeObject<string[][][]>(subArray);
                if (host != null)
                {
                    return host;
                }
                else
                {
                    return new string[0][][];
                }

            }
            else
            {
                return new string[0][][];
            }
        }
        private void SetForbiddenHosts(string pacFilePath, string[][][] rule)
        {
            // 使用正则表达式匹配出PAC文件中rule的内容，并替换为传入的参数rule
            string pacContent = System.IO.File.ReadAllText(pacFilePath);
            string pattern = @"var rules = ([\s\S]*?);";
            Match match = Regex.Match(pacContent, pattern);
            if (match.Success)
            {
                string subArray = match.Groups[1].Value;
                string newPacContent = pacContent.Replace(subArray, JsonConvert.SerializeObject(rule, Formatting.Indented));
                System.IO.File.WriteAllText(pacFilePath, newPacContent);
            }

        }

        private void SavePACFile_Click(object sender, RoutedEventArgs e)
        {
            // 将PACContext中的内容按行分割，并 新建一个数组保存
            string[] PACContextArray = PACContext.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            // 去掉PACContextArray中的""
            PACContextArray = PACContextArray.Where(s => !string.IsNullOrEmpty(s)).ToArray();
            // 去掉PACContextArray中的重复值，并写回文本框
            PACContextArray = PACContextArray.Distinct().ToArray();
            PACContext.Text = "";
            string pacContextText = "";
            foreach (string host in PACContextArray)
            {
                pacContextText += host + "\r\n";
            }
            PACContext.Text = pacContextText;


            string jsonfile = "PAConfig.json";
            JObject jObject = new JObject();
            if (System.IO.File.Exists(jsonfile))
            {
                jObject = Readjson(jsonfile);
            }
            jObject["PACFilePath"] = PACFilePath.Text;
            if (jObject["CustomHost"] == null)
            {
                jObject["CustomHost"] = new JArray();
            }
            // 将 CustomHost 中的内容保存到一个数组中
            string[] CustomHostArray = jObject["CustomHost"].ToObject<string[]>();

            // 将PACContextArray中的内容与CustomHostArray对比,如果CustomHostArray中没有,则添加到CustomHostArray中，并记录添加的条数
            int addCount = 0;
            foreach (string host in PACContextArray)
            {
                if (!CustomHostArray.Contains(host) && host != "")
                {
                    // CustomHostArray中添加这个host
                    CustomHostArray = CustomHostArray.Concat(new string[] { host }).ToArray();
                    addCount++;
                }
            }
            // 如果CustomHostArray有，但是PACContextArray没有，则在CustomHostArray中删除没有的条目，并记录条数
            int deleteCount = 0;
            foreach (string host in CustomHostArray)
            {
                if (!PACContextArray.Contains(host))
                {
                    // CustomHostArray中删除这个host
                    CustomHostArray = CustomHostArray.Where(val => val != host).ToArray();
                    deleteCount++;
                }
            }
            // 去掉 CustomHostArray 中重复的条目和空条目
            CustomHostArray = CustomHostArray.Distinct().ToArray();

            // 将CustomHostArray转换成相应的格式并写入jObject["CustomHost"]中
            jObject["CustomHost"] = JArray.FromObject(CustomHostArray);
            // 将PACContextArray中的内容添加到PACForbiddenHosts[1][1]中
            string[] allPACTXTForbiddenHost = PACForbiddenHosts[1][1];
            // 将PACContextArray中的内容与allPACTXTForbiddenHost对比,如果allPACTXTForbiddenHost中没有,则添加到allPACTXTForbiddenHost中
            foreach (string host in PACContextArray)
            {
                if (!allPACTXTForbiddenHost.Contains(host) && host != "")
                {
                    // allPACTXTForbiddenHost中添加这个host
                    allPACTXTForbiddenHost = allPACTXTForbiddenHost.Concat(new string[] { host }).ToArray();
                }
            }
            PACForbiddenHosts[1][1] = allPACTXTForbiddenHost;
            // 将PACForbiddenHosts写入PAC文件
            SetForbiddenHosts(PACFilePath.Text, PACForbiddenHosts);
            // 将jObject写入PAConfig.json
            Writejson(jsonfile, jObject);
            // 弹出提示框输出添加和删除的条数，并通知成功
            MessageBox.Show("添加了" + addCount + "条，删除了" + deleteCount + "条", "成功", MessageBoxButton.OK, MessageBoxImage.Information);


        }
    }
}
