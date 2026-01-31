using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace httpCPFValidation
{
    public static class validatecpf
    {
        [FunctionName("validatecpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Starting CPF Validation.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                return new BadRequestObjectResult("Please inform CPF.");
            }
            var cpf = data?.cpf;

            if (IsValidCPF((string)cpf))
            {
                log.LogInformation("CPF is valid.");
                var responseMessage = "CPF is valid.";
                return new OkObjectResult(responseMessage);
            }
            else
            {
                log.LogInformation("CPF is invalid.");
                var responseMessage = "CPF is invalid.";

                return new OkObjectResult(responseMessage);
            }
        }

        public static bool IsValidCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            // Remove non-numeric characters
            cpf = System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            // Check for invalid CPF numbers
            string[] invalidCpfs = new string[]
            {
            "00000000000", "11111111111", "22222222222", "33333333333",
            "44444444444", "55555555555", "66666666666", "77777777777",
            "88888888888", "99999999999"
            };

            if (Array.Exists(invalidCpfs, element => element == cpf))
                return false;

            // Validate first digit
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);
            int firstDigit = sum % 11;
            firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;

            if (firstDigit != (cpf[9] - '0'))
                return false;

            // Validate second digit
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);
            int secondDigit = sum % 11;
            secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;

            if (secondDigit != (cpf[10] - '0'))
                return false;

            return true;
        }
    }
}
