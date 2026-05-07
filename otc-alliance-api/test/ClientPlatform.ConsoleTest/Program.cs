using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using ClientPlatform.Pay.Dto;
using ClientPlatform.Pay.Dto.Customer;
using ClientPlatform.Pay.Dto.Account.EUR;
using ClientPlatform.Pay.Dto.Account.USD;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ClientPlatform.ConsoleTest
{
    class Program
    {
        // Helper for randomization
        static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Starting Full End-to-End Dynamic Test ===");

            // 1. Setup Options
            var options = new PayClientOptions
            {
                BaseUrl = "https://sandbox-oapi.sunpay.pro",
                Merchants = new List<PayMerchantOption>
                {
                    new PayMerchantOption
                    {
                        InvitationCode = "PayClient",
                        MerchantKey = "test-merchant-key",
                        MerchantSecret = "test-merchant-secret"
                    }
                }
            };

            // 2. Setup Dependencies
            var logger = new ConsoleLogger<PayClient>();
            var httpClient = new HttpClient();
            // Add headers to bypass Cloudflare
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

            var payClient = new PayClient(new SimpleOptions<PayClientOptions>(options), logger, httpClient);
            var payAppService = new PayAppService(payClient);

            try 
            {
                // Scenario A: Individual Flow (EUR)
                await RunFullFlow(payAppService, payClient, "INDIVIDUAL", "GB", "EUR");

                Console.WriteLine("\n--------------------------------------------------\n");

                // Scenario B: Company Flow (EUR)
                await RunFullFlow(payAppService, payClient, "COMPANY", "GB", "EUR");

                Console.WriteLine("\n--------------------------------------------------\n");

                // Scenario C: Individual Flow (USD)
                await RunFullFlow(payAppService, payClient, "INDIVIDUAL", "GB", "USD");

                Console.WriteLine("\n--------------------------------------------------\n");

                // Scenario D: Company Flow (USD)
                await RunFullFlow(payAppService, payClient, "COMPANY", "GB", "USD");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\n=== Test Completed ===");
        }

        static async Task RunFullFlow(PayAppService service, PayClient client, string type, string country, string currency)
        {
            Console.WriteLine($"[Scenario] Full Flow for {type} in {country} ({currency})");
            
            // Generate Random Data
            var randomId = RandomString(6);
            var email = $"{type.ToLower()}_{randomId}@test.com";
            var outUserId = $"USER_{type}_{randomId}";
            var firstName = $"User{RandomString(4)}";
            var lastName = $"Test{RandomString(4)}";
            
            Console.WriteLine($"   Generated User: {outUserId} / {email} / {firstName} {lastName}");

            // --- Step 1: Customer Inquiry ---
            Console.WriteLine("1. Customer Inquiry: Fetching Required Fields...");
            var custRulesInput = new GetCustomerRequiredFieldsInput { CustomerType = type, CountryCode = country };
            var custRules = await service.GetCustomerRequiredFields(custRulesInput, "PayClient");
            
            if (custRules.Data == null) { Console.WriteLine("   Error: Failed to fetch customer rules."); return; }
            Console.WriteLine($"   API requires {custRules.Data.Count} fields for Customer.");
            foreach(var f in custRules.Data) Console.WriteLine($"     - {f.FieldName} ({f.Type})");

            // --- Step 1.5: Pre-Creation Document Upload (Test) ---
            Console.WriteLine("1.5. Testing Pre-Creation Upload with out_user_id...");
            // Create a dummy file > 1KB
            var sbPre = new StringBuilder();
            for (int i = 0; i < 200; i++) sbPre.Append("This is a dummy document content line to make the file larger than 1KB. ");
            var dummyContentPre = sbPre.ToString();
            var preStream = new MemoryStream(Encoding.UTF8.GetBytes(dummyContentPre));
            
            var preUploadInput = new UploadCustomerDocInput
            {
                CustomerId = outUserId, // Try using out_user_id
                DocumentType = "PASSPORT",
                FileName = "pre_passport.jpg",
                FileStream = preStream
            };
            var preUploadResult = await client.UploadCustomerDocAsync(preUploadInput, "PayClient");
            string preDocId = null;
            if (preUploadResult.IsSuccess)
            {
                Console.WriteLine($"   SUCCESS: Pre-uploaded doc with out_user_id: {preUploadResult.Data.DocumentId}");
                preDocId = preUploadResult.Data.DocumentId.ToString();
            }
            else
            {
                Console.WriteLine($"   Failed to pre-upload doc: {preUploadResult.Code} - {preUploadResult.Msg}");
            }

            // --- Step 2: Customer Assembly & Submission ---
            Console.WriteLine("2. Customer Creation: Assembling and Submitting...");
            
            string customerId = null;

            if (type == "INDIVIDUAL")
            {
                var custPayload = new Dictionary<string, object>
                {
                    { "customer_email", email },
                    { "out_user_id", outUserId },
                    { "country_code", country },
                    { "first_name", firstName },
                    { "last_name", lastName },
                    { "id_document_type", "PASSPORT" },
                    { "id_document_number", "P" + RandomString(8) },
                    { "occupation", "Developer" }
                };

                if (preDocId != null)
                {
                     custPayload["id_front_side_document_id"] = preDocId;
                     custPayload["id_back_side_document_id"] = preDocId;
                     custPayload["address_document_id"] = preDocId;
                     custPayload["fund_source_document_id"] = preDocId;
                }

                var custInput = JsonConvert.DeserializeObject<CreateIndividualCustomerInput>(JsonConvert.SerializeObject(custPayload));
                var custResult = await service.CreateIndividualCustomer(custInput, "PayClient");
                
                if (!custResult.IsSuccess)
                {
                    Console.WriteLine($"   Failed to create individual customer: {custResult.Code} - {custResult.Msg}");
                    return;
                }
                customerId = custResult.Data.CustomerId.ToString();
            }
            else // COMPANY
            {
                var custPayload = new Dictionary<string, object>
                {
                    { "customer_email", email },
                    { "out_user_id", outUserId },
                    { "country_code", country },
                    { "company_name", $"Tech Corp {randomId} Ltd" },
                    { "date_of_incorporation", "2020-01-01" },
                    { "registration_number", "REG" + RandomString(6) },
                    { "company_representative_name", "John Doe" },
                    { "company_representative_document_type", "PASSPORT" },
                    { "company_representative_number", "P" + RandomString(8) }
                };

                if (preDocId != null)
                {
                    custPayload["company_document_id"] = preDocId;
                    custPayload["company_handheld_document_id"] = preDocId;
                }

                var custInput = JsonConvert.DeserializeObject<CreateCompanyCustomerInput>(JsonConvert.SerializeObject(custPayload));
                var custResult = await service.CreateCompanyCustomer(custInput, "PayClient");

                if (!custResult.IsSuccess)
                {
                    Console.WriteLine($"   Failed to create company customer: {custResult.Code} - {custResult.Msg}");
                    return;
                }
                customerId = custResult.Data.CustomerId.ToString();
            }

            Console.WriteLine($"   SUCCESS: Customer Created. ID: {customerId}");

            // --- Step 3: Document Upload ---
            Console.WriteLine("3. Document Upload: Uploading dummy ID...");
            // Create a dummy file > 1KB
            var sb = new StringBuilder();
            for (int i = 0; i < 200; i++) sb.Append("This is a dummy document content line to make the file larger than 1KB. ");
            var dummyContent = sb.ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(dummyContent));
            
            var uploadInput = new UploadCustomerDocInput
            {
                CustomerId = customerId,
                DocumentType = "PASSPORT", 
                FileName = "dummy_passport.jpg",
                FileStream = stream
            };

            var uploadResult = await client.UploadCustomerDocAsync(uploadInput, "PayClient");
            
            if (!uploadResult.IsSuccess)
            {
                Console.WriteLine($"   Failed to upload doc: {uploadResult.Code} - {uploadResult.Msg}");
            }
            else
            {
                Console.WriteLine($"   SUCCESS: Document Uploaded (PASSPORT). ID: {uploadResult.Data.DocumentId}");
            }

            // Upload Bank Statement
            uploadInput.DocumentType = "BANK_STATEMENT";
            uploadInput.FileName = "dummy_bank_statement.pdf";
            uploadInput.FileStream = new MemoryStream(Encoding.UTF8.GetBytes(dummyContent)); // New stream
            var uploadResult2 = await client.UploadCustomerDocAsync(uploadInput, "PayClient");
             if (!uploadResult2.IsSuccess)
            {
                Console.WriteLine($"   Failed to upload doc 2: {uploadResult2.Code} - {uploadResult2.Msg}");
            }
            else
            {
                Console.WriteLine($"   SUCCESS: Document Uploaded (BANK_STATEMENT). ID: {uploadResult2.Data.DocumentId}");
            }
            
            // Wait for eventual consistency
            Console.WriteLine("   Waiting 5 seconds for document processing...");
            await Task.Delay(5000);

            // --- Step 4: Account Inquiry ---
            Console.WriteLine("4. Account Inquiry: Fetching Required Fields...");
            var accRulesInput = new GetAccountRequiredFieldsInput { CustomerId = customerId, Currency = currency };
            var accRules = await service.GetCreateAccountRequiredFields(accRulesInput, "PayClient");

            if (accRules.Data == null) { Console.WriteLine("   Error: Failed to fetch account rules."); return; }
            Console.WriteLine($"   API requires {accRules.Data.Count} fields for Account.");

            // --- Step 5: Account Assembly & Submission ---
            Console.WriteLine("5. Account Creation: Assembling and Submitting...");
            
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            if (currency == "EUR")
            {
                if (type == "INDIVIDUAL")
                {
                    var accPayload = new Dictionary<string, object>
                    {
                        { "customer_id", customerId },
                        { "first_name", firstName },
                        { "last_name", lastName },
                        { "email", email },
                        { "phone", "+442071234567" },
                        { "nationality", "GB" },
                        { "address_line", "123 Test St" },
                        { "city", "London" },
                        { "post_code", "SW1A 1AA" },
                        { "country_code", "GB" },
                        { "birth_date", "1990-01-01 00:00:00" },
                        { "id_document_type", "PASSPORT" },
                        { "id_number", "P" + RandomString(8) }
                    };
                    if (uploadResult.IsSuccess)
                    {
                        accPayload["passport_document_id"] = uploadResult.Data.DocumentId;
                        accPayload["id_front_side_document_id"] = uploadResult.Data.DocumentId;
                    }

                    var accInput = JsonConvert.DeserializeObject<CreateEURIndividualAccountInput>(JsonConvert.SerializeObject(accPayload), settings);
                    var accResult = await service.CreateEURIndividualAccount(accInput, "PayClient");
                    PrintResult(accResult);
                }
                else // EUR COMPANY
                {
                    var accPayload = new Dictionary<string, object>
                    {
                        { "customer_id", customerId },
                        { "email", email },
                        { "company_name", $"Tech Corp {randomId} Ltd" },
                        { "registration_number", "REG" + RandomString(6) },
                        { "company_type", "PRIVATE_LIMITED_COMPANY" },
                        { "company_registration_date", "2020-01-01 00:00:00" },
                        { "trading_address", "123 Trading St" },
                        { "trading_city", "London" },
                        { "trading_country", "GB" },
                        { "trading_post_code", "SW1A 1AA" },
                        { "nationality", "GB" },
                        { "company_contact", "Jane Doe" }
                    };
                    if (uploadResult.IsSuccess)
                    {
                        accPayload["company_incorporation_document_id"] = uploadResult.Data.DocumentId;
                    }

                    var accInput = JsonConvert.DeserializeObject<CreateEURCompanyAccountInput>(JsonConvert.SerializeObject(accPayload), settings);
                    var accResult = await service.CreateEURCompanyAccount(accInput, "PayClient");
                    PrintResult(accResult);
                }
            }
            else if (currency == "USD")
            {
                if (type == "INDIVIDUAL")
                {
                    var accPayload = new Dictionary<string, object>
                    {
                        { "customer_id", customerId },
                        { "first_name", firstName },
                        { "last_name", lastName },
                        { "email", email },
                        { "phone", "+442071234567" },
                        { "nationality", "GB" },
                        { "birth_date", "1990-01-01 00:00:00" },
                        { "id_document_type", "PASSPORT" },
                        
                        { "mailing_address_country_code", "US" },
                        { "mailing_address_state", "NY" },
                        { "mailing_address_city", "New York" },
                        { "mailing_address_address_line", "123 Wall St" },
                        { "mailing_address_postal_code", "10005" },

                        { "employment_status", "EMPLOYEE" },
                        { "employment_description", "Software Engineer" },
                        { "employer_name", "Tech Corp" },
                        { "occupation", "Developer" },
                        { "primary_source_of_funds", "EMPLOYMENT" },
                        { "primary_source_of_funds_description", "Salary" },
                        
                        { "usd_value_of_fiat", "UPTO_10K" },
                        { "usd_value_of_crypto", "UPTO_10K" },
                        { "monthly_deposits", "UPTO_5" },
                        { "monthly_crypto_deposits", "UPTO_5" },
                        { "monthly_investment_deposit", "UPTO_1K" },
                        { "monthly_crypto_investment_deposit", "UPTO_1K" },
                        { "monthly_withdrawals", "UPTO_5" },
                        { "monthly_crypto_withdrawals", "UPTO_5" },
                        { "monthly_investment_withdrawal", "UPTO_1K" },
                        { "monthly_crypto_investment_withdrawal", "UPTO_1K" },
                        
                        { "funds_send_receive_jurisdictions", "US" },
                        { "engage_in_activities", "NONE" },
                        { "vendors_and_counterparties", "SELF" },
                        
                        { "tax_reference_number", "123456789" },
                        { "citizenship", "US" }
                    };

                    if (uploadResult.IsSuccess && uploadResult2.IsSuccess)
                    {
                        accPayload["passport_document_id"] = uploadResult.Data.DocumentId;
                        accPayload["proof_of_address"] = "BANK_STATEMENT"; 
                        accPayload["proof_of_address_bank_statement_document_id"] = uploadResult2.Data.DocumentId;
                        accPayload["source_of_funds"] = "BANK_STATEMENT";
                        accPayload["source_of_funds_bank_statement_document_id"] = uploadResult2.Data.DocumentId;
                    }

                    var accInput = JsonConvert.DeserializeObject<CreateUSDIndividualAccountInput>(JsonConvert.SerializeObject(accPayload), settings);
                    var accResult = await service.CreateUSDIndividualAccount(accInput, "PayClient");
                    PrintResult(accResult);
                }
                else // USD COMPANY
                {
                    // Generate random 9 digit EIN
                    var random = new Random();
                    var ein = random.Next(100000000, 999999999).ToString();
                    var bpTax = random.Next(100000000, 999999999).ToString();

                    var accPayload = new Dictionary<string, object>
                    {
                        { "customer_id", customerId },
                        { "email", email },
                        { "company_name", $"Tech Corp {randomId} Ltd" },
                        { "registration_number", ein },
                        { "company_registration_date", "2020-01-01 00:00:00" },
                        
                        { "mailing_address_country_code", "US" },
                        { "mailing_address_state", "NY" },
                        { "mailing_address_city", "New York" },
                        { "mailing_address_address_line", "123 Wall St" },
                        { "mailing_address_postal_code", "10005" },
                        
                        { "company_registered_address_country_code", "US" },
                        { "company_registered_address_state", "NY" },
                        { "company_registered_address_city", "New York" },
                        { "company_registered_address_address_line1", "123 Corp St" },
                        { "company_registered_address_postal_code", "10005" },
                        
                        { "company_tax_reference_number", ein },
                        { "company_website_address", "https://example.com" },
                        { "company_state_of_incorporation", "NY" },
                        { "company_entity_type", "LLC" },
                        { "company_entity_type_description", "LLC" },
                        { "company_naics", "541511" },
                        { "company_naics_description", "Custom Computer Programming Services" },
                        { "company_primary_business", "SOFTWARE" },
                        { "company_description_of_business_nature", "Tech" },
                        { "company_business_jurisdictions", "US" },
                        { "company_funds_send_receive_jurisdictions", "US" },
                        { "company_regulated_status", "NONE" },
                        
                        { "business_person_first_name", "John" },
                        { "business_person_middle_name", "D" },
                        { "business_person_last_name", "Doe" },
                        { "business_person_birth_date", "1980-01-01 00:00:00" },
                        { "business_person_email", "john.doe@example.com" },
                        { "business_person_phone", "+12125551234" },
                        { "business_person_tax_reference_number", bpTax },
                        { "business_person_ownership", 100 },
                        { "citizenship", "GB" },
                        { "phone", "1234567890" },
                        { "primary_source_of_funds", "EMPLOYMENT" },
                        { "primary_source_of_funds_description", "Salary" },
                        { "usd_value_of_fiat", "UPTO_10K" },
                        { "usd_value_of_crypto", "UPTO_10K" },
                        { "monthly_deposits", "UPTO_5" },
                        { "monthly_crypto_deposits", "UPTO_5" },
                        { "monthly_investment_deposit", "UPTO_1K" },
                        { "monthly_crypto_investment_deposit", "UPTO_1K" },
                        { "monthly_withdrawals", "UPTO_5" },
                        { "monthly_crypto_withdrawals", "UPTO_5" },
                        { "monthly_investment_withdrawal", "UPTO_1K" },
                        { "monthly_crypto_investment_withdrawal", "UPTO_1K" },
                        { "engage_in_activities", "NONE" },
                        { "vendors_and_counterparties", "SELF" }
                    };

                    // Upload extra docs for Company USD
                    var extraDocs = new List<string>();
                    for(int i=0; i<12; i++) 
                    {
                        uploadInput.FileName = $"extra_doc_{i}.jpg";
                        uploadInput.DocumentType = "PASSPORT"; 
                        uploadInput.FileStream = new MemoryStream(Encoding.UTF8.GetBytes(dummyContent));
                        var res = await client.UploadCustomerDocAsync(uploadInput, "PayClient");
                        if(res.IsSuccess) extraDocs.Add(res.Data.DocumentId.ToString());
                    }

                    if (extraDocs.Count >= 12)
                    {
                        accPayload["company_incorporation_document_id"] = extraDocs[0];
                        accPayload["company_articles_of_incorporation_document_id"] = extraDocs[1];
                        accPayload["company_beneficial_ownership_certificate_document_id"] = extraDocs[2];
                        accPayload["company_account_agreement_document_id"] = extraDocs[3];
                        
                        accPayload["company_proof_of_address_document_type"] = "BANK_STATEMENT";
                        accPayload["company_bank_statement_document_id"] = extraDocs[4]; 
                        
                        accPayload["company_source_of_funds_bank_statement_document_id"] = extraDocs[5];
                        accPayload["company_source_of_funds_financial_statement_document_id"] = extraDocs[6]; 
                        accPayload["company_source_of_funds_term_sheet_document_id"] = extraDocs[7];
                        
                        accPayload["company_individual_id_document_type"] = "PASSPORT";
                        accPayload["passport_document_id"] = extraDocs[8]; 
                        
                        accPayload["selfie_document_id"] = extraDocs[9];
                        
                        accPayload["company_individual_proof_of_address_document_type"] = "BANK_STATEMENT";
                        accPayload["company_individual_bank_statement_document_id"] = extraDocs[10];
                        
                        accPayload["company_proof_of_source_of_funds"] = "BANK_STATEMENT";
                    }

                    var accInput = JsonConvert.DeserializeObject<CreateUSDCompanyAccountInput>(JsonConvert.SerializeObject(accPayload), settings);
                    var accResult = await service.CreateUSDCompanyAccount(accInput, "PayClient");
                    PrintResult(accResult);
                }
            }
        }

        static void PrintResult(PayApiResponse<CreateAccountResponse> result)
        {
             if (!result.IsSuccess)
            {
                Console.WriteLine($"   Failed to create account: {result.Code} - {result.Msg}");
                if (result.Msg.Contains("VALIDATION_ERROR")) {
                     Console.WriteLine($"   Validation Details: {result.Msg}");
                }
            }
            else
            {
                Console.WriteLine($"   SUCCESS: Account Created. ID: {result.Data.AccountId}");
                Console.WriteLine($"   Status: {result.Data.Status}");
            }
        }
    }

    public class SimpleOptions<T> : IOptions<T> where T : class, new()
    {
        public T Value { get; }
        public SimpleOptions(T value) { Value = value; }
    }

    public class ConsoleLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= LogLevel.Warning)
            {
                Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            }
        }
        private class NoopDisposable : IDisposable { public void Dispose() { } }
    }
}
