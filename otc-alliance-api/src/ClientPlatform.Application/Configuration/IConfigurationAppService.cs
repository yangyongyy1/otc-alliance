using ClientPlatform.Configuration.Dto;
using System.Threading.Tasks;

namespace ClientPlatform.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
