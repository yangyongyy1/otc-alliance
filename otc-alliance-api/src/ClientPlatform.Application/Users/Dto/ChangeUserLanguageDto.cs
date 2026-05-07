using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}