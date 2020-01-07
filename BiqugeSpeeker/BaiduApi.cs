using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BiqugeSpeeker
{
    public class BaiduApi
    {

        private readonly string API_key = "jN9G0c1Lvwnz8SzpSBhm56k7";
        private readonly string API_secret_key = "jNjK25odluQPdRgjcFNIWGXbIukio7y8";
        private RedisConfigInfo redisConfig = RedisConfigInfo.GetRedisConfigInfo();

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="para_API_key"></param>
        /// <param name="para_API_secret_key"></param>
        /// <returns></returns>
        private string getTokon()
        {
            string token = redisConfig._GetKey<string>("baidu_token");

            if (string.IsNullOrEmpty(token))
            {
                WebClient webClient = new WebClient();
                webClient.BaseAddress = "https://openapi.baidu.com";
                string result = webClient.DownloadString($"https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={API_key}&client_secret={API_secret_key}");
                webClient.Dispose();

                dynamic json = JToken.Parse(result);
                token = json.access_token;
                long expires_in = json.expires_in;
                redisConfig._AddKey<string>("baidu_token", token,new TimeSpan(expires_in*1000*1000*10));
            }
            return token;
        }

        public void GetAudio(string filePath,string text)
        {
            string token = getTokon();
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            HttpClient httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri("http://tsn.baidu.com/text2audio");
            //await异步等待回应
            var response = httpClient.PostAsync($"http://tsn.baidu.com/text2audio?lan=zh&ctp=2&vol=9&per=0&spd=5&pit=5&aue=3&tok={token}&tex={HttpUtility.UrlEncode(HttpUtility.UrlEncode(text))}&cuid={Guid.NewGuid()}&aue=6", null).Result;
            //确保HTTP成功状态值
            response.EnsureSuccessStatusCode();
            //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
            byte[] result = response.Content.ReadAsByteArrayAsync().Result;
            string resonse= response.Content.ReadAsStringAsync().Result;
            using (FileStream fileStream=new FileStream(filePath,FileMode.Create))
            {
                fileStream.Write(result, 0, result.Length);
            }
        }
    }
}
