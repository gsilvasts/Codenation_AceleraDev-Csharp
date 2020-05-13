using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace Codenation_AceleraDev_CSharp
{
    class Program
    {
        private const string requestJson = "https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=d057816a07d0118ec4335d2ae94c360f85899cd7";

        private static HttpClient _httpClient;
        private static HttpClient HttpClient => _httpClient ?? (_httpClient = new HttpClient());


        static void Main(string[] args)
        {

            Task.Run(async () =>
            {
                await GetWebServiceAsync();

            }).GetAwaiter().GetResult();
            Console.ReadLine();

        }

        public static string CalculateSHA1(string text)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(text);
                System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
                return hash;
            }
            catch (Exception x)
            {
                throw new Exception(x.Message);
            }
        }

        private static async Task GetWebServiceAsync()
        {
            
            //requisição do Json
            HttpResponseMessage response = await HttpClient.GetAsync(requestJson);

            if (response.IsSuccessStatusCode)
            {
                //gravando a resposta e imprimindo na tela
                string responseBosyAsText = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBosyAsText);


                //salvando no arquivo answer e convertendo em um objeto
                using (StreamWriter file = File.CreateText(@"C:\Users\gelva\source\repos\Codenation_AceleraDev-CSharp\Codenation_AceleraDev-CSharp\answer.json"))
                {


                    //file.WriteLine(responseBosyAsText);

                    Request deserialized = JsonConvert.DeserializeObject<Request>(responseBosyAsText);


                    var cifrado = deserialized.Cifrado.ToLower();
                    int numero_Casas = deserialized.Numero_Casas;
                    string s = "";


                    foreach (char c in cifrado)
                    {

                        if (Char.IsLetterOrDigit(c))
                        {
                            int cf = Convert.ToInt16(c) - numero_Casas;
                            if (cf < 97)
                            {

                                s += Convert.ToChar(cf + 26);
                            }
                            else
                            {
                                s += Convert.ToChar(cf);
                            }
                        }
                        else
                        {
                            s += Convert.ToChar(c);
                        }
                    }

                    Console.WriteLine(s);

                    deserialized.Decifrado = s;

                    //Criando resumo SHA1

                    string resumo = CalculateSHA1(s);

                    deserialized.Resumo_Criptografico = resumo;

                    

                    //Alterando o arquivo json completo

                    JsonSerializer serializer = new JsonSerializer();

                    serializer.Serialize(file, deserialized);

                    file.Close();

                    //upload do arquivo
                    string filePath = @"C:\Users\gelva\source\repos\Codenation_AceleraDev-CSharp\Codenation_AceleraDev-CSharp\answer.json";
                   
                    using (HttpClient client = new HttpClient())

                    using (MultipartFormDataContent content = new MultipartFormDataContent())

                    using (FileStream fileStream = File.OpenRead(filePath))

                    using (StreamContent fileContent = new StreamContent(fileStream))
                    {
                        
                        fileContent.Headers.Add("file", "answer");
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        
                        content.Add(fileContent, "answer", Path.GetFileName(filePath));
                        
                        var result = client.PostAsync("https://api.codenation.dev/v1/challenge/dev-ps/submit-solution?token=d057816a07d0118ec4335d2ae94c360f85899cd7", content).Result;
                        var responsecontent = await result.Content.ReadAsStringAsync();
                      
                        Console.WriteLine(responsecontent);
                        Console.WriteLine(result.Content);



                    }

                }

            }

        }


    }
}
