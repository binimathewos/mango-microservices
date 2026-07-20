

using Mango.Web.Models;
using Newtonsoft.Json;
using System.Text;
using Mango.Web.Service.IService;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Mango.Web.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ITokenProvider _tokenProvider;

        public BaseService(IHttpClientFactory httpClient, ITokenProvider tokenProvider)
        {
            _httpClient = httpClient;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                var client = _httpClient.CreateClient("MangoAPI");
                HttpRequestMessage message = new HttpRequestMessage();

                if (requestDto.ContentType == SD.ContentType.Json)
                {
                    message.Headers.Add("Accept", "*/*");
                }
                else
                {
                    message.Headers.Add("Accept", "application/json");
                }

                if (withBearer)
                {
                    string token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDto.Url);

                if (requestDto.Data != null)
                {
                    if (requestDto.ContentType == SD.ContentType.MultipartFormData)
                    {
                        var form = new MultipartFormDataContent();
                        foreach (var prop in requestDto.Data.GetType().GetProperties())
                        {
                            var value = prop.GetValue(requestDto.Data);
                            if (value is FormFile)
                            {
                                var file = (FormFile)value;
                                if (file != null)
                                {
                                    form.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                                }
                            }
                            else
                            {
                                form.Add(new StringContent(value?.ToString() ?? string.Empty), prop.Name);
                            }
                        }

                        message.Content = form;
                    }
                    else
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                    }
                }

                HttpResponseMessage? apiResponse = null;
                switch (requestDto.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiResponse = await client.SendAsync(message);
                switch (apiResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        return new ResponseDto { IsSuccess = false, DisplayMessage = "Not Found" };
                    case System.Net.HttpStatusCode.Forbidden:
                        return new ResponseDto { IsSuccess = false, DisplayMessage = "Forbidden" };
                    case System.Net.HttpStatusCode.Unauthorized:
                        return new ResponseDto { IsSuccess = false, DisplayMessage = "Unauthorized" };
                    case System.Net.HttpStatusCode.InternalServerError:
                        return new ResponseDto { IsSuccess = false, DisplayMessage = "Internal Server Error" };
                    case System.Net.HttpStatusCode.BadRequest:
                        return new ResponseDto { IsSuccess = false, DisplayMessage = "Bad Request" };
                }

                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

                return apiResponseDto;
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    IsSuccess = false,
                    DisplayMessage = "Error",
                    Result = ex.Message
                };
                return dto;
            }
        }
    }
}