using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using MySql.Data.MySqlClient;

namespace HttpRequest
{
    class Program
    {
        public ArrayList updateList = new ArrayList();
        static void Main(string[] args)
        {
            Program a = new Program();
            a.execRequest();
            //a.testFun();
            Console.WriteLine("测试完成\n");
            a.showUpdateList();
            Console.WriteLine("显示完成\n");
            Console.ReadKey();
        }

        // 循环执行http请求
        public void execRequest()
        {
            ArrayList publicList = getPublicList();
            //ArrayList publicList = new ArrayList();
            //publicList.Add(new string[3] { "1", "百度，200", "https://www.baidu.com" });
            //publicList.Add(new string[3] { "2", "华丰，404", "http://www.xwky.cn/info/1042/58486.htm" });
            //publicList.Add(new string[3] { "3", "泰安市鲁阳金属制品有限公司", "http://www.talyjs.cn/news/" });
            foreach (string[] eachPublic in publicList)
            {
                httpRequest(eachPublic);
            }
        }

        // 从数据库取出所有链接
        public ArrayList getPublicList()
        {
            string connectStr = "server=localhost;port=3306;user=root;password=;database=nyenv";
            MySqlConnection mysqlConnect = new MySqlConnection(connectStr);

            string sqlString = "SELECT *, getcompanyname(company_id) AS company FROM e_office_public_info";
            MySqlCommand sqlCommand = new MySqlCommand(sqlString, mysqlConnect);

            mysqlConnect.Open();
            MySqlDataReader reader = sqlCommand.ExecuteReader();
            ArrayList publicList = new ArrayList();

            while (reader.Read())
            {
                string[] eachPublic = new string[3];
                eachPublic[0] = reader.GetString("id");
                eachPublic[1] = reader.GetString("company");
                eachPublic[2] = reader.GetString("public_href");
                publicList.Add(eachPublic);
            }
            mysqlConnect.Close();

            return publicList;
        }

        // 执行一次http请求
        public void httpRequest(string[] eachPublic)
        {
            string nowDatetime = DateTime.Now.ToString();
            Regex uriVerify = new Regex(@"^(?:http|https)://\w+");
            if (uriVerify.IsMatch(eachPublic[2]))
            {
                try
                {
                    Console.WriteLine("开始测试：{0}, 网址：{1}", eachPublic[1], eachPublic[2]);
                    HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(eachPublic[2]);
                    myRequest.MaximumAutomaticRedirections = 1;
                    myRequest.MaximumResponseHeadersLength = -1;
                    myRequest.Timeout = 5000;
                    myRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36";

                    HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                    Console.WriteLine("测试结果：{0}, 状态：{1}\n", eachPublic[1], (int)myResponse.StatusCode);
                    updateList.Add(new string[4] { eachPublic[0], eachPublic[1], ((int)myResponse.StatusCode).ToString(), nowDatetime });
                    myResponse.Close();
                }
                catch (WebException requestWrong)
                {
                    Console.WriteLine(requestWrong.Message + "\n");
                    string wrongCode;
                    if (requestWrong.Status == WebExceptionStatus.ProtocolError)
                    {
                        Console.WriteLine(((int)((HttpWebResponse)requestWrong.Response).StatusCode).ToString());
                        wrongCode = ((int)((HttpWebResponse)requestWrong.Response).StatusCode).ToString();
                    } else
                    {
                        Console.WriteLine("未知错误");
                        // 002表示其他错误
                        wrongCode = "002";
                    }
                    updateList.Add(new string[4] { eachPublic[0], eachPublic[1], ((int)((HttpWebResponse)requestWrong.Response).StatusCode).ToString(), nowDatetime });
                }
            }
            else
            {
                // 001表示网址格式错误
                updateList.Add(new string[4] { eachPublic[0], eachPublic[1], "001", nowDatetime });
            }
        }

        // 循环显示更新数据
        public void showUpdateList()
        {
            foreach (string[] eachUpdate in updateList)
            {
                Console.WriteLine("{0}, {1}, {2}, {3}", eachUpdate[0], eachUpdate[1], eachUpdate[2], eachUpdate[3]);
            }
        }

        // 循环更新数据
        //public void

        public void testFun()
        {
            Console.WriteLine(((int)HttpStatusCode.NotFound).ToString());
        }
    }
}
