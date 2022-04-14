using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_web_api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    private static readonly HttpClient _client = new HttpClient();
    
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Hello")]
    public async Task<string> Get()
    {
        const string url  = "http://localhost:5113/Api";
        try
        {
            const string param = "id=222230&accountName=xixx";
            const string aes128Key = "xxxxxx";
            const string sign = "channelId=10000111&timestamp=1638263374&key=xxxxx";


            // Create a new instance of the AesManaged
            // class.  This generates a new key and initialization
            // vector (IV).
            using (AesManaged myAes = new AesManaged())
            {
                // Encrypt the string to an array of bytes.
                byte[] encrypted = EncryptStringToBytes_Aes(param, Encoding.UTF8.GetBytes(aes128Key), Encoding.UTF8.GetBytes(aes128Key));

                // Decrypt the bytes to a string.
                string roundtrip = DecryptStringFromBytes_Aes(encrypted, Encoding.UTF8.GetBytes(aes128Key), Encoding.UTF8.GetBytes(aes128Key));

                //Display the original data and the decrypted data.
                //Display the original data and the decrypted data.
                Console.WriteLine($"base-encrypted = :{Convert.ToBase64String(encrypted)}");
                Console.WriteLine($"roundtrip = :{roundtrip}");
                
                var parameters = new Dictionary<string, string>
                {
                    { "channelId", "9900202007070002" },
                    { "timestamp", "1638263374" },
                    { "param", Convert.ToBase64String(encrypted) },
                    { "sign", "8482024d6c64fe364873725ea0e19008" },
                };

                var json = JsonSerializer.Serialize(parameters);
            
                Console.WriteLine($"发送请求到{url}，发送的数据为{json}");
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                using (request.Content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var httpResponse = await client.SendAsync(request).ConfigureAwait(false);
                    var responseBody = await httpResponse.Content.ReadAsStringAsync();
                
                    Console.WriteLine($"dotnet-demo = :{responseBody}");
                    return await Task.Run(() => responseBody);
                }
            }
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return await Task.Run(() => "");
    }
    
     static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
}
