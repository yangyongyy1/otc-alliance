using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Shouldly;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Dto.Response;
using Xunit;

namespace ClientPlatform.Tests.Pay
{
    public class PayClient_Tests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly PayClient _payClient;
        private readonly PayClientOptions _options;

        public PayClient_Tests()
        {
            _options = new PayClientOptions
            {
                BaseUrl = "https://sandbox-oapi.sunpay.pro",
                Merchants = new System.Collections.Generic.List<PayMerchantOption>
                {
                    new PayMerchantOption
                    {
                        InvitationCode = "PayClient",
                        MerchantKey = "test-merchant-key",
                        MerchantSecret = "test-merchant-secret"
                    }
                }
            };
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri(_options.BaseUrl)
            };

            var optionsMock = new Mock<IOptions<PayClientOptions>>();
            optionsMock.Setup(x => x.Value).Returns(_options);

            var loggerMock = new Mock<ILogger<PayClient>>();

            _payClient = new PayClient(optionsMock.Object, loggerMock.Object, httpClient);
        }

        [Fact]
        public async Task CreateAccountAsync_Should_Send_Correct_Request_For_Individual()
        {
            // Arrange
            var input = new CreateAccountInput
            {
                Currency = "EUR",
                CustomerId = "CUST123",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                City = "London",
                Phone = "123456789",
                IdNumber = "ID123",
                IdDocumentType = "PASSPORT",
                BirthCountry = "GB",
                AddressLine = "123 Street",
                CountryCode = "GB",
                BirthDate = "1990-01-01",
                PostCode = "12345",
                IpAddress = "127.0.0.1"
            };

            var expectedResponse = new PayApiResponse<CreateAccountResponse>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new CreateAccountResponse 
                { 
                    CustomerId = Guid.NewGuid(),
                    Currency = input.Currency,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    AccountId = "ACC123",
                    Status = "Submitted"
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var result = await _payClient.CreateAccountAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("/api/v3/VA/Account") &&
                    req.Headers.Contains("SunPay-Key") &&
                    System.Linq.Enumerable.First(req.Headers.GetValues("SunPay-Key")) == "test-merchant-key" &&
                    req.Headers.Contains("SunPay-Timestamp") &&
                    req.Headers.Contains("SunPay-Nonce") &&
                    req.Headers.Contains("SunPay-Sign")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task CreateAccountAsync_Should_Send_Correct_Request_For_Company()
        {
            // Arrange
            var input = new CreateAccountInput
            {
                CompanyName = "Test Corp",
                RegistrationNumber = "REG123",
                Currency = "EUR",
                CustomerId = "CUST123",
                CompanyRegistrationDate = "2020-01-01",
                CountryOfIncorporation = "GB",
                AddressLine = "123 Street",
                City = "London",
                PostCode = "12345",
                CountryCode = "GB"
            };

            var expectedResponse = new PayApiResponse<CreateAccountResponse>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new CreateAccountResponse 
                { 
                    CustomerId = Guid.NewGuid(),
                    Currency = input.Currency,
                    CompanyName = input.CompanyName,
                    RegistrationNumber = input.RegistrationNumber,
                    AccountId = "ACC456",
                    Status = "Submitted"
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var result = await _payClient.CreateAccountAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("/api/v3/VA/Account") &&
                    req.Headers.Contains("SunPay-Key")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetCustomerRequiredFieldsAsync_Should_Send_Correct_Get_Request()
        {
            // Arrange
            var expectedResponse = new PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new System.Collections.Generic.List<ChannelRequiredFieldResponse>
                {
                    new ChannelRequiredFieldResponse { FieldName = "some_field", Required = true }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var input = new GetCustomerRequiredFieldsInput { CustomerType = "INDIVIDUAL", CountryCode = "GB" };
            var result = await _payClient.GetCustomerRequiredFieldsAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/api/v3/VA/VACustomerRequiredFields") &&
                    req.RequestUri.Query.Contains("customer_type=INDIVIDUAL") &&
                    req.Headers.Contains("SunPay-Key")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        [Fact]
        public async Task GetAccountListAsync_Should_Return_List_Of_Accounts()
        {
            // Arrange
            var expectedResponse = new PayApiResponse<System.Collections.Generic.List<GetAccountsResponse>>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new System.Collections.Generic.List<GetAccountsResponse>
                {
                    new GetAccountsResponse { AccountId = "ACC123", Currency = "USD", Balance = 100.00m }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var input = new GetAccountListInput { CustomerId = "CUST123" };
            var result = await _payClient.GetAccountListAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(1);
            result.Data[0].AccountId.ShouldBe("ACC123");

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/api/v3/VA/Accounts") &&
                    req.RequestUri.Query.Contains("customer_id=CUST123")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetAccountDetailAsync_Should_Return_Account_Detail()
        {
            // Arrange
            var expectedResponse = new PayApiResponse<GetAccountsResponse>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new GetAccountsResponse { AccountId = "ACC123", Currency = "USD", Balance = 100.00m }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var result = await _payClient.GetAccountDetailAsync("ACC123", "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.AccountId.ShouldBe("ACC123");

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/api/v3/VA/Account") &&
                    req.RequestUri.Query.Contains("account_id=ACC123")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task UploadCustomerDocAsync_Should_Return_DocumentId()
        {
            // Arrange
            var expectedResponse = new PayApiResponse<UploadCustomerDocResponse>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new UploadCustomerDocResponse { DocumentId = Guid.NewGuid() }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var input = new UploadCustomerDocInput { CustomerId = "CUST123", DocumentType = "PASSPORT", FileName = "test.jpg" };
            var result = await _payClient.UploadCustomerDocAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.DocumentId.ShouldNotBe(Guid.Empty);

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("/api/v3/VA/UploadCustomerDoc")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetCreateAccountRequiredFieldsAsync_Should_Send_Correct_Get_Request()
        {
            // Arrange
            var expectedResponse = new PayApiResponse<System.Collections.Generic.List<ChannelRequiredFieldResponse>>
            {
                IsSuccess = true,
                Code = 200,
                Msg = "Success",
                Data = new System.Collections.Generic.List<ChannelRequiredFieldResponse>
                {
                    new ChannelRequiredFieldResponse { FieldName = "some_field", Required = true }
                }
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(expectedResponse))
                })
                .Verifiable();

            // Act
            var input = new GetAccountRequiredFieldsInput { CustomerId = "CUST123", Currency = "USD" };
            var result = await _payClient.GetCreateAccountRequiredFieldsAsync(input, "PayClient");

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBe(1);
            result.Data[0].FieldName.ShouldBe("some_field");

            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("/api/v3/VA/AccountRequiredFields") &&
                    req.RequestUri.Query.Contains("customer_id=CUST123") &&
                    req.RequestUri.Query.Contains("currency=USD")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
